using System.Diagnostics;
using System.Text;

const string FarExe = "Far.exe";

// executable path
var baseDirectory = AppContext.BaseDirectory;
var fileName = $"{baseDirectory}\\{FarExe}";

// make arguments
var sb = new StringBuilder("/set:Panel.Left.Visible=false /set:Panel.Right.Visible=false");
var pwd = args.Length > 0 ? args[0] : Environment.CurrentDirectory;
if (pwd is { })
    sb.Append(" \"{pwd}\" \"{pwd}\"");

// trigger
Environment.SetEnvironmentVariable("FAR_START_COMMAND", "ps:$Psf.StartCommandConsole()");

var process = Process.Start(new ProcessStartInfo
{
    UseShellExecute = false,
    FileName = fileName,
    Arguments = sb.ToString(),
});

process!.WaitForExit();
return process.ExitCode;
