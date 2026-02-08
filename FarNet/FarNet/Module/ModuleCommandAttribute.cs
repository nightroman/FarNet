namespace FarNet;

/// <summary>
/// Module command attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ModuleCommandAttribute : ModuleActionAttribute
{
	/// <summary>
	/// The mandatory not empty command prefix.
	/// </summary>
	/// <remarks>
	/// This is the default prefix, the actual prefix may be configured by a user.
	/// </remarks>
	public string Prefix { get; set; } = null!;
}
