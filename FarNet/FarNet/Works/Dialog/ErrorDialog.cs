namespace FarNet.Works;
#pragma warning disable 1591

public static class ErrorDialog
{
	public static void Show(string? title, Exception ex, string more)
	{
		// for module exceptions show just [OK]
		var moduleError = ex as ModuleException;
		string[] buttons =
			moduleError is { } && (moduleError.InnerException is null || moduleError.InnerException is ModuleException) ?
			["OK"] :
			["OK", "More"];

		// resolve title
		if (string.IsNullOrEmpty(title))
			title = moduleError?.Source ?? ex.GetType().FullName;

		// ask
		int res = Far.Api.Message(
			ex.Message,
			title,
			MessageOptions.Warning,
			buttons
		);
		if (res < 1)
			return;

		// write text for editor
		using var writer = new StringWriter();
		{
			// info
			Kit.WriteException(writer, ex);

			// more
			if (more is { })
			{
				writer.WriteLine();
				writer.WriteLine(more);
				writer.WriteLine();
			}

			// full
			var exToString = Kit.FilterExceptionString(ex.ToString());
			writer.WriteLine();
			writer.Write(exToString);
		}

		// open text editor
		_ = Far.Api.AnyEditor.EditTextAsync(new()
		{
			Text = writer.ToString(),
			Title = title,
			IsLocked = true
		});
	}
}
