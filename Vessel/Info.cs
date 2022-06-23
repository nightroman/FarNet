
// FarNet module Vessel
// Copyright (c) Roman Kuzmin

using System;

namespace Vessel;

/// <summary>
/// Collected file information about its usage.
/// </summary>
public class Info
{
	/// <summary>
	/// File path.
	/// </summary>
	public string Path { get; set; }

	/// <summary>
	/// The last recorded time.
	/// </summary>
	public DateTime TimeN { get; set; }

	/// <summary>
	/// Likelihood of using again.
	/// </summary>
	public double Evidence { get; set; }

	//! not static is easier for experiments with members
	internal double CalculateEvidence(int count, TimeSpan age)
	{
		return count == 0 ? 0 : 1 / age.TotalDays;
	}

	public static double FactorSpanMin = 0.5;
	public static double FactorSpanMax = 1.1;

	/// <summary>
	/// For the given age gets the interval for counting evidences.
	/// </summary>
	internal static (TimeSpan, TimeSpan) GetSpanMinMax(TimeSpan age)
	{
		return (
			new TimeSpan((long)(age.Ticks * FactorSpanMin)),
			new TimeSpan((long)(age.Ticks * FactorSpanMax)));
	}
}
