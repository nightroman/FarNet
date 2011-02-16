
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
		readonly string _Location;
		/// <summary>
		/// New explorer.
		/// </summary>
		/// <param name="location">The assigned location (path, current directory, etc.)</param>
		protected Explorer(string location)
		{
			_Location = location ?? string.Empty;
		}
		/// <summary>
		/// Gets the location assigned to this explorer.
		/// </summary>
		public virtual string Location { get { return _Location; } }
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
		/// <para>
		/// In the <see cref="OperationModes.Find"/> mode the method may be called from a background thread, most of FarNet API is not allowed.
		/// In the <see cref="OperationModes.Silent"/> mode any user interactions including error messages are not allowed.
		/// </para>
		/// </remarks>
		public abstract IList<FarFile> Explore(ExplorerArgs args);
		/// <summary>
		/// Returns a new file explorer or null. It must not return itself.
		/// </summary>
		/// <remarks>
		/// It is called when a user enters a file, on search, and scan.
		/// It should just get another explorer and do nothing else.
		/// </remarks>
		public virtual Explorer ExploreFile(ExploreFileArgs args) { return null; }
		/// <summary>
		/// Returns a new root explorer or null. It must not return itself.
		/// </summary>
		public virtual Explorer ExploreRoot(ExplorerArgs args) { return null; }
		/// <summary>
		/// Returns a new parent explorer or null. It must not return itself.
		/// </summary>
		public virtual Explorer ExploreParent(ExplorerArgs args) { return null; }
		/// <summary>
		/// Exports a file.
		/// </summary>
		public virtual void ExportFile(ExportFileArgs args) { }
		/// <summary>
		/// Gets true if the file can be exported.
		/// </summary>
		/// <remarks>
		/// The base method returns true.
		/// </remarks>
		public virtual bool CanExportFile(FarFile file) { return true; }
		/// <summary>
		/// Imports a file.
		/// </summary>
		/// <remarks>
		/// The method can be called "offline".
		/// Example: a file is being saved in the editor after closing its source panel.
		/// If the explorer is not supposed to work then it should be able to ignore the call.
		/// </remarks>
		public virtual void ImportFile(ImportFileArgs args) { }
		/// <summary>
		/// Gets true if the file can be imported.
		/// </summary>
		/// <remarks>
		/// The base method returns true.
		/// </remarks>
		public virtual bool CanImportFile(FarFile file) { return true; }
		/// <summary>
		/// Deletes files.
		/// </summary>
		public virtual void DeleteFiles(DeleteFilesArgs args) { }
		/// <summary>
		/// Gets true if files can be deleted.
		/// </summary>
		/// <remarks>
		/// The base method returns true.
		/// </remarks>
		public virtual bool CanDeleteFiles(DeleteFilesArgs args) { return true; }
		/// <summary>
		/// Creates a panel or returns the current to reuse or returns null to use a default panel.
		/// </summary>
		/// <remarks>
		/// The argument <see cref="PanelMakerArgs.Panel"/> is the current panel.
		/// It can be returned in order to reuse it instead of opening a new child panel.
		/// </remarks>
		public virtual Panel MakePanel(PanelMakerArgs args) { return null; }
		/// <summary>
		/// Setups the panel or does nothing is this is done on creation.
		/// </summary>
		public virtual void SetupPanel(PanelMakerArgs args) { }
		/// <summary>
		/// Updates the panel, e.g. when the explorer has changed.
		/// </summary>
		public virtual void UpdatePanel(PanelMakerArgs args) { }
	}
}
