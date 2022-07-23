
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Job results.
/// </summary>
public enum JobResult
{
	/// <summary>
	/// A job is done.
	/// </summary>
	Done,

	/// <summary>
	/// A job is not done and the core should not use default methods.
	/// Example: a method is supposed to process only special files.
	/// It is implemented and called but it ignores some files.
	/// </summary>
	Ignore,

	/// <summary>
	/// A job is not done and the core should do the job as if a method is not implemented.
	/// Example: a method is implemented but it is only a wrapper of an actual worker.
	/// If a worker is optional and not specified then the core should work itself.
	/// </summary>
	Default,

	/// <summary>
	/// A job is done but not completely. The core should try to recover if possible.
	/// </summary>
	Incomplete,
}
