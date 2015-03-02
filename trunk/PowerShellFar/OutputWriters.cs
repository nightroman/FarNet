
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2015 Roman Kuzmin
*/

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using FarNet;

namespace PowerShellFar
{
	abstract class OutputWriter
	{
		public abstract void Write(string value);
		public abstract void WriteLine();
		public abstract void WriteLine(string value);
		public abstract void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value);
		public abstract void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value);
		public abstract void WriteDebugLine(string message);
		public abstract void WriteErrorLine(string value);
		public abstract void WriteVerboseLine(string message);
		public abstract void WriteWarningLine(string message);
	}

	sealed class ConsoleOutputWriter : OutputWriter
	{
		string _header;
		public ConsoleOutputWriter() { }
		public ConsoleOutputWriter(string header, bool write = false)
		{
			_header = header;
			if (write)
				Writing();
		}
		void Writing()
		{
			// echo the command and drop it
			if (_header != null)
			{
				string header = _header + Environment.NewLine;
				Far.Api.UI.Write(header, Settings.Default.CommandForegroundColor);
				_header = null;

				if (A.Psf.Transcript != null)
					A.Psf.Transcript.WriteLine(Environment.NewLine + header);
			}
		}
		public override void Write(string value)
		{
			Writing();
			Far.Api.UI.Write(value);
			if (A.Psf.Transcript != null)
				A.Psf.Transcript.Write(value);
		}
		public override void WriteLine()
		{
			Writing();
			Far.Api.UI.WriteLine();
			if (A.Psf.Transcript != null)
				A.Psf.Transcript.WriteLine();
		}
		public override void WriteLine(string value)
		{
			Writing();
			Far.Api.UI.WriteLine(value);
			if (A.Psf.Transcript != null)
				A.Psf.Transcript.WriteLine(value);
		}
		public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
		{
			Writing();
			Far.Api.UI.Write(value, foregroundColor, backgroundColor);
			if (A.Psf.Transcript != null)
				A.Psf.Transcript.Write(value);
		}
		public override void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
		{
			Writing();
			Far.Api.UI.WriteLine(value, foregroundColor, backgroundColor);
			if (A.Psf.Transcript != null)
				A.Psf.Transcript.WriteLine(value);
		}
		public override void WriteDebugLine(string message)
		{
			Writing();
			Far.Api.UI.WriteLine("DEBUG: " + message, Settings.Default.DebugForegroundColor);
			if (A.Psf.Transcript != null)
				A.Psf.Transcript.WriteDebugLine(message);
		}
		public override void WriteErrorLine(string value)
		{
			Writing();
			Far.Api.UI.WriteLine(value, Settings.Default.ErrorForegroundColor);
			if (A.Psf.Transcript != null)
				A.Psf.Transcript.WriteErrorLine(value);
		}
		public override void WriteVerboseLine(string message)
		{
			Writing();
			Far.Api.UI.WriteLine("VERBOSE: " + message, Settings.Default.VerboseForegroundColor);
			if (A.Psf.Transcript != null)
				A.Psf.Transcript.WriteVerboseLine(message);
		}
		public override void WriteWarningLine(string message)
		{
			Writing();
			Far.Api.UI.WriteLine("WARNING: " + message, Settings.Default.WarningForegroundColor);
			if (A.Psf.Transcript != null)
				A.Psf.Transcript.WriteWarningLine(message);
		}
	}

	abstract class TextOutputWriter : OutputWriter
	{
		WriteMode _mode;
		/// <summary>
		/// 1 of 3 actual writers.
		/// </summary>
		protected abstract void Append(string value);
		/// <summary>
		/// 2 of 3 actual writers.
		/// </summary>
		protected abstract void AppendLine();
		/// <summary>
		/// 3 of 3 actual writers.
		/// </summary>
		protected abstract void AppendLine(string value);
		public sealed override void Write(string value)
		{
			_mode = WriteMode.None;
			Append(value);
		}
		public sealed override void WriteLine()
		{
			_mode = WriteMode.None;
			AppendLine();
		}
		public sealed override void WriteLine(string value)
		{
			_mode = WriteMode.None;
			AppendLine(value);
		}
		public sealed override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
		{
			_mode = WriteMode.None;
			Append(value);
		}
		public sealed override void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
		{
			_mode = WriteMode.None;
			AppendLine(value);
		}
		public sealed override void WriteDebugLine(string message)
		{
			if (_mode != WriteMode.Debug)
			{
				_mode = WriteMode.Debug;
				AppendLine("DEBUG:");
			}
			AppendLine(message);
		}
		public sealed override void WriteErrorLine(string value)
		{
			if (_mode != WriteMode.Error)
			{
				_mode = WriteMode.Error;
				AppendLine("ERROR:");
			}
			AppendLine(value);
		}
		public sealed override void WriteVerboseLine(string message)
		{
			if (_mode != WriteMode.Verbose)
			{
				_mode = WriteMode.Verbose;
				AppendLine("VERBOSE:");
			}
			AppendLine(message);
		}
		public sealed override void WriteWarningLine(string message)
		{
			if (_mode != WriteMode.Warning)
			{
				_mode = WriteMode.Warning;
				AppendLine("WARNING:");
			}
			AppendLine(message);
		}
	}

	sealed class StreamOutputWriter : TextOutputWriter
	{
		StreamWriter _writer;
		public StreamOutputWriter(StreamWriter writer)
		{
			_writer = writer;
		}
		protected override void Append(string value)
		{
			_writer.Write(value);
		}
		protected override void AppendLine()
		{
			_writer.WriteLine();
		}
		protected override void AppendLine(string value)
		{
			_writer.WriteLine(value);
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
	sealed class TranscriptOutputWriter : TextOutputWriter
	{
		public static string LastFileName { get; private set; }
		static string TextTranscriptPrologue = @"
**********************
Windows PowerShell transcript start
Start time: {0:yyyyMMddHHmmss}
Username  : {1}\{2} 
Machine	  : {3} ({4}) 
**********************
";
		static string TextTranscriptEpilogue = @"
**********************
Windows PowerShell transcript end
End time: {0:yyyyMMddHHmmss}
**********************
";

		static int _fileNameCount;
		StreamWriter _writer;
		string _fileName;
		bool _transcript;

		public string FileName { get { return _fileName; } }

		public TranscriptOutputWriter()
		{ }
		public TranscriptOutputWriter(string path, bool append)
		{
			_writer = new StreamWriter(path, append, Encoding.Unicode);
			_writer.AutoFlush = true;
			_writer.WriteLine(string.Format(null, TextTranscriptPrologue,
			DateTime.Now,
			Environment.UserDomainName,
			Environment.UserName,
			Environment.MachineName,
			Environment.OSVersion.VersionString
			));

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
		string NewFileName()
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
			int process = Process.GetCurrentProcess().Id;
			return Path.Combine(
				directory,
				string.Format(null, "PowerShell_transcript.{0:yyyyMMddHHmmss}.{1}.{2}.txt", DateTime.Now, process, _fileNameCount));
		}
		void Writing()
		{
			if (_writer == null)
			{
				if (_fileName == null)
					_fileName = NewFileName();

				_writer = new StreamWriter(_fileName, false, Encoding.Unicode);
				_writer.AutoFlush = true;
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

	/// <summary>
	/// Trivial editor writer, for example asynchronous.
	/// </summary>
	class EditorOutputWriter1 : TextOutputWriter
	{
		/// <summary>
		/// The editor.
		/// </summary>
		protected IEditor Editor { get; private set; }
		/// <summary>
		/// Write call count.
		/// </summary>
		internal int WriteCount { get; private set; }
		public EditorOutputWriter1(IEditor editor)
		{
			Editor = editor;
		}
		protected override void Append(string value)
		{
			// start
			if (++WriteCount == 1)
				Editor.InsertText("\r<=\r");

			// insert
			Editor.InsertText(value);
		}
		protected override void AppendLine()
		{
			// start
			if (++WriteCount == 1)
				Editor.InsertText("\r<=\r");
			else
				Editor.InsertLine();
		}
		protected override void AppendLine(string value)
		{
			// start
			if (++WriteCount == 1)
				Editor.InsertText("\r<=\r");

			// insert trimmed
			Editor.InsertText(value.TrimEnd() + "\r");
		}
	}

	/// <summary>
	/// Advanced editor synchronous writer.
	/// </summary>
	sealed class EditorOutputWriter2 : EditorOutputWriter1
	{
		Stopwatch _stopwatch = Stopwatch.StartNew();
		public EditorOutputWriter2(IEditor editor) : base(editor) { }
		void Redraw()
		{
			// max 25 redraw per second
			if (_stopwatch.ElapsedMilliseconds > 40)
			{
				Editor.Redraw();
				_stopwatch = Stopwatch.StartNew();
			}
		}
		protected override void Append(string value)
		{
			base.Append(value);
			Redraw();
		}
		protected override void AppendLine()
		{
			base.AppendLine();
			Redraw();
		}
		protected override void AppendLine(string value)
		{
			base.AppendLine(value);
			Redraw();
		}
	}

}
