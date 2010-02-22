/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

namespace FarNet
{
	/// <summary>
	/// List of selected line parts and extra methods to manage editor selected area.
	/// </summary>
	public interface ISelection : ILines
	{
		/// <summary>
		/// Gets true if selection exists.
		/// </summary>
		bool Exists { get; }
		/// <summary>
		/// Gets the selection kind.
		/// </summary>
		RegionKind Kind { get; }
		/// <summary>
		/// Selects a region.
		/// </summary>
		/// <param name="kind">Selection kind.</param>
		/// <param name="pos1">Position 1.</param>
		/// <param name="line1">Line 1.</param>
		/// <param name="pos2">Position 2.</param>
		/// <param name="line2">Line 2.</param>
		void Select(RegionKind kind, int pos1, int line1, int pos2, int line2);
		/// <summary>
		/// Selects all text.
		/// </summary>
		void SelectAll();
		/// <summary>
		/// Turns the selection off.
		/// </summary>
		void Unselect();
		/// <summary>
		/// Gets the selected place.
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
}
