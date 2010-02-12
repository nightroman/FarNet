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

		protected static string MyAppData
		{
			get
			{
				if (_MyAppData == null)
				{
					_MyAppData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Noun);
					Directory.CreateDirectory(_MyAppData);
				}
				return _MyAppData;
			}
		}
		static string _MyAppData;
	}
}
