using System.Diagnostics;
using System.Text;

const string FarExe = "Far.exe";

const string Usage = """
Usage: pwsf [<path> [<path>]] [<arguments>]

Shell arguments, see pwsh:

	-Command | -c ...
	-File | -f <file> ...
	-EncodedCommand | -ec <1200-base64>
	-NoExit | -noe
	-NoProfile | -nop

Far Manager arguments:

	-ro[-]
	-set:<parameter>=<value>
	-s <profilepath> [<localprofilepath>]

Other arguments:

	-nss
	-Panels | -pan
	-Exit | -x <delay>[:<timeout>]

See docs:

	https://github.com/nightroman/FarNet/blob/main/PowerShellFar/README.md#pwsf
""";

bool addReadOnly = false;
bool showPanels = false;

static void ExitUsage()
{
	Console.WriteLine(Usage.Replace("\t", "    "));
	Environment.Exit(-1);
}

static bool IsSwitch(string arg)
{
	return arg.StartsWith("-") || arg.StartsWith("/");
}

Debug.WriteLine($"## pwsf args: {'"' + string.Join("\" \"", args) + '"'}");
try
{
	// executable
	var baseDirectory = AppContext.BaseDirectory;
	var fileName = $"{baseDirectory}\\{FarExe}";

	// arguments
	var sb = new StringBuilder();

	int st;
	if (args.Length == 0 || IsSwitch(args[0]))
	{
		st = 0;
		var pwd = Environment.CurrentDirectory;
		sb.Append('"').Append(pwd).Append('"');
	}
	else
	{
		st = 0;
		while (true)
		{
			var pwd = args[st].TrimEnd('/').TrimEnd('\\');

			if (sb.Length > 0)
				sb.Append(' ');

			sb.Append('"').Append(pwd);
			if (pwd.EndsWith(":"))
				sb.Append('\\');
			sb.Append('"');

			++st;
			if (st > 1 || st >= args.Length || IsSwitch(args[st]))
				break;
		}
	}

	for (int i = st; i < args.Length; ++i)
	{
		var arg = args[i];
		if (!IsSwitch(arg))
			ExitUsage();

		var str = arg.TrimStart('-', '/');

		if (str == "ro" || str == "ro-")
		{
			addReadOnly = true;
		}
		else if (str == "s")
		{
			sb.Append(' ').Append(arg);
			if (++i >= args.Length)
				ExitUsage();

			sb.Append(' ').Append(args[i]);
			if (i + 1 < args.Length && !IsSwitch(args[i + 1]))
			{
				++i;
				sb.Append(' ').Append(args[i]);
			}
		}
		else if (str.StartsWith("set:"))
		{
			sb.Append(" \"").Append(arg).Append('"');
		}
		else if (str.StartsWith("nss"))
		{
			sb.Append(" -set:System.AutoSaveSetup=false");
		}
		else if (str == "NoExit" || str == "noe")
		{
			Environment.SetEnvironmentVariable("FAR_PWSF_NO_EXIT", "1");
		}
		else if (str == "NoProfile" || str == "nop")
		{
			Environment.SetEnvironmentVariable("FAR_PWSF_NO_PROFILE", "1");
		}
		else if (str == "Panels" || str == "pan")
		{
			showPanels = true;
		}
		else if (str == "Exit" || str == "x")
		{
			addReadOnly = true;
			showPanels = true;

			if (++i >= args.Length)
				ExitUsage();

			var parts = args[i].Split(':');
			if (parts.Length > 2)
				throw new Exception("Invalid Exit argument.");

			var delay = parts[0].Trim();
			if (delay.Length > 0)
			{
				if (!int.TryParse(delay, out _))
					throw new Exception("Invalid Exit delay.");

				Environment.SetEnvironmentVariable("FAR_START_DELAY", delay);
			}

			if (parts.Length > 1)
			{
				var timeout = parts[1].Trim();
				if (timeout.Length > 0)
				{
					if (!int.TryParse(timeout, out _))
						throw new Exception("Invalid Exit timeout.");

					Environment.SetEnvironmentVariable("FAR_START_TIMEOUT", timeout);
				}
			}
		}
		else if (str == "EncodedCommand" || str == "ec")
		{
			if (++i >= args.Length)
				ExitUsage();

			try
			{
				var bytes = Convert.FromBase64String(args[i]);
				var cmd = Encoding.Unicode.GetString(bytes);
				Environment.SetEnvironmentVariable("FAR_PWSF_COMMAND", cmd);
			}
			catch (FormatException ex)
			{
				throw new Exception($"Invalid EncodedCommand: {ex.Message}");
			}
		}
		else if (str == "Command" || str == "c")
		{
			if (++i >= args.Length)
				ExitUsage();

			var text = string.Join(" ", args.Skip(i));
			Environment.SetEnvironmentVariable("FAR_PWSF_COMMAND", text);
			break;
		}
		else if (str == "File" || str == "f")
		{
			if (++i >= args.Length)
				ExitUsage();

			var text = string.Join("\n", args.Skip(i));
			Environment.SetEnvironmentVariable("FAR_PWSF_FILE", text);
			break;
		}
		else
		{
			ExitUsage();
		}
	}


	if (addReadOnly)
	{
		sb.Append(" -ro");
	}

	if (showPanels)
	{
		sb.Append(" -set:Panel.Left.Visible=true -set:Panel.Right.Visible=true");
		Environment.SetEnvironmentVariable("FAR_PWSF_PANELS", "1");
	}
	else
	{
		sb.Append(" -set:Panel.Left.Visible=false -set:Panel.Right.Visible=false");
	}

	var arguments = sb.ToString();
	Debug.WriteLine($"## far args: {arguments}");

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
}
catch (Exception ex)
{
	Console.WriteLine($"pwsf: {ex.Message}");
	return -2;
}
