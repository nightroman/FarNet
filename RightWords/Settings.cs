
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
		[SettingsManageability(SettingsManageability.Roaming)]
		public string SkipPattern
		{
			get { return (string)this["SkipPattern"]; }
			set { this["SkipPattern"] = value; }
		}
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
			if (!string.IsNullOrEmpty(SkipPattern))
			{
				try { new Regex(SkipPattern, RegexOptions.IgnorePatternWhitespace); }
				catch (ArgumentException ex) { throw new ModuleException("Invalid skip pattern: " + ex.Message); }
			}

			if (WordPattern == null || WordPattern.Trim().Length == 0)
				throw new ModuleException("Empty word pattern is invalid.");
			
			try { new Regex(WordPattern, RegexOptions.IgnorePatternWhitespace); }
			catch (ArgumentException ex) { throw new ModuleException("Invalid word pattern: " + ex.Message); }

			base.Save();
		}
	}
}
