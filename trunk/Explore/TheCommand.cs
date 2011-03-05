
/*
FarNet module Explore
Copyright (c) 2010 Roman Kuzmin
*/

using System;
using FarNet.Tools;

namespace FarNet.Explore
{
	[System.Runtime.InteropServices.Guid("20b46a91-7ef4-4daa-97f5-a1ef291f7391")]
	[ModuleCommand(Name = "Search in explorer panels", Prefix = "Explore")]
	public class TheCommand : ModuleCommand
	{
		public override void Invoke(object sender, ModuleCommandEventArgs e)
		{
			Panel panel = Far.Net.Panel as Panel;
			if (panel == null)
			{
				Far.Net.Message("This is not a module panel.");
				return;
			}

			// the search
			var search = new FileSearchExplorer(panel.Explorer);

			// parse command, setup the search
			var command = e.Command.Trim();
			var tokens = command.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (var token in tokens)
			{
				if (token.Equals("-Directory", StringComparison.OrdinalIgnoreCase))
				{
					search.Directory = true;
				}
				else if (token.Equals("-Recurse", StringComparison.OrdinalIgnoreCase))
				{
					search.Recurse = true;
				}
				else if (search.Process != null)
				{
					throw new InvalidOperationException("Invalid command line.");
				}
				else
				{
					var pattern = token;
					search.Process = delegate(Explorer explorer, FarFile file)
					{
						return Far.Net.MatchPattern(file.Name, pattern);
					};
				}
			}

			// go
			search.Invoke();
			if (search.ResultFiles.Count == 0)
				return;

			// panel
			search.OpenPanelChild(panel);
		}
	}
}
