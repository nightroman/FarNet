﻿
// FarNet module Vessel
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FarNet.Vessel
{
	public class Actor
	{
		readonly static TimeSpan _smallSpan = TimeSpan.FromSeconds(3);
		readonly Mode _mode;
		readonly int _limit0;
		readonly string _store;
		readonly StringComparer _comparer;
		readonly StringComparison _comparison;
		readonly List<Record> _records;
		readonly Dictionary<string, Record> _latestRecords;
		/// <summary>
		/// Creates the instance with data from the default storage.
		/// </summary>
		public Actor(Mode mode) : this(mode, null) { }
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
			_store = store ?? VesselHost.LogPath[(int)mode];
			_limit0 = settings.Limit0;
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
			// collect
			var infos = CollectInfo(now, false);

			// evidences
			SetEvidences(infos, CollectEvidences());
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

				// add, skip existing
				if (!set.Add(record.Path))
					continue;

				// skip recent and too old
				var idle = now - record.Time;
				if (reduced && (idle.TotalHours < _limit0 || record.What == Record.AGED))
					continue;

				// init info
				var info = new Info()
				{
					Path = record.Path,
					Head = record.Time,
					Tail = record.Time,
					Idle = idle,
					UseCount = 1,
				};

				// get the rest
				CollectFileInfo(info, iRecord + 1, now);

				result.Add(info);
			}

			return result;
		}
		public Dictionary<string, SpanSet> CollectEvidences()
		{
			var result = new Dictionary<string, SpanSet>(_comparer);

			foreach (var record in _records)
			{
				if (!result.TryGetValue(record.Path, out SpanSet spans))
				{
					spans = new SpanSet() { Time = record.Time };
					result.Add(record.Path, spans);
					continue;
				}

				var idle = spans.Time - record.Time;
				spans.Time = record.Time;

				int span = Mat.Span(idle.TotalHours);
				if (span < Info.SpanCount)
					++spans.Spans[span];
			}

			return result;
		}
		/// <summary>
		/// Sets info evidences from collected data.
		/// </summary>
		static void SetEvidences(IEnumerable<Info> infos, Dictionary<string, SpanSet> map)
		{
			// calculate evidences for idle times
			foreach (var info in infos)
			{
				int span = Mat.Span(info.Idle.TotalHours);
				if (span >= Info.SpanCount)
					continue;

				int count = map[info.Path].Spans[span];
				info.SetEvidence(count, span);
			}
		}
		/// <summary>
		/// Collects not empty info for each record at the record's time.
		/// </summary>
		public IEnumerable<Info> CollectOpenInfo()
		{
			var result = new List<Info>();
			for (int iRecord = 0; iRecord < _records.Count; ++iRecord)
			{
				var record = _records[iRecord];

				// init info
				var info = new Info() { Path = record.Path };

				// get the rest
				CollectFileInfo(info, iRecord + 1, record.Time);
				if (info.Idle.TotalHours < _limit0)
					continue;

				// return not empty
				if (info.UseCount > 0)
					result.Add(info);
			}
			SetEvidences(result, CollectEvidences());
			return result;
		}
		/// <summary>
		/// Collects info for a file.
		/// </summary>
		void CollectFileInfo(Info info, int start, DateTime now)
		{
			for (int iRecord = start; iRecord < _records.Count; ++iRecord)
			{
				// skip data from the future
				var record = _records[iRecord];
				if (record.Time > now)
					continue;

				// skip alien
				if (!record.Path.Equals(info.Path, _comparison))
					continue;

				// count cases, init if not yet
				if (++info.UseCount == 1)
				{
					info.Head = record.Time;
					info.Tail = record.Time;
					info.Idle = now - record.Time;
					continue;
				}

				// now save the record time
				info.Head = record.Time;
			}
		}
		/// <summary>
		/// Trains the model.
		/// </summary>
		/// <returns>The best training result.</returns>
		public Result Train()
		{
			var result = new Result();
			var map = CollectEvidences();

			// process records
			foreach (var record in _records)
			{
				// collect (step back 1 tick, ask to exclude recent) and sort by Idle
				var infos = CollectInfo(record.Time - new TimeSpan(1), true).OrderBy(x => x.Idle).ToList();

				// get the plain rank (it is the same for all other ranks)
				int rankPlain = infos.FindIndex(x => x.Path.Equals(record.Path, _comparison));

				// not found means it is the first history record for the file, skip it
				if (rankPlain < 0)
					continue;

				// evidences
				SetEvidences(infos, map);

				// sort with factors, get smart rank
				infos.Sort(new InfoComparer(_limit0));
				int rankSmart = infos.FindIndex(x => x.Path.Equals(record.Path, _comparison));

				int win = rankPlain - rankSmart;
				if (win < 0)
				{
					++result.DownCount;
					result.DownSum -= win;
				}
				else if (win > 0)
				{
					++result.UpCount;
					result.UpSum += win;
				}
				else
				{
					++result.SameCount;
				}
			}

			return result;
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
			Logger.Source.TraceEvent(TraceEventType.Start, 0, "Update {0}", now);

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
						Logger.Source.TraceInformation("Missing: {0}: {1}", removed, path);
						++missingFiles;
					}
					catch (Exception ex)
					{
						Logger.Source.TraceEvent(TraceEventType.Error, 0, "Error: {0}: {1}", path, ex.Message);
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
				Logger.Source.TraceInformation("Extra: {0}: {1}", removed, info.Path);
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
				Logger.Source.TraceInformation("Zero: {0}", zero);

				foreach (var info in infos)
				{
					// skip single or positive
					if (info.UseCount < 2 || info.Head >= zero)
						continue;

					// remove all aged records but the tail
					int removed = _records.RemoveAll(x =>
						x.Time < zero && x.Time != info.Tail && x.Path.Equals(info.Path, _comparison));

					Logger.Source.TraceInformation("Aged records: {0}: {1}", removed, info.Path);
					oldRecords += removed;
				}

				// set negative records to zero
				foreach (var record in _records)
				{
					if (record.Time < zero)
					{
						++agedFiles;
						if (record.What != Record.AGED)
						{
							record.SetAged();
							Logger.Source.TraceInformation("Aged file: {0}", record.Path);
						}
					}
				}
			}

			// save sorted by time
			Store.Write(_store, _records.OrderBy(x => x.Time));

			Logger.Source.TraceEvent(TraceEventType.Stop, 0, "Update");
			return string.Format(@"
REMOVE
Missing items : {0,4}
Excess items  : {1,4}
Aged records  : {2,4}

RESULT
Aged items    : {3,4}
Used items    : {4,4}
Records       : {5,4}
",
			missingFiles,
			excessFiles,
			oldRecords,
			agedFiles,
			infos.Count - agedFiles,
			_records.Count);
		}
	}
}
