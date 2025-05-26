using FarNet.Works;
using System.Collections.Concurrent;

namespace FarNet;

/// <summary>
/// User data for scripts and modules.
/// </summary>
/// <remarks>
/// This class provides the global dictionary <see cref="Data"/> and its helper
/// methods, e.g. PowerShell friendly. Use it for caching, states, and sharing
/// data between threads and components.
/// </remarks>
public static class User
{
	/// <summary>
	/// Gets the global concurrent dictionary.
	/// </summary>
	static public ConcurrentDictionary<string, object> Data { get; } = [];

	/// <summary>
	/// Gets existing or adds a new value to <see cref="Data"/> using the specified function.
	/// </summary>
	/// <param name="key">The key.</param>
	/// <param name="valueFactory">The value function.</param>
	/// <returns>The added or existing value.</returns>
	//! ConcurrentDictionary has no PS friendly GetOrAdd.
	public static object GetOrAdd(string key, Func<string, object> valueFactory) => Data.GetOrAdd(key, valueFactory);

	/// <summary>
	/// Removes the specified key from <see cref="Data"/>.
	/// </summary>
	/// <param name="key">The key.</param>
	//! ConcurrentDictionary has no PS friendly Remove.
	public static void Remove(string key) => Data.TryRemove(key, out _);

	/// <summary>
	/// Gets and removes the specified object from <see cref="Data"/>.
	/// </summary>
	/// <param name="key">The key.</param>
	/// <returns>The removed object or null.</returns>
	public static object? Pop(string key) => Data.TryRemove(key, out object? value) ? value : null;

	/// <summary>
	/// Registers the handler of events triggered by <see cref="IFar.Quit"/>.
	/// </summary>
	/// <param name="handler">
	/// The quitting handler. It is invoked in the main thread.
	/// It may set <see cref="QuittingEventArgs.Ignore"/> to cancel quitting.
	/// If the input value is true then quitting is already cancelled by others.
	/// </param>
	/// <returns>Disposable to unregister the handler when needed.</returns>
	public static Disposable RegisterQuitting(EventHandler<QuittingEventArgs> handler)
	{
		return new DisposableEventHandler<QuittingEventArgs>(
			handler,
			handler => Far2.Api.Quitting += handler,
			handler => Far2.Api.Quitting -= handler);
	}
}
