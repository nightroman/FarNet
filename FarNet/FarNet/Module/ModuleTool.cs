
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// A module tool represented by an item in Far menus.
/// </summary>
/// <remarks>
/// The <see cref="Invoke"/> method has to be implemented.
/// <para>
/// Use <see cref="ModuleToolAttribute"/> and specify the <see cref="ModuleActionAttribute.Name"/>
/// and the menu areas <see cref="ModuleToolAttribute.Options"/>.
/// </para>
/// <include file='doc.xml' path='doc/ActionGuid/*'/>
/// </remarks>
public abstract class ModuleTool : ModuleAction
{
	/// <summary>
	/// This method is called when the tool menu item is invoked.
	/// </summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The arguments.</param>
	public abstract void Invoke(object sender, ModuleToolEventArgs e);
}
