
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet.Works;
#pragma warning disable 1591

public static class Test
{
	static void AssertNormalPanel(IPanel panel, string active)
	{
		if (panel.IsPlugin)
		{
			throw new InvalidOperationException($"Expected {active} panel type: Native, actual: Plugin.");
		}
		if (panel.Kind != PanelKind.File)
		{
			throw new InvalidOperationException($"Expected {active} panel kind: File, actual: {panel.Kind}.");
		}
		if (!panel.IsVisible)
		{
			throw new InvalidOperationException($"Expected {active} panel state: Visible, actial: Hidden.");
		}
	}

	public static void AssertNormalState()
	{
		//! test kind first
		if (Far.Api.Window.Kind != WindowKind.Panels)
		{
			throw new InvalidOperationException($"Expected window: Panels, actual: {Far.Api.Window.Kind}.");
		}
		// 2 = Panels + Desktop
		if (Far.Api.Window.Count != 2)
		{
			throw new InvalidOperationException("Expected no windows but Panels.");
		}
		AssertNormalPanel(Far.Api.Panel, "active");
		AssertNormalPanel(Far.Api.Panel2, "passive");
	}
}
