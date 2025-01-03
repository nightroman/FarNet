using FarNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
	const string TextTranscriptFileExistsNoClobber = "File {0} already exists and {1} was specified.";
	const string TextTranscriptFileReadOnly = "Transcription file is read only.";
	const string TextTranscriptFileMissing = "Transcription file is missing.";
	const string TextTranscriptInProgress = "Transcription has already been started. Use the Stop-Transcript command to stop transcription.";
	const string TranscriptNotInProgress = "Transcription has not been started. Use the Start-Transcript command to start transcription.";
	const string TextTranscriptStarted = "Transcript started, output file is {0}";
	const string TextTranscriptStopped = "Transcript stopped, output file is {0}";

	///
	public static void ShowTranscript(bool internalViewer)
	{
		if (TranscriptOutputWriter.LastFileName == null)
			throw new InvalidOperationException(TranscriptNotInProgress);

		if (!File.Exists(TranscriptOutputWriter.LastFileName))
			throw new InvalidOperationException(TextTranscriptFileMissing);

		if (internalViewer)
		{
			var viewer = Far.Api.CreateViewer();
			viewer.Title = Path.GetFileName(TranscriptOutputWriter.LastFileName);
			viewer.FileName = TranscriptOutputWriter.LastFileName;
			viewer.CodePage = 1200;
			viewer.Open();
		}
		else
		{
			StartExternalViewer(TranscriptOutputWriter.LastFileName);
		}
	}

	//! Start-Transcript and Stop-Transcript get PSObject(string) with note property Path.
	static PSObject GetTranscriptResult(string format, string? path)
	{
		var res = PSObject.AsPSObject(string.Format(format, path));
		res.Properties.Add(new PSNoteProperty("Path", path));
		return res;
	}

	///
	public static PSObject? StopTranscript(bool force)
	{
		if (A.Psf.Transcript is null)
		{
			if (force)
				return null;

			throw new InvalidOperationException(TranscriptNotInProgress);
		}

		A.Psf.Transcript.Close();
		A.Psf.Transcript = null;

		return GetTranscriptResult(TextTranscriptStopped, TranscriptOutputWriter.LastFileName);
	}

	///
	public static PSObject StartTranscript(string path, bool append, bool force, bool noClobber)
	{
		if (A.Psf.Transcript != null)
			throw new InvalidOperationException(TextTranscriptInProgress);

		if (string.IsNullOrEmpty(path))
		{
			path = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.Personal),
				string.Format("PowerShell_transcript.{0:yyyyMMddHHmmss}.txt", DateTime.Now));
		}

		if (File.Exists(path))
		{
			if (noClobber && !append)
				throw new InvalidOperationException(string.Format(TextTranscriptFileExistsNoClobber, path, "NoClobber"));

			var fileInfo = new FileInfo(path);
			if ((fileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
			{
				if (!force)
					throw new InvalidOperationException(TextTranscriptFileReadOnly);

				fileInfo.Attributes &= ~FileAttributes.ReadOnly;
			}
		}

		A.Psf.Transcript = new TranscriptOutputWriter(path, append);

		return GetTranscriptResult(TextTranscriptStarted, path);
	}
	#endregion
}
