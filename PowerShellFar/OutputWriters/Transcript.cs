using FarNet;
using System.Management.Automation;

namespace PowerShellFar;
#pragma warning disable CS1591

public static class Transcript
{
	const string TextFileExistsNoClobber = "File {0} already exists and {1} was specified.";
	const string TextFileReadOnly = "Transcription file is read only.";
	const string TextFileMissing = "Transcription file is missing.";
	const string TextNotInProgress = "The host is not currently transcribing.";
	const string TextTranscriptStarted = "Transcript started, output file is {0}";
	const string TextTranscriptStopped = "Transcript stopped, output file is {0}";

	internal static TranscriptOutputWriter? Writer { get; private set; }
	private static string? _lastFileName;
	private static int _fileNameCount;

	public static void ShowTranscript(bool internalViewer)
	{
		string? fileName = Writer?.FileName ?? _lastFileName;
		if (fileName == null)
			return;

		if (!File.Exists(fileName))
			throw new InvalidOperationException(TextFileMissing);

		if (internalViewer)
		{
			var viewer = Far.Api.CreateViewer();
			viewer.Title = Path.GetFileName(fileName);
			viewer.FileName = fileName;
			viewer.CodePage = 1200;
			viewer.Open();
		}
		else
		{
			Zoo.StartExternalViewer(fileName);
		}
	}

	public static PSObject? StopTranscript(bool force)
	{
		if (Writer is null)
		{
			if (force)
				return null;

			throw new InvalidOperationException(TextNotInProgress);
		}

		_lastFileName = Writer.FileName!;
		Writer = Writer.Close();

		return GetTranscriptResult(TextTranscriptStopped, _lastFileName);
	}

	public static PSObject StartTranscript(Args args)
	{
		// resolve path
		var path = args.Path;
		if (string.IsNullOrEmpty(path))
		{
			if (string.IsNullOrEmpty(args.OutputDirectory))
			{
				var raw = A.GetVariableValue("Global:Transcript");
				if (raw is string str)
					path = str;
				else if (raw != null)
					throw new InvalidOperationException("$Transcript value is not a string.");
			}

			if (string.IsNullOrEmpty(path))
			{
				string dir;
				if (string.IsNullOrEmpty(args.OutputDirectory))
				{
					dir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
				}
				else
				{
					Directory.CreateDirectory(args.OutputDirectory);
					dir = args.OutputDirectory;
				}
				path = Path.Join(dir, NewFileName());
			}
		}

		if (File.Exists(path))
		{
			if (args.NoClobber && !args.Append)
				throw new InvalidOperationException(string.Format(TextFileExistsNoClobber, path, "NoClobber"));

			var fileInfo = new FileInfo(path);
			if ((fileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
			{
				if (!args.Force)
					throw new InvalidOperationException(TextFileReadOnly);

				fileInfo.Attributes &= ~FileAttributes.ReadOnly;
			}
		}
		else if (Directory.Exists(path))
		{
			throw new InvalidOperationException("The specified path is not a file.");
		}

		Writer = new TranscriptOutputWriter(Writer, path, args);

		return GetTranscriptResult(TextTranscriptStarted, path);
	}

	internal static string NewFileName()
	{
		++_fileNameCount;
		int process = Environment.ProcessId;
		return string.Format(null, "PowerShell_transcript.{0:yyyyMMddHHmmss}.{1}.{2}.txt", DateTime.UtcNow, process, _fileNameCount);
	}

	//! Start-Transcript and Stop-Transcript get PSObject(string) with note property Path.
	private static PSObject GetTranscriptResult(string format, string? path)
	{
		var res = PSObject.AsPSObject(string.Format(format, path));
		res.Properties.Add(new PSNoteProperty("Path", path));
		return res;
	}

	public class Args
	{
		public string? Path { get; set; }
		public string? OutputDirectory { get; set; }
		public bool Append { get; set; }
		public bool IncludeInvocationHeader { get; set; }
		public bool Force { get; set; }
		public bool NoClobber { get; set; }
		public bool UseMinimalHeader { get; set; }
	}
}
