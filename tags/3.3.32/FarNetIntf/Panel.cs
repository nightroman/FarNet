/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
*/

using System.Collections.Generic;
using System.Collections;
using System;

namespace FarManager
{
	/// <summary>
	/// Panel of the FAR Manager. See active <see cref="IFar.Panel"/> and passive <see cref="IFar.Panel2"/>.
	/// </summary>
	public interface IPanel
	{
		/// <summary>
		/// Is it active?
		/// </summary>
		bool IsActive { get; }
		/// <summary>
		/// Is it plugin panel?
		/// </summary>
		bool IsPlugin { get; }
		/// <summary>
		/// Is it visible?
		/// </summary>
		bool IsVisible { get; set; }
		/// <summary>
		/// Current panel path.
		/// If the panel is Tree, it is currently selected directory in panel.
		/// If you set an invalid path you get an error message box, not an exception.
		/// </summary>
		string Path { get; set; }
		/// <summary>
		/// Current item.
		/// See <see cref="IPanelPluginInfo.AddDots"/>.
		/// </summary>
		IFile Current { get; }
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
		/// List of all panel items.
		/// See <see cref="IPanelPluginInfo.AddDots"/>.
		/// </summary>
		IList<IFile> Contents { get; }
		/// <summary>
		/// List of selected panel items.
		/// </summary>
		IList<IFile> Selected { get; }
		/// <summary>
		/// List of selected panel items if any or the current; '..' is excluded.
		/// </summary>
		IList<IFile> Targeted { get; }
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
		/// Redraws the panel. [FCTL_REDRAWPANEL]
		/// </summary>
		void Redraw();
		/// <summary>
		/// Redraws the panel and sets the current and\or the first visible item. [FCTL_REDRAWPANEL]
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
		/// Closes the current plugin panel. [FCTL_CLOSEPLUGIN]
		/// </summary>
		void Close();
		/// <summary>
		/// Closes the current plugin panel. [FCTL_CLOSEPLUGIN]
		/// </summary>
		/// <param name="path">
		/// Name of the directory that will be set in the panel after closing the plugin (or {null|empty}).
		/// If the path doesn't exist FAR shows an error.
		/// </param>
		void Close(string path);
	}

	/// <summary>
	/// Item of file system. It is used by <see cref="IPanel"/>.
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
		/// Alternate name.
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
		/// Alias attribute.
		/// </summary>
		bool IsAlias { get; set; }
		/// <summary>
		/// Compressed attribute.
		/// </summary>
		bool IsCompressed { get; set; }
		/// <summary>
		/// Encrypted attribute.
		/// </summary>
		bool IsEncrypted { get; set; }
		/// <summary>
		/// Selected state.
		/// </summary>
		bool IsSelected { get; set; }
		/// <summary>
		/// Applies standard .NET file attributes
		/// </summary>
		/// <param name="attributes">Attributes.</param>
		void SetAttributes(System.IO.FileAttributes attributes);
		/// <summary>
		/// User data. Only for <see cref="IPanelPlugin"/>.
		/// </summary>
		object Data { get; set; }
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
	/// Describes a plugin instance. [OpenPluginInfo]
	/// </summary>
	public interface IPanelPluginInfo
	{
		/// <summary>
		/// Add ".." item automatically if it is absent.
		/// If you enable this mode ".." is not returned by <see cref="IPanel.Current"/> and <see cref="IPanel.Contents"/>.
		/// Thus, if you need this item you should add it yourself.
		/// [OPIF_ADDDOTS]
		/// </summary>
		bool AddDots { get; set; }
		/// <summary>
		/// Use filter in the plugin panel. [OPIF_USEFILTER]
		/// </summary>
		bool UseFilter { get; set; }
		/// <summary>
		/// Use sort groups in the plugin panel. [OPIF_USESORTGROUPS]
		/// </summary>
		bool UseSortGroups { get; set; }
		/// <summary>
		/// Use file highlighting in the plugin panel. [OPIF_USEHIGHLIGHTING]
		/// </summary>
		bool UseHighlighting { get; set; }
		/// <summary>
		/// Use attributes only for file highlighting. File names will be ignored.
		/// Color is chosen from file color groups, which have templates excluded from analysis
		/// (i.e. option "[ ] Match file mask(s)" in file highlighting setup dialog is off). [OPIF_USEATTRHIGHLIGHTING]
		/// </summary>
		bool UseAttrHighlighting { get; set; }
		/// <summary>
		/// Folders may be selected regardless of FAR settings. [OPIF_RAWSELECTION]
		/// </summary>
		bool RawSelection { get; set; }
		/// <summary>
		/// Turns on the standard FAR file processing mechanism if requested operation is not supported by the plugin.
		/// If this flag is set, the items on the plugin panel should be real file names. [OPIF_REALNAMES]
		/// </summary>
		bool RealNames { get; set; }
		/// <summary>
		/// Show file names without paths by default. [OPIF_SHOWNAMESONLY]
		/// </summary>
		bool ShowNamesOnly { get; set; }
		/// <summary>
		/// Show file names right-aligned by default in all panel display modes. [OPIF_SHOWRIGHTALIGNNAMES]
		/// </summary>
		bool RightAligned { get; set; }
		/// <summary>
		/// Show file names using original case regardless of FAR settings. [OPIF_SHOWPRESERVECASE]
		/// </summary>
		bool PreserveCase { get; set; }
		/// <summary>
		/// Convert timestamps to FAT format for the Compare folders operation.
		/// Set this flag if the plugin file system doesn't provide the time accuracy necessary for standard comparison operations. [OPIF_COMPAREFATTIME]
		/// </summary>
		bool CompareFatTime { get; set; }
		/// <summary>
		/// Used with <see cref="RealNames"/> only. Forces usage of corresponding internal FAR function. [OPIF_EXTERNALGET]
		/// </summary>
		bool ExternalGet { get; set; }
		/// <summary>
		/// Used with <see cref="RealNames"/> only. Forces usage of corresponding internal FAR function. [OPIF_EXTERNALPUT]
		/// </summary>
		bool ExternalPut { get; set; }
		/// <summary>
		/// Used with <see cref="RealNames"/> only. Forces usage of corresponding internal FAR function. [OPIF_EXTERNALDELETE]
		/// </summary>
		bool ExternalDelete { get; set; }
		/// <summary>
		/// Used with <see cref="RealNames"/> only. Forces usage of corresponding internal FAR function. [OPIF_EXTERNALMKDIR]
		/// </summary>
		bool ExternalMakeDirectory { get; set; }
		/// <summary>
		/// File name on emulated file system. If plugin doesn't emulate a file system based on files it is null.
		/// </summary>
		string HostFile { get; set; }
		/// <summary>
		/// Current directory of plugin. If plugin returns empty string here, FAR will close plugin automatically if ENTER is pressed on ".." item.
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
		/// The panel view mode to set on panel creation.
		/// When a panel has started it may be used internally for keeping\restoring the current mode.
		/// </summary>
		PanelViewMode StartViewMode { get; set; }
		/// <summary>
		/// The panel sort mode to set on panel creation.
		/// When a panel has started it may be used internally for keeping\restoring the current mode.
		/// </summary>
		PanelSortMode StartSortMode { get; set; }
		/// <summary>
		/// If <see cref="StartSortMode"/> is specified, this field is used to set sort direction.
		/// When a panel has started it may be used internally for keeping\restoring the current mode.
		/// </summary>
		bool StartSortDesc { get; set; }
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
	/// Base <see cref="IPanelPlugin"/> event arguments.
	/// </summary>
	public class PanelEventArgs : EventArgs
	{
		OperationModes _mode;
		bool _ignore;
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="mode">
		/// Combination of the operation mode flags.
		/// </param>
		public PanelEventArgs(OperationModes mode)
		{
			_mode = mode;
		}
		/// <summary>
		/// Combination of the operation mode flags.
		/// </summary>
		public OperationModes Mode
		{
			get { return _mode; }
		}
		/// <summary>
		/// Set true to tell that action has to be ignored; exact meaning depends on an event.
		/// </summary>
		public bool Ignore
		{
			get { return _ignore; }
			set { _ignore = value; }
		}
	}

	/// <summary>
	/// Arguments of <see cref="IPanelPlugin.Executing"/> event.
	/// Set <see cref="PanelEventArgs.Ignore"/> = true to tell that command has been processed internally.
	/// </summary>
	public class ExecutingEventArgs : PanelEventArgs
	{
		string _command;
		/// <summary>
		/// Constructor.
		/// </summary>
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
	/// Arguments of <see cref="IPanelPlugin.ViewModeChanged"/> event.
	/// [FE_CHANGEVIEWMODE], [Column types].
	/// </summary>
	public class ViewModeChangedEventArgs : EventArgs
	{
		string _columns;
		/// <summary>
		/// Constructor.
		/// </summary>
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
	/// Arguments of <see cref="IPanelPlugin.KeyPressed"/> event.
	/// Set <see cref="PanelEventArgs.Ignore"/> = true to tell that the key has been processed internally.
	/// </summary>
	public class PanelKeyEventArgs : PanelEventArgs
	{
		int _code;
		KeyStates _state;
		bool _preprocess;
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="code">Virtual key code.</param>
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
		/// Virtual key code. See FAR API.
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
		/// Preprocess flag. [PKF_PREPROCESS]
		/// </summary>
		public bool Preprocess
		{
			get { return _preprocess; }
		}
	}

	/// <summary>
	/// Arguments of <see cref="IPanelPlugin.SettingDirectory"/> event.
	/// Set <see cref="PanelEventArgs.Ignore"/> = true if the operation fails.
	/// </summary>
	/// <remarks>
	/// Combination of the operation mode flags.
	/// The plugin should be ready to process <see cref="OperationModes.Find"/> flag.
	/// If it is set, the event is raised from Find file or another directory scanning command,
	/// and the plugin must not perform any actions except changing directory or setting <see cref="PanelEventArgs.Ignore"/> = true
	/// if it is impossible to change the directory. (The plugin should not try to close or update the panels,
	/// ask the user for confirmations, show messages and so on.)
	/// </remarks>
	public class SettingDirectoryEventArgs : PanelEventArgs
	{
		string _name;
		/// <summary>
		/// Constructor.
		/// </summary>
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
		/// by <see cref="IPanelPlugin.Info"/>. It is not necessary when "Search from the current folder"
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
		/// <summary>
		/// Constructor.
		/// </summary>
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
	/// Arguments of <see cref="IPanelPlugin.GettingFiles"/>.
	/// Set <see cref="PanelEventArgs.Ignore"/> = true if the operation fails.
	/// </summary>
	public class GettingFilesEventArgs : FilesEventArgs
	{
		string _destination;
		/// <summary>
		/// Constructor.
		/// </summary>
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
	/// Arguments of <see cref="IPanelPlugin.MakingDirectory"/>.
	/// Set <see cref="PanelEventArgs.Ignore"/> = true if the operation fails.
	/// </summary>
	public class MakingDirectoryEventArgs : PanelEventArgs
	{
		string _name;
		/// <summary>
		/// Constructor.
		/// </summary>
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
	/// Panel plugin. It is created by <see cref="IFar.CreatePanelPlugin"/>.
	/// After that you have to set <see cref="Info"/> properties and add event handlers.
	/// </summary>
	public interface IPanelPlugin : IPanel
	{
		/// <summary>
		/// Opens a panel plugin.
		/// </summary>
		void Open();
		/// <summary>
		/// Opens a panel plugin instead of another opened plugin.
		/// </summary>
		/// <param name="oldPanel">Old panel to be replaced.</param>
		void Open(IPanelPlugin oldPanel);
		/// <summary>
		/// True if the panel is opened.
		/// </summary>
		bool IsOpened { get; }
		/// <summary>
		/// Another FAR.NET panel plugin instance or null.
		/// Note that it may be not "yours", use <see cref="Host"/> property for identification.
		/// </summary>
		IPanelPlugin Another { get; }
		/// <summary>
		/// Any user data not used by FAR.NET.
		/// </summary>
		object Data { get; set; }
		/// <summary>
		/// User object that is normally a host of the panel (i.e. container of data, event handlers and etc.).
		/// Normally you should use it if you have several communicating panels.
		/// See <see cref="Another"/>.
		/// </summary>
		object Host { get; set; }
		/// <summary>
		/// Current directory when the panel starts.
		/// </summary>
		string StartDirectory { get; set; }
		/// <summary>
		/// Use this property to set the information about your panel
		/// just after <see cref="IFar.CreatePanelPlugin"/> (e.g. to set properties that never change)
		/// or add you handler to <see cref="GettingInfo"/> (e.g. to set dynamic properties).
		/// </summary>
		IPanelPluginInfo Info { get; }
		/// <summary>
		/// Panel items. For performance and simplicity the list is not protected and it should be used properly.
		/// Normally it is filled on startup and then can be changed by <see cref="GettingData"/> handler.
		/// If it is changed differently then <see cref="IPanel.Update"/> should be called immediately.
		/// </summary>
		IList<IFile> Files { get; }
		/// <summary>
		/// Raised when a plugin info is being requested.
		/// You have to set or change <see cref="Info"/> properties.
		/// If your panel info is never changed you should not use this event,
		/// set all info properties on a panel creation. [GetOpenPluginInfo]
		/// </summary>
		/// <remarks>
		/// <c>PowerShell</c> handlers should be added via <c>$Psf.WrapEventHandler()</c> (workaround for Find mode).
		/// </remarks>
		event EventHandler GettingInfo;
		/// <summary>
		/// Raised to prepare <see cref="Files"/> list in the current directory of the file system emulated by the plugin.
		/// Note: if your file set is constant you may fill it once on the panel creation and not use this event at all. [GetFindData]
		/// </summary>
		/// <remarks>
		/// <c>PowerShell</c> handlers should be added via <c>$Psf.WrapPanelEvent()</c> (workaround for Find mode).
		/// </remarks>
		event EventHandler<PanelEventArgs> GettingData;
		/// <summary>
		/// Raised when a plugin is closed. [ClosePlugin]
		/// </summary>
		event EventHandler Closed;
		/// <summary>
		/// Raised when a plugin is about to be closed. [FE_CLOSE]
		/// </summary>
		event EventHandler<PanelEventArgs> Closing;
		/// <summary>
		/// Raised every few seconds.
		/// A plugin can use this event to request panel updating and redrawing. [FE_IDLE]
		/// </summary>
		event EventHandler Idled;
		/// <summary>
		/// Raised on executing a command from the FAR command line.
		/// Set <see cref="PanelEventArgs.Ignore"/> = true to tell that command has been processed internally. [FE_COMMAND]
		/// </summary>
		event EventHandler<ExecutingEventArgs> Executing;
		/// <summary>
		/// Raised when Ctrl-Break is pressed.
		/// Processing of this event is performed in separate thread,
		/// so be careful when performing console input or output and don't use FAR service functions. [FE_BREAK]
		/// </summary>
		event EventHandler CtrlBreakPressed;
		/// <summary>
		/// Raised when the panel is about to redraw.
		/// Set <see cref="PanelEventArgs.Ignore"/> = true if the plugin redraws the panel itself. [FE_REDRAW]
		/// </summary>
		event EventHandler<PanelEventArgs> Redrawing;
		/// <summary>
		/// Raised when panel view mode is changed. [FE_CHANGEVIEWMODE]
		/// </summary>
		event EventHandler<ViewModeChangedEventArgs> ViewModeChanged;
		/// <summary>
		/// Raised when a key is pressed.
		/// Set <see cref="PanelEventArgs.Ignore"/> = true if the plugin processes the key itself. [ProcessKey]
		/// </summary>
		event EventHandler<PanelKeyEventArgs> KeyPressed;
		/// <summary>
		/// Raised to set the current directory in the file system emulated by the plugin. [SetDirectory]
		/// </summary>
		event EventHandler<SettingDirectoryEventArgs> SettingDirectory;
		/// <summary>
		/// Raised to delete files in the file system emulated by the plugin. [DeleteFiles]
		/// </summary>
		event EventHandler<FilesEventArgs> DeletingFiles;
		/// <summary>
		/// Raised to get files on copy\move operation. [GetFiles]
		/// </summary>
		event EventHandler<GettingFilesEventArgs> GettingFiles;
		/// <summary>
		/// Raised to put files on copy\move operation. [PutFiles]
		/// </summary>
		event EventHandler<FilesEventArgs> PuttingFiles;
		/// <summary>
		/// Rised to create a new directory in the file system emulated by the plugin. [MakeDirectory]
		/// </summary>
		event EventHandler<MakingDirectoryEventArgs> MakingDirectory;
	}
}
