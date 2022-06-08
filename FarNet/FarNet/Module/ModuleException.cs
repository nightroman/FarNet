
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Runtime.Serialization;

namespace FarNet;

/// <summary>
/// Module exception.
/// </summary>
/// <remarks>
/// This exception is supposed to be caught by the core and shown as an error.
/// It is used to distinguish processed module errors from other exceptions.
/// <para>
/// Scenario. Catch some exception, wrap by a module exception with more
/// specific explanation of the problem in the context, throw the new.
/// Attach the original inner exception if it is useful for analysis.
/// </para>
/// </remarks>
[Serializable]
public class ModuleException : Exception
{
	///
	public ModuleException()
	{ }
	/// <inheritdoc/>
	public ModuleException(string message) : base(message)
	{ }
	/// <inheritdoc/>
	public ModuleException(string message, Exception innerException) : base(message, innerException)
	{ }
	/// <inheritdoc/>
	protected ModuleException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	/// <summary>
	/// This text is used as the error dialog title.
	/// By default it is the exception type name.
	/// </summary>
	public override string Source
	{
		get => base.Source ?? GetType().FullName;
		set => base.Source = value;
	}
}
