
/*
FarNet module Vessel
Copyright (c) 2010 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FarNet.Vessel
{
	public class Algo
	{
		readonly List<Deal> _deals;

		/// <summary>
		/// Creates the instance with data from the default storage.
		/// </summary>
		public Algo() : this(null) { }

		/// <summary>
		/// Creates the instance with data ready for analyses.
		/// </summary>
		/// <param name="store">The store path. Empty/null is for the default.</param>
		public Algo(string store)
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
			var infos = CollectInfo(now);

			// order
			if (factor1 < 0)
				return infos.OrderByDescending(x => x.Idle);
			else
				return infos.OrderByDescending(x => x, new InfoComparer(factor1, factor2));
		}

		/// <summary>
		/// Collects the unordered history info.
		/// </summary>
		public IEnumerable<Info> CollectInfo(DateTime now)
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

				// init info
				var info = new Info()
				{
					Path = deal.Path,
					Head = deal.Time,
					Tail = deal.Time,
					Idle = now - deal.Time,
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
		/// Gets or sets the progress form.
		/// </summary>
		internal Tools.ProgressForm Progress { get; set; }
		
		/// <summary>
		/// Gets all the training results.
		/// </summary>
		public Result[,] TrainingResults { get { return _TrainingResults; } }
		Result[,] _TrainingResults;

		/// <summary>
		/// Trains the ranking model based on factors.
		/// </summary>
		/// <param name="limit1">Hours of the period 1.</param>
		/// <param name="limit2">Days of the period 2.</param>
		/// <returns>The best result found on training.</returns>
		public Result Train(int limit1, int limit2)
		{
			++limit1;
			++limit2;

			_TrainingResults = new Result[limit1, limit2];
			for (int i = 0; i < limit1; ++i)
				for (int j = 0; j < limit2; ++j)
					_TrainingResults[i, j] = new Result() { Factor1 = i, Factor2 = j };

			int recordCount = 0;
			foreach (var deal in Deals)
			{
				++recordCount;
				if (Progress != null)
				{
					Progress.Activity = "Record " + recordCount + " out of " + Deals.Count;
					Progress.SetProgressValue(recordCount, Deals.Count);
				}
				
				// collect (step back 1 tick) and sort by Idle
				var infos = CollectInfo(deal.Time - new TimeSpan(1)).OrderBy(x => x.Idle).ToList();

				// get the plain rank (it is the same for all other ranks)
				int rankPlain = infos.FindIndex(x => x.Path.Equals(deal.Path, StringComparison.OrdinalIgnoreCase));

				// not found means it is the first history record for the file, skip it
				if (rankPlain < 0)
					continue;

				// sort with factors, get smart rank
				var info = infos[rankPlain];
				for (int i = 0; i < limit1; ++i)
				{
					for (int j = 0; j < limit2; ++j)
					{
						var r = _TrainingResults[i, j];
						if (info.Recency(r.Factor1, r.Factor2) == 0)
						{
							++r.SameCount;
							continue;
						}

						infos.Sort(new InfoComparer(r.Factor1, r.Factor2));
						int rankSmart = infos.FindIndex(x => x.Path.Equals(deal.Path, StringComparison.OrdinalIgnoreCase));
						if (rankSmart < 0)
							throw new InvalidOperationException("ERROR_101224_015624");

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
			}

			// return the best result
			Result result = null;
			int maxTarget = int.MinValue;
			foreach (var it in _TrainingResults)
			{
				int target = it.Target;
				if (maxTarget < target)
				{
					result = it;
					maxTarget = target;
				}
			}

			return result;
		}

	}
}
