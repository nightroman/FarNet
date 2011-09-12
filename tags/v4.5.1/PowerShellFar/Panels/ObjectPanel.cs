
/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System.Collections.Generic;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar
{
	/// <summary>
	/// .NET objects panel.
	/// </summary>
	public class ObjectPanel : FormatPanel
	{
		///
		public new ObjectExplorer Explorer { get { return (ObjectExplorer)base.Explorer; } }
		///
		public ObjectPanel(ObjectExplorer explorer)
			: base(explorer)
		{
			CurrentLocation = "*";
			SortMode = PanelSortMode.Unsorted;
			UseFilter = true;
		}
		///
		public ObjectPanel() : this(new ObjectExplorer()) { }
		///
		protected override string DefaultTitle { get { return "Objects"; } }
		/// <summary>
		/// Adds a single objects to the panel as it is.
		/// </summary>
		public void AddObject(object value)
		{
			if (value != null)
				Explorer.AddedValues.Add(PSObject.AsPSObject(value));
		}
		/// <summary>
		/// Adds objects to the panel.
		/// </summary>
		/// <param name="values">Objects represented by enumerable or a single object.</param>
		public void AddObjects(object values) { Explorer.AddObjects(values); }
		/// <summary>
		/// Exports objects to Clixml file.
		/// </summary>
		public override bool SaveData()
		{
			UI.ExportDialog.ExportClixml(CollectData(), StartDirectory);
			return true;
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
		/// Files data.
		/// </summary>
		IList<object> CollectData()
		{
			var Files = Explorer.Cache;
			var r = new List<object>();
			r.Capacity = Files.Count;
			foreach (FarFile f in Files)
				if (f.Data != null)
					r.Add(f.Data);
			return r;
		}
	}
}
