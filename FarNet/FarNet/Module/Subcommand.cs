namespace FarNet;

/// <summary>
/// Subcommand used by <see cref="ModuleCommand"/>.
/// </summary>
public abstract class Subcommand
{
	/// <summary>
	/// Invokes the subcommand.
	/// </summary>
	public abstract void Invoke();
}
