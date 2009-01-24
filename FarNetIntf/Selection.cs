/*
FAR.NET plugin for Far Manager
Copyright (c) 2005-2009 FAR.NET Team
*/

using System;

namespace FarManager
{
	/// <summary>
	/// List of selected line parts and extra methods to manage editor selected area.
	/// </summary>
	public interface ISelection : ILines
	{
		/// <summary>
		/// True if selection exists.
		/// </summary>
		bool Exists { get; }
		/// <summary>
		/// Type of selection.
		/// </summary>
		SelectionType Type { get; }
		/// <summary>
		/// Selects a region.
		/// </summary>
		/// <param name="type">Type of selection: <see cref="SelectionType"/>.</param>
		/// <param name="pos1">Position 1.</param>
		/// <param name="line1">Line 1.</param>
		/// <param name="pos2">Position 2.</param>
		/// <param name="line2">Line 2.</param>
		void Select(SelectionType type, int pos1, int line1, int pos2, int line2);
		/// <summary>
		/// Turns the selection off.
		/// </summary>
		void Unselect();
		/// <summary>
		/// Shape of selection.
		/// </summary>
		Place Shape { get; }
		/// <summary>
		/// Gets text with default line separator.
		/// </summary>
		string GetText();
		/// <summary>
		/// Gets text.
		/// </summary>
		/// <param name="separator">Line separator. null ~ CRLF.</param>
		string GetText(string separator);
		/// <summary>
		/// Sets text.
		/// </summary>
		/// <param name="text">New text.</param>
		void SetText(string text);
	}

	/// <summary>
	/// Type of editor selected area.
	/// </summary>
	public enum SelectionType
	{
		/// <summary>
		/// No selection.
		/// </summary>
		None = 0,
		/// <summary>
		/// Rectangular selection.
		/// </summary>
		Rect = 2,
		/// <summary>
		/// Stream selection.
		/// </summary>
		Stream = 1,
	}
}
