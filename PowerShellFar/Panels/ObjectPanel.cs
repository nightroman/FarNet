/*
PowerShellFar plugin for Far Manager
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
			Panel.Info.Title = "Objects";
			Panel.Info.UseAttributeHighlighting = true;
			Panel.Info.UseFilter = true;
			Panel.Info.UseHighlighting = true;

			Panel.PuttingFiles += OnPuttingFiles;
		}

		internal int AddObjectsWorker(object values)
		{
			int r = 0;
			IEnumerable ie = Cast<IEnumerable>.From(values);
			if (ie == null || ie is string)
			{
				Panel.Files.Add(NewFile(values));
				++r;
			}
			else
			{
				foreach (object value in ie)
				{
					if (value != null)
					{
						Panel.Files.Add(NewFile(value));
						++r;
					}
				}
			}
			return r;
		}

		/// <summary>
		/// Adds a single objects to the panel as it is.
		/// </summary>
		public void AddObject(object value)
		{
			if (value != null)
				Panel.Files.Add(NewFile(value));
		}

		/// <summary>
		/// Adds objects to the panel.
		/// </summary>
		/// <param name="values">Objects represented by enumerable or a single object.</param>
		public void AddObjects(object values)
		{
			// >: New-FarObjectPanel # example
			if (values == null)
				return;

			// just add
			bool toUpdate = AddObjectsWorker(values) > 0;

			// update?
			if (toUpdate && Panel.IsOpened && !IsGettingData)
				UpdateRedraw(true);
		}

		internal override bool CanClose()
		{
			if (Child != null)
				return true;
			
			Trace.Assert(Panel.IsOpened);

			if (Parent != null || Panel.Files.Count < 1)
				return true;

			switch (A.Far.Message("How would you like to continue?", "Confirm", MsgOptions.None, new string[] { "Close", "Clear", Res.Cancel }))
			{
				case 0:
					return true;
				case 1:
					Panel.Files.Clear();
					UpdateRedraw(false, 0, 0);
					return false;
				default:
					return false;
			}
		}

		internal override void DeleteFiles(IList<FarFile> files, bool shift)
		{
			if ((A.Far.Confirmations & FarConfirmations.Delete) != 0)
			{
				if (A.Far.Message("Remove object(s) from the panel?", Res.Remove, MsgOptions.None, new string[] { Res.Remove, Res.Cancel }) != 0)
					return;
			}

			foreach (FarFile f in files)
				Panel.Files.Remove(f);
		}

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

		internal override bool UICopyMove(bool move)
		{
			ObjectPanel op = AnotherPanel as ObjectPanel;
			if (op == null)
				return false;

			op.AddObjects(SelectedItems);
			if (move)
			{
				UIDelete(false);
				return true;
			}
			UpdateRedraw(false);
			return true;
		}

		internal override void UICreate()
		{
			PSObject data = new PSObject();
			FarFile file = NewFile(data);
			Panel.Files.Add(file);

			Panel.PostFile(file);
			UpdateRedraw(false);

			MemberPanel child = new MemberPanel(data);
			child.ShowAsChild(this);
			child.UICreate();
		}

		/// <summary>
		/// Updates <see cref="FarFile.Data"/> and <see cref="FarFile.Name"/>
		/// and returns false or updates everything itself and returns true.
		/// </summary>
		internal override bool OnGettingData()
		{
			if (Map != null)
				return true;

			foreach (FarFile f in Panel.Files)
			{
				if (f.Data != null)
					SetFileName(f);
			}

			return false;
		}

		/// <summary>
		/// Creates a new file specific for this panel and its settings.
		/// </summary>
		/// <param name="value">An object to be attached to this file.</param>
		/// <returns>New file. You should use it at once for this panel only.</returns>
		/// <remarks>
		/// This method may be used for example in the script set by <see cref="UserPanel.SetGetFiles"/>.
		/// </remarks>
		public FarFile NewFile(object value)
		{
			if (Map == null)
			{
				FormattedObjectFile r = new FormattedObjectFile();
				r.Data = PSObject.AsPSObject(value);
				return r;
			}
			else
			{
				return new MappedObjectFile(PSObject.AsPSObject(value), Map);
			}
		}

		///
		internal override void HelpMenuInitItems(HelpMenuItems items, PanelMenuEventArgs e)
		{
			if (items.Save == null)
			{
				items.Save = new SetItem();
				items.Save.Text = "Export .clixml...";
				items.Save.Click = delegate { SaveData(); };
			}

			base.HelpMenuInitItems(items, e);
		}
	}
}
