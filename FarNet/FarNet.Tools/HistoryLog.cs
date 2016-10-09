
/*
FarNet.Tools for Far Manager
Copyright (c) 2006-2016 Roman Kuzmin
*/

// Encoding: UTF8 with no BOM (same as in logging tools, BinaryFormatter, etc).
// Text line history files are rather logs, not text files for an editor.
// IO.File methods by default work in this way.

using System.Collections.Generic;
using System.IO;

namespace FarNet.Tools
{
	/// <summary>
	/// TODO
	/// </summary>
	public sealed class HistoryLog
	{
		readonly string _fileName;
		readonly int _maximumCount;
		string _lastLine;
		/// <summary>
		/// TODO
		/// </summary>
		public HistoryLog(string fileName, int maximumCount)
		{
			_fileName = fileName;
			_maximumCount = maximumCount;
		}
		void WriteLines(string[] lines)
		{
			File.WriteAllLines(_fileName, lines);
		}
		/// <summary>
		/// TODO Gets history lines.
		/// </summary>
		public string[] ReadLines()
		{
			// get lines
			try
			{
				var lines = File.ReadAllLines(_fileName);
				if (lines.Length > _maximumCount + _maximumCount / 10)
					return Update(lines);
				else
					return lines;
			}
			catch (FileNotFoundException)
			{
				return new string[0];
			}
		}
		/// <summary>
		/// TODO Removes dupes and extra lines.
		/// </summary>
		public string[] Update(string[] lines)
		{
			// ensure lines
			if (lines == null)
				lines = ReadLines();

			// copy lines
			var list = new List<string>(lines);

			// remove dupes
			var hash = new HashSet<string>();
			for (int i = lines.Length; --i >= 0;)
			{
				var line = lines[i];
				if (!hash.Add(line))
					list.RemoveAt(i);
			}

			// remove lines above the limit
			int removeCount = list.Count - _maximumCount;
			if (removeCount > 0)
				list.RemoveRange(0, removeCount);

			// return the same lines
			if (lines.Length == list.Count)
				return lines;

			// write and return new lines
			lines = list.ToArray();
			WriteLines(lines);
			return lines;
		}
		/// <summary>
		/// TODO Add a new history line.
		/// </summary>
		public void AddLine(string value)
		{
			if (value == _lastLine)
				return;

			_lastLine = value;
			using (var writer = File.AppendText(_fileName))
				writer.WriteLine(value);
		}
	}
}
