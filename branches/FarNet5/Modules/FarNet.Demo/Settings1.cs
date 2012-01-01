
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using FarNet.Settings;
namespace FarNet.Demo
{
	/// <summary>
	/// Settings set 1.
	/// It uses some standard settings (supported by Visual Studio designer)
	/// and shows how to use various settings property attributes.
	/// </summary>
	[SettingsProvider(typeof(ModuleSettingsProvider))]
	public class Settings1 : ModuleSettings
	{
		#region [Default]
		/// <summary>
		/// The only settings instance.
		/// Normally settings are created once, when needed.
		/// </summary>
		/// <remarks>
		/// Use <see cref="SettingsBase.Synchronized"/> in multithreaded scenarious, see <see cref="Settings2._Default"/>.
		/// </remarks>
		static readonly Settings1 _Default = new Settings1();
		/// <summary>
		/// Gets the public access to the settings instance.
		/// It is used for example by the core in order to open the settings panel.
		/// </summary>
		public static Settings1 Default { get { return _Default; } }
		#endregion
		#region [Save]
		/// <summary>
		/// Override this method to perform data validation.
		/// Throw on errors. Call the base on success.
		/// </summary>
		public override void Save()
		{
			if (IntLocal < 0)
				throw new ModuleException("Negative 'IntLocal' is invalid.");

			if (IntRoaming < 0)
				throw new ModuleException("Negative 'IntRoaming' is invalid.");

			base.Save();
		}
		#endregion
		#region [Bool]
		/// <summary>
		/// Local Boolean value.
		/// </summary>
		[UserScopedSetting]
		public bool BoolLocal
		{
			get { return (bool)this["BoolLocal"]; }
			set { this["BoolLocal"] = value; }
		}
		/// <summary>
		/// Local Boolean value.
		/// </summary>
		[UserScopedSetting]
		[DefaultSettingValue("true")]
		[SettingsManageability(SettingsManageability.Roaming)]
		public bool BoolRoaming
		{
			get { return (bool)this["BoolRoaming"]; }
			set { this["BoolRoaming"] = value; }
		}
		#endregion
		#region [Int]
		/// <summary>
		/// Local Int32 value.
		/// </summary>
		[UserScopedSetting]
		public int IntLocal
		{
			get { return (int)this["IntLocal"]; }
			set { this["IntLocal"] = value; }
		}
		/// <summary>
		/// Roaming Int32 value with the default.
		/// </summary>
		[UserScopedSetting]
		[DefaultSettingValue("42")]
		[SettingsManageability(SettingsManageability.Roaming)]
		public int IntRoaming
		{
			get { return (int)this["IntRoaming"]; }
			set { this["IntRoaming"] = value; }
		}
		#endregion
		#region [DateTime]
		/// <summary>
		/// Local DateTime value.
		/// </summary>
		[UserScopedSetting]
		public DateTime DateTimeLocal
		{
			get { return (DateTime)this["DateTimeLocal"]; }
			set { this["DateTimeLocal"] = value; }
		}
		/// <summary>
		/// Roaming DateTime value with the default.
		/// </summary>
		[UserScopedSetting]
		[DefaultSettingValue("2000-11-22")]
		[SettingsManageability(SettingsManageability.Roaming)]
		public DateTime DateTimeRoaming
		{
			get { return (DateTime)this["DateTimeRoaming"]; }
			set { this["DateTimeRoaming"] = value; }
		}
		#endregion
		#region [Double]
		/// <summary>
		/// Local Double value.
		/// </summary>
		[UserScopedSetting]
		public double DoubleLocal
		{
			get { return (double)this["DoubleLocal"]; }
			set { this["DoubleLocal"] = value; }
		}
		/// <summary>
		/// Roaming Double value with the default.
		/// </summary>
		[UserScopedSetting]
		[DefaultSettingValue("3.14159265358979")]
		[SettingsManageability(SettingsManageability.Roaming)]
		public double DoubleRoaming
		{
			get { return (double)this["DoubleRoaming"]; }
			set { this["DoubleRoaming"] = value; }
		}
		#endregion
		#region [String]
		/// <summary>
		/// Local string value.
		/// </summary>
		[UserScopedSetting]
		public string StringLocal
		{
			get { return (string)this["StringLocal"]; }
			set { this["StringLocal"] = value; }
		}
		/// <summary>
		/// Roaming string value with the default.
		/// </summary>
		[UserScopedSetting]
		[DefaultSettingValue("Line1\r\nLine2")]
		[SettingsManageability(SettingsManageability.Roaming)]
		public string StringRoaming
		{
			get { return (string)this["StringRoaming"]; }
			set { this["StringRoaming"] = value; }
		}
		#endregion
		#region [StringCollection]
		/// <summary>
		/// Not browsable in UI local collection.
		/// </summary>
		[Browsable(false)]
		[UserScopedSetting]
		public StringCollection StringCollectionLocal
		{
			get { return (StringCollection)this["StringCollectionLocal"]; }
			set { this["StringCollectionLocal"] = value; }
		}
		/// <summary>
		/// Not browsable in UI roaming collection with the default XML string.
		/// This is just to try and test that the default XML really works,
		/// see <see cref="ModuleSettingsProvider"/> remarks for caveats.
		/// </summary>
		[Browsable(false)]
		[UserScopedSetting]
		[SettingsManageability(SettingsManageability.Roaming)]
		[DefaultSettingValue(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfString xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
<string>Line1</string>
<string>Line2</string>
</ArrayOfString>")]
		public StringCollection StringCollectionRoaming
		{
			get { return (StringCollection)this["StringCollectionRoaming"]; }
			set { this["StringCollectionRoaming"] = value; }
		}
		#endregion
	}
}
