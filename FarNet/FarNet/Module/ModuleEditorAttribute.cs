namespace FarNet;

/// <summary>
/// Module editor action attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ModuleEditorAttribute : ModuleActionAttribute
{
	/// <include file='doc.xml' path='doc/FileMask/*'/>
	public string Mask
	{
		get => _mask ?? string.Empty;
		set => _mask = value;
	}
	string? _mask;
}
