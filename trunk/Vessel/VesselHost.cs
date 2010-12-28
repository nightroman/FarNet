
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
		internal const string REG_LIMITS = "Limits";

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

		static int? _Factor1_;
		static int? _Factor2_;

		static void InitFactors()
		{
			using (var key = Instance.Manager.OpenRegistryKey(null, false))
			{
				if (key == null)
				{
					_Factor1_ = 0;
				}
				else
				{
					var data = ((string)key.GetValue(REG_FACTOR, "0/0")).Split(new char[] { '/' });
					_Factor1_ = int.Parse(data[0]);
					_Factor2_ = data.Length > 1 ? int.Parse(data[1]) : 0;
				}
			}
		}

		static public void SetFactors(int factor1, int factor2)
		{
			_Factor1_ = factor1;
			_Factor2_ = factor2;
			using (var key = Instance.Manager.OpenRegistryKey(null, true))
				key.SetValue(VesselHost.REG_FACTOR, factor1.ToString() + "/" + factor2.ToString());
		}

		public static int Factor
		{
			get
			{
				if (!_Factor1_.HasValue)
					InitFactors();

				return _Factor1_.Value;
			}
		}

		public static int Factor2
		{
			get
			{
				if (!_Factor2_.HasValue)
					InitFactors();

				return _Factor2_.Value;
			}
		}

		static int _Limit0_;
		static int _Limit1_;
		static int _Limit2_;

		static void InitLimits()
		{
			string text = null;
			using (var key = Instance.Manager.OpenRegistryKey(null, false))
			{
				if (key != null)
					text = (string)key.GetValue(REG_LIMITS, null);
			}

			if (text == null)
			{
				_Limit0_ = 2;
				_Limit1_ = 200;
				_Limit2_ = 30;
			}
			else
			{
				var data = text.Split(new char[] { '/' });
				_Limit0_ = int.Parse(data[0]);
				_Limit1_ = int.Parse(data[1]);
				_Limit2_ = int.Parse(data[2]);
			}
		}

		public static int Limit0
		{
			get
			{
				if (_Limit0_ == 0)
					InitLimits();

				return _Limit0_;
			}
		}

		public static int Limit1
		{
			get
			{
				if (_Limit0_ == 0)
					InitLimits();

				return _Limit1_;
			}
		}

		public static int Limit2
		{
			get
			{
				if (_Limit0_ == 0)
					InitLimits();

				return _Limit2_;
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

			Deal.Write(_LogPath, time, keys, what, path);
		}

	}

}
