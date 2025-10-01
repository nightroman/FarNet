﻿namespace FarNet;

/// <summary>
/// Explorer of a virtual file system, file provider and manager, and optionally a panel maker.
/// </summary>
/// <remarks>
/// <para>
/// Explorers are used for virtual file system navigation and operations on files.
/// They provide files and explorers of other file locations.
/// </para>
/// <para>
/// Explorers are designed for panels and they normally implement the <see cref="CreatePanel"/> method.
/// But panels are not required for file operations, explorers can be used for pure file management.
/// Explorers can but do not have to create, configure, and update panels.
/// The core creates default panels for files when needed.
/// </para>
/// <para>
/// On requests explorers have to create and return new explorers or return nulls.
/// They should never return themselves because the core assumes that each explorer
/// is responsible for its own virtual directory that never change. In other words:
/// once created, an explorer should always return the same data, of course,
/// if these data are not changed in the virtual file system.
/// </para>
/// </remarks>
/// <param name="typeId">The explorer type ID.</param>
public abstract class Explorer(Guid typeId)
{
	/// <summary>
	/// Gets the explorer type ID.
	/// </summary>
	/// <remarks>
	/// The core distinguishes explorer types by their type IDs, not by their class types.
	/// Thus, if a few classes share the same type ID then all of them are treated by the core as the same explorer type.
	/// </remarks>
	public Guid TypeId => typeId;

	/// <summary>
	/// Gets or sets the location assigned to this explorer.
	/// It is set once on creation.
	/// </summary>
	public string Location
	{
		get => _Location ?? string.Empty;
		set => _Location = _Location == null ? value : throw new InvalidOperationException("It is set once on creation.");
	}
	string? _Location;

	/// <summary>
	/// Gets or sets the explorer function flags.
	/// It is normally set on creation and should not change.
	/// </summary>
	public ExplorerFunctions Functions { get; set; }

	/// <summary>
	/// Gets or sets the flag in the <see cref="Functions"/>.
	/// If this flag is set to true then the explorer implements <see cref="ExploreLocation"/>
	/// and works with pure paths, i.e. uses files without attached <see cref="FarFile.Data"/>.
	/// </summary>
	public bool CanExploreLocation
	{
		get { return (Functions & ExplorerFunctions.ExploreLocation) != 0; }
		set { Functions = value ? (Functions | ExplorerFunctions.ExploreLocation) : (Functions & ~ExplorerFunctions.ExploreLocation); }
	}

	/// <summary>
	/// Gets or sets the flag in the <see cref="Functions"/>.
	/// </summary>
	public bool CanGetContent
	{
		get { return (Functions & ExplorerFunctions.GetContent) != 0; }
		set { Functions = value ? (Functions | ExplorerFunctions.GetContent) : (Functions & ~ExplorerFunctions.GetContent); }
	}

	/// <summary>
	/// Gets or sets the flag in the <see cref="Functions"/>.
	/// </summary>
	public bool CanSetFile
	{
		get { return (Functions & ExplorerFunctions.SetFile) != 0; }
		set { Functions = value ? (Functions | ExplorerFunctions.SetFile) : (Functions & ~ExplorerFunctions.SetFile); }
	}

	/// <summary>
	/// Gets or sets the flag in the <see cref="Functions"/>.
	/// </summary>
	public bool CanSetText
	{
		get { return (Functions & ExplorerFunctions.SetText) != 0; }
		set { Functions = value ? (Functions | ExplorerFunctions.SetText) : (Functions & ~ExplorerFunctions.SetText); }
	}

	/// <summary>
	/// Gets or sets the flag in the <see cref="Functions"/>.
	/// </summary>
	public bool CanAcceptFiles
	{
		get { return (Functions & ExplorerFunctions.AcceptFiles) != 0; }
		set { Functions = value ? (Functions | ExplorerFunctions.AcceptFiles) : (Functions & ~ExplorerFunctions.AcceptFiles); }
	}

	/// <summary>
	/// Gets or sets the flag in the <see cref="Functions"/>.
	/// </summary>
	public bool CanDeleteFiles
	{
		get { return (Functions & ExplorerFunctions.DeleteFiles) != 0; }
		set { Functions = value ? (Functions | ExplorerFunctions.DeleteFiles) : (Functions & ~ExplorerFunctions.DeleteFiles); }
	}

	/// <summary>
	/// Gets or sets the flag in the <see cref="Functions"/>.
	/// </summary>
	public bool CanExportFiles
	{
		get { return (Functions & ExplorerFunctions.ExportFiles) != 0; }
		set { Functions = value ? (Functions | ExplorerFunctions.ExportFiles) : (Functions & ~ExplorerFunctions.ExportFiles); }
	}

	/// <summary>
	/// Gets or sets the flag in the <see cref="Functions"/>.
	/// </summary>
	public bool CanImportFiles
	{
		get { return (Functions & ExplorerFunctions.ImportFiles) != 0; }
		set { Functions = value ? (Functions | ExplorerFunctions.ImportFiles) : (Functions & ~ExplorerFunctions.ImportFiles); }
	}

	/// <summary>
	/// Gets or sets the flag in the <see cref="Functions"/>.
	/// </summary>
	public bool CanCreateFile
	{
		get { return (Functions & ExplorerFunctions.CreateFile) != 0; }
		set { Functions = value ? (Functions | ExplorerFunctions.CreateFile) : (Functions & ~ExplorerFunctions.CreateFile); }
	}

	/// <summary>
	/// Gets or sets the flag in the <see cref="Functions"/>.
	/// </summary>
	public bool CanOpenFile
	{
		get { return (Functions & ExplorerFunctions.OpenFile) != 0; }
		set { Functions = value ? (Functions | ExplorerFunctions.OpenFile) : (Functions & ~ExplorerFunctions.OpenFile); }
	}

	/// <summary>
	/// Gets or sets the flag in the <see cref="Functions"/>.
	/// </summary>
	public bool CanCloneFile
	{
		get { return (Functions & ExplorerFunctions.CloneFile) != 0; }
		set { Functions = value ? (Functions | ExplorerFunctions.CloneFile) : (Functions & ~ExplorerFunctions.CloneFile); }
	}

	/// <summary>
	/// Gets or sets the flag in the <see cref="Functions"/>.
	/// </summary>
	public bool CanRenameFile
	{
		get { return (Functions & ExplorerFunctions.RenameFile) != 0; }
		set { Functions = value ? (Functions | ExplorerFunctions.RenameFile) : (Functions & ~ExplorerFunctions.RenameFile); }
	}

	/// <summary>
	/// Returns the files.
	/// </summary>
	/// <param name="args">.</param>
	/// <remarks>
	/// <para>
	/// If the result is not <c>IList</c> then it is enumerated once and copied.
	/// If the result is <c>IList</c> then it may be used several times.
	/// This list must not change until the next <c>GetFiles</c>.
	/// </para>
	/// <include file='doc.xml' path='doc/ExplorerModes/*'/>
	/// </remarks>
	public abstract IEnumerable<FarFile> GetFiles(GetFilesEventArgs args);

	/// <summary>
	/// Returns a new directory explorer or null. It must not return itself.
	/// </summary>
	/// <param name="args">.</param>
	/// <remarks>
	/// It is not called if <see cref="Functions"/> contains the <see cref="ExploreLocation"/> flag.
	/// <para>
	/// It is called when a user enters a directory, on search, and scan.
	/// It should just get another explorer with a new location and do nothing else.
	/// </para>
	/// <include file='doc.xml' path='doc/ExplorerModes/*'/>
	/// </remarks>
	public virtual Explorer? ExploreDirectory(ExploreDirectoryEventArgs args) { return null; }

	/// <summary>
	/// Returns a new location explorer or null. It must not return itself.
	/// </summary>
	/// <param name="args">.</param>
	/// <remarks>
	/// It is called only if <see cref="Functions"/> contains the <see cref="ExplorerFunctions.ExploreLocation"/> flag.
	/// Note that this method works with pure paths, i.e. files without attached <see cref="FarFile.Data"/>.
	/// <include file='doc.xml' path='doc/ExplorerModes/*'/>
	/// </remarks>
	public virtual Explorer? ExploreLocation(ExploreLocationEventArgs args) { return null; }

	/// <summary>
	/// Returns a new parent explorer or null. It must not return itself.
	/// </summary>
	/// <param name="args">.</param>
	/// <remarks>
	/// <include file='doc.xml' path='doc/ExplorerModes/*'/>
	/// </remarks>
	public virtual Explorer? ExploreParent(ExploreParentEventArgs args) { return null; }

	/// <summary>
	/// Returns a new root explorer or null. It must not return itself.
	/// </summary>
	/// <param name="args">.</param>
	/// <remarks>
	/// <include file='doc.xml' path='doc/ExplorerModes/*'/>
	/// </remarks>
	public virtual Explorer? ExploreRoot(ExploreRootEventArgs args) { return null; }

	/// <summary>
	/// Exports the file content to a file or returns it as text.
	/// </summary>
	/// <param name="args">.</param>
	/// <remarks>
	/// <para>
	/// It is normally called by the core on [F3], [F4], [CtrlQ], if the
	/// explorer sets the flag <see cref="CanGetContent"/>. It is also
	/// called in order to copy files to native destinations if the
	/// advanced method <see cref="ExportFiles"/> is not implemented. A
	/// user corrects invalid file system names interactively, if this is
	/// allowed. Otherwise such files are ignored.
	/// </para>
	/// <para>
	/// For export operations, especially batch, consider to use <see
	/// cref="ExportFiles"/>, it gets more overall control and it is more
	/// flexible in case of failures. Besides, content for view and edit
	/// operations does not have to be the same as content for export and
	/// import operations.
	/// </para>
	/// <para>
	/// If the content is settable then this method should set the <see
	/// cref="GetContentEventArgs.CanSet"/>. It is used on editing ([F4]).
	/// If the flag is not set then an editor is opened locked, changes are
	/// not allowed by default.
	/// </para>
	/// <para>
	/// There are three ways of getting file content:
	/// <ol>
	/// <li>Assign a string or a line collection to the <see cref="GetContentEventArgs.UseText"/>.</li>
	/// <li>Copy data to a temporary file with the provided <see cref="GetContentEventArgs.FileName"/>.</li>
	/// <li>If a file represents a system file then assign its path to the <see cref="GetContentEventArgs.UseFileName"/>.</li>
	/// </ol>
	/// </para>
	/// </remarks>
	public virtual void GetContent(GetContentEventArgs args) { if (args != null) args.Result = JobResult.Default; }

	/// <summary>
	/// Sets the file content given the system file.
	/// </summary>
	/// <param name="args">.</param>
	/// <remarks>
	/// If this method is used then <see cref="Functions"/> should contain the <see cref="ExplorerFunctions.SetFile"/> flag.
	/// </remarks>
	public virtual void SetFile(SetFileEventArgs args) { if (args != null) args.Result = JobResult.Default; }

	/// <summary>
	/// Sets the file content given the text string.
	/// </summary>
	/// <param name="args">.</param>
	/// <remarks>
	/// If this method is used then <see cref="Functions"/> should contain the <see cref="ExplorerFunctions.SetText"/> flag.
	/// </remarks>
	public virtual void SetText(SetTextEventArgs args) { if (args != null) args.Result = JobResult.Default; }

	/// <summary>
	/// Accepts module files from another explorer, normally from another module panel.
	/// </summary>
	/// <param name="args">.</param>
	/// <remarks>
	/// Read carefully all about <see cref="AcceptFilesEventArgs"/> and all its members.
	/// <para>
	/// The source explorer can be any explorer of any module panel, not even from this module.
	/// The method should check the source type or type ID and ignore unknown or not supported.
	/// </para>
	/// </remarks>
	public virtual void AcceptFiles(AcceptFilesEventArgs args) { if (args != null) args.Result = JobResult.Ignore; }

	/// <summary>
	/// Deletes the files.
	/// </summary>
	/// <param name="args">.</param>
	public virtual void DeleteFiles(DeleteFilesEventArgs args) { if (args != null) args.Result = JobResult.Ignore; }

	/// <summary>
	/// Exports files to a native destination.
	/// </summary>
	/// <param name="args">.</param>
	/// <remarks>
	/// This method gives some more control than default export performed with <see cref="GetContent"/>.
	/// </remarks>
	public virtual void ExportFiles(ExportFilesEventArgs args) { if (args != null) args.Result = JobResult.Ignore; }

	/// <summary>
	/// Imports files from a native source.
	/// </summary>
	/// <param name="args">.</param>
	public virtual void ImportFiles(ImportFilesEventArgs args) { if (args != null) args.Result = JobResult.Ignore; }

	/// <summary>
	/// Clones the file.
	/// </summary>
	/// <param name="args">.</param>
	/// <remarks>
	/// It is normally called for the current item in a panel on [ShiftF5].
	/// </remarks>
	public virtual void CloneFile(CloneFileEventArgs args) { }

	/// <summary>
	/// Creates a new directory/file.
	/// </summary>
	/// <param name="args">.</param>
	/// <remarks>
	/// It is normally called by the core on [F7] if the explorer has its flag <see cref="CanCreateFile"/> set.
	/// If the explorer creates something then it may also want to set one of the <c>Post*</c>,
	/// so that the specified file, normally just created, is set current by the core.
	/// </remarks>
	public virtual void CreateFile(CreateFileEventArgs args) { if (args != null) args.Result = JobResult.Ignore; }

	/// <summary>
	/// Opens the file.
	/// </summary>
	/// <param name="args">.</param>
	/// <returns>The explorer to be opened in a child panel, or null.</returns>
	/// <remarks>
	/// It is normally called for the current file in a panel on [Enter].
	/// The core does nothing after the call if it returns null.
	/// Otherwise it opens the returned explorer.
	/// </remarks>
	public virtual Explorer? OpenFile(OpenFileEventArgs args) { return null; }

	/// <summary>
	/// Renames the file.
	/// </summary>
	/// <param name="args">.</param>
	/// <remarks>
	/// It is normally called for the current item in a panel on [ShiftF6].
	/// If renaming is done then the core updates and redraws the panel
	/// so that an item with the new name remains current.
	/// If names are not unique then this is not always possible to do correctly.
	/// In this case set one of the <c>Post*</c>.
	/// </remarks>
	public virtual void RenameFile(RenameFileEventArgs args) { }

	/// <summary>
	/// Creates a panel to show the explorer files.
	/// </summary>
	/// <remarks>
	/// The base method creates the default panel.
	/// </remarks>
	public virtual Panel CreatePanel() { return new Panel(this); }

	/// <summary>
	/// Updates the panel when this explorer gets assigned to it.
	/// </summary>
	/// <param name="panel">The panel.</param>
	public virtual void EnterPanel(Panel panel) { }

	/// <summary>
	/// Gets or sets the file comparer.
	/// </summary>
	/// <remarks>
	/// Some core procedures have to be able to compare equality of files provided by explorers. This task is difficult:
	/// file names are not always unique and same files from different calls are not always represented by same objects.
	/// Only file explorers know how to compare their files.
	/// <para>
	/// <see cref="FileNameComparer"/> is used by default, i.e. files are compared by names (<c>OrdinalIgnoreCase</c>).
	/// <see cref="FileFileComparer"/> is used when an explorer caches its files and always returns the same file objects.
	/// <see cref="FileDataComparer"/> is used when an explorer caches its data and always gets the same data attached to files.
	/// <c>FileMetaComparer</c> is used in PowerShell scripts.
	/// Alternatively, an explorer may implement and use any custom file comparer.
	/// </para>
	/// <para>
	/// Use case: a panel shows some frequently changed data like current system processes.
	/// On update it simply recreates the files and attaches just requested process objects.
	/// Thus, the files and data change from call to call and they cannot be used for comparison.
	/// If process names are used as file names then names cannot be used for comparison, too,
	/// because process names are not unique even within the same call.
	/// Solution: a custom comparer that compares process IDs extracted from file data.
	/// </para>
	/// </remarks>
	public IEqualityComparer<FarFile> FileComparer
	{
		get => _FileComparer ??= new FileNameComparer();
		set => _FileComparer = value ?? throw new ArgumentNullException(nameof(value));
	}
	IEqualityComparer<FarFile>? _FileComparer;
}
