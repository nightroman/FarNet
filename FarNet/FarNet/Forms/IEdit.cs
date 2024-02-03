
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet.Forms;

/// <summary>
/// Edit control.
/// It is created and added to a dialog by:
/// <see cref="IDialog.AddEdit"/>, <see cref="IDialog.AddEditFixed"/>, <see cref="IDialog.AddEditPassword"/>.
/// </summary>
public interface IEdit : IControl, IEditable, IDropDown
{
	/// <summary>
	/// Gets true if it is the fixed size edit control.
	/// </summary>
	bool Fixed { get; }

	/// <summary>
	/// Tells that it is used for file system path input.
	/// </summary>
	/// <remarks>
	/// Setting this to true enables some extras, e.g. on typing: a dropdown list of matching available paths.
	/// </remarks>
	bool IsPath { get; set; }

	/// <summary>
	/// Gets true if it is used for password input.
	/// </summary>
	/// <remarks>
	/// It is true if it is created by <see cref="IDialog.AddEditPassword"/>.
	/// </remarks>
	bool IsPassword { get; }

	/// <summary>
	/// Gets or sets the history name. It overrides <see cref="Mask"/> text if any.
	/// </summary>
	string History { get; set; }

	/// <summary>
	/// Gets or sets the mask for fixed size mode. It overrides <see cref="History"/> text if any.
	/// </summary>
	/// <remarks>
	/// Setting after opening is not supported.
	/// </remarks>
	string Mask { get; set; }

	/// <summary>
	/// Tells that this is a line of a multi-line group.
	/// </summary>
	/// <remarks>
	/// Sequential edit controls with this flag set are grouped into a simple editor with the ability to insert and delete lines.
	/// </remarks>
	bool Editor { get; set; }

	/// <summary>
	/// Tells to not add items to the history automatically.
	/// </summary>
	/// <remarks>
	/// Specifies that items will be added to the history list of an edit box manually, not automatically.
	/// It should be used together with <see cref="History"/>.
	/// </remarks>
	bool ManualAddHistory { get; set; }

	/// <include file='doc.xml' path='doc/UseLastHistory/*'/>
	bool UseLastHistory { get; set; }

	/// <include file='doc.xml' path='doc/NoFocus/*'/>
	bool NoFocus { get; set; }

	/// <summary>
	/// Tells to disable auto completion from history.
	/// </summary>
	bool NoAutoComplete { get; set; }
}
