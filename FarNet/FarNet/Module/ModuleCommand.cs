
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// A command called by its prefix from command lines and macros.
/// </summary>
/// <remarks>
/// The <see cref="Invoke"/> method has to be implemented.
/// <para>
/// Commands are called by their prefixes from command lines: the panel
/// command line and user menu and file association commands. Macros call
/// commands by <c>Plugin.Call()</c> (see the FarNet manual).
/// </para>
/// <para>
/// Use <see cref="ModuleCommandAttribute"/> and specify <see cref="ModuleActionAttribute.Id"/>,
/// <see cref="ModuleActionAttribute.Name"/>, and the default command prefix <see cref="ModuleCommandAttribute.Prefix"/>.
/// </para>
/// </remarks>
public abstract class ModuleCommand : ModuleAction
{
	/// <summary>
	/// Command handler called from the command line with a prefix.
	/// </summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The arguments.</param>
	public abstract void Invoke(object sender, ModuleCommandEventArgs e);
}
