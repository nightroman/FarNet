using System;

namespace FarManager.Impl
{
	/// <summary>
	/// Point, i.e. line and position.
	/// </summary>
	public class Point : IPoint
	{
		int _pos = 0;
		int _line = 0;

		public int Pos
		{
			get { return _pos; }
			set { _pos = value; }
		}

		public int Line
		{
			get { return _line; }
			set { _line = value; }
		}

		public void Assign(Point point)
		{
			if (point == null)
				throw new ArgumentNullException("point");
			_line = point.Line;
			_pos = point.Pos;
		}

		public override string ToString()
		{
			return "(" + _pos + ", " + _line + ")";
		}
	}
}
