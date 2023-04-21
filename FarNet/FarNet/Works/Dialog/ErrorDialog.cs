
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.IO;

namespace FarNet.Works;
#pragma warning disable 1591

public static class ErrorDialog
{
	public static void Show(string? title, Exception error, string more)
	{
		// unwrap
		error = Kit.UnwrapAggregateException(error);

		// special treatment of module error
		var moduleError = error as ModuleException;
		var buttons =
			moduleError != null && moduleError.InnerException == null ?
			new string[] { "OK" } :
			new string[] { "OK", "More"};

		// resolve title
		if (string.IsNullOrEmpty(title))
		{
			if (moduleError == null)
			{
				title = error.GetType().FullName;
			}
			else
			{
				title = moduleError.Source;
			}
		}

		// ask
		int res = Far.Api.Message(
			error.Message,
			title,
			MessageOptions.LeftAligned | MessageOptions.Warning,
			buttons
		);
		if (res < 1)
			return;

		// write error text
		var writer = new StringWriter();
		Log.FormatException(writer, error);
		if (more != null)
		{
			writer.WriteLine();
			writer.WriteLine(more);
			writer.WriteLine();
		}

		writer.WriteLine();
		writer.WriteLine(error.ToString());

		// show text in the editor
		Far.Api.AnyEditor.EditText(new EditTextArgs()
		{
			Text = writer.ToString(),
			Title = error.GetType().FullName,
			IsLocked = true
		});
	}
}
