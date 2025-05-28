
using System.Text;

namespace Vessel;

public static class Store
{
	const string LINE_HEADER = "Time\tWhat\tPath";
	const string LINE_FORMAT = "{0:yyyy-MM-dd HH:mm:ss}\t{1}\t{2}";

	static readonly Lock _lock = new();

	/// <summary>
	/// Creates the history file and imports the history.
	/// </summary>
	public static void CreateLogFile(string store)
	{
		lock (_lock)
		{
			var dir = Path.GetDirectoryName(store);
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			using var writer = new StreamWriter(store, false, Encoding.UTF8);
			writer.WriteLine(LINE_HEADER);
		}
	}

	/// <summary>
	/// Reads history records from the store.
	/// </summary>
	public static List<Record> Read(string store)
	{
		var res = new List<Record>();
		lock (_lock)
		{
			using var reader = new StreamReader(store, Encoding.UTF8);

			string line;
			int index = -1;
			var sep = new char[] { '\t' };
			while (null != (line = reader.ReadLine()))
			{
				// skip header
				if (++index == 0)
					continue;

				try
				{
					// read and parse the record
					var values = line.Split(sep);
					res.Add(new Record(
						DateTime.Parse(values[0]),
						values[1],
						values[2]));
				}
				catch (Exception ex)
				{
					throw new Exception($"Cannot parse line: {line} -- {ex.Message}", ex);
				}
			}
		}
		return res;
	}

	internal static void Write(string store, IEnumerable<Record> records)
	{
		lock (_lock)
		{
			// write the temp
			string temp = store + ".tmp";

			//! using block closes the file
			using (var writer = new StreamWriter(temp, false, Encoding.UTF8))
			{
				writer.WriteLine(LINE_HEADER);
				foreach (var log in records)
					writer.WriteLine(LINE_FORMAT, log.Time, log.What, log.Path);
			}

			// replace the file with temp
			File.Replace(temp, store, null);
		}
	}

	public static void Append(string store, DateTime time, string what, string path)
	{
		lock (_lock)
		{
			using var writer = new StreamWriter(store, true, Encoding.UTF8);
			writer.WriteLine(LINE_FORMAT, time, what, path);
		}
	}

	public static void Remove(string store, string path, StringComparison comparison)
	{
		Write(store, Read(store).Where(x => !x.Path.Equals(path, comparison)));
	}
}
