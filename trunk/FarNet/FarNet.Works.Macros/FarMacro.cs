/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace FarNet.Works
{
	/// <summary>
	/// The only macro operator exposed by a host.
	/// </summary>
	/// <remarks>
	/// A host derives from this class and implements abstracts.
	/// On requests it gets the solid singleton or a new instance.
	/// The singleton is set internally to the first created instance.
	/// </remarks>
	public abstract class FarMacro : IMacro
	{
		static FarMacro _Instance;

		public static FarMacro Instance { get { return _Instance; } }

		protected FarMacro()
		{
			if (_Instance != null)
				throw new InvalidOperationException();

			_Instance = this;
		}

		#region override
		
		public override object GetConstant(string name)
		{
			return GetScalar(MacroArea.Consts, name);
		}

		public override Macro GetMacro(MacroArea area, string name)
		{
			if (name == null)
				throw new ArgumentNullException("name");

			// _100211_140534 FarMacro workaround
			name = name.Replace("(Slash)", "/");

			using (IRegistryKey key = Far.Net.OpenRegistryKey("KeyMacros\\" + area + "\\" + name, false))
			{
				if (null == key)
					return null;

				Macro r = new Macro();
				r.Area = area;
				r.Name = name;

				// sequence
				object value = key.GetValue("Sequence", null);
				if (null != value)
					r.Sequence = RegistryValueToObject(value).ToString();

				// others
				r.Description = key.GetValue("Description", string.Empty).ToString();
				r.EnableOutput = !ToBool(key.GetValue("DisableOutput", 0));
				r.DisablePlugins = ToBool(key.GetValue("NoSendKeysToPlugins", null));
				r.RunAfterFarStart = ToBool(key.GetValue("RunAfterFarStart", null));
				r.CommandLine = GetThreeState(key.GetValue("NotEmptyCommandLine", null), key.GetValue("EmptyCommandLine", null));
				r.SelectedText = GetThreeState(key.GetValue("EVSelection", null), key.GetValue("NoEVSelection", null));
				r.SelectedItems = GetThreeState(key.GetValue("Selection", null), key.GetValue("NoSelection", null));
				r.PanelIsPlugin = GetThreeState(key.GetValue("NoFilePanels", null), key.GetValue("NoPluginPanels", null));
				r.ItemIsDirectory = GetThreeState(key.GetValue("NoFiles", null), key.GetValue("NoFolders", null));
				r.SelectedItems2 = GetThreeState(key.GetValue("PSelection", null), key.GetValue("NoPSelection", null));
				r.PanelIsPlugin2 = GetThreeState(key.GetValue("NoFilePPanels", null), key.GetValue("NoPluginPPanels", null));
				r.ItemIsDirectory2 = GetThreeState(key.GetValue("NoPFiles", null), key.GetValue("NoPFolders", null));
				return r;
			}
		}

		public override string[] GetNames(MacroArea area)
		{
			using (IRegistryKey key = Far.Net.OpenRegistryKey("KeyMacros\\" + (area == MacroArea.None ? "" : area.ToString()), false))
			{
				if (null == key)
					return new string[0];

				if (area == MacroArea.Consts || area == MacroArea.Vars)
					return key.GetValueNames();
				else
					return key.GetSubKeyNames();
			}
		}

		public override object GetVariable(string name)
		{
			return GetScalar(MacroArea.Vars, name);
		}

		public override void Install(Macro[] macros)
		{
			if (null == macros)
				return;

			if (!ManualSaveLoad)
				Save();

			var done = new List<string>();
			try
			{
				foreach (Macro macro in macros)
				{
					string path1 = string.Format(null, "{0}\\{1}", macro.Area, macro.Name);
					string path2 = path1.ToUpperInvariant();
					if (done.Contains(path2))
						throw new InvalidOperationException(string.Format(null, "Macro '{0}' is defined twice.", path1));

					done.Add(path2);
					Install(macro);
				}
			}
			finally
			{
				if (!ManualSaveLoad)
					Load();
			}
		}

		public override void InstallConstant(string name, object value)
		{
			InstallScalar(MacroArea.Consts, name, value);
		}

		public override void InstallVariable(string name, object value)
		{
			InstallScalar(MacroArea.Vars, name, value);
		}

		public override void Remove(MacroArea area, string[] names)
		{
			if (area == MacroArea.None)
				throw new ArgumentException("Invalid 'area'.");

			if (!ManualSaveLoad)
				Save();

			if (null == names || names.Length == 0 || names.Length == 1 && string.IsNullOrEmpty(names[0]))
			{
				Remove(area, String.Empty);
				return;
			}

			try
			{
				foreach (string name in names)
					if (!string.IsNullOrEmpty(name))
						Remove(area, name);
			}
			finally
			{
				if (!ManualSaveLoad)
					Load();
			}
		}

		#endregion

		#region private

		static void Install(Macro macro)
		{
			if (null == macro)
				throw new ArgumentNullException("macro");

			string macroName = macro.Name;

			if (!string.IsNullOrEmpty(macroName))
			{
				// _100211_140534 Take into account FarMacro workaround
				macroName = macroName.Replace("(Slash)", "/");

				// remove the old
				Remove(macro.Area, macroName);
			}

			using (IRegistryKey key = Far.Net.OpenRegistryKey("KeyMacros\\" + macro.Area.ToString() + "\\" + macroName, true))
			{
				if (string.IsNullOrEmpty(macroName))
					return;

				// sequence
				string[] lines = Regex.Split(macro.Sequence, Kit.SplitLinePattern);
				if (lines.Length == 1)
					key.SetValue("Sequence", lines[0]);
				else
					key.SetValue("Sequence", lines);

				// others
				key.SetValue("Description", macro.Description);
				if (!macro.EnableOutput)
					key.SetValue("DisableOutput", 1);
				if (macro.DisablePlugins)
					key.SetValue("NoSendKeysToPlugins", 1);
				if (macro.RunAfterFarStart)
					key.SetValue("RunAfterFarStart", 1);
				if (macro.CommandLine.Length > 0)
					key.SetValue((macro.CommandLine == "1" ? "NotEmptyCommandLine" : "EmptyCommandLine"), 1);
				if (macro.SelectedText.Length > 0)
					key.SetValue((macro.SelectedText == "1" ? "EVSelection" : "NoEVSelection"), 1);
				if (macro.SelectedItems.Length > 0)
					key.SetValue((macro.SelectedItems == "1" ? "Selection" : "NoSelection"), 1);
				if (macro.PanelIsPlugin.Length > 0)
					key.SetValue((macro.PanelIsPlugin == "1" ? "NoFilePanels" : "NoPluginPanels"), 1);
				if (macro.ItemIsDirectory.Length > 0)
					key.SetValue((macro.ItemIsDirectory == "1" ? "NoFiles" : "NoFolders"), 1);
				if (macro.SelectedItems2.Length > 0)
					key.SetValue((macro.SelectedItems2 == "1" ? "PSelection" : "NoPSelection"), 1);
				if (macro.PanelIsPlugin2.Length > 0)
					key.SetValue((macro.PanelIsPlugin2 == "1" ? "NoFilePPanels" : "NoPluginPPanels"), 1);
				if (macro.ItemIsDirectory2.Length > 0)
					key.SetValue((macro.ItemIsDirectory2 == "1" ? "NoPFiles" : "NoPFolders"), 1);
			}
		}

		void InstallScalar(MacroArea area, string name, object value)
		{
			if (null == name)
				throw new ArgumentNullException("name");
			if (null == value)
				throw new ArgumentNullException("value");

			using (IRegistryKey key = Far.Net.OpenRegistryKey("KeyMacros\\" + area, true))
			{
				key.SetValue(name, ((value is string) ? StringToRegistryValue(value.ToString()) : value));

				if (!ManualSaveLoad)
					Load();
			}
		}

		static void Remove(MacroArea area, string name)
		{
			using (IRegistryKey key = Far.Net.OpenRegistryKey("KeyMacros\\" + area, true))
			{
				if ((area == MacroArea.Consts || area == MacroArea.Vars) && !string.IsNullOrEmpty(name))
				{
					key.SetValue(name, null);
				}
				else
				{
					// _100211_140534 FarMacro workaround
					name = name.Replace("(Slash)", "/");

					key.DeleteSubKey(name);
				}
			}
		}

		static object GetScalar(MacroArea area, string name)
		{
			using (IRegistryKey key = Far.Net.OpenRegistryKey("KeyMacros\\" + area, false))
				return null == key ? null : RegistryValueToObject(key.GetValue(name, null));
		}

		// Converts the string array to the text; returns other objects not changed.
		static object RegistryValueToObject(object value)
		{
			string[] lines = value as string[];
			if (null == lines)
				return value;

			StringBuilder sb = new StringBuilder();
			int i;
			for (i = 0; i < lines.Length - 1; ++i)
				sb.AppendLine(lines[i]);
			sb.Append(lines[i]);
			
			return sb.ToString();
		}

		// Converts the text to the string array or the same string.
		static object StringToRegistryValue(string text)
		{
			string[] lines = Regex.Split(text, Kit.SplitLinePattern);
			if (lines.Length == 1)
				return text;
			else
				return lines;
		}

		static bool ToBool(object value)
		{
			return value != null && ((int)value) != 0;
		}

		static string GetThreeState(object value1, object value2)
		{
			if (null != value1)
				return ((int)value1) != 0 ? "1" : "0";

			if (null != value2)
				return ((int)value2) != 0 ? "0" : "1";

			return string.Empty;
		}

		#endregion

	}
}
