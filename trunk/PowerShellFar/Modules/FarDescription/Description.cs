
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2015 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace FarDescription
{
	/// <summary>
	/// Global settings.
	/// </summary>
	public static class Settings
	{
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public static string[] ListNames { get; set; }
		public static bool AnsiByDefault { get; set; }
		public static bool SaveInUtf { get; set; }
		public static bool SetHidden { get; set; }
	}
	/// <summary>
	/// Far description tools for internal use.
	/// </summary>
	static class Description
	{
		// Directory description data cache, use locked!
		static readonly WeakReference WeakCache = new WeakReference(null);
		// Gets full path of existing or default description file for a directory.
		internal static string GetDescriptionFile(string directory, out bool exists)
		{
			directory = Path.GetFullPath(directory);
			foreach (string name in Settings.ListNames)
			{
				string path = Path.Combine(directory, name);
				if (File.Exists(path))
				{
					exists = true;
					return path;
				}
			}
			exists = false;
			return Path.Combine(directory, Settings.ListNames[0]);
		}
		// Updates the description file and removes it if it is empty.
		internal static void UpdateDescriptionFile(string descriptionFile)
		{
			Dictionary<string, string> data = Import(descriptionFile);
			string directory = Path.GetDirectoryName(descriptionFile);
			string[] names = new string[data.Count];
			data.Keys.CopyTo(names, 0);
			foreach (string name in names)
			{
				string path = Path.Combine(directory, name);
				if (!File.Exists(path) && !Directory.Exists(path))
					data.Remove(name);
			}
			Export(descriptionFile, data);
		}
		static Dictionary<string, string> Import(string descriptionFile)
		{
			var r = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			if (!File.Exists(descriptionFile))
				return r;

			// preamble is recognized anyway, we just set a fallback encoding
			Encoding encoding = Settings.AnsiByDefault ? Encoding.Default : Encoding.GetEncoding(NativeMethods.GetOEMCP());
			using (StreamReader sr = new StreamReader(descriptionFile, encoding, true))
			{
				string line;
				while ((line = sr.ReadLine()) != null)
				{
					int i;
					string name;
					if (line.StartsWith("\"", StringComparison.Ordinal))
					{
						i = line.IndexOf('"', 1);
						if (i < 0)
							continue;

						name = line.Substring(1, i - 1);
						i += 2;
					}
					else
					{
						i = line.IndexOf(' ');
						if (i < 0)
							continue;

						name = line.Substring(0, i);
						++i;
					}

					if (i >= line.Length)
						continue;

					r.Add(name, line.Substring(i).TrimEnd());
				}
			}

			return r;
		}
		// Gets Far description for a FS item.
		internal static string Get(string path)
		{
			//! trim '\': FullName may have it after MoveTo() for directories
			path = path.TrimEnd('\\');
			string directory = Path.GetDirectoryName(path);

			lock (WeakCache)
			{
				// strong cache ASAP
				DescriptionMap cache = (DescriptionMap)WeakCache.Target;

				// description file, if any
				bool exists;
				string descriptionFile = GetDescriptionFile(directory, out exists);
				if (!exists)
					return string.Empty;

				// update the cache
				if (cache == null || !string.Equals(directory, cache.Directory, StringComparison.OrdinalIgnoreCase) || File.GetLastWriteTime(descriptionFile) != cache.Timestamp)
				{
					cache = new DescriptionMap(directory, File.GetLastWriteTime(descriptionFile), Import(descriptionFile));
					WeakCache.Target = cache;
				}

				// description from cache, if any
				string value;
				return cache.Map.TryGetValue(Path.GetFileName(path), out value) ? value : string.Empty;
			}
		}
		// Sets Far description for a FS item.
		internal static void Set(string path, string value)
		{
			//! trim '\': FullName may have it after MoveTo() for directories
			path = path.TrimEnd('\\');
			string directory = Path.GetDirectoryName(path);

			// replace line breaks
			value = value.Replace("\r\n", " _ ").Replace("\r", " _ ");
			
			lock (WeakCache)
			{
				// get data and set new value
				bool exists;
				string descriptionFile = GetDescriptionFile(directory, out exists);
				Dictionary<string, string> map = Import(descriptionFile);
				map[Path.GetFileName(path)] = value;

				// export
				Export(descriptionFile, map);
			}
		}
		//! *** CHANGE CAREFULLY AND TEST WELL
		//! It may fail if another process operates on the file, too.
		//! But it seems to be rare and massive data loss is unlikely.
		//! NB: approach with a temporary file is slower and less stable.
		static void Export(string descriptionFile, Dictionary<string, string> map)
		{
			lock (WeakCache)
			{
				// sort by file name
				string[] keys = new string[map.Count];
				map.Keys.CopyTo(keys, 0);
				Array.Sort(keys, StringComparer.OrdinalIgnoreCase);

				// get existing file info
				FileAttributes attr = 0;
				bool existed = File.Exists(descriptionFile);
				if (existed)
				{
					//! Killing the file now is much less stable.
					attr = File.GetAttributes(descriptionFile);
					if (0 != (attr & (FileAttributes.ReadOnly | FileAttributes.System | FileAttributes.Hidden)))
						File.SetAttributes(descriptionFile, attr & ~(FileAttributes.ReadOnly | FileAttributes.System | FileAttributes.Hidden));
				}

				// encoding
				Encoding encoding;
				if (Settings.SaveInUtf)
					encoding = Encoding.UTF8;
				else if (Settings.AnsiByDefault)
					encoding = Encoding.Default;
				else
					encoding = Encoding.GetEncoding(NativeMethods.GetOEMCP());

				// write
				int nWritten = 0;
				using (StreamWriter sw = new StreamWriter(descriptionFile, false, encoding))
				{
					foreach (string key in keys)
					{
						string description = map[key];
						if (string.IsNullOrEmpty(description))
							continue;

						++nWritten;
						if (key.IndexOf(' ') >= 0)
							sw.WriteLine("\"" + key + "\" " + description);
						else
							sw.WriteLine(key + " " + description);
					}
				}

				// case: nothing is written
				if (nWritten == 0)
				{
					File.Delete(descriptionFile);
					WeakCache.Target = null;
					return;
				}

				// restore attributes + Archive
				if (existed)
					File.SetAttributes(descriptionFile, attr | FileAttributes.Archive);
				// add hidden attribute for a new file
				else if (Settings.SetHidden)
					File.SetAttributes(descriptionFile, File.GetAttributes(descriptionFile) | FileAttributes.Hidden);

				// cache
				WeakCache.Target = new DescriptionMap(Path.GetDirectoryName(descriptionFile), File.GetLastWriteTime(descriptionFile), map);
			}
		}
	}
}
