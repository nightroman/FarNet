
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>
/// The module host. At most one public descendant can be implemented by a module.
/// </summary>
/// <remarks>
/// In many cases the module actions should be implemented instead of the host
/// (see predefined descendants of <see cref="ModuleAction"/>).
/// <para>
/// If the attribute <see cref="ModuleHostAttribute.Load"/> is true then the host is always loaded.
/// If it is false then the host is loaded only on the first call of any action.
/// A single instance of this class is created for the whole session.
/// </para>
/// <para>
/// This class provides virtual methods called by the core.
/// Normally the module implements the <see cref="Connect"/> method.
/// There are a few more optional virtual members that can be implemented when needed.
/// </para>
/// </remarks>
public abstract class ModuleHost : BaseModuleItem
{
	/// <summary>
	/// Override this method to process the module connection.
	/// </summary>
	/// <remarks>
	/// This method is called once. For standard hosts it is called before
	/// creation of the first called module action. For preloadable hosts
	/// it is called immediately after loading of the module assembly and
	/// registration of its actions.
	/// </remarks>
	public virtual void Connect()
	{ }
	/// <summary>
	/// Override this method to process the module disconnection.
	/// </summary>
	/// <remarks>
	/// NOTE: Don't call Far UI, it is not working on exiting.
	/// Consider to use GUI message boxes if it is absolutely needed.
	/// <para>
	/// The host does not have to unregister dynamically registered actions
	/// on disconnection. But added "global" event handlers have to be
	/// removed, for example, handlers added to <see cref="IFar.AnyEditor"/>.
	/// </para>
	/// </remarks>
	public virtual void Disconnect()
	{ }
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
	{ }
	/// <summary>
	/// Can the module exit now?
	/// </summary>
	/// <remarks>
	/// This method is normally called internally by the <see cref="IFar.Quit"/>.
	/// The module can override this to perform preliminary checks before exit.
	/// Note that final exit actions should be performed in <see cref="Disconnect"/>.
	/// <para>
	/// It is allowed to return false but this option should be used sparingly,
	/// there must be really good reasons to disturb normal exiting process.
	/// The most important reason is that a user really wants that.
	/// </para>
	/// </remarks>
	/// <returns>True if the module is ready to exit.</returns>
	public virtual bool CanExit()
	{
		return true;
	}
	/// <summary>
	/// Provides cross-module operations without strongly typed interfaces.
	/// </summary>
	/// <param name="command">The command provided by the module.</param>
	/// <param name="args">The command arguments.</param>
	/// <returns>The command result.</returns>
	public virtual object Interop(string command, object args)
	{
		throw new NotImplementedException();
	}
}
