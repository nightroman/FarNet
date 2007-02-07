using System.Collections.Generic;
using System.Text;
using System;

namespace FarManager.Impl
{
	/// <summary>
	/// Implements IStrings editor lines as strings.
	/// </summary>
	public class EditorStringCollection : IStrings
	{
		ILines _lines;
		bool _selected = false;
		static string _CR = "\r";
		static string _CRLF = "\r\n";

		public EditorStringCollection(ILines lines, bool selected)
		{
			_lines = lines;
			_selected = selected;
		}

		public string Text
		{
			get
			{
				StringBuilder sb = new StringBuilder();
				string eol = string.Empty;
				foreach (ILine line in _lines)
				{
					sb.Append(eol + (_selected ? line.Selection.Text : line.Text));
					eol = line.Eol;
					if (eol.Length == 0)
						eol = _CRLF;
				}
				return sb.ToString();
			}
			set
			{
				if (value == null)
					throw new ArgumentNullException("value");

				if (_selected)
				{
					_lines.Text = value;
				}
				else
				{
					// Notes:
					// *) this method preserves empty last line
					// *) Split() gives 1+ lines even if value is empty

					Clear();
					value = value.Replace(_CRLF, _CR).Replace('\n', '\r');
					bool ell = value.EndsWith(_CR);
					string[] arr = value.Split('\r');
					for (int i = 0, last = arr.Length - 1; ; ++i)
					{
						if (i < last)
						{
							Add(arr[i]);
							continue;
						}
						if (!ell)
						{
							this[i] = arr[i];
						}
						break;
					}
				}
			}
		}

		#region IList Members

		public string this[int index]
		{
			get
			{
				if (_selected)
					return _lines[index].Selection.Text;
				else
					return _lines[index].Text;
			}
			set
			{
				if (_selected)
					_lines[index].Selection.Text = value;
				else
					_lines[index].Text = value;
			}
		}

		public void Add(string item)
		{
			_lines.Add(item);
		}

		public void Clear()
		{
			_lines.Clear();
		}

		public void Insert(int index, string item)
		{
			_lines.Insert(index, item);
		}

		public bool IsFixedSize
		{
			get { return false; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public void RemoveAt(int index)
		{
			_lines.RemoveAt(index);
		}

		public bool Contains(string item)
		{
			throw new NotSupportedException();
		}

		public int IndexOf(string item)
		{
			throw new NotSupportedException();
		}

		public bool Remove(string item)
		{
			throw new NotSupportedException();
		}

		#endregion

		#region ICollection Members

		public bool IsSynchronized
		{
			get { return false; }
		}

		public void CopyTo(string[] array, int arrayIndex)
		{
			if (array == null)
				throw new ArgumentNullException("array");
			if (arrayIndex < 0)
				throw new ArgumentOutOfRangeException("arrayIndex");
			if (array.Length - arrayIndex > Count)
				throw new ArgumentException("array, arrayIndex");
			foreach (string s in this)
				array[arrayIndex++] = s;
		}

		public int Count
		{
			get { return _lines.Count; }
		}

		public object SyncRoot
		{
			get { return this; }
		}

		#endregion

		#region IEnumerable Members

		public IEnumerator<string> GetEnumerator()
		{
			return new EditorStringEnumerator(_lines, 0, Count - 1);
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return new EditorStringEnumerator(_lines, 0, Count - 1);
		}

		#endregion
	}

	/// <summary>
	/// Enumerator of EditorStringCollection.
	/// </summary>
	class EditorStringEnumerator : IEnumerator<string>
	{
		ILines _lines;
		int _first;
		int _last;
		int _index;

		public EditorStringEnumerator(ILines lines, int first, int last)
		{
			_lines = lines;
			_first = first;
			_last = last;
			_index = first - 1;
		}

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			return ++_index <= _last;
		}

		public void Reset()
		{
			_index = _first;
		}

		string IEnumerator<string>.Current
		{
			get { return _lines[_index].Text; }
		}

		Object System.Collections.IEnumerator.Current
		{
			get { return _lines[_index].Text; }
		}
	}
}
