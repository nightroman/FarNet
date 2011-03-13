
/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using FarNet;
using Microsoft.PowerShell.Commands;

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
			Title = "Properties: " + Explorer.ThePath;
			CurrentLocation = Explorer.ThePath + ".*"; //??
			SortMode = PanelSortMode.Name;
		}
		internal sealed override PSObject Target
		{
			get { return Explorer.TheItem; }
		}
		internal override void SetUserValue(PSPropertyInfo info, string value)
		{
			try
			{
				A.SetPropertyValue(Explorer.ThePath, info.Name, Converter.Parse(info, value));
				WhenPropertyChanged(Explorer.ThePath);
			}
			catch (RuntimeException ex)
			{
				A.Message(ex.Message);
			}
		}
		internal override void UICopyHere()
		{
			if (!My.ProviderInfoEx.HasDynamicProperty(Explorer.Provider))
			{
				A.Message(Res.NotSupportedByProvider);
				return;
			}

			FarFile f = CurrentFile;
			if (f == null)
				return;
			string name = f.Name;

			IInputBox ib = Far.Net.CreateInputBox();
			ib.Title = "Copy";
			ib.Prompt = "New name";
			ib.History = "Copy";
			ib.Text = name;
			if (!ib.Show() || ib.Text == name)
				return;

			string src = Kit.EscapeWildcard(Explorer.ThePath);
			A.Psf.Engine.InvokeProvider.Property.Copy(src, name, src, ib.Text);

			UpdateRedraw(false, ib.Text);
		}
		internal override void UIRename()
		{
			FarFile f = CurrentFile;
			if (f == null)
				return;
			string name = f.Name;

			//! Registry: workaround: (default)
			if (Kit.Equals(name, "(default)") && Explorer.Provider.ImplementingType == typeof(RegistryProvider))
			{
				A.Message("Cannot rename this property.");
				return;
			}

			IInputBox ib = Far.Net.CreateInputBox();
			ib.Title = "Rename";
			ib.Prompt = "New name";
			ib.History = "Copy";
			ib.Text = name;
			if (!ib.Show() || ib.Text == name)
				return;

			using (PowerShell p = A.Psf.CreatePipeline())
			{
				Command c = new Command("Rename-ItemProperty");
				c.Parameters.Add(new CommandParameter("LiteralPath", Explorer.ThePath));
				c.Parameters.Add(new CommandParameter(Word.Name, name));
				c.Parameters.Add(new CommandParameter("NewName", ib.Text));
				c.Parameters.Add(Prm.Force);
				c.Parameters.Add(Prm.ErrorAction, ActionPreference.Continue);
				p.Commands.AddCommand(c);
				p.Invoke();
				if (!A.ShowError(p))
					UpdateRedraw(true, ib.Text);
			}
		}
		/// <summary>
		/// Should be called when an item property is changed.
		/// </summary>
		internal static void WhenPropertyChanged(string itemPath)
		{
			foreach (PropertyPanel p in Far.Net.Panels(typeof(PropertyPanel)))
				if (p.Explorer.ThePath == itemPath)
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

			if (items.CopyHere == null)
				items.CopyHere = new SetItem()
				{
					Text = "Copy &here",
					Click = delegate { UICopyHere(); }
				};

			if (items.Move == null)
				items.Move = new SetItem()
				{
					Text = "&Move property(s)",
					Click = delegate { UICopyMove(true); }
				};

			if (items.Rename == null)
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
	}
}
