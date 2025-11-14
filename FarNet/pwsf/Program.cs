using System.Diagnostics;
using System.Text;

var farHome = Environment.GetEnvironmentVariable("FARHOME");
var fileName = string.IsNullOrEmpty(farHome) ? "Far.exe" : $"{farHome}\\Far.exe";

var sb = new StringBuilder("/set:Panel.Left.Visible=false /set:Panel.Right.Visible=false");

var pwd = args.Length > 0 ? args[0] : Environment.CurrentDirectory;
if (pwd is { })
    sb.Append(" \"{pwd}\" \"{pwd}\"");

Environment.SetEnvironmentVariable("FAR_START_COMMAND", "ps:$Psf.StartCommandConsole()");

var process = Process.Start(new ProcessStartInfo
{
    UseShellExecute = false,
    FileName = fileName,
    Arguments = sb.ToString(),
});

process!.WaitForExit();
return process.ExitCode;
