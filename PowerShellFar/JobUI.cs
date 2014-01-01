
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2014 Roman Kuzmin
*/

using System;
using System.IO;
using System.Text;

namespace PowerShellFar
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
	class JobUI : UniformUI
	{
		object _lock = new object();

		// Output writers
		StreamWriter _writer;
		StreamOutputWriter _output;

		// Output file name
		string _fileName;

		// Output length
		long _length;

		public JobUI()
			: base()
		{ }

		internal override OutputWriter Writer
		{
			get { return _output; }
		}

		/// <summary>
		/// Output file name or null.
		/// </summary>
		public string FileName
		{
			get { return _fileName; }
		}

		/// <summary>
		/// Output length.
		/// </summary>
		public long Length
		{
			get
			{
				lock (_lock)
				{
					return _writer == null ? _length : _writer.BaseStream.Length;
				}
			}
		}

		/// <summary>
		/// Closes if it is opened.
		/// </summary>
		internal void Close()
		{
			lock (_lock)
			{
				if (_writer != null)
				{
					_length = _writer.BaseStream.Length;
					_writer.Close();
					_writer = null;
				}
			}
		}

		/// <summary>
		/// Gets ready for writing.
		/// </summary>
		protected override void Writing()
		{
			if (_writer == null)
			{
				// 090831 Stopped to use Far.TempName() to avoid MT issues
				//! NB: GetTempFileName() creates a file, so that then we append
				_fileName = Path.GetTempFileName();
				_writer = new StreamWriter(_fileName, true, Encoding.Unicode);

				// for viewing
				_writer.AutoFlush = true;

				// wrap with output
				_output = new StreamOutputWriter(_writer);
			}
		}

		/// <summary>
		/// Gets opened writer.
		/// </summary>
		internal TextWriter GetWriter()
		{
			Writing();
			return _writer;
		}
	}
}
