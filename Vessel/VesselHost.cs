
/*
FarNet module Vessel
Copyright (c) 2010 Roman Kuzmin
*/

using System;
using System.IO;

namespace FarNet.Vessel
{
	[System.Runtime.InteropServices.Guid("ec92417f-317b-4b97-b6d4-bcbd8f2f0f2d")]
	[ModuleHost(Load = true)]
	public class VesselHost : ModuleHost
	{
		const string LOG_FILE = "VesselHistory.log";
		internal const string REG_FACTOR = "Factor";

		/// <summary>
		/// Gets or sets the disabled flag.
		/// </summary>
		public static bool IsDisabled { get; set; }

		static string _LogPath;
		internal static string LogPath
		{
			get { return VesselHost._LogPath; }
		}

		static VesselHost Instance;
		public override void Connect()
		{
			// the instance
			Instance = this;

			// ensure the log
			_LogPath = Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), LOG_FILE);
			if (!File.Exists(_LogPath))
				Deal.CreateLogFile(_LogPath);

			// subscribe
			Far.Net.AnyViewer.Closed += OnViewerClosed;
			Far.Net.AnyEditor.Closed += OnEditorClosed;
		}

		static double _Factor_ = -1;
		public static double Factor
		{
			get
			{
				if (_Factor_ < 0)
				{
					using (var key = Instance.Manager.OpenRegistryKey(null, false))
						if (key != null)
							_Factor_ = double.Parse((string)key.GetValue(REG_FACTOR, "0"));
				}
				return _Factor_;
			}

			set
			{
				_Factor_ = value <= 1 ? 0 : value;
				using (var key = Instance.Manager.OpenRegistryKey(null, true))
					key.SetValue(VesselHost.REG_FACTOR, _Factor_.ToString());
			}
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
			Update(viewer.TimeOfOpen, 0, "view", viewer.FileName);
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

			Update(editor.TimeOfOpen, editor.KeyCount, (editor.TimeOfSave == DateTime.MinValue ? "edit" : "save"), path);
		}

		static void Update(DateTime time, int keys, string what, string path)
		{
			if (path.StartsWith(Path.GetTempPath(), StringComparison.OrdinalIgnoreCase))
				return;

			Deal.Write(_LogPath, time, keys, what, path);
		}

	}

}
