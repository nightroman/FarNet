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
			Panel.Info.UseFilter = true;

			Panel.Info.UseAttributeHighlighting = true;
			Panel.Info.UseHighlighting = false;

			Panel.PuttingFiles += OnPuttingFiles;
		}

		///
		protected override string DefaultTitle { get { return "Objects"; } }

		/// <summary>
		/// Adds a single objects to the panel as it is.
		/// </summary>
		public void AddObject(object value)
		{
			var added = EnsureAddedValues();
			if (value != null)
				added.Add(PSObject.AsPSObject(value));
		}

		/// <summary>
		/// Adds objects to the panel.
		/// </summary>
		/// <param name="values">Objects represented by enumerable or a single object.</param>
		public void AddObjects(object values)
		{
			var added = EnsureAddedValues();

			if (values == null)
				return;

			IEnumerable enumerable = Cast<IEnumerable>.From(values);
			if (enumerable == null || enumerable is string)
			{
				added.Add(PSObject.AsPSObject(values));
			}
			else
			{
				int maximumFileCount = A.Psf.Settings.MaximumPanelFileCount;
				int fileCount = 0;
				foreach (object value in enumerable)
				{
					if (value == null)
						continue;

					// ask to cancel
					if (fileCount >= maximumFileCount && maximumFileCount > 0)
					{
						int res = ShowTooManyFiles(maximumFileCount, enumerable);
						
						// abort, show what we have got
						if (res == 0)
							break;
						
						if (res == 1)
							// retry with a larger number
							maximumFileCount *= 2;
						else
							// ignore the limit
							maximumFileCount = 0;
					}

					// add
					added.Add(PSObject.AsPSObject(value));
					++fileCount;
				}
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
			if (file == null)
				throw new ArgumentNullException("file");

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
				pi = psData.Properties[Word.Name];
				if (pi != null && pi.Value != null)
				{
					var values = A.Psf.InvokeCode("Get-WmiObject -Class $args[0] -ErrorAction SilentlyContinue", pi.Value.ToString());
					ObjectPanel op = new ObjectPanel();
					op.AddObjects(values);
					op.ShowAsChild(this);
					return;
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

		Collection<PSObject> _Values_;
		Collection<PSObject> AddedValues
		{
			get { return _Values_; }
		}
		void DropAddedValues()
		{
			_Values_ = null;
		}
		Collection<PSObject> EnsureAddedValues()
		{
			return _Values_ ?? (_Values_ = new Collection<PSObject>());
		}

		internal override object GetData()
		{
			try
			{
				//???? it works but looks like a hack
				if (UserWants != UserAction.CtrlR && AddedValues == null && (Map != null || Panel.Files.Count > 0 && Panel.Files[0] is SetFile))
					return Panel.Files;

				if (Map == null || Columns == null)
				{
					if (Panel.Files.Count == 0)
						return AddedValues ?? new Collection<PSObject>();

					var result = new Collection<PSObject>();
					foreach (FarFile file in Panel.Files)
						result.Add(PSObject.AsPSObject(file.Data));
					if (AddedValues != null)
						foreach (PSObject value in AddedValues)
							result.Add(value);

					return result;
				}

				// _100330_191639
				if (AddedValues == null)
					return Panel.Files;

				var files = new List<FarFile>(AddedValues.Count);
				foreach (PSObject value in AddedValues)
					files.Add(new MapFile(value, Map));

				return files;
			}
			finally
			{
				DropAddedValues();
			}
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

		/// <summary>
		/// Files data: .. is excluded; same count and order.
		/// </summary>
		IList<object> CollectData()
		{
			var r = new List<object>();
			r.Capacity = Panel.Files.Count;
			foreach (FarFile f in Panel.Files)
				if (f.Data != null)
					r.Add(f.Data);
			return r;
		}

		static int ShowTooManyFiles(int maximumFileCount, IEnumerable enumerable)
		{
			ICollection collection = enumerable as ICollection;
			string message = collection == null ?
				Invariant.Format("There are more than {0} panel files.", maximumFileCount) :
				Invariant.Format("There are {0} panel files, the limit is {1}.", collection.Count, maximumFileCount);

			return Far.Net.Message(message, "$Psf.Settings.MaximumPanelFileCount", MsgOptions.AbortRetryIgnore);
		}

	}
}
