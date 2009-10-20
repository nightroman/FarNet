/*
PowerShellFar plugin for Far Manager
Copyright (C) 2006-2009 Roman Kuzmin
*/

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using FarNet;

namespace PowerShellFar
{
	abstract class AnyOutputWriter
	{
		public abstract void Append(string value);
		public abstract void AppendLine();
		public abstract void AppendLine(string value);
	}

	/// <summary>
	/// Trivial editor writer, for example asynchronous.
	/// </summary>
	class EditorOutputWriter1 : AnyOutputWriter
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

		public override void Append(string value)
		{
			// start
			if (++WriteCount == 1)
				Editor.Insert("\r<=\r");

			// insert
			Editor.Insert(value);
		}

		public override void AppendLine()
		{
			// start
			if (++WriteCount == 1)
				Editor.Insert("\r<=\r");
			else
				Editor.InsertLine();
		}

		public override void AppendLine(string value)
		{
			// start
			if (++WriteCount == 1)
				Editor.Insert("\r<=\r");

			// insert trimmed
			Editor.Insert(value.TrimEnd() + "\r");
		}
	}

	/// <summary>
	/// Advanced editor synchronous writer.
	/// </summary>
	class EditorOutputWriter2 : EditorOutputWriter1
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

		public override void Append(string value)
		{
			base.Append(value);
			Redraw();
		}

		public override void AppendLine()
		{
			base.AppendLine();
			Redraw();
		}

		public override void AppendLine(string value)
		{
			base.AppendLine(value);
			Redraw();
		}
	}

	class StringOutputWriter : AnyOutputWriter
	{
		StringBuilder _output = new StringBuilder();

		public StringOutputWriter()
		{ }

		/// <summary>
		/// Output builder
		/// </summary>
		internal StringBuilder Output
		{
			get { return _output; }
		}

		public override void Append(string value)
		{
			_output.Append(value);
		}

		public override void AppendLine()
		{
			_output.AppendLine();
		}

		public override void AppendLine(string value)
		{
			_output.AppendLine(value);
		}
	}

	class ExternalOutputWriter : AnyOutputWriter, IDisposable
	{
		// Output file name
		internal static string _fileName;

		// Outer process
		static Process _process;

		// Output writer
		StreamWriter _writer;

		void Open()
		{
			if (_writer == null)
			{
				if (_fileName == null)
					_fileName = Path.GetTempFileName();

				StartViewer();

				_writer = new StreamWriter(_fileName, false, Encoding.Unicode);
				_writer.AutoFlush = true;
			}
			else
			{
				StartViewer();
			}
		}

		static void StartViewer()
		{
			if (_process == null || _process.HasExited)
			{
				string externalViewerFileName = A.Psf.Settings.ExternalViewerFileName;
				string externalViewerArguments;

				// try user defined external viewer
				if (!string.IsNullOrEmpty(externalViewerFileName))
				{
					externalViewerArguments = string.Format(CultureInfo.InvariantCulture, A.Psf.Settings.ExternalViewerArguments, _fileName);
					try
					{
						ProcessStartInfo info = new ProcessStartInfo(externalViewerFileName, externalViewerArguments);
						_process = Process.Start(info);
					}
					catch (Exception)
					{
						A.Far.Msg(
							"Cannot start the external viewer, default viewer will be used.\nYour settings:\nExternalViewerFileName: " + externalViewerFileName + "\nExternalViewerArguments: " + A.Psf.Settings.ExternalViewerArguments,
							Res.Name, MsgOptions.LeftAligned | MsgOptions.Warning);
					}
				}

				// use default external viewer
				if (_process == null)
				{
					externalViewerFileName = Process.GetCurrentProcess().MainModule.FileName;
					externalViewerArguments = "/p /m /v \"" + _fileName + "\"";

					ProcessStartInfo info = new ProcessStartInfo(externalViewerFileName, externalViewerArguments);
					_process = Process.Start(info);
				}
			}
		}

		public void Dispose()
		{
			if (_writer != null)
			{
				_writer.Close();
				_writer = null;
				GC.SuppressFinalize(this);
			}
		}

		public override void Append(string value)
		{
			Open();
			_writer.Write(value);
		}

		public override void AppendLine()
		{
			Open();
			_writer.WriteLine();
		}

		public override void AppendLine(string value)
		{
			Open();
			_writer.WriteLine(value);
		}
	}
}
