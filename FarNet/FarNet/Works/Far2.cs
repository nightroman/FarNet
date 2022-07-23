
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
		get { return _Host; }
		set
		{
			if (_Host != null) throw new InvalidOperationException();
			_Host = value;
		}
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
