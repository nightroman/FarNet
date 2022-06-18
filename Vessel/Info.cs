
// FarNet module Vessel
// Copyright (c) Roman Kuzmin

using System;

namespace Vessel;

/// <summary>
/// Collected file information about its usage.
/// </summary>
public class Info
{
	public const int SpanCount = 10;
	static readonly int[] s_spanHours = new int[SpanCount] { 2, 2, 4, 8, 16, 32, 64, 128, 256, 1024 };

	/// <summary>
	/// File path.
	/// </summary>
	public string Path { get; set; }

	/// <summary>
	/// The first recorded time.
	/// </summary>
	public DateTime Time1 { get; set; }

	/// <summary>
	/// The last recorded time.
	/// </summary>
	public DateTime TimeN { get; set; }

	/// <summary>
	/// Count of records.
	/// </summary>
	public int UseCount { get; set; }

	/// <summary>
	/// Time passed since the last use.
	/// </summary>
	public TimeSpan Idle { get; set; }

	/// <summary>
	/// Likelihood of using again based on evidences.
	/// </summary>
	public float Evidence { get; set; }

	// TODO watch and choose the best CountPlus
	public static int CountPlus { get; set; } = 1;

	internal static float CalculateEvidence(int count, int span)
	{
		return count > 0 ? (float)(count + CountPlus) / s_spanHours[span] : 0;
	}
}
