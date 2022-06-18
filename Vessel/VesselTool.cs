﻿
// FarNet module Vessel
// Copyright (c) Roman Kuzmin

using FarNet;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Vessel;

[System.Runtime.InteropServices.Guid("58ad5e13-d2ba-4f4c-82cd-f53a66e9e8c0")]
[ModuleTool(Name = My.Name, Options = ModuleToolOptions.F11Menus)]
public class VesselTool : ModuleTool
{
	public override void Invoke(object sender, ModuleToolEventArgs e)
	{
		IMenu menu = Far.Api.CreateMenu();
		menu.Title = My.Name;
		menu.HelpTopic = My.HelpTopic("menu-commands");
		menu.Add("&1. Smart files").Click += delegate { ShowFiles(); };
		menu.Add("&2. Smart folders").Click += delegate { ShowFolders(); };
		menu.Add("&3. Smart commands").Click += delegate { ShowCommands(); };
		menu.Add("&0. Update records").Click += delegate
		{
			Update(Mode.File);
			Update(Mode.Folder);
			Update(Mode.Command);
		};
		menu.Show();
	}

	static void Update(Mode mode)
	{
		// update
		var actor = new Actor(mode, VesselHost.LogPath[(int)mode], true);
		var text = actor.Update();

		// show
		Far.Api.UI.WriteLine($"Update {mode}", ConsoleColor.Cyan);
		Far.Api.UI.WriteLine(text);
	}

	static void UpdateWork(object state)
	{
		var mode = (Mode)state;

		// update
		var algo = new Actor(mode, VesselHost.LogPath[(int)mode], true);
		algo.Update();
	}

	static void UpdatePeriodically(Mode mode)
	{
		var settings = Settings.Default.GetData();
		var workings = new Workings();
		var works = workings.GetData();
		var now = DateTime.Now;

		// skip recently updated
		DateTime lastUpdateTime;
		switch (mode)
		{
			case Mode.File:
				lastUpdateTime = works.LastUpdateTime1;
				break;
			case Mode.Folder:
				lastUpdateTime = works.LastUpdateTime2;
				break;
			case Mode.Command:
				lastUpdateTime = works.LastUpdateTime3;
				break;
			default:
				throw new Exception();
		}
		if ((now - lastUpdateTime).TotalHours < settings.Limit0)
			return;

		// save new last update time
		switch (mode)
		{
			case Mode.File:
				works.LastUpdateTime1 = now;
				break;
			case Mode.Folder:
				works.LastUpdateTime2 = now;
				break;
			case Mode.Command:
				works.LastUpdateTime3 = now;
				break;
		}
		workings.Save();

		// start work
		ThreadPool.QueueUserWorkItem(UpdateWork, mode);
	}

	static IListMenu CreateListMenu()
	{
		var menu = Far.Api.CreateListMenu();
		menu.IncrementalOptions = PatternOptions.Substring;
		menu.SelectLast = true;
		menu.UsualMargins = true;
		return menu;
	}

	static void ShowFiles()
	{
		var settings = Settings.Default.GetData();

		var mode = Mode.File;
		var store = VesselHost.LogPath[(int)mode];
		var limit = TimeSpan.FromHours(settings.Limit0);

		var menu = CreateListMenu();
		menu.HelpTopic = My.HelpTopic("file-history");
		menu.Title = $"Smart files";
		menu.TypeId = new Guid("23b390e8-d91d-4ff1-a9ab-de0ceffdc0ac");

		menu.AddKey(KeyCode.Enter, ControlKeyStates.LeftCtrlPressed);
		menu.AddKey(KeyCode.Enter, ControlKeyStates.ShiftPressed);
		menu.AddKey(KeyCode.F3);
		menu.AddKey(KeyCode.F3, ControlKeyStates.LeftCtrlPressed);
		menu.AddKey(KeyCode.F4);
		menu.AddKey(KeyCode.F4, ControlKeyStates.LeftCtrlPressed);
		menu.AddKey(KeyCode.R, ControlKeyStates.LeftCtrlPressed);
		var area = Far.Api.Window.Kind;
		if (area == WindowKind.Panels || area == WindowKind.Editor || area == WindowKind.Viewer)
			menu.AddKey(KeyCode.Delete, ControlKeyStates.ShiftPressed);

		for (; ; menu.Items.Clear())
		{
			var actor = new Actor(mode, store);
			bool needSeparator = true;
			foreach (var it in actor.GetHistory(DateTime.Now))
			{
				// separator
				if (needSeparator && it.Idle < limit)
				{
					menu.Add(string.Empty).IsSeparator = true;
					needSeparator = false;
				}

				// item
				var item = menu.Add(it.Path);
				item.Data = it;
				if (it.Evidence > 0)
					item.Checked = true;
			}

		show:

			//! show and check the result or after Esc index may be > 0
			//! e.g. ShiftDel the last record + Esc == index out of range
			if (!menu.Show() || menu.Selected < 0)
				return;

			// update:
			if (menu.Key.IsCtrl(KeyCode.R))
			{
				var algo = new Actor(mode, store, true);
				algo.Update();
				continue;
			}

			// the file
			int indexSelected = menu.Selected;
			string path = menu.Items[indexSelected].Text;

			// delete:
			if (menu.Key.IsShift(KeyCode.Delete))
			{
				if (My.AskDiscard(path))
				{
					Store.Remove(store, path, StringComparison.OrdinalIgnoreCase);

					// Known far history items: Edit: PATH | Edit:-PATH | View: PATH | Ext.: ...
					// Remove 1-3 and 4 if 4 ends with PATH (note, proper commands use "PATH", i.e. do not end with PATH)
					Far.Api.PostMacro($"Keys 'AltF11'; while Menu.Select({Lua.StringLiteral(path)}, 2) > 0 do Keys 'ShiftDel' end; if Area.Menu then Keys 'Esc' end");
					return; //!
				}

				goto show;
			}

			// missing?
			if (!File.Exists(path))
			{
				Far.Api.Message("File does not exist.");
				goto show;
			}

			// if it is not logged yet, log the existing Far record(s)
			if (!actor.IsLoggedPath(path))
			{
				var info = menu.Items[indexSelected].Data as Info;
				Store.Append(store, info.Time1, Record.GOTO, path);
				if (info.Time1 != info.TimeN)
					Store.Append(store, info.TimeN, Record.GOTO, path);
			}

			// go to:
			if (menu.Key.IsCtrl(KeyCode.Enter))
			{
				Far.Api.Panel.GoToPath(path);
				Store.Append(store, DateTime.Now, Record.GOTO, path);
			}
			// view:
			else if (menu.Key.VirtualKeyCode == KeyCode.F3)
			{
				IViewer viewer = Far.Api.CreateViewer();
				viewer.FileName = path;

				viewer.Closed += delegate
				{
					Store.Append(store, DateTime.Now, Record.VIEW, path);
				};

				if (menu.Key.IsCtrl())
				{
					viewer.Open(OpenMode.Modal);
					goto show;
				}

				viewer.Open();
			}
			// edit:
			else
			{
				IEditor editor = Far.Api.CreateEditor();
				editor.FileName = path;

				editor.Closed += delegate
				{
					Store.Append(store, DateTime.Now, Record.EDIT, path);
				};

				if (menu.Key.IsCtrl(KeyCode.F4))
				{
					editor.Open(OpenMode.Modal);
					goto show;
				}

				editor.Open();

				if (menu.Key.IsShift(KeyCode.Enter))
					goto show;
			}

			UpdatePeriodically(mode);
			return;
		}
	}

	static void ShowFolders()
	{
		var settings = Settings.Default.GetData();

		var mode = Mode.Folder;
		var store = VesselHost.LogPath[(int)mode];
		var limit = TimeSpan.FromHours(settings.Limit0);

		var menu = CreateListMenu();
		menu.HelpTopic = My.HelpTopic("folder-history");
		menu.Title = $"Smart folders";
		menu.TypeId = new Guid("ee448906-ec7d-4ea7-bc2e-848f48cddd39");

		menu.AddKey(KeyCode.R, ControlKeyStates.LeftCtrlPressed);
		menu.AddKey(KeyCode.Enter, ControlKeyStates.ShiftPressed);
		if (Far.Api.Window.Kind == WindowKind.Panels)
			menu.AddKey(KeyCode.Delete, ControlKeyStates.ShiftPressed);

		for (; ; menu.Items.Clear())
		{
			var actor = new Actor(mode, store);
			bool needSeparator = true;
			foreach (var it in actor.GetHistory(DateTime.Now))
			{
				// separator
				if (needSeparator && it.Idle < limit)
				{
					menu.Add(string.Empty).IsSeparator = true;
					needSeparator = false;
				}

				// item
				var item = menu.Add(it.Path);
				item.Data = it;
				if (it.Evidence > 0)
					item.Checked = true;
			}

		show:

			//! show and check the result or after Esc index may be > 0
			//! e.g. ShiftDel the last record + Esc == index out of range
			if (!menu.Show() || menu.Selected < 0)
				return;

			// update:
			if (menu.Key.IsCtrl(KeyCode.R))
			{
				var algo = new Actor(mode, store, true);
				algo.Update();
				continue;
			}

			// the folder
			int indexSelected = menu.Selected;
			string path = menu.Items[indexSelected].Text;

			// delete:
			if (menu.Key.IsShift(KeyCode.Delete))
			{
				if (My.AskDiscard(path))
				{
					Store.Remove(store, path, StringComparison.OrdinalIgnoreCase);
					Far.Api.PostMacro($"Keys 'AltF12'; if Menu.Select({Lua.StringLiteral(path)}) > 0 then Keys 'ShiftDel' end; if Area.Menu then Keys 'Esc' end");
					return; //!
				}
				goto show;
			}

			// this function logs and periodically updates
			void LogAndUpdate()
			{
				// if it is not logged yet, log the existing Far record
				if (!actor.IsLoggedPath(path))
				{
					var info = menu.Items[indexSelected].Data as Info;
					Store.Append(store, info.Time1, Record.OPEN, path);
				}
				// then log the current record
				Store.Append(store, DateTime.Now, Record.OPEN, path);

				UpdatePeriodically(mode);
			}

			// open in new console: active panel = selected path, passive panel = current path
			if (menu.Key.IsShift(KeyCode.Enter))
			{
				Process.Start(new ProcessStartInfo()
				{
					FileName = $"{Environment.GetEnvironmentVariable("FARHOME")}\\Far.exe",
					Arguments = $"\"{path}\" \"{Far.Api.CurrentDirectory}\""
				});
				LogAndUpdate();
				return;
			}

			// Enter:
			if (Far.Api.Window.Kind != WindowKind.Panels && !Far.Api.Window.IsModal)
				Far.Api.Window.SetCurrentAt(-1);

			// set selected path
			if (Far.Api.Window.Kind == WindowKind.Panels)
			{
				Far.Api.Panel.CurrentDirectory = path;
			}
			else
			{
				My.BadWindow();
			}

			LogAndUpdate();
			return;
		}
	}

	static void ShowCommands()
	{
		var settings = Settings.Default.GetData();

		var mode = Mode.Command;
		var store = VesselHost.LogPath[(int)mode];
		var limit = TimeSpan.FromHours(settings.Limit0);

		var menu = CreateListMenu();
		menu.HelpTopic = My.HelpTopic("command-history");
		menu.Title = $"Smart commands";
		menu.TypeId = new Guid("1baa6870-4d49-40e5-8d20-19ff4b8ac5e6");

		menu.AddKey(KeyCode.R, ControlKeyStates.LeftCtrlPressed);
		menu.AddKey(KeyCode.Enter, ControlKeyStates.LeftCtrlPressed);
		if (Far.Api.Window.Kind == WindowKind.Panels)
			menu.AddKey(KeyCode.Delete, ControlKeyStates.ShiftPressed);

		for (; ; menu.Items.Clear())
		{
			var actor = new Actor(mode, store);
			bool needSeparator = true;
			foreach (var it in actor.GetHistory(DateTime.Now))
			{
				// separator
				if (needSeparator && it.Idle < limit)
				{
					menu.Add(string.Empty).IsSeparator = true;
					needSeparator = false;
				}

				// item
				var item = menu.Add(it.Path);
				item.Data = it;
				if (it.Evidence > 0)
					item.Checked = true;
			}

		show:

			//! show and check the result or after Esc index may be > 0
			//! e.g. ShiftDel the last record + Esc == index out of range
			if (!menu.Show() || menu.Selected < 0)
				return;

			// update:
			if (menu.Key.IsCtrl(KeyCode.R))
			{
				var algo = new Actor(mode, store, true);
				algo.Update();
				continue;
			}

			// the command
			int indexSelected = menu.Selected;
			string path = menu.Items[indexSelected].Text;

			// delete:
			if (menu.Key.IsShift(KeyCode.Delete))
			{
				if (My.AskDiscard(path))
				{
					Store.Remove(store, path, StringComparison.Ordinal);
					Far.Api.PostMacro($"Keys 'AltF8'; if Menu.Select({Lua.StringLiteral(path)}) > 0 then Keys 'ShiftDel' end; if Area.Menu then Keys 'Esc' end");
					return; //!
				}
				goto show;
			}

			// Enter | CtrlEnter:
			if (Far.Api.Window.Kind != WindowKind.Panels && !Far.Api.Window.IsModal)
				Far.Api.Window.SetCurrentAt(-1);

			// put/post command
			if (Far.Api.Window.Kind == WindowKind.Panels)
			{
				Far.Api.CommandLine.Text = path;
				if (!menu.Key.IsCtrl())
					Far.Api.PostMacro("Keys'Enter'");
			}
			else
			{
				My.BadWindow();
			}

			// if it is not logged yet, log the existing Far record
			if (!actor.IsLoggedPath(path))
			{
				var info = menu.Items[indexSelected].Data as Info;
				Store.Append(store, info.Time1, Record.OPEN, path);
			}
			// then log the current record
			Store.Append(store, DateTime.Now, Record.OPEN, path);

			UpdatePeriodically(mode);
			return;
		}
	}
}
