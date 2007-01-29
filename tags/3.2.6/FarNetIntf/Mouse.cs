using System;

namespace FarManager
{
	/// <summary>
	/// Represents Control key state
	/// </summary>
	public class Controls
	{
		/// <summary>
		/// Left Alt down
		/// </summary>
		public bool LAlt { get { return _lAlt; } set { _lAlt = value; } }
		bool _lAlt;
		/// <summary>
		/// Right Alt down
		/// </summary>
		public bool RAlt { get { return _rAlt; } set { _rAlt = value; } }
		bool _rAlt;
		/// <summary>
		/// Left Cotrol down
		/// </summary>
		public bool LCtrl { get { return _lCtrl; } set { _lCtrl = value; } }
		bool _lCtrl;
		/// <summary>
		/// Right control down
		/// </summary>
		public bool RCtrl { get { return _rCtrl; } set { _rCtrl = value; } }
		bool _rCtrl;
		/// <summary>
		/// Shift down
		/// </summary>
		public bool Shift { get { return _shift; } set { _shift = value; } }
		bool _shift;
		/// <summary>
		/// NumLock key down
		/// </summary>
		public bool NumLock { get { return _numLock; } set { _numLock = value; } }
		bool _numLock;
		/// <summary>
		/// ScrollLock key down
		/// </summary>
		public bool ScrollLock { get { return _scrollLock; } set { _scrollLock = value; } }
		bool _scrollLock;
		/// <summary>
		/// Capslock key down
		/// </summary>
		public bool CapsLock { get { return _capsLock; } set { _capsLock = value; } }
		bool _capsLock;
		/// <summary>
		/// Enhanced key down
		/// </summary>
		public bool Enhanced { get { return _enhanced; } set { _enhanced = value; } }
		bool _enhanced;
		/// <summary>
		/// Ignore key down
		/// </summary>
		public bool Ignore { get { return _ignore; } set { _ignore = value; } }
		bool _ignore;
	}
	/// <summary>
	/// Represents mouse Event
	/// </summary>
	public class Mouse : Controls
	{
		/// <summary>
		/// Create new mouse event data
		/// </summary>
		public Mouse()
		{ }
		/// <summary>
		/// Line where ecent occurs
		/// </summary>
		public int Line { get { return _line; } set { _line = value; } }
		int _line;
		/// <summary>
		/// Position in the line
		/// </summary>
		public int Pos { get { return _pos; } set { _pos = value; } }
		int _pos;
		/// <summary>
		/// Left button down
		/// </summary>
		public bool Left { get { return _left; } set { _left = value; } }
		bool _left;
		/// <summary>
		/// Right button down
		/// </summary>
		public bool Right { get { return _right; } set { _right = value; } }
		bool _right;
		/// <summary>
		/// Button clicked
		/// </summary>
		public bool Click { get { return _click; } set { _click = value; } }
		bool _click;
		/// <summary>
		/// Double clicked
		/// </summary>
		public bool DoubleClick { get { return _doubleClick; } set { _doubleClick = value; } }
		bool _doubleClick;
		/// <summary>
		/// Moved
		/// </summary>
		public bool Moved { get { return _moved; } set { _moved = value; } }
		bool _moved;
		/// <summary>
		/// Mouse wheel rotated
		/// </summary>
		public bool Wheeled { get { return _wheeled; } set { _wheeled = value; } }
		bool _wheeled;
	}
}
