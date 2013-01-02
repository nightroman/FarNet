
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2013 Roman Kuzmin
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace FarNet
{
	/// <summary>
	/// Any panel: native, plugin, or module.
	/// Exposed as <see cref="IFar.Panel"/> and <see cref="IFar.Panel2"/>.
	/// </summary>
	public interface IPanel //! think twice when convert to abstract class (see Panel2 : Panel1, IPanel)
	{
		#region Settable modes
		/// <summary>
		/// Gets or sets the case sensitive sort flag.
		/// </summary>
		bool CaseSensitiveSort { get; set; }
		/// <summary>
		/// Gets or sets the directories first sort flag.
		/// </summary>
		bool DirectoriesFirst { get; set; }
		/// <summary>
		/// Gets or sets the numeric sort flag.
		/// </summary>
		bool NumericSort { get; set; }
		#endregion
		#region Read only modes
		/// <summary>
		/// Gets true if file highlighting is turned on.
		/// </summary>
		bool Highlight { get; }
		/// <summary>
		/// Gets (all panels) or sets (module panel) the real file system names flag.
		/// </summary>
		/// <remarks>
		/// If this flag is set then panel item names are related to real file system names.
		/// </remarks>
		bool RealNames { get; set; }
		/// <summary>
		/// Gets the show selected files first flag.
		/// </summary>
		bool SelectedFirst { get; }
		/// <summary>
		/// Gets true if hidden and system files are shown.
		/// </summary>
		bool ShowHidden { get; }
		/// <summary>
		/// Gets (all panels) or sets (module panel) the use sort groups flags.
		/// </summary>
		bool UseSortGroups { get; set; }
		#endregion
		#region Properties
		/// <include file='doc.xml' path='doc/CurrentDirectory/*'/>
		string CurrentDirectory { get; set; }
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
		/// Gets the current frame: current and top file indexes.
		/// </summary>
		Point Frame { get; }
		/// <summary>
		/// Gets true if the panel is active.
		/// </summary>
		bool IsActive { get; }
		/// <summary>
		/// Gets true if the panel is the left panel.
		/// </summary>
		bool IsLeft { get; }
		/// <summary>
		/// Gets true if the panel is designed for navigation by paths.
		/// </summary>
		bool IsNavigation { get; }
		/// <summary>
		/// Gets true if it is a plugin panel.
		/// </summary>
		/// <remarks>
		/// Note: module panels are plugin panels because they are created by the plugin.
		/// Plugin panels may or may not be module panels.
		/// </remarks>
		bool IsPlugin { get; }
		/// <summary>
		/// Gets or sets visibility of the panel.
		/// </summary>
		/// <remarks>
		/// Setting takes effect only when Far gets control.
		/// </remarks>
		bool IsVisible { get; set; }
		/// <summary>
		/// Gets the panel kind.
		/// </summary>
		PanelKind Kind { get; }
		/// <summary>
		/// Gets all selected panel files at once or the current file if none is selected.
		/// </summary>
		/// <remarks>
		/// In contrast to <see cref="SelectedList"/> this list is a snapshot of files,
		/// it can be used even after changes in the panel.
		/// </remarks>
		IList<FarFile> SelectedFiles { get; }
		/// <summary>
		/// Gets all selected panel files or the current file if none is selected.
		/// </summary>
		/// <remarks>
		/// In contrast to <see cref="SelectedFiles"/> you must not change panel items while using this list.
		/// </remarks>
		IList<FarFile> SelectedList { get; }
		/// <summary>
		/// Gets all shown panel files at once. File ".." is excluded.
		/// </summary>
		/// <remarks>
		/// In contrast to <see cref="ShownList"/> this list is a snapshot of all files,
		/// it can be used even after changes in the panel.
		/// </remarks>
		IList<FarFile> ShownFiles { get; }
		/// <summary>
		/// Gets all shown panel files including "..".
		/// </summary>
		/// <remarks>
		/// In contrast to <see cref="ShownFiles"/> you must not change panel items while using this list.
		/// The current file index in this list is <see cref="CurrentIndex"/>.
		/// </remarks>
		IList<FarFile> ShownList { get; }
		/// <summary>
		/// Gets the first visible file index.
		/// </summary>
		int TopIndex { get; }
		/// <summary>
		/// Gets or sets the panel sort mode.
		/// </summary>
		PanelSortMode SortMode { get; set; }
		/// <summary>
		/// Gets or sets the panel view mode.
		/// </summary>
		PanelViewMode ViewMode { get; set; }
		/// <include file='doc.xml' path='doc/ViewPlan/*'/>
		PanelPlan ViewPlan { get; }
		/// <summary>
		/// Gets the panel window position.
		/// </summary>
		Place Window { get; }
		#endregion
		#region Methods
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
		/// Closes the plugin panel and opens the original file panel.
		/// </summary>
		/// <remarks>
		/// Mantis 1114: the current original panel item depends on the current plugin panel item on closing.
		/// It is resolved for module panels, the original current and even selected items should be restored.
		/// </remarks>
		void Close(); // _090321_210416
		/// <summary>
		/// Closes the plugin panel and opens a file panel with the specified path.
		/// </summary>
		/// <param name="path">
		/// Name of the directory that will be set in the panel after closing the panel (or {null|empty}).
		/// If the path doesn't exist the core shows an error message box always.
		/// </param>
		void Close(string path);
		/// <summary>
		/// Sets the specified item current by name, if it exists.
		/// </summary>
		/// <param name="name">Name or alternative name of an item to be set current.</param>
		/// <remarks>
		/// If the item does not exist or it is hidden and the panel does not show hidden items
		/// then just nothing happens, it is not an error.
		/// </remarks>
		/// <seealso cref="CurrentDirectory"/>
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
		/// <seealso cref="CurrentDirectory"/>
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
		/// <seealso cref="CurrentDirectory"/>
		/// <seealso cref="GoToName(string)"/>
		void GoToPath(string path);
		/// <summary>
		/// Selects all shown items.
		/// </summary>
		/// <remarks>
		/// Call <see cref="Redraw()"/> after that.
		/// </remarks>
		void SelectAll();
		/// <summary>
		/// Unselects all shown items.
		/// </summary>
		/// <remarks>
		/// Call <see cref="Redraw()"/> after that.
		/// </remarks>
		void UnselectAll();
		/// <summary>
		/// Selects shown items by their indexes.
		/// </summary>
		/// <param name="indexes">Indexes of items to be selected. Null is OK.</param>
		/// <remarks>
		/// Call <see cref="Redraw()"/> after that.
		/// </remarks>
		void SelectAt(int[] indexes);
		/// <summary>
		/// Unselects shown items by their indexes. See <see cref="Redraw()"/>.
		/// </summary>
		/// <param name="indexes">Indexes of items to be unselected. Null os OK.</param>
		/// <remarks>
		/// Call <see cref="Redraw()"/> after that.
		/// </remarks>
		void UnselectAt(int[] indexes);
		/// <include file='doc.xml' path='doc/SelectNames/*'/>
		void SelectNames(IEnumerable names);
		/// <include file='doc.xml' path='doc/UnselectNames/*'/>
		void UnselectNames(IEnumerable names);
		/// <summary>
		/// Pushes or puts the panel to the internal panel shelve.
		/// </summary>
		void Push();
		/// <summary>
		/// Gets indexes of selected items.
		/// </summary>
		/// <remarks>
		/// The indexes are valid only for the <see cref="ShownList"/> items.
		/// Unlike the <see cref="SelectedFiles"/> or <see cref="SelectedList"/> this list is empty if none is selected.
		/// </remarks>
		int[] SelectedIndexes();
		/// <summary>
		/// Gets true if selection exists.
		/// </summary>
		bool SelectionExists { get; }
		#endregion
	}

	/// <summary>
	/// Far panel kind.
	/// </summary>
	public enum PanelKind
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
		/// Sorted by hard link count.
		/// </summary>
		LinkCount,
		/// <summary>
		/// Sorted by NTFS stream count.
		/// </summary>
		StreamCount,
		/// <summary>
		/// Sorted by NTFS stream data size.
		/// </summary>
		StreamSize,
		/// <summary>
		/// Sorted by full name.
		/// </summary>
		FullName,
		/// <summary>
		/// Sorted by time of any change.
		/// </summary>
		ChangeTime,
		/// <summary>
		/// Unsorted mode.
		/// </summary>
		UnsortedReversed = -Unsorted,
		/// <summary>
		/// Sorted by name.
		/// </summary>
		NameReversed = -Name,
		/// <summary>
		/// Sorted by extension.
		/// </summary>
		ExtensionReversed = -Extension,
		/// <summary>
		/// Sorted by modification time.
		/// </summary>
		LastWriteTimeReversed = -LastWriteTime,
		/// <summary>
		/// Sorted by creation time.
		/// </summary>
		CreationTimeReversed = -CreationTime,
		/// <summary>
		/// Sorted by access time.
		/// </summary>
		LastAccessTimeReversed = -LastAccessTime,
		/// <summary>
		/// Sorted by length.
		/// </summary>
		LengthReversed = -Length,
		/// <summary>
		/// Sorted by description.
		/// </summary>
		DescriptionReversed = -Description,
		/// <summary>
		/// Sorted by owner.
		/// </summary>
		OwnerReversed = -Owner,
		/// <summary>
		/// Sorted by compressed size.
		/// </summary>
		CompressedSizeReversed = -CompressedSize,
		/// <summary>
		/// Sorted by hard link count.
		/// </summary>
		LinkCountReversed = -LinkCount,
		/// <summary>
		/// Sorted by NTFS stream count.
		/// </summary>
		StreamCountReversed = -StreamCount,
		/// <summary>
		/// Sorted by NTFS stream data size.
		/// </summary>
		StreamSizeReversed = -StreamSize,
		/// <summary>
		/// Sorted by full name.
		/// </summary>
		FullNameReversed = -FullName,
		/// <summary>
		/// Sorted by time of any change.
		/// </summary>
		ChangeTimeReversed = -ChangeTime
	}

	//! DictionaryEntry is not good for this, because it is a value type.
	//! DataItem is a reference type with some advantages.
	/// <summary>
	/// Named data item, e.g. an info panel item (<see cref="Panel.InfoItems"/>).
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
	/// Panel highlighting modes.
	/// </summary>
	public enum PanelHighlighting
	{
		/// <summary>
		/// Highlighting by item attributes only.
		/// </summary>
		Default,
		/// <summary>
		/// Highlighting by attributes and names.
		/// </summary>
		Full,
		/// <summary>
		/// Highlighting is turned off.
		/// </summary>
		Off
	}

	/// <summary>
	/// Panel dots item modes.
	/// </summary>
	public enum PanelDotsMode
	{
		/// <summary>
		/// Add the dots item if the panel is a child panel.
		/// </summary>
		Auto,
		/// <summary>
		/// Always add the dots item.
		/// </summary>
		Dots,
		/// <summary>
		/// No dots item.
		/// </summary>
		Off
	}

	/// <summary>
	/// Panel view plan.
	/// </summary>
	/// <remarks>
	/// Normally it is used for <see cref="Panel.SetPlan"/>.
	/// When a panel is opened you can change modes dynamically, but do not forget
	/// to reset the list itself, changes in items are not reflected without this.
	/// <para>
	/// WARNING: column titles, kinds and custom columns is a sort of low level stuff;
	/// if you use this incorrectly the Far may crash.
	/// </para>
	/// </remarks>
	/// <seealso cref="FarFile.Columns"/>
	/// <seealso cref="SetFile.Columns"/>
	public sealed class PanelPlan : ICloneable
	{
		/// <summary>
		/// Columns info.
		/// </summary>
		/// <remarks>
		/// <para>
		/// All supported kinds: "N", "Z", "O", "S", "DC", "DM", "DA", "C0", "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9".
		/// Supported Far column kind suffixes may be added to the end, e.g. NR, ST, DCB, and etc., see Far API [Column types].
		/// </para>
		/// <para>
		/// Default column kind sequence: "N", "Z", "O", "C0", "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9".
		/// It is exposed as <see cref="FarColumn.DefaultColumnKinds"/>.
		/// </para>
		/// <para>
		/// Column kind rules:
		/// <ul>
		/// <li>Specify column kinds only when you really have to do so, especially try to avoid C0..C9, let them to be processed by default.</li>
		/// <li>C0...C9 must be listed incrementally without gaps; but other kinds between them is OK. E.g. C0, C2 is bad; C0, N, C1 is OK.</li>
		/// <li>If a kind is not specified then the next available from the remaining default sequence is taken.</li>
		/// <li>Column kinds should not be specified more than once.</li>
		/// </ul>
		/// </para>
		/// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public FarColumn[] Columns { get; set; }
		/// <summary>
		/// Status columns info.
		/// </summary>
		/// <remarks>
		/// Use it for status columns in the same way as <see cref="Columns"/> is used.
		/// Column names are ignored.
		/// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
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
		/// Tells to align file extensions.
		/// </summary>
		public bool IsAlignedExtensions { get; set; }
		/// <summary>
		/// Tells to use name case conversion.
		/// </summary>
		public bool IsCaseConversion { get; set; }
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
	/// Panel event arguments.
	/// </summary>
	public class PanelEventArgs : EventArgs
	{
		/// <summary>
		/// Tells that a job is done or an action has to be ignored, it depends on the event.
		/// </summary>
		public bool Ignore { get; set; }
	}

	/// <summary>
	/// Arguments of <see cref="Panel.WorksInvokingCommand"/>.
	/// Set <see cref="PanelEventArgs.Ignore"/> = true to tell that command has been processed internally.
	/// </summary>
	public sealed class CommandLineEventArgs : PanelEventArgs
	{
		///
		public CommandLineEventArgs(string command) { Command = command; }
		/// <summary>
		/// Gets the command to be processed.
		/// </summary>
		public string Command { get; private set; }
	}

	/// <summary>
	/// Arguments of <see cref="Panel.ViewChanged"/> event. [FE_CHANGEVIEWMODE], [Column types].
	/// </summary>
	public sealed class ViewChangedEventArgs : PanelEventArgs
	{
		///
		public ViewChangedEventArgs(string columns) { Columns = columns; }
		/// <summary>
		/// Gets column kinds, e.g. N,S,D,T.
		/// </summary>
		public string Columns { get; private set; }
	}

	/// <summary>
	/// Panel column options (abstract).
	/// </summary>
	/// <remarks>
	/// Column options are used by <see cref="PanelPlan.Columns"/> and <see cref="PanelPlan.StatusColumns"/>.
	/// <para>
	/// This class is only a base for <see cref="SetColumn"/> (recommended and ready to use)
	/// and other classes derived by modules (basically they are not needed).
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
		/// Column kind. See <see cref="PanelPlan.Columns"/>.
		/// </summary>
		public virtual string Kind { get { return null; } set { throw new NotImplementedException(); } }
		/// <summary>
		/// Column width.
		/// </summary>
		/// <remarks>
		/// Positive: absolute width; negative: percentage; 0: variable.
		/// </remarks>
		public virtual int Width { get { return 0; } set { throw new NotImplementedException(); } }
		/// <summary>
		/// Default column kind sequence: "N", "Z", "O", "C0", "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9".
		/// </summary>
		public static ReadOnlyCollection<string> DefaultColumnKinds { get { return _DefaultColumnKinds; } }
		// _100202_113617 If count changes update related features.
		static readonly ReadOnlyCollection<string> _DefaultColumnKinds = new ReadOnlyCollection<string>(new string[] { "N", "Z", "O", "C0", "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9" });
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
		public override string Kind { get; set; }
		///
		public override int Width { get; set; }
	}

	/// <summary>
	/// Panel key bar item.
	/// </summary>
	public sealed class KeyBar
	{
		///
		public KeyBar(int virtualKeyCode, ControlKeyStates controlKeyState, string text, string longText)
		{
			Key = new KeyData(virtualKeyCode, controlKeyState);
			Text = text;
			LongText = longText;
		}
		///
		public KeyData Key { get; private set; }
		///
		public string Text { get; private set; }
		///
		public string LongText { get; private set; }
	}
}
