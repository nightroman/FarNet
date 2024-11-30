
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using System.IO;
using System.Text;
using System.Threading;

namespace PowerShellFar;

class JobUI : UniformUI
{
	readonly Lock _lock = new();

	// Output writers
	StreamWriter? _writer;
	StreamOutputWriter _output = null!;

	// Output file name
	string? _fileName;

	// Output length
	long _length;

	public JobUI() : base()
	{
	}

	internal override OutputWriter Writer => _output;

	/// <summary>
	/// Output file name or null.
	/// </summary>
	public string? FileName => _fileName;

	/// <summary>
	/// Output length.
	/// </summary>
	public long Length
	{
		get
		{
			lock (_lock)
			{
				return _writer is null ? _length : _writer.BaseStream.Length;
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
		if (_writer is null)
		{
			// 090831 Stopped to use Far.TempName() to avoid MT issues
			//! NB: GetTempFileName() creates a file, so that then we append
			_fileName = Path.GetTempFileName();
			_writer = new StreamWriter(_fileName, true, Encoding.Unicode)
			{
				// for viewing
				AutoFlush = true
			};

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
		return _writer!;
	}
}
