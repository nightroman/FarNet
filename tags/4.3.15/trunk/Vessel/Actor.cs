
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
		readonly string _store;
		readonly List<Record> _records;

		private int _FastStep1 = 20;
		private int _FastStep2 = 4;

		/// <summary>
		/// Gets or sets the fast training step 1.
		/// </summary>
		public int FastStep1
		{
			get { return _FastStep1; }
			set { _FastStep1 = value; }
		}

		/// <summary>
		/// Gets or sets the fast training step 2.
		/// </summary>
		public int FastStep2
		{
			get { return _FastStep2; }
			set { _FastStep2 = value; }
		}

		/// <summary>
		/// Gets or sets the random number generator.
		/// </summary>
		/// <remarks>
		/// It is designed for tests in order to repeat the same sequences.
		/// If it is not set then a time based instance is created internally.
		/// </remarks>
		public Random Random
		{
			get { return _Random_ ?? (_Random_ = new Random()); }
			set { _Random_ = value; }
		}
		Random _Random_;

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

			// evidences
			SetEvidences(infos, CollectEvidences());
			return infos.OrderByDescending(x => x, new InfoComparer(Settings.Default.Limit0, factor1, factor2));
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
				if (reduced && (idle.TotalHours < Settings.Default.Limit0 || record.What == Record.AGED))
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

				result.Add(info);
			}

			return result;
		}

		public Dictionary<string, SpanSet> CollectEvidences()
		{
			var result = new Dictionary<string, SpanSet>(StringComparer.OrdinalIgnoreCase);
			var set1 = new SpanSet();

			foreach (var record in _records)
			{
				SpanSet set2;
				if (!result.TryGetValue(record.Path, out set2))
				{
					set2 = new SpanSet() { Time = record.Time };
					result.Add(record.Path, set2);
					continue;
				}

				var idle = set2.Time - record.Time;
				set2.Time = record.Time;

				int span = Mat.EvidenceSpan(idle.TotalHours, Info.SpanScale);
				if (span < Info.SpanCount)
				{
					++set1.Spans[span];
					++set2.Spans[span];
				}
			}

			result.Add(string.Empty, set1);
			return result;
		}

		/// <summary>
		/// Sets info evidences from collected data.
		/// </summary>
		static void SetEvidences(IEnumerable<Info> infos, Dictionary<string, SpanSet> map)
		{
			// total counts
			var spans = map[string.Empty].Spans;

			// calculate evidences for idle times
			foreach (var info in infos)
			{
				int span = Mat.EvidenceSpan(info.Idle.TotalHours, Info.SpanScale);
				if (span >= Info.SpanCount)
					continue;

				// skip singles, use at least two cases, or we get huge overfitting
				int count = map[info.Path].Spans[span];
				if (count >= 2)
					info.Evidence = 100 * count / spans[span];
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
				if (info.Idle.TotalHours < Settings.Default.Limit0)
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

				// now save the record time
				info.Head = record.Time;
			}
		}

		/// <summary>
		/// Gets all the training results.
		/// </summary>
		public IList<Result> TrainingResults { get { return _TrainingResults; } }
		List<Result> _TrainingResults;

		/// <summary>
		/// Makes training.
		/// </summary>
		Result Train()
		{
			var Limit0 = Settings.Default.Limit0;
			
			// evidences once
			var map = CollectEvidences();

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

				// evidences
				SetEvidences(infos, map);

				// sort with factors, get smart rank
				foreach (var r in _TrainingResults)
				{
					infos.Sort(new InfoComparer(Limit0, r.Factor1, r.Factor2));
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
			var Limit0 = Settings.Default.Limit0;
			var Limit1 = Settings.Default.Limit1;
			var Limit2 = Settings.Default.Limit2;

			int x1 = Math.Max(0, factor1 - xRadius);
			int y1 = Math.Max(0, factor2 - yRadius);
			int x2 = Math.Min(Limit1, factor1 + xRadius);
			int y2 = Math.Min(Limit2, factor2 + yRadius);

			_TrainingResults = new List<Result>(
				(x2 - x1 + 1) * (y2 - y1 + 1) + (Limit1 / _FastStep1 + 1) * (Limit2 / _FastStep2 + 1));

			for (int i = x1; i <= x2; ++i)
				for (int j = y1; j <= y2; ++j)
					_TrainingResults.Add(new Result() { Factor1 = i, Factor2 = j });

			int xStart = Random.Next(_FastStep1);
			int yStart = Random.Next(_FastStep2);
			for (int i = xStart; i <= Limit1; i += _FastStep1)
			{
				for (int j = yStart; j <= Limit2; j += _FastStep2)
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
				for (int j = i / 24; j <= limit2; ++j)
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

			// sanity
			int maxDays = Settings.Default.MaximumDayCount;
			if (maxDays < 30)
				throw new InvalidOperationException("Use at least 30 as the maximum day count.");
			int maxFiles = Settings.Default.MaximumFileCount;
			if (maxFiles < 100)
				throw new InvalidOperationException("Use at least 100 as the maximum file count.");

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
			while (infos.Count > maxFiles)
			{
				var info = infos[infos.Count - 1];
				infos.RemoveAt(infos.Count - 1);
				int removed = _records.RemoveAll(x => x.Path.Equals(info.Path, StringComparison.OrdinalIgnoreCase));
				Logger.Source.TraceInformation("Extra: {0}: {1}", removed, info.Path);
				++extraFiles;
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
			Record.Write(_store, _records.Reverse<Record>().OrderBy(x => x.Time));

			Logger.Source.TraceEvent(TraceEventType.Stop, 0, "Update");
			return string.Format(@"
Missing files : {0,4}
Extra files   : {1,4}
Old records   : {2,4}

Aged files    : {3,4}
Used files    : {4,4}
Records       : {5,4}
",
			missingFiles,
			extraFiles,
			oldRecords,
			agedFiles,
			infos.Count - agedFiles,
			_records.Count);
		}

	}
}
