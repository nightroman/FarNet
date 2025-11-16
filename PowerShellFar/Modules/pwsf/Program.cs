using System.Diagnostics;
using System.Text;

const string FarExe = "Far.exe";

// executable
var baseDirectory = AppContext.BaseDirectory;
var fileName = $"{baseDirectory}\\{FarExe}";

// arguments

string pwd;
int st;
if (args.Length > 0 && !args[0].StartsWith("-") && !args[0].StartsWith("/"))
{
	pwd = args[0].TrimEnd('/').TrimEnd('\\');
	if (pwd.EndsWith(":"))
		pwd += "\\\\";
	st = 1;
}
else
{
	pwd = Environment.CurrentDirectory;
	st = 0;
}

var sb = new StringBuilder($"\"{pwd}\" \"{pwd}\" -set:Panel.Left.Visible=false -set:Panel.Right.Visible=false");
for (int i = st; i < args.Length; i++)
{
	var arg = args[i];
	if (arg.StartsWith("-") || arg.StartsWith("/"))
	{
		var str = arg.TrimStart('-', '/');

		if (str.Equals("ro", StringComparison.OrdinalIgnoreCase) ||
			str.Equals("ro-", StringComparison.OrdinalIgnoreCase))
		{
			sb.Append($" {arg}");
			continue;
		}

		if (str.StartsWith("set:", StringComparison.OrdinalIgnoreCase))
		{
			sb.Append($" \"{arg}\"");
			continue;
		}
	}

	Console.WriteLine("""
		Usage: pwsf [<path>] [<arguments>]
		Arguments:
		-ro[-]
		-set:<parameter>=<value>
		""");

	return 1;
}

var arguments = sb.ToString();
Debug.WriteLine($"## pwsf: args: {string.Join(", ", args)}");
Debug.WriteLine($"## pwsf: arguments: {arguments}");

// trigger
Environment.SetEnvironmentVariable("FAR_START_COMMAND", "ps:$Psf.StartCommandConsole()");

var process = Process.Start(new ProcessStartInfo
{
	UseShellExecute = false,
	FileName = fileName,
	Arguments = arguments,
});

process!.WaitForExit();
return process.ExitCode;
