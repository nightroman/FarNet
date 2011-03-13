
/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;
using System.Collections.Generic;

namespace FarNet
{
	/// <summary>
	/// Explorer of a virtual file system, file provider and manager, and optionally a panel maker.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Explorers are used for virtual file system navigation and operations on files.
	/// They provide files and other new explorers for that files.
	/// </para>
	/// <para>
	/// Explorers are designed for panels and they normally implement at least one of the panel methods.
	/// But panels are not required for file operations, explorers can be used for pure file management.
	/// Explorers can but do not have to create, configure, and update panels.
	/// The core creates default panels for files when needed.
	/// </para>
	/// <para>
	/// On requests explorers have to create and return new explorers or return null.
	/// They should never return themselves because the core assumes that each explorer
	/// is responsible for its own virtual directory that never change. In other words,
	/// once created an explorer should always return absolutely the same data, of
	/// course, if these data do not change in the virtual file system.
	/// </para>
	/// </remarks>
	public abstract class Explorer
	{
		/// <summary>
		/// New explorer with its type ID.
		/// </summary>
		protected Explorer(Guid typeId) { _TypeId = typeId; }
		/// <summary>
		/// Gets the explorer type ID.
		/// </summary>
		public Guid TypeId { get { return _TypeId; } }
		Guid _TypeId;
		/// <summary>
		/// Gets or sets the location assigned to this explorer.
		/// It is set once on creation.
		/// </summary>
		public string Location
		{
			get { return _Location ?? string.Empty; }
			set
			{
				if (_Location != null) throw new InvalidOperationException("It is set once on creation.");
				_Location = value;
			}
		}
		string _Location;
		/// <summary>
		/// Gets or sets the explorer function flags.
		/// It is normally set on creation and should not change.
		/// </summary>
		public ExplorerFunctions Functions { get; set; }
		/// <summary>
		/// Gets or sets the flag in the <see cref="Functions"/>.
		/// </summary>
		public bool CanExploreLocation
		{
			get { return (Functions & ExplorerFunctions.ExploreLocation) != 0; }
			set { Functions = value ? (Functions | ExplorerFunctions.ExploreLocation) : (Functions & ~ExplorerFunctions.ExploreLocation); }
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
		public bool CanAcceptOther
		{
			get { return (Functions & ExplorerFunctions.AcceptOther) != 0; }
			set { Functions = value ? (Functions | ExplorerFunctions.AcceptOther) : (Functions & ~ExplorerFunctions.AcceptOther); }
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
		public bool CanCreateFile
		{
			get { return (Functions & ExplorerFunctions.CreateFile) != 0; }
			set { Functions = value ? (Functions | ExplorerFunctions.CreateFile) : (Functions & ~ExplorerFunctions.CreateFile); }
		}
		/// <summary>
		/// Gets or sets the flag in the <see cref="Functions"/>.
		/// </summary>
		public bool CanExportFile
		{
			get { return (Functions & ExplorerFunctions.ExportFile) != 0; }
			set { Functions = value ? (Functions | ExplorerFunctions.ExportFile) : (Functions & ~ExplorerFunctions.ExportFile); }
		}
		/// <summary>
		/// Gets or sets the flag in the <see cref="Functions"/>.
		/// </summary>
		public bool CanImportFile
		{
			get { return (Functions & ExplorerFunctions.ImportFile) != 0; }
			set { Functions = value ? (Functions | ExplorerFunctions.ImportFile) : (Functions & ~ExplorerFunctions.ImportFile); }
		}
		/// <summary>
		/// Gets or sets the flag in the <see cref="Functions"/>.
		/// </summary>
		public bool CanImportText
		{
			get { return (Functions & ExplorerFunctions.ImportText) != 0; }
			set { Functions = value ? (Functions | ExplorerFunctions.ImportText) : (Functions & ~ExplorerFunctions.ImportText); }
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
		/// Returns the files.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The method should choose the type of the result list carefully.
		/// The caller does not make a copy, it users the result as it is.
		/// The caller may iterate through the list many times.
		/// The caller assumes that the list is never changed.
		/// If this is not the case then the method has to return a copy.
		/// But it is fine to reuse or update the same list on next calls.
		/// </para>
		/// <include file='doc.xml' path='doc/ExplorerModes/*'/>
		/// </remarks>
		public abstract IList<FarFile> GetFiles(GetFilesEventArgs args);
		/// <summary>
		/// Returns a new directory explorer or null. It must not return itself.
		/// </summary>
		/// <remarks>
		/// It is not called if <see cref="Functions"/> contains the <see cref="ExploreLocation"/> flag.
		/// <para>
		/// It is called when a user enters a directory, on search, and scan.
		/// It should just get another explorer with a new location and do nothing else.
		/// </para>
		/// <include file='doc.xml' path='doc/ExplorerModes/*'/>
		/// </remarks>
		public virtual Explorer ExploreDirectory(ExploreDirectoryEventArgs args) { return null; }
		/// <summary>
		/// Returns a new location explorer or null. It must not return itself.
		/// </summary>
		/// <remarks>
		/// It is called only if <see cref="Functions"/> contains the <see cref="ExploreLocation"/> flag.
		/// <include file='doc.xml' path='doc/ExplorerModes/*'/>
		/// </remarks>
		public virtual Explorer ExploreLocation(ExploreLocationEventArgs args) { return null; }
		/// <summary>
		/// Returns a new parent explorer or null. It must not return itself.
		/// </summary>
		/// <remarks>
		/// <include file='doc.xml' path='doc/ExplorerModes/*'/>
		/// </remarks>
		public virtual Explorer ExploreParent(ExploreParentEventArgs args) { return null; }
		/// <summary>
		/// Returns a new root explorer or null. It must not return itself.
		/// </summary>
		/// <remarks>
		/// <include file='doc.xml' path='doc/ExplorerModes/*'/>
		/// </remarks>
		public virtual Explorer ExploreRoot(ExploreRootEventArgs args) { return null; }
		/// <summary>
		/// Accepts module files from another explorer, normally from another module panel.
		/// </summary>
		/// <remarks>
		/// Read carefully all about <see cref="AcceptFilesEventArgs"/> and all its members.
		/// <para>
		/// The source explorer can be any explorer of any module panel, not even from this module.
		/// The method should check the source type or type ID and ignore unknown or not supported.
		/// </para>
		/// </remarks>
		public virtual void AcceptFiles(AcceptFilesEventArgs args) { if (args != null) args.Result = JobResult.Ignore; }
		/// <summary>
		/// Accepts other files, i.e. files from native and plugin panels, not module panels.
		/// </summary>
		public virtual void AcceptOther(AcceptOtherEventArgs args) { if (args != null) args.Result = JobResult.Ignore; }
		/// <summary>
		/// Deletes the files.
		/// </summary>
		public virtual void DeleteFiles(DeleteFilesEventArgs args) { if (args != null) args.Result = JobResult.Ignore; }
		/// <summary>
		/// Creates a new file or directory.
		/// </summary>
		/// <remarks>
		/// It is normally called by the core on [F7] if the explorer has its flag <see cref="CanCreateFile"/> set.
		/// If the explorer creates something then it may also want to set one of the <c>Post*</c>,
		/// so that the specified file, normally just created, is set current.
		/// </remarks>
		public virtual void CreateFile(CreateFileEventArgs args) { if (args != null) args.Result = JobResult.Ignore; }
		/// <summary>
		/// Exports the file.
		/// </summary>
		/// <remarks>
		/// It is normally called by the core on [F3], [F4], [CtrlQ], and in some cases on [F5]
		/// if the explorer has its flag <see cref="CanExportFile"/> set.
		/// If the file can be imported later back then this method should set <see cref="ExportFileEventArgs.CanImport"/>,
		/// this is used on [F4]: if the flag is not set then the editor will be opened in the locked mode.
		/// <para>
		/// There are three ways to export file data.
		/// <ol>
		/// <li>Assign the text or line collection to the <see cref="ExportFileEventArgs.UseText"/>.</li>
		/// <li>Copy data to the temporary file with the provided <see cref="ExportFileEventArgs.FileName"/>.</li>
		/// <li>If there is an actual system file then its path can be assigned to the <see cref="ExportFileEventArgs.UseFileName"/>.</li>
		/// </ol>
		/// </para>
		/// </remarks>
		public virtual void ExportFile(ExportFileEventArgs args) { if (args != null) args.Result = JobResult.Default; }
		/// <summary>
		/// Updates the file.
		/// </summary>
		/// <remarks>
		/// If this method is implemented then <see cref="Functions"/> should has the <see cref="ExplorerFunctions.ImportFile"/> flag.
		/// <para>
		/// The method can be called "offline".
		/// Example: a file is being saved in the editor after closing its source panel.
		/// If the explorer is not supposed to work then it should be able to ignore the call.
		/// </para>
		/// </remarks>
		public virtual void ImportFile(ImportFileEventArgs args) { if (args != null) args.Result = JobResult.Default; }
		/// <summary>
		/// Updates the text.
		/// </summary>
		/// <remarks>
		/// If this method is implemented then <see cref="Functions"/> should has the <see cref="ExplorerFunctions.ImportText"/> flag.
		/// <para>
		/// The method can be called "offline".
		/// Example: a file is being saved in the editor after closing its source panel.
		/// If the explorer is not supposed to work then it should be able to ignore the call.
		/// </para>
		/// </remarks>
		public virtual void ImportText(ImportTextEventArgs args) { if (args != null) args.Result = JobResult.Default; }
		/// <summary>
		/// Opens the file.
		/// </summary>
		public virtual void OpenFile(OpenFileEventArgs args) { }
		/// <summary>
		/// Creates a panel to show the explorer files.
		/// </summary>
		/// <remarks>
		/// The base method creates a new default panel.
		/// </remarks>
		public virtual Panel CreatePanel() { return new Panel(this); }
		/// <summary>
		/// Updates the panel when this explorer gets assigned to it.
		/// </summary>
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
			get { return _FileComparer ?? (_FileComparer = new FileNameComparer()); }
			set { if (value == null) throw new ArgumentNullException("value"); _FileComparer = value; }
		}
		IEqualityComparer<FarFile> _FileComparer;
		/// <summary>
		/// Opens the explorer in a panel.
		/// </summary>
		public void OpenPanel()
		{
			var panel = CreatePanel();
			panel.Open();
		}
		/// <summary>
		/// Opens the explorer in a panel that is a child of the specified panel.
		/// </summary>
		public void OpenPanelChild(Panel parent)
		{
			var panel = CreatePanel();
			panel.OpenChild(parent);
		}
	}
}
