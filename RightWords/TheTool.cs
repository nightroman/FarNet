
// FarNet module RightWords
// Copyright (c) Roman Kuzmin

namespace FarNet.RightWords;

[ModuleTool(Name = Settings.ModuleName, Options = ModuleToolOptions.Dialog | ModuleToolOptions.Editor | ModuleToolOptions.Panels)]
[System.Runtime.InteropServices.Guid("ca7ecdc0-f446-4bff-a99d-06c90fe0a3a9")]
public class TheTool : ModuleTool
{
	public override void Invoke(object sender, ModuleToolEventArgs e)
	{
		var menu = Far.Api.CreateMenu();
		menu.Title = Settings.ModuleName;
		menu.HelpTopic = GetHelpTopic(HelpTopic.MainMenu);

		menu.Add(Text.DoCorrectWord).Click = delegate { Actor.CorrectWord(); };

		if (e.From == ModuleToolOptions.Editor)
			menu.Add(Text.DoCorrectText).Click = delegate { Actor.CorrectText(); };

		menu.Show();
	}
}
