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
	-title:<text>
	-set:<parameter>=<value>
	-s <profilepath> [<localprofilepath>]

Other arguments:

	-far
	-nss
	-Panels | -pan
	-env <name>=<value>
	-Exit | -x <delay>[:<timeout>]

See docs:

	https://github.com/nightroman/FarNet/blob/main/PowerShellFar/README.md#pwsf
""";

bool addReadOnly = false;
bool noExit = false;
bool showPanels = false;

static void ExitUsage()
{
	Console.WriteLine(Usage.Replace("\t", "    "));
	Environment.Exit(-1);
}

static bool IsSwitch(string arg)
{
	return arg.StartsWith("-");
}

Debug.WriteLine($"## pwsf args: {'"' + string.Join("\" \"", args) + '"'}");
try
{
	ConsoleHelper.EnableAnsiEscapeSequences();
	Console.OutputEncoding = Encoding.UTF8;

	// executable
	var baseDirectory = AppContext.BaseDirectory;
	var fileName = $"{baseDirectory}\\{FarExe}";

	// arguments
	var sb = new StringBuilder();

	int st;
	string dir1, dir2;
	if (args.Length == 0 || IsSwitch(args[0]))
	{
		st = 0;
		dir1 = dir2 = Environment.CurrentDirectory;
	}
	else
	{
		dir1 = args[0].TrimEnd('/').TrimEnd('\\');
		if (dir1.EndsWith(":"))
			dir1 += '\\';

		if (args.Length == 1 || IsSwitch(args[1]))
		{
			st = 1;
			dir2 = dir1;
		}
		else
		{
			st = 2;
			dir2 = args[1].TrimEnd('/').TrimEnd('\\');
			if (dir2.EndsWith(":"))
				dir2 += '\\';
		}
	}
	sb.Append('"').Append(dir1).Append("\" \"").Append(dir2).Append('"');

	for (int i = st; i < args.Length; ++i)
	{
		var arg = args[i];
		if (!IsSwitch(arg))
			ExitUsage();

		var str = arg.TrimStart('-');

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
		else if (str.StartsWith("set:") || str.StartsWith("title:"))
		{
			sb.Append(" \"").Append(arg).Append('"');
		}
		else if (str.StartsWith("nss"))
		{
			sb.Append(" -set:System.AutoSaveSetup=0");
		}
		else if (str == "far")
		{
			noExit = true;
			showPanels = true;
		}
		else if (str == "NoExit" || str == "noe")
		{
			noExit = true;
		}
		else if (str == "NoProfile" || str == "nop")
		{
			Environment.SetEnvironmentVariable("FAR_PWSF_NO_PROFILE", "1");
		}
		else if (str == "Panels" || str == "pan")
		{
			showPanels = true;
		}
		else if (str == "env")
		{
			if (++i >= args.Length)
				ExitUsage();
			var envArg = args[i];
			var eq = envArg.IndexOf('=');
			if (eq < 1 || eq >= envArg.Length - 1)
				throw new Exception("Invalid env argument.");
			var name = envArg.Substring(0, eq).Trim();
			var value = envArg.Substring(eq + 1).Trim();
			Environment.SetEnvironmentVariable(name, value);
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

				Environment.SetEnvironmentVariable("FAR_PWSF_DELAY", delay);
			}

			if (parts.Length > 1)
			{
				var timeout = parts[1].Trim();
				if (timeout.Length > 0)
				{
					if (!int.TryParse(timeout, out _))
						throw new Exception("Invalid Exit timeout.");

					Environment.SetEnvironmentVariable("FAR_PWSF_TIMEOUT", timeout);
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

	if (noExit)
	{
		Environment.SetEnvironmentVariable("FAR_PWSF_NO_EXIT", "1");
	}

	if (addReadOnly)
	{
		sb.Append(" -ro");
	}

	if (showPanels)
	{
		sb.Append(" -set:Panel.LeftFocus=0 -set:Panel.Left.Visible=1 -set:Panel.Right.Visible=1");
		Environment.SetEnvironmentVariable("FAR_PWSF_PANELS", "1");
	}
	else
	{
		sb.Append(" -set:Panel.Left.Visible=0 -set:Panel.Right.Visible=0");
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
		RedirectStandardOutput = true,
		StandardOutputEncoding = Encoding.UTF8,
	});

	var output = process.StandardOutput;
	string? line;
	while ((line = output.ReadLine()) is { })
	{
		if (!line.StartsWith("Far Manager, version") && !line.StartsWith("Copyright © 1996-2000 Eugene Roshal"))
			Console.WriteLine(line);
	}

	process!.WaitForExit();
	return process.ExitCode;
}
catch (Exception ex)
{
	Console.WriteLine($"pwsf: {ex.Message}");
	return -2;
}
