
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2016 Roman Kuzmin
*/

/*
	[1]: \r\n written as two chars \r and \n
	if enter `far`in FSharpFar interactive then
		Funny output with extra empty lines and increasing indent with each next line
	do
		if last written char is \r then ignore \n
*/

using System.Globalization;
using System.IO;
using System.Text;

namespace FarNet.Works
{
	public sealed class EditorTextWriter : TextWriter
	{
		bool _RCharWritten;
		readonly IEditor _Editor;
		public EditorTextWriter(IEditor editor)
			: base(CultureInfo.CurrentCulture)
		{
			_Editor = editor;
			NewLine = "\r";
		}
		public override void Write(char value)
		{
			//! [1]
			if (value == '\r')
			{
				_RCharWritten = true;
			}
			else if (value == '\n')
			{
				if (_RCharWritten)
				{
					_RCharWritten = false;
					return;
				}
			}
			else
			{
				_RCharWritten = false;
			}

			_Editor.InsertChar(value);
		}
		public override void Write(string value)
		{
			_Editor.InsertText(value);
		}
		public override Encoding Encoding
		{
			get { return Encoding.Unicode; }
		}
	}
}
