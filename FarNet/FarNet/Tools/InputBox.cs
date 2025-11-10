using FarNet.Forms;
using FarNet.Works;

namespace FarNet.Tools;

/// <summary>
/// Alternative input box.
/// </summary>
public class InputBox
{
	/// <summary>
	/// Default dialog type id.
	/// </summary>
	public const string DefaultTypeId = "3765bf71-3b99-4df6-8d85-5a6a57253091";

	/// <summary>
	/// Gets the dialog instance.
	/// </summary>
	public IDialog Dialog { get; }

	/// <summary>
	/// Gets the edit control.
	/// </summary>
	public IEdit Edit { get; }

	/// <summary>
	/// Creates the dialog.
	/// </summary>
	/// <param name="prompt">Optional input prompt. Multiline is supported.</param>
	/// <param name="title">Optional dialog title.</param>
	public InputBox(string? prompt = null, string? title = null)
	{
		var promptLines = prompt is null ? [] : Kit.SplitLines(prompt);
		int h = 5 + promptLines.Length;

		Dialog = Far.Api.CreateDialog(-1, -1, -1, h);
		int w = Dialog.Rect.Width;

		Dialog.TypeId = new Guid(DefaultTypeId);
		Dialog.AddBox(3, 1, w - 4, h - 2, title);

		foreach (var s in promptLines)
			Dialog.AddText(5, -1, w - 6, s);

		Edit = Dialog.AddEdit(5, -1, w - 6, string.Empty);
		Edit.IsPath = true;
	}

	/// <summary>
	/// Shows the dialog and returns the result text or null.
	/// </summary>
	public string? Show()
	{
		if (Dialog.Show())
			return Edit.Text;
		else
			return null;
	}

	/// <summary>
	/// Shows the dialog and returns the result text or null.
	/// It may be called from any thread.
	/// </summary>
	public Task<string?> ShowAsync()
	{
		return Tasks.Dialog(Dialog, (e) =>
		{
			if (e.Control is null)
				return null;
			else
				return Edit.Text;
		});
	}
}
