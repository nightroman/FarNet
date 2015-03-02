
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2015 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Runtime.InteropServices;

namespace FarDescription
{
	/// <summary>
	/// This class exposes some Windows API native methods.
	/// </summary>
	static class NativeMethods //! name ~ FxCop
	{
		[DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
		internal static extern int GetOEMCP();
	}

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
			if (instance == null)
				throw new ArgumentNullException("instance");

			FileSystemInfo info = instance.BaseObject as FileSystemInfo;
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
			if (instance == null)
				throw new ArgumentNullException("instance");

			FileSystemInfo info = instance.BaseObject as FileSystemInfo;
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
		public static object FileSystemInfoMoveTo(PSObject instance, string value)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			FileSystemInfo info = instance.BaseObject as FileSystemInfo;
			if (info == null)
				return null;

			string path = info.FullName;
			string desc = Description.Get(path);

			FileInfo file = info as FileInfo;
			if (file != null)
				file.MoveTo(value);
			else
				((DirectoryInfo)info).MoveTo(value);

			Description.Set(path, string.Empty);
			Description.Set(info.FullName, desc);
			return null;
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
			if (instance == null)
				throw new ArgumentNullException("instance");

			FileInfo file1 = instance.BaseObject as FileInfo;
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
		public static object FileInfoDelete(PSObject instance)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			FileInfo file = instance.BaseObject as FileInfo;
			if (file == null)
				return null;

			string path = file.FullName;
			file.Delete();
			Description.Set(path, string.Empty);
			return null;
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
}
