/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;
using System.Collections;

namespace FarNet
{
	/// <summary>
	/// Key macros host. Exposed as <see cref="IFar.Macro"/>.
	/// </summary>
	/// <remarks>
	/// Important: your macro changes are all in the storage,
	/// not in memory where the current macros are loaded by Far.
	/// Thus, when you work on macros you should normally follow this scheme:
	/// 1) call <see cref="Save"/> (it saves memory macros to the storage);
	/// 2) do your work on macros (remember, you operate on data in the storage);
	/// 3) call <see cref="Load"/> after changes (it gets changes from the storage to memory).
	/// </remarks>
	public interface IMacro
	{
		/// <summary>
		/// Save all macros from Far memory into the storage.
		/// </summary>
		/// <remarks>
		/// It is recommended to call this before your work on macros.
		/// Otherwise you cannot get not yet saved changes of macros in memory and they can be even lost on your changes.
		/// </remarks>
		void Save();
		/// <summary>
		/// Gets existing area names or macro names from the specified area.
		/// </summary>
		/// <param name="area">Macro area or empty string.</param>
		string[] GetNames(string area);
		/// <summary>
		/// Gets key macro data.
		/// </summary>
		/// <param name="area">Macro area.</param>
		/// <param name="name">Macro name.</param>
		/// <returns>Macro data or null.</returns>
		MacroData GetData(string area, string name);
		/// <summary>
		/// Removes the specified macro.
		/// </summary>
		/// <param name="area">Macro area.</param>
		/// <param name="name">Macro name.</param>
		void Remove(string area, string name);
		/// <summary>
		/// Loads all macros from the storage into Far memory. Previous values are erased.
		/// </summary>
		/// <remarks>
		/// It is recommended to call this when you have finished macro changes.
		/// Otherwise your changes are not yet active and they can be even lost on future saving macros from memory.
		/// </remarks>
		void Load();
		/// <summary>
		/// Installs one macro.
		/// </summary>
		/// <param name="area">Macro area.</param>
		/// <param name="name">Macro name.</param>
		/// <param name="data">Macro data.</param>
		void Install(string area, string name, MacroData data);
		/// <summary>
		/// Installs several macros in batch mode using a set of macro data dictionaries.
		/// </summary>
		/// <param name="dataSet">Set of dictionaries providing portions of macro data incrementally.</param>
		/// <remarks>
		/// Any dictionary provides a portion of macro data and triggers installation if data are ready.
		/// Data are ready when at least these three values are defined: 'Area', 'Name' and 'Sequence'.
		/// <para>
		/// If a dictionary in the sequence is null then all the current data are reset,
		/// so that the next dictionary starts a new portion of data for next macros.
		/// </para>
		/// <para>
		/// Dictionary keys and values: strings 'Area' and 'Name' define macro area and name,
		/// the others correspond to property names and values of <see cref="MacroData"/>.
		/// You don't have to set default values. Values are active for all next entries
		/// until a null dictionary in the sequence.
		/// </para>
		/// <para>
		/// This method at first may look unusual, but it is suitable for scripting and it:
		/// *) allows to set many macros in one shot effectively;
		/// *) calls Save() and Load() internally, so that one this call is enough for installation;
		/// *) makes it easy to set the same macro in several areas or several similar macros in the same area.
		/// </para>
		/// <para>
		/// An exceptions is thrown if no macro is actually installed ('Area', 'Name' or 'Sequence' is never set)
		/// or the same macro is defined more than once (the same 'Area' and 'Name' are used second time).
		/// Note that macros installed before the exception remain installed.
		/// </para>
		/// </remarks>
		void Install(IDictionary[] dataSet);
	}

	/// <summary>
	/// Key macro data. See <see cref="IMacro"/>.
	/// </summary>
	public class MacroData
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
