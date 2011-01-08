
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
		const int DefaultMaximumDayCount = 30;
		const int DefaultMaximumFileCount = 512;

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

		static VesselHost Instance;
		public override void Connect()
		{
			// the instance
			Instance = this;

			// ensure the log
			_LogPath = Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), NameLogFile);
			if (!File.Exists(_LogPath))
				Record.CreateLogFile(_LogPath);

			// subscribe
			Far.Net.AnyViewer.Closed += OnViewerClosed;
			Far.Net.AnyEditor.Closed += OnEditorClosed;
		}

		static int GetInt(string propertyName, int defaultValue)
		{
			using (var key = Instance.Manager.OpenRegistryKey(null, false))
			{
				if (key == null)
					return defaultValue;

				var data = key.GetValue(propertyName, null);
				if (data == null)
					return defaultValue;

				return int.Parse(data.ToString());
			}
		}

		public static int MaximumDayCount { get { return GetInt(NameMaximumDayCount, DefaultMaximumDayCount); } }
		public static int MaximumFileCount { get { return GetInt(NameMaximumFileCount, DefaultMaximumFileCount); } }

		static int? _Factor1_;
		static int? _Factor2_;

		static void InitFactors()
		{
			using (var key = Instance.Manager.OpenRegistryKey(null, false))
			{
				if (key == null)
				{
					_Factor1_ = -1;
					_Factor2_ = -1;
				}
				else
				{
					var data = ((string)key.GetValue(NameFactor, "-1/-1")).Split(new char[] { '/' });
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
				key.SetValue(VesselHost.NameFactor, factor1.ToString() + "/" + factor2.ToString());
		}

		public static int Factor1
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
					text = (string)key.GetValue(NameLimits, null);
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

			Record.Write(_LogPath, time, keys, what, path);

			if (path.Equals(PathToTrain, StringComparison.OrdinalIgnoreCase))
			{
				PathToTrain = null;
				VesselTool.StartFastTraining();
			}
		}

	}

}
