
/*
FarNet module Drawer
Copyright (c) 2012 Roman Kuzmin
*/

using System;
using System.Configuration;
using System.Text.RegularExpressions;
using FarNet.Settings;
namespace FarNet.Drawer
{
	[SettingsProvider(typeof(ModuleSettingsProvider))]
	public sealed class Settings : ModuleSettings
	{
		public const string CurrentWordGuid = "a9a6f877-e049-4438-a315-d5914b200988";
		public const string CurrentWordName = "Current word";
		static readonly Settings _Default = new Settings();
		public static Settings Default { get { return _Default; } }
		[UserScopedSetting]
		[DefaultSettingValue(@"\w[-\w]*")]
		[SettingsManageability(SettingsManageability.Roaming)]
		public string CurrentWordPattern
		{
			get { return (string)this["CurrentWordPattern"]; }
			set { this["CurrentWordPattern"] = value; }
		}
		[UserScopedSetting]
		[DefaultSettingValue("Black")]
		[SettingsManageability(SettingsManageability.Roaming)]
		public ConsoleColor CurrentWordColorForeground
		{
			get { return (ConsoleColor)this["CurrentWordColorForeground"]; }
			set { this["CurrentWordColorForeground"] = value; }
		}
		[UserScopedSetting]
		[DefaultSettingValue("Gray")]
		[SettingsManageability(SettingsManageability.Roaming)]
		public ConsoleColor CurrentWordColorBackground
		{
			get { return (ConsoleColor)this["CurrentWordColorBackground"]; }
			set { this["CurrentWordColorBackground"] = value; }
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		public override void Save()
		{
			if (CurrentWordPattern == null || CurrentWordPattern.Trim().Length == 0)
				throw new ModuleException("Empty current word pattern is invalid.");

			try { new Regex(CurrentWordPattern, RegexOptions.IgnorePatternWhitespace); }
			catch (ArgumentException ex) { throw new ModuleException("Invalid current word pattern: " + ex.Message); }

			base.Save();
		}
	}
}
