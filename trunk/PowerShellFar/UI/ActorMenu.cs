/*
PowerShellFar plugin for Far Manager
Copyright (C) 2006-2009 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using FarNet;

namespace PowerShellFar.UI
{
	static class ActorMenu
	{
		static List<KeyValuePair<FarItem, ToolOptions>> _pendingItems;

		static IMenu _menuDialog;
		static IMenu _menuEditor;
		static IMenu _menuPanels;
		static IMenu _menuViewer;

		static bool _toSeparate;
		static bool _menuDialogSeparated;
		static bool _menuEditorSeparated;
		static bool _menuPanelsSeparated;
		static bool _menuViewerSeparated;

		static void Create()
		{
			Debug.Assert(_menuEditor == null);

			_menuDialog = A.Far.CreateMenu();
			_menuEditor = A.Far.CreateMenu();
			_menuPanels = A.Far.CreateMenu();
			_menuViewer = A.Far.CreateMenu();

			_menuDialog.Title = Res.Name;
			_menuEditor.Title = Res.Name;
			_menuPanels.Title = Res.Name;
			_menuViewer.Title = Res.Name;

			string helpTopic = A.Psf.HelpTopic + "MenuCommands";
			_menuDialog.HelpTopic = helpTopic;
			_menuEditor.HelpTopic = helpTopic;
			_menuPanels.HelpTopic = helpTopic;
			_menuViewer.HelpTopic = helpTopic;

			AddTool(Res.MenuInvokeInputCode, delegate { A.Psf.InvokeInputCode(); }, ToolOptions.F11Menus);
			AddTool(Res.MenuInvokeSelectedCode, delegate { A.Psf.InvokeSelectedCode(); }, ToolOptions.Editor | ToolOptions.Panels | ToolOptions.Dialog);
			AddTool(Res.MenuBackgroundJobs, delegate { A.Psf.ShowJobs(); }, ToolOptions.F11Menus);
			AddTool(Res.MenuCommandHistory, delegate { A.Psf.ShowHistory(); }, ToolOptions.F11Menus);
			AddTool(Res.MenuEditorConsole, delegate { A.Psf.ShowConsole(OpenMode.None); }, ToolOptions.F11Menus);
			AddTool(Res.MenuPowerPanel, delegate { A.Psf.ShowPanel(); }, ToolOptions.F11Menus);
			AddTool(Res.MenuTabExpansion, delegate { A.Psf.ExpandCode(null); }, ToolOptions.Editor | ToolOptions.Panels | ToolOptions.Dialog);
			AddTool(Res.MenuSnapin, delegate { A.Psf.ShowModules(); }, ToolOptions.F11Menus);
			AddTool(Res.MenuDebugger, delegate { A.Psf.ShowDebugger(); }, ToolOptions.F11Menus);
			AddTool(Res.MenuError, delegate { A.Psf.ShowErrors(); }, ToolOptions.F11Menus);

			_toSeparate = true;
			if (_pendingItems != null)
			{
				foreach (KeyValuePair<FarItem, ToolOptions> kv in _pendingItems)
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

		internal static void Show(object sender, ToolEventArgs e)
		{
			//! NOTE: 1) do Invoking()!; 2) do sync for item handlers
			A.Psf.Invoking();
			string currentDirectory = A.Psf.SyncPaths();
			try
			{
				// create once
				if (_menuDialog == null)
					Create();

				IMenu menu;

				switch (e.From)
				{
					case ToolOptions.Dialog: menu = _menuDialog; break;
					case ToolOptions.Editor: menu = _menuEditor; break;
					case ToolOptions.Panels: menu = _menuPanels; break;
					case ToolOptions.Viewer: menu = _menuViewer; break;
					default: return;
				}

				// reset, lock and show
				menu.Selected = -1;
				menu.Lock();
				menu.Show();
			}
			finally
			{
				A.SetCurrentDirectoryFinally(currentDirectory);
			}
		}

		static FarItem NewItem(string text, EventHandler click)
		{
			SetItem item = new SetItem();
			item.Text = text;
			if (click == null)
				item.IsSeparator = true;
			else
				item.Click = click;
			return item;
		}

		static void AddTool(string text, EventHandler click, ToolOptions from)
		{
			AddItem(NewItem(text, click), from);
		}

		static void AddItem(FarItem item, ToolOptions from)
		{
			if (from == ToolOptions.None)
				from = ToolOptions.F11Menus;

			if (0 < (from & ToolOptions.Dialog))
			{
				if (_toSeparate && !_menuDialogSeparated)
				{
					_menuDialogSeparated = true;
					_menuDialog.Items.Add(NewItem("Actions", null));
				}
				_menuDialog.Items.Add(item);
			}
			
			if (0 < (from & ToolOptions.Editor))
			{
				if (_toSeparate && !_menuEditorSeparated)
				{
					_menuEditorSeparated = true;
					_menuEditor.Items.Add(NewItem("Actions", null));
				}
				_menuEditor.Items.Add(item);
			}
			
			if (0 < (from & ToolOptions.Panels))
			{
				if (_toSeparate && !_menuPanelsSeparated)
				{
					_menuPanelsSeparated = true;
					_menuPanels.Items.Add(NewItem("Actions", null));
				}
				_menuPanels.Items.Add(item);
			}
			
			if (0 < (from & ToolOptions.Viewer))
			{
				if (_toSeparate && !_menuViewerSeparated)
				{
					_menuViewerSeparated = true;
					_menuViewer.Items.Add(NewItem("Actions", null));
				}
				_menuViewer.Items.Add(item);
			}
		}

		public static void AddUserTool(string text, EventHandler click, ToolOptions from)
		{
			// case: just collect, e.g. called from profile
			if (_menuDialog == null)
			{
				if (_pendingItems == null)
					_pendingItems = new List<KeyValuePair<FarItem, ToolOptions>>();

				_pendingItems.Add(new KeyValuePair<FarItem, ToolOptions>(NewItem(text, click), from));
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
