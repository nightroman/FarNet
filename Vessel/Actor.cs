
// FarNet module Vessel
// Copyright (c) Roman Kuzmin

using FarNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Vessel;

public partial class Actor
{
	// mode based
	readonly Mode _mode;
	readonly string _store;
	readonly StringComparer _comparer;
	readonly StringComparison _comparison;

	// settings based
	readonly TimeSpan _limit0;
	readonly string _choiceLog;
	readonly int _MaximumFileCountFromFar;
	readonly int _MinimumRecentFileCount;

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
				throw new ArgumentException("Invalid mode.");
		}

		_mode = mode;
		_store = string.IsNullOrEmpty(store) ? VesselHost.LogPath[(int)mode] : store;

		var settings = Settings.Default.GetData();
		_limit0 = TimeSpan.FromHours(settings.Limit0);
		_MaximumFileCountFromFar = settings.MaximumFileCountFromFar;
		_MinimumRecentFileCount = settings.MinimumRecentFileCount;
		_choiceLog = Environment.ExpandEnvironmentVariables(settings.ChoiceLog ?? string.Empty);

		_records = Store.Read(_store).ToList();
	}

	public Mode Mode => _mode;

	public void RemoveRecordFromStore(string path)
	{
		Store.Remove(_store, path, _comparison);
	}

	public void AppendRecordToStore(string what, string path)
	{
		Store.Append(_store, DateTime.Now, what, path);
	}

	HistoryInfo[] GetFarHistory()
	{
		switch (_mode)
		{
			case Mode.File:
				// ignore viewer (little value, not nice merge) -> feature: viewer does not affect our lists
				return Far.Api.History.GetHistory(new GetHistoryArgs { Kind = HistoryKind.Editor, Last = _MaximumFileCountFromFar });

			case Mode.Folder:
				return Far.Api.History.GetHistory(new GetHistoryArgs { Kind = HistoryKind.Folder, Last = _MaximumFileCountFromFar });

			case Mode.Command:
				return Far.Api.History.GetHistory(new GetHistoryArgs { Kind = HistoryKind.Command, Last = _MaximumFileCountFromFar });
		}
		throw null;
	}

	/// <summary>
	/// Adds missing and tweaks existing by later records from history.
	/// </summary>
	void MergeFarHistory(Dictionary<string, Record> map)
	{
		var farHistory = GetFarHistory();
		foreach (var item in farHistory)
		{
			if (map.TryGetValue(item.Name, out Record info))
			{
				if (item.Time > info.Time)
					info.Time = item.Time;
			}
			else
			{
				map.Add(item.Name, new Record(item.Time, string.Empty, item.Name));
			}
		}
	}

	/// <summary>
	/// Gets the ordered history list.
	/// </summary>
	public List<Record> GetHistory(DateTime old)
	{
		var map = CollectLatestRecords();
		MergeFarHistory(map);

		// first sort by time
		var records = map.Values.ToList();
		records.Sort(new Record.TimeComparer());

		// and mark recent
		for (int iRecord = records.Count - 1; iRecord >= 0; --iRecord)
		{
			if (records[iRecord].Time > old || records.Count - iRecord <= _MinimumRecentFileCount)
				records[iRecord].IsRecent = true;
			else
				break;
		}

		// the sort by rank and return for show
		records.Sort(new Record.RankComparer());
		return records;
	}

	/// <summary>
	/// Collects the latest records map by paths.
	/// </summary>
	public Dictionary<string, Record> CollectLatestRecords()
	{
		var map = new Dictionary<string, Record>(_comparer);

		// from new to old (normally should be)
		for (int iRecord = _records.Count - 1; iRecord >= 0; --iRecord)
		{
			var record = _records[iRecord];
			if (!map.ContainsKey(record.Path))
				map.Add(record.Path, record);
		}

		return map;
	}

	// It returns if the log is not set. Otherwise it simply starts a task.
	// Requires: the selected index is ready to use as the choice by rank.
	internal void LogChoice(IEnumerable<Record> records, int indexSelected, string path)
	{
		if (string.IsNullOrWhiteSpace(_choiceLog))
			return;

		Task.Run(() =>
		{
			if (!File.Exists(_choiceLog))
				File.AppendAllText(_choiceLog, "Gain\tRank\tAge\tMode\tPath\r\n");

			var list = records.ToList();
			var rank = list.Count - 1 - indexSelected;
			var age = (int)(DateTime.Now - list[indexSelected].Time).TotalHours;

			list.Sort(new Record.TimeComparer());
			var index = list.Count - 1 - list.FindLastIndex(x => x.Path.Equals(path, _comparison));

			File.AppendAllText(_choiceLog, $"{index - rank}\t{rank}\t{age}\t{_mode}\t{path}\r\n");
		});
	}

	internal string Update()
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

		// save last update time
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

		// collect and set latest records sorted by time
		{
			var map = CollectLatestRecords();
			_records.Clear();
			_records.AddRange(map.Values.OrderBy(x => x.Time));
		}

		// step: remove untracked records
		{
			for (int iRecord = _records.Count - 1; iRecord >= 0; --iRecord)
			{
				if (!_records[iRecord].IsTracked && (now - _records[iRecord].Time) > _limit0)
					_records.RemoveAt(iRecord);
			}
		}

		// step: remove missing file records
		int missingFiles = 0;
		if (_mode == Mode.File || _mode == Mode.Folder)
		{
			for (int iRecord = _records.Count - 1; iRecord >= 0; --iRecord)
			{
				var path = _records[iRecord].Path;
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

					++missingFiles;
					_records.RemoveAt(iRecord);
				}
				catch (Exception ex)
				{
					Far.Api.UI.WriteLine($"Error: {path}: {ex.Message}", ConsoleColor.Red);
				}
			}
		}

		// step: remove the oldest excess files
		int excessFiles = 0;
		while (_records.Count > maxFileCount || (_records.Count > 0 && (today - _records[0].Time).TotalDays > maxFileAge))
		{
			++excessFiles;
			_records.RemoveAt(0);
		}

		// step: mark aged records
		int agedFiles = 0;
		int newAgedFiles = 0;
		var timeAged = today.AddDays(- maxDays);
		foreach (var record in _records)
		{
			if (record.Time < timeAged)
			{
				++agedFiles;
				if (record.What != Record.AGED)
				{
					++newAgedFiles;
					record.What = Record.AGED;
				}
			}
			else
			{
				if (record.What == Record.AGED)
					record.What = Record.USED;
			}
		}

		// save updated records
		Store.Write(_store, _records);

		return string.Format(@"
Removed missing : {0,4}    Aged items    : {3,4}
Removed excess  : {1,4}    Used items    : {4,4}
Marked aged     : {2,4}    All items     : {5,4}
",
		missingFiles,
		excessFiles,
		newAgedFiles,
		agedFiles,
		_records.Count - agedFiles,
		_records.Count);
	}
}
