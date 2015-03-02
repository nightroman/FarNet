
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2015 Roman Kuzmin
*/

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar
{
	/// <summary>
	/// Help tools.
	/// </summary>
	static class Help
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		internal static void ShowHelpForContext(string defaultTopic = null)
		{
			ILine line = Far.Api.Line;
			if (line == null)
			{
				ShowAreaHelp();
				return;
			}

			// line text, replace prefixes with spaces to avoid parsing problems
			string text = line.Text;
			string prefix = string.Empty;
			if (line.WindowKind == WindowKind.Panels)
				Entry.SplitCommandWithPrefix(ref text, out prefix);

			// trim end and process the empty case
			text = text.TrimEnd();
			if (text.Length == 0)
			{
				if (defaultTopic == null)
					ShowAreaHelp();
				else
					Far.Api.ShowHelpTopic(defaultTopic);
				return;
			}

			int pos = line.Caret - prefix.Length;
			string script = null;
			string command = null;
			object[] args = null;
			Collection<PSParseError> errors;
			Collection<PSToken> tokens = PSParser.Tokenize(text, out errors);
			foreach (PSToken token in tokens)
			{
				if (token.Type == PSTokenType.Command)
					command = token.Content;

				if (pos >= (token.StartColumn - 1) && pos <= token.EndColumn)
				{
					if (token.Type == PSTokenType.Command)
					{
						//! Call the Help function, just in case it is redefined.
						//! It used to call now retired Get-FarHelp.
						script = "Help $args[1] -Full > $args[0]";
						args = new object[] { null, command };
					}
					else if (token.Type == PSTokenType.CommandParameter)
					{
						string parameter = token.Content.TrimStart('-').ToUpperInvariant();
						if (parameter == "VERBOSE" ||
							parameter == "DEBUG" ||
							parameter == "ERRORACTION" ||
							parameter == "ERRORVARIABLE" ||
							parameter == "WARNINGACTION" ||
							parameter == "WARNINGVARIABLE" ||
							parameter == "OUTVARIABLE" ||
							parameter == "OUTBUFFER" ||
							parameter == "WHATIF" ||
							parameter == "CONFIRM")
						{
							script = "Get-Help about_CommonParameters > $args[0]";
							args = new object[] { null };
						}
						else
						{
							script = "Get-Help $args[1] -Parameter $args[2] > $args[0]";
							args = new object[] { null, command, parameter };
						}
					}
					else if (token.Type == PSTokenType.Keyword)
					{
						script = string.Format(null, "Get-Help about_{0} > $args[0]", token.Content);
						args = new object[] { null };
					}
					else if (token.Type == PSTokenType.Operator)
					{
						script = "Get-Help about_operators > $args[0]";
						args = new object[] { null };
					}

					break;
				}
			}

			if (script == null)
			{
				Far.Api.Message("No help targets found at the editor caret position.", Res.Me);
				return;
			}

			bool ok = false;
			string file = Path.GetTempFileName();
			try
			{
				args[0] = file;
				A.InvokeCode(script, args);
				ok = true;
			}
			catch (RuntimeException)
			{ }
			finally
			{
				if (!ok && File.Exists(file))
					File.Delete(file);
			}

			if (ok)
			{
				IViewer viewer = Far.Api.CreateViewer();
				viewer.FileName = file;
				viewer.DeleteSource = DeleteSource.File;
				viewer.DisableHistory = true;
				viewer.Title = "Help";
				viewer.Open();
			}
		}

		static void ShowAreaHelp()
		{
			switch (Far.Api.Window.Kind)
			{
				case WindowKind.Panels:
					Far.Api.ShowHelpTopic("CommandLine");
					return;
				default:
					Far.Api.ShowHelpTopic("Contents");
					return;
			}
		}
	}
}
