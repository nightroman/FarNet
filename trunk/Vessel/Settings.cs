
/*
FarNet module Vessel
Copyright (c) 2011-2012 Roman Kuzmin
*/

using System;
using System.ComponentModel;
using System.Configuration;
using FarNet.Settings;

namespace FarNet.Vessel
{
	[SettingsProvider(typeof(ModuleSettingsProvider))]
	public sealed class Settings : ModuleSettings
	{
		static readonly Settings _Default = new Settings();
		public static Settings Default { get { return _Default; } }
		[Browsable(false)]
		[UserScopedSetting]
		public DateTime LastUpdateTime
		{
			get { return (DateTime)this["LastUpdateTime"]; }
			set { this["LastUpdateTime"] = value; }
		}
		[Browsable(false)]
		[UserScopedSetting]
		[DefaultSettingValue("-1")]
		public int Factor1
		{
			get { return (int)this["Factor1"]; }
			set { this["Factor1"] = value; }
		}
		[Browsable(false)]
		[UserScopedSetting]
		[DefaultSettingValue("-1")]
		public int Factor2
		{
			get { return (int)this["Factor2"]; }
			set { this["Factor2"] = value; }
		}
		[UserScopedSetting]
		[DefaultSettingValue("30")]
		[SettingsManageability(SettingsManageability.Roaming)]
		public int MaximumDayCount
		{
			get { return (int)this["MaximumDayCount"]; }
			set { this["MaximumDayCount"] = value; }
		}
		[UserScopedSetting]
		[DefaultSettingValue("512")]
		[SettingsManageability(SettingsManageability.Roaming)]
		public int MaximumFileCount
		{
			get { return (int)this["MaximumFileCount"]; }
			set { this["MaximumFileCount"] = value; }
		}
		[UserScopedSetting]
		[DefaultSettingValue("2")]
		[SettingsManageability(SettingsManageability.Roaming)]
		public int Limit0
		{
			get { return (int)this["Limit0"]; }
			set { this["Limit0"] = value; }
		}
		[UserScopedSetting]
		[DefaultSettingValue("200")]
		[SettingsManageability(SettingsManageability.Roaming)]
		public int Limit1
		{
			get { return (int)this["Limit1"]; }
			set { this["Limit1"] = value; }
		}
		[UserScopedSetting]
		[DefaultSettingValue("30")]
		[SettingsManageability(SettingsManageability.Roaming)]
		public int Limit2
		{
			get { return (int)this["Limit2"]; }
			set { this["Limit2"] = value; }
		}
	}
}
