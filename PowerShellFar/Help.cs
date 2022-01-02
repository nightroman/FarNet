
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
				return;
			}

			// line text, mind prefixes to avoid parsing problems
			string text = line.Text;
			string prefix = string.Empty;
			if (line.WindowKind == WindowKind.Panels)
				Entry.SplitCommandWithPrefix(ref text, out prefix);

			int pos = line.Caret - prefix.Length;
			ShowHelpForText(text, pos, defaultTopic);
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
					Far.Api.ShowHelpTopic(defaultTopic);
				return;
			}

			// find the token
			Collection<PSToken> tokens = PSParser.Tokenize(text, out _);
			var token = tokens.FirstOrDefault(token => pos >= (token.StartColumn - 1) && pos <= token.EndColumn);
			if (token == null)
				return;

			string script;
			object[] args = null;

			if (token.Type == PSTokenType.Command)
			{
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
					script = "Get-Help about_CommonParameters > $args[0]";
					args = new object[] { null };
				}
				else
				{
					var command = tokens.LastOrDefault(token => token.Type == PSTokenType.Command && token.EndColumn <= pos);
					if (command == null)
						return;

					script = "Get-Help $args[1] -Parameter $args[2] > $args[0]";
					args = new object[] { null, command.Content, parameter };
				}
			}
			else if (token.Type == PSTokenType.Keyword)
			{
				script = $"Get-Help about_{token.Content} > $args[0]";
				args = new object[] { null };
			}
			else if (token.Type == PSTokenType.Operator)
			{
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
				ShowHelpFile(file, openMode);
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
		static void ShowHelpFile(string fileName, OpenMode openMode)
		{
			var editor = Far.Api.CreateEditor();
			editor.FileName = fileName;
			editor.DeleteSource = DeleteSource.File;
			editor.DisableHistory = true;
			editor.IsLocked = true;
			editor.Title = "Help";
			editor.Open(openMode);
		}

		static void ShowAreaHelp()
		{
			switch (Far.Api.Window.Kind)
			{
				case WindowKind.Panels:
					Far.Api.ShowHelpTopic(HelpTopic.CommandLine);
					return;
				default:
					Far.Api.ShowHelpTopic(HelpTopic.Contents);
					return;
			}
		}
	}
}
