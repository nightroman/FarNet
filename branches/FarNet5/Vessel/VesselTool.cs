
/*
FarNet module Vessel
Copyright (c) 2011-2012 Roman Kuzmin
*/

using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace FarNet.Vessel
{
	[System.Runtime.InteropServices.Guid("58ad5e13-d2ba-4f4c-82cd-f53a66e9e8c0")]
	[ModuleTool(Name = "Vessel history", Options = ModuleToolOptions.F11Menus)]
	public class VesselTool : ModuleTool
	{
		const int
			KeyEdit = (KeyCode.F4),
			KeyEditAdd = (KeyMode.Shift | KeyCode.Enter),
			KeyEditModal = (KeyMode.Ctrl | KeyCode.F4),
			KeyGoToFile = (KeyMode.Ctrl | KeyCode.Enter),
			KeyDiscard = (KeyMode.Shift | KeyCode.Del),
			KeyUpdate = (KeyMode.Ctrl | 'R'),
			KeyView = (KeyCode.F3),
			KeyViewModal = (KeyMode.Ctrl | KeyCode.F3);

		static string AppHome { get { return Path.GetDirectoryName((Assembly.GetExecutingAssembly()).Location); } }
		static string HelpTopic { get { return "<" + AppHome + "\\>"; } }

		static string _TrainingReport;
		internal static int TrainingRecordCount { get; set; }
		internal static int TrainingRecordIndex { get; set; }

		public override void Invoke(object sender, ModuleToolEventArgs e)
		{
			IMenu menu = Far.Net.CreateMenu();
			menu.Title = "Vessel";
			menu.HelpTopic = HelpTopic + "MenuCommands";
			menu.Add("&1. Smart file history").Click += delegate { ShowHistory(true); };
			menu.Add("&2. Plain file history").Click += delegate { ShowHistory(false); };
			switch (TrainingStatus)
			{
				case TrainingState.None:
					menu.Add("&3. Background training").Click += delegate { TrainFull(); };
					break;
				case TrainingState.Started:
					menu.Add("Training: record " + TrainingRecordIndex + "/" + TrainingRecordCount).Disabled = true;
					break;
				case TrainingState.Completed:
					menu.Add("&3. Training results").Click += delegate { ShowResults(); };
					break;
			}
			menu.Add("&0. Update history file").Click += delegate { Update(); };

			menu.Show();
		}

		static TrainingState TrainingStatus
		{
			get
			{
				//! snapshot
				var value = _TrainingReport;

				if (value == null)
					return TrainingState.None;

				if (value.Length == 0)
					return TrainingState.Started;

				return TrainingState.Completed;
			}
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
 Settings.Default.Limit0.ToString() + "/" + result.Factor1 + "/" + result.Factor2);
		}

		static void ShowResults()
		{
			Far.Net.Message(_TrainingReport, "Training results", MsgOptions.LeftAligned);
			_TrainingReport = null;
		}

		static void SaveFactors(Result result)
		{
			var settings = Settings.Default;
			if (result.Target > 0)
			{
				if (result.Factor1 != settings.Factor1 || result.Factor2 != settings.Factor2)
				{
					settings.Factor1 = result.Factor1;
					settings.Factor2 = result.Factor2;
					settings.Save();
				}
			}
			else
			{
				if (settings.Factor1 >= 0)
				{
					settings.Factor1 = -1;
					settings.Factor2 = -1;
					settings.Save();
				}
			}
		}

		static void TrainWorkerFull()
		{
			// post started
			_TrainingReport = string.Empty;

			// train/save
			var algo = new Actor();
			var result = algo.TrainFull(Settings.Default.Limit1, Settings.Default.Limit2);
			SaveFactors(result);

			// post done
			_TrainingReport = ResultText(result);
		}

		static void TrainFull()
		{
			var thread = new Thread(TrainWorkerFull);
			thread.Start();
		}

		static void TrainWorkerFast()
		{
			// post started
			_TrainingReport = string.Empty;

			// train/save
			var algo = new Actor();
			var result = algo.TrainFast(Settings.Default.Factor1, Settings.Default.Factor2);
			SaveFactors(result);

			// post done
			_TrainingReport = ResultText(result);
		}

		public static void StartFastTraining()
		{
			var thread = new Thread(TrainWorkerFast);
			thread.Start();
		}

		static void Update()
		{
			// update
			var algo = new Actor(VesselHost.LogPath);
			var text = algo.Update();

			// retrain
			StartFastTraining();

			// show update info
			Far.Net.Message(text, "Update", MsgOptions.LeftAligned);
		}

		static void ShowHistory(bool smart)
		{
			var Factor1 = Settings.Default.Factor1;
			var Factor2 = Settings.Default.Factor2;
			var Limit0 = Settings.Default.Limit0;

			// drop smart for the negative factor
			if (smart && Factor1 < 0)
				smart = false;

			IListMenu menu = Far.Net.CreateListMenu();
			menu.HelpTopic = HelpTopic + "FileHistory";
			menu.SelectLast = true;
			menu.UsualMargins = true;
			if (smart)
				menu.Title = string.Format("File history ({0}/{1}/{2})", Limit0, Factor1, Factor2);
			else
				menu.Title = "File history (plain)";

			menu.FilterHistory = "RegexFileHistory";
			menu.FilterRestore = true;
			menu.FilterOptions = PatternOptions.Regex;
			menu.IncrementalOptions = PatternOptions.Substring;

			menu.AddKey(KeyEdit);
			menu.AddKey(KeyEditAdd);
			menu.AddKey(KeyEditModal);
			menu.AddKey(KeyGoToFile);
			menu.AddKey(KeyDiscard);
			menu.AddKey(KeyUpdate);
			menu.AddKey(KeyView);
			menu.AddKey(KeyViewModal);

			for (; ; menu.Items.Clear())
			{
				int group1 = -1;
				int indexLimit0 = int.MaxValue;
				foreach (var it in Record.GetHistory(null, DateTime.Now, (smart ? Factor1 : -1), Factor2))
				{
					// separator
					if (smart)
					{
						int group2 = it.Group(Limit0, Factor1, Factor2);
						if (group1 != group2)
						{
							if (group1 >= 0)
							{
								menu.Add("").IsSeparator = true;
								if (group2 == 0)
									indexLimit0 = menu.Items.Count;
							}
							group1 = group2;
						}
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
				if (menu.BreakKey == KeyUpdate)
				{
					var algo = new Actor(VesselHost.LogPath);
					algo.Update();
					continue;
				}

				// the file
				int indexSelected = menu.Selected;
				string path = menu.Items[indexSelected].Text;

				// delete:
				if (menu.BreakKey == KeyDiscard)
				{
					if (0 == Far.Net.Message("Discard " + path, "Confirm", MsgOptions.OkCancel))
					{
						Record.Remove(VesselHost.LogPath, path);
						continue;
					}

					goto show;
				}

				// go to:
				if (menu.BreakKey == KeyGoToFile)
				{
					Far.Net.Panel.GoToPath(path);
				}
				// view:
				else if (menu.BreakKey == KeyView || menu.BreakKey == KeyViewModal)
				{
					if (!File.Exists(path))
						continue;

					IViewer viewer = Far.Net.CreateViewer();
					viewer.FileName = path;

					if (menu.BreakKey == KeyViewModal)
					{
						viewer.DisableHistory = true;
						viewer.Open(OpenMode.Modal);
						goto show;
					}

					viewer.Open();

					// post fast training
					if (smart && indexSelected < indexLimit0)
						VesselHost.PathToTrain = path;
				}
				// edit:
				else
				{
					IEditor editor = Far.Net.CreateEditor();
					editor.FileName = path;

					if (menu.BreakKey == KeyEditModal)
					{
						editor.DisableHistory = true;
						editor.Open(OpenMode.Modal);
						goto show;
					}

					editor.Open();

					if (menu.BreakKey == KeyEditAdd)
						goto show;

					// post fast training
					if (smart && indexSelected < indexLimit0)
						VesselHost.PathToTrain = path;
				}

				return;
			}
		}

	}
}
