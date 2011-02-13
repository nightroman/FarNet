
/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
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
		/// New explorer.
		/// </summary>
		/// <param name="location">The assigned location (path, current directory, etc.)</param>
		public PowerExplorer(string location) : base(location) {}
		/// <summary>
		/// Gets or sets user data.
		/// </summary>
		public PSObject Data { get; set; }
		#region Explorer
		/// <summary>
		/// <see cref="Explorer.Explore"/> worker. It must be set.
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
		/// <see cref="Explorer.ExploreFile"/> worker.
		/// </summary>
		/// <remarks>
		/// Script variables: <c>$this</c> is this explorer, <c>$_</c> is <see cref="ExploreFileArgs"/>.
		/// </remarks>
		public ScriptBlock AsExploreFile { get; set; }
		/// <summary>
		/// Calls <see cref="AsExploreFile"/>.
		/// </summary>
		public override Explorer ExploreFile(ExploreFileArgs args)
		{
			return InvokeExplorerScript(AsExploreFile, args);
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
		#endregion
		#region Panel
		/// <summary>
		/// <see cref="Explorer.MakePanel"/> worker.
		/// </summary>
		/// <remarks>
		/// Script variables: <c>$this</c> is this explorer, <c>$_</c> is <see cref="PanelMakerArgs"/>.
		/// </remarks>
		public ScriptBlock AsMakePanel { get; set; }
		/// <summary>
		/// Calls <see cref="AsMakePanel"/>.
		/// </summary>
		public override Panel MakePanel(PanelMakerArgs args)
		{
			if (args == null) throw new ArgumentNullException("args");
			if (AsMakePanel == null)
			{
				args.Result = ExplorerResult.Default;
				return null;
			}

			var data = A.InvokeScript(AsMakePanel, this, args);
			if (data.Count == 0)
				return null;

			return (Panel)LanguagePrimitives.ConvertTo(data[0], typeof(Panel), null);
		}
		/// <summary>
		/// <see cref="Explorer.SetupPanel"/> worker.
		/// </summary>
		/// <remarks>
		/// Script variables: <c>$this</c> is this explorer, <c>$_</c> is <see cref="PanelMakerArgs"/>.
		/// </remarks>
		public ScriptBlock AsSetupPanel { get; set; }
		/// <summary>
		/// Calls <see cref="AsSetupPanel"/>.
		/// </summary>
		public override void SetupPanel(PanelMakerArgs args)
		{
			if (args == null) throw new ArgumentNullException("args");
			if (AsSetupPanel == null)
				args.Result = ExplorerResult.Default;
			else
				A.InvokeScriptReturnAsIs(AsSetupPanel, this, args);
		}
		/// <summary>
		/// <see cref="Explorer.UpdatePanel"/> worker.
		/// </summary>
		/// <remarks>
		/// Script variables: <c>$this</c> is this explorer, <c>$_</c> is <see cref="PanelMakerArgs"/>.
		/// </remarks>
		public ScriptBlock AsUpdatePanel { get; set; }
		/// <summary>
		/// Calls <see cref="AsUpdatePanel"/>.
		/// </summary>
		public override void UpdatePanel(PanelMakerArgs args)
		{
			if (args == null) throw new ArgumentNullException("args");
			if (AsUpdatePanel == null)
				args.Result = ExplorerResult.Default;
			else
				A.InvokeScriptReturnAsIs(AsUpdatePanel, this, args);
		}
		#endregion
		#region File
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
				args.Result = ExplorerResult.Default;
			else
				A.InvokeScriptReturnAsIs(AsExportFile, this, args);
		}
		/// <summary>
		/// <see cref="Explorer.CanExportFile"/> worker.
		/// </summary>
		public ScriptBlock AsCanExportFile { get; set; }
		/// <summary>
		/// Calls <see cref="AsCanExportFile"/>.
		/// </summary>
		public override bool CanExportFile(FarFile file)
		{
			if (AsExportFile == null)
				return false;

			if (AsCanExportFile == null)
				return true;

			return (bool)LanguagePrimitives.ConvertTo(A.InvokeScriptReturnAsIs(AsCanExportFile, this, file), typeof(bool), null);
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
				args.Result = ExplorerResult.Default;
			else
				A.InvokeScriptReturnAsIs(AsImportFile, this, args);
		}
		/// <summary>
		/// <see cref="Explorer.CanImportFile"/> worker.
		/// </summary>
		public ScriptBlock AsCanImportFile { get; set; }
		/// <summary>
		/// Calls <see cref="AsCanImportFile"/>.
		/// </summary>
		public override bool CanImportFile(FarFile file)
		{
			if (AsImportFile == null)
				return false;

			if (AsCanImportFile == null)
				return true;

			return (bool)LanguagePrimitives.ConvertTo(A.InvokeScriptReturnAsIs(AsCanImportFile, this, file), typeof(bool), null);
		}
		#endregion
		///
		internal Explorer InvokeExplorerScript(ScriptBlock script, ExplorerArgs args)
		{
			if (script == null)
			{
				args.Result = ExplorerResult.Default;
				return null;
			}

			var data = A.InvokeScript(script, this, args);
			if (data.Count == 0)
				return null;

			return (Explorer)LanguagePrimitives.ConvertTo(data[0], typeof(Explorer), null);
		}
	}
}
