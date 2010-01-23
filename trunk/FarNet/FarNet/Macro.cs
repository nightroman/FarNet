/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;

namespace FarNet
{
	/// <summary>
	/// Key macro operator. Exposed as <see cref="IFar.Macro"/>.
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
		/// Gets key macro.
		/// </summary>
		/// <param name="area">Macro area.</param>
		/// <param name="name">Macro name.</param>
		/// <returns>Macro data or null.</returns>
		Macro GetMacro(string area, string name);
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
		/// Installs one or more macros.
		/// </summary>
		/// <param name="macros">Macro data.</param>
		/// <remarks>
		/// This operation is not atomic: if there are several macros to install and it fails
		/// in the middle then some first macros may be installed and the rest of them are not.
		/// <para>
		/// This method fails if macro data are invalid or a macro with the same area and name
		/// has be already installed by the same call of this method.
		/// </para>
		/// </remarks>
		void Install(params Macro[] macros);
	}

	/// <summary>
	/// Key macro data. See <see cref="IMacro"/>.
	/// </summary>
	public class Macro
	{
		/// <summary>
		/// Area name.
		/// </summary>
		public string Area { get; set; }
		/// <summary>
		/// Key name.
		/// </summary>
		public string Name { get; set; }
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
