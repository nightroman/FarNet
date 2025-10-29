using FarNet.Forms;

namespace FarNet;

/// <summary>
/// Common for <see cref="IDialog"/>, <see cref="IEditor"/>, <see cref="IViewer"/>, <see cref="IPanel"/>.
/// </summary>
public interface IFace
{
	/// <summary>
	/// Gets the identifier.
	/// </summary>
	public nint Id { get; }

	/// <summary>
	/// Gets the window kind.
	/// </summary>
	public WindowKind WindowKind { get; }
}
