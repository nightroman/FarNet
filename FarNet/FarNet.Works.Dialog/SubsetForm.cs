/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;
using System.Collections.Generic;
using FarNet.Forms;

namespace FarNet.Works
{
	public sealed class SubsetForm : Form, ISubsetForm
	{
		const int DLG_XSIZE = 78;
		const int DLG_YSIZE = 22;

		public int[] Indexes { get; set; }
		public object[] Items { get; set; }
		public Converter<object, string> ItemToString { get; set; }

		IListBox _ListBox1;
		IListBox _ListBox2;

		public SubsetForm()
		{
			Title = "Select";
			Dialog.KeyPressed += OnKeyPressed;

			// "available" and "selected" list
			const int L1 = 4;
			const int R2 = DLG_XSIZE - 5;
			const int R1 = (L1 + R2) / 2;
			const int L2 = R1 + 1;
			const int BB = DLG_YSIZE - 4;

			_ListBox1 = Dialog.AddListBox(L1, 2, R1, BB, "Available");
			_ListBox1.NoClose = true;
			_ListBox1.MouseClicked += OnListBox1Clicked;

			_ListBox2 = Dialog.AddListBox(L2, 2, R2, BB, "Selected");
			_ListBox2.NoClose = true;
			_ListBox2.MouseClicked += OnListBox2Clicked;

			// buttons
			const int yButton = DLG_YSIZE - 3;
			IButton button;

			button = Dialog.AddButton(0, yButton, "Add");
			button.CenterGroup = true;
			button.ButtonClicked += OnAddButtonClicked;

			button = Dialog.AddButton(0, yButton, "Remove");
			button.CenterGroup = true;
			button.ButtonClicked += OnRemoveButtonClicked;

			button = Dialog.AddButton(0, yButton, "Up");
			button.CenterGroup = true;
			button.ButtonClicked += OnUpButtonClicked;

			button = Dialog.AddButton(0, yButton, "Down");
			button.CenterGroup = true;
			button.ButtonClicked += OnDownButtonClicked;

			button = Dialog.AddButton(0, yButton, "OK");
			button.CenterGroup = true;
			Dialog.Default = button;

			button = Dialog.AddButton(0, yButton, "Cancel");
			button.CenterGroup = true;
			Dialog.Cancel = button;
		}

		public override bool Show()
		{
			// no job
			if (Items == null || Items.Length == 0)
				return false;

			// drop items, Show() may be called several times
			_ListBox1.Items.Clear();
			_ListBox2.Items.Clear();

			// fill both lists
			if (Indexes != null && Indexes.Length > 0)
			{
				for (int index1 = 0; index1 < Items.Length; ++index1)
				{
					FarItem item;
					if (Array.IndexOf(Indexes, index1) < 0)
						item = _ListBox1.Add(DoItemToString(Items[index1]));
					else
						item = _ListBox2.Add(DoItemToString(Items[index1]));
					item.Data = index1;
				}
			}
			else
			{
				for (int index1 = 0; index1 < Items.Length; ++index1)
					_ListBox1.Add(DoItemToString(Items[index1])).Data = index1;
			}

			// the last fake selected item for inserting to the end
			_ListBox2.Add(String.Empty).Data = -1;
			_ListBox2.SelectLast = true;

			// go!
			if (!base.Show())
				return false;

			// collect and reset selected indexes
			List<int> r = new List<int>();
			foreach (FarItem item in _ListBox2.Items)
			{
				int index = (int)item.Data;
				if (index < 0)
					break;
				r.Add(index);
			}
			Indexes = r.ToArray();
			return true;
		}

		void DoAdd()
		{
			int selected1 = _ListBox1.Selected;
			if (selected1 >= 0)
			{
				FarItem item = _ListBox1.Items[selected1];
				_ListBox1.Items.RemoveAt(selected1);
				int selected2 = _ListBox2.Selected;
				_ListBox2.Items.Insert(selected2, item);
				_ListBox2.Selected = selected2 + 1;
			}
		}

		string DoItemToString(object value)
		{
			if (ItemToString != null)
				return ItemToString(value);

			if (value == null)
				return string.Empty;

			return value.ToString();
		}

		void DoRemove()
		{
			int selected2 = _ListBox2.Selected;
			if (selected2 >= 0 && selected2 < _ListBox2.Items.Count - 1)
			{
				FarItem item = _ListBox2.Items[selected2];
				int index2 = (int)item.Data;
				_ListBox2.Items.RemoveAt(selected2);
				for (int i = 0; i < _ListBox1.Items.Count; ++i)
				{
					int index1 = (int)_ListBox1.Items[i].Data;
					if (index2 < index1)
					{
						_ListBox1.Items.Insert(i, item);
						return;
					}
				}
				_ListBox1.Items.Add(item);
			}
		}

		void DoDown()
		{
			int selected2 = _ListBox2.Selected;
			if (selected2 >= 0 && selected2 < _ListBox2.Items.Count - 2)
			{
				FarItem item = _ListBox2.Items[selected2];
				FarItem next = _ListBox2.Items[selected2 + 1];
				_ListBox2.Items[selected2] = next;
				_ListBox2.Items[selected2 + 1] = item;
				_ListBox2.Selected = selected2 + 1;
			}
		}

		void DoUp()
		{
			int selected2 = _ListBox2.Selected;
			if (selected2 > 0 && selected2 < _ListBox2.Items.Count - 1)
			{
				FarItem item = _ListBox2.Items[selected2];
				FarItem prev = _ListBox2.Items[selected2 - 1];
				_ListBox2.Items[selected2] = prev;
				_ListBox2.Items[selected2 - 1] = item;
				_ListBox2.Selected = selected2 - 1;
			}
		}

		void OnAddButtonClicked(object sender, ButtonClickedEventArgs e)
		{
			e.Ignore = true;
			DoAdd();
		}

		//! Do not add Close() on Enter, Enter is called on ButtonClick (why?)
		void OnKeyPressed(object sender, KeyPressedEventArgs e)
		{
			switch (e.Code)
			{
				case KeyMode.Ctrl | KeyCode.Up:
					e.Ignore = true;
					Dialog.SetFocus(_ListBox2.Id);
					DoUp();
					return;
				case KeyMode.Ctrl | KeyCode.Down:
					e.Ignore = true;
					Dialog.SetFocus(_ListBox2.Id);
					DoDown();
					return;
				case KeyCode.Tab:
					if (Dialog.Focused == _ListBox2)
					{
						e.Ignore = true;
						Dialog.SetFocus(_ListBox1.Id);
						return;
					}
					break;
				case KeyCode.Enter:
				case KeyCode.Space:
					if (Dialog.Focused == _ListBox1)
					{
						e.Ignore = true;
						DoAdd();
						return;
					}
					else if (Dialog.Focused == _ListBox2)
					{
						e.Ignore = true;
						DoRemove();
						return;
					}
					break;
			}
		}

		void OnListBox1Clicked(object sender, MouseClickedEventArgs e)
		{
			if (e.Mouse.Action == MouseAction.DoubleClick)
			{
				e.Ignore = true;
				DoAdd();
			}
		}

		void OnListBox2Clicked(object sender, MouseClickedEventArgs e)
		{
			if (e.Mouse.Action == MouseAction.DoubleClick)
			{
				e.Ignore = true;
				DoRemove();
			}
		}

		void OnRemoveButtonClicked(object sender, ButtonClickedEventArgs e)
		{
			e.Ignore = true;
			DoRemove();
		}

		void OnDownButtonClicked(object sender, ButtonClickedEventArgs e)
		{
			e.Ignore = true;
			DoDown();
		}

		void OnUpButtonClicked(object sender, ButtonClickedEventArgs e)
		{
			e.Ignore = true;
			DoUp();
		}
	}
}
