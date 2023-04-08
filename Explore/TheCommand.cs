
// FarNet module Explore
// Copyright (c) Roman Kuzmin

using FarNet.Tools;

namespace FarNet.Explore;

[ModuleCommand(Name = "Search in explorer panels", Prefix = "Explore", Id = "20b46a91-7ef4-4daa-97f5-a1ef291f7391")]
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
		Panel panel = Far.Api.Panel as Panel;
		if (panel == null)
		{
			Far.Api.Message("This is not a module panel.");
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
			"-File",
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
				if (!Far.Api.IsMaskValid(mask))
					throw new ModuleException("Invalid mask.");

				search.Filter = delegate(Explorer explorer, FarFile file)
				{
					return Far.Api.IsMaskMatch(file.Name, mask);
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
						if (++iToken >= token.Length)
							throw new ModuleException("Invalid -XFile.");

						search.XFile = tokens[iToken];
						break;
					}
				case "-Depth":
					{
						if (++iToken >= token.Length)
							throw new ModuleException("Invalid -Depth.");

						search.Depth = int.Parse(tokens[iToken]);
						break;
					}
				case "-Directory":
					{
						search.Directory = true;
						break;
					}
				case "-File":
					{
						search.File = true;
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

		if (search.Directory && search.File)
			throw new ModuleException("-Directory and -File cannot be used together.");

		// go
		if (async)
			search.InvokeAsync(panel);
		else
			search.Invoke(panel);
	}
}
