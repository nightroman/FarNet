
// FarNet module RightWords
// Copyright (c) Roman Kuzmin

namespace FarNet.RightWords
{
	[System.Runtime.InteropServices.Guid(My.GuidString)]
	[ModuleTool(Name = Settings.ModuleName, Options = ModuleToolOptions.Dialog | ModuleToolOptions.Editor | ModuleToolOptions.Panels)]
	public class TheTool : ModuleTool
	{
		public override void Invoke(object sender, ModuleToolEventArgs e)
		{
			var menu = Far.Api.CreateMenu();
			menu.Title = Settings.ModuleName;
			menu.HelpTopic = Far.Api.GetHelpTopic("main-menu");

			menu.Add(My.DoCorrectWord).Click = delegate { Actor.CorrectWord(); };

			if (e.From == ModuleToolOptions.Editor)
				menu.Add(My.DoCorrectText).Click = delegate { Actor.CorrectText(); };

			menu.Add(My.DoThesaurus).Click = delegate { Actor.ShowThesaurus(); };

			menu.Show();
		}
	}
}
