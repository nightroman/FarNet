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
	sealed class ItemExplorer : FormatExplorer
	{
		const string TypeIdString = "07e4dde7-e113-4622-b2e9-81cf3cda927a";
		public ItemExplorer(string location)
			: base(new Guid(TypeIdString))
		{
			Location = location;
			Functions =
				ExplorerFunctions.AcceptFiles |
				ExplorerFunctions.DeleteFiles |
				ExplorerFunctions.CreateFile |
				ExplorerFunctions.ExportFile |
				ExplorerFunctions.ImportText;
		}
		internal ItemExplorer(PowerPath info)
			: this(info.Path)
		{
			_Info_ = info;
		}
		//! Very slow operation, that is why we propagate the provider on exploring.
		internal PowerPath Info() { return _Info_ ?? (_Info_ = new PowerPath(Location)); }
		PowerPath _Info_;
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

			// event
			var panel = args.Panel as ItemPanel;
			if (panel != null)
				panel.DoItemsChanged();
		}
		///
		public override void DoDeleteFiles(DeleteFilesEventArgs args)
		{
			if (args == null) return;

			var panel = args.Panel as ItemPanel;

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

					if (panel != null)
					{
						ItemPanel pp2 = panel.TargetPanel as ItemPanel;
						if (pp2 != null)
							pp2.UpdateRedraw(true);
					}
				}
			}
			catch
			{
				args.Result = JobResult.Incomplete;
				throw;
			}
			finally
			{
				// fire
				if (panel != null)
					panel.DoItemsChanged();
			}
		}
		///
		public override void DoExportFile(ExportFileEventArgs args)
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

			args.CanImport = true;

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
		public override void DoImportText(ImportTextEventArgs args)
		{
			if (args == null) return;

			try
			{
				// item path
				string itemPath = My.PathEx.Combine(Location, args.File.Name);

				// read
				string text = args.Text.TrimEnd();

				// set
				if (!A.SetContentUI(itemPath, text))
					return;

				// update a panel after edit
				if (0 != (args.Mode & ExplorerModes.Edit)) //???? in 99% it is not needed, think to avoi
				{
					var panel = args.Panel as ItemPanel;
					if (panel != null)
						panel.UpdateRedraw(true);
				}
			}
			catch (RuntimeException ex)
			{
				if (args.UI)
					A.Message(ex.Message);
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
				return Explore(null, args);

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
				}
				else
				{
					path = null;
				}
				return Explore(path, args);
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
					string share = UI.SelectMenu.SelectShare(computer);
					if (share == null)
						path = null;
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
			// new explorer
			ItemExplorer newExplorer;

			// no location, show the drive menu
			if (location == null)
			{
				var panel = args.Panel as ItemPanel;

				// silent
				if (panel == null || !args.UI)
					return null;

				// custom columns or drive
				if (panel.Columns != null || panel.Drive.Length > 0)
					return null;

				// menu
				location = UI.SelectMenu.SelectDrive(Info().DriveName, false);
				if (location == null)
					return null;

				// unknown
				newExplorer = new ItemExplorer(location);
			}
			else
			{
				//! propagate the provider, or performance sucks
				newExplorer = new ItemExplorer(location);
				newExplorer.Provider = Provider;
				newExplorer.Columns = Columns;
			}

			return newExplorer;
		}
		///
		public override void DoCreateFile(CreateFileEventArgs args)
		{
			if (args == null) return;
			
			// all done
			args.Result = JobResult.Ignore;

			var panel = args.Panel as ItemPanel;
			if (panel == null)
			{
				args.Result = JobResult.Ignore;
				return;
			}

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

					// fire
					panel.DoItemsChanged();

					// update this panel with name
					panel.UpdateRedraw(false, ui.Name.Text);

					// update that panel if the path is the same
					ItemPanel pp2 = panel.TargetPanel as ItemPanel;
					if (pp2 != null && pp2.Explorer.Location == Location)
						pp2.UpdateRedraw(true);

					// exit the loop
					return;
				}
				catch (RuntimeException exception)
				{
					A.Message(exception.Message);
					continue;
				}
			}
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
	}
}
