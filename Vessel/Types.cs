
// FarNet module Vessel
// Copyright (c) Roman Kuzmin

using System;
using System.IO;

namespace Vessel;

public enum Mode
{
	File,
	Folder,
	Command
}

public class Result
{
	/// <summary>
	/// Number of wins.
	/// </summary>
	public int Score => UpCount - DownCount;

	/// <summary>
	/// Actual gain ~ average position win.
	/// </summary>
	public double Gain => Tests == 0 ? 0 : Math.Round((double)(UpSum - DownSum) / Tests, 2);

	/// <summary>
	/// Maximum possible gain ~ average position win.
	/// </summary>
	public double MaxGain => Tests == 0 ? 0 : Math.Round((double)MaxSum / Tests, 2);

	public int UpCount { get; set; }
	public int DownCount { get; set; }
	public int UpSum { get; set; }
	public int DownSum { get; set; }

	/// <summary>
	/// Total number of comparisons.
	/// </summary>
	public int Tests { get; set; }

	/// <summary>
	/// Maximum possible score.
	/// </summary>
	public int MaxScore { get; set; }

	/// <summary>
	/// Maximum possible gained sum.
	/// </summary>
	public int MaxSum { get; set; }
}

static class Lua
{
	public static string StringLiteral(string value)
	{
		value = value.Replace(@"\", @"\\").Replace(@"'", @"\'");
		return $"'{value}'";
	}
}

static class My
{
	public const string Name = "Vessel";
	static string AppHome => Path.GetDirectoryName(typeof(My).Assembly.Location);
	static string HelpRoot => "<" + AppHome + "\\>";
	public static string HelpTopic(string topic) => HelpRoot + topic;
}
