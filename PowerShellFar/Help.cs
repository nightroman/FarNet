using FarNet;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace PowerShellFar;

/// <summary>
/// Help tools.
/// </summary>
static class Help
{
	internal static void ShowHelpForContext(string? defaultTopic = null)
	{
		var line = Far.Api.Line;
		if (line is null)
		{
			ShowAreaHelp();
		}
		else
		{
			// mind prefix to avoid parsing problems
			FarNet.Works.Kit.SplitCommandWithPrefix(line.Text, out var prefix, out var text, Entry.IsMyPrefix);
			ShowHelpForText(text, line.Caret - prefix.Length, defaultTopic);
		}
	}

	static bool ShowOnlineHelp(string command)
	{
		var po = (PSObject)ScriptBlock.Create("Get-Command $args[0] -ErrorAction 0 | Select-Object HelpUri, CommandType -First 1").InvokeReturnAsIs(command);
		if (po.Properties["CommandType"]?.Value?.ToString() == "Cmdlet")
		{
			var url = po.Properties["HelpUri"]?.Value?.ToString();
			if (url is not null && url.Contains("microsoft.com"))
			{
				My.ProcessEx.OpenBrowser(url);
				return true;
			}
		}
		return false;
	}

	internal static void ShowHelpForText(
		ReadOnlySpan<char> text,
		int pos,
		string? defaultTopic,
		OpenMode openMode = OpenMode.None)
	{
		// case: empty text
		text = text.TrimEnd();
		if (text.Length == 0)
		{
			if (defaultTopic is null)
				ShowAreaHelp();
			else
				Entry.Instance.ShowHelpTopic(defaultTopic);
			return;
		}

		// find the token
		Collection<PSToken> tokens = PSParser.Tokenize(text.ToString(), out _);
		var token = tokens.FirstOrDefault(token => pos >= (token.StartColumn - 1) && pos <= token.EndColumn);
		if (token is null)
			return;

		if (token.Type != PSTokenType.Command)
		{
			token = tokens.LastOrDefault(token => token.Type == PSTokenType.Command && token.EndColumn <= pos);
			if (token is null)
				return;
		}

		if (ShowOnlineHelp(token.Content))
			return;

		var title = $"Help {token.Content}";
		var script = "Get-Help $args[0] -Full | Out-File -LiteralPath $args[1] -Width $args[2]";
		var file = FarNet.Works.Kit.TempFileName("txt");
		try
		{
			A.InvokeCode(script, token.Content, file, Far.Api.UI.WindowSize.X - 2);
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
