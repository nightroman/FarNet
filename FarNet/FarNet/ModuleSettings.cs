
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System.ComponentModel;

namespace FarNet
{
	/// <summary>
	/// Module settings base class.
	/// </summary>
	/// <typeparam name="T">
	/// The serializable settings data type.
	/// It may implement <see cref="IValidate"/>.
	/// </typeparam>
	/// <remarks>
	/// See the <see href="https://github.com/nightroman/FarNet/tree/master/Modules/FarNet.Demo">FarNet.Demo</see> module
	/// for examples of browsable roaming settings (user preferences) and non-browsable local settings (working data).
	/// Other modules with settings: Drawer, FSharpFar, RightControl, RightWords, Vessel.
	/// <para>
	/// Choose the derived class name carefully, it is used as the display name
	/// in the settings menu and as the base file name where the data are stored.
	/// </para>
	/// <para>
	/// Stored file locations:
	/// (1) roaming - <c>%FARPROFILE%\FarNet\{ModuleName}\{SettingsName}.xml</c>;
	/// (2) local - <c>%FARLOCALPROFILE%\FarNet\{ModuleName}\{SettingsName}.xml</c>.
	/// <para>
	/// Settings are roaming by default.
	/// To make local, use the base constructor with <see cref="ModuleSettingsArgs.IsLocal"/> set to true.
	/// </para>
	/// </para>
	/// <para>
	/// Settings are browsable by default, i.e. shown in the settings menu and may be opened for editing.
	/// To exclude from the menu, use the attribute <see cref="BrowsableAttribute"/> with false.
	/// </para>
	/// <para>
	/// If a module uses cached settings then the recommended way is public static property <c>Default</c>.
	/// Then settings opened and edited from the settings menu use and update this cached instance.
	/// Example:
	/// <code>
	/// public static Settings Default { get; } = new Settings();
	/// </code>
	/// The above code does not trigger reading from the file.
	/// Reading only happens on the first call of <see cref="GetData"/>.
	/// </para>
	/// </remarks>
	public abstract class ModuleSettings<T> : ModuleSettingsBase
	{
		/// <summary>
		/// Creates roaming settings.
		/// </summary>
		protected ModuleSettings() : base(typeof(T), new ModuleSettingsArgs())
		{ }

		/// <summary>
		/// Creates settings with the specified arguments.
		/// </summary>
		/// <param name="args">Module settings arguments, e.g. for making local settings.</param>
		protected ModuleSettings(ModuleSettingsArgs args) : base(typeof(T), args)
		{ }

		/// <summary>
		/// Gets the current settings data.
		/// </summary>
		/// <remarks>
		/// Do not cache the returned object, it is cached internally, properly.
		/// Because the current object may change on next calls after updates.
		/// </remarks>
		public T GetData()
		{
			return (T)GetOrReadData();
		}
	}
}
