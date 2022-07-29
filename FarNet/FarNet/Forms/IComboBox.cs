
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet.Forms;

/// <summary>
/// Combo box control.
/// It is created and added to a dialog by <see cref="IDialog.AddComboBox"/>.
/// </summary>
public interface IComboBox : IBaseList, IEditable, IDropDown
{
	/// <summary>
	/// Tells to be a non editable drop down list.
	/// </summary>
	bool DropDownList { get; set; }
}
