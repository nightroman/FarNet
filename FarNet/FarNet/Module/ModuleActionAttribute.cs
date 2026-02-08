namespace FarNet;

/// <summary>
/// Any action attribute parameters.
/// </summary>
public abstract class ModuleActionAttribute : Attribute, ICloneable
{
	/// <summary>
	/// The mandatory module action GUID.
	/// </summary>
	public string Id { get; set; } = null!;

	/// <summary>
	/// The mandatory user interface name.
	/// </summary>
	public string Name { get; set; } = null!;

	/// <summary>
	/// Tells to use <see cref="Name"/> as the resource string name.
	/// </summary>
	/// <remarks>
	/// Restart after changing the current language or module culture
	/// in order to update module action names from resources.
	/// </remarks>
	public bool Resources { get; set; }

	/// <summary>
	/// Calls <see cref="object.MemberwiseClone"/>.
	/// </summary>
	public object Clone() => MemberwiseClone();
}
