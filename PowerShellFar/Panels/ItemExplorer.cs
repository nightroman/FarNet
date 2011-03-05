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
		internal readonly PowerPath ThePath;
		public ItemExplorer(string path)
			: base(new Guid(TypeIdString))
		{
			// location and position
			if (!string.IsNullOrEmpty(path))
			{
				// resolve a share for a computer
				if (path.StartsWith("\\\\", StringComparison.Ordinal))
				{
					string computer = path.Substring(2);
					while (computer.EndsWith("\\", StringComparison.Ordinal))
						computer = computer.Substring(0, computer.Length - 1);
					if (computer.IndexOf('\\') < 0)
					{
						string share = UI.SelectMenu.SelectShare(computer);
						if (share == null)
							path = ".";
						else
							path = "\\\\" + computer + "\\" + share;
					}
				}
			}

			ThePath = new PowerPath(path);
			Location = ThePath.Path;
			Functions =
				ExplorerFunctions.AcceptFiles |
				ExplorerFunctions.DeleteFiles |
				ExplorerFunctions.CreateFile |
				ExplorerFunctions.ExportFile |
				ExplorerFunctions.ImportText;

		}
		///
		public override Panel DoCreatePanel() //?????
		{
			throw new ModuleException("Panel is not yet supported.");
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
			if (!My.ProviderInfoEx.IsNavigation(ThePath.Provider))
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
			c.Parameters.Add("Destination", this.ThePath.Path);
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
					ps.Invoke(Panel.SelectedItems);

					if (ps.Streams.Error.Count > 0)
					{
						args.Result = JobResult.Incomplete;
						if (args.UI)
							A.ShowError(ps);
					}

					ItemPanel pp2 = Panel.TargetPanel as ItemPanel;
					if (pp2 != null)
						pp2.UpdateRedraw(true);
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
				((ItemPanel)Panel).DoItemsChanged();
			}
		}
		///
		public override void DoExportFile(ExportFileEventArgs args)
		{
			if (args == null) return;

			if (!My.ProviderInfoEx.HasContent(ThePath.Provider))
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
				string itemPath = My.PathEx.Combine(ThePath.Path, args.File.Name);

				// get content
				const string code = "Get-Content -LiteralPath $args[0] -ReadCount 0";
				args.UseText = A.Psf.InvokeCode(code, itemPath);
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
				string itemPath = My.PathEx.Combine(ThePath.Path, args.File.Name);

				// read
				string text = args.Text.TrimEnd();

				// set
				if (!A.SetContentUI(itemPath, text))
					return;

				// update a panel
				Panel.UpdateRedraw(true); //?????
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

			if (!args.UI) //?????
				return null;
			
			return ExplorePath(My.PathEx.Combine(ThePath.Path, args.File.Name));
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
			var path = ThePath.Path;
			// 090814 use ":\\" instead of "\\", [_090814_130836]
			if (path.EndsWith(":\\", StringComparison.Ordinal))
				return ExplorePath(null);

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
				return ExplorePath(path);
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
			if (path != null)
				path += "\\";

			return ExplorePath(path);
		}
		///
		public override Explorer DoExploreRoot(ExploreRootEventArgs args)
		{
			string driveName = ThePath.DriveName;
			if (string.IsNullOrEmpty(driveName))
				return null;

			return ExplorePath(driveName + ":\\");
		}
		Explorer ExplorePath(string path)
		{
			// no location, show drive menu
			if (path == null)
			{
				// fixed drive?
				if (((ItemPanel)Panel).Drive.Length > 0)
					return null;

				// menu
				path = UI.SelectMenu.SelectDrive(ThePath.DriveName, false);
				if (path == null)
					return null;
			}

			var explorer = new ItemExplorer(path);
			//1
			explorer.Panel = Panel;
			//2
			explorer.Columns = Columns; //?????? yes but need for the same provider only; when create a panel, do set plan.
			((ItemPanel)Panel).ChangeLocation(ThePath, explorer.ThePath);

			return explorer;
		}
		internal override void BuildFiles(Collection<PSObject> values)
		{
			if (!My.ProviderInfoEx.IsNavigation(ThePath.Provider))
			{
				base.BuildFiles(values);
				return;
			}

			Cache.Clear();
			if (ThePath.Provider.ImplementingType == typeof(FileSystemProvider))
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
		///
		public override void DoCreateFile(CreateFileEventArgs args)
		{
			// it does all itself //?????
			args.Result = JobResult.Ignore;
			
			var panel = args.Panel as ItemPanel;
			if (panel == null)
			{
				args.Result = JobResult.Ignore;
				return;
			}
			
			UI.NewValueDialog ui = new UI.NewValueDialog("New " + ThePath.Provider.Name + " item");
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
						c.Parameters.Add("Path", My.PathEx.Combine(ThePath.Path, ui.Name.Text));
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
					if (pp2 != null && pp2.Explorer.ThePath.Path == ThePath.Path)
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
	}
}
