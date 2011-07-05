
/*
FarNet module Vessel
Copyright (c) 2011 Roman Kuzmin
*/

using System;
using System.IO;

namespace FarNet.Vessel
{
	[System.Runtime.InteropServices.Guid("ec92417f-317b-4b97-b6d4-bcbd8f2f0f2d")]
	[ModuleHost(Load = true)]
	public class VesselHost : ModuleHost
	{
		const string NameLogFile = "VesselHistory.log";
		internal const string NameFactor = "Factor";
		internal const string NameLimits = "Limits";
		internal const string NameMaximumDayCount = "MaximumDayCount";
		internal const string NameMaximumFileCount = "MaximumFileCount";
		/// <summary>
		/// Gets or sets the disabled flag.
		/// </summary>
		public static bool IsDisabled { get; set; }
		/// <summary>
		/// Tells to train on closing
		/// </summary>
		internal static string PathToTrain { get; set; }
		static string _LogPath;
		internal static string LogPath
		{
			get { return VesselHost._LogPath; }
		}
		public override void Connect()
		{
			// ensure the log
			_LogPath = Path.Combine(Manager.GetFolderPath(SpecialFolder.LocalData, false), NameLogFile);
			if (!File.Exists(_LogPath))
				Record.CreateLogFile(_LogPath);

			// subscribe
			Far.Net.AnyViewer.Closed += OnViewerClosed;
			Far.Net.AnyEditor.Closed += OnEditorClosed;
		}
		static void OnViewerClosed(object sender, EventArgs e)
		{
			if (IsDisabled)
				return;

			IViewer viewer = (IViewer)sender;

			// not for history
			if (viewer.DisableHistory)
				return;

			// quick view, presumably
			if (Far.Net.Window.Kind == WindowKind.Panels)
				return;

			// go
			Update(DateTime.Now, 0, "view", viewer.FileName);
		}
		static void OnEditorClosed(object sender, EventArgs e)
		{
			if (IsDisabled)
				return;

			IEditor editor = (IEditor)sender;
			if (editor.DisableHistory)
				return;

			string path = editor.FileName;

			// skip ?New File?
			if (path.EndsWith("?", StringComparison.Ordinal))
				return;

			Update(DateTime.Now, editor.KeyCount, (editor.TimeOfSave == DateTime.MinValue ? "edit" : "save"), path);
		}
		static void Update(DateTime time, int keys, string what, string path)
		{
			if (path.StartsWith(Path.GetTempPath(), StringComparison.OrdinalIgnoreCase))
				return;

			Record.Write(_LogPath, time, keys, what, path);

			if (path.Equals(PathToTrain, StringComparison.OrdinalIgnoreCase))
			{
				PathToTrain = null;
				VesselTool.StartFastTraining();
			}
		}
	}
}
