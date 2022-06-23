
// FarNet module Vessel
// Copyright (c) Roman Kuzmin

using FarNet;
using System;
using System.Collections.Generic;
using System.IO;

namespace Vessel;

public enum Mode
{
	File,
	Folder,
	Command
}

public class Record
{
	internal const string AGED = "aged";
	internal const string EDIT = "edit";
	internal const string GOTO = "goto";
	internal const string OPEN = "open";
	internal const string VIEW = "view";
	public DateTime Time { get; }
	public string What { get; private set; }
	public string Path { get; }

	internal Record(DateTime time, string what, string path)
	{
		Time = time;
		What = what;
		Path = path;
	}

	public void SetAged()
	{
		What = AGED;
	}

	public class Comparer : IComparer<Record>
	{
		public int Compare(Record left, Record right)
		{
			return left.Time.CompareTo(right.Time);
		}
	}
}

public class Result
{
	/// <summary>
	/// Actual gain ~ average position win.
	/// </summary>
	public double Gain => Tests == 0 ? 0 : Math.Round((double)(UpSum - DownSum) / Tests, 2);

	/// <summary>
	/// Maximum possible gain ~ average position win.
	/// </summary>
	public double MaxGain => Tests == 0 ? 0 : Math.Round((double)MaxSum / Tests, 2);

	public int Score => UpCount - DownCount;
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
	public static void BadWindow() => Far.Api.Message("Unexpected window.", Name);
	public static bool AskDiscard(string value) => 0 == Far.Api.Message(value, "Discard", MessageOptions.OkCancel);
}
