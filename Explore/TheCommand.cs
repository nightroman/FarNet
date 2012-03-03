
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
			// tokenize
			var tokens = Parser.Tokenize(e.Command, "-XPath");

			// open the empty panel
			if (tokens.Count == 0)
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

			// parameters
			var parameters = new string[]
			{
				"-Asynchronous",
				"-Depth",
				"-Directory",
				"-Recurse",
				"-XFile",
				"-XPath",
			};

			// parse, setup the search
			bool async = false;
			for (int iToken = 0; iToken < tokens.Count; ++iToken)
			{
				var token = tokens[iToken];
				var parameter = Parser.ResolveName(token, parameters);

				// mask
				if (parameter == null)
				{
					if (search.Filter != null)
						throw new ModuleException("Invalid command line.");

					var mask = token;
					if (!Far.Net.IsMaskValid(mask))
						throw new ModuleException("Invalid mask.");
					
					search.Filter = delegate(Explorer explorer, FarFile file)
					{
						return Far.Net.IsMaskMatch(file.Name, mask);
					};
					continue;
				}

				switch (parameter)
				{
					case "-XPath":
						{
							search.XPath = tokens[iToken + 1];
							if (search.XPath.Length == 0)
								throw new ModuleException("Invalid -XPath.");
							iToken = tokens.Count;
							break;
						}
					case "-XFile":
						{
							if (++iToken >= token.Length) throw new ModuleException("Invalid -XFile.");
							search.XFile = tokens[iToken];
							break;
						}
					case "-Depth":
						{
							if (++iToken >= token.Length) throw new ModuleException("Invalid -Depth.");
							search.Depth = int.Parse(tokens[iToken]);
							break;
						}
					case "-Directory":
						{
							search.Directory = true;
							break;
						}
					case "-Recurse":
						{
							search.Recurse = true;
							break;
						}
					case "-Asynchronous":
						{
							async = true;
							break;
						}
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
