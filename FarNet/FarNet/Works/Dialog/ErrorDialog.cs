namespace FarNet.Works;
#pragma warning disable 1591

public static class ErrorDialog
{
	public static void Show(string? title, Exception error, string more)
	{
		// for module exceptions show just [OK]
		var moduleError = error as ModuleException;
		string[] buttons =
			moduleError is { } && (moduleError.InnerException is null || moduleError.InnerException is ModuleException) ?
			["OK"] :
			["OK", "More"];

		// resolve title
		if (string.IsNullOrEmpty(title))
			title = moduleError?.Source ?? error.GetType().FullName;

		// ask
		int res = Far.Api.Message(
			error.Message,
			title,
			MessageOptions.Warning,
			buttons
		);
		if (res < 1)
			return;

		// write error text
		using var writer = new StringWriter();
		{
			// info
			Kit.WriteException(writer, error);

			// more
			if (more is { })
			{
				writer.WriteLine();
				writer.WriteLine(more);
				writer.WriteLine();
			}

			// full
			var errorText = Kit.FilterExceptionString(error.ToString());
			writer.WriteLine();
			writer.Write(errorText);
		}

		// open error text editor
		_ = Far.Api.AnyEditor.EditTextAsync(new()
		{
			Text = writer.ToString(),
			Title = error.GetType().FullName,
			IsLocked = true
		});
	}
}
