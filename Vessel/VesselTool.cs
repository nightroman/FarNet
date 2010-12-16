
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
			menu.Add("&1. Smart file history").Click += delegate { ShowHistory(VesselHost.Factor); };
			menu.Add("&2. Plain file history").Click += delegate { ShowHistory(0); };
			menu.Add("&3. Train smart history").Click += OnQualify;
			menu.Add("&0. Update history file").Click += OnUpdate;

			menu.Show();
		}

		void OnQualify(object sender, EventArgs e)
		{
			var factors = new double[28];
			for (int i = 0; i < factors.Length; ++i)
				factors[i] = i + 2;

			var stats = Deal.Qualify(null, 0, factors);

			// Maximize (Up - 2 * Down), not (Up - Down), i.e. put more penalty on Down.
			// (Up - Down) maximum is often found at 25: it is usually only a bit better
			// than at small factors but it makes the list very different from the plain.
			var target = stats.Max(x => x.UpSum - 2 * x.DownSum);
			var stat = stats.First(x => x.UpSum - 2 * x.DownSum == target);

			double factor = stat.TotalAverage > 0 ? stat.Factor : 0;
			VesselHost.Factor = factor;

			var text = string.Format(@"
Factor         : {0,6}
Up count       : {1,6}
Down count     : {2,6}
Same count     : {3,6}

Up sum         : {4,6}
Down sum       : {5,6}
Total sum      : {6,6}

Up average     : {7,6:n2}
Down average   : {8,6:n2}
Total average  : {9,6:n2}
Global average : {10,6:n2}
",
 stat.Factor,
 stat.UpCount,
 stat.DownCount,
 stat.SameCount,
 stat.UpSum,
 stat.DownSum,
 stat.TotalSum,
 stat.UpAverage,
 stat.DownAverage,
 stat.TotalAverage,
 stat.GlobalAverage);

			Far.Net.Message(text, "Training results", MsgOptions.LeftAligned);
		}

		void OnUpdate(object sender, EventArgs e)
		{
			var text = Deal.Update(VesselHost.LogPath);
			Far.Net.Message(text, "Update", MsgOptions.LeftAligned);
		}

		void ShowHistory(double factor)
		{
			IListMenu menu = Far.Net.CreateListMenu();
			menu.Title = string.Format("File history (factor {0})", factor);
			menu.HelpTopic = HelpTopic + "FileHistory";
			menu.SelectLast = true;
			menu.UsualMargins = true;

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
				foreach (var it in Deal.GetHistory(null, DateTime.Now, factor))
					menu.Add(it.Path).Checked = it.Frequency > 0;

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
