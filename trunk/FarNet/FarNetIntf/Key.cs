using System;

namespace FarManager
{
	/// <summary>
	/// Keyboard event data
	/// </summary>
	public class Key : Controls
	{
		/// <summary>
		/// Creates new keyboard state
		/// </summary>
		public Key()
		{}
		/// <summary>
		/// Char code of key pressed
		/// </summary>
		public int Code { get { return _code; } set { _code = value; } }
		int _code;
		/// <summary>
		/// Scan code of key pressed
		/// </summary>
		public int Scan { get { return _scan; } set { _scan = value; } }
		int _scan;
		/// <summary>
		/// Character code
		/// </summary>
		public char Char { get { return _char; } set { _char = value; } }
		char _char;
		/// <summary>
		/// Number of repeats
		/// </summary>
		public int RepeatCount { get { return _repeatCount; } set { _repeatCount = value; } }
		int _repeatCount;
		/// <summary>
		/// Is key down
		/// </summary>
		public bool Down { get { return _down; } set { _down = value; } }
		bool _down;
	}
}
