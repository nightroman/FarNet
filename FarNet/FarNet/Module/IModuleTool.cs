
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Module tool runtime representation.
/// </summary>
/// <remarks>
/// It represents an auto registered <see cref="ModuleTool"/> or a tool registered by <see cref="IModuleManager.RegisterTool"/>.
/// It can be accessed by <see cref="IFar.GetModuleAction"/> from any module.
/// </remarks>
public interface IModuleTool : IModuleAction
{
	/// <summary>
	/// Processes the tool event.
	/// </summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The arguments.</param>
	void Invoke(object sender, ModuleToolEventArgs e);

	/// <summary>
	/// Gets the tool options. Setting is for internal use.
	/// </summary>
	ModuleToolOptions Options { get; set; }

	/// <summary>
	/// Gets the default tool options.
	/// </summary>
	ModuleToolOptions DefaultOptions { get; }
}
