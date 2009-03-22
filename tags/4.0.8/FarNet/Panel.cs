/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace FarNet
{
	/// <summary>
	/// Panel interface (FAR or plugin panel). Exposed as <see cref="IFar.Panel"/> and <see cref="IFar.Panel2"/>.
	/// </summary>
	public interface IPanel
	{
		/// <summary>
		/// Is the panel active?
		/// </summary>
		bool IsActive { get; }
		/// <summary>
		/// Is the panel a plugin panel?
		/// </summary>
		bool IsPlugin { get; }
		/// <summary>
		/// Is the panel visible?
		/// If you set it it takes effect only when FAR gets control.
		/// </summary>
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
		/// You may call <see cref="Redraw()"/> after changing the path so that FAR shows changes immediately.
		/// </para>
		/// </remarks>
		/// <seealso cref="GoToName"/>
		/// <seealso cref="GoToPath"/>
		string Path { get; set; }
		/// <summary>
		/// Current file.
		/// </summary>
		IFile CurrentFile { get; }
		/// <summary>
		/// Current item index.
		/// </summary>
		int CurrentIndex { get; }
		/// <summary>
		/// The first visible item index.
		/// </summary>
		int TopIndex { get; }
		/// <summary>
		/// View mode.
		/// </summary>
		PanelViewMode ViewMode { get; set; }
		/// <summary>
		/// Shown panel items.
		/// </summary>
		IList<IFile> ShownFiles { get; }
		/// <summary>
		/// List of selected panel items if any or the current; '..' is excluded.
		/// </summary>
		IList<IFile> SelectedFiles { get; }
		/// <summary>
		/// Panel type.
		/// </summary>
		PanelType Type { get; }
		/// <summary>
		/// Panel sort mode.
		/// </summary>
		PanelSortMode SortMode { get; set; }
		/// <summary>
		/// Hidden and system files are displayed.
		/// </summary>
		bool ShowHidden { get; }
		/// <summary>
		/// File highlighting is used.
		/// </summary>
		bool Highlight { get; }
		/// <summary>
		/// Reversed sort order is used.
		/// </summary>
		bool ReverseSortOrder { get; set; }
		/// <summary>
		/// Sort groups are used.
		/// </summary>
		bool UseSortGroups { get; }
		/// <summary>
		/// Show selected files first.
		/// </summary>
		bool SelectedFirst { get; }
		/// <summary>
		/// Plugin panel items are shown with real file names.
		/// </summary>
		bool RealNames { get; }
		/// <summary>
		/// Numeric sort mode.
		/// </summary>
		bool NumericSort { get; set; }
		/// <summary>
		/// Redraws the panel.
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
		/// Gets current frame: current and top index.
		/// </summary>
		Point Frame { get; }
		/// <summary>
		/// Panel window position.
		/// </summary>
		Place Window { get; }
		/// <summary>
		/// Closes the current plugin panel.
		/// </summary>
		/// <remarks>
		/// Bug [_090321_210416]: the current Far panel item depends on the current plugin panel item.
		/// </remarks>
		void Close();
		/// <summary>
		/// Closes the current plugin panel.
		/// </summary>
		/// <param name="path">
		/// Name of the directory that will be set in the panel after closing the plugin (or {null|empty}).
		/// If the path doesn't exist FAR shows an error.
		/// </param>
		void Close(string path);
		/// <summary>
		/// Is this a left panel? FAR 1.71.2348
		/// </summary>
		bool IsLeft { get; }
		/// <summary>
		/// Sets the specified item current by name.
		/// </summary>
		/// <param name="name">Name or alternative name of an item to be set current.</param>
		/// <remarks>
		/// If the item does not exist or it is hidden and the panel does not show hidden items
		/// then just nothing happens, it is not an error.
		/// </remarks>
		/// <seealso cref="Path"/>
		/// <seealso cref="GoToPath"/>
		void GoToName(string name);
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
		/// <seealso cref="GoToName"/>
		void GoToPath(string path);
	}

	/// <summary>
	/// <see cref="IPanel"/> item representing standard file or directory or a plugin item.
	/// </summary>
	public interface IFile
	{
		/// <summary>
		/// File name.
		/// </summary>
		string Name { get; set; }
		/// <summary>
		/// Description.
		/// </summary>
		string Description { get; set; }
		/// <summary>
		/// Owner.
		/// </summary>
		string Owner { get; set; }
		/// <summary>
		/// Alternate name, can be used as a file system name.
		/// </summary>
		string AlternateName { get; set; }
		/// <summary>
		/// Creation time.
		/// </summary>
		DateTime CreationTime { get; set; }
		/// <summary>
		/// Last access time.
		/// </summary>
		DateTime LastAccessTime { get; set; }
		/// <summary>
		/// Last access time.
		/// </summary>
		DateTime LastWriteTime { get; set; }
		/// <summary>
		/// File length.
		/// </summary>
		long Length { get; set; }
		/// <summary>
		/// User data. Only for <see cref="IPluginPanel"/>.
		/// </summary>
		object Data { get; set; }
		/// <summary>
		/// Standard file attributes.
		/// </summary>
		FileAttributes Attributes { get; set; }
		/// <summary>
		/// Read only attribute.
		/// </summary>
		bool IsReadOnly { get; set; }
		/// <summary>
		/// Hidden attribute.
		/// </summary>
		bool IsHidden { get; set; }
		/// <summary>
		/// System attribute.
		/// </summary>
		bool IsSystem { get; set; }
		/// <summary>
		/// Directory attribute.
		/// </summary>
		bool IsDirectory { get; set; }
		/// <summary>
		/// Archive attribute.
		/// </summary>
		bool IsArchive { get; set; }
		/// <summary>
		/// Reparse point attribute.
		/// </summary>
		bool IsReparsePoint { get; set; }
		/// <summary>
		/// Compressed attribute.
		/// </summary>
		bool IsCompressed { get; set; }
		/// <summary>
		/// Encrypted attribute.
		/// </summary>
		bool IsEncrypted { get; set; }
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

	/// <summary>
	/// Named data item.
	/// </summary>
	public class DataItem
	{
		/// <param name="name">Name (or separator text in some cases).</param>
		/// <param name="data">Data (or null for separator in some cases).</param>
		public DataItem(string name, object data)
		{
			_Name = name;
			_Data = data;
		}
		string _Name;
		/// <summary>
		/// Name (or separator text in some cases).
		/// </summary>
		public string Name
		{
			get { return _Name; }
			set { _Name = value; }
		}
		object _Data;
		/// <summary>
		/// Data (or null for separator in some cases).
		/// </summary>
		public object Data
		{
			get { return _Data; }
			set { _Data = value; }
		}
	}

	/// <summary>
	/// Describes a panel.
	/// For better performance set its properties only when they are really changed.
	/// </summary>
	public interface IPluginPanelInfo
	{
		/// <summary>
		/// The panel view mode to set on panel creation.
		/// When a panel is started it is used internally for keeping and restoring the current mode.
		/// </summary>
		PanelViewMode StartViewMode { get; set; }
		/// <summary>
		/// The panel sort mode to set on panel creation.
		/// When a panel is started it is used internally for keeping and restoring the current mode.
		/// </summary>
		PanelSortMode StartSortMode { get; set; }
		/// <summary>
		/// If <see cref="StartSortMode"/> is specified, this field is used to set sort direction.
		/// When a panel is started it is used internally for keeping and restoring the current mode.
		/// </summary>
		bool StartSortDesc { get; set; }
		/// <summary>
		/// Use filter in the plugin panel.
		/// </summary>
		bool UseFilter { get; set; }
		/// <summary>
		/// Use sort groups in the plugin panel.
		/// </summary>
		bool UseSortGroups { get; set; }
		/// <summary>
		/// Use file highlighting in the plugin panel.
		/// </summary>
		bool UseHighlighting { get; set; }
		/// <summary>
		/// Use attributes only for file highlighting. File names will be ignored.
		/// Color is chosen from file color groups, which have templates excluded from analysis
		/// (i.e. option "[ ] Match file mask(s)" in file highlighting setup dialog is off).
		/// </summary>
		bool UseAttrHighlighting { get; set; }
		/// <summary>
		/// Folders may be selected regardless of FAR settings.
		/// </summary>
		bool RawSelection { get; set; }
		/// <summary>
		/// Turns on the standard FAR file processing mechanism if requested operation is not supported by the plugin.
		/// If this flag is set, the items on the plugin panel should be real file names.
		/// </summary>
		bool RealNames { get; set; }
		/// <summary>
		/// Show file names without paths by default.
		/// </summary>
		bool ShowNamesOnly { get; set; }
		/// <summary>
		/// Show file names right-aligned by default in all panel display modes.
		/// </summary>
		bool RightAligned { get; set; }
		/// <summary>
		/// Show file names using original case regardless of FAR settings.
		/// </summary>
		bool PreserveCase { get; set; }
		/// <summary>
		/// Convert timestamps to FAT format for the Compare folders operation.
		/// Set this flag if the plugin file system doesn't provide the time accuracy necessary for standard comparison operations.
		/// </summary>
		bool CompareFatTime { get; set; }
		/// <summary>
		/// Used with <see cref="RealNames"/> only. Forces usage of corresponding internal FAR function.
		/// </summary>
		bool ExternalGet { get; set; }
		/// <summary>
		/// Used with <see cref="RealNames"/> only. Forces usage of corresponding internal FAR function.
		/// </summary>
		bool ExternalPut { get; set; }
		/// <summary>
		/// Used with <see cref="RealNames"/> only. Forces usage of corresponding internal FAR function.
		/// </summary>
		bool ExternalDelete { get; set; }
		/// <summary>
		/// Used with <see cref="RealNames"/> only. Forces usage of corresponding internal FAR function.
		/// </summary>
		bool ExternalMakeDirectory { get; set; }
		/// <summary>
		/// File name on emulated file system. If plugin doesn't emulate a file system based on files it is empty.
		/// </summary>
		string HostFile { get; set; }
		/// <summary>
		/// Plugin current directory. If it is empty, FAR closes the plugin if ENTER is pressed on ".." item.
		/// </summary>
		string CurrentDirectory { get; set; }
		/// <summary>
		/// Plugin's format name. This is shown in the file copy dialog.
		/// </summary>
		string Format { get; set; }
		/// <summary>
		/// Plugin panel header.
		/// </summary>
		string Title { get; set; }
		/// <summary>
		/// Info panel item array. If you get it and change items then set it again after changes.
		/// </summary>
		DataItem[] InfoItems { get; set; }
		/// <summary>
		/// 1-12 key bar labels, use empty labels for FAR defaults.
		/// </summary>
		void SetKeyBarMain(string[] labels);
		/// <summary>
		/// 1-12 key bar labels, use empty labels for FAR defaults.
		/// </summary>
		void SetKeyBarCtrl(string[] labels);
		/// <summary>
		/// 1-12 key bar labels, use empty labels for FAR defaults.
		/// </summary>
		void SetKeyBarAlt(string[] labels);
		/// <summary>
		/// 1-12 key bar labels, use empty labels for FAR defaults.
		/// </summary>
		void SetKeyBarShift(string[] labels);
		/// <summary>
		/// 1-12 key bar labels, use empty labels for FAR defaults.
		/// </summary>
		void SetKeyBarCtrlShift(string[] labels);
		/// <summary>
		/// 1-12 key bar labels, use empty labels for FAR defaults.
		/// </summary>
		void SetKeyBarAltShift(string[] labels);
		/// <summary>
		/// 1-12 key bar labels, use empty labels for FAR defaults.
		/// </summary>
		void SetKeyBarCtrlAlt(string[] labels);
	}

	/// <summary>
	/// Sent to a plugin additional information about function operation mode and place, from which it was called.
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
		/// This flag is set when executing Shift-F2 and Shift-F3 FAR commands outside of host file.
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
		bool _Ignore;
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
		public bool Ignore
		{
			get { return _Ignore; }
			set { _Ignore = value; }
		}
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
		IList<IFile> _files;
		bool _move;
		/// <param name="files">Files to delete.</param>
		/// <param name="mode">Combination of the operation mode flags.</param>
		/// <param name="move">Files are moved.</param>
		public FilesEventArgs(IList<IFile> files, OperationModes mode, bool move)
			: base(mode)
		{
			_files = files;
			_move = move;
		}
		/// <summary>
		/// Files to process.
		/// </summary>
		public IList<IFile> Files
		{
			get { return _files; }
		}
		/// <summary>
		/// Files are moved.
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
		/// <param name="files">Files to delete.</param>
		/// <param name="mode">Combination of the operation mode flags.</param>
		/// <param name="move">Files are moved.</param>
		/// <param name="destination">Destination path to put files.</param>
		public GettingFilesEventArgs(IList<IFile> files, OperationModes mode, bool move, string destination)
			: base(files, mode, move)
		{
			_destination = destination;
		}
		/// <summary>
		/// Destination path to put files.
		/// </summary>
		public string Destination
		{
			get { return _destination; }
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
		/// Pushes the panel to the stack and shows a standard FAR panel.
		/// </summary>
		void Push();
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
		/// Comparison of posted and current data.
		/// </summary>
		/// <remarks>
		/// When a panel opens a child panel for the current item it is normally expected that on return
		/// the current item will be the same. Methods <see cref="PostData"/>, <see cref="PostFile"/>
		/// and <see cref="PostName"/> are designed to post an item to be restored as current.
		/// But in some cases with not trivial equality a comparison is needed in addition.
		/// <para>
		/// Example: a panel shows some frequently changed data like current system processes.
		/// On update it simply recreates the list. In this case it cannot just use <c>PostFile</c>,
		/// because they are changed. It cannot just use <c>PostData</c> because process objects may be
		/// not equal even if they represent the same process. Finally, it cannot just use <c>PostName</c>
		/// because there may be more than one process with the same name. Solution: a comparison that
		/// returns 0 if objects representing processes have the same process Id.
		/// </para>
		/// </remarks>
		Comparison<object> DataComparison { get; set; }
		/// <summary>
		/// Tells to add item ".." automatically.
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
		/// <seealso cref="Id"/>
		object Host { get; set; }
		/// <summary>
		/// Current directory when the panel starts.
		/// </summary>
		string StartDirectory { get; set; }
		/// <summary>
		/// Use this to set the panel properties.
		/// For better performance set its properties only when they are really changed.
		/// Redraw the opened panel if you change properties not from events calling redraw.
		/// </summary>
		IPluginPanelInfo Info { get; }
		/// <summary>
		/// Panel items. For performance and simplicity the list is not protected and it should be used properly.
		/// Normally it is filled on startup and then can be changed by <see cref="GettingData"/> handler.
		/// If it is changed differently then <see cref="IPanel.Update"/> should be called immediately.
		/// </summary>
		IList<IFile> Files { get; }
		/// <summary>
		/// User panel type ID.
		/// </summary>
		/// <remarks>
		/// This property is optionally set once, normally by a creator.
		/// It is used for distinguishing panel types when <see cref="Host"/> is not enough.
		/// </remarks>
		/// <seealso cref="IFar.GetPluginPanel(Guid)"/>
		Guid Id { get; set; }
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
		/// Raised to get a plugin <see cref="Info"/> data. Normally you don't have to use this but
		/// if info is changed externally you may check and update really changed properties.
		/// </summary>
		/// <remarks>
		/// <c>PowerShell</c> handlers should be added via <c>$Psf.WrapEventHandler()</c> (workaround for Find mode).
		/// </remarks>
		event EventHandler GettingInfo;
		/// <summary>
		/// Raised to prepare <see cref="Files"/> list in the current directory of the file system emulated by the plugin.
		/// Note: if your file set is constant you may fill it once on the panel creation and not use this event at all.
		/// </summary>
		/// <remarks>
		/// <c>PowerShell</c> handlers should be added via <c>$Psf.WrapPanelEvent()</c> (workaround for Find mode).
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
		/// Unfortunately FAR triggers this also on plugin commands from command line
		/// even if a new panel is not going to be opened and the current one closed.
		/// </remarks>
		event EventHandler<PanelEventArgs> Closing;
		/// <summary>
		/// Raised every few seconds.
		/// Plugins can use this event to request panel updating and redrawing.
		/// </summary>
		/// <seealso cref="IdleUpdate"/>
		event EventHandler Idled;
		/// <summary>
		/// Raised on executing a command from the FAR command line.
		/// Set <see cref="PanelEventArgs.Ignore"/> = true to tell that command has been processed internally.
		/// </summary>
		event EventHandler<ExecutingEventArgs> Executing;
		/// <summary>
		/// Raised when Ctrl-Break is pressed.
		/// Processing of this event is performed in separate thread,
		/// so be careful when performing console input or output and don't use FAR service functions.
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
		/// Raised to set the current directory in the file system emulated by the plugin.
		/// </summary>
		event EventHandler<SettingDirectoryEventArgs> SettingDirectory;
		/// <summary>
		/// Raised to delete files in the file system emulated by the plugin.
		/// </summary>
		event EventHandler<FilesEventArgs> DeletingFiles;
		/// <summary>
		/// Raised to get files on copy\move operation.
		/// </summary>
		event EventHandler<GettingFilesEventArgs> GettingFiles;
		/// <summary>
		/// Raised to put files on copy\move operation.
		/// </summary>
		event EventHandler<FilesEventArgs> PuttingFiles;
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
		/// <seealso cref="DataComparison"/>
		void PostData(object data);
		/// <summary>
		/// Panel file to be set current.
		/// </summary>
		void PostFile(IFile file);
		/// <summary>
		/// Panel name to be set current.
		/// </summary>
		void PostName(string name);
	}

}
