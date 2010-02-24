/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Globalization;
using System.Management.Automation;
using FarNet;

namespace FarMacro
{
	static class Res
	{
		public const string
			InvalidDestinationPath = "Invalid destination path.";
	}

	public class AreaItem
	{
		public MacroArea Area { get; private set; }
		public string Description { get; private set; }

		internal AreaItem(MacroArea area, string description)
		{
			Area = area;
			Description = description;
		}
	}

	class Way
	{
		public Way(string path)
		{
			if (path == null)
				return;

			path = path.TrimStart('\\');
			if (path.Length == 0)
				return;

			path = path.Replace("/", "(Slash)");

			int i = path.IndexOf('\\');
			if (i < 0)
			{
				Area = (MacroArea)Enum.Parse(typeof(MacroArea), path, true);
			}
			else
			{
				Area = (MacroArea)Enum.Parse(typeof(MacroArea), path.Substring(0, i), true);
				Name = path.Substring(i + 1);
				if (Name.Length == 0)
					throw new ArgumentException("Invalid path: " + path);
			}
		}

		public MacroArea Area { get; private set; }
		public string Name { get; private set; }
	}

	static class Kit
	{
		public static object ConvertTo(object valueToConvert, Type resultType)
		{
			return LanguagePrimitives.ConvertTo(valueToConvert, resultType, CultureInfo.InvariantCulture);
		}
	}

}
