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
	/// Opens modal editor with the text and gets the result text.
	/// For non-modal editor use <see cref="EditTextAsync"/>.
	/// </summary>
	/// <param name="args">Arguments.</param>
	/// <returns>The result text.</returns>
	public abstract string EditText(EditTextArgs args);

	/// <summary>
	/// Opens non-modal editor with the text and gets the result text task.
	/// </summary>
	/// <param name="args">Arguments.</param>
	/// <returns>The result text task.</returns>
	public abstract Task<string> EditTextAsync(EditTextArgs args);
}
