using System;

namespace FarManager.Impl
{
	public class Stream : ITwoPoint
	{
		IPoint _first = new Point();
		IPoint _last = new Point();

		public Stream()
		{
		}

		public Stream(int left, int top, int right, int bottom)
		{
			_first.Pos = left;
			_first.Line = top;
			_last.Pos = right;
			_last.Line = bottom;
		}

		public IPoint First
		{
			get { return _first; }
			set { _first = value; }
		}

		public IPoint Last
		{
			get { return _last; }
			set { _last = value; }
		}

		public int Top
		{
			get { return _first.Line; }
			set { _first.Line = value; }
		}
		
		public int Left
		{
			get { return _first.Pos; }
			set { _first.Pos = value; }
		}
		
		public int Bottom
		{
			get { return _last.Line; }
			set { _last.Line = value; }
		}
		
		public int Right
		{
			get { return _last.Pos; }
			set { _last.Pos = value; }
		}

		public bool IsA(object that)
		{
			if (that == null)
				throw new ArgumentNullException("that");
			return that.GetType() == this.GetType();
		}

		public int Width
		{
			get { return this.Right - this.Left + 1; }
			set { this.Right = (this.Left + value - 1); }
		}

		public int Height
		{
			get { return this.Bottom - this.Top + 1; }
			set { this.Bottom = (this.Top + value - 1); }
		}

		public virtual bool Contains(IPoint point)
		{
			if (point == null)
				throw new ArgumentNullException("point");
			return
				((point.Pos >= First.Pos) && (point.Pos <= Last.Pos) &&
				(point.Line >= First.Line) && (point.Line <= Last.Line)) ||
				((point.Line > First.Line) && (point.Line < Last.Line));
		}

		public override string ToString()
		{
			return "(" + Left + ", " + Top + ", " + Right + ", " + Bottom + ")";
		}
	}
}
