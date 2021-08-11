
// FarNet module RightControl
// Copyright (c) Roman Kuzmin

using System;
using System.Configuration;
using System.Text.RegularExpressions;
using FarNet.Settings;

namespace FarNet.RightControl
{
	[SettingsProvider(typeof(ModuleSettingsProvider))]
	public sealed class Settings : ModuleSettings
	{
		// default patterns
		const string RegexLeftDefault = @"^ | $ | (?<=\b|\s)\S";
		const string RegexRightDefault = @"^ | $ | (?<=\b|\s)\S";

		// cached regexes
		Regex _regexLeft;
		Regex _regexRight;

		public static Settings Default { get; } = new Settings();

		[UserScopedSetting]
		[DefaultSettingValue(RegexLeftDefault)]
		[SettingsManageability(SettingsManageability.Roaming)]
		public string RegexLeft
		{
			get { return (string)this[nameof(RegexLeft)]; }
			set { this[nameof(RegexLeft)] = value; }
		}

		[UserScopedSetting]
		[DefaultSettingValue(RegexRightDefault)]
		[SettingsManageability(SettingsManageability.Roaming)]
		public string RegexRight
		{
			get { return (string)this[nameof(RegexRight)]; }
			set { this[nameof(RegexRight)] = value; }
		}

		public Regex GetRegexLeft()
		{
			if (_regexLeft == null)
				InitRegex(true);

			return _regexLeft;
		}

		public Regex GetRegexRight()
		{
			if (_regexRight == null)
				InitRegex(true);

			return _regexRight;
		}

		void InitRegex(bool resetOnErrors)
		{
			try
			{
				_regexLeft = new Regex(RegexLeft, RegexOptions.IgnorePatternWhitespace);
			}
			catch (Exception ex)
			{
				Far.Api.Message(
					$"{nameof(RegexLeft)} error:\r{ex.Message}",
					"RightControl",
					MessageOptions.LeftAligned | MessageOptions.Warning);

				if (resetOnErrors)
					_regexLeft = new Regex(RegexLeftDefault, RegexOptions.IgnorePatternWhitespace);
			}

			try
			{
				_regexRight = new Regex(RegexRight, RegexOptions.IgnorePatternWhitespace);
			}
			catch (Exception ex)
			{
				Far.Api.Message(
					$"{nameof(RegexRight)} error:\r{ex.Message}",
					"RightControl",
					MessageOptions.LeftAligned | MessageOptions.Warning);

				if (resetOnErrors)
					_regexRight = new Regex(RegexRightDefault, RegexOptions.IgnorePatternWhitespace);
			}
		}

		public override void Save()
		{
			InitRegex(false);
			base.Save();
		}
	}
}
