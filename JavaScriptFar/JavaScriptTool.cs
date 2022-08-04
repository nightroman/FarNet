
// JavaScriptFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System.IO;

namespace JavaScriptFar;

[System.Runtime.InteropServices.Guid("e3780723-862e-4880-868c-66fc577f6fe2")]
[ModuleTool(Name = Res.MyName, Options = ModuleToolOptions.AllMenus)]
public class JavaScriptTool : ModuleTool
{
	public override void Invoke(object sender, ModuleToolEventArgs e)
	{
		var menu = Far.Api.CreateMenu();
		menu.Title = Res.MyName;
		menu.Add("&1. Configuration", (s, e) => Configuration());
		menu.Add("&0. Sessions...", (s, e) => Sessions());
		menu.Show();
	}

	static void Configuration()
	{
		var file = Path.Combine(Far.Api.CurrentDirectory, Session.SessionConfigFile);
		if (!File.Exists(file))
		{
			if (0 != Far.Api.Message($"Create configuration {file}?", Res.MyName, MessageOptions.YesNo))
				return;
		}

		new ModuleSettings<SessionConfiguration>(file).Edit();
	}

	static void Sessions()
	{
		var menu = Far.Api.CreateListMenu();
		menu.Title = "JavaScript sessions";
		menu.Bottom = "Enter, Del";
		menu.UsualMargins = true;

		menu.AddKey(KeyCode.Delete);

		foreach (var session in Session.Sessions)
		{
			var item = menu.Add(session.Root);
			item.Data = session;
			item.Checked = session.IsDebug;
		}

		if (!menu.Show())
			return;

		var selectedSession = (Session)menu.SelectedData;
		if (selectedSession is null)
			return;

		// Del
		if (menu.Key.Is(KeyCode.Delete))
		{
			selectedSession.Dispose();
			return;
		}

		// Enter
		try { Far.Api.Window.SetCurrentAt(-1); } catch { }
		try { Far.Api.Panel.CurrentDirectory = selectedSession.Root; } catch { }
	}
}
