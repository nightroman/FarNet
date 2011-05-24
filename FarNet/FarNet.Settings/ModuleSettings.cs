
/*
FarNet.Settings library for FarNet
Copyright (c) 2011 Roman Kuzmin
*/

using System;
using System.Configuration;
using System.Runtime.InteropServices;

namespace FarNet.Settings
{
	/// <summary>
	/// Module settings base class.
	/// </summary>
	/// <remarks>
	/// How to make a module settings class and setting properties
	/// <ul>
	/// <li>Derive the settings class from <see cref="ModuleSettings"/>.</li>
	/// <li>Set <see cref="ModuleSettingsProvider"/> attribute for the class.</li>
	/// <li>Use <c>UserScopedSetting</c> attribute to define setting properties.</li>
	/// <li>Use <c>DefaultSettingValue</c> attribute to define default values.</li>
	/// <li>Use <c>SettingsManageability</c> attribute for roaming settings.</li>
	/// <li>Use <c>Browsable(false)</c> attribute to exclude settings from UI.</li>
	/// <li>
	/// Use <c>SettingsSerializeAs(String|Xml|Binary)</c> attribute to specify serialization.
	/// Normally value and primitive types are serialized as strings, complex types are serialized as XML.
	/// </li>
	/// </ul>
	/// <para>
	/// A module may have more than one settings classes, if needed.
	/// In that case they are shown in the settings menu separately
	/// and each settings set has its own settings panel.
	/// </para>
	/// <para>
	/// Settings class names are used as settings file names in the local and roaming module directories.
	/// Thus, choose names carefully and avoid future renamings (it is fine to rename namespaces, though).
	/// </para>
	/// <para>
	/// It is fine to add new setting properties, they will appear with their default values.
	/// It is fine to remove or rename properties, the old data will be removed from settings on saving.
	/// </para>
	/// <para>
	/// Use primitive types convertible to and from strings or serializable types.
	/// Settings for user changes in UI should be convertible to and from strings.
	/// Do not change types or type members significantly, this may not work well.
	/// Use custom types in settings carefully, avoid renamings and brute changes,
	/// or design serialization to be tolerant to such changes.
	/// </para>
	/// <para>
	/// Override the <c>Save</c> method in order to perform data validation.
	/// Throw informative exceptions on errors. Call the base method on success.
	/// </para>
	/// <para>
	/// If the settings class should have a single instance then use the public static property <c>Default</c>, see the example.
	/// The settings panel looks for this property and uses its instance. If it is not found then a new instance is created
	/// (in this case the class must have the default constructor).
	/// </para>
	/// <example>
	/// <code>
	///[SettingsProvider(typeof(ModuleSettingsProvider))]
	///public class Settings : ModuleSettings
	///{
	///	static readonly Settings _Default = new Settings();
	///	public static Settings Default { get { return _Default; } }
	///	[UserScopedSetting]
	///	[DefaultSettingValue("2000-11-22")]
	///	public DateTime DateTimeValue
	///	{
	///		get { return (DateTime)this["DateTimeValue"]; }
	///		set { this["DateTimeValue"] = value; }
	///	}
	///	[UserScopedSetting]
	///	[DefaultSettingValue("42")]
	///	[SettingsManageability(SettingsManageability.Roaming)]
	///	public int IntValue
	///	{
	///		get { return (int)this["IntValue"]; }
	///		set { this["IntValue"] = value; }
	///	}
	///}
	/// </code>
	/// </example>
	/// </remarks>
	[ComVisible(false)]
	public abstract class ModuleSettings : ApplicationSettingsBase
	{
		///
		protected ModuleSettings()
		{
			var append = "\\" + GetType().Name + ".resources";
			Context[ModuleSettingsProvider.LocalFileName] = Manager.GetFolderPath(SpecialFolder.LocalData) + append;
			Context[ModuleSettingsProvider.RoamingFileName] = Manager.GetFolderPath(SpecialFolder.RoamingData) + append;
		}
		/// <summary>
		/// Gets the module manager.
		/// </summary>
		protected IModuleManager Manager
		{
			get { return _Manager ?? (_Manager = Far.Net.GetModuleManager(GetType())); }
		}
		IModuleManager _Manager;
		/// <summary>
		/// Seals the context.
		/// </summary>
		public sealed override SettingsContext Context
		{
			get { return base.Context; }
		}
	}
}
