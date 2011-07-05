
/*
FarNet module RightControl
Copyright (c) 2010-2011 Roman Kuzmin
*/

using System.Configuration;
using FarNet.Settings;

namespace FarNet.RightControl
{
	[SettingsProvider(typeof(ModuleSettingsProvider))]
	public sealed class Settings : ModuleSettings
	{
		internal const string RegexDefault = @"^ | $ | (?<=\b|\s)\S";
		[UserScopedSetting]
		[DefaultSettingValue(RegexDefault)]
		[SettingsManageability(SettingsManageability.Roaming)]
		public string Regex
		{
			get { return (string)this["Regex"]; }
			set { this["Regex"] = value; }
		}
	}
}
