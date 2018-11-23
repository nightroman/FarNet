
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
		public int GetLimit(int mode)
		{
			return mode == 0 ? Limit1 : Limit2;
		}
		public int GetFactor(int mode)
		{
			return mode == 0 ? Factor1 : Factor2;
		}
		public void SetFactor(int mode, int value)
		{
			if (mode == 0)
				Factor1 = value;
			else
				Factor2 = value;
		}

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

		/// <summary>
		/// History training factor.
		/// </summary>
		[Browsable(false)]
		[UserScopedSetting]
		[DefaultSettingValue("0")]
		public int Factor1
		{
			get { return (int)this["Factor1"]; }
			set { this["Factor1"] = value; }
		}

		/// <summary>
		/// Folders training factor.
		/// </summary>
		[Browsable(false)]
		[UserScopedSetting]
		[DefaultSettingValue("0")]
		public int Factor2
		{
			get { return (int)this["Factor2"]; }
			set { this["Factor2"] = value; }
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

		/// <summary>
		/// Limit in hours for the second sort group for history.
		/// </summary>
		[UserScopedSetting]
		[DefaultSettingValue("96")]
		[SettingsManageability(SettingsManageability.Roaming)]
		public int Limit1
		{
			get { return (int)this["Limit1"]; }
			set { this["Limit1"] = value; }
		}

		/// <summary>
		/// Limit in hours for the second sort group for folders.
		/// </summary>
		[UserScopedSetting]
		[DefaultSettingValue("96")]
		[SettingsManageability(SettingsManageability.Roaming)]
		public int Limit2
		{
			get { return (int)this["Limit2"]; }
			set { this["Limit2"] = value; }
		}
	}
}
