using FarNet;
using System.Management.Automation;

namespace PowerShellFar;
#pragma warning disable CS1591

public static class Transcript
{
	const string TextTranscriptFileExistsNoClobber = "File {0} already exists and {1} was specified.";
	const string TextTranscriptFileReadOnly = "Transcription file is read only.";
	const string TextTranscriptFileMissing = "Transcription file is missing.";
	const string TextTranscriptInProgress = "Transcription has already been started. Use the Stop-Transcript command to stop transcription.";
	const string TranscriptNotInProgress = "Transcription has not been started. Use the Start-Transcript command to start transcription.";
	const string TextTranscriptStarted = "Transcript started, output file is {0}";
	const string TextTranscriptStopped = "Transcript stopped, output file is {0}";

	internal static TranscriptOutputWriter? Writer { get; private set; }

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
			Zoo.StartExternalViewer(TranscriptOutputWriter.LastFileName);
		}
	}

	public static PSObject? StopTranscript(bool force)
	{
		if (Writer is null)
		{
			if (force)
				return null;

			throw new InvalidOperationException(TranscriptNotInProgress);
		}

		Writer.Close();
		Writer = null;

		return GetTranscriptResult(TextTranscriptStopped, TranscriptOutputWriter.LastFileName);
	}

	public static PSObject StartTranscript(string path, bool append, bool force, bool noClobber)
	{
		if (Writer != null)
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

		Writer = new TranscriptOutputWriter(path, append);

		return GetTranscriptResult(TextTranscriptStarted, path);
	}

	//! Start-Transcript and Stop-Transcript get PSObject(string) with note property Path.
	private static PSObject GetTranscriptResult(string format, string? path)
	{
		var res = PSObject.AsPSObject(string.Format(format, path));
		res.Properties.Add(new PSNoteProperty("Path", path));
		return res;
	}
}
