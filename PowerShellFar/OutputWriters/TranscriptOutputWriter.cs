using System.Text;

namespace PowerShellFar;

sealed class TranscriptOutputWriter : TextOutputWriter
{
	#region Text
	const string TextHeaderFull = """
		**********************
		PowerShell transcript start
		Start time: {0:yyyyMMddHHmmss}
		Username  : {1}\{2}
		Machine	  : {3} ({4})
		**********************
		""";

	const string TextHeaderMinimal = """
		**********************
		PowerShell transcript start
		Start time: {0:yyyyMMddHHmmss}
		**********************
		""";

	const string TextCommand = """
		**********************
		Command start time: {0:yyyyMMddHHmmss}
		**********************
		""";

	const string TextFooter = """

		**********************
		PowerShell transcript end
		End time: {0:yyyyMMddHHmmss}
		**********************
		""";
	#endregion

	readonly TranscriptOutputWriter? _parent;
	readonly Transcript.Args? _args;
	readonly bool _transcript;
	StreamWriter? _writer;
	string? _fileName;

	public string? FileName => _fileName;

	public TranscriptOutputWriter()
	{
	}

	public TranscriptOutputWriter(TranscriptOutputWriter? parent, string path, Transcript.Args args)
	{
		_parent = parent;
		_args = args;
		_writer = new StreamWriter(path, args.Append, Encoding.UTF8) { AutoFlush = true };

		if (args.UseMinimalHeader)
		{
			_writer.WriteLine(
				TextHeaderMinimal,
				DateTime.UtcNow);
		}
		else
		{
			_writer.WriteLine(
				TextHeaderFull,
				DateTime.UtcNow,
				Environment.UserDomainName,
				Environment.UserName,
				Environment.MachineName,
				Environment.OSVersion.VersionString);
		}

		_fileName = path;
		_transcript = true;
	}

	public TranscriptOutputWriter? Close()
	{
		if (_writer != null)
		{
			if (_transcript)
				_writer.Write(string.Format(null, TextFooter, DateTime.UtcNow));

			_writer.Close();
			_writer = null;
		}
		return _parent;
	}

	static string NewTempFileName()
	{
		// Tried to use the Personal folder (like PS does). For some reasons
		// some files are not deleted due to UnauthorizedAccessException.
		// It might be a virus scanner or an indexing service. Enough, the
		// files are temporary, use the Temp path. It's better to have not
		// deleted files there than in Personal.
		// NB: the above is for "transcribe always".

		return Path.Join(Path.GetTempPath(), Transcript.NewFileName());
	}

	void Writing()
	{
		if (_writer == null)
		{
			_fileName ??= NewTempFileName();

			_writer = new StreamWriter(_fileName, false, Encoding.Unicode) { AutoFlush = true };
		}
	}

	internal void WriteEcho(string echo)
	{
		_parent?.WriteEcho(echo);

		var sb = new StringBuilder(Environment.NewLine);
		if (_args?.IncludeInvocationHeader == true)
			sb.AppendLine(string.Format(null, TextCommand, DateTime.UtcNow));
		sb.AppendLine(echo);

		_writer!.Write(sb.ToString());
	}

	protected override void Append(string value)
	{
		_parent?.Append(value);

		Writing();
		_writer!.Write(value);
	}

	protected override void AppendLine()
	{
		_parent?.AppendLine();

		Writing();
		_writer!.WriteLine();
	}

	protected override void AppendLine(string value)
	{
		_parent?.AppendLine(value);

		Writing();
		_writer!.WriteLine(value);
	}
}
