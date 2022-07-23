
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Threading.Tasks;

namespace FarNet;

/// <summary>
/// Any editor operator, common editor events, options and tools.
/// </summary>
/// <remarks>
/// It is exposed as <see cref="IFar.AnyEditor"/>.
/// <para>
/// It is used to subscribe to events of editors that are not yet opened.
/// It also exposes common editor tools.
/// </para>
/// </remarks>
public abstract class IAnyEditor : IEditorBase
{
	/// <summary>
	/// Called once on opening the first editor.
	/// </summary>
	public abstract event EventHandler FirstOpening;

	/// <summary>
	/// Opens a modal editor in order to edit the text.
	/// </summary>
	/// <param name="args">Arguments.</param>
	/// <returns>The result text.</returns>
	public abstract string EditText(EditTextArgs args);

	/// <summary>
	/// Opens a non-modal editor in order to edit the text.
	/// </summary>
	/// <param name="args">Arguments.</param>
	/// <returns>The result text task.</returns>
	public abstract Task<string> EditTextAsync(EditTextArgs args);
}
