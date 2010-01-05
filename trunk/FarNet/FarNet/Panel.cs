/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FarNet
{
	/// <summary>
	/// Panel interface (Far or plugin panel).
	/// Exposed as <see cref="IFar.Panel"/> and <see cref="IFar.Panel2"/>.
	/// </summary>
	public interface IPanel
	{
		/// <summary>
		/// Gets true if the panel is active.
		/// </summary>
		bool IsActive { get; }
		/// <summary>
		/// Gets true if the panel is a plugin panel.
		/// </summary>
		bool IsPlugin { get; }
		/// <summary>
		/// Gets or sets visibility of the panel.
		/// </summary>
		/// <remarks>
		/// Setting takes effect only when Far gets control.
		/// </remarks>
		bool IsVisible { get; set; }
		/// <summary>
		/// Gets or sets the panel path.
		/// </summary>
		/// <remarks>
		/// If the panel is a directory tree panel then the path is the currently selected directory in the tree.
		/// <para>
		/// If it is a plugin panel and you set a path the action depends on <see cref="IPluginPanel.SettingDirectory"/> handler.
		/// If the panel does not have this handler and the path exists then the panel is closed and
		/// a file panel is opened at the specified path.
		/// </para>
		/// <para>
		/// On opening a file panel an exception is thrown if a path is not valid or does not exist.
		/// </para>
		/// <para>
		/// You may call <see cref="Redraw()"/> after changing the path so that Far shows changes immediately.
		/// </para>
		/// </remarks>
		/// <seealso cref="GoToName(string)"/>
		/// <seealso cref="GoToPath"/>
		string Path { get; set; }
		/// <summary>
		/// Gets the current file.
		/// </summary>
		FarFile CurrentFile { get; }
		/// <summary>
		/// Gets the current file index in the <see cref="ShownList"/> files.
		/// </summary>
		/// <remarks>
		/// This is the index of the current file in the <see cref="ShownList"/> files.
		/// It is not directly related to other panel file collections.
		/// </remarks>
		int CurrentIndex { get; }
		/// <summary>
		/// Gets the first visible file index.
		/// </summary>
		int TopIndex { get; }
		/// <summary>
		/// Gets or sets the panel view mode.
		/// </summary>
		PanelViewMode ViewMode { get; set; }
		/// <summary>
		/// Gets all shown panel files at once. File ".." is excluded.
		/// </summary>
		/// <remarks>
		/// In contrast to <see cref="ShownList"/> this list is a snapshot of all files,
		/// it can be used even after changes in the panel.
		/// </remarks>
		IList<FarFile> ShownFiles { get; }
		/// <summary>
		/// Gets all selected panel files at once or the current file if none is selected.
		/// </summary>
		/// <remarks>
		/// In contrast to <see cref="SelectedList"/> this list is a snapshot of files,
		/// it can be used even after changes in the panel.
		/// </remarks>
		IList<FarFile> SelectedFiles { get; }
		/// <summary>
		/// Gets all shown panel files including "..".
		/// </summary>
		/// <remarks>
		/// In contrast to <see cref="ShownFiles"/> you must not change panel items while using this list.
		/// The current file index in this list is <see cref="CurrentIndex"/>.
		/// </remarks>
		IList<FarFile> ShownList { get; }
		/// <summary>
		/// Gets all selected panel files or the current file if none is selected.
		/// </summary>
		/// <remarks>
		/// In contrast to <see cref="SelectedFiles"/> you must not change panel items while using this list.
		/// </remarks>
		IList<FarFile> SelectedList { get; }
		/// <summary>
		/// Gets the panel type.
		/// </summary>
		PanelType Type { get; }
		/// <summary>
		/// Gets or sets the panel sort mode.
		/// </summary>
		PanelSortMode SortMode { get; set; }
		/// <summary>
		/// Gets visibility of hidden and system files.
		/// </summary>
		bool ShowHidden { get; }
		/// <summary>
		/// Gets file highlighting flag.
		/// </summary>
		bool Highlight { get; }
		/// <summary>
		/// Gets or sets reverse sort order flag.
		/// </summary>
		bool ReverseSortOrder { get; set; }
		/// <summary>
		/// Gets sort groups flags.
		/// </summary>
		bool UseSortGroups { get; }
		/// <summary>
		/// Gets show selected files first flag.
		/// </summary>
		bool SelectedFirst { get; }
		/// <summary>
		/// Gets real file system names flag.
		/// </summary>
		/// <remarks>
		/// If this flag is set then panel item names are related to real file system names.
		/// </remarks>
		bool RealNames { get; }
		/// <summary>
		/// Gets or sets numeric sort flag.
		/// </summary>
		bool NumericSort { get; set; }
		/// <summary>
		/// Redraws the panel. Normally you should call it after changes to make them visible.
		/// </summary>
		void Redraw();
		/// <summary>
		/// Redraws the panel and sets the current and\or the first visible item.
		/// If both arguments are negative, result is the same as per <see cref="Redraw()"/>
		/// </summary>
		/// <param name="current">Index of the current panel item.</param>
		/// <param name="top">Index of the first visible panel item.</param>
		void Redraw(int current, int top);
		/// <summary>
		/// Updates panel contents.
		/// </summary>
		/// <param name="keepSelection">Keep the current selection.</param>
		void Update(bool keepSelection);
		/// <summary>
		/// Gets the current frame: current and top file indexes.
		/// </summary>
		Point Frame { get; }
		/// <summary>
		/// Gets the panel window position.
		/// </summary>
		Place Window { get; }
		/// <summary>
		/// Closes the plugin panel and opens the original file panel.
		/// </summary>
		/// <remarks>
		/// Mantis 1114: the current original panel item depends on the current plugin panel item on closing.
		/// It is resolved for FarNet panels, the original current and even selected items should be restored.
		/// </remarks>
		void Close(); // _090321_210416
		/// <summary>
		/// Closes the plugin panel and opens a file panel with the specified path.
		/// </summary>
		/// <param name="path">
		/// Name of the directory that will be set in the panel after closing the plugin (or {null|empty}).
		/// If the path doesn't exist Far shows an error.
		/// </param>
		void Close(string path);
		/// <summary>
		/// Gets true if the panel is the left panel.
		/// </summary>
		bool IsLeft { get; }
		/// <summary>
		/// Sets the specified item current by name, if it exists.
		/// </summary>
		/// <param name="name">Name or alternative name of an item to be set current.</param>
		/// <remarks>
		/// If the item does not exist or it is hidden and the panel does not show hidden items
		/// then just nothing happens, it is not an error.
		/// </remarks>
		/// <seealso cref="Path"/>
		/// <seealso cref="GoToPath"/>
		/// <seealso cref="GoToName(string, bool)"/>
		void GoToName(string name);
		/// <summary>
		/// Sets the specified item current by name and optionally fails if it is not shown.
		/// </summary>
		/// <returns>True if a file is found.</returns>
		/// <param name="name">Name or alternative name of a file to be set current.</param>
		/// <param name="fail">Fail mode: to throw if a file is not shown.</param>
		/// <exception cref="FileNotFoundException">Fail mode: a file is not shown.</exception>
		/// <seealso cref="Path"/>
		/// <seealso cref="GoToName(string)"/>
		/// <seealso cref="GoToPath"/>
		bool GoToName(string name, bool fail);
		/// <summary>
		/// Opens the parent directory of a specified item and sets the item current in the panel.
		/// </summary>
		/// <param name="path">Path of an item to be set current.</param>
		/// <remarks>
		/// If the path is not valid or the parent directory does not exist an exception is thrown.
		/// Otherwise the directory of the item is opened on the panel. If the item does not exist
		/// or it is hidden and the panel does not show hidden items it is fine, in this case the
		/// top panel item is set current after the call. Otherwise the requested item is set as
		/// the current.
		/// </remarks>
		/// <seealso cref="Path"/>
		/// <seealso cref="GoToName(string)"/>
		void GoToPath(string path);
		/// <summary>
		/// Selects shown items by their indexes. See <see cref="Redraw()"/>.
		/// </summary>
		/// <param name="indexes">Indexes of items to be selected. Null is OK.</param>
		void SelectAt(int[] indexes);
		/// <summary>
		/// Selects all shown items. See <see cref="Redraw()"/>.
		/// </summary>
		void SelectAll();
		/// <summary>
		/// Unselects shown items by their indexes. See <see cref="Redraw()"/>.
		/// </summary>
		/// <param name="indexes">Indexes of items to be unselected. Null os OK.</param>
		void UnselectAt(int[] indexes);
		/// <summary>
		/// Unselects all shown items. See <see cref="Redraw()"/>.
		/// </summary>
		void UnselectAll();
		/// <summary>
		/// Select panel items with specified names.
		/// </summary>
		/// <param name="names">Names to be selected. Null is OK.</param>
		/// <remarks>
		/// Input and panel names are processed as case sensitive, not found input names are ignored.
		/// </remarks>
		void SelectNames(string[] names);
		/// <summary>
		/// Pushes or puts the panel to the internal panel shelve.
		/// </summary>
		void Push();
	}

	/// <summary>
	/// Type of a panel.
	/// </summary>
	public enum PanelType
	{
		/// <summary>
		/// File list.
		/// </summary>
		File,
		/// <summary>
		/// File tree.
		/// </summary>
		Tree,
		/// <summary>
		/// Quick view.
		/// </summary>
		QView,
		/// <summary>
		/// Information.
		/// </summary>
		Info
	}

	/// <summary>
	/// Panel view mode.
	/// </summary>
	public enum PanelViewMode
	{
		/// <summary>
		/// Alternative full (Ctrl-0).
		/// </summary>
		AlternativeFull,
		/// <summary>
		/// Brief (Ctrl-1).
		/// </summary>
		Brief,
		/// <summary>
		/// Medium (Ctrl-2).
		/// </summary>
		Medium,
		/// <summary>
		/// Full (Ctrl-3).
		/// </summary>
		Full,
		/// <summary>
		/// Wide (Ctrl-4).
		/// </summary>
		Wide,
		/// <summary>
		/// Detailed (Ctrl-5).
		/// </summary>
		Detailed,
		/// <summary>
		/// Descriptions (Ctrl-6).
		/// </summary>
		Descriptions,
		/// <summary>
		/// LongDescriptions (Ctrl-7).
		/// </summary>
		LongDescriptions,
		/// <summary>
		/// FileOwners (Ctrl-8).
		/// </summary>
		FileOwners,
		/// <summary>
		/// FileLinks (Ctrl-9).
		/// </summary>
		FileLinks,
		/// <summary>
		/// Undefined.
		/// </summary>
		Undefined = -0x30
	}

	/// <summary>
	/// Panel sort mode.
	/// </summary>
	public enum PanelSortMode
	{
		/// <summary>
		/// Default mode.
		/// </summary>
		Default,
		/// <summary>
		/// Unsorted mode.
		/// </summary>
		Unsorted,
		/// <summary>
		/// Sorted by name.
		/// </summary>
		Name,
		/// <summary>
		/// Sorted by extension.
		/// </summary>
		Extension,
		/// <summary>
		/// Sorted by modification time.
		/// </summary>
		LastWriteTime,
		/// <summary>
		/// Sorted by creation time.
		/// </summary>
		CreationTime,
		/// <summary>
		/// Sorted by access time.
		/// </summary>
		LastAccessTime,
		/// <summary>
		/// Sorted by length.
		/// </summary>
		Length,
		/// <summary>
		/// Sorted by description.
		/// </summary>
		Description,
		/// <summary>
		/// Sorted by owner.
		/// </summary>
		Owner,
		/// <summary>
		/// Sorted by compressed size.
		/// </summary>
		CompressedSize,
		/// <summary>
		/// Sorted by hard link number.
		/// </summary>
		LinksNumber,
	}

	//! DictionaryEntry is not good for this, because it is a value type.
	//! DataItem is a reference type with some advantages.
	/// <summary>
	/// Named data item, e.g. an info panel item (<see cref="IPluginPanelInfo.InfoItems"/>).
	/// </summary>
	public class DataItem
	{
		/// <summary>
		/// New completely defined data item.
		/// </summary>
		/// <param name="name">Name (or separator text in some cases).</param>
		/// <param name="data">Data (or null for separator in some cases).</param>
		public DataItem(string name, object data)
		{
			Name = name;
			Data = data;
		}
		/// <summary>
		/// Name (or separator text in some cases).
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// Data (or null for separator in some cases).
		/// </summary>
		public object Data { get; set; }
	}

	/// <summary>
	/// Property set describing a plugin panel.
	/// </summary>
	/// <remarks>
	/// This information is requested by Far for a panel very often.
	/// For better performance FarNet caches internal native representation of these data.
	/// After opening a panel it is recommended to avoid frequent modifications of these data.
	/// </remarks>
	public interface IPluginPanelInfo
	{
		/// <summary>
		/// Panel view mode to be set on panel creation.
		/// When a panel is started it is used internally for keeping and restoring the current mode.
		/// </summary>
		PanelViewMode StartViewMode { get; set; }
		/// <summary>
		/// Panel sort mode to be set on panel creation.
		/// When a panel is started it is used internally for keeping and restoring the current mode.
		/// </summary>
		PanelSortMode StartSortMode { get; set; }
		/// <summary>
		/// Tells to generate and use alternate names internally.
		/// </summary>
		/// <remarks>
		/// Alternate names are used by Far for example for Quick View (CtrlQ) temp file names.
		/// This is important because plugin files may have any names, including prohibited by
		/// the file system; in this case alternate names help.
		/// <para>
		/// If you set this flag then alternate names will be generated and used internally
		/// and <see cref="FarFile.AlternateName"/> of your panel files will not be used at all
		/// (derived from <see cref="FarFile"/> classes do not have to implement this property setter).
		/// But this convenience is not completely free: on <see cref="IPluginPanel.GettingFiles"/> event
		/// you have to use alternate names from additional list <see cref="GettingFilesEventArgs.Names"/>,
		/// not from the files.
		/// </para>
		/// </remarks>
		bool AutoAlternateNames { get; set; }
		/// <summary>
		/// If <see cref="StartSortMode"/> is specified, this flag tells to set sort direction.
		/// When a panel is started it is used internally for keeping and restoring the current mode.
		/// </summary>
		bool StartSortDesc { get; set; }
		/// <summary>
		/// Tells to use filter in the plugin panel.
		/// </summary>
		bool UseFilter { get; set; }
		/// <summary>
		/// Tells to use sort groups in the plugin panel.
		/// </summary>
		bool UseSortGroups { get; set; }
		/// <summary>
		/// Tells to use file highlighting in the plugin panel.
		/// </summary>
		bool UseHighlighting { get; set; }
		/// <summary>
		/// Tells to use attributes only for file highlighting.
		/// </summary>
		/// <remarks>
		/// File names are ignored.
		/// Color is chosen from file color groups, which have templates excluded from analysis
		/// (i.e. option "[ ] Match file mask(s)" in file highlighting setup dialog is off).
		/// </remarks>
		bool UseAttrHighlighting { get; set; }
		/// <summary>
		/// Tells that folders may be selected regardless of Far settings.
		/// </summary>
		bool RawSelection { get; set; }
		/// <summary>
		/// Tells that items represent real file system.
		/// </summary>
		/// <remarks>
		/// Turns on the standard Far file processing mechanism if requested operation is not supported by the plugin.
		/// If this flag is set, the items on the plugin panel should be real file names.
		/// </remarks>
		bool RealNames { get; set; }
		/// <summary>
		/// Tells to show file names without paths by default.
		/// </summary>
		bool ShowNamesOnly { get; set; }
		/// <summary>
		/// Tells to show file names right-aligned by default in all panel display modes.
		/// </summary>
		bool RightAligned { get; set; }
		/// <summary>
		/// Tells to show file names using original case regardless of Far settings.
		/// </summary>
		bool PreserveCase { get; set; }
		/// <summary>
		/// Tells to convert timestamps to FAT format for the Compare folders operation.
		/// </summary>
		/// <remarks>
		/// Set this flag if the plugin file system doesn't provide time accuracy necessary for standard comparison operations.
		/// </remarks>
		bool CompareFatTime { get; set; }
		/// <summary>
		/// Used with <see cref="RealNames"/> only. Forces usage of corresponding internal Far function.
		/// </summary>
		bool ExternalGet { get; set; }
		/// <summary>
		/// Used with <see cref="RealNames"/> only. Forces usage of corresponding internal Far function.
		/// </summary>
		bool ExternalPut { get; set; }
		/// <summary>
		/// Used with <see cref="RealNames"/> only. Forces usage of corresponding internal Far function.
		/// </summary>
		bool ExternalDelete { get; set; }
		/// <summary>
		/// Used with <see cref="RealNames"/> only. Forces usage of corresponding internal Far function.
		/// </summary>
		bool ExternalMakeDirectory { get; set; }
		/// <summary>
		/// Gets or sets the base file of emulated file system.
		/// </summary>
		/// <remarks>
		/// If plugin doesn't emulate a file system based on files it should be empty.
		/// </remarks>
		string HostFile { get; set; }
		/// <summary>
		/// Gets or sets the plugin panel current directory.
		/// </summary>
		/// <remarks>
		/// If it is empty, Far closes the plugin if [Enter] is pressed on ".." item.
		/// </remarks>
		string CurrentDirectory { get; set; }
		/// <summary>
		/// Gets or sets the format name (shown in the file copy dialog).
		/// </summary>
		string FormatName { get; set; }
		/// <summary>
		/// Gets or sets the plugin panel header.
		/// </summary>
		string Title { get; set; }
		/// <summary>
		/// Gets or sets info panel item array.
		/// </summary>
		/// <remarks>
		/// If you get it and change items then set it again after changes.
		/// Without that individual item changes will have no effect.
		/// </remarks>
		DataItem[] InfoItems { get; set; }
		/// <summary>
		/// Sets 1-12 key bar labels, use empty labels for Far defaults.
		/// </summary>
		void SetKeyBarMain(string[] labels);
		/// <summary>
		/// Sets 1-12 key bar labels, use empty labels for Far defaults.
		/// </summary>
		void SetKeyBarCtrl(string[] labels);
		/// <summary>
		/// Sets 1-12 key bar labels, use empty labels for Far defaults.
		/// </summary>
		void SetKeyBarAlt(string[] labels);
		/// <summary>
		/// Sets 1-12 key bar labels, use empty labels for Far defaults.
		/// </summary>
		void SetKeyBarShift(string[] labels);
		/// <summary>
		/// Sets 1-12 key bar labels, use empty labels for Far defaults.
		/// </summary>
		void SetKeyBarCtrlShift(string[] labels);
		/// <summary>
		/// Sets 1-12 key bar labels, use empty labels for Far defaults.
		/// </summary>
		void SetKeyBarAltShift(string[] labels);
		/// <summary>
		/// Sets 1-12 key bar labels, use empty labels for Far defaults.
		/// </summary>
		void SetKeyBarCtrlAlt(string[] labels);
		/// <summary>
		/// Gets panel mode information or null if it is not set.
		/// </summary>
		/// <param name="viewMode">View mode to get information for.</param>
		/// <returns>
		/// Mode information. If you change it for opened panel then call <see cref="SetMode"/>.
		/// </returns>
		PanelModeInfo GetMode(PanelViewMode viewMode);
		/// <summary>
		/// Sets panel mode information.
		/// </summary>
		/// <param name="viewMode">View mode to set information for.</param>
		/// <param name="modeInfo">Mode information.</param>
		void SetMode(PanelViewMode viewMode, PanelModeInfo modeInfo);
	}

	/// <summary>
	/// Describes one panel view mode.
	/// </summary>
	/// <remarks>
	/// Normally it is used for <see cref="IPluginPanelInfo.SetMode"/>.
	/// When a panel is opened you can change modes dynamically, but do not forget
	/// to reset the list itself, changes in items are not reflected without this.
	/// <para>
	/// Properties <c>IsAlignedExtensions</c> and <c>IsCaseConversion</c>
	/// can be implemented in the future on demand.
	/// </para>
	/// <para>
	/// WARNING: titles, types and custom columns is a sort of low level stuff;
	/// if you use this incorrectly the Far may crash. For performance sake
	/// FarNet does only basic sanity checks.
	/// </para>
	/// </remarks>
	/// <seealso cref="FarFile.Columns"/>
	/// <seealso cref="SetFile.Columns"/>
	public sealed class PanelModeInfo : ICloneable
	{
		/// <summary>
		/// Columns info.
		/// </summary>
		/// <remarks>
		/// <para>
		/// All supported types: "N", "Z", "O", "S", "DC", "DM", "DA", "C0", "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9".
		/// Supported Far column type suffixes may be added to the end, e.g. NR, ST, DCB, and etc., see Far API [Column types].
		/// </para>
		/// <para>
		/// Default column type sequence: "N", "Z", "O", "C0", "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9".
		/// It is exposed as <see cref="FarColumn.DefaultColumnTypes"/>.
		/// </para>
		/// <para>
		/// Type rules:
		/// <ul>
		/// <li>Specify column types only when you really have to do so, especially try to avoid C0..C9, let them to be processed by default.</li>
		/// <li>C0...C9 must be listed incrementally without gaps; but other types between them is OK. E.g. C0, C2 is bad; C0, N, C1 is OK.</li>
		/// <li>If a type is not specified then the next available from the remaining default sequence is taken.</li>
		/// <li>Column types should not be specified more than once.</li>
		/// </ul>
		/// </para>
		/// </remarks>
		public FarColumn[] Columns { get; set; }
		/// <summary>
		/// Status columns info.
		/// </summary>
		/// <remarks>
		/// Use it for status columns in the same way as <see cref="Columns"/> is used.
		/// Column names are ignored.
		/// </remarks>
		public FarColumn[] StatusColumns { get; set; }
		/// <summary>
		/// Tells to resize panel to fill the entire window (instead of a half).
		/// </summary>
		public bool IsFullScreen { get; set; }
		/// <summary>
		/// Tells to display full status info for a file.
		/// </summary>
		/// <remarks>
		/// Tells to display full status info for a file if <c>Status*</c> are not defined.
		/// Otherwise, the status line displays the file name.
		/// </remarks>
		public bool IsDetailedStatus { get; set; }
		/// <summary>
		/// Creates a new mode as a shallow copy of this.
		/// </summary>
		/// <remarks>
		/// Use it to create another mode with the same properties and then change a few of them.
		/// </remarks>
		public object Clone()
		{
			return MemberwiseClone();
		}
	}

	/// <summary>
	/// Additional information about the operation a plugin if called for.
	/// </summary>
	[Flags]
	public enum OperationModes
	{
		/// <summary>
		/// Nothing.
		/// </summary>
		None = 0,
		/// <summary>
		/// Plugin should minimize user requests if possible, because the called function is only a part of a more complex file operation.
		/// </summary>
		Silent = 0x0001,
		/// <summary>
		/// Plugin function is called from Find file or another directory scanning command. Screen output has to be minimized.
		/// </summary>
		Find = 0x0002,
		/// <summary>
		/// Plugin function is called as part of a file view operation.
		/// If file is viewed on quickview panel, than both <c>View</c> and <c>QuickView</c> are set.
		/// </summary>
		View = 0x0004,
		/// <summary>
		/// Plugin function is called as part of a file edit operation.
		/// </summary>
		Edit = 0x0008,
		/// <summary>
		/// All files in host file of file based plugin should be processed.
		/// This flag is set when executing Shift-F2 and Shift-F3 Far commands outside of host file.
		/// Passed to plugin functions files list also contains all necessary information,
		/// so plugin can either ignore this flag or use it to speed up processing.
		/// </summary>
		TopLevel = 0x0010,
		/// <summary>
		/// Plugin function is called to get or put file with file descriptions.
		/// </summary>
		Descript = 0x0020,
		/// <summary>
		/// Plugin function is called as part of a file view operation activated from the quick view panel
		/// (activated by pressing Ctrl-Q in the file panels).
		/// </summary>
		QuickView = 0x0040,
		/// <summary>
		/// Helper flag combination.
		/// </summary>
		FindSilent = (Find | Silent),
	}

	/// <summary>
	/// Base <see cref="IPluginPanel"/> event arguments.
	/// </summary>
	public class PanelEventArgs : EventArgs
	{
		OperationModes _Mode;
		///
		public PanelEventArgs()
		{ }
		/// <param name="mode">
		/// Combination of the operation mode flags.
		/// </param>
		public PanelEventArgs(OperationModes mode)
		{
			_Mode = mode;
		}
		/// <summary>
		/// Combination of the operation mode flags.
		/// </summary>
		public OperationModes Mode
		{
			get { return _Mode; }
		}
		/// <summary>
		/// Set true to tell that action has to be ignored; exact meaning depends on an event.
		/// </summary>
		public bool Ignore { get; set; }
	}

	/// <summary>
	/// Arguments of <see cref="IPluginPanel.Executing"/> event.
	/// Set <see cref="PanelEventArgs.Ignore"/> = true to tell that command has been processed internally.
	/// </summary>
	public class ExecutingEventArgs : PanelEventArgs
	{
		string _command;
		/// <param name="command">Command text.</param>
		public ExecutingEventArgs(string command)
			: base(0)
		{
			_command = command;
		}
		/// <summary>
		/// Command about to be processed.
		/// </summary>
		public string Command
		{
			get { return _command; }
		}
	}

	/// <summary>
	/// Arguments of <see cref="IPluginPanel.ViewModeChanged"/> event. [FE_CHANGEVIEWMODE], [Column types].
	/// </summary>
	public class ViewModeChangedEventArgs : EventArgs
	{
		string _columns;
		/// <param name="columns">Column types, e.g. N,S,D,T.</param>
		public ViewModeChangedEventArgs(string columns)
		{
			_columns = columns;
		}
		/// <summary>
		/// Column types, e.g. N,S,D,T.
		/// </summary>
		public string Columns
		{
			get { return _columns; }
		}
	}

	/// <summary>
	/// Arguments of <see cref="IPluginPanel.KeyPressed"/> event.
	/// Set <see cref="PanelEventArgs.Ignore"/> = true to tell that the key has been processed internally.
	/// </summary>
	public class PanelKeyEventArgs : PanelEventArgs
	{
		int _code;
		KeyStates _state;
		bool _preprocess;
		/// <param name="code"><see cref="VKeyCode"/> code.</param>
		/// <param name="state">Indicates key states.</param>
		/// <param name="preprocess">Preprocess flag.</param>
		public PanelKeyEventArgs(int code, KeyStates state, bool preprocess)
			: base(0)
		{
			_code = code;
			_state = state;
			_preprocess = preprocess;
		}
		/// <summary>
		/// <see cref="VKeyCode"/> code.
		/// </summary>
		public int Code
		{
			get { return _code; }
		}
		/// <summary>
		/// Indicates key states.
		/// </summary>
		public KeyStates State
		{
			get { return _state; }
		}
		/// <summary>
		/// Preprocess flag.
		/// </summary>
		public bool Preprocess
		{
			get { return _preprocess; }
		}
	}

	/// <summary>
	/// Arguments of <see cref="IPluginPanel.SettingDirectory"/> event.
	/// Set <see cref="PanelEventArgs.Ignore"/> = true if the operation fails.
	/// </summary>
	/// <remarks>
	/// The plugin should be ready to process <see cref="OperationModes.Find"/> flag.
	/// If it is set, the event is raised from Find file or another directory scanning command,
	/// and the plugin must not perform any actions except changing directory or setting <see cref="PanelEventArgs.Ignore"/> = true
	/// if it is impossible to change the directory. (The plugin should not try to close or update the panels,
	/// ask the user for confirmations, show messages and so on.)
	/// </remarks>
	public class SettingDirectoryEventArgs : PanelEventArgs
	{
		string _name;
		/// <param name="name">Directory name.</param>
		/// <param name="mode">Combination of the operation mode flags.</param>
		public SettingDirectoryEventArgs(string name, OperationModes mode)
			: base(mode)
		{
			_name = name;
		}
		/// <summary>
		/// Directory name.
		/// Usually contains only the name, without full path.
		/// To provide basic functionality the plugin should also process the names '..' and '\'.
		/// For correct restoring of current directory after using "Search from the root folder" mode
		/// in the Find file dialog, the plugin should be able to process full directory name returned
		/// by <see cref="IPluginPanel.Info"/>. It is not necessary when "Search from the current folder"
		/// mode is set in the Find file dialog.
		/// </summary>
		public string Name
		{
			get { return _name; }
		}
	}

	/// <summary>
	/// Arguments of file events (copy, move, delete, etc).
	/// Set <see cref="PanelEventArgs.Ignore"/> = true if the operation fails.
	/// </summary>
	public class FilesEventArgs : PanelEventArgs
	{
		IList<FarFile> _files;
		bool _move;
		/// <param name="files">Files to delete.</param>
		/// <param name="mode">Combination of the operation mode flags.</param>
		/// <param name="move">Files are moved.</param>
		public FilesEventArgs(IList<FarFile> files, OperationModes mode, bool move)
			: base(mode)
		{
			_files = files;
			_move = move;
		}
		/// <summary>
		/// Files to process.
		/// </summary>
		public IList<FarFile> Files
		{
			get { return _files; }
		}
		/// <summary>
		/// Files are being moved on copy operations or deleted in alternative way on delete operations.
		/// </summary>
		public bool Move
		{
			get { return _move; }
		}
	}

	/// <summary>
	/// Arguments of <see cref="IPluginPanel.GettingFiles"/>.
	/// Set <see cref="PanelEventArgs.Ignore"/> = true if the operation fails.
	/// </summary>
	public class GettingFilesEventArgs : FilesEventArgs
	{
		string _destination;
		IList<string> _names;

		/// <param name="files">Files to process.</param>
		/// <param name="names">Alternate names.</param>
		/// <param name="mode">Combination of the operation mode flags.</param>
		/// <param name="move">Files are moved.</param>
		/// <param name="destination">Destination path to put files.</param>
		public GettingFilesEventArgs(IList<FarFile> files, IList<string> names, OperationModes mode, bool move, string destination)
			: base(files, mode, move)
		{
			_destination = destination;
			_names = names;
		}
		/// <summary>
		/// Destination directory path.
		/// </summary>
		public string Destination
		{
			get { return _destination; }
		}
		/// <summary>
		/// Alternate destination names (if <see cref="IPluginPanelInfo.AutoAlternateNames"/> is set) or null.
		/// </summary>
		public IList<string> Names
		{
			get { return _names; }
		}
	}

	/// <summary>
	/// Arguments of <see cref="IPluginPanel.PuttingFiles"/>.
	/// Set <see cref="PanelEventArgs.Ignore"/> = true if the operation fails.
	/// </summary>
	public class PuttingFilesEventArgs : FilesEventArgs
	{
		string _source;

		/// <param name="files">Files to process.</param>
		/// <param name="mode">Combination of the operation mode flags.</param>
		/// <param name="move">Files are moved.</param>
		/// <param name="source">Source path to get files from.</param>
		public PuttingFilesEventArgs(IList<FarFile> files, OperationModes mode, bool move, string source)
			: base(files, mode, move)
		{
			_source = source;
		}
		/// <summary>
		/// Source directory path.
		/// </summary>
		public string Source
		{
			get { return _source; }
		}
	}

	/// <summary>
	/// Arguments of <see cref="IPluginPanel.MakingDirectory"/>.
	/// Set <see cref="PanelEventArgs.Ignore"/> = true if the operation fails.
	/// </summary>
	public class MakingDirectoryEventArgs : PanelEventArgs
	{
		string _name;
		/// <param name="name">New directory name.</param>
		/// <param name="mode">Combination of the operation mode flags.</param>
		public MakingDirectoryEventArgs(string name, OperationModes mode)
			: base(mode)
		{
			_name = name;
		}
		/// <summary>
		/// New directory name.
		/// </summary>
		public string Name
		{
			get { return _name; }
		}
	}

	/// <summary>
	/// Gets data of an input object.
	/// </summary>
	/// <param name="value">Input object.</param>
	/// <returns>Result data.</returns>
	/// <remarks>
	/// To get this delegate in <b>PowerShellFar</b> scripts use the helper class <c>PowerShellFar.Meta</c>,
	/// for example: <c>$panel.DataId = New-Object PowerShellFar.Meta 'Id'</c>
	/// or even shorter: <c>$panel.DataId = [PowerShellFar.Meta]'Id'</c>.
	/// </remarks>
	public delegate object Getter(object value);

	/// <summary>
	/// Plugin panel. It is created by <see cref="IFar.CreatePluginPanel()"/>.
	/// Then you set <see cref="Info"/>, add event handlers and open it.
	/// </summary>
	public interface IPluginPanel : IPanel
	{
		/// <summary>
		/// Tells to open the panel when the plugin call is completed.
		/// Only one panel can be opened during this plugin call, otherwise it throws.
		/// </summary>
		/// <remarks>
		/// Normally a panel should be opened when a plugin is called from panels window
		/// (command line, disk menu or plugins menu in panels). If panels window cannot
		/// be set current, this method fails.
		/// <para>
		/// Other possible reasons of failure: *) another panel has been already registered for opening;
		/// *) the plugin is not called for opening, e.g. it is called to process events, not opening.
		/// </para>
		/// <para>
		/// It is recommended to call this as soon as possible and only then configure the panel and other data.
		/// Technically this method only tries to post the panel for opening and may fail due to the reasons above.
		/// </para>
		/// </remarks>
		void Open();
		/// <summary>
		/// Opens a panel by replacing another opened FarNet panel.
		/// </summary>
		/// <param name="oldPanel">Old panel to be replaced.</param>
		void Open(IPluginPanel oldPanel);
		/// <summary>
		/// True if the panel is opened.
		/// </summary>
		bool IsOpened { get; }
		/// <summary>
		/// True if the panel is pushed.
		/// </summary>
		bool IsPushed { get; }
		/// <summary>
		/// Another FarNet plugin panel instance or null.
		/// Note that it may be not "yours", use <see cref="Host"/> property for identification.
		/// </summary>
		IPluginPanel AnotherPanel { get; }
		/// <summary>
		/// Gets or sets a delegate providing IDs of panel file data.
		/// </summary>
		/// <remarks>
		/// When a panel opens a child panel for the current item it is normally expected that on return
		/// the current item will be the same. Methods <see cref="PostData"/>, <see cref="PostFile"/>
		/// and <see cref="PostName"/> are designed to post an item to be restored as current.
		/// But in some cases with not trivial equality this delegate is needed in addition.
		/// <para>
		/// Example: a panel shows some frequently changed data like current system processes.
		/// On update it simply recreates the list. In this case it cannot just use <c>PostFile</c>,
		/// because they are changed. It cannot just use <c>PostData</c> because process objects may be
		/// not equal even if they represent the same process. Finally, it cannot just use <c>PostName</c>
		/// because there may be more than one process with the same name. Solution: a delegate that
		/// returns process IDs.
		/// </para>
		/// <para>
		/// How to use <b>PowerShellFar</b>: see <see cref="Getter"/> remarks.
		/// </para>
		/// </remarks>
		Getter DataId { get; set; }
		/// <summary>
		/// Tells to add an item ".." automatically.
		/// See also <see cref="DotsDescription"/>.
		/// </summary>
		bool AddDots { get; set; }
		/// <summary>
		/// If <see cref="AddDots"/> is true it is used as ".." item description.
		/// </summary>
		string DotsDescription { get; set; }
		/// <summary>
		/// Any user data not used by FarNet.
		/// </summary>
		object Data { get; set; }
		/// <summary>
		/// User object that is normally a host of the panel (i.e. container of data, event handlers, ...).
		/// It can be used for example by communicating panels.
		/// </summary>
		/// <seealso cref="AnotherPanel"/>
		/// <seealso cref="TypeId"/>
		object Host { get; set; }
		/// <summary>
		/// Gets <see cref="IFar.ActivePath"/> saved when the panel starts.
		/// </summary>
		string ActivePath { get; }
		/// <summary>
		/// Use this to set the panel properties.
		/// For better performance set its properties only when they are really changed.
		/// Redraw the opened panel if you change properties not from events calling redraw.
		/// </summary>
		IPluginPanelInfo Info { get; }
		/// <summary>
		/// Panel items. For performance and simplicity the list is not protected and it should be used carefully.
		/// Normally it is filled on startup and then can be changed by <see cref="GettingData"/> handler.
		/// If it is changed differently then <see cref="IPanel.Update"/> should be called immediately;
		/// otherwise not coherent panel and list data may cause unpredictable problems.
		/// </summary>
		IList<FarFile> Files { get; set; }
		/// <summary>
		/// User panel type ID.
		/// </summary>
		/// <remarks>
		/// This property is optionally set once, normally by a creator.
		/// It is used for distinguishing panel types when <see cref="Host"/> is not enough.
		/// </remarks>
		/// <seealso cref="IFar.GetPluginPanel(Guid)"/>
		Guid TypeId { get; set; }
		/// <summary>
		/// Tells to update and redraw the panel automatically when idle.
		/// </summary>
		/// <remarks>
		/// If it is set the panel is updated automatically every few seconds when idle.
		/// This is suitable only for panels with very frequently changed data,
		/// otherwise it may cause overhead job for nothing.
		/// </remarks>
		/// <seealso cref="Idled"/>
		bool IdleUpdate { get; set; }
		/// <summary>
		/// Called to request <see cref="Info"/> data. Use it only when it is absolutely needed.
		/// </summary>
		/// <remarks>
		/// Normally you should not use this event. It is not recommended for many reasons:
		/// *) it is called very frequently;
		/// *) it is expensive due to internal technical details;
		/// *) it may have problems if panel data are accessed, even for reading.
		/// <para>
		/// Try to update <see cref="Info"/> only from other panel handlers. Even if this panel
		/// info depends on external data that are changed outside, consider to update the info
		/// in <see cref="Redrawing"/> event handler.
		/// </para>
		/// </remarks>
		event EventHandler GettingInfo;
		/// <summary>
		/// Called to prepare <see cref="Files"/> list in the current directory of the file system emulated by the plugin.
		/// </summary>
		/// <remarks>
		/// If the file set is constant and may be filled once on the panel creation then this event is not needed.
		/// </remarks>
		event EventHandler<PanelEventArgs> GettingData;
		/// <summary>
		/// Raised when a panel has been closed.
		/// </summary>
		event EventHandler Closed;
		/// <summary>
		/// Raised when a panel is about to be closed.
		/// </summary>
		/// <remarks>
		/// Bug [_090321_165608].
		/// Unfortunately Far triggers this also on plugin commands from command line
		/// even if a new panel is not going to be opened and the current one closed.
		/// </remarks>
		event EventHandler<PanelEventArgs> Closing;
		/// <summary>
		/// Event is triggered periodically when a user is idle.
		/// Plugins can use this event to request panel updating and redrawing.
		/// </summary>
		/// <seealso cref="IdleUpdate"/>
		/// <seealso cref="IdledHandler"/>
		event EventHandler Idled;
		/// <summary>
		/// Raised on executing a command from the Far command line.
		/// Set <see cref="PanelEventArgs.Ignore"/> = true to tell that command has been processed internally.
		/// </summary>
		event EventHandler<ExecutingEventArgs> Executing;
		/// <summary>
		/// Raised when Ctrl-Break is pressed.
		/// Processing of this event is performed in separate thread,
		/// so be careful when performing console input or output and don't use Far service functions.
		/// </summary>
		event EventHandler CtrlBreakPressed;
		/// <summary>
		/// Raised when the panel is about to redraw.
		/// Set <see cref="PanelEventArgs.Ignore"/> = true if the plugin redraws the panel itself.
		/// </summary>
		event EventHandler<PanelEventArgs> Redrawing;
		/// <summary>
		/// Raised when panel view mode is changed.
		/// </summary>
		event EventHandler<ViewModeChangedEventArgs> ViewModeChanged;
		/// <summary>
		/// Raised when a key is pressed.
		/// Set <see cref="PanelEventArgs.Ignore"/> = true if the plugin processes the key itself.
		/// </summary>
		event EventHandler<PanelKeyEventArgs> KeyPressed;
		/// <summary>
		/// Called to set the current directory in the file system emulated by the plugin.
		/// </summary>
		event EventHandler<SettingDirectoryEventArgs> SettingDirectory;
		/// <summary>
		/// Called to delete files in the file system emulated by the plugin.
		/// </summary>
		event EventHandler<FilesEventArgs> DeletingFiles;
		/// <summary>
		/// Called to get files on copy\move operation.
		/// </summary>
		event EventHandler<GettingFilesEventArgs> GettingFiles;
		/// <summary>
		/// Called to put files on copy\move operation.
		/// </summary>
		event EventHandler<PuttingFilesEventArgs> PuttingFiles;
		/// <summary>
		/// Rised to create a new directory in the file system emulated by the plugin.
		/// </summary>
		event EventHandler<MakingDirectoryEventArgs> MakingDirectory;
		/// <summary>
		/// A panel has got focus.
		/// </summary>
		event EventHandler GotFocus;
		/// <summary>
		/// A panel is losing focus.
		/// </summary>
		event EventHandler LosingFocus;
		/// <summary>
		/// Raised when [Escape] is pressed and the command line is empty.
		/// </summary>
		/// <remarks>
		/// The default action for now is standard key processing.
		/// Some advanced default action perhaps will be added in a future.
		/// In any case a handler has to set <see cref="PanelEventArgs.Ignore"/> to stop avoid processing.
		/// </remarks>
		event EventHandler<PanelEventArgs> Escaping;
		/// <summary>
		/// Panel data to be set current.
		/// </summary>
		/// <seealso cref="DataId"/>
		void PostData(object data);
		/// <summary>
		/// Panel file to be set current.
		/// </summary>
		void PostFile(FarFile file);
		/// <summary>
		/// Panel name to be set current.
		/// </summary>
		void PostName(string name);
	}

	/// <summary>
	/// Panel column options (abstract).
	/// </summary>
	/// <remarks>
	/// Column options are used by <see cref="PanelModeInfo.Columns"/> and <see cref="PanelModeInfo.StatusColumns"/>.
	/// <para>
	/// This class is only a base for <see cref="SetColumn"/> (recommended and ready to use by plugins)
	/// and other classes derived by plugins (basically they are not needed).
	/// </para>
	/// </remarks>
	public class FarColumn
	{
		/// <summary>
		/// Column name.
		/// </summary>
		/// <remarks>
		/// Title of a standard panel column. It is ignored for a status column.
		/// </remarks>
		public virtual string Name { get { return null; } set { throw new NotImplementedException(); } }
		/// <summary>
		/// Column type. See <see cref="PanelModeInfo.Columns"/>.
		/// </summary>
		public virtual string Type { get { return null; } set { throw new NotImplementedException(); } }
		/// <summary>
		/// Column width (absolute or percentage).
		/// </summary>
		/// <remarks>
		/// It is a number ("30") or a number with % ("30%").
		/// Null or empty is the same as "0" (Far decides).
		/// See Far API [Column width].
		/// </remarks>
		public virtual string Width { get { return null; } set { throw new NotImplementedException(); } }
		/// <summary>
		/// Default column type sequence: "N", "Z", "O", "C0", "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9".
		/// </summary>
		public static ReadOnlyCollection<string> DefaultColumnTypes { get { return _DefaultColumnTypes; } }
		static readonly ReadOnlyCollection<string> _DefaultColumnTypes = new ReadOnlyCollection<string>(new string[] { "N", "Z", "O", "C0", "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9" });
		/// <summary>
		/// Only for derived classes.
		/// </summary>
		protected FarColumn() { }
	}

	/// <summary>
	/// Panel column options.
	/// </summary>
	/// <remarks>
	/// Use this class directly to create column options instance and set its properties.
	/// See <see cref="FarColumn"/> for details.
	/// </remarks>
	public sealed class SetColumn : FarColumn
	{
		///
		public override string Name { get; set; }
		///
		public override string Type { get; set; }
		///
		public override string Width { get; set; }
	}

}
