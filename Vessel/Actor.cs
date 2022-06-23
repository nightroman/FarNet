
// FarNet module Vessel
// Copyright (c) Roman Kuzmin

using FarNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Vessel;

public partial class Actor
{
	readonly static TimeSpan _smallSpan = TimeSpan.FromSeconds(3);
	readonly Mode _mode;
	readonly string _store;
	readonly TimeSpan _limit0;
	readonly StringComparer _comparer;
	readonly StringComparison _comparison;

	/// <summary>
	/// File records from old to new.
	/// Mostly read only but pruned in place by updates before saving.
	/// </summary>
	readonly List<Record> _records;

	/// <summary>
	/// Creates the instance with data ready for analyses.
	/// </summary>
	/// <param name="mode">Files/Folders/Commands mode.</param>
	/// <param name="store">The store path. Empty/null is the default.</param>
	public Actor(Mode mode, string store = null)
	{
		switch (mode)
		{
			case Mode.File:
				_comparer = StringComparer.OrdinalIgnoreCase;
				_comparison = StringComparison.OrdinalIgnoreCase;
				break;
			case Mode.Folder:
				_comparer = StringComparer.OrdinalIgnoreCase;
				_comparison = StringComparison.OrdinalIgnoreCase;
				break;
			case Mode.Command:
				_comparer = StringComparer.Ordinal;
				_comparison = StringComparison.Ordinal;
				break;
			default:
				throw new ArgumentException("Invalid mode.", nameof(mode));
		}

		var settings = Settings.Default.GetData();

		_mode = mode;
		_store = string.IsNullOrEmpty(store) ? VesselHost.LogPath[(int)mode] : store;
		_limit0 = TimeSpan.FromHours(settings.Limit0);
		_records = Store.Read(_store).ToList();
	}

	static void AddFarHistory(Dictionary<string, Info> infos, DateTime old, HistoryKind historyKind)
	{
		var items = Far.Api.History.GetHistory(new GetHistoryArgs { Kind = historyKind });
		foreach (var item in items)
		{
			if (infos.TryGetValue(item.Name, out Info info))
			{
				// override if recent
				if (item.Time > old && item.Time - info.TimeN > _smallSpan)
					infos[item.Name] = new Info { Path = item.Name, TimeN = item.Time };
			}
			else
			{
				infos.Add(item.Name, new Info { Path = item.Name, TimeN = item.Time });
			}
		}
	}

	/// <summary>
	/// Gets the ordered history info list.
	/// </summary>
	/// <param name="now">The time to generate the list for, normally the current.</param>
	public IEnumerable<Info> GetHistory(DateTime now)
	{
		var infos = CollectInfo(now, false);
		var old = now - _limit0;

		// add missing and tweak later records
		switch (_mode)
		{
			case Mode.File:
				// Far editor history
				AddFarHistory(infos, old, HistoryKind.Editor);

				// Far viewer history
				AddFarHistory(infos, old, HistoryKind.Viewer);
				break;

			case Mode.Folder:
				// Far folder history
				AddFarHistory(infos, old, HistoryKind.Folder);
				break;

			case Mode.Command:
				// Far command history
				AddFarHistory(infos, old, HistoryKind.Command);
				break;
		}

		return infos.Values.OrderByDescending(x => x, new InfoComparer(old));
	}

	/// <summary>
	/// Collects the unordered history info.
	/// </summary>
	public Dictionary<string, Info> CollectInfo(DateTime now, bool skipNewAndAged, bool skipEvidences = false)
	{
		var old = now - _limit0;
		var map = new Dictionary<string, Info>(_comparer);

		for (int iRecord = _records.Count - 1; iRecord >= 0; --iRecord)
		{
			// skip data from the future
			var record = _records[iRecord];
			if (record.Time > now)
				continue;

			// skip added and processed path
			if (map.ContainsKey(record.Path))
				continue;

			// skip recent and aged
			if (skipNewAndAged && (record.Time > old || record.What == Record.AGED))
				continue;

			// init info with the newest record
			var info = new Info
			{
				Path = record.Path,
				TimeN = record.Time,
			};

			// add now before skips
			map.Add(record.Path, info);

			// skip evidences, note that aged items have no more records
			if (skipEvidences || record.What == Record.AGED)
				continue;

			// collect the info for this file
			CollectEvidence(info, _records, iRecord - 1, now, _comparison);
		}

		return map;
	}

	/// <summary>
	/// Collects info for a file.
	/// </summary>
	static void CollectEvidence(Info info, IList<Record> records, int start, DateTime now, StringComparison comparison)
	{
		var lastTime = info.TimeN;
		var age = now - lastTime;
		var (min, max) = Info.GetSpanMinMax(age);

		// from new to old
		for (int iRecord = start; iRecord >= 0; --iRecord)
		{
			// skip different files
			var record = records[iRecord];
			if (!record.Path.Equals(info.Path, comparison))
				continue;

			// test for evidence
			var idle = lastTime - record.Time;
			if (idle > min && idle < max)
			{
				info.Evidence = info.CalculateEvidence(1, age);
				return;
			}

			// update the last time
			lastTime = record.Time;
		}
	}

	public string Update()
	{
		var today = DateTime.Today;
		var settings = Settings.Default.GetData();

		// get settings and check sanity
		int maxDays = settings.MaximumDayCount;
		if (maxDays < 30)
			throw new InvalidOperationException("Use at least 30 as the maximum day count.");
		int maxFileCount = settings.MaximumFileCount;
		if (maxFileCount < 100)
			throw new InvalidOperationException("Use at least 100 as the maximum file count.");
		int maxFileAge = settings.MaximumFileAge;
		if (maxFileAge < maxDays)
			maxFileAge = maxDays;

		var now = DateTime.Now;

		var workings = new Workings();
		var works = workings.GetData();
		switch (_mode)
		{
			case Mode.File:
				works.LastUpdateTime1 = now;
				break;
			case Mode.Folder:
				works.LastUpdateTime2 = now;
				break;
			case Mode.Command:
				works.LastUpdateTime3 = now;
				break;
		}
		workings.Save();

		int recordCount = _records.Count;

		// ensure records sorted by time, just in case the log was edited manually
		_records.Sort(new Record.Comparer());

		// collect and sort by time from new to old
		var infos = CollectInfo(DateTime.Now, false, true).Values.OrderByDescending(x => x.TimeN).ToList();

		// step: remove missing files from infos and records
		int missingFiles = 0;
		if (_mode == Mode.File || _mode == Mode.Folder)
		{
			foreach (var path in infos.Select(x => x.Path).ToArray())
			{
				try
				{
					if (_mode == Mode.File)
					{
						if (File.Exists(path))
							continue;
					}
					else
					{
						if (Directory.Exists(path))
							continue;
					}

					infos.RemoveAll(x => x.Path.Equals(path, _comparison));
					_records.RemoveAll(x => x.Path.Equals(path, _comparison));
					++missingFiles;
				}
				catch (Exception ex)
				{
					Far.Api.UI.WriteLine($"Error: {path}: {ex.Message}", ConsoleColor.Red);
				}
			}
		}

		// step: remove the most idle excess files
		int excessFiles = 0;
		while (infos.Count > maxFileCount || (infos.Count > 0 && (today - infos[infos.Count - 1].TimeN).TotalDays > maxFileAge))
		{
			var info = infos[infos.Count - 1];
			infos.RemoveAt(infos.Count - 1);
			_records.RemoveAll(x => x.Path.Equals(info.Path, _comparison));
			++excessFiles;
		}

		// cound recorded days excluding today
		var days = _records
			.Select(x => x.Time.Date)
			.Where(x => x != today)
			.Distinct()
			.OrderByDescending(x => x)
			.ToArray();

		// step: remove old records
		int agedFiles = 0;
		int oldRecords = 0;
		if (days.Length > maxDays)
		{
			var aged = days[maxDays - 1];

			// remove old records keeping the last
			foreach (var info in infos)
			{
				int removed = _records.RemoveAll(
					x => x.Time < aged && x.Time != info.TimeN && x.Path.Equals(info.Path, _comparison));

				oldRecords += removed;
			}

			// mark old records aged
			foreach (var record in _records)
			{
				if (record.Time < aged)
				{
					++agedFiles;
					record.SetAged();
				}
			}
		}

		// save sorted by time
		Store.Write(_store, _records.OrderBy(x => x.Time));

		return string.Format(@"
Missing items : {0,4}    Aged items    : {3,4}
Excess items  : {1,4}    Used items    : {4,4}
Aged records  : {2,4}    Records       : {5,4}
",
		missingFiles,
		excessFiles,
		oldRecords,
		agedFiles,
		infos.Count - agedFiles,
		_records.Count);
	}
}
