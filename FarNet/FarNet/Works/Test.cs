namespace FarNet.Works;
#pragma warning disable 1591

public static class Test
{
	private static string? ErrorNormalPanel(IPanel? panel, string active)
	{
		if (panel is null)
			return "Expected panel.";

		if (panel.IsPlugin)
			return $"Expected {active} panel type: Native, actual: Plugin.";

		if (panel.Kind != PanelKind.File)
			return $"Expected {active} panel kind: File, actual: {panel.Kind}.";

		if (!panel.IsVisible)
			return $"Expected {active} panel state: Visible, actual: Hidden.";

		if (panel.SelectedFirst)
			return $"Expected {active} panel SelectedFirst: Off, actual: On.";

		return null;
	}

	private static void AssertNormalPanel(IPanel? panel, string active)
	{
		for (int i = 20; --i >= 0; Thread.Sleep(200))
		{
			var error = ErrorNormalPanel(panel, active);
			if (error is null)
				return;

			if (i == 0)
				throw new InvalidOperationException(error);
		}
	}

	public static void AssertNormalState()
	{
		//! test kind first
		if (Far.Api.Window.Kind != WindowKind.Panels)
			throw new InvalidOperationException($"Expected window: Panels, actual: {Far.Api.Window.Kind}.");

		// 2 = Panels + Desktop
		if (Far.Api.Window.Count != 2)
			throw new InvalidOperationException("Expected no windows but Panels.");

		AssertNormalPanel(Far.Api.Panel, "active");
		AssertNormalPanel(Far.Api.Panel2, "passive");
	}
}
