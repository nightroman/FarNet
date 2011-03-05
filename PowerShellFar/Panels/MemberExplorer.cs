
/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text.RegularExpressions;
using FarNet;

namespace PowerShellFar
{
	/// <summary>
	/// Explorer of an object members (properties by default or all members).
	/// </summary>
	public sealed class MemberExplorer : Explorer
	{
		const string TypeIdString = "be8984d6-fc4d-466d-9e5b-dede881f2232";
		/// <summary>
		/// Regular expression pattern of members to be excluded.
		/// </summary>
		public string ExcludeMemberPattern
		{
			get { return _ExcludeMemberRegex == null ? null : _ExcludeMemberRegex.ToString(); }
			set { _ExcludeMemberRegex = new Regex(value, RegexOptions.IgnoreCase); }
		}
		Regex _ExcludeMemberRegex;
		/// <summary>
		/// Regular expression pattern of members to be hidden.
		/// </summary>
		public string HideMemberPattern
		{
			get { return _HideMemberRegex == null ? null : _HideMemberRegex.ToString(); }
			set { _HideMemberRegex = new Regex(value, RegexOptions.IgnoreCase); }
		}
		Regex _HideMemberRegex;
		internal readonly PSObject Value;
		internal int MemberMode { get; set; }
		/// <summary>
		/// New member explorer with an object.
		/// </summary>
		/// <param name="instance">An object which members are shown.</param>
		public MemberExplorer(object instance) :
			base(new Guid(TypeIdString))
		{
			if (instance == null) throw new ArgumentNullException("instance");
			if (instance is IPanel) throw new NotSupportedException("Panel instance is not supported.");
			Value = PSObject.AsPSObject(instance);
			if (Value.BaseObject == null)
				throw new ArgumentNullException("instance");

			Functions =
				ExplorerFunctions.DeleteFiles |
				ExplorerFunctions.CreateFile |
				ExplorerFunctions.ExportFile |
				ExplorerFunctions.ImportText;
		}
		///
		public override Panel CreatePanel()
		{
			return new MemberPanel(this);
		}
		///
		public override IList<FarFile> GetFiles(GetFilesEventArgs args)
		{
			var result = new List<FarFile>();
			if (args == null) return result;

			try
			{
				if (MemberMode == 0)
				{
					//! _100426_034702
					//! Get actual members to show:
					//! _Value.Properties contains too much, i.e. for DataRow: HasErrors, ItemArray, RowError, RowState;
					//! use Get-Member at first (don't use it itself, because it returns sorted data, we want unsorted).
					//! Idea to cache them is not good:
					//! price: high (have to sync on exclude, add, delete, etc.)
					//! value: low (it is UI and member number is normally small)
					var membersToShow = new List<string>();
					{
						string code = "Get-Member -InputObject $args[0] -MemberType Properties -ErrorAction 0";
						foreach (PSObject o in A.Psf.InvokeCode(code, Value))
						{
							string name = o.Properties[Word.Name].Value.ToString();
							if (_ExcludeMemberRegex == null || !_ExcludeMemberRegex.IsMatch(name))
								membersToShow.Add(name);
						}
					}

					// now we are ready to process properties in their original order
					foreach (PSPropertyInfo pi in Value.Properties)
					{
						if (!membersToShow.Contains(pi.Name))
							continue;

						//! exceptions, e.g. exit code of running process
						object value;
						try
						{
							value = pi.Value;
						}
						catch (RuntimeException)
						{
							continue;
						}

						SetFile file = new SetFile()
						{
							Name = pi.Name,
							Data = pi
						};

						// base object
						PSObject asPSObject = value as PSObject;
						if (asPSObject != null)
							value = asPSObject.BaseObject;

						// value
						file.Description = Converter.FormatValue(value, A.Psf.Settings.FormatEnumerationLimit);

						// hidden
						if (_HideMemberRegex != null && _HideMemberRegex.IsMatch(file.Name))
							file.IsHidden = true;

						result.Add(file);
					}
				}
				else
				{
					string code;
					if (MemberMode == 1)
						code = "Get-Member -InputObject $args[0] -ErrorAction 0 -View All";
					else
						code = "Get-Member -InputObject $args[0] -ErrorAction 0 -View All -Static";
					foreach (PSObject o in A.Psf.InvokeCode(code, Value))
					{
						SetFile f = new SetFile();
						f.Name = o.Properties[Word.Name].Value.ToString();

						PSPropertyInfo pi;
						pi = o.Properties["MemberType"];
						f.Description = pi.Value.ToString();

						pi = o.Properties["Definition"];
						f.Description += " " + pi.Value.ToString();

						f.Data = o;
						result.Add(f);
					}
				}
			}
			catch (RuntimeException exception)
			{
				if (args.UI)
					A.Message(exception.Message);
			}
			
			return result;
		}
		///
		public override void ExportFile(ExportFileEventArgs args)
		{
			if (args == null) return;
			
			// info
			PSPropertyInfo pi = Cast<PSPropertyInfo>.From(args.File.Data);
			if (pi == null)
			{
				args.Result = JobResult.Ignore;
				return;
			}

			// text
			args.UseText = Converter.InfoToText(pi);
			if (args.UseText == null)
			{
				args.Result = JobResult.Ignore;
				return;
			}

			args.CanImport = pi.IsSettable;
		}
		///
		public override void ImportText(ImportTextEventArgs args)
		{
			if (args == null) return;
			
			PSPropertyInfo pi = Cast<PSPropertyInfo>.From(args.File.Data);
			if (pi == null)
				args.Result = JobResult.Ignore;
			else
				A.SetPropertyFromTextUI(Value, pi, args.Text.TrimEnd());
		}
		///
		public override void DeleteFiles(DeleteFilesEventArgs args)
		{
			if (args == null) return;

			// skip not default modes
			if (MemberMode != 0)
			{
				args.Result = JobResult.Ignore;
				return;
			}

			// ask
			if (args.UI && (Far.Net.Confirmations & FarConfirmations.Delete) != 0)
			{
				if (Far.Net.Message("Delete selected members", Res.Delete, MsgOptions.None, new string[] { Res.Delete, Res.Cancel }) != 0)
				{
					args.Result = JobResult.Ignore;
					return;
				}
			}

			try
			{
				int count1 = 0;
				foreach (var it in Value.Properties)
					++count1;

				foreach (FarFile file in args.Files)
				{
					PSPropertyInfo pi = file.Data as PSPropertyInfo;
					if (pi == null)
						continue;

					Value.Properties.Remove(pi.Name);
				}

				int count2 = 0;
				foreach (var it in Value.Properties)
					++count2;
				
				if (count1 - args.Files.Count != count2)
					args.Result = JobResult.Incomplete;
			}
			finally
			{
				// update always, some members can be deleted, don't leave them
				MemberPanel.WhenMemberChanged(Value); //????? will it be 2 times for THIS panel?
			}
		}
		///
		public override void CreateFile(CreateFileEventArgs args)
		{
			// it does all itself //?????
			args.Result = JobResult.Ignore;
			
			var panel = args.Panel as MemberPanel;
			if (panel == null)
				return;
			
			if (panel.Parent is DataPanel) //?????
				return;

			UI.NewValueDialog ui = new UI.NewValueDialog("New property");
			while (ui.Dialog.Show())
			{
				try
				{
					// get value, typed if needed
					object value = null;
					if (ui.Type.Text.Length == 0)
					{
						value = ui.Value.Text;
					}
					else
					{
						foreach (PSObject o in A.Psf.InvokeCode("[" + ui.Type.Text + "]$args[0]", ui.Value.Text))
						{
							value = o.BaseObject;
							break;
						}
					}

					// add member
					A.Psf.InvokeCode(
						"$args[0] | Add-Member -MemberType NoteProperty -Name $args[1] -Value $args[2] -Force -ErrorAction Stop",
						panel.Target, ui.Name.Text, value);

					// update this panel with name
					panel.UpdateRedraw(false, ui.Name.Text);

					// update that panel if the instance is the same
					MemberPanel pp2 = panel.TargetPanel as MemberPanel;
					if (pp2 != null && pp2.Target == panel.Target)
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
