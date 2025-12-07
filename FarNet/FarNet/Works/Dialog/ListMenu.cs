using FarNet.Forms;
using System.Text.RegularExpressions;

namespace FarNet.Works;
#pragma warning disable 1591

public sealed class ListMenu : AnyMenu, IListMenu
{
	private const int DefaultMaxWidth = 100;

	IListBox _box = null!;

	// Original user defined filter
	string _Incremental_ = string.Empty;

	// Currently used filter
	string _filter = string.Empty;

	// To update the filter
	bool _toFilter;

	// Key handler was invoked
	bool _isKeyHandled;

	// Filtered
	List<int>? _ii;
	Regex? _re;

	public bool AutoSelect { get; set; }

	public bool NoInfo { get; set; }

	public bool UsualMargins { get; set; }

	public int ScreenMargin { get; set; }

	public int MaxWidth { get; set; } = DefaultMaxWidth;

	public Guid TypeId { get; set; } = new Guid("01a43865-b81d-4bca-b3a4-a9ae4f9f7b55");

	public PatternOptions IncrementalOptions { get; set; }

	public string Incremental
	{
		get => _Incremental_;
		set
		{
			_Incremental_ = value ?? throw new ArgumentNullException(nameof(value));
			_filter = value;
			_re = null;
		}
	}

	void MakeFilter()
	{
		// done? skip
		if (!_toFilter)
			return;

		// mark done
		_toFilter = false;

		// create
		_re ??= BulletFilter.ToRegex(_filter, IncrementalOptions);
		if (_re == null)
			return;

		// case: filter already filtered
		if (_ii != null)
		{
			var ii = new List<int>();
			foreach (int k in _ii)
			{
				if (_re.IsMatch(myItems[k].Text))
					ii.Add(k);
			}
			_ii = ii;
			return;
		}

		// case: not yet filtered
		_ii = [];
		int i = -1;
		foreach (var mi in Items)
		{
			++i;
			if (_re.IsMatch(mi.Text))
				_ii.Add(i);
		}
	}

	string InfoLine()
	{
		var r = "(";
		if (_ii != null)
			r += _ii.Count + "/";
		r += Items.Count + ")";
		return Kit.JoinText(r, Bottom);
	}

	void GetInfo(out string head, out string foot)
	{
		head = Title ?? string.Empty;
		foot = NoInfo ? string.Empty : InfoLine();
		if (!string.IsNullOrEmpty(Bottom))
			foot += " ";
		if (IncrementalOptions != 0 && _filter.Length > 0)
		{
			if (SelectLast)
				foot = "[" + _filter + "]" + foot;
			else
				head = Kit.JoinText("[" + _filter + "]", head);
		}
	}

	void UpdateInfo(IDialog dialog)
	{
		GetInfo(out string t, out string b);
		dialog[0].Text = t;
		dialog[2].Text = b;
	}

	// Validates rect position and width by screen size so that rect is visible.
	static void ValidateRect(ref int x, ref int w, int min, int size)
	{
		if (x < 0)
			x = min + (size - w) / 2;
		int r = x + w - 1;
		if (r > min + size - 1)
		{
			x -= (r - min - size + 1);
			if (x < min)
				x = min;
			r = x + w - 1;
			if (r > min + size - 1)
				w -= (r - min - size + 1);
		}
	}

	void MakeSizes(IDialog dialog, Point size)
	{
		// controls with text
		var border = dialog[0];
		var bottom = dialog[2];

		// text lengths
		var borderText = border.Text;
		int borderTextLength = borderText == null ? 0 : borderText.Length;
		var bottomText = bottom?.Text;
		int bottomTextLength = bottomText == null ? 0 : bottomText.Length;

		// margins
		int ms = ScreenMargin > 1 ? ScreenMargin : 1;
		int mx = UsualMargins ? 2 : 0;
		int my = UsualMargins ? 1 : 0;

		// width
		int w = 0;
		if (_ii == null)
		{
			foreach (var mi in myItems)
				if (mi.Text.Length > w)
					w = mi.Text.Length;
		}
		else
		{
			foreach (int k in _ii)
				if (myItems[k].Text.Length > w)
					w = myItems[k].Text.Length;
		}
		w += 2 + 2 * mx; // if less last chars are lost

		// height
		int n = _ii == null ? myItems.Count : _ii.Count;
		if (MaxHeight > 0 && n > MaxHeight)
			n = MaxHeight;

		// adjust width
		if (w < borderTextLength)
			w = borderTextLength + 4;
		if (w < bottomTextLength)
			w = bottomTextLength + 4;
		if (w < 20)
			w = 20;

		// limit width, do X
		int dw = w + 4, dx = X, max;
		if (MaxWidth > 0 && dw > (max = MaxWidth + (UsualMargins ? 2 : 0)))
			dw = max;
		ValidateRect(ref dx, ref dw, ms, size.X - 2 * ms);

		// smart left
		int left = (Console.CursorLeft + dx) / 2;
		if (dx > left)
			dx = left;

		// do Y
		int dh = n + 2 + 2 * my, dy = Y;
		ValidateRect(ref dy, ref dh, ms, size.Y - 2 * ms);

		// dialog
		dialog.Rect = new Place(dx, dy, dx + dw - 1, dy + dh - 1);

		// border
		border.Rect = new Place(mx, my, dw - 1 - mx, dh - 1 - my);

		// list
		dialog[1].Rect = new Place(1 + mx, 1 + my, dw - 2 - mx, dh - 2 - my);

		// bottom
		if (bottom != null)
		{
			var xy = new Point(1 + mx, dh - 1 - my);
			bottom.Rect = new Place(xy.X, xy.Y, xy.X + bottomTextLength - 1, xy.Y);
		}
	}

	void OnConsoleSizeChanged(object? sender, SizeEventArgs e)
	{
		MakeSizes((IDialog)sender!, e.Size);
	}

	void OnKeyPressed(object? sender, KeyPressedEventArgs e)
	{
		// Tab: go to next
		if (e.Key.VirtualKeyCode == KeyCode.Tab)
		{
			var box = (IListBox)e.Control!;
			++box.Selected;
			e.Ignore = true;
			return;
		}

		var dialog = (IDialog)sender!;

		//! break keys first
		var key = new KeyData(e.Key.VirtualKeyCode, e.Key.CtrlAltShift());
		myKeyIndex = myKeys.IndexOf(key);
		if (myKeyIndex >= 0)
		{
			var handler = myHandlers[myKeyIndex];
			if (handler is null)
			{
				dialog.Close();
			}
			else
			{
				_isKeyHandled = true;
				Selected = _box.Selected;
				if (_ii != null && Selected >= 0)
					Selected = _ii[Selected];

				var a = new MenuEventArgs(Selected >= 0 ? myItems[Selected] : null);
				handler(Sender ?? this, a);
				if (a.Ignore)
				{
					e.Ignore = true;
					return;
				}
				dialog.Close();
				if (a.Restart)
				{
					myKeyIndex = -2;
					_ii = null;
					_toFilter = true;
				}
			}
			return;
		}

		// CtrlC: copy to clipboard
		if (e.Key.IsCtrl(KeyCode.C) || e.Key.IsCtrl(KeyCode.Insert))
		{
			var box = (IListBox)e.Control!;
			Far.Api.CopyToClipboard(box.Text);
			e.Ignore = true;
			return;
		}

		// not incremental?
		if (IncrementalOptions == 0)
			return;

		if (key.Is(KeyCode.Insert))
		{
			_filter = BulletFilter.AddBullet(_filter);
			UpdateInfo(dialog);
			return;
		}

		if (key.Is(KeyCode.Backspace) || key.IsShift(KeyCode.Backspace))
		{
			if (_filter.Length == 0)
				return;

			// case: Shift, drop incremental, including predefined
			if (key.IsShift())
			{
				Incremental = string.Empty;
				_ii = null;
				myKeyIndex = -2;
				_toFilter = false;
				dialog.Close();
				return;
			}

			// let only delete the filter excess over predefined
			if (_filter.Length > Incremental.Length || _filter.Length == Incremental.Length && Incremental.EndsWith('*'))
			{
				char c = _filter[^1];
				_filter = _filter[..^1];
				_re = null;
				// '*'
				if (0 == (IncrementalOptions & PatternOptions.Literal) && c == '*')
				{
					UpdateInfo(dialog);
				}
				else
				{
					_ii = null;
					_toFilter = true;
				}
			}
		}
		else
		{
			// char or paste
			var append = string.Empty;
			if (e.Key.Character >= ' ')
			{
				append = e.Key.Character.ToString();
			}
			else if (e.Key.IsCtrl(KeyCode.V) || e.Key.IsShift(KeyCode.Insert))
			{
				append = Far.Api.PasteFromClipboard();
			}

			if (append.Length > 0)
			{
				// keep and change filter
				var filterBak = _filter;
				var reBak = _re;
				_filter += append;
				_re = null;

				// append "*" -> do not close, just update title/bottom
				if (0 == (IncrementalOptions & PatternOptions.Literal) && append == "*")
				{
					UpdateInfo(dialog);
					return;
				}

				// try the filter, rollback on empty
				var iiBak = _ii;
				_toFilter = true;
				MakeFilter();
				if (_ii != null && _ii.Count == 0)
				{
					_filter = filterBak;
					_re = reBak;
					_ii = iiBak;
					return;
				}

				_toFilter = true;
			}
		}

		if (_toFilter)
			dialog.Close();
	}

	public override bool Show()
	{
		//! drop filter indexes because they are invalid on the second show if items have changed
		_ii = null;
		_toFilter = true;

		// main loop
		for (int pass = 0; ; ++pass)
		{
			// filter
			MakeFilter();

			// filtered item number
			int nItem2 = _ii == null ? myItems.Count : _ii.Count;
			if (nItem2 < 2 && AutoSelect)
			{
				if (nItem2 == 1)
				{
					Selected = _ii == null ? 0 : _ii[0];
					return true;
				}
				else if (pass == 0)
				{
					Selected = -1;
					return false;
				}
			}

			// title, bottom
			GetInfo(out string title, out string info);

			// dialog
			var dialog = Far.Api.CreateDialog(1, 1, 1, 1);
			dialog.HelpTopic = string.IsNullOrEmpty(HelpTopic) ? "list-menu" : HelpTopic;
			dialog.NoShadow = NoShadow;
			dialog.TypeId = TypeId;

			// title
			dialog.AddBox(1, 1, 1, 1, title);

			// list
			_box = dialog.AddListBox(1, 1, 1, 1, string.Empty);
			_box.Selected = Selected;
			_box.SelectLast = SelectLast;
			_box.NoBox = true;
			_box.WrapCursor = WrapCursor;
			if (IncrementalOptions == PatternOptions.None)
			{
				_box.AutoAssignHotkeys = AutoAssignHotkeys;
				_box.NoAmpersands = !ShowAmpersands;
			}

			// "bottom"
			if (info.Length > 0)
				dialog.AddText(1, 1, 1, info);

			// items and filter
			_box.ReplaceItems(myItems, _ii!);

			// now we are ready to make sizes
			MakeSizes(dialog, Far.Api.UI.WindowSize);

			// handlers
			dialog.ConsoleSizeChanged += OnConsoleSizeChanged;
			_box.KeyPressed += OnKeyPressed;

			// go!
			_toFilter = _isKeyHandled = false;
			myKeyIndex = -1;
			bool ok = dialog.Show();
			if (!ok)
				return false;
			if (myKeyIndex == -2 || _toFilter)
				continue;

			// correct by filter
			Selected = _box.Selected;
			if (_ii != null && Selected >= 0)
				Selected = _ii[Selected];

			// call click if a key was not handled yet
			if (Selected >= 0 && !_isKeyHandled)
			{
				var item = myItems[Selected];
				if (item.Click != null)
				{
					var e = new MenuEventArgs(item);
					item.Click(Sender ?? this, e);
					if (e.Ignore || e.Restart)
						continue;
				}
			}

			//! empty + enter = -1
			return Selected >= 0;
		}
	}
}
