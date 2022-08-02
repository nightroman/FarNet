
// JavaScriptFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;

namespace JavaScriptFar;

[System.Runtime.InteropServices.Guid("e3780723-862e-4880-868c-66fc577f6fe2")]
[ModuleTool(Name = Res.MyName, Options = ModuleToolOptions.AllMenus)]
public class JavaScriptTool : ModuleTool
{
	public override void Invoke(object sender, ModuleToolEventArgs e)
	{
		var menu = Far.Api.CreateMenu();
		menu.Title = Res.MyName;
		menu.Add("&0. Sessions...", (s, e) => ShowSessions());
		menu.Show();
	}

	static void ShowSessions()
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
