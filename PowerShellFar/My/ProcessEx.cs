
using FarNet;
using System.Diagnostics;

namespace My;

static class ProcessEx
{
	/// <summary>
	/// Just a wrapper and helper to watch calls.
	/// </summary>
	public static Process Start(string fileName, string arguments)
	{
		return Process.Start(new ProcessStartInfo { FileName = fileName, Arguments = arguments, UseShellExecute = true })!;
	}

	/// <summary>
	/// Opens the default browser.
	/// </summary>
	/// <remarks>
	/// https://brockallen.com/2016/09/24/process-start-for-urls-on-net-core/
	/// </remarks>
	public static void OpenBrowser(string url)
	{
		try
		{
			Process.Start(url);
		}
		catch (Exception ex)
		{
			Log.TraceException(ex);

			// hack because of this: https://github.com/dotnet/corefx/issues/10361
			url = url.Replace("&", "^&");
			Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
		}
	}
}
