/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using FarNet;

namespace PowerShellFar
{
	/// <summary>
	/// Panel exploring any .NET objects.
	/// </summary>
	public class ObjectPanel : FormatPanel
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public ObjectPanel()
		{
			Panel.Info.CurrentDirectory = "*";
			Panel.Info.StartSortMode = PanelSortMode.Unsorted;
			Panel.Info.UseAttributeHighlighting = true;
			Panel.Info.UseFilter = true;
			Panel.Info.UseHighlighting = true;

			Panel.PuttingFiles += OnPuttingFiles;
		}

		///
		protected override string DefaultTitle { get { return "Objects"; } }

		/// <summary>
		/// Adds a single objects to the panel as it is.
		/// </summary>
		public void AddObject(object value)
		{
			if (value != null)
				Values.Add(PSObject.AsPSObject(value));
		}

		/// <summary>
		/// Adds objects to the panel.
		/// </summary>
		/// <param name="values">Objects represented by enumerable or a single object.</param>
		public void AddObjects(object values)
		{
			if (values == null)
				return;

			IEnumerable ie = Cast<IEnumerable>.From(values);
			if (ie == null || ie is string)
			{
				Values.Add(PSObject.AsPSObject(values));
			}
			else
			{
				foreach (object value in ie)
					if (value != null)
						Values.Add(PSObject.AsPSObject(value));
			}
		}

		internal override void DeleteFiles(IList<FarFile> files, bool shift)
		{
			if ((Far.Net.Confirmations & FarConfirmations.Delete) != 0)
			{
				if (Far.Net.Message("Remove object(s) from the panel?", Res.Remove, MsgOptions.None, new string[] { Res.Remove, Res.Cancel }) != 0)
					return;
			}

			foreach (FarFile f in files)
				Panel.Files.Remove(f);
		}

		//! Update is called by the Far core.
		void OnPuttingFiles(object sender, PuttingFilesEventArgs e)
		{
			AddObjects(A.Psf.InvokeCode("Get-FarItem -Selected"));
		}

		/// <summary>
		/// Opens a member panel or another panel.
		/// </summary>
		public override void OpenFile(FarFile file)
		{
			PSObject psData = PSObject.AsPSObject(file.Data);

			// case: linear type: do not enter, there is no much sense
			if (Converter.IsLinearType(psData.BaseObject.GetType()))
				return;

			// case: enumerable (string is excluded by linear type case)
			IEnumerable ie = Cast<IEnumerable>.From(file.Data);
			if (ie != null)
			{
				ObjectPanel op = new ObjectPanel();
				op.AddObjects(ie);
				op.ShowAsChild(this);
				return;
			}

			// case: group
			PSPropertyInfo pi = psData.Properties["Group"];
			if (pi != null && pi.Value is IEnumerable && !(pi.Value is string))
			{
				ObjectPanel op = new ObjectPanel();
				op.AddObjects(pi.Value as IEnumerable);
				op.ShowAsChild(this);
				return;
			}

			// case: ManagementClass
			if (psData.BaseObject.GetType().FullName == "System.Management.ManagementClass")
			{
				pi = psData.Properties["Name"];
				if (pi != null && pi.Value != null)
				{
					using (PowerShell p = A.Psf.CreatePipeline())
					{
						Command c = new Command("Get-WmiObject");
						c.Parameters.Add("Class", pi.Value.ToString());
						c.Parameters.Add(Prm.EASilentlyContinue);
						p.Commands.AddCommand(c);
						Collection<PSObject> oo = p.Invoke();

						ObjectPanel op = new ObjectPanel();
						op.AddObjects(oo);
						op.ShowAsChild(this);
						return;
					}
				}
			}

			// open members
			OpenFileMembers(file);
		}

		/// <summary>
		/// Exports objects to Clixml file.
		/// </summary>
		public override bool SaveData()
		{
			UI.ExportDialog.ExportClixml(CollectData(), Panel.ActivePath);
			return true;
		}

		/// <summary>
		/// Keeps current panel file.
		/// </summary>
		internal override void SaveState()
		{
			FarFile f = Panel.CurrentFile;
			if (f != null)
				Panel.PostFile(f);
		}

		// _100227_073909
		internal override bool UICopyMove(bool move)
		{
			ObjectPanel that = AnotherPanel as ObjectPanel;
			if (that == null)
				return false;

			that.AddObjects(SelectedItems);
			that.UpdateRedraw(true);

			if (move)
				UIDelete(false);

			return true;
		}

		// Prompts a user to enter a command that gets new panel objects
		internal override void UICreate()
		{
			// prompt for a command
			string code = Far.Net.MacroState == FarMacroState.None ? A.Psf.InputCode() : Far.Net.Input(null);
			if (string.IsNullOrEmpty(code))
				return;

			// invoke the command
			Collection<PSObject> values = A.Psf.InvokeCode(code);
			if (values.Count == 0)
				return;

			// add the objects
			AddObjects(values);

			// post the first object and update
			Panel.PostData(values[0]);
			UpdateRedraw(false);
		}

		readonly Collection<PSObject> _Values = new Collection<PSObject>();
		internal Collection<PSObject> Values { get { return _Values; } }

		internal override object GetData()
		{
			//???? mb it works but looks like a hack
			if (UserWants != UserAction.CtrlR && Values.Count == 0 && (Map != null || Panel.Files.Count > 0 && Panel.Files[0] is SetFile))
				return Panel.Files;

			if (Map == null || Columns == null)
			{
				if (Panel.Files.Count == 0)
					return Values;

				var result = new Collection<PSObject>();
				foreach (FarFile file in Panel.Files)
					result.Add(PSObject.AsPSObject(file.Data));
				foreach (PSObject value in Values)
					result.Add(value);

				Values.Clear();
				return result;
			}

			var files = new List<FarFile>(Values.Count);
			foreach (PSObject value in Values)
				files.Add(new MapFile(value, Map));

			Values.Clear();
			return files;
		}

		/// <summary>
		/// Sets file name if any suitable exists.
		/// </summary>
		static void SetFileName(FarFile file)
		{
			// case: try to get display name
			PSObject data = PSObject.AsPSObject(file.Data);
			PSPropertyInfo pi = A.FindDisplayProperty(data);
			if (pi != null)
			{
				file.Name = pi.Value == null ? "<null>" : pi.Value.ToString();
				return;
			}

			// other: use ToString(), but skip too verbose PSCustomObject
			if (!(data.BaseObject is PSCustomObject))
				file.Name = data.ToString();
		}

		///
		internal override void HelpMenuInitItems(HelpMenuItems items, PanelMenuEventArgs e)
		{
			if (items.Save == null)
				items.Save = new SetItem()
				{
					Text = "Export .clixml...",
					Click = delegate { SaveData(); }
				};

			base.HelpMenuInitItems(items, e);
		}
	}
}
