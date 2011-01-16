/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.IO;
using System.Management.Automation;
using FarNet;

namespace FarMacro
{
	public class BaseCmdlet : PSCmdlet
	{
		internal const string Noun = "FarMacro";
		const string TempName = Noun;

		/// <summary>
		/// Gets existing fixed path for the temporary files.
		/// </summary>
		/// <returns></returns>
		protected static string GetTempPath()
		{
			var path = Path.Combine(Path.GetTempPath(), TempName);
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);
			return path;
		}
	}
}
