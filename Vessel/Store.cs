
// FarNet module Vessel
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Vessel;

public static class Store
{
	const string LINE_HEADER = "Time\tWhat\tPath";
	const string LINE_FORMAT = "{0:yyyy-MM-dd HH:mm:ss}\t{1}\t{2}";
	static readonly object _lock = new object();
	/// <summary>
	/// Creates the history file and imports the history.
	/// </summary>
	public static void CreateLogFile(string store)
	{
		lock (_lock)
		{
			var dir = System.IO.Path.GetDirectoryName(store);
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			using (StreamWriter writer = new StreamWriter(store, false, Encoding.UTF8))
				writer.WriteLine(LINE_HEADER);
		}
	}
	/// <summary>
	/// Reads history records from the store.
	/// </summary>
	public static IEnumerable<Record> Read(string store)
	{
		lock (_lock)
		{
			using (StreamReader reader = new StreamReader(store, Encoding.UTF8))
			{
				string line;
				int index = -1;
				var sep = new char[] { '\t' };
				var trim = new char[] { '"', ' ' };
				while (null != (line = reader.ReadLine()))
				{
					// skip header
					if (++index == 0)
						continue;

					Record record;
					try
					{
						// read and parse the record
						var values = line.Split(sep);
						if (values.Length == 3)
						{
							// current version, 3 values
							record = new Record(
								DateTime.Parse(values[0].Trim(trim)),
								values[1].Trim(trim),
								values[2].Trim(trim));
						}
						else
						{
							// old version, 4 values
							record = new Record(
								DateTime.Parse(values[0].Trim(trim)),
								values[2].Trim(trim),
								values[3].Trim(trim));
						}
					}
					catch
					{
						//! skip problems
						continue;
					}
					yield return record;
				}
			}
		}
	}
	internal static void Write(string store, IEnumerable<Record> records)
	{
		lock (_lock)
		{
			// write the temp
			string temp = store + ".tmp";
			using (StreamWriter writer = new StreamWriter(temp, false, Encoding.UTF8))
			{
				writer.WriteLine(LINE_HEADER);
				foreach (var log in records)
					writer.WriteLine(LINE_FORMAT, log.Time, log.What, log.Path);
			}

			// replace with the temp
			File.Replace(temp, store, null);
		}
	}
	public static void Append(string store, DateTime time, string what, string path)
	{
		lock (_lock)
		{
			using (StreamWriter writer = new StreamWriter(store, true, Encoding.UTF8))
				writer.WriteLine(LINE_FORMAT, time, what, path);
		}
	}
	public static void Remove(string store, string path, StringComparison comparison)
	{
		Write(store, Read(store).Where(x => !x.Path.Equals(path, comparison)));
	}
}
