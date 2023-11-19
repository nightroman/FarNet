
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.Generic;

namespace FarNet;

/// <summary>
/// Module drawer event arguments.
/// </summary>
/// <param name="colors">See <see cref="Colors"/></param>
/// <param name="lines">See <see cref="Lines"/></param>
/// <param name="startChar">See <see cref="StartChar"/></param>
/// <param name="endChar">See <see cref="EndChar"/></param>
public class ModuleDrawerEventArgs(ICollection<EditorColor> colors, IList<ILine> lines, int startChar, int endChar) : EventArgs
{
	/// <summary>
	/// Gets the result color collection. A drawer adds colors to it.
	/// </summary>
	public ICollection<EditorColor> Colors { get; } = colors;

	/// <summary>
	/// Gets the lines to get colors for. A drawer should not change this collection.
	/// </summary>
	public IList<ILine> Lines { get; } = lines;

	/// <summary>
	/// Gets the index of the first character.
	/// </summary>
	public int StartChar { get; } = startChar;

	/// <summary>
	/// Gets the index of the character after the last.
	/// </summary>
	public int EndChar { get; } = endChar;
}
