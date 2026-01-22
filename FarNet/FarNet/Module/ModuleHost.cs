namespace FarNet;

/// <summary>
/// The module host. At most one public descendant can be implemented by a module.
/// </summary>
/// <remarks>
/// In many cases the module actions should be implemented instead of the host
/// (see predefined descendants of <see cref="ModuleAction"/>).
/// <para>
/// If <see cref="ToLoad"/> is true then the host is always loaded.
/// If it is false then the host is loaded on the first module action.
/// </para>
/// <para>
/// Implement module initialization in the default constructor.
/// If needed, implement <see cref="IDisposable"/> for unloading.
/// </para>
/// </remarks>
public abstract class ModuleHost : BaseModuleItem
{
	/// <summary>
	/// Obsolete, use the constructor.
	/// </summary>
	[Obsolete("Use the constructor.")]
	public virtual void Connect()
	{
	}

	/// <summary>
	/// Tells to always load the module.
	/// </summary>
	public virtual bool ToLoad { get; }

	/// <summary>
	/// Tells to call <see cref="UseEditors"/> on first editor opening.
	/// </summary>
	public virtual bool ToUseEditors { get; }

	/// <summary>
	/// Called on first editor opening if <see cref="ToUseEditors"/> is true.
	/// </summary>
	public virtual void UseEditors() { }

	/// <summary>
	/// Obsolete, use <see cref="IDisposable"/>.
	/// </summary>
	[Obsolete("Use IDisposable.")]
	public virtual void Disconnect()
	{
	}

	/// <summary>
	/// Called before invocation of any module action.
	/// </summary>
	/// <remarks>
	/// The module may override this method to perform preparation procedures.
	/// Normally this is not needed for a simple module with a single action.
	/// It is useful when a complex module provides several actions and
	/// wants common steps to be performed by this method.
	/// <para>
	/// NOTE: This method is called only for module actions:
	/// <c>Invoke()</c> methods and handlers registered by <c>Register*()</c> methods.
	/// It is not called on events added by a module to editors, viewers, dialogs or panels.
	/// </para>
	/// <para>
	/// Example: PowerShellFar starts loading of the PowerShell engine in a background thread on connection.
	/// This method waits for the engine loading to complete, if needed. Registered PowerShellFar actions
	/// simply assume that the engine is already loaded. But editor event handlers still have to care.
	/// </para>
	/// </remarks>
	public virtual void Invoking()
	{
	}

	/// <summary>
	/// Provides cross-module operations without strongly typed interfaces.
	/// </summary>
	/// <param name="command">The command provided by the module.</param>
	/// <param name="args">The command arguments.</param>
	/// <returns>The command result.</returns>
	public virtual object Interop(string command, object? args)
	{
		throw new NotImplementedException();
	}
}
