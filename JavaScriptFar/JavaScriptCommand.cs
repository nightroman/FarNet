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
		if (isDocument)
		{
			if (command.StartsWith("@debug:"))
			{
				isDebug = true;
				command = command[7..];
			}
			else
			{
				command = command[1..];
			}

			command = Environment.ExpandEnvironmentVariables(command.TrimStart());

			if (!Path.IsPathRooted(command))
				command = Path.GetFullPath(Path.Combine(Far.Api.CurrentDirectory, command));
		}

		Actor.Execute(new Actor.ExecuteArgs(command) { IsDocument = isDocument, IsDebug = isDebug });
	}
}
