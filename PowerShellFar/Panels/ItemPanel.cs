
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

/*
_090929_061740 Far 2.0.1145 does not sync the current directory with the panel path.
	//! Empty provider path: open item panel, do 'cd \\DEV-RKUZ9400-SWK'
	//! PS shows an error and the location is set to invalid 'Microsoft.PowerShell.Core\FileSystem::'
	PathInfo pi = A.Psf.Engine.SessionState.Path.CurrentLocation;
	if (pi.ProviderPath.Length > 0 && !pi.ProviderPath.StartsWith(@"\\", StringComparison.Ordinal))
		Environment.CurrentDirectory = pi.ProviderPath;
*/

using FarNet;
using Microsoft.PowerShell.Commands;
using System;
using System.IO;

namespace PowerShellFar;

/// <summary>
/// PowerShell provider item panel.
/// </summary>
public sealed class ItemPanel : FormatPanel
{
	internal new ItemExplorer Explorer => (ItemExplorer)base.Explorer;

	internal ItemPanel(ItemExplorer explorer) : base(explorer)
	{
		// setup
		DoExplored(null);
	}

	/// <summary>
	/// Creates a panel for provider items at the given location.
	/// </summary>
	/// <param name="path">Path to start at.</param>
	public ItemPanel(string? path) : this(new ItemExplorer(new PathInfoEx(path)))
	{
		// current location, post the current name
		if (string.IsNullOrEmpty(path))
		{
			var file = Far.Api.Panel?.CurrentFile;
			if (file != null)
				PostName(file.Name);
		}
	}

	/// <summary>
	/// Creates a panel for provider items at the current location.
	/// </summary>
	public ItemPanel() : this((string?)null)
	{
	}

	/// <summary>
	/// Fixed drive name.
	/// </summary>
	/// <remarks>
	/// When a drive is fixed the panel is used for some custom operations on this drive items.
	/// </remarks>
	public string Drive
	{
		get => _Drive;
		set => _Drive = value ?? throw new ArgumentNullException(nameof(value));
	}
	string _Drive = string.Empty;

	bool UIAttributesCan()
	{
		return Drive.Length == 0 && My.ProviderInfoEx.HasProperty(Explorer.Provider);
	}

	internal override void UIAttributes()
	{
		if (Drive.Length > 0)
			return;

		// has property?
		if (!My.ProviderInfoEx.HasProperty(Explorer.Provider))
		{
			A.Message(Res.NotSupportedByProvider);
			return;
		}

		// open property panel
		var file = CurrentFile;
		new PropertyExplorer(file is null ? Explorer.Location : My.PathEx.Combine(Explorer.Location, file.Name)).OpenPanelChild(this);
	}

	internal override bool UICopyMoveCan(bool move)
	{
		if (base.UICopyMoveCan(move))
			return true;

		//! Actually e.g. functions can be copied, see UICopyHere
		return My.ProviderInfoEx.IsNavigation(Explorer.Provider);
	}

	/// <inheritdoc/>
	/// <remarks>
	/// The method updates internal data depending on the current explorer.
	/// </remarks>
	public override void UIExplorerEntered(ExplorerEnteredEventArgs args)
	{
		if (args is null)
			throw new ArgumentNullException(nameof(args));

		base.UIExplorerEntered(args);
		DoExplored((ItemExplorer)args.Explorer);
	}

	private void DoExplored(ItemExplorer? explorer)
	{
		var info1 = explorer?.Info();
		var info2 = Explorer.Info();

		// fixed drive?
		if (Drive.Length > 0 && !Kit.Equals(Drive, info2.DriveName))
			return;

		// customise if not yet
		if (Drive.Length == 0 && (explorer is null || info1!.Provider.ImplementingType != info2.Provider.ImplementingType))
		{
			if (string.IsNullOrEmpty(Drive))
			{
				Columns = null;
				ExcludeMemberPattern = null;

				if (A.Psf.Providers[info2.Provider.Name] is System.Collections.IDictionary options)
				{
					try
					{
						Converter.SetProperties(this, options, true);
					}
					catch (ArgumentException ex)
					{
						throw new InvalidDataException("Invalid settings for '" + info2.Provider.Name + "' provider: " + ex.Message);
					}
				}
			}
		}

		// Set-Location, the core remembers it for the drive, this is handy
		try
		{
			A.Psf.Engine.SessionState.Path.SetLocation(Kit.EscapeWildcard(Explorer.Location));
		}
		catch (Exception ex)
		{
			Log.TraceException(ex);
		}

		//! path is used for Set-Location on Invoking()
		Title = "Items: " + Explorer.Location;
		CurrentLocation = Explorer.Location; //????

		if (info2.Provider.ImplementingType == typeof(FileSystemProvider))
		{
			UseSortGroups = true;
			Highlighting = PanelHighlighting.Full;

			// _090929_061740 Before Far 2.0.1145 we used to sync the current directory to
			// the PS location. Now it is not needed because Far does not do that any more.
		}
		else
		{
			UseSortGroups = false;
			Highlighting = PanelHighlighting.Default;
		}
	}

	///
	internal override void HelpMenuInitItems(HelpMenuItems items, PanelMenuEventArgs e)
	{
		if (items.OpenFileAttributes is null && UIAttributesCan())
			items.OpenFileAttributes = new SetItem()
			{
				Text = "Item &properties",
				Click = delegate { UIAttributes(); }
			};

		if (items.OpenFile is null && e.CurrentFile != null)
			items.OpenFile = new SetItem()
			{
				Text = "Child items",
				Click = delegate { UIOpenFile(e.CurrentFile); }
			};

		if (items.Copy is null && UICopyMoveCan(false))
			items.Copy = new SetItem()
			{
				Text = "&Copy item(s)",
				Click = delegate { UICopyMove(false); }
			};

		if (items.CopyHere is null && e.CurrentFile != null)
			items.CopyHere = new SetItem()
			{
				Text = "Copy &here",
				Click = delegate { UIClone(); }
			};

		if (items.Move is null && UICopyMoveCan(true))
			items.Move = new SetItem()
			{
				Text = "&Move item(s)",
				Click = delegate { UICopyMove(true); }
			};

		if (items.Rename is null && e.CurrentFile != null && Explorer.CanRenameFile)
			items.Rename = new SetItem()
			{
				Text = "&Rename item",
				Click = delegate { UIRename(); }
			};

		items.Create ??= new SetItem()
			{
				Text = "&New item",
				Click = delegate { UICreate(); }
			};

		items.Delete ??= new SetItem()
			{
				Text = "&Delete item(s)",
				Click = delegate { UIDelete(false); }
			};

		base.HelpMenuInitItems(items, e);
	}

	/// <inheritdoc/>
	public override void UISetText(SetTextEventArgs args)
	{
		if (args is null)
			throw new ArgumentNullException(nameof(args));

		// call
		base.UISetText(args);
		if (args.Result != JobResult.Done)
			return;

		// update after edit
		if (0 != (args.Mode & ExplorerModes.Edit)) //????? in many cases it is not needed, think to avoid/do effectively
			UpdateRedraw(true);
	}

	/// <inheritdoc/>
	/// <remarks>
	/// The method updates the panel if its location is the same as that panel location.
	/// </remarks>
	public override void OnThatFileChanged(Panel that, EventArgs args)
	{
		if (that is not ItemPanel panel)
			return;

		if (panel.Explorer.Location != Explorer.Location)
			return;

		Update(true);
		Redraw();
	}
}
