
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Threading;
using System.Threading.Tasks;

namespace FarNet.Works;
#pragma warning disable 1591

public abstract class Far2
{
	static Far2? _Host;

	public static Far2 Api
	{
		get => _Host!;
		set => _Host = _Host == null ? value : throw new InvalidOperationException();
	}

	// Creates the module panel.
	public abstract IPanelWorks CreatePanel(Panel panel, Explorer explorer);

	// Waits for posted steps to be invoked.
	public abstract Task WaitSteps();

	// Posts macro and gets its wait handle.
	public abstract WaitHandle PostMacroWait(string macro);
}
