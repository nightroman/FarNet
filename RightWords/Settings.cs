
/*
FarNet module RightWords
Copyright (c) 2011 Roman Kuzmin
*/

using System;
using System.Configuration;
using System.Text.RegularExpressions;
using FarNet.Settings;

namespace FarNet.RightWords
{
	[SettingsProvider(typeof(ModuleSettingsProvider))]
	public sealed class Settings : ModuleSettings
	{
		static readonly Settings _Default = new Settings();
		public static Settings Default { get { return _Default; } }
		[UserScopedSetting]
		[DefaultSettingValue(@"\p{Lu}?\p{Ll}+")]
		[SettingsManageability(SettingsManageability.Roaming)]
		public string WordPattern
		{
			get { return (string)this["WordPattern"]; }
			set { this["WordPattern"] = value; }
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		public override void Save()
		{
			try { new Regex(WordPattern); }
			catch (ArgumentException ex) { throw new ModuleException("Invalid word pattern: " + ex.Message); }

			base.Save();
		}
	}
}
