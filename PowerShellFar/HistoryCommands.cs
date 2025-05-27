
using FarNet;
using FarNet.Tools;

namespace PowerShellFar;

class HistoryCommands : HistoryStore
{
	static readonly List<HistoryInfo> _sessionLines = [];

	public static void AddSessionLine(string line)
	{
		_sessionLines.RemoveAll(x => x.Name == line);
		_sessionLines.Add(new(line, DateTime.Now, false));
	}

	public static bool HasPrefix(string line)
	{
		return
			line.StartsWith(Entry.Prefix1, StringComparison.OrdinalIgnoreCase) ||
			line.StartsWith(Entry.Prefix2, StringComparison.OrdinalIgnoreCase);
	}

	public static string RemovePrefix(string line)
	{
		FarNet.Works.Kit.SplitCommandWithPrefix(line, out var prefix, out var command, Entry.IsMyPrefix);
		return prefix.Length == 0 ? line : command.ToString();
	}

	public override string[] ReadLines()
	{
		// get dialog history
		var res = new List<HistoryInfo>(Far.Api.History.Dialog(Res.History));

		// append filtered command history
		foreach (var info in Far.Api.History.Command())
		{
			if (HasPrefix(info.Name))
				res.Add(info);
		}

		// session
		res.AddRange(_sessionLines);

		// sort and trim
		return [.. res
			.OrderBy(x => x.Time)
			.TakeLast(Settings.Default.MaximumHistoryCount)
			.Select(x => x.Name)];
	}
}
