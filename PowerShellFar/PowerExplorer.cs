
/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
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
		public virtual IList<FarFile> DoGetFiles(ExplorerEventArgs args) { return Cache; }
		/// <summary>
		/// <see cref="Explorer.GetFiles"/> worker.
		/// </summary>
		/// <remarks>
		/// Script variables: <c>$this</c> is this explorer, <c>$_</c> is <see cref="ExplorerEventArgs"/>.
		/// <para>
		/// The script outputs files or nothing. In the latter case the predefined <see cref="Cache"/> list is used.
		/// </para>
		/// </remarks>
		public ScriptBlock AsGetFiles { get; set; }
		/// <summary>
		/// Calls As-Script or Do-Method.
		/// </summary>
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
		/// Script variables: <c>$this</c> is this explorer, <c>$_</c> is <see cref="ExploreDirectoryEventArgs"/>.
		/// </remarks>
		public ScriptBlock AsExploreDirectory { get; set; }
		/// <summary>
		/// Calls As-Script or Do-Method.
		/// </summary>
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
		/// Script variables: <c>$this</c> is this explorer, <c>$_</c> is <see cref="ExploreLocationEventArgs"/>.
		/// </remarks>
		public ScriptBlock AsExploreLocation { get; set; }
		/// <summary>
		/// Calls As-Script or Do-Method.
		/// </summary>
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
		/// Script variables: <c>$this</c> is this explorer, <c>$_</c> is <see cref="ExplorerEventArgs"/>.
		/// </remarks>
		public ScriptBlock AsExploreParent { get; set; }
		/// <summary>
		/// Calls As-Script or Do-Method.
		/// </summary>
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
		/// Script variables: <c>$this</c> is this explorer, <c>$_</c> is <see cref="ExplorerEventArgs"/>.
		/// </remarks>
		public ScriptBlock AsExploreRoot { get; set; }
		/// <summary>
		/// Calls As-Script or Do-Method.
		/// </summary>
		public sealed override Explorer ExploreRoot(ExploreRootEventArgs args)
		{
			if (AsExploreRoot == null)
				return DoExploreRoot(args);
			else
				return InvokeExplorerScript(AsExploreRoot, args);
		}
		/// <summary>
		/// <see cref="Explorer.CreateFile"/> worker.
		/// </summary>
		public virtual void DoCreateFile(CreateFileEventArgs args) { base.CreateFile(args); }
		/// <summary>
		/// <see cref="Explorer.CreateFile"/> worker.
		/// </summary>
		/// <remarks>
		/// Script variables: <c>$this</c> is this explorer, <c>$_</c> is <see cref="CreateFileEventArgs"/>.
		/// </remarks>
		public ScriptBlock AsCreateFile { get; set; }
		/// <summary>
		/// Calls As-Script or Do-Method.
		/// </summary>
		public sealed override void CreateFile(CreateFileEventArgs args)
		{
			if (AsCreateFile == null)
				DoCreateFile(args);
			else
				A.InvokeScriptReturnAsIs(AsCreateFile, this, args);
		}
		/// <summary>
		/// <see cref="Explorer.ExportFile"/> worker.
		/// </summary>
		public virtual void DoExportFile(ExportFileEventArgs args) { base.ExportFile(args); }
		/// <summary>
		/// <see cref="Explorer.ExportFile"/> worker.
		/// </summary>
		/// <remarks>
		/// Script variables: <c>$this</c> is this explorer, <c>$_</c> is <see cref="ExportFileEventArgs"/>.
		/// </remarks>
		public ScriptBlock AsExportFile { get; set; }
		/// <summary>
		/// Calls As-Script or Do-Method.
		/// </summary>
		public sealed override void ExportFile(ExportFileEventArgs args)
		{
			if (AsExportFile == null)
				DoExportFile(args);
			else
				A.InvokeScriptReturnAsIs(AsExportFile, this, args);
		}
		/// <summary>
		/// <see cref="Explorer.ImportFile"/> worker.
		/// </summary>
		public virtual void DoImportFile(ImportFileEventArgs args) { base.ImportFile(args); }
		/// <summary>
		/// <see cref="Explorer.ImportFile"/> worker.
		/// </summary>
		/// <remarks>
		/// Script variables: <c>$this</c> is this explorer, <c>$_</c> is <see cref="ImportFileEventArgs"/>.
		/// </remarks>
		public ScriptBlock AsImportFile { get; set; }
		/// <summary>
		/// Calls As-Script or Do-Method.
		/// </summary>
		public sealed override void ImportFile(ImportFileEventArgs args)
		{
			if (AsImportFile == null)
				DoImportFile(args);
			else
				A.InvokeScriptReturnAsIs(AsImportFile, this, args);
		}
		/// <summary>
		/// <see cref="Explorer.ImportText"/> worker.
		/// </summary>
		public virtual void DoImportText(ImportTextEventArgs args) { base.ImportText(args); }
		/// <summary>
		/// <see cref="Explorer.ImportText"/> worker.
		/// </summary>
		/// <remarks>
		/// Script variables: <c>$this</c> is this explorer, <c>$_</c> is <see cref="ImportTextEventArgs"/>.
		/// </remarks>
		public ScriptBlock AsImportText { get; set; }
		/// <summary>
		/// Calls As-Script or Do-Method.
		/// </summary>
		public sealed override void ImportText(ImportTextEventArgs args)
		{
			if (AsImportText == null)
				DoImportText(args);
			else
				A.InvokeScriptReturnAsIs(AsImportText, this, args);
		}
		/// <summary>
		/// <see cref="Explorer.OpenFile"/> worker.
		/// </summary>
		public virtual void DoOpenFile(OpenFileEventArgs args) { base.OpenFile(args); }
		/// <summary>
		/// <see cref="Explorer.OpenFile"/> worker.
		/// </summary>
		/// <remarks>
		/// Script variables: <c>$this</c> is this explorer, <c>$_</c> is <see cref="OpenFileEventArgs"/>.
		/// </remarks>
		public ScriptBlock AsOpenFile { get; set; }
		/// <summary>
		/// Calls As-Script or Do-Method.
		/// </summary>
		public sealed override void OpenFile(OpenFileEventArgs args)
		{
			if (AsOpenFile == null)
				DoOpenFile(args);
			else
				A.InvokeScriptReturnAsIs(AsOpenFile, this, args);
		}
		/// <summary>
		/// <see cref="Explorer.AcceptFiles"/> worker.
		/// </summary>
		public virtual void DoAcceptFiles(AcceptFilesEventArgs args) { base.AcceptFiles(args); }
		/// <summary>
		/// <see cref="Explorer.AcceptFiles"/> worker.
		/// </summary>
		/// <remarks>
		/// Script variables: <c>$this</c> is this explorer, <c>$_</c> is <see cref="AcceptFilesEventArgs"/>.
		/// </remarks>
		public ScriptBlock AsAcceptFiles { get; set; }
		/// <summary>
		/// Calls As-Script or Do-Method.
		/// </summary>
		public sealed override void AcceptFiles(AcceptFilesEventArgs args)
		{
			if (AsAcceptFiles == null)
				DoAcceptFiles(args);
			else
				A.InvokeScriptReturnAsIs(AsAcceptFiles, this, args);
		}
		/// <summary>
		/// <see cref="Explorer.AcceptOther"/> worker.
		/// </summary>
		public virtual void DoAcceptOther(AcceptOtherEventArgs args) { base.AcceptOther(args); }
		/// <summary>
		/// <see cref="Explorer.AcceptOther"/> worker.
		/// </summary>
		/// <remarks>
		/// Script variables: <c>$this</c> is this explorer, <c>$_</c> is <see cref="AcceptOtherEventArgs"/>.
		/// </remarks>
		public ScriptBlock AsAcceptOther { get; set; }
		/// <summary>
		/// Calls As-Script or Do-Method.
		/// </summary>
		public sealed override void AcceptOther(AcceptOtherEventArgs args)
		{
			if (AsAcceptOther == null)
				DoAcceptOther(args);
			else
				A.InvokeScriptReturnAsIs(AsAcceptOther, this, args);
		}
		/// <summary>
		/// <see cref="Explorer.DeleteFiles"/> worker.
		/// </summary>
		public virtual void DoDeleteFiles(DeleteFilesEventArgs args) { base.DeleteFiles(args); }
		/// <summary>
		/// <see cref="Explorer.DeleteFiles"/> worker.
		/// </summary>
		/// <remarks>
		/// Script variables: <c>$this</c> is this explorer, <c>$_</c> is <see cref="DeleteFilesEventArgs"/>.
		/// </remarks>
		public ScriptBlock AsDeleteFiles { get; set; }
		/// <summary>
		/// Calls As-Script or Do-Method.
		/// </summary>
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
		/// Script variables: <c>$this</c> is this explorer.
		/// </remarks>
		public ScriptBlock AsCreatePanel { get; set; }
		/// <summary>
		/// Calls As-Script or Do-Method.
		/// </summary>
		public sealed override Panel CreatePanel()
		{
			if (AsCreatePanel == null)
				return DoCreatePanel();
			else
				return (Panel)LanguagePrimitives.ConvertTo(A.InvokeScriptReturnAsIs(AsCreatePanel, this, null), typeof(Panel), null);
		}
		/// <summary>
		/// <see cref="Explorer.EnterPanel"/> worker.
		/// </summary>
		public virtual void DoEnterPanel(Panel panel) { base.EnterPanel(panel); }
		/// <summary>
		/// <see cref="Explorer.EnterPanel"/> worker.
		/// </summary>
		/// <remarks>
		/// Script variables: <c>$this</c> is this explorer, <c>$_</c> is the <see cref="Panel"/> to be updated.
		/// </remarks>
		public ScriptBlock AsEnterPanel { get; set; }
		/// <summary>
		/// Calls As-Script or Do-Method.
		/// </summary>
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
