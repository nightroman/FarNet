
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// A module editor action.
/// </summary>
/// <remarks>
/// This action deals with opening an editor, not with editor menu commands.
/// For menu commands use <see cref="ModuleTool"/> configured for editors.
/// <para>
/// The <see cref="Invoke"/> method has to be implemented.
/// </para>
/// <para>
/// Use <see cref="ModuleEditorAttribute"/> and specify <see cref="ModuleActionAttribute.Id"/> and <see cref="ModuleActionAttribute.Name"/>.
/// The optional default file mask is defined as <see cref="ModuleEditorAttribute.Mask"/>.
/// </para>
/// </remarks>
public abstract class ModuleEditor : ModuleAction
{
	/// <summary>
	/// Editor <see cref="IEditorBase.Opened"/> handler.
	/// </summary>
	/// <param name="editor">The editor.</param>
	/// <param name="e">The arguments.</param>
	/// <remarks>
	/// This method is called on opening an editor.
	/// Normally it adds editor event handlers, then they do the jobs.
	/// </remarks>
	/// <example>
	/// See the <c>EditorKit</c> module.
	/// </example>
	public abstract void Invoke(IEditor editor, ModuleEditorEventArgs e);
}
