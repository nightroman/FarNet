
/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using FarNet;
using Microsoft.PowerShell.Commands;

namespace PowerShellFar
{
	/// <summary>
	/// Explorer of a provider item properties.
	/// </summary>
	public sealed class PropertyExplorer : Explorer
	{
		const string TypeIdString = "19f5261b-4f82-4a0a-93c0-1741f6715752";
		readonly PathInfoEx _ThePath;
		internal string ItemPath { get { return _ThePath.Path; } }
		internal ProviderInfo Provider { get { return _ThePath.Provider; } }
		/// <summary>
		/// New property explorer with a provider item path.
		/// </summary>
		/// <param name="itemPath">Item path.</param>
		public PropertyExplorer(string itemPath) :
			base(new Guid(TypeIdString))
		{
			if (itemPath == null) throw new ArgumentNullException("itemPath");

			// the path
			_ThePath = new PathInfoEx(itemPath);

			Functions =
				ExplorerFunctions.GetContent |
				ExplorerFunctions.SetText;

			if (My.ProviderInfoEx.HasDynamicProperty(Provider))
				Functions |= (
					ExplorerFunctions.AcceptFiles |
					ExplorerFunctions.DeleteFiles |
					ExplorerFunctions.CloneFile |
					ExplorerFunctions.CreateFile |
					ExplorerFunctions.RenameFile);
		}
		///
		public override Panel CreatePanel()
		{
			return new PropertyPanel(this);
		}
		///
		public override IList<FarFile> GetFiles(GetFilesEventArgs args)
		{
			var result = new List<FarFile>();
			if (args == null) return result;

			try
			{
				//! get properties
				// - Using -LiteralPath is a problem, e.g. Registry: returns nothing.
				// - Script is used for PS conversion of property values to string.
				// - Script has to ignore a property with empty name (if any, can be in Registry).
				// - If PS* included then they can't be found by `gp <path> <name>`;
				// so, don't add, they are noisy anyway (even if marked system or hidden).

				// get property bag 090409
				Collection<PSObject> bag = A.Psf.Engine.InvokeProvider.Property.Get(Kit.EscapeWildcard(ItemPath), null);

				// filter
				var filter = new List<string>(5);
				filter.Add("PSChildName");
				filter.Add("PSDrive");
				filter.Add("PSParentPath");
				filter.Add("PSPath");
				filter.Add("PSProvider");

				// add
				foreach (PSObject o in bag)
				{
					foreach (PSPropertyInfo pi in o.Properties)
					{
						// skip empty ?? still needed?
						string name = pi.Name;
						if (string.IsNullOrEmpty(name))
							continue;

						// filter and shrink filter
						int i = filter.IndexOf(name);
						if (i >= 0)
						{
							filter.RemoveAt(i);
							continue;
						}

						// create file
						SetFile file = new SetFile()
						{
							Name = name,
							IsReadOnly = !pi.IsSettable,
							Data = pi,
							// set its value
							Description = Converter.FormatValue(pi.Value, Settings.Default.FormatEnumerationLimit)
						};

						// add
						result.Add(file);
					}
				}
			}
			catch (RuntimeException error)
			{
				if (args.UI)
					A.Message(error.Message);
			}

			return result;
		}
		///
		public override void GetContent(GetContentEventArgs args)
		{
			if (args == null) return;

			PSPropertyInfo pi = args.File.Data as PSPropertyInfo;
			if (pi == null)
			{
				args.Result = JobResult.Ignore;
				return;
			}

			try
			{
				args.CanSet = pi.IsSettable;
				args.UseText = Converter.InfoToLine(pi);
				if (args.UseText == null)
					args.UseText = A.InvokeCode("$args[0] | Out-String -Width $args[1] -ErrorAction Stop", pi.Value, int.MaxValue)[0].ToString();
			}
			catch (RuntimeException ex)
			{
				Far.Net.ShowError("Edit", ex);
			}
		}
		///
		public override void SetText(SetTextEventArgs args)
		{
			if (args == null) return;

			PSPropertyInfo pi = args.File.Data as PSPropertyInfo;
			if (pi == null)
			{
				args.Result = JobResult.Ignore;
				return;
			}

			try
			{
				object value;
				string text = args.Text.TrimEnd();
				if (pi.TypeNameOfValue.EndsWith("]", StringComparison.Ordinal))
				{
					ArrayList lines = new ArrayList();
					foreach (var line in FarNet.Works.Kit.SplitLines(text))
						lines.Add(line);
					value = lines;
				}
				else
				{
					value = text;
				}

				A.SetPropertyValue(ItemPath, pi.Name, Converter.Parse(pi, value));
				PropertyPanel.WhenPropertyChanged(ItemPath);
			}
			catch (RuntimeException ex)
			{
				if (args.UI)
					A.Msg(ex);
			}
		}
		///
		public override void DeleteFiles(DeleteFilesEventArgs args)
		{
			if (args == null) return;

			// to ask
			bool confirm = args.UI && (Far.Net.Confirmations & FarConfirmations.Delete) != 0;

			// names to be deleted
			List<string> names = A.FileNameList(args.Files);

			//! Registry: workaround: (default)
			if (Provider.ImplementingType == typeof(RegistryProvider))
			{
				for (int i = names.Count; --i >= 0; )
				{
					if (Kit.Equals(names[i], "(default)"))
					{
						// remove or not
						if (!confirm || 0 == Far.Net.Message("Delete the (default) property", Res.Delete, MsgOptions.YesNo))
							A.Psf.Engine.InvokeProvider.Property.Remove(Kit.EscapeWildcard(ItemPath), string.Empty);

						// remove from the list in any case
						names.RemoveAt(i);
						break;
					}
				}

				// done?
				if (names.Count == 0)
					return;
			}

			using (PowerShell ps = A.Psf.CreatePipeline())
			{
				Command command = new Command("Remove-ItemProperty");
				command.Parameters.Add("LiteralPath", ItemPath);
				command.Parameters.Add(Word.Name, names);
				if (confirm)
					command.Parameters.Add(Prm.Confirm);
				command.Parameters.Add(Prm.Force);
				command.Parameters.Add(Prm.ErrorAction, ActionPreference.Continue);
				ps.Commands.AddCommand(command);
				ps.Invoke();

				// ?? V2 CTP3 bug: Registry: Remove-ItemProperty -Confirm fails on 'No':
				// Remove-ItemProperty : Property X does not exist at path HKEY_CURRENT_USER\Y
				// There is no workaround added yet. Submitted: MS Connect #484664.
				if (ps.Streams.Error.Count > 0)
				{
					args.Result = JobResult.Incomplete;
					if (args.UI)
						A.ShowError(ps);
				}
			}
		}
		///
		public override void AcceptFiles(AcceptFilesEventArgs args)
		{
			if (args == null) return;

			// that source
			var that = args.Explorer as PropertyExplorer;
			if (that == null)
			{
				if (args.UI) A.Message(Res.UnknownFileSource);
				args.Result = JobResult.Ignore;
				return;
			}

			// names
			List<string> names = A.FileNameList(args.Files);

			//! gotchas in {Copy|Move}-ItemProperty:
			//! *) -Name takes a single string only? (help: yes (copy), no (move) - odd!)
			//! *) Names can't be pipelined (help says they can)
			//! so, use provider directly (no confirmation)
			string source = Kit.EscapeWildcard(that.ItemPath);
			string target = Kit.EscapeWildcard(this.ItemPath);
			if (args.Move)
			{
				foreach (string name in names)
					A.Psf.Engine.InvokeProvider.Property.Move(source, name, target, name);
			}
			else
			{
				foreach (string name in names)
					A.Psf.Engine.InvokeProvider.Property.Copy(source, name, target, name);
			}
		}
		///
		public override void RenameFile(RenameFileEventArgs args)
		{
			if (args == null) return;

			var newName = args.Parameter as string;
			if (newName == null)
				throw new InvalidOperationException(Res.ParameterString);

			//! Registry: workaround: (default)
			if (Kit.Equals(args.File.Name, "(default)") && Provider.ImplementingType == typeof(RegistryProvider))
			{
				args.Result = JobResult.Ignore;
				if (args.UI)
					A.Message("Cannot rename this property.");
				return;
			}

			using (PowerShell ps = A.Psf.CreatePipeline())
			{
				Command c = new Command("Rename-ItemProperty");
				c.Parameters.Add(new CommandParameter("LiteralPath", ItemPath));
				c.Parameters.Add(new CommandParameter(Word.Name, args.File.Name));
				c.Parameters.Add(new CommandParameter("NewName", newName));
				c.Parameters.Add(Prm.Force);
				c.Parameters.Add(Prm.ErrorAction, ActionPreference.Continue);
				ps.Commands.AddCommand(c);
				ps.Invoke();

				if (ps.Streams.Error.Count > 0)
				{
					args.Result = JobResult.Ignore;
					if (args.UI)
						A.ShowError(ps);
				}
			}
		}
		///
		public override void CreateFile(CreateFileEventArgs args)
		{
			if (args == null) return;
			args.Result = JobResult.Ignore;

			UI.NewValueDialog ui = new UI.NewValueDialog("New property");
			while (ui.Dialog.Show())
			{
				try
				{
					using (PowerShell p = A.Psf.CreatePipeline())
					{
						//! Don't use Value if it is empty (e.g. to avoid (default) property at new key in Registry).
						//! Don't use -Force or you silently kill existing item\property (with all children, properties, etc.)
						Command c = new Command("New-ItemProperty");
						c.Parameters.Add("LiteralPath", ItemPath);
						c.Parameters.Add(Word.Name, ui.Name.Text);
						c.Parameters.Add("PropertyType", ui.Type.Text);

						if (ui.Value.Text.Length > 0)
							c.Parameters.Add("Value", ui.Value.Text);
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
				catch (RuntimeException exception)
				{
					A.Message(exception.Message);
					continue;
				}
			}
		}
		///
		public override void CloneFile(CloneFileEventArgs args)
		{
			if (args == null) return;

			var newName = args.Parameter as string;
			if (newName == null)
				throw new InvalidOperationException(Res.ParameterString);
			
			string src = Kit.EscapeWildcard(ItemPath);
			A.Psf.Engine.InvokeProvider.Property.Copy(src, args.File.Name, src, newName);

			args.PostName = newName;
		}
	}
}
