using System.Collections.Generic;
using System;

namespace FarManager
{
	/// <summary>
	/// Item of file system
	/// </summary>
	public interface IFile
	{
		/// <summary>
		/// Full path
		/// </summary>
		string Path { get; set; }
		/// <summary>
		/// Name
		/// </summary>
		string Name { get; set; }
		/// <summary>
		/// Is the item folder
		/// </summary>
		bool IsFolder { get; set; }
		/// <summary>
		/// Description
		/// </summary>
		string Description { get; set; }
		/// <summary>
		/// Owner
		/// </summary>
		string Owner { get; set; }
		/// <summary>
		/// Alternate name
		/// </summary>
		string AlternateName { get; set; }
		/// <summary>
		/// Creation time
		/// </summary>
		DateTime CreationTime { get; set; }
		/// <summary>
		/// Last access time
		/// </summary>
		DateTime LastAccessTime { get; set; }
		/// <summary>
		/// Size of file
		/// </summary>
		long Size { get; set; }
		/// <summary>
		/// Is file readonly
		/// </summary>
		bool IsReadOnly { get; set; }
		/// <summary>
		/// is file hidden
		/// </summary>
		bool IsHidden { get; set; }
		/// <summary>
		/// is the file volume label
		/// </summary>
		bool IsVolume { get; set; }
		/// <summary>
		/// System attribute
		/// </summary>
		bool IsSystem { get; set; }
		/// <summary>
		/// Directory attribute
		/// </summary>
		bool IsDirectory { get; set; }
		/// <summary>
		/// Archive attribute
		/// </summary>
		bool IsArchive { get; set; }
		/// <summary>
		/// Alias attribute
		/// </summary>
		bool IsAlias { get; set; }
		/// <summary>
		/// Compressed attribute
		/// </summary>
		bool IsCompressed { get; set; }
		/// <summary>
		/// Encrypted attribute
		/// </summary>
		bool IsEncrypted { get; set; }
		/// <summary>
		/// Selected state
		/// </summary>
		bool IsSelected { get; set; }
		/// <summary>
		/// Parent folder
		/// </summary>
		IFolder Parent { get; set; }
	}

	/// <summary>
	/// Folder of file system
	/// </summary>
	public interface IFolder : IFile
	{
		/// <summary>
		/// File items in the folder
		/// </summary>
		IList<IFile> Files { get; }
	}

	/// <summary>
	/// Type of panel.
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
		FileLinks
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
		MTime,
		/// <summary>
		/// Sorted by creation time.
		/// </summary>
		CTime,
		/// <summary>
		/// Sorted by access time.
		/// </summary>
		ATime,
		/// <summary>
		/// Sorted by size.
		/// </summary>
		Size,
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
		LinksNumber
	}

	/// <summary>
	/// Panel of the Far Manager
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
		/// Currentt item.
		/// </summary>
		IFile Current { get; }
		/// <summary>
		/// Top item.
		/// </summary>
		IFile Top { get; }
		/// <summary>
		/// View mode.
		/// </summary>
		PanelViewMode ViewMode { get; }
		/// <summary>
		/// Folder displayed by panel.
		/// </summary>
		IFolder Contents { get; }
		/// <summary>
		/// List of selected items.
		/// </summary>
		IList<IFile> Selected { get; }
		/// <summary>
		/// Panel type.
		/// </summary>
		PanelType Type { get; }
		/// <summary>
		/// Panel sort mode.
		/// </summary>
		PanelSortMode SortMode { get; }
		/// <summary>
		/// Hidden and system files are displayed.
		/// </summary>
		bool ShowHidden { get; }
		/// <summary>
		/// File highlighting is used.
		/// </summary>
		bool Highlight { get; }
		/// <summary>
		/// Descending sort is used.
		/// </summary>
		bool ReverseSortOrder { get; }
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
		bool NumericSort { get; }
		/// <summary>
		/// Redraws the panel.
		/// </summary>
		void Redraw();
		/// <summary>
		/// Updates panel contents.
		/// </summary>
		/// <param name="keepSelection">Keep the current selection.</param>
		void Update(bool keepSelection);
	}
}
