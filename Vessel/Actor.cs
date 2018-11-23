
// FarNet module Vessel
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FarNet.Vessel
{
	public class Actor
	{
		readonly int _mode;
		readonly int _limit0;
		readonly string _store;
		readonly List<Record> _records;
		/// <summary>
		/// Creates the instance with data from the default storage.
		/// </summary>
		public Actor(int mode) : this(mode, null) { }
		/// <summary>
		/// Creates the instance with data ready for analyses.
		/// </summary>
		/// <param name="mode">History (0) or Folders (1).</param>
		/// <param name="store">The store path. Empty/null is the default.</param>
		/// <param name="noHistory">Tells to exclude Far folder history.</param>
		public Actor(int mode, string store, bool noHistory = false)
		{
			if (mode < 0 || mode > 1) throw new ArgumentException("Invalid mode.", "mode");

			_mode = mode;
			_store = store;
			_limit0 = Settings.Default.Limit0;
			_records = Store.Read(mode, store).ToList();

			if (mode == 0 || noHistory)
			{
				_records.Reverse();
			}
			else
			{
				var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				foreach (var record in _records)
				{
					set.Add(record.Path);
				}
				foreach (var folder in Far.Api.History.Folder())
				{
					if (!set.Contains(folder.Name))
						_records.Add(new Record(folder.Time, string.Empty, folder.Name));
				}
				_records = new List<Record>(_records.OrderByDescending(x => x.Time));
			}
		}
		/// <summary>
		/// Gets records from the most recent to old.
		/// </summary>
		public ReadOnlyCollection<Record> Records { get { return new ReadOnlyCollection<Record>(_records); } }
		/// <summary>
		/// Gets the ordered history info list.
		/// </summary>
		/// <param name="now">The time to generate the list for, normally the current.</param>
		/// <param name="factor">Smart history factor.</param>
		public IEnumerable<Info> GetHistory(DateTime now, int factor)
		{
			// collect
			var infos = CollectInfo(now, false);

			// evidences
			SetEvidences(infos, CollectEvidences());
			return infos.OrderByDescending(x => x, new InfoComparer(_limit0, factor));
		}
		/// <summary>
		/// Collects the unordered history info.
		/// </summary>
		public IList<Info> CollectInfo(DateTime now, bool reduced)
		{
			var result = new List<Info>();
			var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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
					DayCount = 1,
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
			var result = new Dictionary<string, SpanSet>(StringComparer.OrdinalIgnoreCase);

			foreach (var record in _records)
			{
				SpanSet spans;
				if (!result.TryGetValue(record.Path, out spans))
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
				info.Evidence = count;
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
				if (!record.Path.Equals(info.Path, StringComparison.OrdinalIgnoreCase))
					continue;

				// count cases, init if not yet
				if (++info.UseCount == 1)
				{
					info.Head = record.Time;
					info.Tail = record.Time;
					info.Idle = now - record.Time;
					info.DayCount = 1;
					continue;
				}

				// different days
				if (info.Head.Date != record.Time.Date)
				{
					++info.DayCount;

					// Why 2 is the best for all records so far, 3..4 are so so, and 5 is bad?
					// NB
					// Factor 4 gives values 0..4 (max is used); factors 5..6 still give 0..4 (max is not used).
					// Should we just use 4 or should we dig the max factor value M which gives values 0..M?
					// NB
					// With 2 and Idle > X Activity is rare/never actually equal to 2.
					if ((now - record.Time).TotalDays < 2)
						++info.Activity;
				}

				// now save the record time
				info.Head = record.Time;
			}
		}
		/// <summary>
		/// Trains the model.
		/// </summary>
		/// <param name="factor">Maximum factor in hours.</param>
		/// <param name="results">Optional result list.</param>
		/// <returns>The best training result.</returns>
		public Result Train(int factor, List<Result> results)
		{
			if (factor < _limit0)
				factor = _limit0;

			// init results
			if (results == null)
				results = new List<Result>();
			else
				results.Clear();
			for (int i = _limit0; i <= factor; i += (i < 24 ? 1 : (i < 48 ? 2 : 4)))
				results.Add(new Result() { Factor = i });

			// evidences once
			var map = CollectEvidences();

			// process records
			foreach (var record in Records)
			{
				// collect (step back 1 tick, ask to exclude recent) and sort by Idle
				var infos = CollectInfo(record.Time - new TimeSpan(1), true).OrderBy(x => x.Idle).ToList();

				// get the plain rank (it is the same for all other ranks)
				int rankPlain = infos.FindIndex(x => x.Path.Equals(record.Path, StringComparison.OrdinalIgnoreCase));

				// not found means it is the first history record for the file, skip it
				if (rankPlain < 0)
					continue;

				// evidences
				SetEvidences(infos, map);

				// sort with factors, get smart rank
				foreach (var r in results)
				{
					infos.Sort(new InfoComparer(_limit0, r.Factor));
					int rankSmart = infos.FindIndex(x => x.Path.Equals(record.Path, StringComparison.OrdinalIgnoreCase));

					int win = rankPlain - rankSmart;
					if (win < 0)
					{
						++r.DownCount;
						r.DownSum -= win;
					}
					else if (win > 0)
					{
						++r.UpCount;
						r.UpSum += win;
					}
					else
					{
						++r.SameCount;
					}
				}
			}

			// the best result
			Result result = null;
			var maxTarget = int.MinValue;
			foreach (var it in results)
			{
				var target = it.TotalSum;
				if (maxTarget < target)
				{
					result = it;
					maxTarget = target;
				}
			}

			return result;
		}
		public string Update()
		{
			// sanity
			int maxDays = Settings.Default.MaximumDayCount;
			if (maxDays < 30)
				throw new InvalidOperationException("Use at least 30 as the maximum day count.");
			int maxFiles = Settings.Default.MaximumFileCount;
			if (maxFiles < 100)
				throw new InvalidOperationException("Use at least 100 as the maximum file count.");

			var now = DateTime.Now;
			Logger.Source.TraceEvent(TraceEventType.Start, 0, "Update {0}", now);

			if (_mode == 0)
				Settings.Default.LastUpdateTime1 = now;
			else
				Settings.Default.LastUpdateTime2 = now;
			Settings.Default.Save();

			int recordCount = _records.Count;

			// collect and sort by idle
			var infos = CollectInfo(DateTime.Now, false).OrderBy(x => x.Idle).ToList();

			// step 1: remove missing files from infos and records
			int missingFiles = 0;
			foreach (var path in infos.Select(x => x.Path).ToArray())
			{
				try
				{
					if (_mode == 0)
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
					int removed = _records.RemoveAll(x => x.Path.Equals(path, StringComparison.OrdinalIgnoreCase));
					Logger.Source.TraceInformation("Missing: {0}: {1}", removed, path);
					++missingFiles;
				}
				catch (Exception ex)
				{
					Logger.Source.TraceEvent(TraceEventType.Error, 0, "Error: {0}: {1}", path, ex.Message);
				}
			}

			// step 2: remove the most idle excess files
			int excessFiles = 0;
			while (infos.Count > maxFiles)
			{
				var info = infos[infos.Count - 1];
				infos.RemoveAt(infos.Count - 1);
				int removed = _records.RemoveAll(x => x.Path.Equals(info.Path, StringComparison.OrdinalIgnoreCase));
				Logger.Source.TraceInformation("Extra: {0}: {1}", removed, info.Path);
				++excessFiles;
			}

			// step 3: cound days excluding today and remove aged records
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
						x.Time < zero && x.Time != info.Tail && x.Path.Equals(info.Path, StringComparison.OrdinalIgnoreCase));

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
Missing paths : {0,4}
Excess paths  : {1,4}
Old records   : {2,4}

RESULT
Aged paths    : {3,4}
Used paths    : {4,4}
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
