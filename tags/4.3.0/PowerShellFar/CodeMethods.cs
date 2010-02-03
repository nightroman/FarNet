/*
PowerShellFar plugin for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Text;
using Microsoft.Win32;

namespace PowerShellFar
{
	/// <summary>
	/// Infrastructure.
	/// </summary>
	public static class CodeMethods
	{
		/// <summary>
		/// Gets Far description for a FS item.
		/// </summary>
		public static string FileSystemInfoGetFarDescription(PSObject instance)
		{
			FileSystemInfo info = Cast<FileSystemInfo>.From(instance);
			if (info != null)
				return Description.Get(info.FullName);
			else
				return string.Empty;
		}

		/// <summary>
		/// Sets Far description for a FS item.
		/// </summary>
		public static void FileSystemInfoSetFarDescription(PSObject instance, string value)
		{
			FileSystemInfo info = Cast<FileSystemInfo>.From(instance);
			if (info != null)
				Description.Set(info.FullName, value);
		}

		/// <summary>
		/// Moves a file or directory and its Far description.
		/// </summary>
		/// <remarks>
		/// It is a wrapper of <c>System.IO.FileInfo.MoveTo()</c> and <c>System.IO.DirectoryInfo.MoveTo()</c>:
		/// in addition it moves the Far description.
		/// </remarks>
		public static void FileSystemInfoMoveTo(PSObject instance, string value)
		{
			FileSystemInfo info = Cast<FileSystemInfo>.From(instance);
			if (info == null)
				return;

			string path = info.FullName;
			string desc = Description.Get(path);

			FileInfo file = info as FileInfo;
			if (file != null)
				file.MoveTo(value);
			else
				((DirectoryInfo)info).MoveTo(value);

			Description.Set(path, string.Empty);
			Description.Set(info.FullName, desc);
		}

		/// <summary>
		/// Copies a file and its Far description.
		/// </summary>
		/// <remarks>
		/// It is a wrapper of <c>System.IO.FileInfo.CopyTo()</c>:
		/// in addition it copies the Far description.
		/// </remarks>
		public static FileInfo FileInfoCopyTo(PSObject instance, string value)
		{
			FileInfo file1 = Cast<FileInfo>.From(instance);
			if (file1 == null)
				return null;

			FileInfo file2 = file1.CopyTo(value);
			Description.Set(file2.FullName, Description.Get(file1.FullName));
			return file2;
		}

		/// <summary>
		/// Deletes a file and its Far description.
		/// </summary>
		/// <remarks>
		/// It is a wrapper of <c>System.IO.FileInfo.Delete()</c>:
		/// in addition it deletes the Far description.
		/// </remarks>
		public static void FileInfoDelete(PSObject instance)
		{
			FileInfo file = Cast<FileInfo>.From(instance);
			if (file == null)
				return;

			string path = file.FullName;
			file.Delete();
			Description.Set(path, string.Empty);
		}
	}

	// Directorty item description map; used for caching directory descriptions.
	class DescriptionMap
	{
		public DescriptionMap(string directory, DateTime timestamp, Dictionary<string, string> map)
		{
			Directory = directory;
			Timestamp = timestamp;
			Map = map;
		}
		public string Directory { get; private set; }
		public DateTime Timestamp { get; private set; }
		public Dictionary<string, string> Map { get; private set; }
	}

	/// <summary>
	/// Far description tools for internal use.
	/// </summary>
	static class Description
	{
		// Default description file name
		const string DefaultDescriptionName = "Descript.ion";

		// Options: registry key name
		static string RegKeyName
		{
			get
			{
				return Registry.CurrentUser.Name + "\\" + A.Far.RootFar + "\\Descriptions";
			}
		}

		// Description file names from registry, at least one.
		static string[] DescriptionNames { get { Init(); return _DescriptionNames; } }
		static string[] _DescriptionNames;

		static bool IsAnsiByDefault
		{
			get
			{
				return 0 != (int)Registry.GetValue(RegKeyName, "AnsiByDefault", 0);
			}
		}

		static bool IsSaveInUtf
		{
			get
			{
				return 0 != (int)Registry.GetValue(RegKeyName, "SaveInUtf", 0);
			}
		}

		static bool IsSetHidden
		{
			get
			{
				return 0 != (int)Registry.GetValue(RegKeyName, "SetHidden", 0);
			}
		}

		// Directory description data cache, use locked!
		static readonly WeakReference WeakCache = new WeakReference(null);

		// Gets settings once
		internal static void Init()
		{
			if (_DescriptionNames != null)
				return;

			// ListNames
			string csv = (string)Registry.GetValue(RegKeyName, "ListNames", DefaultDescriptionName);
			string[] names = csv.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			if (names.Length == 0)
				_DescriptionNames = new string[] { DefaultDescriptionName };
			else
				_DescriptionNames = names;
		}

		// Gets full path of existing or default description file for a directory.
		internal static string GetDescriptionFile(string directory, out bool exists)
		{
			directory = Path.GetFullPath(directory);
			foreach (string name in DescriptionNames)
			{
				string path = Path.Combine(directory, name);
				if (File.Exists(path))
				{
					exists = true;
					return path;
				}
			}
			exists = false;
			return Path.Combine(directory, DescriptionNames[0]);
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
			Dictionary<string, string> r = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			if (!File.Exists(descriptionFile))
				return r;

			// preamble is recognized anyway, we just set a fallback encoding
			Encoding encoding = IsAnsiByDefault ? Encoding.Default : Encoding.GetEncoding(A.Far.Zoo.OemCP);
			using (StreamReader sr = new StreamReader(descriptionFile, encoding, true))
			{
				string line;
				while ((line = sr.ReadLine()) != null)
				{
					string name, description;
					if (line.StartsWith("\"", StringComparison.Ordinal))
					{
						int i = line.IndexOf('"', 1);
						if (i < 0)
							continue;

						name = line.Substring(1, i - 1);
						description = line.Substring(i + 1).Trim();
					}
					else
					{
						int i = line.IndexOf(' ');
						if (i < 0)
							continue;

						name = line.Substring(0, i);
						description = line.Substring(i + 1).Trim();
					}

					r.Add(name, description);
				}
			}

			return r;
		}

		// Gets Far description for a FS item.
		internal static string Get(string path)
		{
			//! trim '\': FullName may have it after MoveTo() for directories
			path = path.TrimEnd('\\');

			lock (WeakCache)
			{
				// strong cache ASAP
				DescriptionMap cache = (DescriptionMap)WeakCache.Target;

				// description file, if any
				string directory = Path.GetDirectoryName(path);
				bool exists;
				string descriptionFile = GetDescriptionFile(directory, out exists);
				if (!exists)
					return string.Empty;

				// update the cache
				if (cache == null || Kit.Compare(directory, cache.Directory) != 0 || File.GetLastWriteTime(descriptionFile) != cache.Timestamp)
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
			
			lock (WeakCache)
			{
				// get data and set new value
				string directory = Path.GetDirectoryName(path);
				bool exists;
				string descriptionFile = GetDescriptionFile(directory, out exists);
				Dictionary<string, string> map = Import(descriptionFile);
				map[Path.GetFileName(path)] = value.TrimEnd().Replace("\r\n", " _ ").Replace("\r", " _ ");

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
				if (IsSaveInUtf)
					encoding = Encoding.UTF8;
				else if (IsAnsiByDefault)
					encoding = Encoding.Default;
				else
					encoding = Encoding.GetEncoding(A.Far.Zoo.OemCP);

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
				else if (IsSetHidden)
					File.SetAttributes(descriptionFile, File.GetAttributes(descriptionFile) | FileAttributes.Hidden);

				// cache
				WeakCache.Target = new DescriptionMap(Path.GetDirectoryName(descriptionFile), File.GetLastWriteTime(descriptionFile), map);
			}
		}

	}
}