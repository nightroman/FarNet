namespace FarNet.Works;
#pragma warning disable 1591

public static class ErrorDialog
{
	public static void Show(string? title, Exception error, string more)
	{
		// unwrap
		error = Kit.UnwrapAggregateException(error);

		// for module exceptions show just [OK]
		var moduleError = error as ModuleException;
		string[] buttons =
			moduleError is { } && (moduleError.InnerException is null || moduleError.InnerException is ModuleException) ?
			["OK"] :
			["OK", "More"];

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

		var errorText = Kit.FilterExceptionString(error.ToString());
		writer.WriteLine();
		writer.Write(errorText);

		// show text in the editor
		Far.Api.AnyEditor.EditText(new EditTextArgs()
		{
			Text = writer.ToString(),
			Title = error.GetType().FullName,
			IsLocked = true
		});
	}
}
