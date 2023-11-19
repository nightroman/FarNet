// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Panel key bar item.
/// </summary>
/// <param name="virtualKeyCode">Virtual key code of <see cref="Key"/></param>
/// <param name="controlKeyState">Control states of <see cref="Key"/></param>
/// <param name="text">See <see cref="Text"/></param>
/// <param name="longText">See <see cref="LongText"/></param>
public sealed class KeyBar(int virtualKeyCode, ControlKeyStates controlKeyState, string text, string longText)
{
	/// <summary>
	/// The assigned key.
	/// </summary>
	public KeyData Key { get; } = new KeyData(virtualKeyCode, controlKeyState);

	/// <summary>
	/// The short key bar text.
	/// </summary>
	public string Text { get; } = text;

	/// <summary>
	/// The long key bar text.
	/// </summary>
	public string LongText { get; } = longText;
}
