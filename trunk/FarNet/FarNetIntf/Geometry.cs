using System;

namespace FarManager
{
	/// <summary>
	/// Some geometric shape
	/// </summary>
	public interface IShape
	{
		//ILineRegions RegionsOf();
		/// <summary>
		/// Does this page contains point
		/// </summary>
		/// <param name="point">a point</param>
		/// <returns>true, if contains</returns>
		bool Contains(IPoint point);
	}
	/// <summary>
	/// 2D point
	/// </summary>
	public interface IPoint
	{
		/// <summary>
		/// Position (x coordinate)
		/// </summary>
		int Pos { get; set; }
		/// <summary>
		/// Line (y coordinate)
		/// </summary>
		int Line { get; set; }
	}
	/// <summary>
	/// Shape which is defined by two points
	/// </summary>
	public interface ITwoPoint : IShape
	{
		/// <summary>
		/// First point
		/// </summary>
		IPoint First { get; set; }
		/// <summary>
		/// Last point
		/// </summary>
		IPoint Last { get; set; }
		/// <summary>
		/// Top line
		/// </summary>
		int Top { get; set; }
		/// <summary>
		/// Left pos
		/// </summary>
		int Left { get; set; }
		/// <summary>
		/// Right pos
		/// </summary>
		int Right { get; set; }
		/// <summary>
		/// Bottom line
		/// </summary>
		int Bottom { get; set; }
		/// <summary>
		/// Width of shape
		/// </summary>
		int Width { get; set; }
		/// <summary>
		/// Height of the shape
		/// </summary>
		int Height { get; set; }
	}
	/// <summary>
	/// Rectangle
	/// </summary>
	public interface IRect : ITwoPoint
	{
	}
}
