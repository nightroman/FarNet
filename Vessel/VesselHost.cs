
// FarNet module Vessel
// Copyright (c) Roman Kuzmin

using System;
using System.IO;

namespace FarNet.Vessel
{
	[System.Runtime.InteropServices.Guid("ec92417f-317b-4b97-b6d4-bcbd8f2f0f2d")]
	[ModuleHost(Load = true)]
	public class VesselHost : ModuleHost
	{
		const string NameLogFile1 = "VesselHistory.txt";
		const string NameLogFile2 = "VesselFolders.txt";
		internal const string NameFactor = "Factor";
		internal const string NameLimits = "Limits";
		internal const string NameMaximumDayCount = "MaximumDayCount";
		internal const string NameMaximumFileCount = "MaximumFileCount";
		/// <summary>
		/// Gets or sets the disabled flag.
		/// </summary>
		public static bool IsDisabled { get; set; }
		internal static string[] LogPath { get; private set; }
		public override void Connect()
		{
			var dir = Manager.GetFolderPath(SpecialFolder.LocalData, true);
			LogPath = new string[] { Path.Combine(dir, NameLogFile1), Path.Combine(dir, NameLogFile2) };

			// ensure logs
			if (!File.Exists(LogPath[0]))
				Store.CreateLogFile(0, LogPath[0]);
			if (!File.Exists(LogPath[1]))
				Store.CreateLogFile(1, LogPath[1]);

			// subscribe
			Far.Api.AnyViewer.Closed += OnViewerClosed;
			Far.Api.AnyEditor.Closed += OnEditorClosed;
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
			if (Far.Api.Window.Kind == WindowKind.Panels)
				return;

			// go
			Update(DateTime.Now, Record.VIEW, viewer.FileName);
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

			Update(DateTime.Now, (editor.TimeOfSave == DateTime.MinValue ? Record.EDIT : Record.SAVE), path);
		}
		static void Update(DateTime time, string what, string path)
		{
			if (!path.StartsWith(Path.GetTempPath(), StringComparison.OrdinalIgnoreCase))
				Store.Append(LogPath[0], time, what, path);
		}
	}
}
