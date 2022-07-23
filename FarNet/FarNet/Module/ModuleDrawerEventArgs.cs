
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.Generic;

namespace FarNet;

/// <summary>
/// Module drawer event arguments.
/// </summary>
public class ModuleDrawerEventArgs : EventArgs
{
	/// <param name="colors">See <see cref="Colors"/></param>
	/// <param name="lines">See <see cref="Lines"/></param>
	/// <param name="startChar">See <see cref="StartChar"/></param>
	/// <param name="endChar">See <see cref="EndChar"/></param>
	public ModuleDrawerEventArgs(ICollection<EditorColor> colors, IList<ILine> lines, int startChar, int endChar)
	{
		Colors = colors;
		Lines = lines;
		StartChar = startChar;
		EndChar = endChar;
	}

	/// <summary>
	/// Gets the result color collection. A drawer adds colors to it.
	/// </summary>
	public ICollection<EditorColor> Colors { get; }

	/// <summary>
	/// Gets the lines to get colors for. A drawer should not change this collection.
	/// </summary>
	public IList<ILine> Lines { get; }

	/// <summary>
	/// Gets the index of the first character.
	/// </summary>
	public int StartChar { get; }

	/// <summary>
	/// Gets the index of the character after the last.
	/// </summary>
	public int EndChar { get; }
}
