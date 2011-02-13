
/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace FarNet
{
	/// <summary>
	/// Module panel. Create it directly or a derived class instance,
	/// set properties, add event handlers and open it.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Exposed as <see cref="IFar.Panel"/> and <see cref="IFar.Panel2"/>.
	/// </para>
	/// <para>
	/// Properties of this class are requested by the core for a panel very often.
	/// For better performance it caches internal native representation of these data.
	/// After opening a panel it is recommended to avoid excessive modifications of data.
	/// </para>
	/// <para>
	/// Properties often configured before opening are:
	/// <see cref="Title"/>,
	/// <see cref="SortMode"/>,
	/// <see cref="ViewMode"/>.
	/// </para>
	/// </remarks>
	public partial class Panel : IPanel
	{
		readonly Works.IPanelWorks _Panel;
		Panel _Parent;
		Panel _Child;
		/// <summary>
		/// New module panel.
		/// </summary>
		public Panel() { _Panel = Far.Net.WorksPanel(this); }
		/// <summary>
		/// Gets or sets the attached file explorer.
		/// </summary>
		/// <remarks>
		/// An explorer should be attached only to a just created and not yet opened panel.
		/// Once the panel is opened its explorer should never be changed directly.
		/// But the core normally changes panel explorers on navigation.
		/// </remarks>
		public virtual Explorer Explorer { get; set; }
		///
		public int WorksId { get { return _Panel.WorksId; } }
		/// <include file='doc.xml' path='doc/Data/*'/>
		public Hashtable Data { get { return _Data ?? (_Data = new Hashtable()); } }
		Hashtable _Data;
		/// <summary>
		/// Gets the child panel.
		/// </summary>
		public Panel Child { get { return _Child; } }
		/// <summary>
		/// Gets the parent panel.
		/// </summary>
		/// <remarks>
		/// The parent panel is null if this panel is not a child panel.
		/// </remarks>
		public Panel Parent { get { return _Parent; } }
		/// <summary>
		/// Gets the default panel title to be set on show.
		/// </summary>
		protected virtual string DefaultTitle { get { return GetType().Name; } }
		/// <summary>
		/// Gets or sets the panel header.
		/// </summary>
		public string Title { get { return _Panel.Title; } set { _Panel.Title = value; } }
		// Opening worker, including a posted step
		void Open(object sender, EventArgs e)
		{
			if (_Parent == null)
				_Panel.Open();
			else
				_Panel.OpenReplace(_Parent); //???? is it correct to close the parent? should I better deny tricky openings?
		}
		/// <summary>
		/// Tells to open the panel when the core gets control.
		/// </summary>
		/// <remarks>
		/// Normally a panel should be opened when a module is called from panels window
		/// (command line, disk menu or plugins menu in panels). If panels window cannot
		/// be set current, this method fails.
		/// <para>
		/// Other possible reasons of failure:
		/// *) another panel has been already posted for opening;
		/// *) the module is not called for opening, e.g. it is called to process events.
		/// </para>
		/// <para>
		/// It is recommended to call this as soon as possible and only then to configure the panel and other data.
		/// Technically this method only tries to post the panel for opening and may fail due to the reasons above.
		/// Early call and failure helps to avoid some jobs for nothing.
		/// </para>
		/// </remarks>
		/// <seealso cref="Open(bool)"/>
		/// <seealso cref="OpenChild"/>
		public virtual void Open()
		{
			// done?
			if (IsOpened)
				return;

			// set the title to default
			if (string.IsNullOrEmpty(Title))
				Title = DefaultTitle;

			// try to open even not from panels
			WindowKind wt = Far.Net.Window.Kind;
			if (wt != WindowKind.Panels)
			{
				try
				{
					Far.Net.Window.SetCurrentAt(0);
				}
				catch (InvalidOperationException ex)
				{
					throw new ModuleException("Cannot open a panel because panels window cannot be set current.", ex);
				}

				// 090623 PostJob may not work from the editor, for example, see "... because a module is not called for opening".
				// I tried to ignore my check - a panel did not open. In contrast, PostStep calls via the menu where
				// a panel is opened from with no problems.
				Far.Net.PostStep(Open);
				return;
			}

			Open(null, null);
		}
		/// <summary>
		/// Calls <see cref="Open()"/> or <see cref="OpenChild"/> depending on the parameter.
		/// </summary>
		/// <param name="child">Tells to open the panel as a child of the active module panel, if any.</param>
		public void Open(bool child)
		{
			if (child)
				OpenChild(null);
			else
				Open();
		}
		/// <summary>
		/// Opens this panel as a child of the parent panel.
		/// </summary>
		/// <param name="parent">The opend parent panel. Null tells to use the active module panel, if any.</param>
		/// <remarks>
		/// When this panel is opened as a child of the parent panel, the parent is hidden, not closed.
		/// When the child closes itself later then the parent is shown again and its state is restored.
		/// </remarks>
		public void OpenChild(Panel parent)
		{
			// resolve 'null' parent
			if (parent == null)
			{
				// try to use the active as parent; do not use the passive, show as normal
				parent = FindPanel(true);
				if (parent == null)
				{
					// go
					Open();
					return;
				}
			}

			// sanity check
			if (IsOpened || _Parent != null)
				throw new InvalidOperationException();

			// allow to setup/fail
			if (!OpenChildBegin(parent))
				return;

			// link
			_Parent = parent;
			_Parent._Child = this;

			// begin
			_Parent.SaveState();

			// go
			Open();
		}
		/// <summary>
		/// It is called from <see cref="OpenChild"/> before linking the panels together.
		/// </summary>
		/// <param name="parent">The panel about to be parent.</param>
		/// <returns>False to cancel opening.</returns>
		/// <remarks>
		/// This method is designed for cases when the child opening depends on the parent.
		/// For example the initial current file may depend on the parent current file.
		/// When this methods is called the parent is the active panel and its data can
		/// be used as usual. The panels are not yet linked together, use the parameter
		/// in order to access the parent.
		/// <para>
		/// If it returns false or even throws then the parent panel remains opened.
		/// </para>
		/// </remarks>
		protected virtual bool OpenChildBegin(Panel parent) { return true; }
		/// <summary>
		/// Opens the panel by replacing another panel.
		/// </summary>
		/// <param name="current">The current panel to be replaced.</param>
		/// <remarks>
		/// Opening instead of the already opened panel may have some advantages:
		/// *) there are less core restrictions than for opening a new panel;
		/// *) the new panel inherits and keeps alive the parent panels.
		/// <para>
		/// If the current panel is not ready (see <see cref="CanClose()"/>) then this call has no effect.
		/// </para>
		/// </remarks>
		public void OpenInstead(Panel current)
		{
			if (current == null) throw new ArgumentNullException("current");

			// ask
			if (!current.CanClose())
				return;

			// clean current
			current.WorksClosed(false);

			// replace
			_Panel.OpenReplace(current);
		}
		/// <summary>
		/// Gets the active or passive module panel if any.
		/// </summary>
		/// <param name="active">Active or passive panel.</param>
		/// <returns>Found panel or null.</returns>
		public static Panel FindPanel(bool active)
		{
			Panel panel = Far.Net.FindPanel(typeof(Panel));
			if (panel == null)
				return null;
			if (panel.IsActive == active)
				return panel;
			return panel.AnotherPanel;
		}
		/// <summary>
		/// Gets another started module panel or null. Assume this panel is shown.
		/// </summary>
		public Panel AnotherPanel { get { return _Panel.AnotherPanel; } }
		/// <summary>
		/// Saves the panel state.
		/// This version saves a file name; it is the least effective but:
		/// *) indexes may change (when items added|removed)
		/// *) panel files can be recreated on getting data.
		/// </summary>
		protected virtual void SaveState()
		{
			FarFile f = CurrentFile;
			if (f != null)
				PostName(f.Name);
		}
		/// <summary>
		/// Closes this child panel and opens the parent panel if both panels are ready.
		/// </summary>
		/// <remarks>
		/// The method only tries to close the child panel.
		/// The panel is not closed if <see cref="CanClose"/> or <see cref="CanCloseChild"/> gets false.
		/// </remarks>
		public void CloseChild()
		{
			if (_Parent == null)
				throw new InvalidOperationException("The panel is not child.");

			// ask child
			if (!CanClose())
				return;

			// ask parent
			if (!_Parent.CanCloseChild())
				return;

			try
			{
				// clean this
				WorksClosed(false);
			}
			finally
			{
				// open parent
				_Parent._Panel.OpenReplace(this);

				// unlink
				_Parent._Child = null;
				_Parent = null;
			}
		}
		/// <summary>
		/// Can the panel close now?
		/// </summary>
		/// <remarks>
		/// It can be called from a child; in this case the panel is offline.
		/// </remarks>
		protected virtual bool CanClose() { return true; }
		/// <summary>
		/// Can the parent panel close its child?
		/// </summary>
		protected virtual bool CanCloseChild() { return true; }
		/// <summary>
		/// Gets the list of user objects to be disposed when the panel is closed.
		/// </summary>
		/// <remarks>
		/// It is mostly designed for PowerShell scripts and not recommended for modules.
		/// </remarks>
		[Obsolete("PowerShell scripts helper.")]
		public IList<IDisposable> Garbage { get { return _Garbage ?? (_Garbage = new List<IDisposable>()); } }
		List<IDisposable> _Garbage;
		/// <summary>
		/// Saves the panel data.
		/// </summary>
		/// <returns>True if there is no more data to save.</returns>
		public virtual bool SaveData()
		{
			return true;
		}
		#region Work Properties
		/// <summary>
		/// Gets <see cref="IFar.CurrentDirectory"/> saved when the panel starts.
		/// </summary>
		public string StartDirectory { get { return _Panel.StartDirectory; } }
		/// <summary>
		/// Tells how to add the dots item. See also <see cref="DotsDescription"/>.
		/// </summary>
		public PanelDotsMode DotsMode { get; set; }
		/// <summary>
		/// Gets or sets the dots item item description.
		/// </summary>
		/// <remarks>
		/// This text is used and shown only if the dots item is added, see <see cref="DotsMode"/>.
		/// </remarks>
		public string DotsDescription { get; set; }
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
		public Getter DataId { get; set; } //???? think to move this to explorer
		/// <summary>
		/// Gets true if the panel is opened.
		/// </summary>
		public bool IsOpened { get { return _Panel.IsOpened; } }
		/// <summary>
		/// Gets true if the panel is pushed.
		/// </summary>
		public bool IsPushed { get { return _Panel.IsPushed; } }
		/// <summary>
		/// Tells to update and redraw the panel automatically when idle.
		/// </summary>
		/// <remarks>
		/// If it is set the panel is updated automatically every few seconds when idle.
		/// This is suitable only for panels with very frequently changed data,
		/// otherwise it may cause overhead job for nothing.
		/// </remarks>
		/// <seealso cref="Idled"/>
		public bool IdleUpdate { get; set; }
		/// <summary>
		/// Gets or sets the panel type ID.
		/// </summary>
		/// <remarks>
		/// This property is optionally set once, normally by a creator.
		/// It is used for distinguishing panel types when a class type is not enough.
		/// </remarks>
		/// <seealso cref="IFar.FindPanel(Guid)"/>
		public Guid TypeId { get; set; }
		#endregion
		#region Work Methods
		/// <summary>
		/// Posts file data to be used to find and set the current file.
		/// </summary>
		/// <seealso cref="DataId"/>
		public void PostData(object data) { _Panel.PostData(data); }
		/// <summary>
		/// Posts a file to be found and set the current.
		/// </summary>
		public void PostFile(FarFile file) { _Panel.PostFile(file); }
		/// <summary>
		/// Posts a file name to be used to find and set the current file.
		/// </summary>
		public void PostName(string name) { _Panel.PostName(name); }
		#endregion
		#region Settable modes
		/// <summary>
		/// Gets or sets the case sensitive sort flag.
		/// </summary>
		public bool CaseSensitiveSort { get { return _Panel.CaseSensitiveSort; } set { _Panel.CaseSensitiveSort = value; } }
		/// <summary>
		/// Gets or sets the directories first sort flag.
		/// </summary>
		public bool DirectoriesFirst { get { return _Panel.DirectoriesFirst; } set { _Panel.DirectoriesFirst = value; } }
		/// <summary>
		/// Gets or sets the numeric sort flag.
		/// </summary>
		public bool NumericSort { get { return _Panel.NumericSort; } set { _Panel.NumericSort = value; } }
		#endregion
		#region Read only modes (at least for open)
		/// <summary>
		/// Gets the file highlighting flag.
		/// </summary>
		public bool Highlight { get { return _Panel.Highlight; } }
		/// <summary>
		/// Tells that the panel items represent the real file system.
		/// Set it before opening.
		/// </summary>
		/// <remarks>
		/// If this flag is set then panel item names are related to real file system names.
		/// </remarks>
		public bool RealNames { get { return _Panel.RealNames; } set { _Panel.RealNames = value; } }
		/// <summary>
		/// Gets the show selected files first flag.
		/// </summary>
		public bool SelectedFirst { get { return _Panel.SelectedFirst; } }
		/// <summary>
		/// Gets true if hidden and system files are shown.
		/// </summary>
		public bool ShowHidden { get { return _Panel.ShowHidden; } }
		/// <summary>
		/// Gets the use sort groups flags.
		/// Set it before opening.
		/// </summary>
		public bool UseSortGroups { get { return _Panel.UseSortGroups; } set { _Panel.UseSortGroups = value; } }
		#endregion
		#region Properties
		/// <summary>
		/// Gets the current file.
		/// </summary>
		public FarFile CurrentFile { get { return _Panel.CurrentFile; } }
		/// <summary>
		/// Gets the current file index in the <see cref="ShownList"/> files.
		/// </summary>
		/// <remarks>
		/// This is the index of the current file in the <see cref="ShownList"/> files.
		/// It is not directly related to other panel file collections.
		/// </remarks>
		public int CurrentIndex { get { return _Panel.CurrentIndex; } }
		/// <summary>
		/// Gets the current frame: current and top file indexes.
		/// </summary>
		public Point Frame { get { return _Panel.Frame; } }
		/// <summary>
		/// Gets true if the panel is active.
		/// </summary>
		public bool IsActive { get { return _Panel.IsActive; } }
		/// <summary>
		/// Gets true if the panel is the left panel.
		/// </summary>
		public bool IsLeft { get { return _Panel.IsLeft; } }
		/// <summary>
		/// Gets true always.
		/// </summary>
		public bool IsPlugin { get { return true; } }
		/// <summary>
		/// Gets or sets visibility of the panel.
		/// </summary>
		/// <remarks>
		/// Setting takes effect only when Far gets control.
		/// </remarks>
		public bool IsVisible { get { return _Panel.IsVisible; } set { _Panel.IsVisible = value; } }
		/// <summary>
		/// Gets file kind always.
		/// </summary>
		public PanelKind Kind { get { return PanelKind.File; } }
		/// <summary>
		/// Gets or sets the panel current directory.
		/// </summary>
		/// <remarks>
		/// Changing of the current directory depends on <see cref="SetPanelDirectory"/> handler.
		/// If the handler is missing and the directory exists in the core file system
		/// then the module panel is closed and a file panel is opened instead.
		/// </remarks>
		/// <seealso cref="GoToName(string)"/>
		/// <seealso cref="GoToPath"/>
		public string CurrentDirectory { get { return _Panel.CurrentDirectory; } set { _Panel.CurrentDirectory = value; } }
		/// <summary>
		/// Gets all selected panel files at once or the current file if none is selected.
		/// </summary>
		/// <remarks>
		/// In contrast to <see cref="SelectedList"/> this list is a snapshot of files,
		/// it can be used even after changes in the panel.
		/// </remarks>
		public IList<FarFile> SelectedFiles { get { return _Panel.SelectedFiles; } }
		/// <summary>
		/// Gets all selected panel files or the current file if none is selected.
		/// </summary>
		/// <remarks>
		/// In contrast to <see cref="SelectedFiles"/> you must not change panel items while using this list.
		/// </remarks>
		public IList<FarFile> SelectedList { get { return _Panel.SelectedList; } }
		/// <summary>
		/// Gets all shown panel files at once. File ".." is excluded.
		/// </summary>
		/// <remarks>
		/// In contrast to <see cref="ShownList"/> this list is a snapshot of all files,
		/// it can be used even after changes in the panel.
		/// </remarks>
		public IList<FarFile> ShownFiles { get { return _Panel.ShownFiles; } }
		/// <summary>
		/// Gets all shown panel files including "..".
		/// </summary>
		/// <remarks>
		/// In contrast to <see cref="ShownFiles"/> you must not change panel items while using this list.
		/// The current file index in this list is <see cref="CurrentIndex"/>.
		/// </remarks>
		public IList<FarFile> ShownList { get { return _Panel.ShownList; } }
		/// <summary>
		/// Gets the first visible file index.
		/// </summary>
		public int TopIndex { get { return _Panel.TopIndex; } }
		/// <summary>
		/// Gets or sets the panel sort mode. It works before and after opening.
		/// </summary>
		public PanelSortMode SortMode { get { return _Panel.SortMode; } set { _Panel.SortMode = value; } }
		/// <summary>
		/// Gets or sets the panel view mode. It works before and after opening.
		/// </summary>
		public PanelViewMode ViewMode { get { return _Panel.ViewMode; } set { _Panel.ViewMode = value; } }
		/// <summary>
		/// Gets the panel window position.
		/// </summary>
		public Place Window { get { return _Panel.Window; } }
		#endregion
		#region Core
		/// <summary>
		/// Tells to convert timestamps to FAT format for the Compare folders operation.
		/// </summary>
		/// <remarks>
		/// Set this flag if the panel file system doesn't provide time accuracy necessary for standard comparison operations.
		/// </remarks>
		public bool CompareFatTime { get { return _Panel.CompareFatTime; } set { _Panel.CompareFatTime = value; } }
		/// <summary>
		/// Tells to show file names using original case regardless of Far settings.
		/// </summary>
		public bool PreserveCase { get { return _Panel.PreserveCase; } set { _Panel.PreserveCase = value; } }
		/// <summary>
		/// Tells that folders are selected regardless of the core settings.
		/// </summary>
		public bool RawSelection { get { return _Panel.RawSelection; } set { _Panel.RawSelection = value; } }
		/// <summary>
		/// Tells to use the core method instead of <see cref="Panel.DeleteFiles"/> if <see cref="RealNames"/> = true.
		/// </summary>
		public bool RealNamesDeleteFiles { get { return _Panel.RealNamesDeleteFiles; } set { _Panel.RealNamesDeleteFiles = value; } }
		/// <summary>
		/// Tells to use the core method instead of <see cref="Panel.ExportFiles"/> if <see cref="RealNames"/> = true.
		/// </summary>
		public bool RealNamesExportFiles { get { return _Panel.RealNamesExportFiles; } set { _Panel.RealNamesExportFiles = value; } }
		/// <summary>
		/// Tells to use the core method instead of <see cref="Panel.ImportFiles"/> if <see cref="RealNames"/> = true.
		/// </summary>
		public bool RealNamesImportFiles { get { return _Panel.RealNamesImportFiles; } set { _Panel.RealNamesImportFiles = value; } }
		/// <summary>
		/// Tells to use the core method instead of <see cref="Panel.MakeDirectory"/> if <see cref="RealNames"/> = true.
		/// </summary>
		public bool RealNamesMakeDirectory { get { return _Panel.RealNamesMakeDirectory; } set { _Panel.RealNamesMakeDirectory = value; } }
		/// <summary>
		/// Tells to show file names right-aligned by default in all panel display modes.
		/// </summary>
		public bool RightAligned { get { return _Panel.RightAligned; } set { _Panel.RightAligned = value; } }
		/// <summary>
		/// Tells to show file names without paths by default.
		/// </summary>
		public bool ShowNamesOnly { get { return _Panel.ShowNamesOnly; } set { _Panel.ShowNamesOnly = value; } }
		/// <summary>
		/// Tells to use filter in the panel.
		/// </summary>
		public bool UseFilter { get { return _Panel.UseFilter; } set { _Panel.UseFilter = value; } }
		/// <summary>
		/// Gets or sets the panel highlighting mode.
		/// </summary>
		public PanelHighlighting Highlighting { get { return _Panel.Highlighting; } set { _Panel.Highlighting = value; } }
		#endregion
		#region Methods
		/// <summary>
		/// Redraws the panel. Normally you should call it after changes to make them visible.
		/// </summary>
		public void Redraw() { _Panel.Redraw(); }
		/// <summary>
		/// Redraws the panel and sets the current and\or the first visible item.
		/// If both arguments are negative, result is the same as per <see cref="Redraw()"/>
		/// </summary>
		/// <param name="current">Index of the current panel item.</param>
		/// <param name="top">Index of the first visible panel item.</param>
		public void Redraw(int current, int top) { _Panel.Redraw(current, top); }
		/// <summary>
		/// Updates panel contents.
		/// </summary>
		/// <param name="keepSelection">Keep the current selection.</param>
		public void Update(bool keepSelection) { _Panel.Update(keepSelection); }
		/// <summary>
		/// Closes the module panel and all parents and opens the original file panel.
		/// </summary>
		/// <remarks>
		/// Mantis 1114: the current original panel item depends on the current plugin panel item on closing.
		/// It is resolved for module panels, the original current and even selected items should be restored.
		/// </remarks>
		public void Close() { _Panel.Close(); } // _090321_210416
		/// <summary>
		/// Closes the module panel and all parents and opens the original file panel with the specified path.
		/// </summary>
		/// <param name="path">
		/// Name of the directory that will be set in the panel after closing the panel (or {null|empty}).
		/// If the path doesn't exist the core shows an error message box always.
		/// </param>
		public void Close(string path) { _Panel.Close(path); }
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
		public void GoToName(string name) { _Panel.GoToName(name); }
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
		public bool GoToName(string name, bool fail) { return _Panel.GoToName(name, fail); }
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
		public void GoToPath(string path) { _Panel.GoToPath(path); }
		/// <summary>
		/// Selects all shown items.
		/// </summary>
		/// <remarks>
		/// Call <see cref="Redraw()"/> after that.
		/// </remarks>
		public void SelectAll() { _Panel.SelectAll(); }
		/// <summary>
		/// Unselects all shown items.
		/// </summary>
		/// <remarks>
		/// Call <see cref="Redraw()"/> after that.
		/// </remarks>
		public void UnselectAll() { _Panel.UnselectAll(); }
		/// <summary>
		/// Selects shown items by their indexes. See <see cref="Redraw()"/>.
		/// </summary>
		/// <param name="indexes">Indexes of items to be selected. Null is OK.</param>
		/// <remarks>
		/// Call <see cref="Redraw()"/> after that.
		/// </remarks>
		public void SelectAt(int[] indexes) { _Panel.SelectAt(indexes); }
		/// <summary>
		/// Unselects shown items by their indexes. See <see cref="Redraw()"/>.
		/// </summary>
		/// <param name="indexes">Indexes of items to be unselected. Null os OK.</param>
		/// <remarks>
		/// Call <see cref="Redraw()"/> after that.
		/// </remarks>
		public void UnselectAt(int[] indexes) { _Panel.UnselectAt(indexes); }
		/// <summary>
		/// Select panel items with specified names.
		/// </summary>
		/// <param name="names">Names to be selected. Null is OK.</param>
		/// <remarks>
		/// Call <see cref="Redraw()"/> after that.
		/// <para>
		/// Names are processed as case sensitive, not found input names are ignored.
		/// </para>
		/// </remarks>
		public void SelectNames(string[] names) { _Panel.SelectNames(names); }
		/// <summary>
		/// Unselect panel items with specified names.
		/// </summary>
		/// <param name="names">Names to be selected. Null is OK.</param>
		/// <remarks>
		/// Call <see cref="Redraw()"/> after that.
		/// <para>
		/// Names are processed as case sensitive, not found input names are ignored.
		/// </para>
		/// </remarks>
		public void UnselectNames(string[] names) { _Panel.UnselectNames(names); }
		/// <summary>
		/// Pushes or puts the panel to the internal panel shelve.
		/// </summary>
		public void Push() { _Panel.Push(); }
		/// <summary>
		/// Gets indexes of selected items.
		/// </summary>
		/// <remarks>
		/// The indexes are valid only for the <see cref="ShownList"/> items.
		/// Unlike the <see cref="SelectedFiles"/> this list is empty if none is selected.
		/// </remarks>
		public int[] SelectedIndexes() { return _Panel.SelectedIndexes(); }
		#endregion
		/// <summary>
		/// Gets or sets the list of panel items.
		/// </summary>
		/// <remarks>
		/// This is the main member of the panel: its items.
		/// <para>
		/// For performance and simplicity sake the list is not protected and it should be used carefully.
		/// Normally it is filled on startup and then can be changed by <see cref="UpdateFiles"/> handler.
		/// If it is changed differently then <see cref="IPanel.Update"/> should be called immediately;
		/// otherwise not coherent panel and list data may cause unpredictable problems.
		/// </para>
		/// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public IList<FarFile> Files { get { return _Panel.Files; } set { _Panel.Files = value; } }
		#region Core Events
		/// <summary>
		/// Called periodically when a user is idle.
		/// Modules can use this event for panel updating and redrawing.
		/// </summary>
		/// <seealso cref="IdleUpdate"/>
		/// <seealso cref="IdledHandler"/>
		public event EventHandler Idled;
		///
		public void WorksIdled()
		{
			if (Idled != null)
				Idled(this, null);
		}
		/// <summary>
		/// Called when a panel has been closed.
		/// </summary>
		/// <remarks>
		/// The handler should clean the panel resources and do nothing else.
		/// </remarks>
		public event EventHandler Closed;
		///
		public void WorksClosed(bool all)
		{
			// 1) event
			if (Closed != null)
				Closed(this, null);

			// 2) garbage
			if (_Garbage != null)
			{
				foreach (var it in _Garbage)
					it.Dispose();
				_Garbage = null;
			}

			// 3) parents
			if (all && _Parent != null)
				_Parent.WorksClosed(true);
		}
		/// <summary>
		/// Called on invoking a command from the command line.
		/// </summary>
		/// <remarks>
		/// If the command is supported then set <see cref="PanelEventArgs.Ignore"/> = true
		/// as soon as possible before potential exceptions.
		/// </remarks>
		public event EventHandler<CommandLineEventArgs> InvokingCommand;
		///
		public bool WorksInvokingCommand(CommandLineEventArgs e)
		{
			if (InvokingCommand == null)
				return false;
			if (e != null)
				InvokingCommand(this, e);
			return true;
		}
		/// <summary>
		/// Called on panel properties request. Use it only when it is absolutely needed.
		/// </summary>
		/// <remarks>
		/// Normally you should not use this event. It is not recommended for many reasons:
		/// *) it is called very frequently;
		/// *) it is expensive due to internal technical details;
		/// *) it may have problems if panel data are accessed, even for reading.
		/// <para>
		/// Try to update the panel info only from other panel handlers. Even if this panel
		/// info depends on external data that are changed outside, consider to update the info
		/// in <see cref="Redrawing"/> event handler.
		/// </para>
		/// </remarks>
		public event EventHandler UpdateInfo;
		///
		public void WorksUpdateInfo()
		{
			if (UpdateInfo != null)
				UpdateInfo(this, null);
		}
		/// <summary>
		/// Called to prepare <see cref="Files"/> list in the current directory of a file system emulated by the panel.
		/// </summary>
		/// <remarks>
		/// If the file set is constant and may be filled once on the panel creation then this event is not needed.
		/// </remarks>
		public event EventHandler<PanelEventArgs> UpdateFiles;
		///
		public bool WorksUpdateFiles(PanelEventArgs e)
		{
			if (UpdateFiles == null)
				return false;
			if (e != null)
				UpdateFiles(this, e);
			return true;
		}
		/// <summary>
		/// Called when a panel is about to be closed.
		/// </summary>
		/// <remarks>
		/// Bug [_090321_165608].
		/// Unfortunately Far triggers this also on plugin commands from command line
		/// even if a new panel is not going to be opened and the current one closed.
		/// </remarks>
		public event EventHandler<PanelEventArgs> Closing;
		///
		public bool WorksClosing(PanelEventArgs e)
		{
			if (Closing == null)
				return false;
			if (e != null)
				Closing(this, e);
			return true;
		}
		/// <summary>
		/// Called when [CtrlBreak] is pressed.
		/// </summary>
		/// <remarks>
		/// Processing is performed in a separate thread,
		/// so be careful, use only thread safe approaches.
		/// </remarks>
		public event EventHandler CtrlBreak;
		///
		public void WorksCtrlBreak()
		{
			if (CtrlBreak != null)
				CtrlBreak(this, null);
		}
		/// <summary>
		/// Called when the panel is about to redraw.
		/// </summary>
		/// <remarks>
		/// Set <see cref="PanelEventArgs.Ignore"/> = true if the module redraws the panel itself.
		/// </remarks>
		public event EventHandler<PanelEventArgs> Redrawing;
		///
		public bool WorksRedrawing(PanelEventArgs e)
		{
			if (Redrawing == null)
				return false;
			if (e != null)
				Redrawing(this, e);
			return true;
		}
		/// <summary>
		/// Called when panel view mode is changed.
		/// </summary>
		public event EventHandler<ViewChangedEventArgs> ViewChanged;
		///
		public bool WorksViewChanged(ViewChangedEventArgs e)
		{
			if (ViewChanged == null)
				return false;
			if (e != null)
				ViewChanged(this, e);
			return true;
		}
		/// <summary>
		/// Called when a key is about to be processed.
		/// </summary>
		/// <remarks>
		/// Set <see cref="PanelEventArgs.Ignore"/> = true if the module processes the key itself.
		/// <para>
		/// Normally panels are not interested in this event, they use <see cref="KeyPressed"/>.
		/// This event is needed for advanced processing of some special keys.
		/// </para>
		/// <para>
		/// In Far Manager this event is called from the <c>ProcessKeyW</c> with the <c>PKF_PREPROCESS</c> flag.
		/// </para>
		/// </remarks>
		public event EventHandler<PanelKeyEventArgs> KeyPressing;
		///
		public bool WorksKeyPressing(PanelKeyEventArgs e)
		{
			if (KeyPressing == null)
				return false;
			if (e != null)
				KeyPressing(this, e);
			return true;
		}
		/// <summary>
		/// Called in order to set the <see cref="PanelDirectory"/> in the virtual file system.
		/// If all is fine it becomes the <see cref="CurrentDirectory"/>.
		/// </summary>
		/// <remarks>
		/// <para>
		/// *) This method is not called if the <see cref="Explorer"/> is not null.
		/// *) It is recommended to use the explorer approach.
		/// *) This event presumably will be removed.
		/// </para>
		/// <para>
		/// This method is vitally important for navigation in the virtual file system, especially for find and scan operations.
		/// Design it carefully, read all the remarks for <see cref="SetDirectoryEventArgs"/> and its members.
		/// </para>
		/// </remarks>
		public event EventHandler<SetDirectoryEventArgs> SetPanelDirectory;
		///
		public bool WorksSetPanelDirectory(SetDirectoryEventArgs e)
		{
			if (SetPanelDirectory == null)
				return false;
			if (e != null)
				SetPanelDirectory(this, e);
			return true;
		}
		/// <summary>
		/// Called to delete files in the file system emulated by the panel.
		/// </summary>
		public event EventHandler<FilesEventArgs> DeleteFiles;
		///
		public bool WorksDeleteFiles(FilesEventArgs e)
		{
			if (DeleteFiles == null)
				return false;
			if (e != null)
				DeleteFiles(this, e);
			return true;
		}
		/// <summary>
		/// Called to export files on copy, move, edit, and view operations.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This event is not called if <see cref="Panel.RealNames"/> is true,
		/// Far Manager performs the operations itself.
		/// </para>
		/// <para>
		/// The module has to process its virtual directories itself.
		/// </para>
		/// </remarks>
		public event EventHandler<ExportFilesEventArgs> ExportFiles;
		///
		public bool WorksExportFiles(ExportFilesEventArgs e)
		{
			if (ExportFiles == null)
				return false;
			if (e != null)
				ExportFiles(this, e);
			return true;
		}
		/// <summary>
		/// Called to import files to the panel on copy and move operations.
		/// </summary>
		public event EventHandler<ImportFilesEventArgs> ImportFiles;
		///
		public bool WorksImportFiles(ImportFilesEventArgs e)
		{
			if (ImportFiles == null)
				return false;
			if (e != null)
				ImportFiles(this, e);
			return true;
		}
		/// <summary>
		/// Called to create a new panel item on [F7] hotkey.
		/// </summary>
		/// <remarks>
		/// A handler should be ready to process <see cref="OperationModes.Silent"/> flag.
		/// Set <see cref="PanelEventArgs.Ignore"/> to true if processing fails or should be ignored.
		/// <para>
		/// It is assumed that this method creates a new item with the <see cref="MakeDirectoryEventArgs.Name"/> name.
		/// If the <see cref="PanelEventArgs.Mode"/> has no <see cref="OperationModes.Silent"/> flag you are supposed
		/// to return (set) a new item name, so that this item will be current after panel update and redraw.
		/// </para>
		/// <para>
		/// Don't be confused by the method's name, it is given historically due to associated [F7] hotkey.
		/// In fact, you can create panel items of any kind, directories (containers) or files (leaves).
		/// </para>
		/// <para>
		/// Remember that this is not the only way of adding items, for example you can process the same [F7] key
		/// (or any other key) in <see cref="KeyPressed"/> event, or add items by a menu command, and etc.
		/// In these cases you may want to set a new item current yourself, e.g. by calling <c>Post*()</c> methods.
		/// </para>
		/// <para>
		/// Main reasons to use alternative ways of creating items:
		/// *) an operation is started not by pressing [F7];
		/// *) item names are not unique in a panel.
		/// </para>
		/// </remarks>
		public event EventHandler<MakeDirectoryEventArgs> MakeDirectory;
		///
		public bool WorksMakeDirectory(MakeDirectoryEventArgs e)
		{
			if (MakeDirectory == null)
				return false;
			if (e != null)
				MakeDirectory(this, e);
			return true;
		}
		/// <summary>
		/// Called when the panel has got focus.
		/// </summary>
		public event EventHandler GotFocus;
		///
		public void WorksGotFocus()
		{
			if (GotFocus != null)
				GotFocus(this, null);
		}
		/// <summary>
		/// Called when the panel is losing focus.
		/// </summary>
		public event EventHandler LosingFocus;
		///
		public void WorksLosingFocus()
		{
			if (LosingFocus != null)
				LosingFocus(this, null);
		}
		#endregion
		#region Other Info
		/// <summary>
		/// Gets or sets the format name (shown in the file copy dialog).
		/// </summary>
		public string FormatName { get { return _Panel.FormatName; } set { _Panel.FormatName = value; } }
		/// <summary>
		/// Gets or sets the base file of emulated file system.
		/// </summary>
		/// <remarks>
		/// If the panel doesn't emulate a file system based on files it should be empty.
		/// </remarks>
		public string HostFile { get { return _Panel.HostFile; } set { _Panel.HostFile = value; } }
		#endregion
		/// <summary>
		/// Gets or sets info panel item array.
		/// </summary>
		/// <remarks>
		/// If you get it and change items then set it again after changes.
		/// Without that individual item changes will have no effect.
		/// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public DataItem[] InfoItems { get { return _Panel.InfoItems; } set { _Panel.InfoItems = value; } }
		/// <summary>
		/// Sets 1-12 key bar labels, use empty labels for Far defaults.
		/// </summary>
		public void SetKeyBar(string[] labels) { _Panel.SetKeyBar(labels); }
		/// <summary>
		/// Sets 1-12 key bar labels, use empty labels for Far defaults.
		/// </summary>
		public void SetKeyBarCtrl(string[] labels) { _Panel.SetKeyBarCtrl(labels); }
		/// <summary>
		/// Sets 1-12 key bar labels, use empty labels for Far defaults.
		/// </summary>
		public void SetKeyBarAlt(string[] labels) { _Panel.SetKeyBarAlt(labels); }
		/// <summary>
		/// Sets 1-12 key bar labels, use empty labels for Far defaults.
		/// </summary>
		public void SetKeyBarShift(string[] labels) { _Panel.SetKeyBarShift(labels); }
		/// <summary>
		/// Sets 1-12 key bar labels, use empty labels for Far defaults.
		/// </summary>
		public void SetKeyBarCtrlShift(string[] labels) { _Panel.SetKeyBarCtrlShift(labels); }
		/// <summary>
		/// Sets 1-12 key bar labels, use empty labels for Far defaults.
		/// </summary>
		public void SetKeyBarAltShift(string[] labels) { _Panel.SetKeyBarAltShift(labels); }
		/// <summary>
		/// Sets 1-12 key bar labels, use empty labels for Far defaults.
		/// </summary>
		public void SetKeyBarCtrlAlt(string[] labels) { _Panel.SetKeyBarCtrlAlt(labels); }
		/// <summary>
		/// Gets the panel view plan or null if it is not set.
		/// </summary>
		/// <param name="mode">View mode to get the plan for.</param>
		/// <returns>
		/// The view plan. If you change it for opened panel then call <see cref="SetPlan"/>.
		/// </returns>
		public PanelPlan GetPlan(PanelViewMode mode) { return _Panel.GetPlan(mode); }
		/// <summary>
		/// Sets the panel plan.
		/// </summary>
		/// <param name="mode">View mode to set the plan for.</param>
		/// <param name="plan">The plan.</param>
		public void SetPlan(PanelViewMode mode, PanelPlan plan) { _Panel.SetPlan(mode, plan); }
		/// <summary>
		/// Gets or sets the panel directory of the virtual file system.
		/// </summary>
		/// <remarks>
		/// If it is empty then the core closes the panel when [Enter] is pressed on the dots item.
		/// </remarks>
		public string PanelDirectory { get { return _Panel.PanelDirectory; } set { _Panel.PanelDirectory = value; } }
		/// <summary>
		/// Gets or sets the extension of temporary files.
		/// </summary>
		/// <remarks>
		/// This might be useful in order to get proper syntax highlighting by the <i>Colorer</i> plugin.
		/// </remarks>
		public string TempFileExtension { get; set; }
	}
}
