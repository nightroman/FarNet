/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;

namespace FarNet
{
	/// <summary>
	/// Key macro data. See <see cref="IKeyMacroHost"/>.
	/// </summary>
	public class KeyMacroData
	{
		/// <summary>
		/// Sequence of the keystrokes and macro language elements. Multiline sequence is supported.
		/// </summary>
		public string Sequence { get { return _Sequence; } set { _Sequence = value; } }
		string _Sequence = string.Empty;
		/// <summary>
		/// Macro command description.
		/// </summary>
		public string Description { get { return _Description; } set { _Description = value; } }
		string _Description = string.Empty;
		/// <summary>
		/// Enable screen output while executing the macro.
		/// </summary>
		public bool EnableOutput { get; set; }
		/// <summary>
		/// Don't send keystrokes to the plugins during recording and executing.
		/// </summary>
		public bool DisablePlugins { get; set; }
		/// <summary>
		/// Execute macro command after Far startup. This flag is only for macros in the Shell area.
		/// </summary>
		public bool RunAfterFarStart { get; set; }
		/// <summary>
		/// Execute the macro only if: 1: command line is not empty: 0: command line is empty.
		/// Empty string (default): execute the macro in both cases.
		/// </summary>
		public string CommandLine { get { return _CommandLine; } set { _CommandLine = ToThreeState(value); } }
		string _CommandLine = string.Empty;
		/// <summary>
		/// Execute the macro only if: 1: there is selected text; 0: there is no selected text.
		/// Empty string (default): execute the macro in both cases.
		/// </summary>
		public string SelectedText { get { return _SelectedText; } set { _SelectedText = ToThreeState(value); } }
		string _SelectedText = string.Empty;
		/// <summary>
		/// Execute the macro only if the active panel: 1: has selected items; 0: has no selected items.
		/// Empty string (default): execute the macro in both cases.
		/// </summary>
		public string SelectedItems { get { return _SelectedItems; } set { _SelectedItems = ToThreeState(value); } }
		string _SelectedItems = string.Empty;
		/// <summary>
		/// Execute the macro only if the active panel: 1: is plugin; 0: is not plugin.
		/// Empty string (default): execute the macro in both cases.
		/// </summary>
		public string PanelIsPlugin { get { return _PanelIsPlugin; } set { _PanelIsPlugin = ToThreeState(value); } }
		string _PanelIsPlugin = string.Empty;
		/// <summary>
		/// Execute the macro only if the current active panel item: 1: is directory; 0: is not directory.
		/// Empty string (default): execute the macro in both cases.
		/// </summary>
		public string ItemIsDirectory { get { return _ItemIsDirectory; } set { _ItemIsDirectory = ToThreeState(value); } }
		string _ItemIsDirectory = string.Empty;
		/// <summary>
		/// Execute the macro only if the passive panel: 1: has selected items; 0: has no selected items.
		/// Empty string (default): execute the macro in both cases.
		/// </summary>
		public string SelectedItems2 { get { return _SelectedItems2; } set { _SelectedItems2 = ToThreeState(value); } }
		string _SelectedItems2 = string.Empty;
		/// <summary>
		/// Execute the macro only if the passive panel: 1: is plugin; 0: is not plugin.
		/// Empty string (default): execute the macro in both cases.
		/// </summary>
		public string PanelIsPlugin2 { get { return _PanelIsPlugin2; } set { _PanelIsPlugin2 = ToThreeState(value); } }
		string _PanelIsPlugin2 = string.Empty;
		/// <summary>
		/// Execute the macro only if the current passive panel item: 1: is directory; 0: is not directory.
		/// Empty string (default): execute the macro in both cases.
		/// </summary>
		public string ItemIsDirectory2 { get { return _ItemIsDirectory2; } set { _ItemIsDirectory2 = ToThreeState(value); } }
		string _ItemIsDirectory2 = string.Empty;
		/// <summary>
		/// True if any restriction is set.
		/// </summary>
		public bool IsRestricted()
		{
			return
				DisablePlugins ||
				_CommandLine.Length > 0 ||
				_SelectedText.Length > 0 ||
				_SelectedItems.Length > 0 ||
				_PanelIsPlugin.Length > 0 ||
				_ItemIsDirectory.Length > 0 ||
				_SelectedItems2.Length > 0 ||
				_PanelIsPlugin2.Length > 0 ||
				_ItemIsDirectory2.Length > 0;
		}
		// 0, 1 or empty/null
		static string ToThreeState(string value)
		{
			if (value == null)
				return string.Empty;

			value = value.Trim();
			if (value.Length == 0)
				return string.Empty;

			switch (value)
			{
				case "0":
					return "0";
				case "1":
					return "1";
				default:
					throw new FormatException("Valid values are: '0', '1', empty or null.");
			}
		}
	}
}
