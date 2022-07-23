
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>
/// Common explorer method arguments.
/// </summary>
public abstract class ExplorerEventArgs : EventArgs
{
	/// <param name="mode">See <see cref="Mode"/></param>
	protected ExplorerEventArgs(ExplorerModes mode)
	{
		Mode = mode;
	}

	/// <summary>
	/// Gets the explorer mode.
	/// </summary>
	public ExplorerModes Mode { get; }

	/// <summary>
	/// Gets or sets the parameter to be used by the explorer.
	/// </summary>
	public object Parameter { get; set; }

	/// <summary>
	/// Gets or sets the job result.
	/// </summary>
	public JobResult Result { get; set; }

	/// <summary>
	/// Gets or sets any co-explorer data (not used by the core).
	/// </summary>
	/// <remarks>
	/// Use case. Some complex operation has to be performed by several calls to one or more co-explorers.
	/// On the first call an explorer may ask a user for extra options and keep this information in here.
	/// On the next calls this or another co-explorer uses this information.
	/// </remarks>
	public object Data { get; set; }

	/// <summary>
	/// To be set current.
	/// </summary>
	public object PostData { get; set; }

	/// <summary>
	/// To be set current.
	/// </summary>
	public FarFile PostFile { get; set; }

	/// <summary>
	/// To be set current.
	/// </summary>
	public string PostName { get; set; }

	/// <summary>
	/// Tells whether user interaction is allowed.
	/// </summary>
	public bool UI { get { return 0 == (Mode & (ExplorerModes.Find | ExplorerModes.Silent)); } }

	/// <summary>
	/// Casts the not null <see cref="Parameter"/> to the specified type or creates and returns a new instance.
	/// </summary>
	/// <typeparam name="T">The type of the parameter to be created or converted to.</typeparam>
	/// <exception cref="InvalidCastException">The parameter cannot be converted to the specified type.</exception>
	public T ParameterOrDefault<T>() where T : class, new()
	{
		return Parameter == null ? new T() : (T)Parameter;
	}
}
