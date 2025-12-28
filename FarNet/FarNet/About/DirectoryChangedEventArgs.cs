namespace FarNet;

/// <summary>
/// <see cref="IFar.DirectoryChanged"/> arguments.
/// </summary>
/// <param name="path">.</param>
public class DirectoryChangedEventArgs(string path) : EventArgs
{
	/// <summary>
	/// The new directory path.
	/// </summary>
	public string Path { get; } = path;
}
