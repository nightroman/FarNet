
// JavaScriptFar module for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.IO;
using FarNet;

namespace JavaScriptFar;

[System.Runtime.InteropServices.Guid("ce853894-1ff2-4713-96d1-64cd97bc9f89")]
[ModuleCommand(Name = "Execute file", Prefix = Res.Prefix)]
public class JavaScriptCommand : ModuleCommand
{
	public override void Invoke(object sender, ModuleCommandEventArgs e)
	{
		var command = e.Command.Trim();

		bool isDocument = command.StartsWith('@');
		bool isDebug = false;
		bool isTask = false;
		if (isDocument)
		{
			command = command[1..].TrimStart();
			while (true)
			{
				if (command.StartsWith("debug:"))
				{
					isDebug = true;
					command = command[6..].TrimStart();
					continue;
				}

				if (command.StartsWith("task:"))
				{
					isTask = true;
					command = command[5..].TrimStart();
					continue;
				}

				break;
			}

			command = Environment.ExpandEnvironmentVariables(command);

			if (!Path.IsPathRooted(command))
				command = Path.GetFullPath(Path.Combine(Far.Api.CurrentDirectory, command));
		}

		Actor.Execute(new ExecuteArgs(command) { IsDocument = isDocument, IsDebug = isDebug, IsTask = isTask });
	}
}
