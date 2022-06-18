
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
	readonly TimeSpan _limit0;
	readonly string _store;
	readonly StringComparer _comparer;
	readonly StringComparison _comparison;
	readonly Dictionary<string, Record> _latestRecords;

	/// <summary>
	/// File records from new to old.
	/// </summary>
	readonly List<Record> _records;

	/// <summary>
	/// Creates the instance with data from the default storage.
	/// </summary>
	public Actor(Mode mode) : this(mode, null)
	{
	}

	/// <summary>
	/// Creates the instance with data ready for analyses.
	/// </summary>
	/// <param name="mode">Files/Folders/Commands mode.</param>
	/// <param name="store">The store path. Empty/null is the default.</param>
	/// <param name="noHistory">Tells to exclude Far history.</param>
	public Actor(Mode mode, string store, bool noHistory = false)
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

		if (noHistory)
		{
			_records.Reverse();
		}
		else
		{
			// original latest records by paths (assuming ascending order in the log)
			_latestRecords = new Dictionary<string, Record>(_comparer);
			foreach (var record in _records)
				_latestRecords[record.Path] = record;

			var args = new GetHistoryArgs() { Last = settings.MaximumFileCountFromFar };
			switch (mode)
			{
				case Mode.File:
					// add missing and later records from Far editor history
					args.Kind = HistoryKind.Editor;
					AddFarHistory(args);

					// add missing and later records from Far viewer history
					args.Kind = HistoryKind.Viewer;
					AddFarHistory(args);
					break;
				case Mode.Folder:
					// add missing and later records from Far folder history
					args.Kind = HistoryKind.Folder;
					AddFarHistory(args);
					break;
				case Mode.Command:
					// add missing and later records from Far command history
					args.Kind = HistoryKind.Command;
					AddFarHistory(args);
					break;
			}
		}

		// get sorted
		_records = new List<Record>(_records.OrderByDescending(x => x.Time));
	}

	void AddFarHistory(GetHistoryArgs args)
	{
		var items = Far.Api.History.GetHistory(args);
		foreach (var item in items)
		{
			if (!_latestRecords.TryGetValue(item.Name, out Record record) || item.Time - record.Time > _smallSpan)
				_records.Add(new Record(item.Time, Record.NOOP, item.Name));
		}
	}

	/// <summary>
	/// Gets true if the path exists in the log.
	/// </summary>
	public bool IsLoggedPath(string path)
	{
		// If _latestRecords is null then Actor is created without
		// Far history and this method is not supposed to be used.
		return _latestRecords.ContainsKey(path);
	}

	/// <summary>
	/// Gets the ordered history info list.
	/// </summary>
	/// <param name="now">The time to generate the list for, normally the current.</param>
	public IEnumerable<Info> GetHistory(DateTime now)
	{
		var infos = CollectInfo(now, false);
		return infos.OrderByDescending(x => x, new InfoComparer(_limit0));
	}

	/// <summary>
	/// Collects the unordered history info.
	/// </summary>
	public IList<Info> CollectInfo(DateTime now, bool reduced)
	{
		var result = new List<Info>();
		var set = new HashSet<string>(_comparer);

		for (int iRecord = 0; iRecord < _records.Count; ++iRecord)
		{
			// skip data from the future
			var record = _records[iRecord];
			if (record.Time > now)
				continue;

			// add path, skip existing
			if (!set.Add(record.Path))
				continue;

			// skip recent and aged
			var idle = now - record.Time;
			if (reduced && (idle < _limit0 || record.What == Record.AGED))
				continue;

			// init info with the newest record
			var info = new Info
			{
				Path = record.Path,
				Idle = idle,
				Time1 = record.Time,
				TimeN = record.Time,
				UseCount = 1,
			};

			// collect the info for this file
			CollectFileInfo(info, iRecord + 1);

			result.Add(info);
		}

		return result;
	}

	/// <summary>
	/// Collects info for a file.
	/// </summary>
	void CollectFileInfo(Info info, int start)
	{
		int evidenceCount = 0;
		var thisSpan = Mat.Span(info.Idle);

		// from new to old
		for (int iRecord = start; iRecord < _records.Count; ++iRecord)
		{
			// skip different files
			var record = _records[iRecord];
			if (!record.Path.Equals(info.Path, _comparison))
				continue;

			// 1. count evidence
			int span = Mat.Span(info.Time1 - record.Time);
			if (span == thisSpan && span < Info.SpanCount)
				++evidenceCount;

			// 2. update the first use and count
			info.Time1 = record.Time;
			++info.UseCount;
		}

		// finally
		if (thisSpan < Info.SpanCount)
			info.Evidence = Info.CalculateEvidence(evidenceCount, thisSpan);
	}

	public string Update()
	{
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

		// collect and sort by idle
		var infos = CollectInfo(DateTime.Now, false).OrderBy(x => x.Idle).ToList();

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

					infos.RemoveAll(x => x.Path == path);
					int removed = _records.RemoveAll(x => x.Path.Equals(path, _comparison));
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
		while (infos.Count > maxFileCount || (infos.Count > 0 && infos[infos.Count - 1].Idle.TotalDays > maxFileAge))
		{
			var info = infos[infos.Count - 1];
			infos.RemoveAt(infos.Count - 1);
			int removed = _records.RemoveAll(x => x.Path.Equals(info.Path, _comparison));
			++excessFiles;
		}

		// step: cound days excluding today and remove aged records
		int agedFiles = 0;
		int oldRecords = 0;
		var today = DateTime.Today;
		var days = _records.Select(x => x.Time.Date).Where(x => x != today).Distinct().OrderByDescending(x => x).ToArray();
		if (days.Length > maxDays)
		{
			var zero = days[maxDays - 1];

			foreach (var info in infos)
			{
				// skip single or positive
				if (info.UseCount < 2 || info.Time1 >= zero)
					continue;

				// remove all aged records but the tail
				int removed = _records.RemoveAll(x =>
					x.Time < zero && x.Time != info.TimeN && x.Path.Equals(info.Path, _comparison));

				oldRecords += removed;
			}

			// set negative records to zero
			foreach (var record in _records)
			{
				if (record.Time < zero)
				{
					++agedFiles;
					if (record.What != Record.AGED)
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
