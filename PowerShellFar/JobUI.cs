/*
PowerShellFar plugin for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.IO;
using System.Text;

namespace PowerShellFar
{
	class JobUI : UniformUI
	{
		object _lock = new object();
		
		// Output writer
		StreamWriter _writer;

		// Output file name
		string _fileName;

		// Output length
		long _length;

		// Error write count
		int _error;

		public JobUI()
			: base()
		{ }

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
		/// Is there an error?
		/// </summary>
		public bool IsError
		{
			get
			{
				lock (_lock)
				{
					return _error > 0;
				}
			}
			set
			{
				lock (_lock)
				{
					_error = value ? _error + 1 : 0;
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
		void Writing()
		{
			if (_writer == null)
			{
				// 090831 Stopped to use Far.TempName() to avoid MT issues
				//! NB: GetTempFileName() creates a file, so that then we append
				_fileName = Path.GetTempFileName();
				_writer = new StreamWriter(_fileName, true, Encoding.Unicode);

				// for viewing
				_writer.AutoFlush = true;
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

		internal override void Append(string value)
		{
			Writing();
			_writer.Write(value);
		}

		internal override void AppendLine()
		{
			Writing();
			_writer.WriteLine();
		}

		internal override void AppendLine(string value)
		{
			Writing();
			_writer.WriteLine(value);
		}

		public override void WriteErrorLine(string value)
		{
			++_error;
			base.WriteErrorLine(value);
		}

	}
}
