using FarNet;
using FarNet.Tools;

namespace PowerShellFar.UI;

sealed class InputBox2 : InputBox
{
	public InputBox2(string? prompt = null, string? title = null) : base(prompt, title)
	{
		Dialog.TypeId = new System.Guid(Guids.InputDialog);

		// hotkeys
		Edit.KeyPressed += (sender, e) =>
		{
			switch (e.Key.VirtualKeyCode)
			{
				case KeyCode.Tab:
					e.Ignore = true;
					EditorKit.ExpandCode(Edit.Line, null);
					break;
				case KeyCode.F1:
					e.Ignore = true;
					Help.ShowHelpForContext(HelpTopic.InvokeCommands);
					break;
			}
		};
	}
}
