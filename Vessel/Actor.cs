
/*
FarNet module Vessel
Copyright (c) 2011 Roman Kuzmin
*/

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
		const int MAX_DAYS = 30;
		const int MAX_FILES = 512;

		readonly string _store;
		readonly List<Record> _records;

		/// <summary>
		/// Creates the instance with data from the default storage.
		/// </summary>
		public Actor() : this(null) { }

		/// <summary>
		/// Creates the instance with data ready for analyses.
		/// </summary>
		/// <param name="store">The store path. Empty/null is for the default.</param>
		public Actor(string store)
		{
			_store = store;
			_records = Record.Read(store).ToList();
			_records.Reverse();
		}

		/// <summary>
		/// Gets records from the most recent to old.
		/// </summary>
		public ReadOnlyCollection<Record> Records { get { return new ReadOnlyCollection<Record>(_records); } }

		/// <summary>
		/// Gets the ordered history info list.
		/// </summary>
		/// <param name="now">The time to generate the list for, normally the current.</param>
		/// <param name="factor1">0+: smart history; otherwise: plain history.</param>
		/// <param name="factor2">0+: smart history second factor.</param>
		public IEnumerable<Info> GetHistory(DateTime now, int factor1, int factor2)
		{
			// collect
			var infos = CollectInfo(now, false);

			// order
			if (factor1 < 0)
				return infos.OrderByDescending(x => x.Idle);
			else
				return infos.OrderByDescending(x => x, new InfoComparer(factor1, factor2));
		}

		/// <summary>
		/// Collects the unordered history info.
		/// </summary>
		public IEnumerable<Info> CollectInfo(DateTime now, bool excludeRecent)
		{
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

				// skip recent
				var idle = now - record.Time;
				if (excludeRecent && idle.TotalHours < VesselHost.Limit0)
					continue;

				// init info
				var info = new Info()
				{
					Path = record.Path,
					Head = record.Time,
					Tail = record.Time,
					Idle = idle,
					KeyCount = record.Keys,
					DayCount = 1,
					UseCount = 1,
				};

				// get the rest
				CollectFileInfo(info, iRecord + 1, now);

				yield return info;
			}
		}

		/// <summary>
		/// Collects not empty info for each record at the record's time.
		/// </summary>
		public IEnumerable<Info> CollectOpenInfo()
		{
			for (int iRecord = 0; iRecord < _records.Count; ++iRecord)
			{
				var record = _records[iRecord];

				// init info
				var info = new Info() { Path = record.Path };

				// get the rest
				CollectFileInfo(info, iRecord + 1, record.Time);

				// return not empty
				if (info.UseCount > 0)
					yield return info;
			}
		}

		/// <summary>
		/// Collects the history info for the file.
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
					info.KeyCount = record.Keys;
					info.DayCount = 1;
					continue;
				}

				// sum keys
				info.KeyCount += record.Keys;

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

				// cases of idle larger than the current idle
				var idle = info.Head - record.Time;
				if (idle > info.Idle)
					++info.Frequency;

				// now save the record time
				info.Head = record.Time;
			}
		}

		/// <summary>
		/// Gets all the training results.
		/// </summary>
		public IList<Result> TrainingResults { get { return _TrainingResults; } }
		List<Result> _TrainingResults;

		Result Train()
		{
			// process records
			VesselTool.TrainingRecordCount = Records.Count;
			VesselTool.TrainingRecordIndex = 0;
			foreach (var record in Records)
			{
				++VesselTool.TrainingRecordIndex;

				// collect (step back 1 tick, ask to exclude recent) and sort by Idle
				var infos = CollectInfo(record.Time - new TimeSpan(1), true).OrderBy(x => x.Idle).ToList();

				// get the plain rank (it is the same for all other ranks)
				int rankPlain = infos.FindIndex(x => x.Path.Equals(record.Path, StringComparison.OrdinalIgnoreCase));

				// not found means it is the first history record for the file, skip it
				if (rankPlain < 0)
					continue;

				// sort with factors, get smart rank
				foreach (var r in _TrainingResults)
				{
					infos.Sort(new InfoComparer(r.Factor1, r.Factor2));
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

			// return the best result
			Result result = null;
			var maxTarget = float.MinValue;
			foreach (var it in _TrainingResults)
			{
				var target = it.Target;
				if (maxTarget < target)
				{
					result = it;
					maxTarget = target;
				}
			}

			return result;
		}

		const int xRadius = 5;
		const int yRadius = 2;

		Result TrainFastEpoch(int factor1, int factor2)
		{
			const int xStep = 20;
			const int yStep = 4;

			int x1 = Math.Max(0, factor1 - xRadius);
			int y1 = Math.Max(0, factor2 - yRadius);
			int x2 = Math.Min(VesselHost.Limit1, factor1 + xRadius);
			int y2 = Math.Min(VesselHost.Limit2, factor2 + yRadius);

			_TrainingResults = new List<Result>((x2 - x1 + 1) * (y2 - y1 + 1) + (VesselHost.Limit1 / xStep + 1) * (VesselHost.Limit2 / yStep + 1));

			for (int i = x1; i <= x2; ++i)
				for (int j = y1; j <= y2; ++j)
					_TrainingResults.Add(new Result() { Factor1 = i, Factor2 = j });

			var random = new Random();
			int xStart = random.Next(xStep);
			int yStart = random.Next(yStep);
			for (int i = xStart; i <= VesselHost.Limit1; i += xStep)
			{
				for (int j = yStart; j <= VesselHost.Limit2; j += yStep)
					if (i < x1 || i > x2 || j < y1 || j > y2)
						_TrainingResults.Add(new Result() { Factor1 = i, Factor2 = j });
			}

			Logger.Source.TraceInformation("Training: Capacity {0}; Count {1}; Starts {2}/{3}",
				_TrainingResults.Capacity,
				_TrainingResults.Count,
				xStart,
				yStart);

			var result = Train();

			return result;
		}

		/// <summary>
		/// Trains the model using all factors.
		/// </summary>
		/// <param name="limit1">Limit 1 in hours.</param>
		/// <param name="limit2">Limit 2 in days.</param>
		/// <returns>The best training result.</returns>
		public Result TrainFull(int limit1, int limit2)
		{
			_TrainingResults = new List<Result>((limit1 + 1) * (limit2 + 1));
			for (int i = 0; i <= limit1; ++i)
				for (int j = 0; j <= limit2; ++j)
					_TrainingResults.Add(new Result() { Factor1 = i, Factor2 = j });

			return Train();
		}

		/// <summary>
		/// Trains the model using the old factors.
		/// </summary>
		/// <param name="factor1">Old factor 1.</param>
		/// <param name="factor2">Old factor 2.</param>
		/// <returns>The best training result.</returns>
		public Result TrainFast(int factor1, int factor2)
		{
			var sw = Stopwatch.StartNew();
			Logger.Source.TraceEvent(TraceEventType.Start, 0, "Training {0}", DateTime.Now);

			if (factor1 < 0)
			{
				factor1 = xRadius;
				factor2 = yRadius;
			}

			for (; ; )
			{
				var result = TrainFastEpoch(factor1, factor2);
				Logger.Source.TraceInformation("Training: Target {0}; Factors {1}/{2}", result.Target, result.Factor1, result.Factor2);

				if (Math.Abs(factor1 - result.Factor1) > xRadius || Math.Abs(factor2 - result.Factor2) > yRadius)
				{
					factor1 = result.Factor1;
					factor2 = result.Factor2;
					continue;
				}

				Logger.Source.TraceEvent(TraceEventType.Stop, 0, "Training {0}", sw.Elapsed);
				return result;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public string Update()
		{
			Logger.Source.TraceEvent(TraceEventType.Start, 0, "Update {0}", DateTime.Now);
			int recordCount = _records.Count;

			// collect and sort by idle
			var infos = CollectInfo(DateTime.Now, false).OrderBy(x => x.Idle).ToList();

			// step 1: remove missing file data from infos and records
			int missingFiles = 0;
			foreach (var path in infos.Select(x => x.Path).ToArray())
			{
				try
				{
					if (File.Exists(path))
						continue;

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

			// step 2: remove the most idle extra files
			int extraFiles = 0;
			while (infos.Count > MAX_FILES)
			{
				var info = infos[infos.Count - 1];
				infos.RemoveAt(infos.Count - 1);
				int removed = _records.RemoveAll(x => x.Path.Equals(info.Path, StringComparison.OrdinalIgnoreCase));
				Logger.Source.TraceInformation("Extra: {0}: {1}", removed, info.Path);
				++extraFiles;
			}

			// step 3: cound days excluding today and remove aged records
			int oldRecords = 0;
			var today = DateTime.Today;
			var days = _records.Select(x => x.Time.Date).Where(x => x != today).Distinct().OrderByDescending(x => x).ToArray();
			if (days.Length > MAX_DAYS)
			{
				var zeroDate = days[MAX_DAYS - 1];
				Logger.Source.TraceInformation("Zero: {0}", zeroDate);

				foreach (var info in infos)
				{
					if (info.UseCount > 1 && info.Head < zeroDate)
					{
						// remove all but the last
						int removed = _records.RemoveAll(x => x.Time < zeroDate && x.Time != info.Tail && x.Path.Equals(info.Path, StringComparison.OrdinalIgnoreCase));

						// find and make the last aged record less important;
						// null is rare in not standard cases, e.g. manual changes
						var record = _records.FirstOrDefault(x => x.Time == info.Tail);
						if (record != null && record.Time < zeroDate)
							record.SetAged();

						Logger.Source.TraceInformation("Aged: {0}: {1}", removed, info.Path);
						oldRecords += removed;

					}
				}
			}

			// save sorted by time
			Record.Write(_store, _records.Reverse<Record>().OrderBy(x => x.Time));

			Logger.Source.TraceEvent(TraceEventType.Stop, 0, "Update");
			return string.Format(@"
Missing files : {0,4}
Extra files   : {1,4}
Old records   : {2,4}
Records       : {3,4}
",
			missingFiles,
			extraFiles,
			oldRecords,
			_records.Count);
		}

	}
}
