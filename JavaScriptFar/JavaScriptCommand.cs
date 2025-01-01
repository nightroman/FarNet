using FarNet;
using System;

namespace JavaScriptFar;

[ModuleCommand(Name = "Run JavaScript", Prefix = Res.Prefix, Id = "ce853894-1ff2-4713-96d1-64cd97bc9f89")]
public class JavaScriptCommand : ModuleCommand
{
	public override void Invoke(object sender, ModuleCommandEventArgs e)
	{
		try
		{
			var text = e.Command.AsSpan().Trim();
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
				args.Parameters = CommandParameters.ParseParameters(text[(index + 2)..].TrimStart().ToString());
				text = text[0..index].TrimEnd();
			}

			// document?
			if (text.StartsWith('@'))
			{
				args.Document = Far.Api.FS.GetFullPath(Environment.ExpandEnvironmentVariables(text[1..].TrimStart().ToString()));
			}
			else
			{
				args.Command = text.ToString();
			}

			Actor.Execute(args);
		}
		catch (Exception ex)
		{
			throw new ModuleException(ex.Message, ex);
		}
	}
}
