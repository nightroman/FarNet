
// FarNet module Vessel
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.Generic;
using System.Linq;

//_220619_ry Why use full evidences on testing, not partial calculated on getting partial infos?
// Because this is the actual trained model. Applying it to partial lists is fair, not cheating.

namespace Vessel;

public partial class Actor
{
	/// <summary>
	/// Evidence spans used on model testing.
	/// </summary>
	public class SpanSet
	{
		public int[] Spans { get; } = new int[Info.SpanCount];
		internal DateTime NextRecordTime { get; set; }
	}

	/// <summary>
	/// Evaluates the trained model.
	/// </summary>
	/// <returns>Training results.</returns>
	public Result Train()
	{
		var result = new Result();

		// collect and compare evidences with existing, _220619_ry
		var spanMap = CollectEvidences();
		{
			var infos = CollectInfo(DateTime.Now, false);
			foreach (var info in infos)
			{
				var span = Mat.Span(info.Idle);
				if (span < Info.SpanCount)
				{
					var evidence = Info.CalculateEvidence(spanMap[info.Path].Spans[span], span);
					if (evidence != info.Evidence)
						throw new InvalidOperationException("Different evidences.");
				}
			}
		}

		// records from new to old
		foreach (var record in _records)
		{
			//! skip noop records, do not tune against them, they were not open from smart lists
			if (record.What == Record.NOOP)
				continue;

			// collect infos (step back 1 second and tell to exclude not interesting), then sort by times
			var infos = CollectInfo(record.Time - TimeSpan.FromSeconds(1), true).OrderBy(x => x.Idle).ToList();

			// get the simple rank
			int rankSimple = infos.FindIndex(x => x.Path.Equals(record.Path, _comparison));

			// not found means it is the first history record for the file, skip it
			if (rankSimple < 0)
				continue;

			// inject evidences, _220619_ry
			SetEvidences(infos, spanMap);

			// sort smart, get rank
			infos.Sort(new InfoComparer(_limit0));
			int rankSmart = infos.FindIndex(x => x.Path.Equals(record.Path, _comparison));

			int win = rankSimple - rankSmart;
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

	/// <summary>
	/// Counts uses by items and time spans.
	/// </summary>
	/// <returns>Dictionary of paths to time spans with counts.</returns>
	public Dictionary<string, SpanSet> CollectEvidences()
	{
		var result = new Dictionary<string, SpanSet>(_comparer);

		// from new to old
		foreach (var record in _records)
		{
			// add new span set with the record time as the next
			if (!result.TryGetValue(record.Path, out SpanSet spans))
			{
				spans = new SpanSet { NextRecordTime = record.Time };
				result.Add(record.Path, spans);
				continue;
			}

			// time passed from this record to the next
			var idle = spans.NextRecordTime - record.Time;

			// update next record time
			spans.NextRecordTime = record.Time;

			int span = Mat.Span(idle);
			if (span < Info.SpanCount)
				++spans.Spans[span];
		}

		return result;
	}

	/// <summary>
	/// Sets info evidences from collected data.
	/// </summary>
	private static void SetEvidences(IEnumerable<Info> infos, Dictionary<string, SpanSet> spanMap)
	{
		// calculate evidences for idle times
		foreach (var info in infos)
		{
			int span = Mat.Span(info.Idle);
			if (span < Info.SpanCount)
				info.Evidence = Info.CalculateEvidence(spanMap[info.Path].Spans[span], span);
		}
	}
}
