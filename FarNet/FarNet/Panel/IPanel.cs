
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections;
using System.Collections.Generic;

namespace FarNet;

/// <summary>
/// Any panel: native, plugin, or module.
/// Exposed as <see cref="IFar.Panel"/> and <see cref="IFar.Panel2"/>.
/// </summary>
public interface IPanel //! think twice when convert to abstract class (see Panel2 : Panel1, IPanel)
{
	#region Settable modes
	/// <summary>
	/// Gets or sets the directories first sort flag.
	/// </summary>
	bool DirectoriesFirst { get; set; }
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
	/// Gets the current file index in <see cref="Files"/>.
	/// </summary>
	int CurrentIndex { get; }

	/// <summary>
	/// Gets the current frame: current and top file indexes.
	/// </summary>
	Point Frame { get; }

	/// <summary>
	/// Gets true if the panel is active.
	/// </summary>
	/// <seealso cref="SetActive"/>
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

	/// <include file='doc.xml' path='doc/Files/*'/>
	IList<FarFile> Files { get; }

	/// <include file='doc.xml' path='doc/GetFiles/*'/>
	FarFile[] GetFiles();

	/// <include file='doc.xml' path='doc/SelectedList/*'/>
	IList<FarFile> SelectedList { get; }

	/// <include file='doc.xml' path='doc/GetSelectedFiles/*'/>
	FarFile[] GetSelectedFiles();

	/// <summary>
	/// Obsolete, use GetSelectedFiles.
	/// </summary>
	[Obsolete("use GetSelectedFiles")]
	IList<FarFile> SelectedFiles { get; }

	/// <summary>
	/// Obsolete, use GetFiles.
	/// </summary>
	[Obsolete("use GetFiles")]
	IList<FarFile> ShownFiles { get; }

	/// <summary>
	/// Obsolete, use Files.
	/// </summary>
	[Obsolete("use Files")]
	IList<FarFile> ShownList { get; }

	/// <summary>
	/// Gets true if selection exists.
	/// </summary>
	bool SelectionExists { get; }

	/// <summary>
	/// Gets or sets the panel sort mode.
	/// </summary>
	PanelSortMode SortMode { get; set; }

	/// <summary>
	/// Gets the first visible file index.
	/// </summary>
	int TopIndex { get; }

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

	/// <include file='doc.xml' path='doc/SelectedIndexes/*'/>
	int[] SelectedIndexes();

	/// <summary>
	/// Sets the panel active.
	/// </summary>
	/// <seealso cref="IsActive"/>
	/// <exception cref="InvalidOperationException">The panel cannot be active, e.g. it is hidden.</exception>
	void SetActive();
	#endregion
}
