namespace FarNet;

/// <summary>
/// Disposable with known disposed state.
/// </summary>
public abstract class Disposable : IDisposable
{
	/// <summary>
	/// Gets true if disposed.
	/// </summary>
	public bool IsDisposed { get; protected set; }

	/// <summary>
	/// Disposes this.
	/// </summary>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Protected disposing.
	/// </summary>
	protected abstract void Dispose(bool disposing);
}
