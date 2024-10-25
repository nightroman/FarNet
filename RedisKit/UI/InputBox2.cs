
using FarNet;
using FarNet.Forms;
using System;

namespace RedisKit.UI;

class InputBox2
{
	public const string TypeId = "7ddc97f7-9806-4bf4-bdf2-79742ada47c2";

	public string? Title { get; set; }
	public string? Text1 { get; set; }
	public string? Text2 { get; set; }
	public string? Prompt1 { get; set; }
	public string? Prompt2 { get; set; }
	public string? History1 { get; set; }
	public string? History2 { get; set; }

	public bool Show()
	{
		int w = Far.Api.UI.WindowSize.X - 7;
		int h = 8;

		// dialog
		var dialog = Far.Api.CreateDialog(-1, -1, w, h);
		dialog.TypeId = new Guid(TypeId);
		dialog.AddBox(3, 1, w - 4, h - 2, Title);

		// field 1
		dialog.AddText(5, -1, w - 6, Prompt1);
		IEdit edit1 = dialog.AddEdit(5, -1, w - 6, Text1);
		edit1.History = History1 ?? string.Empty;

		// field 2
		dialog.AddText(5, -1, w - 6, Prompt2);
		IEdit edit2 = dialog.AddEdit(5, -1, w - 6, Text2);
		edit2.History = History2 ?? string.Empty;

		if (!dialog.Show())
			return false;

		Text1 = edit1.Text;
		Text2 = edit2.Text;

		return true;
	}
}
