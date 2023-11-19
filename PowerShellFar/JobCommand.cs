
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Management.Automation;

namespace PowerShellFar;

/// <summary>
/// Job command (a command name or a script block).
/// </summary>
/// <remarks>
/// Normally you should not create instances directly.
/// When needed they are created internally from strings or script blocks.
/// </remarks>
public sealed class JobCommand
{
	readonly bool IsScript;

	/// <summary>
	/// Command name or code.
	/// </summary>
	public string Command { get; }

	/// <summary>
	/// Creates a command from a command name.
	/// </summary>
	/// <param name="commandName">Command name or script file name/path.</param>
	public JobCommand(string commandName)
	{
		Command = commandName ?? throw new ArgumentNullException(nameof(commandName));
	}

	/// <summary>
	/// Creates a command from a script block.
	/// </summary>
	/// <param name="scriptBlock">Job script block.</param>
	public JobCommand(ScriptBlock scriptBlock)
	{
		ArgumentNullException.ThrowIfNull(scriptBlock);

		Command = scriptBlock.ToString();
		IsScript = true;
	}

	/// <summary>
	/// Creates a command from text.
	/// </summary>
	/// <param name="commandText">Cmdlet/script name or script code.</param>
	/// <param name="isScript">Command text is script code.</param>
	public JobCommand(string commandText, bool isScript)
	{
		Command = commandText ?? throw new ArgumentNullException(nameof(commandText));

		IsScript = isScript;
	}

	internal PowerShell Add(PowerShell shell)
	{
		return IsScript ? shell.AddScript(Command) : shell.AddCommand(Command);
	}
}
