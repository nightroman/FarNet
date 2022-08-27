
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System.Diagnostics;

namespace PowerShellFar;

/// <summary>
/// Base editor writer.
/// </summary>
abstract class EditorOutputWriter1 : TextOutputWriter
{
	Stopwatch _stopwatch = Stopwatch.StartNew();

	/// <summary>
	/// The editor.
	/// </summary>
	protected IEditor Editor { get; }

	public EditorOutputWriter1(IEditor editor)
	{
		Editor = editor;
	}

	protected abstract void Redraw();

	void Update()
	{
		// max 25 redraw per second
		if (_stopwatch.ElapsedMilliseconds > 40)
		{
			_stopwatch = Stopwatch.StartNew();
			Redraw();
		}
	}

	protected override void Append(string value)
	{
		Editor.InsertText(value);
		Update();
	}

	protected override void AppendLine()
	{
		Editor.InsertLine();
		Update();
	}

	protected override void AppendLine(string value)
	{
		Editor.InsertText(value.TrimEnd() + "\r");
		Update();
	}
}
