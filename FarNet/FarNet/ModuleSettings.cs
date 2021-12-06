
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet
{
	/// <summary>
	/// Module settings base class.
	/// </summary>
	/// <typeparam name="T">
	/// The data type.
	/// It must have the attribute <c>[Serializable]</c>.
	/// It may implement <see cref="IValidate"/> for data validation and completion.
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
	/// To exclude from the menu, use the attribute <c>[System.ComponentModel.Browsable(false)]</c>.
	/// </para>
	/// <para>
	/// If a module needs cached settings then use the public static property <c>Default</c>.
	/// Then settings opened and edited from the menu use and update this cached instance.
	/// Example:
	/// <code>
	/// public static Settings Default { get; } = new Settings();
	/// </code>
	/// The above code does not trigger reading from the file.
	/// Reading only happens on the first call of <see cref="GetData"/>.
	/// </para>
	/// <para>
	/// For migrating old data, override <see cref="UpdateData"/>.
	/// See its remarks for details.
	/// </para>
	/// <para>
	/// For data validation and completion the data type may implement <see cref="IValidate"/>.
	/// It is called when the data are deserialized or default created.
	/// </para>
	/// </remarks>
	public abstract class ModuleSettings<T> : ModuleSettingsBase where T : new()
	{
		/// <summary>
		/// Creates roaming settings.
		/// </summary>
		protected ModuleSettings() : base(typeof(T), new ModuleSettingsArgs())
		{ }

		/// <summary>
		/// Creates settings with the file.
		/// </summary>
		/// <param name="fileName">Settings file path.</param>
		protected ModuleSettings(string fileName) : base(typeof(T), new ModuleSettingsArgs { FileName = fileName })
		{ }

		/// <summary>
		/// Creates settings with the arguments.
		/// </summary>
		/// <param name="args">Module settings arguments.</param>
		protected ModuleSettings(ModuleSettingsArgs args) : base(typeof(T), args)
		{ }

		/// <summary>
		/// Gets the current settings data.
		/// </summary>
		/// <remarks>
		/// Do not cache the returned object, it is already cached internally.
		/// The current object may be different on next calls after updates.
		/// </remarks>
		public T GetData()
		{
			return (T)GetOrReadData();
		}

		/// <summary>
		/// It is called after deserializing from the file.
		/// </summary>
		/// <param name="data">Deserialized data.</param>
		/// <returns>True to save the updated data.</returns>
		/// <remarks>
		/// <para>
		/// For migrating old data, override this method.
		/// The data class usually have the property <c>Version</c>.
		/// If the saved is different from current, update the data.
		/// Use the XML from <see cref="ModuleSettingsBase.FileName"/>.
		/// See the module FarNet.Demo for the complete example.
		/// </para>
		/// </remarks>
		protected virtual bool UpdateData(T data)
		{
			return false;
		}

		internal sealed override object DoNewData()
		{
			return new T();
		}

		internal sealed override bool DoUpdateData(object data)
		{
			return UpdateData((T)data);
		}
	}
}
