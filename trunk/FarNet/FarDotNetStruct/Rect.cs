using System;

namespace FarManager.Impl
{
	public class Rect : Stream, IRect
	{
		public Rect()
		{ }

		public Rect(int left, int top, int right, int bottom)
		{
			Top = top;
			Left = left;
			Right = right;
			Bottom = bottom;
		}

		public void Assign(Stream rect)
		{
			if (rect == null)
				throw new ArgumentNullException("rect");
			Top = rect.Top;
			Left = rect.Left;
			Right = rect.Right;
			Bottom = rect.Bottom;
		}

		public override bool Contains(IPoint point)
		{
			if (point == null)
				throw new ArgumentNullException("point");
			return
				(point.Pos >= First.Pos) && (point.Pos <= Last.Pos) &&
				(point.Line >= First.Line) && (point.Line <= Last.Line);
		}
	}
}
