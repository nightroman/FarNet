
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2013 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar.UI
{
	static class ActorMenu
	{
		static List<KeyValuePair<FarItem, ModuleToolOptions>> _pendingItems;

		static IMenu _menuDialog;
		static IMenu _menuEditor;
		static IMenu _menuPanels;
		static IMenu _menuViewer;

		static bool _toSeparate;
		static bool _menuDialogSeparated;
		static bool _menuEditorSeparated;
		static bool _menuPanelsSeparated;
		static bool _menuViewerSeparated;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "TabExpansion")]
		static void Create()
		{
			Debug.Assert(_menuEditor == null);

			_menuDialog = Far.Api.CreateMenu();
			_menuEditor = Far.Api.CreateMenu();
			_menuPanels = Far.Api.CreateMenu();
			_menuViewer = Far.Api.CreateMenu();

			_menuDialog.Title = Res.Me;
			_menuEditor.Title = Res.Me;
			_menuPanels.Title = Res.Me;
			_menuViewer.Title = Res.Me;

			string helpTopic = Far.Api.GetHelpTopic("MenuCommands");
			_menuDialog.HelpTopic = helpTopic;
			_menuEditor.HelpTopic = helpTopic;
			_menuPanels.HelpTopic = helpTopic;
			_menuViewer.HelpTopic = helpTopic;

			AddTool(Res.MenuInvokeInputCode, delegate { A.Psf.InvokeInputCode(); }, ModuleToolOptions.F11Menus);
			AddTool(Res.MenuInvokeSelectedCode, delegate { A.Psf.InvokeSelectedCode(); }, ModuleToolOptions.Editor | ModuleToolOptions.Panels | ModuleToolOptions.Dialog);
			AddTool(Res.MenuBackgroundJobs, delegate { A.Psf.ShowJobs(); }, ModuleToolOptions.F11Menus);
			AddTool(Res.MenuCommandHistory, delegate { A.Psf.ShowHistory(); }, ModuleToolOptions.F11Menus);
			AddTool(Res.MenuEditorConsole, delegate { A.Psf.ShowConsole(OpenMode.None); }, ModuleToolOptions.F11Menus);
			AddTool(Res.MenuPowerPanel, delegate { A.Psf.ShowPanel(); }, ModuleToolOptions.F11Menus);
			AddTool(Res.MenuTabExpansion, delegate { A.Psf.ExpandCode(null); }, ModuleToolOptions.Editor | ModuleToolOptions.Panels | ModuleToolOptions.Dialog);
			AddTool(Res.MenuSnapin, delegate { A.Psf.ShowModules(); }, ModuleToolOptions.F11Menus);
			AddTool(Res.MenuDebugger, delegate { A.Psf.ShowDebugger(); }, ModuleToolOptions.F11Menus);
			AddTool(Res.MenuError, delegate { A.Psf.ShowErrors(); }, ModuleToolOptions.F11Menus);
			AddTool(Res.MenuHelp, delegate { A.Psf.ShowHelp(); }, ModuleToolOptions.F11Menus);

			_toSeparate = true;
			if (_pendingItems != null)
			{
				foreach (KeyValuePair<FarItem, ModuleToolOptions> kv in _pendingItems)
					AddItem(kv.Key, kv.Value);

				_pendingItems = null;
			}
		}

		internal static void Destroy()
		{
			_pendingItems = null;

			_toSeparate = false;
			_menuDialogSeparated = false;
			_menuEditorSeparated = false;
			_menuPanelsSeparated = false;
			_menuViewerSeparated = false;

			if (_menuDialog == null)
				return;

			_menuDialog.Unlock();
			_menuEditor.Unlock();
			_menuPanels.Unlock();
			_menuViewer.Unlock();

			_menuDialog = null;
			_menuEditor = null;
			_menuPanels = null;
			_menuViewer = null;
		}

		internal static void Show(object sender, ModuleToolEventArgs e)
		{
			//! NOTE: do sync for item handlers
			string currentDirectory = A.Psf.SyncPaths();
			try
			{
				// create once
				if (_menuDialog == null)
					Create();

				IMenu menu;

				switch (e.From)
				{
					case ModuleToolOptions.Dialog: menu = _menuDialog; break;
					case ModuleToolOptions.Editor: menu = _menuEditor; break;
					case ModuleToolOptions.Panels: menu = _menuPanels; break;
					case ModuleToolOptions.Viewer: menu = _menuViewer; break;
					default: return;
				}

				// reset, lock and show
				menu.Selected = -1;
				menu.Lock();
				menu.Show();
			}
			catch (PipelineStoppedException)
			{
				// Ignore this exception, a user has halted, e.g. on menu action:
				// Remove-Variable Far -Confirm
				// -- Confirm dialog -- press [Esc] -- click [Halt] -- we are in here
			}
			finally
			{
				A.SetCurrentDirectoryFinally(currentDirectory);
			}
		}

		static FarItem NewItem(string text, EventHandler<MenuEventArgs> click)
		{
			SetItem item = new SetItem() { Text = text };
			if (click == null)
				item.IsSeparator = true;
			else
				item.Click = click;
			return item;
		}

		static void AddTool(string text, EventHandler<MenuEventArgs> click, ModuleToolOptions from)
		{
			AddItem(NewItem(text, click), from);
		}

		static void AddItem(FarItem item, ModuleToolOptions from)
		{
			if (from == ModuleToolOptions.None)
				from = ModuleToolOptions.F11Menus;

			if (0 < (from & ModuleToolOptions.Dialog))
			{
				if (_toSeparate && !_menuDialogSeparated)
				{
					_menuDialogSeparated = true;
					_menuDialog.Items.Add(NewItem("Actions", null));
				}
				_menuDialog.Items.Add(item);
			}

			if (0 < (from & ModuleToolOptions.Editor))
			{
				if (_toSeparate && !_menuEditorSeparated)
				{
					_menuEditorSeparated = true;
					_menuEditor.Items.Add(NewItem("Actions", null));
				}
				_menuEditor.Items.Add(item);
			}

			if (0 < (from & ModuleToolOptions.Panels))
			{
				if (_toSeparate && !_menuPanelsSeparated)
				{
					_menuPanelsSeparated = true;
					_menuPanels.Items.Add(NewItem("Actions", null));
				}
				_menuPanels.Items.Add(item);
			}

			if (0 < (from & ModuleToolOptions.Viewer))
			{
				if (_toSeparate && !_menuViewerSeparated)
				{
					_menuViewerSeparated = true;
					_menuViewer.Items.Add(NewItem("Actions", null));
				}
				_menuViewer.Items.Add(item);
			}
		}

		public static void AddUserTool(string text, EventHandler<MenuEventArgs> click, ModuleToolOptions from)
		{
			// case: just collect, e.g. called from profile
			if (_menuDialog == null)
			{
				if (_pendingItems == null)
					_pendingItems = new List<KeyValuePair<FarItem, ModuleToolOptions>>();

				_pendingItems.Add(new KeyValuePair<FarItem, ModuleToolOptions>(NewItem(text, click), from));
				return;
			}

			// unlock
			_menuDialog.Unlock();
			_menuEditor.Unlock();
			_menuPanels.Unlock();
			_menuViewer.Unlock();

			// add
			AddTool(text, click, from);
		}

	}
}
