
// JavaScriptFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System;
using System.Data.Common;
using System.IO;

namespace JavaScriptFar;

[System.Runtime.InteropServices.Guid("ce853894-1ff2-4713-96d1-64cd97bc9f89")]
[ModuleCommand(Name = "Run JavaScript", Prefix = Res.Prefix)]
public class JavaScriptCommand : ModuleCommand
{
	public override void Invoke(object sender, ModuleCommandEventArgs e)
	{
		var text = e.Command.Trim();
		var args = new ExecuteArgs();

		// task?
		if (text.StartsWith("task:"))
		{
			args.IsTask = true;
			text = text[5..].TrimStart();
		}

		// args?
		int index = text.IndexOf("::");
		if (index >= 0)
		{
			var connectionString = text[(index + 2)..].TrimStart();
			try
			{
				args.Parameters = new DbConnectionStringBuilder() { ConnectionString = connectionString };
			}
			catch (Exception ex)
			{
				throw new ModuleException($"Error in parameters:\n{connectionString}\n{ex.Message}");
			}

			text = text[0..index].TrimEnd();
		}

		// document?
		if (text.StartsWith('@'))
		{
			text = text[1..].TrimStart();

			text = Environment.ExpandEnvironmentVariables(text);

			if (!Path.IsPathRooted(text))
				text = Path.GetFullPath(Path.Combine(Far.Api.CurrentDirectory, text));

			args.Document = text;
		}
		else
		{
			args.Command = text;
		}

		Actor.Execute(args);
	}
}
