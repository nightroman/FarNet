
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Threading;
using System.Threading.Tasks;

namespace FarNet.Works;

/// <summary>
/// INTERNAL
/// </summary>
public abstract class Far2
{
	static Far2 _Host;

	/// <summary>
	/// INTERNAL
	/// </summary>
	public static Far2 Api
	{
		get => _Host;
		set => _Host = _Host == null ? value : throw new InvalidOperationException();
	}

	/// <summary>
	/// Creates the module panel.
	/// </summary>
	public abstract IPanelWorks CreatePanel(Panel panel, Explorer explorer);

	/// <summary>
	/// Waits for posted steps to be invoked.
	/// </summary>
	public abstract Task WaitSteps();

	/// <summary>
	/// Posts macro and gets its wait handle.
	/// </summary>
	public abstract WaitHandle PostMacroWait(string macro);
}
