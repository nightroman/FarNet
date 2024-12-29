using FarNet;
using FarNet.Tools;

namespace Explore;

[ModuleCommand(Name = "Search in module panels", Prefix = "explore", Id = "20b46a91-7ef4-4daa-97f5-a1ef291f7391")]
public class Command : ModuleCommand
{
	public override void Invoke(object sender, ModuleCommandEventArgs e)
	{
		// tokenize
		var tokens = Parser.Tokenize(e.Command, "-XPath");

		// open the file system panel
		if (tokens.Count == 0)
		{
			new FileSystemExplorer().CreatePanel().Open();
			return;
		}

		// get the module panel and explorer or defaults
		var panel = Far.Api.Panel as Panel;
		var explorer =  panel is null ? new FileSystemExplorer() : panel.Explorer;

		// the search
		var search = new SearchFileCommand(explorer);

		// parameters
		string[] parameters = [
			"-Async",
			"-Bfs",
			"-Depth",
			"-Directory",
			"-Exclude",
			"-File",
			"-XFile",
			"-XPath",
		];

		// parse, setup the search
		bool async = false;
		for (int iToken = 0; iToken < tokens.Count; ++iToken)
		{
			var token = tokens[iToken];
			var parameter = Parser.ResolveName(token, parameters);

			// mask
			if (parameter is null)
			{
				if (token[0] == '-' || search.Filter is not null)
					throw new ModuleException($"Invalid command token '{token}'. Valid parameters: {string.Join(", ", parameters)}.");

				var mask = token;
				if (!Far.Api.IsMaskValid(mask))
					throw new ModuleException("Invalid mask.");

				search.Filter = (explorer, file) => Far.Api.IsMaskMatch(file.Name, mask);
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
				case "-Exclude":
					{
						if (++iToken >= token.Length)
							throw new ModuleException("Invalid -Exlude.");

						var mask = tokens[iToken];
						if (!Far.Api.IsMaskValid(mask))
							throw new ModuleException("Invalid mask.");

						search.Exclude = (explorer, file) => Far.Api.IsMaskMatch(file.Name, mask);
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
				case "-Bfs":
					{
						search.Bfs = true;
						break;
					}
				case "-Async":
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
