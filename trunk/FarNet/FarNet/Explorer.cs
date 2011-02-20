
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
		/// It is set on creation and should not change.
		/// </summary>
		public ExplorerFunctions Function { get; set; }
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
		public abstract IList<FarFile> Explore(ExplorerArgs args);
		/// <summary>
		/// Returns a new directory explorer or null. It must not return itself.
		/// </summary>
		/// <remarks>
		/// It is not called if <see cref="Function"/> contains the <see cref="ExploreLocation"/> flag.
		/// <para>
		/// It is called when a user enters a directory, on search, and scan.
		/// It should just get another explorer with a new location and do nothing else.
		/// </para>
		/// <include file='doc.xml' path='doc/ExplorerModes/*'/>
		/// </remarks>
		public virtual Explorer ExploreDirectory(ExploreDirectoryArgs args) { return null; }
		/// <summary>
		/// Returns a new location explorer or null. It must not return itself.
		/// </summary>
		/// <remarks>
		/// It is called only if <see cref="Function"/> contains the <see cref="ExploreLocation"/> flag.
		/// <include file='doc.xml' path='doc/ExplorerModes/*'/>
		/// </remarks>
		public virtual Explorer ExploreLocation(ExploreLocationArgs args) { return null; }
		/// <summary>
		/// Returns a new parent explorer or null. It must not return itself.
		/// </summary>
		/// <remarks>
		/// <include file='doc.xml' path='doc/ExplorerModes/*'/>
		/// </remarks>
		public virtual Explorer ExploreParent(ExplorerArgs args) { return null; }
		/// <summary>
		/// Returns a new root explorer or null. It must not return itself.
		/// </summary>
		/// <remarks>
		/// <include file='doc.xml' path='doc/ExplorerModes/*'/>
		/// </remarks>
		public virtual Explorer ExploreRoot(ExplorerArgs args) { return null; }
		/// <summary>
		/// Exports the file.
		/// </summary>
		/// <remarks>
		/// If the file can be also imported later back then set <see cref="ExportFileArgs.CanImport"/>.
		/// </remarks>
		public virtual void ExportFile(ExportFileArgs args) { }
		/// <summary>
		/// Imports the file.
		/// </summary>
		/// <remarks>
		/// The method can be called "offline".
		/// Example: a file is being saved in the editor after closing its source panel.
		/// If the explorer is not supposed to work then it should be able to ignore the call.
		/// </remarks>
		public virtual void ImportFile(ImportFileArgs args) { }
		/// <summary>
		/// Deletes the files.
		/// </summary>
		public virtual void DeleteFiles(DeleteFilesArgs args) { }
		/// <summary>
		/// Creates a custom panel or returns null to allow the core to create a default panel.
		/// </summary>
		/// <remarks>
		/// This method should be implemented in two cases:
		/// 1) the explorer uses a custom panel, e.g. a subclass of the base panel;
		/// 2) on navigation the explorer wants to reuse the same panel for all explorers of the same type,
		/// then it creates a panel and sets its <see cref="Panel.ExplorerTypeId"/> to its own <see cref="TypeId"/>.
		/// </remarks>
		public virtual Panel CreatePanel() { return null; }
		/// <summary>
		/// Updates the panel, e.g. when the explorer has changed.
		/// </summary>
		public virtual void UpdatePanel(Panel panel) { }
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
		/// <see cref="FileMetaComparer"/> covers other cases with a helper delegate (in particular it is useful in PowerShell scripts).
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
			set
			{
				if (value == null) throw new ArgumentNullException("value");
				_FileComparer = value;
			}
		}
		IEqualityComparer<FarFile> _FileComparer;
	}
}
