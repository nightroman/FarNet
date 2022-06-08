
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace PowerShellFar
{
	/// <summary>
	/// Help tools.
	/// </summary>
	static class Help
	{
		internal static void ShowHelpForContext(string defaultTopic = null)
		{
			var line = Far.Api.Line;
			if (line == null)
			{
				ShowAreaHelp();
			}
			else
			{
				// mind prefix to avoid parsing problems
				var split = Zoo.SplitCommandWithPrefix(line.Text);
				ShowHelpForText(split.Value, line.Caret - split.Key.Length, defaultTopic);
			}
		}

		internal static void ShowHelpForText(
			string text,
			int pos,
			string defaultTopic,
			OpenMode openMode = OpenMode.None)
		{
			// case: empty text
			text = text.TrimEnd();
			if (text.Length == 0)
			{
				if (defaultTopic == null)
					ShowAreaHelp();
				else
					Entry.Instance.ShowHelpTopic(defaultTopic);
				return;
			}

			// find the token
			Collection<PSToken> tokens = PSParser.Tokenize(text, out _);
			var token = tokens.FirstOrDefault(token => pos >= (token.StartColumn - 1) && pos <= token.EndColumn);
			if (token == null)
				return;

			string title;
			string script;
			object[] args = null;

			if (token.Type == PSTokenType.Command)
			{
				title = $"Help {token.Content}";
				script = "Get-Help $args[1] -Full > $args[0]";
				args = new object[] { null, token.Content };
			}
			else if (token.Type == PSTokenType.CommandParameter)
			{
				string parameter = token.Content.TrimStart('-');
				string upper = parameter.ToUpperInvariant();
				if (upper == "CONFIRM" ||
					upper == "DEBUG" ||
					upper == "ERRORACTION" ||
					upper == "ERRORVARIABLE" ||
					upper == "INFORMATIONACTION" ||
					upper == "INFORMATIONVARIABLE" ||
					upper == "OUTBUFFER" ||
					upper == "OUTVARIABLE" ||
					upper == "PIPELINEVARIABLE" ||
					upper == "VERBOSE" ||
					upper == "WARNINGACTION" ||
					upper == "WARNINGVARIABLE" ||
					upper == "WHATIF")
				{
					title = "Help about_CommonParameters";
					script = "Get-Help about_CommonParameters > $args[0]";
					args = new object[] { null };
				}
				else
				{
					var command = tokens.LastOrDefault(token => token.Type == PSTokenType.Command && token.EndColumn <= pos);
					if (command == null)
						return;

					title = $"Help {command.Content} -{parameter}";
					script = "Get-Help $args[1] -Parameter $args[2] > $args[0]";
					args = new object[] { null, command.Content, parameter };
				}
			}
			else if (token.Type == PSTokenType.Keyword)
			{
				title = $"Help about_{token.Content}";
				script = $"Get-Help about_{token.Content} > $args[0]";
				args = new object[] { null };
			}
			else if (token.Type == PSTokenType.Operator)
			{
				title = "Help about_operators";
				script = "Get-Help about_operators > $args[0]";
				args = new object[] { null };
			}
			else
			{
				Far.Api.Message("No help targets found at the editor caret position.", Res.Me);
				return;
			}

			var file = FarNet.Works.Kit.TempFileName("txt");
			try
			{
				args[0] = file;
				A.InvokeCode(script, args);
				ShowHelpFile(file, title, openMode);
			}
			catch (RuntimeException)
			{
				if (File.Exists(file))
					File.Delete(file);
			}
		}

		// Why editor and .txt:
		// - can copy all or parts
		// - .txt is known to Colorer
		static void ShowHelpFile(string fileName, string title, OpenMode openMode)
		{
			var editor = Far.Api.CreateEditor();
			editor.FileName = fileName;
			editor.DeleteSource = DeleteSource.File;
			editor.DisableHistory = true;
			editor.IsLocked = true;
			editor.Title = title;
			editor.Open(openMode);
		}

		static void ShowAreaHelp()
		{
			switch (Far.Api.Window.Kind)
			{
				case WindowKind.Panels:
					Entry.Instance.ShowHelpTopic(HelpTopic.CommandLine);
					return;
				default:
					Entry.Instance.ShowHelpTopic(HelpTopic.Contents);
					return;
			}
		}
	}
}
