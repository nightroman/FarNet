
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// A module drawer action.
/// </summary>
/// <remarks>
/// This action is called on editor drawing in order to get colors for the specified lines.
/// <para>
/// The <see cref="Invoke"/> method has to be implemented.
/// It should work as fast as possible because it is called frequently.
/// Its goal is to fill the color collection, it should not change anything.
/// </para>
/// <para>
/// Use <see cref="ModuleDrawerAttribute"/> and specify the <see cref="ModuleActionAttribute.Name"/>.
/// The optional default file mask is defined as <see cref="ModuleDrawerAttribute.Mask"/>
/// and the default color priority <see cref="ModuleDrawerAttribute.Priority"/>.
/// </para>
/// <include file='doc.xml' path='doc/ActionGuid/*'/>
/// </remarks>
public abstract class ModuleDrawer : ModuleAction
{
	/// <summary>
	/// Gets colors for the specified editor lines.
	/// </summary>
	/// <param name="editor">The editor.</param>
	/// <param name="e">The arguments.</param>
	public abstract void Invoke(IEditor editor, ModuleDrawerEventArgs e);
}
