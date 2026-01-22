namespace FarNet;

/// <summary>
/// Obsolete, use <see cref="ModuleHost.ToLoad"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
[Obsolete("Use ModuleHost.ToLoad")]
public sealed class ModuleHostAttribute : Attribute
{
	/// <summary>
	/// Obsolete, use <see cref="ModuleHost.ToLoad"/>.
	/// </summary>
	public bool Load { get; set; }
}
