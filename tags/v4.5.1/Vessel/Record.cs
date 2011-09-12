
/*
FarNet module Vessel
Copyright (c) 2011 Roman Kuzmin
*/

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
		public int Keys { get; private set; }
		public string What { get; private set; }
		public string Path { get; private set; }

		Record(DateTime time, int keys, string what, string path)
		{
			Time = time;
			Keys = keys;
			What = what;
			Path = path;
		}

		public void SetAged()
		{
			Keys = 0;
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
			
			using (StreamWriter writer = new StreamWriter(store, false, Encoding.Unicode))
			{
				writer.WriteLine(LINE_HEADER);

				var paths = Far.Net.GetHistory("SavedViewHistory", "01").ToList();
				for (int i = 0; i < paths.Count; ++i)
					writer.WriteLine(LINE_FORMAT, DateTime.Now - new TimeSpan(0, 0, paths.Count - i), 0, "view", paths[i]);
			}
		}

		/// <summary>
		/// Reads history records from the store.
		/// </summary>
		public static IEnumerable<Record> Read(string store)
		{
			if (string.IsNullOrEmpty(store))
				store = VesselHost.LogPath;

			using (StreamReader reader = new StreamReader(store, Encoding.Unicode))
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

					var values = line.Split(sep);
					yield return new Record(
						DateTime.Parse(values[0].Trim(trim)),
						int.Parse(values[1].Trim(trim)),
						values[2].Trim(trim),
						values[3].Trim(trim));
				}
			}
		}

		internal static void Write(string store, IEnumerable<Record> records)
		{
			using (StreamWriter writer = new StreamWriter(store, false, Encoding.Unicode))
			{
				writer.WriteLine(LINE_HEADER);
				foreach (var log in records)
					writer.WriteLine(LINE_FORMAT, log.Time, log.Keys, log.What, log.Path);
			}
		}

		public static void Write(string store, DateTime time, int keys, string what, string path)
		{
			using (StreamWriter writer = new StreamWriter(store, true, Encoding.Unicode))
				writer.WriteLine(LINE_FORMAT, time, keys, what, path);
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
