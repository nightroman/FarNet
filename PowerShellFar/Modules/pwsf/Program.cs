using System.Diagnostics;
using System.Text;

const string FarExe = "Far.exe";

const string Usage = """
Usage: pwsf [<path>] [<arguments>]

Arguments:
    -ro[-]
    -set:<parameter>=<value>
    -s <profilepath> [<localprofilepath>]

    -Command | -c ...
    -File | -f <file> ...
    -EncodedCommand | -ec <utf16-base64>
    -NoExit | -noe
    -NoProfile | -nop
""";

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

static void Exit()
{
	Console.WriteLine(Usage);
	Environment.Exit(1);
}

var sb = new StringBuilder($"\"{pwd}\" \"{pwd}\" -set:Panel.Left.Visible=false -set:Panel.Right.Visible=false");
for (int i = st; i < args.Length; i++)
{
	var arg = args[i];
	if (!arg.StartsWith("-") && !arg.StartsWith("/"))
		Exit();

	var str = arg.TrimStart('-', '/');

	if (str == "ro" || str == "ro-")
	{
		sb.Append($" {arg}");
	}
	else if (str == "s")
	{
		sb.Append($" {arg}");
		if (++i >= args.Length)
			Exit();

		sb.Append($" {args[i]}");
		int j = i + 1;
		if (j < args.Length && args[j][0] != '-' && args[j][0] != '/')
		{
			++i;
			sb.Append($" {args[i]}");
		}
	}
	else if (str.StartsWith("set:"))
	{
		sb.Append($" \"{arg}\"");
	}
	else if (str == "NoExit" || str == "noe")
	{
		Environment.SetEnvironmentVariable("FAR_PWSF_NO_EXIT", "1");
	}
	else if (str == "NoProfile" || str == "nop")
	{
		Environment.SetEnvironmentVariable("FAR_PWSF_NO_PROFILE", "1");
	}
	else if (str == "EncodedCommand" || str == "ec")
	{
		if (++i >= args.Length)
			Exit();

		try
		{
			var bytes = Convert.FromBase64String(args[i]);
			var cmd = Encoding.Unicode.GetString(bytes);
			Environment.SetEnvironmentVariable("FAR_PWSF_COMMAND", cmd);
		}
		catch (FormatException ex)
		{
			Console.WriteLine($"EncodedCommand: {ex.Message}");
			Environment.Exit(1);
		}
	}
	else if (str == "Command" || str == "c")
	{
		if (++i >= args.Length)
			Exit();

		var text = string.Join(" ", args.Skip(i));
		Environment.SetEnvironmentVariable("FAR_PWSF_COMMAND", text);
		break;
	}
	else if (str == "File" || str == "f")
	{
		if (++i >= args.Length)
			Exit();

		var text = string.Join("\n", args.Skip(i));
		Environment.SetEnvironmentVariable("FAR_PWSF_FILE", text);
		break;
	}
	else
	{
		Exit();
	}
}

var arguments = sb.ToString();
Debug.WriteLine($"## pwsf: args: {string.Join(", ", args)}");
Debug.WriteLine($"## pwsf: arguments: {arguments}");

// trigger
Environment.SetEnvironmentVariable("FAR_PWSF_MODE", "1");

var process = Process.Start(new ProcessStartInfo
{
	UseShellExecute = false,
	FileName = fileName,
	Arguments = arguments,
});

process!.WaitForExit();
return process.ExitCode;
