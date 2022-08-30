
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.IO;
using System.Text;

namespace PowerShellFar;

sealed class TranscriptOutputWriter : TextOutputWriter
{
	public static string LastFileName { get; private set; }
	const string TextTranscriptPrologue = @"
**********************
Windows PowerShell transcript start
Start time: {0:yyyyMMddHHmmss}
Username  : {1}\{2}
Machine	  : {3} ({4})
**********************
";
	const string TextTranscriptEpilogue = @"
**********************
Windows PowerShell transcript end
End time: {0:yyyyMMddHHmmss}
**********************
";

	static int _fileNameCount;
	StreamWriter _writer;
	string _fileName;
	readonly bool _transcript;

	public string FileName => _fileName;

	public TranscriptOutputWriter()
	{
	}

	public TranscriptOutputWriter(string path, bool append)
	{
		_writer = new StreamWriter(path, append, Encoding.Unicode)
		{
			AutoFlush = true
		};

		_writer.WriteLine(
			TextTranscriptPrologue,
			DateTime.Now,
			Environment.UserDomainName,
			Environment.UserName,
			Environment.MachineName,
			Environment.OSVersion.VersionString);

		_fileName = path;
		_transcript = true;
		LastFileName = path;
	}

	public void Close()
	{
		if (_writer != null)
		{
			if (_transcript)
				_writer.Write(string.Format(null, TextTranscriptEpilogue, DateTime.Now));

			_writer.Close();
			_writer = null;
		}
	}

	static string NewFileName()
	{
		// Tried to use the Personal folder (like PS does). For some reasons
		// some files are not deleted due to UnauthorizedAccessException.
		// It might be a virus scanner or an indexing service. Enough, the
		// files are temporary, use the Temp path. It's better to have not
		// deleted files there than in Personal.
		// NB: the above is for "transcribe always".

		string directory = Path.GetTempPath();

		// next instant transcript
		++_fileNameCount;
		int process = Environment.ProcessId;
		return Path.Combine(
			directory,
			string.Format(null, "PowerShell_transcript.{0:yyyyMMddHHmmss}.{1}.{2}.txt", DateTime.Now, process, _fileNameCount));
	}

	void Writing()
	{
		if (_writer == null)
		{
			_fileName ??= NewFileName();

			_writer = new StreamWriter(_fileName, false, Encoding.Unicode)
			{
				AutoFlush = true
			};
		}
	}

	protected override void Append(string value)
	{
		Writing();
		_writer.Write(value);
	}

	protected override void AppendLine()
	{
		Writing();
		_writer.WriteLine();
	}

	protected override void AppendLine(string value)
	{
		Writing();
		_writer.WriteLine(value);
	}
}
