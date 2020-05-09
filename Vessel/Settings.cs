
// FarNet module Vessel
// Copyright (c) Roman Kuzmin

using System;
using System.ComponentModel;
using System.Configuration;
using FarNet.Settings;

namespace FarNet.Vessel
{
	[SettingsProvider(typeof(ModuleSettingsProvider))]
	public sealed class Settings : ModuleSettings
	{
		// synchronized settings because we use threads for training and saving
		static readonly Settings _Default = (Settings)SettingsBase.Synchronized(new Settings());
		public static Settings Default { get { return _Default; } }

		/// <summary>
		/// History log last update time.
		/// </summary>
		[Browsable(false)]
		[UserScopedSetting]
		public DateTime LastUpdateTime1
		{
			get { return (DateTime)this["LastUpdateTime1"]; }
			set { this["LastUpdateTime1"] = value; }
		}

		/// <summary>
		/// Folders log last update time.
		/// </summary>
		[Browsable(false)]
		[UserScopedSetting]
		public DateTime LastUpdateTime2
		{
			get { return (DateTime)this["LastUpdateTime2"]; }
			set { this["LastUpdateTime2"] = value; }
		}

		[UserScopedSetting]
		[DefaultSettingValue("42")]
		[SettingsManageability(SettingsManageability.Roaming)]
		public int MaximumDayCount
		{
			get { return (int)this["MaximumDayCount"]; }
			set { this["MaximumDayCount"] = value; }
		}

		[UserScopedSetting]
		[DefaultSettingValue("365")]
		[SettingsManageability(SettingsManageability.Roaming)]
		public int MaximumFileAge
		{
			get { return (int)this["MaximumFileAge"]; }
			set { this["MaximumFileAge"] = value; }
		}

		[UserScopedSetting]
		[DefaultSettingValue("1000")]
		[SettingsManageability(SettingsManageability.Roaming)]
		public int MaximumFileCount
		{
			get { return (int)this["MaximumFileCount"]; }
			set { this["MaximumFileCount"] = value; }
		}

		/// <summary>
		/// Limit in hours for the first sort group.
		/// The default and recommended value is 2.
		/// </summary>
		[UserScopedSetting]
		[DefaultSettingValue("2")]
		[SettingsManageability(SettingsManageability.Roaming)]
		public int Limit0
		{
			get { return (int)this["Limit0"]; }
			set { this["Limit0"] = value; }
		}
	}
}
