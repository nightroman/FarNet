
/*
FarNet module Vessel
Copyright (c) 2010 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FarNet.Vessel
{
	public class Deal
	{
		const string LINE_HEADER = "Time\tKeys\tWhat\tPath";
		const string LINE_FORMAT = "{0:yyyy-MM-dd HH:mm:ss}\t{1}\t{2}\t{3}";

		public DateTime Time { get; private set; }
		public int Keys { get; private set; }
		public string What { get; private set; }
		public string Path { get; private set; }

		Deal(DateTime time, int keys, string what, string path)
		{
			Time = time;
			Keys = keys;
			What = what;
			Path = path;
		}

		/// <summary>
		/// Creates the history file and imports the history.
		/// </summary>
		public static void CreateLogFile(string store)
		{
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
		public static IEnumerable<Deal> Read(string store)
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
					yield return new Deal(
						DateTime.Parse(values[0].Trim(trim)),
						int.Parse(values[1].Trim(trim)),
						values[2].Trim(trim),
						values[3].Trim(trim));
				}
			}
		}

		static void Write(string store, IEnumerable<Deal> deals)
		{
			using (StreamWriter writer = new StreamWriter(store, false, Encoding.Unicode))
			{
				writer.WriteLine(LINE_HEADER);
				foreach (var log in deals)
					writer.WriteLine(LINE_FORMAT, log.Time, log.Keys, log.What, log.Path);
			}
		}

		public static void Write(string store, DateTime time, int keys, string what, string path)
		{
			using (StreamWriter writer = new StreamWriter(store, true, Encoding.Unicode))
				writer.WriteLine(LINE_FORMAT, time, keys, what, path);
		}

		public static string Update(string store)
		{
			var sb = new StringBuilder();

			var deals = Read(store).ToList();

			// cound days and remove old records
			var days = deals.Select(x => x.Time.Date).Distinct().OrderBy(x => x).ToList();
			sb.AppendLine("Total days: " + days.Count);
			int daysToDiscard = days.Count - 30;
			if (daysToDiscard > 0)
			{
				sb.AppendLine("Discarded days: " + daysToDiscard);
				for (int i = 0; i < daysToDiscard; ++i)
					deals.RemoveAll(x => x.Time.Date == days[i]);
			}

			// find and remove missing file records
			foreach (var path in deals.Select(x => x.Path).Distinct(StringComparer.OrdinalIgnoreCase).ToList())
			{
				// skip existing or unknown files
				try
				{
					if (!File.Exists(path))
					{
						sb.AppendLine("Missing: " + path);
						deals.RemoveAll(x => x.Path.Equals(path, StringComparison.OrdinalIgnoreCase));
					}
				}
				catch (Exception ex)
				{
					sb.AppendLine("Error: " + path + ": " + ex.Message);
				}
			}

			// save sorted by date
			Write(store, deals.OrderBy(x => x.Time));

			// result info
			return sb.ToString();
		}

		public static void Remove(string store, string path)
		{
			Write(store, Read(store).Where(x => !x.Path.Equals(path, StringComparison.OrdinalIgnoreCase)).ToList());
		}

		public static IEnumerable<Info> GetHistory(string store, DateTime now, int factor1, int factor2)
		{
			var algo = new Algo(store);
			return algo.GetHistory(now, factor1, factor2);
		}

	}
}
