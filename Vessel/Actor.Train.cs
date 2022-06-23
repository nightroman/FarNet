
// FarNet module Vessel
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.Generic;
using System.Linq;

//_220619_ry Why use full evidences on testing, not partial calculated on getting partial infos?
// Because this is the model used for actual ranking and we test its performance, not partial.

namespace Vessel;

public partial class Actor
{
	public class TrainArgs
	{
	}

	/// <summary>
	/// Evaluates the trained model.
	/// </summary>
	/// <returns>Training results.</returns>
	public Result Train(TrainArgs args = null)
	{
		var result = new Result();
		if (_records.Count == 0)
			return result;

		var now = _records[_records.Count - 1].Time + _limit0 + TimeSpan.FromSeconds(1);
		args ??= new TrainArgs();

		// collect spans and compare evidences to avoid bugs, _220619_ry
		var spans = CollectSpans();
		{
			var infos = CollectInfo(now, true).Values;
			foreach (var info in infos)
			{
				var age = now - info.TimeN;
				var (min, max) = Info.GetSpanMinMax(age);
				double evidence = 0;
				if (spans[info.Path].Exists(x => x > min && x < max))
					evidence = info.CalculateEvidence(1, age);
				if (evidence != info.Evidence)
					throw new InvalidOperationException("Different evidences.");
			}
		}

		foreach (var record in _records)
		{
			// skip too old, not interesting
			if (record.What == Record.AGED)
				continue;

			// collect infos (step back 1 second and exclude not interesting), then sort by times
			var timeTrain = record.Time - TimeSpan.FromSeconds(1);
			var infos = CollectInfo(timeTrain, true, true).Values.OrderByDescending(x => x.TimeN).ToList();

			// get the simple rank
			int rankSimple = infos.FindIndex(x => x.Path.Equals(record.Path, _comparison));

			// not found means stepping back in time has discarded the last record
			if (rankSimple < 0)
				continue;

			// test stats
			++result.Tests;
			result.MaxSum += rankSimple;
			result.MaxScore += rankSimple > 0 ? 1 : 0;

			// inject evidences, _220619_ry
			SetEvidences(infos, spans, timeTrain);

			// sort smart, get rank
			infos.Sort(new InfoComparer(timeTrain - _limit0));
			int rankSmart = infos.FindIndex(x => x.Path.Equals(record.Path, _comparison));

			// score and gain stats
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
		}

		return result;
	}

	/// <summary>
	/// Collects spans between uses by paths.
	/// </summary>
	/// <returns>Dictionary of paths to idles.</returns>
	public Dictionary<string, List<TimeSpan>> CollectSpans()
	{
		var map = new Dictionary<string, List<TimeSpan>>(_comparer);

		// from new to old
		for (int iRecord1 = _records.Count - 1; iRecord1 >= 0; --iRecord1)
		{
			var record = _records[iRecord1];
			if (map.ContainsKey(record.Path))
				continue;

			var thisPath = record.Path;
			var lastTime = record.Time;
			var spans = new List<TimeSpan>();
			map.Add(thisPath, spans);

			for (int iRecord2 = iRecord1 - 1; iRecord2 >= 0; --iRecord2)
			{
				record = _records[iRecord2];
				if (record.Path.Equals(thisPath, _comparison))
				{
					spans.Add(lastTime - record.Time);
					lastTime = record.Time;
				}
			}
		}

		return map;
	}

	/// <summary>
	/// Sets info evidences from collected data.
	/// </summary>
	private static void SetEvidences(IEnumerable<Info> infos, Dictionary<string, List<TimeSpan>> spans, DateTime now)
	{
		foreach (var info in infos)
		{
			var (min, max) = Info.GetSpanMinMax(now - info.TimeN);
			if (spans[info.Path].Exists(x => x > min && x < max))
				info.Evidence = info.CalculateEvidence(1, max - min);
			else
				info.Evidence = 0;
		}
	}
}
