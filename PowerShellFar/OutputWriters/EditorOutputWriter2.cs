
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using System.Diagnostics;
using FarNet;

namespace PowerShellFar
{
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
