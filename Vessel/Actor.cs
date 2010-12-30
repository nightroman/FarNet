
/*
FarNet module Vessel
Copyright (c) 2010 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace FarNet.Vessel
{
	public class Actor
	{
		readonly List<Deal> _deals;

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
			_deals = Deal.Read(store).ToList();
			_deals.Reverse();
		}

		/// <summary>
		/// Gets deals from the most recent to old.
		/// </summary>
		public ReadOnlyCollection<Deal> Deals { get { return new ReadOnlyCollection<Deal>(_deals); } }

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
			for (int iDeal = 0; iDeal < _deals.Count; ++iDeal)
			{
				// skip data from the future
				var deal = _deals[iDeal];
				if (deal.Time > now)
					continue;

				// add, skip existing
				if (!set.Add(deal.Path))
					continue;

				// skip recent
				var idle = now - deal.Time;
				if (excludeRecent && idle.TotalHours < VesselHost.Limit0)
					continue;

				// init info
				var info = new Info()
				{
					Path = deal.Path,
					Head = deal.Time,
					Tail = deal.Time,
					Idle = idle,
					KeyCount = deal.Keys,
					DayCount = 1,
					UseCount = 1,
				};

				// get the rest
				CollectFileInfo(info, iDeal + 1, now);

				yield return info;
			}
		}

		/// <summary>
		/// Collects not empty info for each deal at the deal's time.
		/// </summary>
		public IEnumerable<Info> CollectOpenInfo()
		{
			for (int iDeal = 0; iDeal < _deals.Count; ++iDeal)
			{
				var deal = _deals[iDeal];

				// init info
				var info = new Info() { Path = deal.Path };

				// get the rest
				CollectFileInfo(info, iDeal + 1, deal.Time);

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
			for (int iDeal = start; iDeal < _deals.Count; ++iDeal)
			{
				// skip data from the future
				var deal = _deals[iDeal];
				if (deal.Time > now)
					continue;

				// skip alien
				if (!deal.Path.Equals(info.Path, StringComparison.OrdinalIgnoreCase))
					continue;

				// count cases, init if not yet
				if (++info.UseCount == 1)
				{
					info.Head = deal.Time;
					info.Tail = deal.Time;
					info.Idle = now - deal.Time;
					info.KeyCount = deal.Keys;
					info.DayCount = 1;
					continue;
				}

				// sum keys
				info.KeyCount += deal.Keys;

				// different days
				if (info.Head.Date != deal.Time.Date)
				{
					++info.DayCount;

					// Why 2 is the best for all deals so far, 3..4 are so so, and 5 is bad?
					// NB
					// Factor 4 gives values 0..4 (max is used); factors 5..6 still give 0..4 (max is not used).
					// Should we just use 4 or should we dig the max factor value M which gives values 0..M?
					// NB
					// With 2 and Idle > X Activity is rare/never actually equal to 2.
					if ((now - deal.Time).TotalDays < 2)
						++info.Activity;
				}

				// cases of idle larger than the current idle
				var idle = info.Head - deal.Time;
				if (idle > info.Idle)
					++info.Frequency;

				// now save the deal time
				info.Head = deal.Time;
			}
		}

		/// <summary>
		/// Gets all the training results.
		/// </summary>
		public IList<Result> TrainingResults { get { return _TrainingResults; } }
		List<Result> _TrainingResults;

		/// <summary>
		/// Trains the ranking model based on factors.
		/// </summary>
		/// <param name="limit1">Hours of the period 1.</param>
		/// <param name="limit2">Days of the period 2.</param>
		/// <returns>The best result found on training.</returns>
		public Result Train(int limit1, int limit2)
		{
			_TrainingResults = new List<Result>((limit1 + 1) * (limit2 + 1));
			for (int i = 0; i <= limit1; ++i)
				for (int j = 0; j <= limit2; ++j)
					_TrainingResults.Add(new Result() { Factor1 = i, Factor2 = j });

			return Train();
		}

		Result Train()
		{
			// process records
			VesselTool.TrainingRecordCount = Deals.Count;
			VesselTool.TrainingRecordIndex = 0;
			foreach (var deal in Deals)
			{
				++VesselTool.TrainingRecordIndex;

				// collect (step back 1 tick, ask to exclude recent) and sort by Idle
				var infos = CollectInfo(deal.Time - new TimeSpan(1), true).OrderBy(x => x.Idle).ToList();

				// get the plain rank (it is the same for all other ranks)
				int rankPlain = infos.FindIndex(x => x.Path.Equals(deal.Path, StringComparison.OrdinalIgnoreCase));

				// not found means it is the first history record for the file, skip it
				if (rankPlain < 0)
					continue;

				// sort with factors, get smart rank
				foreach (var r in _TrainingResults)
				{
					infos.Sort(new InfoComparer(r.Factor1, r.Factor2));
					int rankSmart = infos.FindIndex(x => x.Path.Equals(deal.Path, StringComparison.OrdinalIgnoreCase));

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

		Result TrainFast(int factor1, int factor2)
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

		internal Result TrainFast()
		{
			var sw = Stopwatch.StartNew();
			Logger.Source.TraceEvent(TraceEventType.Start, 0, "Training {0}", DateTime.Now);

			int factor1 = VesselHost.Factor1, factor2 = VesselHost.Factor2;
			if (factor1 < 0)
			{
				factor1 = xRadius;
				factor2 = yRadius;
			}

			for (; ; )
			{
				var result = TrainFast(factor1, factor2);
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

	}
}
