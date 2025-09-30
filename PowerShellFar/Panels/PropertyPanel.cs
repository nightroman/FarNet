using FarNet;
using System.Management.Automation;

namespace PowerShellFar;

/// <summary>
/// Panel exploring PowerShell provider item properties.
/// </summary>
public sealed class PropertyPanel : ListPanel
{
	/// <summary>
	/// Gets the panel explorer.
	/// </summary>
	public new PropertyExplorer Explorer => (PropertyExplorer)base.Explorer;

	/// <summary>
	/// New property panel with the item property explorer.
	/// </summary>
	/// <param name="explorer">The panel explorer.</param>
	public PropertyPanel(PropertyExplorer explorer) : base(explorer)
	{
		Title = "Properties: " + Explorer.ItemPath;
		CurrentLocation = Explorer.ItemPath + ".*"; //??
		SortMode = PanelSortMode.Name;
	}

	internal sealed override PSObject Target
	{
		get { return A.Engine.InvokeProvider.Item.Get([Explorer.ItemPath], true, true)[0]; }
	}

	internal override void SetUserValue(PSPropertyInfo info, string? value)
	{
		try
		{
			A.SetPropertyValue(Explorer.ItemPath, info.Name, Converter.Parse(info, value!));
			WhenPropertyChanged(Explorer.ItemPath);
		}
		catch (RuntimeException ex)
		{
			A.MyMessage(ex.Message);
		}
	}

	/// <summary>
	/// Should be called when an item property is changed.
	/// </summary>
	internal static void WhenPropertyChanged(string itemPath)
	{
		foreach (var panel in Far.Api.Panels(typeof(PropertyPanel)).Cast<PropertyPanel>())
			if (panel.Explorer.ItemPath == itemPath)
				panel.UpdateRedraw(true);
	}

	///
	internal override void HelpMenuInitItems(HelpMenuItems items, PanelMenuEventArgs e)
	{
		items.Copy ??= new SetItem()
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

		items.Move ??= new SetItem()
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

	/// <inheritdoc/>
	public override void UICreateFile(CreateFileEventArgs args)
	{
		ArgumentNullException.ThrowIfNull(args);

		// call
		Explorer.CreateFile(args);
		if (args.Result != JobResult.Done)
			return;

		// update that panel if the path is the same
		if (TargetPanel is PropertyPanel that && that.Explorer.ItemPath == Explorer.ItemPath)
			that.UpdateRedraw(true);
	}
}
