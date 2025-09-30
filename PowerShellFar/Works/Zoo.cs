using FarNet;
using System.ComponentModel;
using System.Diagnostics;
using System.Management.Automation;

namespace PowerShellFar;

/// <summary>
/// For internal use and testing.
/// </summary>
public static class Zoo
{
	///
	public static Meta[] TablePanelSetupColumns(object[] columns)
	{
		return Format.SetupColumns(columns);
	}

	///
	public static Process StartExternalViewer(string fileName)
	{
		string externalViewerFileName = Settings.Default.ExternalViewerFileName;
		string externalViewerArguments;

		// try the user defined viewer
		if (!string.IsNullOrEmpty(externalViewerFileName))
		{
			externalViewerArguments = string.Format(null, Settings.Default.ExternalViewerArguments, fileName);
			try
			{
				return My.ProcessEx.Start(externalViewerFileName, externalViewerArguments);
			}
			catch (Win32Exception)
			{
				Far.Api.Message(
					"Cannot start the external viewer.",
					Res.Me, MessageOptions.LeftAligned | MessageOptions.Warning);
			}
		}

		// use default external viewer
		externalViewerFileName = Environment.ProcessPath!;
		externalViewerArguments = "/w- /ro /m /p /v \"" + fileName + "\"";
		return My.ProcessEx.Start(externalViewerFileName, externalViewerArguments);
	}

	#region Transcript

	#endregion
}
