namespace FarNet;

/// <summary>
/// Quitting arguments.
/// </summary>
public sealed class QuittingEventArgs : EventArgs
{
	/// <summary>
	/// Ignore event.
	/// </summary>
	public bool Ignore { get => _Ignore; set { if (value) _Ignore = true; } }
	bool _Ignore;
}
