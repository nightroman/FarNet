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
	/// WARNING: Methods that change macro data in the storage are not recommended
	/// if there are other running Far instances that also change macro data.
	/// In such cases some changes can be lost.
	/// </remarks>
	public interface IMacro
	{
		/// <summary>
		/// Gets or sets the manual save\load mode.
		/// </summary>
		/// <remarks>
		/// For batch operations on several macros you may set the manual mode,
		/// call <see cref="Save"/>, perform operations, call <see cref="Load"/>
		/// and restore the flag.
		/// </remarks>
		bool ManualSaveLoad { get; set; }
		/// <summary>
		/// Saves all macros from Far memory to the storage.
		/// </summary>
		/// <remarks>
		/// Use it carefully or even don't use if there are more than one running Far instances.
		/// </remarks>
		void Save();
		/// <summary>
		/// Loads all macros from the storage to Far memory.
		/// </summary>
		/// <remarks>
		/// Previous macro values in memory are erased.
		/// </remarks>
		void Load();
		/// <summary>
		/// Gets macro names in the area.
		/// </summary>
		/// <param name="area">Macro area.</param>
		/// <returns>Macro names in the area or, if the area is <c>Root</c>, then names of existing in the storage areas.</returns>
		string[] GetNames(MacroArea area);
		/// <summary>
		/// Gets the key macro.
		/// </summary>
		/// <param name="area">Macro area.</param>
		/// <param name="name">Macro name.</param>
		/// <returns>The macro or null if the name does not exist.</returns>
		Macro GetMacro(MacroArea area, string name);
		/// <summary>
		/// Removes the specified macros or the empty area.
		/// </summary>
		/// <param name="area">Macro area.</param>
		/// <param name="names">Macro names to be removed.</param>
		/// <remarks>
		/// If input names are not provided (null or empty) or there is the only empty name
		/// then this method removes the area if it is not used, i.e. contains no macros.
		/// Areas with macros cannot be removed.
		/// </remarks>
		void Remove(MacroArea area, params string[] names);
		/// <summary>
		/// Installs one or more macros in the storage and loads them.
		/// </summary>
		/// <param name="macros">Macro data.</param>
		/// <remarks>
		/// The method saves current macros, writes data to the storage, and reloads macros.
		/// <para>
		/// This operation is not atomic: if there are several macros to install and it fails
		/// in the middle then some first macros may be installed and the rest of them are not.
		/// </para>
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
		/// Area.
		/// </summary>
		public MacroArea Area { get; set; }
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
		/// Don't send keystrokes to plugins during recording and executing.
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

	/// <summary>
	/// Macro areas.
	/// </summary>
	public enum MacroArea
	{
		/// <summary>
		/// No area.
		/// </summary>
		Root,
		/// <summary>
		/// Lowest priority macros used everywhere.
		/// </summary>
		Common,
		/// <summary>
		/// Dialog boxes.
		/// </summary>
		Dialog,
		/// <summary>
		/// Drive selection menu.
		/// </summary>
		Disks,
		/// <summary>
		/// Internal file editor.
		/// </summary>
		Editor,
		/// <summary>
		/// Folder search panel.
		/// </summary>
		FindFolder,
		/// <summary>
		/// Help system.
		/// </summary>
		Help,
		/// <summary>
		/// Informational panel.
		/// </summary>
		Info,
		/// <summary>
		/// Main menu.
		/// </summary>
		MainMenu,
		/// <summary>
		/// Other menus.
		/// </summary>
		Menu,
		/// <summary>
		/// Screen capturing mode.
		/// </summary>
		Other,
		/// <summary>
		/// Quick view panel.
		/// </summary>
		QView,
		/// <summary>
		/// Quick file search.
		/// </summary>
		Search,
		/// <summary>
		/// File panels.
		/// </summary>
		Shell,
		/// <summary>
		/// Folder tree panel.
		/// </summary>
		Tree,
		/// <summary>
		/// User menu.
		/// </summary>
		UserMenu,
		/// <summary>
		/// Internal file viewer.
		/// </summary>
		Viewer
	}
}
