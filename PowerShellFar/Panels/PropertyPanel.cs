
/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System.Management.Automation;
using FarNet;

namespace PowerShellFar
{
	/// <summary>
	/// Panel exploring PowerShell provider item properties.
	/// </summary>
	public sealed class PropertyPanel : ListPanel
	{
		///
		public new PropertyExplorer Explorer { get { return (PropertyExplorer)base.Explorer; } }
		/// <summary>
		/// New property panel with the item property explorer.
		/// </summary>
		public PropertyPanel(PropertyExplorer explorer)
			: base(explorer)
		{
			Title = "Properties: " + Explorer.ItemPath;
			CurrentLocation = Explorer.ItemPath + ".*"; //??
			SortMode = PanelSortMode.Name;
		}
		internal sealed override PSObject Target
		{
			get { return A.Psf.Engine.InvokeProvider.Item.Get(new string[] { Explorer.ItemPath }, true, true)[0]; }
		}
		internal override void SetUserValue(PSPropertyInfo info, string value)
		{
			try
			{
				A.SetPropertyValue(Explorer.ItemPath, info.Name, Converter.Parse(info, value));
				WhenPropertyChanged(Explorer.ItemPath);
			}
			catch (RuntimeException ex)
			{
				A.Message(ex.Message);
			}
		}
		/// <summary>
		/// Should be called when an item property is changed.
		/// </summary>
		internal static void WhenPropertyChanged(string itemPath)
		{
			foreach (PropertyPanel p in Far.Net.Panels(typeof(PropertyPanel)))
				if (p.Explorer.ItemPath == itemPath)
					p.UpdateRedraw(true);
		}
		///
		internal override void HelpMenuInitItems(HelpMenuItems items, PanelMenuEventArgs e)
		{
			if (items.Copy == null)
				items.Copy = new SetItem()
				{
					Text = "&Copy property(s)",
					Click = delegate { UICopyMove(false); }
				};

			if (items.CopyHere == null && Explorer.CanCloneFile)
				items.CopyHere = new SetItem()
				{
					Text = "Copy &here",
					Click = delegate { UIClone(); }
				};

			if (items.Move == null)
				items.Move = new SetItem()
				{
					Text = "&Move property(s)",
					Click = delegate { UICopyMove(true); }
				};

			if (items.Rename == null && e.CurrentFile != null && Explorer.CanRenameFile)
				items.Rename = new SetItem()
				{
					Text = "&Rename property",
					Click = delegate { UIRename(); }
				};

			if (items.Create == null && Explorer.CanCreateFile)
				items.Create = new SetItem()
				{
					Text = "&New property",
					Click = delegate { UICreate(); }
				};

			if (items.Delete == null && Explorer.CanDeleteFiles)
				items.Delete = new SetItem()
				{
					Text = "&Delete property(s)",
					Click = delegate { UIDelete(false); }
				};

			base.HelpMenuInitItems(items, e);
		}
		///
		public override void UICreateFile(CreateFileEventArgs args)
		{
			if (args == null) return;

			// call
			Explorer.CreateFile(args);
			if (args.Result != JobResult.Done)
				return;
			
			// update that panel if the path is the same
			PropertyPanel that = TargetPanel as PropertyPanel;
			if (that != null && that.Explorer.ItemPath == Explorer.ItemPath)
				that.UpdateRedraw(true);
		}
	}
}
