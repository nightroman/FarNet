
// JavaScriptFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System.IO;

namespace JavaScriptFar;

[ModuleTool(Name = Res.MyName, Options = ModuleToolOptions.AllMenus, Id = "e3780723-862e-4880-868c-66fc577f6fe2")]
public class JavaScriptTool : ModuleTool
{
	public override void Invoke(object sender, ModuleToolEventArgs e)
	{
		var menu = Far.Api.CreateMenu();
		menu.Title = Res.MyName;
		menu.Add("&1. Start debugging", (_, _) => StartDebugging(e.From));
		menu.Add("&2. Configuration", (_, _) => Configuration());
		menu.Add("&0. Sessions...", (_, _) => Sessions());
		menu.Show();
	}

	static void StartDebugging(ModuleToolOptions from)
	{
		switch (from)
		{
			case ModuleToolOptions.Editor:
				{
					var editor = Far.Api.Editor!;
					var file = editor.FileName;
					if (Session.IsFileDocument(file))
					{
						editor.Save();
						var caret = editor.Caret;
						Actor.StartDebugging(file, caret.Y + 1, caret.X + 1);
						return;
					}
				}
				break;
			case ModuleToolOptions.Panels:
				{
					var file = Far.Api.Panel!.CurrentFile;
					if (file is not null && Session.IsFileDocument(file.Name))
					{
						var path = Path.Join(Far.Api.CurrentDirectory, file.Name);
						if (File.Exists(path))
						{
							Actor.StartDebugging(path);
							return;
						}
					}
				}
				break;
		}
		Actor.StartDebugging();
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

		for (; ; menu.Items.Clear())
		{
			foreach (var session in Session.Sessions)
			{
				var item = menu.Add(session.Root);
				item.Data = session;
				item.Checked = session.IsDebug;
			}

			if (!menu.Show())
				return;

			var selectedSession = (Session?)menu.SelectedData;
			if (selectedSession is null)
				return;

			// Del
			if (menu.Key.Is(KeyCode.Delete))
			{
				selectedSession.Dispose();
				continue;
			}

			// Enter
			try { Far.Api.Window.SetCurrentAt(-1); } catch { }
			try { Far.Api.Panel!.CurrentDirectory = selectedSession.Root; } catch { }
			return;
		}
	}
}
