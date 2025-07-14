
namespace FarNet;

/// <summary>
/// Disposable object with <see cref="IsDisposed"/>.
/// </summary>
public abstract class Disposable : IDisposable
{
	bool _IsDisposed;

	/// <summary>
	/// Gets the disposed state.
	/// </summary>
	public bool IsDisposed => IsDisposed;

	/// <summary>
	/// Disposes this object.
	/// </summary>
	public void Dispose()
	{
		if (!_IsDisposed)
		{
			Disposing();
			_IsDisposed = true;
			GC.SuppressFinalize(this);
		}
	}

	/// <summary>
	/// Implements disposing.
	/// </summary>
	protected abstract void Disposing();
}
