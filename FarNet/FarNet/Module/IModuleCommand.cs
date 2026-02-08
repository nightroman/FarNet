namespace FarNet;

/// <summary>
/// Module command runtime representation.
/// </summary>
/// <remarks>
/// It represents an auto registered <see cref="ModuleCommand"/> or a command registered by <see cref="IModuleManager.RegisterCommand"/>.
/// It can be accessed by <see cref="IFar.GetModuleAction"/> from any module.
/// </remarks>
public interface IModuleCommand : IModuleAction
{
	/// <summary>
	/// Processes the command event.
	/// </summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The arguments.</param>
	void Invoke(object sender, ModuleCommandEventArgs e);

	/// <summary>
	/// Gets the command prefix. Setting is for internal use.
	/// </summary>
	string Prefix { get; set; }
}
