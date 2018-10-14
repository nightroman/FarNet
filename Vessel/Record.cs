
// FarNet module Vessel
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FarNet.Vessel
{
	public class Record
	{
		internal const string AGED = "aged";
		const string LINE_HEADER = "Time\tKeys\tWhat\tPath";
		const string LINE_FORMAT = "{0:yyyy-MM-dd HH:mm:ss}\t{1}\t{2}\t{3}";
		public DateTime Time { get; private set; }
		public string What { get; private set; }
		public string Path { get; private set; }
		Record(DateTime time, string what, string path)
		{
			Time = time;
			What = what;
			Path = path;
		}
		public void SetAged()
		{
			What = AGED;
		}
		/// <summary>
		/// Creates the history file and imports the history.
		/// </summary>
		public static void CreateLogFile(string store)
		{
			var dir = System.IO.Path.GetDirectoryName(store);
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			using (StreamWriter writer = new StreamWriter(store, false, Encoding.UTF8))
			{
				writer.WriteLine(LINE_HEADER);

				foreach (var it in Far.Api.History.Editor())
					writer.WriteLine(LINE_FORMAT, it.Time, 0, "edit", it.Name);

				foreach (var it in Far.Api.History.Viewer())
					writer.WriteLine(LINE_FORMAT, it.Time, 0, "view", it.Name);
			}
		}
		/// <summary>
		/// Reads history records from the store.
		/// </summary>
		public static IEnumerable<Record> Read(string store)
		{
			if (string.IsNullOrEmpty(store))
				store = VesselHost.LogPath;

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

					Record record = null;
					try
					{
						// read and parse the record
						var values = line.Split(sep);
						record = new Record(
							DateTime.Parse(values[0].Trim(trim)),
							values[2].Trim(trim),
							values[3].Trim(trim));
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
		internal static void Write(string store, IEnumerable<Record> records)
		{
			// write the temp
			string temp = store + ".tmp";
			using (StreamWriter writer = new StreamWriter(temp, false, Encoding.UTF8))
			{
				writer.WriteLine(LINE_HEADER);
				foreach (var log in records)
					writer.WriteLine(LINE_FORMAT, log.Time, 0, log.What, log.Path);
			}

			// replace with the temp
			File.Replace(temp, store, null);
		}
		public static void Append(string store, DateTime time, string what, string path)
		{
			using (StreamWriter writer = new StreamWriter(store, true, Encoding.UTF8))
				writer.WriteLine(LINE_FORMAT, time, 0, what, path);
		}
		public static void Remove(string store, string path)
		{
			Write(store, Read(store).Where(x => !x.Path.Equals(path, StringComparison.OrdinalIgnoreCase)).ToList());
		}
		public static IEnumerable<Info> GetHistory(string store, DateTime now, int factor1, int factor2)
		{
			var algo = new Actor(store);
			return algo.GetHistory(now, factor1, factor2);
		}
	}
}
