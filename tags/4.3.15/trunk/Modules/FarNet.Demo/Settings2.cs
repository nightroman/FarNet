
using System;
using System.Configuration;
using FarNet.Settings;
namespace FarNet.Demo
{
	/// <summary>
	/// Settings set 2 (modules may have any number of settings sets).
	/// Goal: show and test various ways to serialize setting values.
	/// </summary>
	[SettingsProvider(typeof(ModuleSettingsProvider))]
	public class Settings2 : ModuleSettings
	{
		#region [Default]
		/// <summary>
		/// Synchronized version of the single settings instance for multithreaded scenarious.
		/// </summary>
		static readonly Settings2 _Default = (Settings2)SettingsBase.Synchronized(new Settings2());
		/// <summary>
		/// Gets the public access to the settings instance.
		/// It is used for example by the core in order to open the settings panel.
		/// </summary>
		public static Settings2 Default { get { return _Default; } }
		#endregion
		#region [DateTime]
		/// <summary>
		/// DateTime serialized as string (default and recommended)
		/// </summary>
		[UserScopedSetting]
		public DateTime DateTimeAsString
		{
			get { return (DateTime)this["DateTimeAsString"]; }
			set { this["DateTimeAsString"] = value; }
		}
		/// <summary>
		/// DateTime serialized as binary (just for testing)
		/// </summary>
		[UserScopedSetting]
		[SettingsSerializeAs(SettingsSerializeAs.Binary)]
		public DateTime DateTimeAsBinary
		{
			get { return (DateTime)this["DateTimeAsBinary"]; }
			set { this["DateTimeAsBinary"] = value; }
		}
		/// <summary>
		/// DateTime serialized as XML (just for testing)
		/// </summary>
		[UserScopedSetting]
		[SettingsSerializeAs(SettingsSerializeAs.Xml)]
		public DateTime DateTimeAsXml
		{
			get { return (DateTime)this["DateTimeAsXml"]; }
			set { this["DateTimeAsXml"] = value; }
		}
		#endregion
		#region [Decimal]
		/// <summary>
		/// Decimal serialized as string.
		/// </summary>
		[UserScopedSetting]
		public decimal DecimalAsString
		{
			get { return (decimal)this["DecimalAsString"]; }
			set { this["DecimalAsString"] = value; }
		}
		#endregion
		#region [Guid]
		/// <summary>
		/// Guid value serialized as string.
		/// </summary>
		[UserScopedSetting]
		public Guid GuidAsString
		{
			get { return (Guid)this["GuidAsString"]; }
			set { this["GuidAsString"] = value; }
		}
		#endregion
		#region [Int]
		/// <summary>
		/// Int32 serialized as string
		/// </summary>
		[UserScopedSetting]
		public int IntAsString
		{
			get { return (int)this["IntAsString"]; }
			set { this["IntAsString"] = value; }
		}
		#endregion
		#region [String]
		/// <summary>
		/// String serialized as string
		/// </summary>
		[UserScopedSetting]
		public string StringAsString
		{
			get { return (string)this["StringAsString"]; }
			set { this["StringAsString"] = value; }
		}
		#endregion
		#region [User]
		/// <summary>
		/// User data serialized as XML.
		/// This class uses standard XML serialization.
		/// </summary>
		[UserScopedSetting]
		public UserDataXml UserAsXml
		{
			get { return (UserDataXml)this["UserAsXml"]; }
			set { this["UserAsXml"] = value; }
		}
		/// <summary>
		/// User data serialized as binary.
		/// Standard binary serialization could be used in this case
		/// but the class uses custom serialization, just for testing.
		/// </summary>
		[UserScopedSetting]
		[SettingsSerializeAs(SettingsSerializeAs.Binary)]
		public UserDataBinary UserAsBinary
		{
			get { return (UserDataBinary)this["UserAsBinary"]; }
			set { this["UserAsBinary"] = value; }
		}
		#endregion
	}
}
