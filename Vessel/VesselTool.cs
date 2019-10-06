
// FarNet module Vessel
// Copyright (c) Roman Kuzmin

using System;
using System.IO;
using System.Threading;

namespace FarNet.Vessel
{
	[System.Runtime.InteropServices.Guid("58ad5e13-d2ba-4f4c-82cd-f53a66e9e8c0")]
	[ModuleTool(Name = "Vessel", Options = ModuleToolOptions.F11Menus)]
	public class VesselTool : ModuleTool
	{
		static string AppHome { get { return Path.GetDirectoryName(typeof(VesselTool).Assembly.Location); } }
		static string HelpTopic { get { return "<" + AppHome + "\\>"; } }
		public override void Invoke(object sender, ModuleToolEventArgs e)
		{
			IMenu menu = Far.Api.CreateMenu();
			menu.Title = "Vessel";
			menu.HelpTopic = HelpTopic + "MenuCommands";
			menu.Add("&1. Smart history").Click += delegate { ShowHistory(); };
			menu.Add("&2. Smart folders").Click += delegate { ShowFolders(); };
			menu.Add("&3. Train history").Click += delegate { Train(0); };
			menu.Add("&4. Train folders").Click += delegate { Train(1); };
			menu.Add("&5. Update history").Click += delegate { Update(0); };
			menu.Add("&6. Update folders").Click += delegate { Update(1); };

			menu.Show();
		}
		static string ResultText(Result result)
		{
			return string.Format(@"
Up count   : {0,8}
Down count : {1,8}
Same count : {2,8}

Up sum     : {3,8}
Down sum   : {4,8}
Total sum  : {5,8}

Average    : {6,8:n2}
Factors    : {7,8}
",
 result.UpCount,
 result.DownCount,
 result.SameCount,
 result.UpSum,
 result.DownSum,
 result.TotalSum,
 result.Average,
 Settings.Default.Limit0.ToString() + "/" + result.Factor);
		}
		static void SaveFactors(int mode, Result result)
		{
			var settings = Settings.Default;
			var factor = settings.GetFactor(mode);
			if (result.Factor != factor)
			{
				settings.SetFactor(mode, result.Factor);
				settings.Save();
			}
		}
		static void Train(int mode)
		{
			// train/save
			var algo = new Actor(mode);
			var result = algo.Train(Settings.Default.GetLimit(mode), null);
			SaveFactors(mode, result);

			// show report
			var report = ResultText(result);
			Far.Api.Message(report, "Training results", MessageOptions.LeftAligned);
		}
		static void StartTrainingWork(object state)
		{
			int mode = (int)state;
			var algo = new Actor(mode);
			var result = algo.Train(Settings.Default.GetLimit(mode), null);
			SaveFactors(mode, result);
		}
		public static void StartTraining(int mode)
		{
			ThreadPool.QueueUserWorkItem(StartTrainingWork, mode);
		}
		static void Update(int mode)
		{
			// update
			var algo = new Actor(mode, VesselHost.LogPath[mode], true);
			var text = algo.Update();

			// train
			StartTraining(mode);

			// show update info
			Far.Api.Message(text, "Update", MessageOptions.LeftAligned);
		}
		static void UpdateWork(object state)
		{
			int mode = (int)state;

			// update
			var algo = new Actor(mode, VesselHost.LogPath[mode], true);
			algo.Update();

			// train
			StartTraining(mode);
		}
		static void UpdatePeriodically(int mode)
		{
			var now = DateTime.Now;

			// skip recently updated
			var lastUpdateTime = mode == 0 ? Settings.Default.LastUpdateTime1 : Settings.Default.LastUpdateTime2;
			if ((now - lastUpdateTime).TotalHours < Settings.Default.Limit0)
				return;

			// save new last update time
			if (mode == 0)
				Settings.Default.LastUpdateTime1 = now;
			else
				Settings.Default.LastUpdateTime2 = now;
			Settings.Default.Save();

			// start work
			ThreadPool.QueueUserWorkItem(UpdateWork, mode);
		}
		static void ShowHistory()
		{
			var factor0 = Settings.Default.Limit0;
			var factor1 = Settings.Default.Factor1;

			IListMenu menu = Far.Api.CreateListMenu();
			menu.HelpTopic = HelpTopic + "FileHistory";
			menu.SelectLast = true;
			menu.UsualMargins = true;
			menu.Title = string.Format("File history ({0}/{1})", factor0, factor1);

			menu.IncrementalOptions = PatternOptions.Substring;

			menu.AddKey(KeyCode.Delete, ControlKeyStates.ShiftPressed);
			menu.AddKey(KeyCode.Enter, ControlKeyStates.LeftCtrlPressed);
			menu.AddKey(KeyCode.Enter, ControlKeyStates.ShiftPressed);
			menu.AddKey(KeyCode.F3);
			menu.AddKey(KeyCode.F3, ControlKeyStates.LeftCtrlPressed);
			menu.AddKey(KeyCode.F4);
			menu.AddKey(KeyCode.F4, ControlKeyStates.LeftCtrlPressed);
			menu.AddKey(KeyCode.R, ControlKeyStates.LeftCtrlPressed);

			for (; ; menu.Items.Clear())
			{
				int lastGroup = -1;
				foreach (var it in Store.GetHistory(0, null, DateTime.Now, factor1))
				{
					// separator
					int nextGroup = it.Group(factor0, factor1);
					if (lastGroup != nextGroup)
					{
						if (lastGroup > 0)
							menu.Add("").IsSeparator = true;

						lastGroup = nextGroup;
					}

					// item
					menu.Add(it.Path).Checked = it.Evidence > 0;
				}

			show:

				//! show and check the result or after Esc index may be > 0
				//! e.g. ShiftDel the last record + Esc == index out of range
				if (!menu.Show() || menu.Selected < 0)
					return;

				// update:
				if (menu.Key.IsCtrl(KeyCode.R))
				{
					var algo = new Actor(0, VesselHost.LogPath[0], true);
					algo.Update();
					continue;
				}

				// the file
				int indexSelected = menu.Selected;
				string path = menu.Items[indexSelected].Text;

				// delete:
				if (menu.Key.IsShift(KeyCode.Delete))
				{
					if (0 == Far.Api.Message("Discard " + path, "Confirm", MessageOptions.OkCancel))
					{
						Store.Remove(0, VesselHost.LogPath[0], path);
						continue;
					}

					goto show;
				}

				// go to:
				if (menu.Key.IsCtrl(KeyCode.Enter))
				{
					Far.Api.Panel.GoToPath(path);
				}
				// view:
				else if (menu.Key.VirtualKeyCode == KeyCode.F3)
				{
					if (!File.Exists(path))
						continue;

					IViewer viewer = Far.Api.CreateViewer();
					viewer.FileName = path;

					if (menu.Key.IsCtrl())
					{
						viewer.DisableHistory = true;
						viewer.Open(OpenMode.Modal);
						goto show;
					}

					viewer.Closed += delegate
					{
						Store.Append(VesselHost.LogPath[0], DateTime.Now, Record.VIEW, path);
					};

					viewer.Open();
				}
				// edit:
				else
				{
					IEditor editor = Far.Api.CreateEditor();
					editor.FileName = path;

					if (menu.Key.IsCtrl(KeyCode.F4))
					{
						editor.DisableHistory = true;
						editor.Open(OpenMode.Modal);
						goto show;
					}

					editor.Closed += delegate
					{
						Store.Append(VesselHost.LogPath[0], DateTime.Now, Record.EDIT, path);
					};

					editor.Open();

					if (menu.Key.IsShift(KeyCode.Enter))
						goto show;
				}

				UpdatePeriodically(0);
				return;
			}
		}
		static void ShowFolders()
		{
			var limit = Settings.Default.Limit0;
			var factor = Settings.Default.Factor2;

			IListMenu menu = Far.Api.CreateListMenu();
			menu.HelpTopic = HelpTopic + "FolderHistory";
			menu.SelectLast = true;
			menu.UsualMargins = true;
			menu.Title = string.Format("Folder history ({0}/{1})", limit, factor);

			menu.IncrementalOptions = PatternOptions.Substring;

			menu.AddKey(KeyCode.Delete, ControlKeyStates.ShiftPressed);
			menu.AddKey(KeyCode.R, ControlKeyStates.LeftCtrlPressed);

			for (; ; menu.Items.Clear())
			{
				int lastGroup = -1;
				foreach (var it in Store.GetHistory(1, null, DateTime.Now, factor))
				{
					// separator
					int nextGroup = it.Group(limit, factor);
					if (lastGroup != nextGroup)
					{
						if (lastGroup > 0)
							menu.Add("").IsSeparator = true;
						lastGroup = nextGroup;
					}

					// item
					menu.Add(it.Path).Checked = it.Evidence > 0;
				}

			show:

				//! show and check the result or after Esc index may be > 0
				//! e.g. ShiftDel the last record + Esc == index out of range
				if (!menu.Show() || menu.Selected < 0)
					return;

				// update:
				if (menu.Key.IsCtrl(KeyCode.R))
				{
					var algo = new Actor(1, VesselHost.LogPath[1], true);
					algo.Update();
					continue;
				}

				// the folder
				int indexSelected = menu.Selected;
				string path = menu.Items[indexSelected].Text;

				// delete:
				if (menu.Key.IsShift(KeyCode.Delete))
				{
					if (0 == Far.Api.Message("Discard " + path, "Confirm", MessageOptions.OkCancel))
					{
						Store.Remove(1, VesselHost.LogPath[1], path);
						continue;
					}
					goto show;
				}

				// Enter:
				if (Far.Api.Window.Kind != WindowKind.Panels && !Far.Api.Window.IsModal)
					Far.Api.Window.SetCurrentAt(-1);

				Far.Api.Panel.CurrentDirectory = path;
				Store.Append(VesselHost.LogPath[1], DateTime.Now, Record.OPEN, path);

				UpdatePeriodically(1);
				return;
			}
		}
	}
}
