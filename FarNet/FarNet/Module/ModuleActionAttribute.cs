
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>
/// Any action attribute parameters.
/// </summary>
public abstract class ModuleActionAttribute : Attribute, ICloneable
{
	/// <summary>
	/// The mandatory action name shown in menus.
	/// </summary>
	/// <remarks>
	/// <para>
	/// If the module uses this name itself, for example as message boxes titles, then define this text
	/// as a public const string in a class, then use its name as the value of this attribute parameter.
	/// </para>
	/// </remarks>
	public string Name { get; set; }
	/// <summary>
	/// Tells to use the <see cref="Name"/> as the resource name of the localized string.
	/// </summary>
	/// <remarks>
	/// Restart Far after changing the current Far language or the module culture
	/// to make sure that this and other action names are updated from resources.
	/// </remarks>
	public bool Resources { get; set; }
	/// <summary>
	/// Calls <see cref="object.MemberwiseClone"/>.
	/// </summary>
	public object Clone() { return MemberwiseClone(); }
}
