/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using FarNet;
using Microsoft.PowerShell.Commands;

namespace PowerShellFar
{
	/// <summary>
	/// PowerShell provider item explorer.
	/// </summary>
	public sealed class ItemExplorer : FormatExplorer
	{
		const string TypeIdString = "07e4dde7-e113-4622-b2e9-81cf3cda927a";
		///
		public ItemExplorer(string location)
			: base(new Guid(TypeIdString))
		{
			Location = location;
			Functions =
				ExplorerFunctions.AcceptFiles |
				ExplorerFunctions.DeleteFiles |
				ExplorerFunctions.CloneFile |
				ExplorerFunctions.CreateFile |
				ExplorerFunctions.GetContent |
				ExplorerFunctions.SetText |
				ExplorerFunctions.RenameFile;
		}
		internal ItemExplorer(PathInfoEx info)
			: this(info.Path)
		{
			_Info_ = info;
		}
		//! Very slow operation, that is why we propagate the provider on exploring.
		internal PathInfoEx Info() { return _Info_ ?? (_Info_ = new PathInfoEx(Location)); }
		PathInfoEx _Info_;
		internal ProviderInfo Provider
		{
			get { return _Provider_ ?? (_Provider_ = Info().Provider); }
			private set { _Provider_ = value; }
		}
		ProviderInfo _Provider_;
		///
		public override Panel DoCreatePanel()
		{
			return new ItemPanel(this);
		}
		///
		public override void DoAcceptFiles(AcceptFilesEventArgs args)
		{
			if (args == null) return;

			// that source
			var that = args.Explorer as ItemExplorer;
			if (that == null)
			{
				if (args.UI) A.Message(Res.UnknownFileSource);
				args.Result = JobResult.Ignore;
				return;
			}

			// this target
			if (!My.ProviderInfoEx.IsNavigation(Provider))
			{
				//! Actually e.g. functions can be copied, see UICopyHere
				if (args.UI) A.Message(Res.NotSupportedByProvider);
				args.Result = JobResult.Ignore;
				return;
			}

			Command c;
			if (args.Move)
			{
				// Move-Item -Force
				c = new Command("Move-Item");
				c.Parameters.Add(Prm.Force);
			}
			else
			{
				// Copy-Item -Recurse
				c = new Command("Copy-Item");
				c.Parameters.Add(Prm.Recurse);
			}
			// -Destination
			c.Parameters.Add("Destination", this.Location);
			// -Confirm
			c.Parameters.Add(Prm.Confirm);
			// -ErrorAction
			c.Parameters.Add(Prm.ErrorAction, ActionPreference.Continue);

			// call
			using (PowerShell ps = A.Psf.CreatePipeline())
			{
				ps.Commands.AddCommand(c);
				ps.Invoke(args.FilesData);

				// errors
				if (ps.Streams.Error.Count > 0)
				{
					args.Result = JobResult.Incomplete;
					if (args.UI)
						A.ShowError(ps);
				}
			}
		}
		///
		public override void DoDeleteFiles(DeleteFilesEventArgs args)
		{
			if (args == null) return;

			// Remove-Item
			Command c = new Command("Remove-Item");

			// -Confirm -Recurse
			FarConfirmations confirm = Far.Net.Confirmations;
			if ((confirm & FarConfirmations.Delete) != 0 && (confirm & FarConfirmations.DeleteNotEmptyFolders) != 0)
			{
				c.Parameters.Add(Prm.Confirm);
			}
			else if ((confirm & FarConfirmations.Delete) != 0)
			{
				c.Parameters.Add(Prm.Confirm);
				c.Parameters.Add(Prm.Recurse);
			}
			else if ((confirm & FarConfirmations.DeleteNotEmptyFolders) == 0)
			{
				c.Parameters.Add(Prm.Recurse);
			}

			// -Force
			c.Parameters.Add(Prm.Force);

			// -ErrorAction
			c.Parameters.Add(Prm.ErrorAction, ActionPreference.Continue);

			// call
			try
			{
				using (PowerShell ps = A.Psf.CreatePipeline())
				{
					ps.Commands.AddCommand(c);
					ps.Invoke(args.FilesData);

					if (ps.Streams.Error.Count > 0)
					{
						args.Result = JobResult.Incomplete;
						if (args.UI)
							A.ShowError(ps);
					}
				}
			}
			catch
			{
				args.Result = JobResult.Incomplete;
				throw;
			}
		}
		///
		public override void DoGetContent(GetContentEventArgs args)
		{
			if (args == null) return;

			if (!My.ProviderInfoEx.HasContent(Provider))
			{
				args.Result = JobResult.Ignore;
				if (args.UI)
					A.Message(Res.NotSupportedByProvider);
				return;
			}

			if (args.File.IsDirectory)
				return;

			args.CanSet = true;

			// actual file
			string filePath = My.PathEx.TryGetFilePath(args.File.Data);
			if (filePath != null) //????base.UIEditFile(file); // to use RealNames?
			{
				args.UseFileName = filePath;
				return;
			}

			try
			{
				// item path
				string itemPath = My.PathEx.Combine(Location, args.File.Name);

				// get content
				const string code = "Get-Content -LiteralPath $args[0] -ReadCount 0";
				args.UseText = A.InvokeCode(code, itemPath);
			}
			catch (RuntimeException ex)
			{
				if (args.UI)
					Far.Net.ShowError("Edit", ex);
			}
		}
		///
		public override Explorer DoExploreDirectory(ExploreDirectoryEventArgs args)
		{
			if (args == null) return null;

			return Explore(My.PathEx.Combine(Location, args.File.Name), args);
		}
		///
		public override Explorer DoExploreParent(ExploreParentEventArgs args)
		{
			if (args == null) return null;

			/*
			We might use 'cd ..' but we have to be sure that the current location is in sync
			with the panel path. It looks more reliable to build the parent path from the panel
			path. This way is good for some not standard cases. This way is bad if current
			location is problematic:
			??
			cd HKCU:\Software\Far2\PluginHotkeys
			cd Plugins/Brackets/Brackets.dll
			[..] --> error; it is rather PS issue though...
			*/
			var path = Location;
			// 090814 use ":\\" instead of "\\", [_090814_130836]
			if (path.EndsWith(":\\", StringComparison.Ordinal))
				return null;

			// 090814 [_090814_130836] PS V2 may get paths with extra '\' in the end
			path = path.TrimEnd(new char[] { '\\' });

			// find name
			int iSlash = path.LastIndexOf('\\');
			if (iSlash < 0)
			{
				// no slashes, split by ::
				int iProvider = path.IndexOf("::", StringComparison.Ordinal);
				if (iProvider > 0)
				{
					// FarMacro
					args.PostName = path.Substring(iProvider + 2);
					path = path.Substring(0, iProvider + 2);
					return Explore(path, args);
				}
				else
				{
					return null;
				}
			}

			//! Issue with names z:|z, Microsoft.PowerShell.Core\Registry::HKEY_CURRENT_USER - Far doesn't set cursor there
			if (path.Length > iSlash + 2)
				args.PostName = path.Substring(iSlash + 1);

			path = path.Substring(0, iSlash);
			if (path.StartsWith("\\\\", StringComparison.Ordinal)) //HACK network path
			{
				iSlash = path.LastIndexOf('\\');
				if (iSlash <= 1)
				{
					// show computer shares menu
					string computer = path.Substring(2);
					string share = UI.SelectMenu.SelectShare(computer); //???? kill?
					if (share == null)
						return null;
					else
						path += "\\" + share;
				}
			}

			// add \, else we can't step to the root from level 1
			if (path != null && path.EndsWith(":", StringComparison.Ordinal))
				path += "\\";

			return Explore(path, args);
		}
		///
		public override Explorer DoExploreRoot(ExploreRootEventArgs args)
		{
			string driveName = Info().DriveName;
			if (string.IsNullOrEmpty(driveName))
				return null;

			return Explore(driveName + ":\\", args);
		}
		Explorer Explore(string location, ExplorerEventArgs args)
		{
			//! propagate the provider, or performance sucks
			ItemExplorer newExplorer = new ItemExplorer(location);
			newExplorer.Provider = Provider;
			newExplorer.Columns = Columns;
			return newExplorer;
		}
		internal override void BuildFiles(Collection<PSObject> values)
		{
			if (!My.ProviderInfoEx.IsNavigation(Provider))
			{
				base.BuildFiles(values);
				return;
			}

			Cache.Clear();
			if (Provider.ImplementingType == typeof(FileSystemProvider))
			{
				foreach (PSObject value in values)
					Cache.Add(new SystemMapFile(value, Map));
			}
			else
			{
				foreach (PSObject value in values)
					Cache.Add(new ItemMapFile(value, Map));
			}
		}
		internal override object GetData(ExplorerEventArgs args)
		{
			// get child items for the panel location
			var items = A.GetChildItems(Location);

			// standard
			if (0 == (args.Mode & ExplorerModes.Find) || !My.ProviderInfoEx.IsNavigation(Provider))
				return items;

			// faster
			Cache.Clear();
			foreach (PSObject value in items)
				Cache.Add(new ItemFile(value));
			return Cache;
		}
		///
		public override void DoRenameFile(RenameFileEventArgs args)
		{
			if (args == null) return;

			var newName = args.Parameter as string;
			if (newName == null)
				throw new InvalidOperationException(Res.ParameterString);

			// workaround; Rename-Item has no -LiteralPath; e.g. z`z[z.txt is a big problem
			string src = Kit.EscapeWildcard(My.PathEx.Combine(Location, args.File.Name));
			A.Psf.Engine.InvokeProvider.Item.Rename(src, newName);
		}
		///
		public override void DoCreateFile(CreateFileEventArgs args)
		{
			if (args == null) return;

			args.Result = JobResult.Ignore;

			UI.NewValueDialog ui = new UI.NewValueDialog("New " + Provider.Name + " item");
			while (ui.Dialog.Show())
			{
				try
				{
					using (PowerShell p = A.Psf.CreatePipeline())
					{
						//! Don't use Value if it is empty (e.g. to avoid (default) property at new key in Registry).
						//! Don't use -Force or you silently kill existing item\property (with all children, properties, etc.)
						Command c = new Command("New-Item");
						// -Path (it is literal)
						c.Parameters.Add("Path", My.PathEx.Combine(Location, ui.Name.Text));
						// -ItemType
						if (ui.Type.Text.Length > 0)
							c.Parameters.Add("ItemType", ui.Type.Text);
						// -Value
						if (ui.Value.Text.Length > 0)
							c.Parameters.Add("Value", ui.Value.Text);
						// -ErrorAction
						c.Parameters.Add(Prm.ErrorAction, ActionPreference.Continue);
						p.Commands.AddCommand(c);
						p.Invoke();
						if (A.ShowError(p))
							continue;
					}

					// done
					args.Result = JobResult.Done;
					args.PostName = ui.Name.Text;
					return;
				}
				catch (RuntimeException ex)
				{
					A.Message(ex.Message);
					continue;
				}
			}
		}
		///
		public override void DoSetText(SetTextEventArgs args)
		{
			if (args == null) return;

			try
			{
				// path
				string path = My.PathEx.Combine(Location, args.File.Name);

				// read
				string text = args.Text.TrimEnd();

				// set
				if (!A.SetContentUI(path, text))
					return;
			}
			catch (RuntimeException ex)
			{
				if (args.UI)
					A.Message(ex.Message);
			}
		}
		///
		public override void DoCloneFile(CloneFileEventArgs args)
		{
			if (args == null) return;

			var newName = args.Parameter as string;
			if (newName == null)
				throw new InvalidOperationException(Res.ParameterString);

			string source = Kit.EscapeWildcard(My.PathEx.Combine(Location, args.File.Name));
			string target = My.PathEx.Combine(Location, newName);
			A.Psf.Engine.InvokeProvider.Item.Copy(source, target, false, CopyContainers.CopyTargetContainer);

			args.PostName = newName;
		}
	}
}
