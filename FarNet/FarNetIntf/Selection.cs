using System;

namespace FarManager
{
	/// <summary>
	/// List of selected line parts and extra methods.
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
	}

	/// <summary>
	/// Types of selection. <see cref="ISelection.Type"/>
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
