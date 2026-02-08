namespace FarNet;

/// <summary>
/// Module editor runtime representation.
/// </summary>
/// <remarks>
/// It represents an auto registered <see cref="ModuleEditor"/> actions.
/// </remarks>
public interface IModuleEditor : IModuleAction
{
	/// <summary>
	/// Processes the editor event.
	/// </summary>
	/// <param name="editor">The editor.</param>
	/// <param name="e">The arguments.</param>
	void Invoke(IEditor editor, ModuleEditorEventArgs e);

	/// <summary>
	/// Gets the file mask. Setting is for internal use.
	/// </summary>
	string Mask { get; set; }
}
