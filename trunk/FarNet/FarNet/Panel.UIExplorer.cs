
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2015 Roman Kuzmin
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FarNet
{
	public partial class Panel
	{
		/// <summary>
		/// Calls <see cref="FarNet.Explorer.GetFiles"/>.
		/// </summary>
		/// <param name="args">.</param>
		public virtual IList<FarFile> UIGetFiles(GetFilesEventArgs args)
		{
			return Explorer.GetFiles(args);
		}
		/// <summary>
		/// Calls <see cref="FarNet.Explorer.ExploreDirectory"/>.
		/// </summary>
		/// <param name="args">.</param>
		public virtual Explorer UIExploreDirectory(ExploreDirectoryEventArgs args)
		{
			return Explorer.ExploreDirectory(args);
		}
		/// <summary>
		/// Calls <see cref="FarNet.Explorer.ExploreLocation"/>.
		/// </summary>
		/// <param name="args">.</param>
		public virtual Explorer UIExploreLocation(ExploreLocationEventArgs args)
		{
			return Explorer.ExploreLocation(args);
		}
		/// <summary>
		/// Calls <see cref="FarNet.Explorer.ExploreParent"/>.
		/// </summary>
		/// <param name="args">.</param>
		public virtual Explorer UIExploreParent(ExploreParentEventArgs args)
		{
			return Explorer.ExploreParent(args);
		}
		/// <summary>
		/// Calls <see cref="FarNet.Explorer.ExploreRoot"/>.
		/// </summary>
		/// <param name="args">.</param>
		public virtual Explorer UIExploreRoot(ExploreRootEventArgs args)
		{
			return Explorer.ExploreRoot(args);
		}
		/// <summary>
		/// Calls <see cref="FarNet.Explorer.GetContent"/>.
		/// </summary>
		/// <param name="args">.</param>
		public virtual void UIGetContent(GetContentEventArgs args)
		{
			Explorer.GetContent(args);
		}
		/// <summary>
		/// Calls <see cref="FarNet.Explorer.SetFile"/> and <see cref="OnThisFileChanged"/>.
		/// </summary>
		/// <param name="args">.</param>
		public virtual void UISetFile(SetFileEventArgs args)
		{
			if (args == null) return;

			Explorer.SetFile(args);

			if (args.Result != JobResult.Ignore)
				OnThisFileChanged(args);
		}
		/// <summary>
		/// Calls <see cref="FarNet.Explorer.SetText"/> and <see cref="OnThisFileChanged"/>.
		/// </summary>
		/// <param name="args">.</param>
		public virtual void UISetText(SetTextEventArgs args)
		{
			if (args == null) return;

			Explorer.SetText(args);

			if (args.Result != JobResult.Ignore)
				OnThisFileChanged(args);
		}
		/// <summary>
		/// Calls <see cref="FarNet.Explorer.AcceptFiles"/> and <see cref="OnThisFileChanged"/>.
		/// </summary>
		/// <param name="args">.</param>
		public virtual void UIAcceptFiles(AcceptFilesEventArgs args)
		{
			if (args == null) return;

			Explorer.AcceptFiles(args);

			if (args.Result != JobResult.Ignore)
				OnThisFileChanged(args);
		}
		/// <summary>
		/// Calls <see cref="FarNet.Explorer.DeleteFiles"/> and <see cref="OnThisFileChanged"/>.
		/// </summary>
		/// <param name="args">.</param>
		public virtual void UIDeleteFiles(DeleteFilesEventArgs args)
		{
			if (args == null) return;

			Explorer.DeleteFiles(args);

			if (args.Result != JobResult.Ignore)
				OnThisFileChanged(args);
		}
		/// <summary>
		/// Calls <see cref="FarNet.Explorer.ExportFiles"/> and <see cref="OnThisFileChanged"/>.
		/// </summary>
		/// <param name="args">.</param>
		public virtual void UIExportFiles(ExportFilesEventArgs args)
		{
			if (args == null) return;

			Explorer.ExportFiles(args);

			if (args.Result != JobResult.Ignore && args.Move)
				OnThisFileChanged(args);
		}
		/// <summary>
		/// Calls <see cref="FarNet.Explorer.ImportFiles"/> and <see cref="OnThisFileChanged"/>.
		/// </summary>
		/// <param name="args">.</param>
		public virtual void UIImportFiles(ImportFilesEventArgs args)
		{
			if (args == null) return;

			Explorer.ImportFiles(args);

			if (args.Result != JobResult.Ignore)
				OnThisFileChanged(args);
		}
		/// <summary>
		/// Calls <see cref="FarNet.Explorer.CloneFile"/> and <see cref="OnThisFileChanged"/>.
		/// </summary>
		/// <param name="args">.</param>
		public virtual void UICloneFile(CloneFileEventArgs args)
		{
			if (args == null) return;

			Explorer.CloneFile(args);

			if (args.Result != JobResult.Ignore)
				OnThisFileChanged(args);
		}
		/// <summary>
		/// Calls <see cref="FarNet.Explorer.CreateFile"/> and <see cref="OnThisFileChanged"/>.
		/// </summary>
		/// <param name="args">.</param>
		public virtual void UICreateFile(CreateFileEventArgs args)
		{
			if (args == null) return;

			Explorer.CreateFile(args);

			if (args.Result != JobResult.Ignore)
				OnThisFileChanged(args);
		}
		/// <summary>
		/// Calls <see cref="FarNet.Explorer.OpenFile"/>.
		/// </summary>
		/// <param name="args">.</param>
		public virtual Explorer UIOpenFile(OpenFileEventArgs args)
		{
			return Explorer.OpenFile(args);
		}
		/// <summary>
		/// Calls <see cref="FarNet.Explorer.RenameFile"/> and <see cref="OnThisFileChanged"/>.
		/// </summary>
		/// <param name="args">.</param>
		public virtual void UIRenameFile(RenameFileEventArgs args)
		{
			if (args == null) return;

			Explorer.RenameFile(args);
			
			if (args.Result != JobResult.Ignore)
				OnThisFileChanged(args);
		}
	}
}
