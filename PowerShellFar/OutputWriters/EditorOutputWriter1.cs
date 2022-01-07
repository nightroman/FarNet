
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;

namespace PowerShellFar
{
	/// <summary>
	/// Trivial editor writer, for example asynchronous.
	/// </summary>
	class EditorOutputWriter1 : TextOutputWriter
	{
		/// <summary>
		/// The editor.
		/// </summary>
		protected IEditor Editor { get; private set; }
		public EditorOutputWriter1(IEditor editor)
		{
			Editor = editor;
		}
		protected override void Append(string value)
		{
			Editor.InsertText(value);
		}
		protected override void AppendLine()
		{
			Editor.InsertLine();
		}
		protected override void AppendLine(string value)
		{
			Editor.InsertText(value.TrimEnd() + "\r");
		}
	}
}
