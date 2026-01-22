
using System;

namespace FarNet.Demo;

/// <summary>
/// This module host is created on the first module action.
/// </summary>
public sealed class DemoHost : ModuleHost, IDisposable
{
	/// <summary>
	/// Initializes the module.
	/// </summary>
	public DemoHost()
	{
		// update the local settings
		var settings = new Workings();
		var data = settings.GetData();
		data.LastLoadTime = DateTime.Now;
		data.LoadCount += 1;
		settings.Save();
	}

	/// <summary>
	/// Used to release resources.
	/// </summary>
	public void Dispose()
	{
	}
}
