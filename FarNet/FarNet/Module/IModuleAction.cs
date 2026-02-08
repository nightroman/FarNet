namespace FarNet;

/// <summary>
/// Module action runtime representation.
/// </summary>
/// <remarks>
/// Any registered module action has its runtime representation, one of this inderface descendants.
/// These representation interfaces are not directly related to action classes or handlers, they only represent them.
/// <para>
/// Action representations can be requested by their IDs by <see cref="IFar.GetModuleAction"/>.
/// </para>
/// </remarks>
public interface IModuleAction
{
	/// <summary>
	/// Gets the action ID.
	/// </summary>
	Guid Id { get; }

	/// <summary>
	/// Gets the action name.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Gets the action kind.
	/// </summary>
	ModuleItemKind Kind { get; }

	/// <summary>
	/// Gets the module manager.
	/// </summary>
	IModuleManager Manager { get; }

	/// <summary>
	/// Unregisters the module action dynamically.
	/// </summary>
	/// <remarks>
	/// Normally it is used for temporary actions dynamically registered by <c>Register*()</c>.
	/// <para>
	/// Note that module hosts on disconnection does not have to unregister registered actions.
	/// </para>
	/// </remarks>
	void Unregister();
}
