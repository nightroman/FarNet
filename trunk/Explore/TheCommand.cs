
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
			// open the empty panel
			var command = e.Command.Trim();
			if (command.Length == 0)
			{
				(new SuperExplorer()).OpenPanel();
				return;
			}

			// the module panel
			Panel panel = Far.Net.Panel as Panel;
			if (panel == null)
			{
				Far.Net.Message("This is not a module panel.");
				return;
			}

			// the search
			var search = new SearchFileCommand(panel.Explorer);

			// parse command, setup the search
			var tokens = command.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			bool async = false;
			for (int iToken = 0; iToken < tokens.Length; ++iToken)
			{
				var token = tokens[iToken];
				if (token.Equals("-Depth", StringComparison.OrdinalIgnoreCase))
				{
					if (++iToken >= token.Length) throw new InvalidOperationException("Invalid depth.");
					search.Depth = int.Parse(tokens[iToken]);
				}
				else if (token.Equals("-Directory", StringComparison.OrdinalIgnoreCase))
				{
					search.Directory = true;
				}
				else if (token.Equals("-Recurse", StringComparison.OrdinalIgnoreCase))
				{
					search.Recurse = true;
				}
				else if (token.Equals("-Asynchronous", StringComparison.OrdinalIgnoreCase))
				{
					async = true;
				}
				else if (search.Filter != null)
				{
					throw new InvalidOperationException("Invalid command line.");
				}
				else
				{
					var pattern = token;
					search.Filter = delegate(Explorer explorer, FarFile file)
					{
						return Far.Net.MatchPattern(file.Name, pattern);
					};
				}
			}

			// go
			if (async)
				search.InvokeAsync(panel);
			else
				search.Invoke(panel);
		}
	}
}
