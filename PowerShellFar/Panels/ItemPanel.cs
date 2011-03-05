
/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

/*
_090929_061740 Far 2.0.1145 does not sync the current directory with the panel path.
	//! Empty provider path: open item panel, do 'cd \\DEV-RKUZ9400-SWK'
	//! PS shows an error and the location is set to invalid 'Microsoft.PowerShell.Core\FileSystem::'
	PathInfo pi = A.Psf.Engine.SessionState.Path.CurrentLocation;
	if (pi.ProviderPath.Length > 0 && !pi.ProviderPath.StartsWith(@"\\", StringComparison.Ordinal))
		Environment.CurrentDirectory = pi.ProviderPath;
*/

using System;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using FarNet;
using Microsoft.PowerShell.Commands;

namespace PowerShellFar
{
	/// <summary>
	/// PowerShell provider item panel.
	/// </summary>
	public sealed class ItemPanel : FormatPanel
	{
		internal new ItemExplorer Explorer { get { return (ItemExplorer)base.Explorer; } }
		/// <summary>
		/// Creates a panel for provider items at the given location.
		/// </summary>
		/// <param name="path">Path to start at.</param>
		public ItemPanel(string path)
			: base(new ItemExplorer(path))
		{
			Explorer.Panel = this;

			// setup
			ChangeLocation(null, Explorer.ThePath);

			// current location, post the current name
			if (string.IsNullOrEmpty(path))
			{
				FarFile file = Far.Net.Panel.CurrentFile;
				if (file != null)
					PostName(file.Name);
			}
		}
		/// <summary>
		/// Creates a panel for provider items at the current location.
		/// </summary>
		public ItemPanel() : this(null) { }
		/// <summary>
		/// It is fired when items are changed.
		/// </summary>
		public event EventHandler ItemsChanged;
		internal void DoItemsChanged()
		{
			if (ItemsChanged != null)
				ItemsChanged(this, null);
		}
		/// <summary>
		/// Fixed drive name.
		/// </summary>
		/// <remarks>
		/// When a drive is fixed the panel is used for some custom operations on this drive items.
		/// </remarks>
		public string Drive
		{
			get { return _Drive; }
			set
			{
				if (value == null)
					throw new ArgumentNullException("value");

				_Drive = value;
			}
		}
		string _Drive = string.Empty;
		internal override object GetData()
		{
			// get child items for the panel location
			return A.GetChildItems(Explorer.ThePath.Path);
		}
		bool UIAttributesCan()
		{
			return Drive.Length == 0 && My.ProviderInfoEx.HasProperty(Explorer.ThePath.Provider);
		}
		internal override void UIAttributes()
		{
			if (Drive.Length > 0)
				return;

			// has property?
			if (!My.ProviderInfoEx.HasProperty(Explorer.ThePath.Provider))
			{
				A.Message(Res.NotSupportedByProvider);
				return;
			}

			// open property panel
			FarFile file = CurrentFile;
			(new PropertyExplorer(file == null ? Explorer.ThePath.Path : My.PathEx.Combine(Explorer.ThePath.Path, file.Name))).OpenPanelChild(this);
		}
		internal override void UICopyHere()
		{
			FarFile file = CurrentFile;
			if (file == null)
				return;
			string name = file.Name;

			// ask
			IInputBox ib = Far.Net.CreateInputBox();
			ib.Title = "Copy";
			ib.Prompt = "New name";
			ib.History = "Copy";
			ib.Text = name;
			if (!ib.Show() || ib.Text == name)
				return;

			// copy
			string source = Kit.EscapeWildcard(My.PathEx.Combine(Explorer.ThePath.Path, name));
			string target = My.PathEx.Combine(Explorer.ThePath.Path, ib.Text);
			A.Psf.Engine.InvokeProvider.Item.Copy(source, target, false, CopyContainers.CopyTargetContainer);

			// fire
			if (ItemsChanged != null)
				ItemsChanged(this, null);

			UpdateRedraw(false, ib.Text);
		}
		internal override bool UICopyMoveCan(bool move)
		{
			if (base.UICopyMoveCan(move))
				return true;

			//! Actually e.g. functions can be copied, see UICopyHere
			return My.ProviderInfoEx.IsNavigation(Explorer.ThePath.Provider);
		}
		internal override void UIRename()
		{
			FarFile f = CurrentFile;
			if (f == null)
				return;
			string name = f.Name;

			IInputBox ib = Far.Net.CreateInputBox();
			ib.Title = "Rename";
			ib.Prompt = "New name";
			ib.History = "Copy";
			ib.Text = name;
			if (!ib.Show() || ib.Text == name)
				return;

			// workaround; Rename-Item has no -LiteralPath; e.g. z`z[z.txt is a big problem
			string src = Kit.EscapeWildcard(My.PathEx.Combine(Explorer.ThePath.Path, name));
			A.Psf.Engine.InvokeProvider.Item.Rename(src, ib.Text);

			// fire
			if (ItemsChanged != null)
				ItemsChanged(this, null);

			UpdateRedraw(false, ib.Text);
		}
		/// <summary>
		/// Sets new location
		/// </summary>
		internal void ChangeLocation(PowerPath location1, PowerPath location2)
		{
			// fixed drive?
			if (Drive.Length > 0 && !Kit.Equals(Drive, location2.DriveName))
				return;

			// customise if not yet
			if (Drive.Length == 0 && (location1 == null || location1.Provider.ImplementingType != location2.Provider.ImplementingType))
			{
				if (string.IsNullOrEmpty(Drive))
				{
					Columns = null;
					ExcludeMemberPattern = null;

					System.Collections.IDictionary options = A.Psf.Providers[location2.Provider.Name] as System.Collections.IDictionary;
					if (options != null)
					{
						try
						{
							Converter.SetProperties(this, options, true);
						}
						catch (ArgumentException ex)
						{
							throw new InvalidDataException("Invalid settings for '" + location2.Provider.Name + "' provider: " + ex.Message);
						}
					}
				}
			}

			//! path is used for Set-Location on Invoking()
			Title = "Items: " + location2.Path;
			CurrentLocation = location2.Path; //????

			if (location2.Provider.ImplementingType == typeof(FileSystemProvider))
			{
				UseFilter = true;
				UseSortGroups = true;
				Highlighting = PanelHighlighting.Full;

				// _090929_061740 Before Far 2.0.1145 we used to sync the current directory to
				// the PS location. Now it is not needed because Far does not do that any more.
			}
			else
			{
				UseFilter = true;
				UseSortGroups = false;
				Highlighting = PanelHighlighting.Default;
			}
		}
		///
		internal override void HelpMenuInitItems(HelpMenuItems items, PanelMenuEventArgs e)
		{
			if (items.OpenFileAttributes == null && UIAttributesCan())
				items.OpenFileAttributes = new SetItem()
				{
					Text = "Item &properties",
					Click = delegate { UIAttributes(); }
				};

			if (items.OpenFile == null)
				items.OpenFile = new SetItem()
				{
					Text = "Child items",
					Click = delegate { UIOpenFile(CurrentFile); }
				};

			if (items.Copy == null && UICopyMoveCan(false))
				items.Copy = new SetItem()
				{
					Text = "&Copy item(s)",
					Click = delegate { UICopyMove(false); }
				};

			if (items.CopyHere == null && e.CurrentFile != null)
				items.CopyHere = new SetItem()
				{
					Text = "Copy &here",
					Click = delegate { UICopyHere(); }
				};

			if (items.Move == null && UICopyMoveCan(true))
				items.Move = new SetItem()
				{
					Text = "&Move item(s)",
					Click = delegate { UICopyMove(true); }
				};

			if (items.Rename == null)
				items.Rename = new SetItem()
				{
					Text = "&Rename item",
					Click = delegate { UIRename(); }
				};

			if (items.Create == null)
				items.Create = new SetItem()
				{
					Text = "&New item",
					Click = delegate { UICreate(); }
				};

			if (items.Delete == null)
				items.Delete = new SetItem()
				{
					Text = "&Delete item(s)",
					Click = delegate { UIDelete(false); }
				};

			base.HelpMenuInitItems(items, e);
		}
	}
}
