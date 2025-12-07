using FarNet;
using System.Diagnostics;

namespace Vessel;

[ModuleTool(Name = My.Name, Options = ModuleToolOptions.F11Menus, Id = "58ad5e13-d2ba-4f4c-82cd-f53a66e9e8c0")]
public class VesselTool : ModuleTool
{
	public override void Invoke(object sender, ModuleToolEventArgs e)
	{
		IMenu menu = Far.Api.CreateMenu();
		menu.Title = My.Name;
		menu.HelpTopic = My.HelpTopic("menu-commands");
		menu.Add("&1. Files").Click += delegate { ShowFiles(); };
		menu.Add("&2. Folders").Click += delegate { ShowFolders(); };
		menu.Add("&3. Commands").Click += delegate { ShowCommands(); };
		menu.Add("&0. Update logs").Click += delegate
		{
			UpdateInteractively(Mode.File);
			UpdateInteractively(Mode.Folder);
			UpdateInteractively(Mode.Command);
		};
		menu.Show();
	}

	static void BadWindow() => Far.Api.Message("Unexpected window.", My.Name);

	static void UpdateInteractively(Mode mode)
	{
		// update
		var text = new Actor(mode).Update();

		// show
		Far.Api.UI.WriteLine($"Update {mode}", ConsoleColor.Cyan);
		Far.Api.UI.WriteLine(text);
	}

	static void UpdatePeriodically(Mode mode)
	{
		var settings = Settings.Default.GetData();
		var workings = new Workings();
		var works = workings.GetData();
		var now = DateTime.Now;

		// skip recently updated
		var lastUpdateTime = mode switch
		{
			Mode.File => works.LastUpdateTime1,
			Mode.Folder => works.LastUpdateTime2,
			Mode.Command => works.LastUpdateTime3,
			_ => throw new Exception(),
		};
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

		// start
		Task.Run(() => new Actor(mode).Update());
	}

	static IListMenu CreateListMenu()
	{
		var menu = Far.Api.CreateListMenu();
		menu.IncrementalOptions = PatternOptions.Substring;
		menu.SelectLast = true;
		menu.UsualMargins = true;
		return menu;
	}

	class Context
	{
		public Actor Actor { get; }
		public Record SelectedRecord { get; private set; } = null!;
		public string SelectedPath => SelectedRecord.Path;

		readonly IEnumerable<Record> _records;
		readonly IListMenu _menu;
		int _indexSeparator;
		int _indexSelected;

		public Context(IListMenu menu, Mode mode, DateTime old)
		{
			Actor = new Actor(mode);
			_menu = menu;
			_records = Actor.GetHistory(old);

			PopulateMenu();
		}

		void PopulateMenu()
		{
			_menu.Items.Clear();
			_indexSeparator = int.MaxValue;
			foreach (var record in _records)
			{
				// separator
				if (record.IsRecent && _indexSeparator == int.MaxValue)
				{
					_indexSeparator = _menu.Items.Count;
					_menu.Add(string.Empty).IsSeparator = true;
				}

				// item
				var item = _menu.Add(record.Path);
				item.Data = record;
				if (record.What.Length > 0)
					item.Checked = true;
			}
		}

		public bool Show()
		{
			if (!_menu.Show() || _menu.Selected < 0)
				return false;

			_indexSelected = _menu.Selected;
			SelectedRecord = (Record)_menu.SelectedData!;
			return true;
		}

		public void StartUpdate()
		{
			// capture
			var path = SelectedRecord.Path;
			var what = _indexSelected < _indexSeparator || SelectedRecord.IsTracked ? Record.USED : string.Empty;

			// we are interested only in older cases, so the selected index is valid and used as is
			if (_indexSelected < _indexSeparator)
				Actor.LogChoice(_records, _indexSelected, path);

			Tasks.Job(() =>
			{
				Actor.AppendRecordToStore(what, path);

				UpdatePeriodically(Actor.Mode);
			});
		}

		public void RemoveTracking()
		{
			if (SelectedRecord.IsTracked)
				Actor.RemoveRecordFromStore(SelectedRecord.Path);
		}

		public void ToggleTracking()
		{
			if (SelectedRecord.IsTracked)
				Actor.RemoveRecordFromStore(SelectedRecord.Path);
			else
				Actor.AppendRecordToStore(Record.USED, SelectedRecord.Path);
		}
	}

	static void FilterByCurrentDirectory(object? sender, MenuEventArgs e)
	{
		var menu = (IListMenu)sender!;
		menu.Incremental = menu.Incremental.Length > 0 ? string.Empty : Far.Api.CurrentDirectory + @"\*";
		menu.Selected = -1;
		e.Restart = true;
	}

	static void RunRemoveItem(Context context, string macro, Action loop)
	{
		context.RemoveTracking();
		Task.Run(async () =>
		{
			await Tasks.Macro(macro);
			await Tasks.Job(loop);
		});
	}

	static void ShowFiles()
	{
		var settings = Settings.Default.GetData();

		var mode = Mode.File;
		var limit = TimeSpan.FromHours(settings.Limit0);

		var menu = CreateListMenu();
		menu.HelpTopic = My.HelpTopic("file-history");
		menu.Title = $"Files";
		menu.TypeId = new Guid("23b390e8-d91d-4ff1-a9ab-de0ceffdc0ac");

		menu.AddKey(KeyCode.Delete, ControlKeyStates.None);
		menu.AddKey(KeyCode.Enter, ControlKeyStates.LeftCtrlPressed);
		menu.AddKey(KeyCode.Enter, ControlKeyStates.ShiftPressed);
		menu.AddKey(KeyCode.F3);
		menu.AddKey(KeyCode.F3, ControlKeyStates.LeftCtrlPressed);
		menu.AddKey(KeyCode.F4);
		menu.AddKey(KeyCode.F4, ControlKeyStates.LeftCtrlPressed);
		menu.AddKey(KeyCode.F12, ControlKeyStates.None, FilterByCurrentDirectory);
		menu.AddKey(KeyCode.R, ControlKeyStates.LeftCtrlPressed);
		var area = Far.Api.Window.Kind;
		if (area == WindowKind.Panels || area == WindowKind.Editor || area == WindowKind.Viewer)
			menu.AddKey(KeyCode.Delete, ControlKeyStates.ShiftPressed);

		Loop();

		void Loop()
		{
			for (; ; )
			{
				var context = new Context(menu, mode, DateTime.Now - limit);

			show:

				if (!context.Show())
					return;

				// update:
				if (menu.Key.IsCtrl(KeyCode.R))
				{
					context.Actor.Update();
					continue;
				}

				// toggle tracking:
				if (menu.Key.Is(KeyCode.Delete))
				{
					context.ToggleTracking();
					continue;
				}

				// discard:
				if (menu.Key.IsShift(KeyCode.Delete))
				{
					// Known far history items: Edit: PATH | Edit:-PATH | View: PATH | Ext.: ...
					// Remove 1-3 and 4 if 4 ends with PATH (note, proper commands use "PATH", i.e. do not end with PATH)
					var macro = $"Keys 'AltF11'; while Menu.Select({Lua.StringLiteral(context.SelectedPath)}, 2) > 0 do Keys 'ShiftDel' end; if Area.Menu then Keys 'Esc' end";
					RunRemoveItem(context, macro, Loop);
					return;
				}

				// missing?
				if (!File.Exists(context.SelectedPath))
				{
					Far.Api.Message("File does not exist.");
					goto show;
				}

				// selected!
				context.StartUpdate();

				// go to:
				if (menu.Key.IsCtrl(KeyCode.Enter))
				{
					if (Far.Api.Panel is { } panel)
						panel.GoToPath(context.SelectedPath);
				}
				// view:
				else if (menu.Key.VirtualKeyCode == KeyCode.F3)
				{
					IViewer viewer = Far.Api.CreateViewer();
					viewer.FileName = context.SelectedPath;

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
					editor.FileName = context.SelectedPath;

					if (menu.Key.IsCtrl(KeyCode.F4))
					{
						editor.Open(OpenMode.Modal);
						goto show;
					}

					editor.Open();

					if (menu.Key.IsShift(KeyCode.Enter))
						goto show;
				}

				return;
			}
		}
	}

	static void ShowFolders()
	{
		var settings = Settings.Default.GetData();

		var mode = Mode.Folder;
		var limit = TimeSpan.FromHours(settings.Limit0);

		var menu = CreateListMenu();
		menu.HelpTopic = My.HelpTopic("folder-history");
		menu.Title = $"Folders";
		menu.TypeId = new Guid("ee448906-ec7d-4ea7-bc2e-848f48cddd39");

		menu.AddKey(KeyCode.Delete, ControlKeyStates.None);
		menu.AddKey(KeyCode.Enter, ControlKeyStates.LeftCtrlPressed);
		menu.AddKey(KeyCode.Enter, ControlKeyStates.ShiftPressed);
		menu.AddKey(KeyCode.F12, ControlKeyStates.None, FilterByCurrentDirectory);
		menu.AddKey(KeyCode.R, ControlKeyStates.LeftCtrlPressed);
		if (Far.Api.Window.Kind == WindowKind.Panels)
			menu.AddKey(KeyCode.Delete, ControlKeyStates.ShiftPressed);

		Loop();

		void Loop()
		{
			for (; ; )
			{
				var context = new Context(menu, mode, DateTime.Now - limit);

				if (!context.Show())
					return;

				// update:
				if (menu.Key.IsCtrl(KeyCode.R))
				{
					context.Actor.Update();
					continue;
				}

				// toggle tracking:
				if (menu.Key.Is(KeyCode.Delete))
				{
					context.ToggleTracking();
					continue;
				}

				// discard:
				if (menu.Key.IsShift(KeyCode.Delete))
				{
					var macro = $"Keys 'AltF12'; if Menu.Select({Lua.StringLiteral(context.SelectedPath)}) > 0 then Keys 'ShiftDel' end; if Area.Menu then Keys 'Esc' end";
					RunRemoveItem(context, macro, Loop);
					return;
				}

				// selected!
				context.StartUpdate();

				// open in new console: active panel = selected path, passive panel = current path
				if (menu.Key.IsShift(KeyCode.Enter))
				{
					var info = new ProcessStartInfo($"{Environment.GetEnvironmentVariable("FARHOME")}\\Far.exe") { UseShellExecute = true };
					info.ArgumentList.Add(context.SelectedPath);
					info.ArgumentList.Add(Far.Api.CurrentDirectory);
					Process.Start(info);
					return;
				}

				// change to panels
				if (Far.Api.Window.Kind != WindowKind.Panels && !Far.Api.Window.IsModal)
					Far.Api.Window.SetCurrentAt(-1);

				// go to or open
				if (Far.Api.Window.Kind == WindowKind.Panels)
				{
					if (menu.Key.IsCtrl(KeyCode.Enter))
					{
						// go to:
						if (Far.Api.Panel is { } panel)
							panel.GoToPath(context.SelectedPath);
					}
					else
					{
						// open:
						if (Far.Api.Panel is { } panel)
							panel.CurrentDirectory = context.SelectedPath;
					}
				}
				else
				{
					BadWindow();
				}

				return;
			}
		}
	}

	static void ShowCommands()
	{
		var settings = Settings.Default.GetData();

		var mode = Mode.Command;
		var limit = TimeSpan.FromHours(settings.Limit0);

		var menu = CreateListMenu();
		menu.HelpTopic = My.HelpTopic("command-history");
		menu.Title = $"Commands";
		menu.TypeId = new Guid("1baa6870-4d49-40e5-8d20-19ff4b8ac5e6");

		menu.AddKey(KeyCode.Delete, ControlKeyStates.None);
		menu.AddKey(KeyCode.Enter, ControlKeyStates.LeftCtrlPressed);
		menu.AddKey(KeyCode.R, ControlKeyStates.LeftCtrlPressed);
		if (Far.Api.Window.Kind == WindowKind.Panels)
			menu.AddKey(KeyCode.Delete, ControlKeyStates.ShiftPressed);

		Loop();

		void Loop()
		{
			for (; ; )
			{
				var context = new Context(menu, mode, DateTime.Now - limit);

				if (!context.Show())
					return;

				// update:
				if (menu.Key.IsCtrl(KeyCode.R))
				{
					context.Actor.Update();
					continue;
				}

				// toggle tracking:
				if (menu.Key.Is(KeyCode.Delete))
				{
					context.ToggleTracking();
					continue;
				}

				// discard:
				if (menu.Key.IsShift(KeyCode.Delete))
				{
					var macro = $"Keys 'AltF8'; if Menu.Select({Lua.StringLiteral(context.SelectedPath)}) > 0 then Keys 'ShiftDel' end; if Area.Menu then Keys 'Esc' end";
					RunRemoveItem(context, macro, Loop);
					return;
				}

				// selected!
				context.StartUpdate();

				// Enter | CtrlEnter:
				var from = Far.Api.Window.Kind;
				if (from != WindowKind.Dialog && from != WindowKind.Panels && !Far.Api.Window.IsModal)
					Far.Api.Window.SetCurrentAt(-1);

				// put/post command
				ILine? line;
				if (Far.Api.Window.Kind == WindowKind.Panels)
				{
					Far.Api.CommandLine.Text = context.SelectedPath;
					if (!menu.Key.IsCtrl())
						Far.Api.PostMacro("Keys'Enter'");
				}
				else if ((line = Far.Api.Line) is { } && !line.IsReadOnly)
				{
					line.InsertText(context.SelectedPath);
				}
				else
				{
					BadWindow();
				}

				return;
			}
		}
	}
}
