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
		/// Type of selection. <see cref="SelectionType"/>
		/// </summary>
		SelectionType Type { get; }
		/// <summary>
		/// Selects a region.
		/// </summary>
		/// <param name="type">Type of selection: <see cref="SelectionType"/>.</param>
		/// <param name="left">Left position.</param>
		/// <param name="top">Top line.</param>
		/// <param name="right">Right position.</param>
		/// <param name="bottom">Bottom line.</param>
		void Select(SelectionType type, int left, int top, int right, int bottom);
		/// <summary>
		/// Turns the selection off.
		/// </summary>
		void Unselect();
		/// <summary>
		/// Shape of selection; see <see cref="IFar.CreateRect"/>, <see cref="IFar.CreateStream"/>.
		/// </summary>
		ITwoPoint Shape { get; set; }
	}

	/// <summary>
	/// Types of selection. <see cref="ISelection.Type"/>
	/// </summary>
	public enum SelectionType
	{
		/// <summary>
		/// No selection.
		/// </summary>
		None,
		/// <summary>
		/// Stream selection.
		/// </summary>
		Stream,
		/// <summary>
		/// Rectangular selection.
		/// </summary>
		Rect
	}
}
