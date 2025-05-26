namespace FarNet;

/// <summary>
/// Disposable event handler.
/// </summary>
/// <typeparam name="TEventArgs">.</typeparam>
public sealed class DisposableEventHandler<TEventArgs> : Disposable where TEventArgs : EventArgs
{
	readonly EventHandler<TEventArgs> _handler;
	readonly Action<EventHandler<TEventArgs>> _remove;

	/// <summary>
	/// .
	/// </summary>
	/// <param name="handler">Event handler.</param>
	/// <param name="add">Use this: <c>handler => MyEvent += handler</c></param>
	/// <param name="remove">Use this: <c>handler => MyEvent -= handler</c></param>
	public DisposableEventHandler(
		EventHandler<TEventArgs> handler,
		Action<EventHandler<TEventArgs>> add,
		Action<EventHandler<TEventArgs>> remove)
	{
		_handler = handler;
		_remove = remove;
		add(Invoke);
	}

	/// <inheritdoc/>
	protected override void Dispose(bool disposing)
	{
		if (!IsDisposed)
		{
			_remove(Invoke);
			IsDisposed = true;
		}
	}

	void Invoke(object? sender, TEventArgs e)
	{
		_handler(sender, e);
	}
}
