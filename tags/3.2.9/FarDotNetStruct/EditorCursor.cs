using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace FarManager.Impl
{
	/// <summary>
	/// Cursor linked with another one
	/// </summary>
	public class ProxyEditorCursor : ICursor
	{
		ICursor _impl;

		public ICursor Impl
		{
			get { return _impl; }
			set { _impl = value; }
		}

		#region ICursor Members

		public int Line
		{
			get { return Impl.Line; }
			set { Impl.Line = value; }
		}

		public int Pos
		{
			get { return Impl.Pos; }
			set { Impl.Pos = value; }
		}

		public int TabPos
		{
			get { return Impl.TabPos; }
			set { Impl.TabPos = value; }
		}

		public int LeftPos
		{
			get { return Impl.LeftPos; }
			set { Impl.LeftPos = value; }
		}

		public int TopLine
		{
			get { return Impl.TopLine; }
			set { Impl.TopLine = value; }
		}

		public void Assign(ICursor cursor)
		{
			Impl.Assign(cursor);
		}

		public void Set(int pos, int line)
		{
			Impl.Set(pos, line);
		}

		#endregion

		public override string ToString()
		{
			return "(" + Pos + "/" + TabPos + ", " + Line + ")";
		}
	}

	/// <summary>
	/// Simple ICursor, just stored data
	/// </summary>
	public class StoredEditorCursor : ICursor
	{
		int _pos = -1;
		int _line = -1;
		int _tabPos = -1;
		int _leftPos = -1;
		int _topLine = -1;

		#region ICursor Members

		public int Line
		{
			get { return _line; }
			set { _line = value; }
		}

		public int Pos
		{
			get { return _pos; }
			set { _pos = value; }
		}
		
		public int TabPos
		{
			get { return _tabPos; }
			set { _tabPos = value; }
		}

		public int TopLine
		{
			get { return _topLine; }
			set { _topLine = value; }
		}

		public int LeftPos
		{
			get { return _leftPos; }
			set { _leftPos = value; }
		}

		public void Assign(ICursor cursor)
		{
			if (cursor == null)
				throw new ArgumentNullException("cursor");
			_pos = cursor.Pos;
			_line = cursor.Line;
			_tabPos = cursor.TabPos;
			_topLine = cursor.TopLine;
			_leftPos = cursor.LeftPos;
		}

		public void Set(int pos, int line)
		{
			_pos = pos;
			_line = line;
		}

		#endregion
	}
}
