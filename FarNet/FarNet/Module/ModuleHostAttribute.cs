namespace FarNet;

/// <summary>
/// Module host attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ModuleHostAttribute : Attribute
{
	/// <summary>
	/// Tells to load and connect the host.
	/// </summary>
	/// <remarks>
	/// If the module host is the only implemented module item then this flag
	/// should to be set to true. Otherwise the module is not loaded.
	/// </remarks>
	public bool Load { get; set; }
}
