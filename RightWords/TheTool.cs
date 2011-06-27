
/*
FarNet module RightWords
Copyright (c) 2011 Roman Kuzmin
*/

namespace FarNet.RightWords
{
	[System.Runtime.InteropServices.Guid("ca7ecdc0-f446-4bff-a99d-06c90fe0a3a9")]
	[ModuleTool(Name = Settings.Name, Options = ModuleToolOptions.Dialog | ModuleToolOptions.Editor | ModuleToolOptions.Panels)]
	public class TheTool : ModuleTool
	{
		public override void Invoke(object sender, ModuleToolEventArgs e)
		{
			if (e == null) return;

			var menu = Far.Net.CreateMenu();
			menu.Title = Settings.Name;

			menu.Add("&1. Correct word").Click += delegate { Actor.CorrectWord(); };

			if (e.From == ModuleToolOptions.Editor)
			{
				var editor = Far.Net.Editor;

				menu.Add("&2. Correct text").Click += delegate { Actor.CorrectText(); };
				
				var itemHighlighting = menu.Add("&3. Highlighting");
				itemHighlighting.Click += delegate { Actor.Highlight(editor); };
				if (editor.Data[Settings.EditorDataId] != null)
					itemHighlighting.Checked = true;
			}

			menu.Add("&0. Thesaurus...").Click += delegate { Actor.ShowThesaurus(); };

			menu.Show();
		}
	}
}
