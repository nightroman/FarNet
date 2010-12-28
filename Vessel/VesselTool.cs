
/*
FarNet module Vessel
Copyright (c) 2010 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

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

		public override void Invoke(object sender, ModuleToolEventArgs e)
		{
			IMenu menu = Far.Net.CreateMenu();
			menu.Title = "Vessel";
			menu.HelpTopic = HelpTopic + "MenuCommands";
			menu.Add("&1. Smart file history").Click += delegate { ShowHistory(VesselHost.Factor, VesselHost.Factor2); };
			menu.Add("&2. Plain file history").Click += delegate { ShowHistory(0, 0); };
			menu.Add("&3. Train smart history").Click += OnTrain;
			menu.Add("&0. Update history file").Click += OnUpdate;

			menu.Show();
		}

		void OnTrain(object sender, EventArgs e)
		{
			Result result = null;
			var algo = new Algo();
			algo.Progress = new Tools.ProgressForm();
			algo.Progress.Title = "Training";
			algo.Progress.Invoke(new System.Threading.ThreadStart(delegate { result = algo.Train(VesselHost.Limit1, VesselHost.Limit2); }));

			// save factors
			int factor1 = result.Target > 0 ? result.Factor1 : -1;
			VesselHost.SetFactors(factor1, result.Factor2);

			var text = string.Format(@"
Factor         : {0,6}
Up count       : {1,6}
Down count     : {2,6}
Same count     : {3,6}

Up sum         : {4,6}
Down sum       : {5,6}
Total sum      : {6,6}

Change average : {7,6:n2}
Global average : {8,6:n2}
",
 result.Factor1.ToString() + "/" + result.Factor2.ToString(),
 result.UpCount,
 result.DownCount,
 result.SameCount,
 result.UpSum,
 result.DownSum,
 result.TotalSum,
 result.ChangeAverage,
 result.GlobalAverage);

			Far.Net.Message(text, "Training results", MsgOptions.LeftAligned);
		}

		void OnUpdate(object sender, EventArgs e)
		{
			var text = Deal.Update(VesselHost.LogPath);
			Far.Net.Message(text, "Update", MsgOptions.LeftAligned);
		}

		void ShowHistory(int factor1, int factor2)
		{
			IListMenu menu = Far.Net.CreateListMenu();
			menu.HelpTopic = HelpTopic + "FileHistory";
			menu.SelectLast = true;
			menu.UsualMargins = true;
			if (factor1 < 0)
				menu.Title = "File history";
			else
				menu.Title = string.Format("File history (factor {0}/{1})", factor1, factor2);

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
				int recency = -1;
				foreach (var it in Deal.GetHistory(null, DateTime.Now, factor1, factor2))
				{
					// separator
					if (factor1 > 0)
					{
						int recency2 = it.Recency(factor1, factor2);
						if (recency != recency2)
						{
							if (recency >= 0)
								menu.Add("").IsSeparator = true;
							recency = recency2;
						}
					}

					// item
					menu.Add(it.Path).Checked = it.Frequency > 0;
				}

			show:

				if (!menu.Show() || menu.Selected < 0)
					return;

				// update:
				if (menu.BreakKey == KeyUpdate)
				{
					Deal.Update(VesselHost.LogPath);
					continue;
				}

				// the file
				string path = menu.Items[menu.Selected].Text;

				// delete:
				if (menu.BreakKey == KeyDiscard)
				{
					if (0 == Far.Net.Message("Discard " + path, "Confirm", MsgOptions.OkCancel))
					{
						Deal.Remove(VesselHost.LogPath, path);
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
				}

				return;
			}
		}

	}
}
