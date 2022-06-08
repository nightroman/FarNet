
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>
/// Arguments of a module command event.
/// </summary>
public class ModuleCommandEventArgs : EventArgs
{
	/// <param name="command">See <see cref="Command"/></param>
	public ModuleCommandEventArgs(string command)
	{
		Command = command;
	}
	/// <summary>
	/// Gets the command text.
	/// </summary>
	public string Command { get; private set; }
	/// <summary>
	/// Tells that command is called by <c>Plugin.Call()</c>.
	/// </summary>
	public bool IsMacro { get; set; }
	/// <summary>
	/// Tells to ignore the call and allows alternative actions.
	/// </summary>
	/// <remarks>
	/// This flag is used when the command is called from a macro.
	/// <para>
	/// A handler sets this to true to tell that nothing is done and
	/// it makes sense for a caller to perform an alternative action.
	/// </para>
	/// <para>
	/// Note: this is not the case when processing has started and failed;
	/// the handler should either throw an exception or keep this value as false:
	/// fallback actions make no sense, the problems have to be resolved instead.
	/// </para>
	/// </remarks>
	public bool Ignore { get; set; }
}
