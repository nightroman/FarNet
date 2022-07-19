
// FarNet module RightWords
// Copyright (c) Roman Kuzmin

using FarNet.Forms;

namespace FarNet.RightWords;

class UIWordDialog
{
	readonly IDialog _Dialog;
	readonly IEdit _Stem1;
	readonly IEdit _Stem2;
	public UIWordDialog(string stem1, string stem2)
	{
		const int h = 6;
		const int x = 20;

		_Dialog = Far.Api.CreateDialog(-1, -1, 77, h);
		_Dialog.HelpTopic = TheHost.Instance.GetHelpTopic(HelpTopic.AddToDictionary);

		_Dialog.AddBox(3, 1, 0, 0, Text.AddToDictionary);
		int y = 1;

		_Dialog.AddText(5, ++y, 0, Text.NewWord);
		_Stem1 = _Dialog.AddEdit(x, y, 71, stem1);

		_Dialog.AddText(5, ++y, 0, Text.SampleStem);
		_Stem2 = _Dialog.AddEdit(x, y, 71, stem2);
		_Stem2.History = "RightWordsStem";
	}
	public bool Show()
	{
		return _Dialog.Show();
	}
	public string Stem1 { get { return _Stem1.Text; } }
	public string Stem2 { get { return _Stem2.Text; } set { _Stem2.Text = value; } }
}
