
// FarNet.Tools for Far Manager
// Copyright (c) Roman Kuzmin

// Encoding: UTF8 with no BOM (same as in logging tools, BinaryFormatter, etc).
// Text line history files are rather logs, not text files for an editor.
// IO.File methods by default work in this way.

using System.Collections.Generic;
using System.IO;

namespace FarNet.Tools
{
	/// <summary>
	/// The tool for reading, writing, and cleaning history logs.
	/// </summary>
	/// <seealso cref="HistoryMenu"/>.
	public sealed class HistoryLog
	{
		readonly string _fileName;
		readonly int _maximumCount;
		string _lastLine;
		/// <summary>
		/// New history log.
		/// </summary>
		/// <param name="fileName">History log file name.</param>
		/// <param name="maximumCount">Maximum number of history records.</param>
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
		/// Gets history lines.
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
		/// Removes duplicated lines, then lines above the maximum.
		/// </summary>
		/// <param name="lines">Input lines.</param>
		/// <returns>Output lines.</returns>
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
		/// Adds a new history line.
		/// </summary>
		/// <param name="value">History line.</param>
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
