/*
PowerShellFar plugin for Far Manager
Copyright (C) 2006-2009 Roman Kuzmin
*/

using System;
using System.Collections;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace PowerShellFar
{
	/// <summary>
	/// Job command (a command name or a script block).
	/// </summary>
	/// <remarks>
	/// Normally you should not create instances directly.
	/// When needed they are created internally from strings or script blocks.
	/// </remarks>
	public sealed class JobCommand
	{
		bool IsScript;

		/// <summary>
		/// Command name or code.
		/// </summary>
		public string Command { get; private set; }

		/// <summary>
		/// Creates a command from a command name.
		/// </summary>
		/// <param name="commandName">Command name or script file name/path.</param>
		public JobCommand(string commandName)
		{
			if (commandName == null)
				throw new ArgumentNullException("commandName");

			Command = commandName;
		}

		/// <summary>
		/// Creates a command from a script block.
		/// </summary>
		/// <param name="scriptBlock">Job script block.</param>
		public JobCommand(ScriptBlock scriptBlock)
		{
			if (scriptBlock == null)
				throw new ArgumentNullException("scriptBlock");

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
			if (commandText == null)
				throw new ArgumentNullException("commandText");

			Command = commandText;
			IsScript = isScript;
		}

		internal PowerShell Add(PowerShell shell)
		{
			return IsScript ? shell.AddScript(Command) : shell.AddCommand(Command);
		}
	}
}
