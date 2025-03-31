using FarNet;

namespace PowerShellFar.UI;

static class ActorMenu
{
	static IMenu _menuDialog = null!;
	static IMenu _menuEditor = null!;
	static IMenu _menuPanels = null!;
	static IMenu _menuViewer = null!;

	static ActorMenu()
	{
		_menuDialog = Far.Api.CreateMenu();
		_menuEditor = Far.Api.CreateMenu();
		_menuPanels = Far.Api.CreateMenu();
		_menuViewer = Far.Api.CreateMenu();

		_menuDialog.Title = Res.Me;
		_menuEditor.Title = Res.Me;
		_menuPanels.Title = Res.Me;
		_menuViewer.Title = Res.Me;

		string helpTopic = Entry.Instance.GetHelpTopic(HelpTopic.MenuCommands);
		_menuDialog.HelpTopic = helpTopic;
		_menuEditor.HelpTopic = helpTopic;
		_menuPanels.HelpTopic = helpTopic;
		_menuViewer.HelpTopic = helpTopic;

		AddTool(Res.MenuInvokeCommands, delegate { A.Psf.StartInvokeCommands(); }, ModuleToolOptions.F11Menus);
		AddTool(Res.MenuInvokeSelected, delegate { A.Psf.InvokeSelectedCode(); }, ModuleToolOptions.Editor | ModuleToolOptions.Panels | ModuleToolOptions.Dialog);
		AddTool(Res.MenuCommandHistory, delegate { A.Psf.ShowHistory(); }, ModuleToolOptions.F11Menus);
		AddTool(Res.MenuInteractive, delegate { A.Psf.ShowInteractive(OpenMode.None); }, ModuleToolOptions.F11Menus);
		AddTool(Res.MenuPowerPanel, delegate { A.Psf.ShowPanel(); }, ModuleToolOptions.F11Menus);
		AddTool(Res.MenuTabExpansion, delegate { A.Psf.ExpandCode(null); }, ModuleToolOptions.Editor | ModuleToolOptions.Panels | ModuleToolOptions.Dialog);
		AddTool(Res.MenuDebugger, delegate { A.Psf.ShowDebugger(); }, ModuleToolOptions.F11Menus);
		AddTool(Res.MenuError, delegate { A.Psf.ShowErrors(); }, ModuleToolOptions.F11Menus);
		AddTool(Res.MenuHelp, delegate { A.Psf.ShowHelp(); }, ModuleToolOptions.F11Menus);
	}

	internal static void Close()
	{
		_menuDialog.Unlock();
		_menuEditor.Unlock();
		_menuPanels.Unlock();
		_menuViewer.Unlock();

		_menuDialog = null!;
		_menuEditor = null!;
		_menuPanels = null!;
		_menuViewer = null!;
	}

	internal static void Show(ModuleToolEventArgs e)
	{
		IMenu menu;
		switch (e.From)
		{
			case ModuleToolOptions.Dialog: menu = _menuDialog; break;
			case ModuleToolOptions.Editor: menu = _menuEditor; break;
			case ModuleToolOptions.Panels: menu = _menuPanels; break;
			case ModuleToolOptions.Viewer: menu = _menuViewer; break;
			default: return;
		}

		// reset, lock
		menu.Selected = -1;
		menu.Lock();

		//! show, with sync for handlers, e.g. TabEx depends
		A.Psf.SyncPaths();
		menu.Show();
	}

	static void AddTool(string text, EventHandler<MenuEventArgs> click, ModuleToolOptions from)
	{
		var item = new SetItem { Text = text, Click = click };

		if (from == ModuleToolOptions.None)
			from = ModuleToolOptions.F11Menus;

		if (0 < (from & ModuleToolOptions.Dialog))
			_menuDialog.Items.Add(item);

		if (0 < (from & ModuleToolOptions.Editor))
			_menuEditor.Items.Add(item);

		if (0 < (from & ModuleToolOptions.Panels))
			_menuPanels.Items.Add(item);

		if (0 < (from & ModuleToolOptions.Viewer))
			_menuViewer.Items.Add(item);
	}
}
