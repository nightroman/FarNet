
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
	/// Scripted explorer of a virtual file system directory.
	/// </summary>
	public sealed class PowerExplorer : Explorer
	{
		/// <summary>
		/// New explorer with its type ID.
		/// </summary>
		public PowerExplorer(Guid typeId) : base(typeId) { }
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
		/// <see cref="Explorer.Explore"/> worker. It must be set in a script.
		/// </summary>
		/// <remarks>
		/// Script variables: <c>$this</c> is this explorer, <c>$_</c> is <see cref="ExplorerArgs"/>.
		/// </remarks>
		public ScriptBlock AsExplore { get; set; }
		/// <summary>
		/// Calls <see cref="AsExplore"/>.
		/// </summary>
		public override IList<FarFile> Explore(ExplorerArgs args)
		{
			if (AsExplore == null)
				throw new InvalidOperationException("Explore script is not set.");

			if (Runspace.DefaultRunspace == null)
				Runspace.DefaultRunspace = A.Psf.Runspace;
			
			var result = new List<FarFile>();
			foreach (var it in A.InvokeScript(AsExplore, this, args))
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
		/// <remarks>
		/// Script variables: <c>$this</c> is this explorer, <c>$_</c> is <see cref="ExploreDirectoryArgs"/>.
		/// </remarks>
		public ScriptBlock AsExploreDirectory { get; set; }
		/// <summary>
		/// Calls <see cref="AsExploreDirectory"/>.
		/// </summary>
		public override Explorer ExploreDirectory(ExploreDirectoryArgs args)
		{
			return InvokeExplorerScript(AsExploreDirectory, args);
		}
		/// <summary>
		/// <see cref="Explorer.ExploreLocation"/> worker.
		/// </summary>
		/// <remarks>
		/// Script variables: <c>$this</c> is this explorer, <c>$_</c> is <see cref="ExploreLocationArgs"/>.
		/// </remarks>
		public ScriptBlock AsExploreLocation { get; set; }
		/// <summary>
		/// Calls <see cref="AsExploreLocation"/>.
		/// </summary>
		public override Explorer ExploreLocation(ExploreLocationArgs args)
		{
			return InvokeExplorerScript(AsExploreLocation, args);
		}
		/// <summary>
		/// <see cref="Explorer.ExploreParent"/> worker.
		/// </summary>
		/// <remarks>
		/// Script variables: <c>$this</c> is this explorer, <c>$_</c> is <see cref="ExplorerArgs"/>.
		/// </remarks>
		public ScriptBlock AsExploreParent { get; set; }
		/// <summary>
		/// Calls <see cref="AsExploreParent"/>.
		/// </summary>
		public override Explorer ExploreParent(ExplorerArgs args)
		{
			return InvokeExplorerScript(AsExploreParent, args);
		}
		/// <summary>
		/// <see cref="Explorer.ExploreRoot"/> worker.
		/// </summary>
		/// <remarks>
		/// Script variables: <c>$this</c> is this explorer, <c>$_</c> is <see cref="ExplorerArgs"/>.
		/// </remarks>
		public ScriptBlock AsExploreRoot { get; set; }
		/// <summary>
		/// Calls <see cref="AsExploreRoot"/>.
		/// </summary>
		public override Explorer ExploreRoot(ExplorerArgs args)
		{
			return InvokeExplorerScript(AsExploreRoot, args);
		}
		/// <summary>
		/// <see cref="Explorer.ExportFile"/> worker.
		/// </summary>
		/// <remarks>
		/// Script variables: <c>$this</c> is this explorer, <c>$_</c> is <see cref="ExportFileArgs"/>.
		/// </remarks>
		public ScriptBlock AsExportFile { get; set; }
		/// <summary>
		/// Calls <see cref="AsExportFile"/>.
		/// </summary>
		public override void ExportFile(ExportFileArgs args)
		{
			if (args == null) throw new ArgumentNullException("args");
			if (AsExportFile == null)
				args.Result = JobResult.Default;
			else
				A.InvokeScriptReturnAsIs(AsExportFile, this, args);
		}
		/// <summary>
		/// <see cref="Explorer.ImportFile"/> worker.
		/// </summary>
		/// <remarks>
		/// Script variables: <c>$this</c> is this explorer, <c>$_</c> is <see cref="ImportFileArgs"/>.
		/// </remarks>
		public ScriptBlock AsImportFile { get; set; }
		/// <summary>
		/// Calls <see cref="AsImportFile"/>.
		/// </summary>
		public override void ImportFile(ImportFileArgs args)
		{
			if (args == null) throw new ArgumentNullException("args");
			if (AsImportFile == null)
				args.Result = JobResult.Default;
			else
				A.InvokeScriptReturnAsIs(AsImportFile, this, args);
		}
		/// <summary>
		/// <see cref="Explorer.DeleteFiles"/> worker.
		/// </summary>
		/// <remarks>
		/// Script variables: <c>$this</c> is this explorer, <c>$_</c> is <see cref="DeleteFilesArgs"/>.
		/// </remarks>
		public ScriptBlock AsDeleteFiles { get; set; }
		/// <summary>
		/// Calls <see cref="AsDeleteFiles"/>.
		/// </summary>
		public override void DeleteFiles(DeleteFilesArgs args)
		{
			if (AsDeleteFiles != null)
				A.InvokeScriptReturnAsIs(AsDeleteFiles, this, args);
		}
		/// <summary>
		/// <see cref="Explorer.CreatePanel"/> worker.
		/// </summary>
		/// <remarks>
		/// Script variables: <c>$this</c> is this explorer.
		/// </remarks>
		public ScriptBlock AsCreatePanel { get; set; }
		/// <summary>
		/// Calls <see cref="AsCreatePanel"/>.
		/// </summary>
		public override Panel CreatePanel()
		{
			if (AsCreatePanel == null)
				return null;

			var data = A.InvokeScript(AsCreatePanel, this, null);
			if (data.Count == 0)
				return null;

			return (Panel)LanguagePrimitives.ConvertTo(data[0], typeof(Panel), null);
		}
		/// <summary>
		/// <see cref="Explorer.UpdatePanel"/> worker.
		/// </summary>
		/// <remarks>
		/// Script variables: <c>$this</c> is this explorer, <c>$_</c> is the <see cref="Panel"/> to be updated.
		/// </remarks>
		public ScriptBlock AsUpdatePanel { get; set; }
		/// <summary>
		/// Calls <see cref="AsUpdatePanel"/>.
		/// </summary>
		public override void UpdatePanel(Panel panel)
		{
			if (AsUpdatePanel != null)
				A.InvokeScriptReturnAsIs(AsUpdatePanel, this, panel);
		}
		///
		internal Explorer InvokeExplorerScript(ScriptBlock script, ExplorerArgs args)
		{
			if (script == null)
			{
				args.Result = JobResult.Default;
				return null;
			}

			if (Runspace.DefaultRunspace == null)
				Runspace.DefaultRunspace = A.Psf.Runspace;

			var data = A.InvokeScript(script, this, args);
			if (data.Count == 0)
				return null;

			return (Explorer)LanguagePrimitives.ConvertTo(data[0], typeof(Explorer), null);
		}
	}
}
