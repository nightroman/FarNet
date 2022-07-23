// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Panel key bar item.
/// </summary>
public sealed class KeyBar
{
	/// <param name="virtualKeyCode">Virtual key code of <see cref="Key"/></param>
	/// <param name="controlKeyState">Control states of <see cref="Key"/></param>
	/// <param name="text">See <see cref="Text"/></param>
	/// <param name="longText">See <see cref="LongText"/></param>
	public KeyBar(int virtualKeyCode, ControlKeyStates controlKeyState, string text, string longText)
	{
		Key = new KeyData(virtualKeyCode, controlKeyState);
		Text = text;
		LongText = longText;
	}

	/// <summary>
	/// The assigned key.
	/// </summary>
	public KeyData Key { get; }

	/// <summary>
	/// The short key bar text.
	/// </summary>
	public string Text { get; }

	/// <summary>
	/// The long key bar text.
	/// </summary>
	public string LongText { get; }
}
