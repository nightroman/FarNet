using System;
using System.IO;
using FarNet;

namespace JavaScriptFar;

[System.Runtime.InteropServices.Guid("ce853894-1ff2-4713-96d1-64cd97bc9f89")]
[ModuleCommand(Name = "Execute file", Prefix = Prefix)]
public class JavaScriptCommand : ModuleCommand
{
	internal const string Prefix = "js";

	public override void Invoke(object sender, ModuleCommandEventArgs e)
	{
		var command = e.Command.Trim();

		bool isDocument = command.StartsWith('@');
		if (isDocument)
		{
			command = Environment.ExpandEnvironmentVariables(command[1..].TrimStart());
			if (!Path.IsPathRooted(command))
				command = Path.GetFullPath(Path.Combine(Far.Api.CurrentDirectory, command));
		}

		Actor.Execute(new Actor.ExecuteArgs(command) { IsDocument = isDocument });
	}
}
