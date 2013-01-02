
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2013 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using FarNet;

namespace PowerShellFar
{
	/// <summary>
	/// Explorer designed for PowerShell scripts.
	/// </summary>
	public class PowerExplorer : Explorer
	{
		/// <summary>
		/// New explorer with its type ID.
		/// </summary>
		public PowerExplorer(Guid typeId) : base(typeId) { }
		/// <summary>
		/// Gets the collection ready to use as the file cache, if needed.
		/// </summary>
		public IList<FarFile> Cache { get { return _Cache; } internal set { _Cache = value ?? new List<FarFile>(); } }
		IList<FarFile> _Cache = new List<FarFile>();
		/// <summary>
		/// Gets or sets the user data object.
		/// </summary>
		/// <remarks>
		/// Normally it should be set on creation to describe the assigned explorer location,
		/// so that other explorer methods can use this information. There is no much sense
		/// to change these data later (note: each explorer deals with one fixed location).
		/// But it is fine to cache files in here and refresh them when needed.
		/// </remarks>
		public PSObject Data { get; set; }
		/// <summary>
		/// <see cref="Explorer.GetFiles"/> worker.
		/// </summary>
		public virtual IList<FarFile> DoGetFiles(GetFilesEventArgs args) { return Cache; }
		/// <summary>
		/// <see cref="Explorer.GetFiles"/> worker.
		/// </summary>
		/// <remarks>
		/// Arguments: 0: this explorer, 1: <see cref="ExplorerEventArgs"/>.
		/// <para>
		/// The script outputs files or nothing. In the latter case the predefined <see cref="Cache"/> list is used.
		/// </para>
		/// </remarks>
		public ScriptBlock AsGetFiles { get; set; }
		/// <include file='doc.xml' path='doc/ScriptFork/*'/>
		public sealed override IList<FarFile> GetFiles(GetFilesEventArgs args)
		{
			if (AsGetFiles == null)
				return DoGetFiles(args);

			// nothing, use the predefined file list
			var output = A.InvokeScript(AsGetFiles, this, args);
			if (output.Count == 0)
				return Cache;

			// convert the output to files
			var result = new List<FarFile>();
			foreach (var it in output)
			{
				FarFile file = (FarFile)LanguagePrimitives.ConvertTo(it, typeof(FarFile), null);
				if (file != null)
					result.Add(file);
			}

			return result;
		}
		/// <summary>
		/// <see cref="Explorer.ExploreDirectory"/> worker.
		/// </summary>
		public virtual Explorer DoExploreDirectory(ExploreDirectoryEventArgs args) { return null; }
		/// <summary>
		/// <see cref="Explorer.ExploreDirectory"/> worker.
		/// </summary>
		/// <remarks>
		/// Arguments: 0: this explorer, 1: <see cref="ExploreDirectoryEventArgs"/>.
		/// </remarks>
		public ScriptBlock AsExploreDirectory { get; set; }
		/// <include file='doc.xml' path='doc/ScriptFork/*'/>
		public sealed override Explorer ExploreDirectory(ExploreDirectoryEventArgs args)
		{
			if (AsExploreDirectory == null)
				return DoExploreDirectory(args);
			else
				return InvokeExplorerScript(AsExploreDirectory, args);
		}
		/// <summary>
		/// <see cref="Explorer.ExploreLocation"/> worker.
		/// </summary>
		public virtual Explorer DoExploreLocation(ExploreLocationEventArgs args) { return null; }
		/// <summary>
		/// <see cref="Explorer.ExploreLocation"/> worker.
		/// </summary>
		/// <remarks>
		/// Arguments: 0: this explorer, 1: <see cref="ExploreLocationEventArgs"/>.
		/// </remarks>
		public ScriptBlock AsExploreLocation { get; set; }
		/// <include file='doc.xml' path='doc/ScriptFork/*'/>
		public sealed override Explorer ExploreLocation(ExploreLocationEventArgs args)
		{
			if (AsExploreLocation == null)
				return DoExploreLocation(args);
			else
				return InvokeExplorerScript(AsExploreLocation, args);
		}
		/// <summary>
		/// <see cref="Explorer.ExploreParent"/> worker.
		/// </summary>
		public virtual Explorer DoExploreParent(ExploreParentEventArgs args) { return null; }
		/// <summary>
		/// <see cref="Explorer.ExploreParent"/> worker.
		/// </summary>
		/// <remarks>
		/// Arguments: 0: this explorer, 1: <see cref="ExplorerEventArgs"/>.
		/// </remarks>
		public ScriptBlock AsExploreParent { get; set; }
		/// <include file='doc.xml' path='doc/ScriptFork/*'/>
		public sealed override Explorer ExploreParent(ExploreParentEventArgs args)
		{
			if (AsExploreParent == null)
				return DoExploreParent(args);
			else
				return InvokeExplorerScript(AsExploreParent, args);
		}
		/// <summary>
		/// <see cref="Explorer.ExploreRoot"/> worker.
		/// </summary>
		public virtual Explorer DoExploreRoot(ExploreRootEventArgs args) { return null; }
		/// <summary>
		/// <see cref="Explorer.ExploreRoot"/> worker.
		/// </summary>
		/// <remarks>
		/// Arguments: 0: this explorer, 1: <see cref="ExplorerEventArgs"/>.
		/// </remarks>
		public ScriptBlock AsExploreRoot { get; set; }
		/// <include file='doc.xml' path='doc/ScriptFork/*'/>
		public sealed override Explorer ExploreRoot(ExploreRootEventArgs args)
		{
			if (AsExploreRoot == null)
				return DoExploreRoot(args);
			else
				return InvokeExplorerScript(AsExploreRoot, args);
		}
		/// <summary>
		/// <see cref="Explorer.GetContent"/> worker.
		/// </summary>
		public virtual void DoGetContent(GetContentEventArgs args) { base.GetContent(args); }
		/// <summary>
		/// <see cref="Explorer.GetContent"/> worker.
		/// </summary>
		/// <remarks>
		/// Arguments: 0: this explorer, 1: <see cref="GetContentEventArgs"/>.
		/// </remarks>
		public ScriptBlock AsGetContent { get; set; }
		/// <include file='doc.xml' path='doc/ScriptFork/*'/>
		public sealed override void GetContent(GetContentEventArgs args)
		{
			if (AsGetContent == null)
				DoGetContent(args);
			else
				A.InvokeScriptReturnAsIs(AsGetContent, this, args);
		}
		/// <summary>
		/// <see cref="Explorer.SetFile"/> worker.
		/// </summary>
		public virtual void DoSetFile(SetFileEventArgs args) { base.SetFile(args); }
		/// <summary>
		/// <see cref="Explorer.SetFile"/> worker.
		/// </summary>
		/// <remarks>
		/// Arguments: 0: this explorer, 1: <see cref="SetFileEventArgs"/>.
		/// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "AsSet")]
		public ScriptBlock AsSetFile { get; set; }
		/// <include file='doc.xml' path='doc/ScriptFork/*'/>
		public sealed override void SetFile(SetFileEventArgs args)
		{
			if (AsSetFile == null)
				DoSetFile(args);
			else
				A.InvokeScriptReturnAsIs(AsSetFile, this, args);
		}
		/// <summary>
		/// <see cref="Explorer.SetText"/> worker.
		/// </summary>
		public virtual void DoSetText(SetTextEventArgs args) { base.SetText(args); }
		/// <summary>
		/// <see cref="Explorer.SetText"/> worker.
		/// </summary>
		/// <remarks>
		/// Arguments: 0: this explorer, 1: <see cref="SetTextEventArgs"/>.
		/// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "AsSet")]
		public ScriptBlock AsSetText { get; set; }
		/// <include file='doc.xml' path='doc/ScriptFork/*'/>
		public sealed override void SetText(SetTextEventArgs args)
		{
			if (AsSetText == null)
				DoSetText(args);
			else
				A.InvokeScriptReturnAsIs(AsSetText, this, args);
		}
		/// <summary>
		/// <see cref="Explorer.CloneFile"/> worker.
		/// </summary>
		public virtual void DoCloneFile(CloneFileEventArgs args) { base.CloneFile(args); }
		/// <summary>
		/// <see cref="Explorer.CloneFile"/> worker.
		/// </summary>
		/// <remarks>
		/// Arguments: 0: this explorer, 1: <see cref="CloneFileEventArgs"/>.
		/// </remarks>
		public ScriptBlock AsCloneFile { get; set; }
		/// <include file='doc.xml' path='doc/ScriptFork/*'/>
		public sealed override void CloneFile(CloneFileEventArgs args)
		{
			if (AsCloneFile == null)
				DoCloneFile(args);
			else
				A.InvokeScriptReturnAsIs(AsCloneFile, this, args);
		}
		/// <summary>
		/// <see cref="Explorer.CreateFile"/> worker.
		/// </summary>
		public virtual void DoCreateFile(CreateFileEventArgs args) { base.CreateFile(args); }
		/// <summary>
		/// <see cref="Explorer.CreateFile"/> worker.
		/// </summary>
		/// <remarks>
		/// Arguments: 0: this explorer, 1: <see cref="CreateFileEventArgs"/>.
		/// </remarks>
		public ScriptBlock AsCreateFile { get; set; }
		/// <include file='doc.xml' path='doc/ScriptFork/*'/>
		public sealed override void CreateFile(CreateFileEventArgs args)
		{
			if (AsCreateFile == null)
				DoCreateFile(args);
			else
				A.InvokeScriptReturnAsIs(AsCreateFile, this, args);
		}
		/// <summary>
		/// <see cref="Explorer.OpenFile"/> worker.
		/// </summary>
		public virtual Explorer DoOpenFile(OpenFileEventArgs args) { return base.OpenFile(args); }
		/// <summary>
		/// <see cref="Explorer.OpenFile"/> worker.
		/// </summary>
		/// <remarks>
		/// Arguments: 0: this explorer, 1: <see cref="OpenFileEventArgs"/>.
		/// </remarks>
		public ScriptBlock AsOpenFile { get; set; }
		/// <include file='doc.xml' path='doc/ScriptFork/*'/>
		public sealed override Explorer OpenFile(OpenFileEventArgs args)
		{
			if (AsOpenFile == null)
				return DoOpenFile(args);
			else
				return InvokeExplorerScript(AsOpenFile, args);
		}
		/// <summary>
		/// <see cref="Explorer.RenameFile"/> worker.
		/// </summary>
		public virtual void DoRenameFile(RenameFileEventArgs args) { base.RenameFile(args); }
		/// <summary>
		/// <see cref="Explorer.RenameFile"/> worker.
		/// </summary>
		/// <remarks>
		/// Arguments: 0: this explorer, 1: <see cref="RenameFileEventArgs"/>.
		/// </remarks>
		public ScriptBlock AsRenameFile { get; set; }
		/// <include file='doc.xml' path='doc/ScriptFork/*'/>
		public sealed override void RenameFile(RenameFileEventArgs args)
		{
			if (AsRenameFile == null)
				DoRenameFile(args);
			else
				A.InvokeScriptReturnAsIs(AsRenameFile, this, args);
		}
		/// <summary>
		/// <see cref="Explorer.AcceptFiles"/> worker.
		/// </summary>
		public virtual void DoAcceptFiles(AcceptFilesEventArgs args) { base.AcceptFiles(args); }
		/// <summary>
		/// <see cref="Explorer.AcceptFiles"/> worker.
		/// </summary>
		/// <remarks>
		/// Arguments: 0: this explorer, 1: <see cref="AcceptFilesEventArgs"/>.
		/// </remarks>
		public ScriptBlock AsAcceptFiles { get; set; }
		/// <include file='doc.xml' path='doc/ScriptFork/*'/>
		public sealed override void AcceptFiles(AcceptFilesEventArgs args)
		{
			if (AsAcceptFiles == null)
				DoAcceptFiles(args);
			else
				A.InvokeScriptReturnAsIs(AsAcceptFiles, this, args);
		}
		/// <summary>
		/// <see cref="Explorer.ImportFiles"/> worker.
		/// </summary>
		public virtual void DoImportFiles(ImportFilesEventArgs args) { base.ImportFiles(args); }
		/// <summary>
		/// <see cref="Explorer.ImportFiles"/> worker.
		/// </summary>
		/// <remarks>
		/// Arguments: 0: this explorer, 1: <see cref="ImportFilesEventArgs"/>.
		/// </remarks>
		public ScriptBlock AsImportFiles { get; set; }
		/// <include file='doc.xml' path='doc/ScriptFork/*'/>
		public sealed override void ImportFiles(ImportFilesEventArgs args)
		{
			if (AsImportFiles == null)
				DoImportFiles(args);
			else
				A.InvokeScriptReturnAsIs(AsImportFiles, this, args);
		}
		/// <summary>
		/// <see cref="Explorer.ExportFiles"/> worker.
		/// </summary>
		public virtual void DoExportFiles(ExportFilesEventArgs args) { base.ExportFiles(args); }
		/// <summary>
		/// <see cref="Explorer.ExportFiles"/> worker.
		/// </summary>
		/// <remarks>
		/// Arguments: 0: this explorer, 1: <see cref="ExportFilesEventArgs"/>.
		/// </remarks>
		public ScriptBlock AsExportFiles { get; set; }
		/// <include file='doc.xml' path='doc/ScriptFork/*'/>
		public sealed override void ExportFiles(ExportFilesEventArgs args)
		{
			if (AsExportFiles == null)
				DoExportFiles(args);
			else
				A.InvokeScriptReturnAsIs(AsExportFiles, this, args);
		}
		/// <summary>
		/// <see cref="Explorer.DeleteFiles"/> worker.
		/// </summary>
		public virtual void DoDeleteFiles(DeleteFilesEventArgs args) { base.DeleteFiles(args); }
		/// <summary>
		/// <see cref="Explorer.DeleteFiles"/> worker.
		/// </summary>
		/// <remarks>
		/// Arguments: 0: this explorer, 1: <see cref="DeleteFilesEventArgs"/>.
		/// </remarks>
		public ScriptBlock AsDeleteFiles { get; set; }
		/// <include file='doc.xml' path='doc/ScriptFork/*'/>
		public sealed override void DeleteFiles(DeleteFilesEventArgs args)
		{
			if (AsDeleteFiles == null)
				DoDeleteFiles(args);
			else
				A.InvokeScriptReturnAsIs(AsDeleteFiles, this, args);
		}
		/// <summary>
		/// <see cref="Explorer.CreatePanel"/> worker.
		/// </summary>
		public virtual Panel DoCreatePanel() { return base.CreatePanel(); }
		/// <summary>
		/// <see cref="Explorer.CreatePanel"/> worker.
		/// </summary>
		/// <remarks>
		/// Arguments: 0: this explorer.
		/// </remarks>
		public ScriptBlock AsCreatePanel { get; set; }
		/// <include file='doc.xml' path='doc/ScriptFork/*'/>
		public sealed override Panel CreatePanel()
		{
			if (AsCreatePanel == null)
				return DoCreatePanel();
			else
				return (Panel)LanguagePrimitives.ConvertTo(A.InvokeScriptReturnAsIs(AsCreatePanel, this), typeof(Panel), null);
		}
		/// <summary>
		/// <see cref="Explorer.EnterPanel"/> worker.
		/// </summary>
		public virtual void DoEnterPanel(Panel panel) { base.EnterPanel(panel); }
		/// <summary>
		/// <see cref="Explorer.EnterPanel"/> worker.
		/// </summary>
		/// <remarks>
		/// Arguments: 0: this explorer, 1: <see cref="Panel"/> to be updated.
		/// </remarks>
		public ScriptBlock AsEnterPanel { get; set; }
		/// <include file='doc.xml' path='doc/ScriptFork/*'/>
		public sealed override void EnterPanel(Panel panel)
		{
			if (AsEnterPanel == null)
				DoEnterPanel(panel);
			else
				A.InvokeScriptReturnAsIs(AsEnterPanel, this, panel);
		}
		internal Explorer InvokeExplorerScript(ScriptBlock script, ExplorerEventArgs args)
		{
			var data = A.InvokeScript(script, this, args);
			if (data.Count == 0)
				return null;

			return (Explorer)LanguagePrimitives.ConvertTo(data[0], typeof(Explorer), null);
		}
	}
}
