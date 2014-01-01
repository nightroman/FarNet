
/*
FarNet module RightWords
Copyright (c) 2011-2014 Roman Kuzmin
*/

using System.Collections.Generic;
namespace FarNet.RightWords
{
	class UIWordMenu
	{
		readonly IListMenu _menu;
		readonly FarItem _itemIgnore;
		readonly FarItem _itemIgnoreAll;
		readonly FarItem _itemAddToDictionary;
		public UIWordMenu(List<string> words, string word, int column, int line)
		{
			// menu
			_menu = Far.Api.CreateListMenu();
			_menu.Title = word;
			_menu.NoInfo = true;
			_menu.X = column;
			_menu.Y = line;

			// menu keys
			_menu.AddKey(KeyCode.D1);
			_menu.AddKey(KeyCode.D2);
			_menu.AddKey(KeyCode.D3);

			// menu items
			foreach (var it in words)
				_menu.Add(it);

			// menu commands
			_menu.Add(string.Empty).IsSeparator = true;
			_itemIgnore = _menu.Add(My.DoIgnore);
			_itemIgnoreAll = _menu.Add(My.DoIgnoreAll);
			_itemAddToDictionary = _menu.Add(My.DoAddToDictionary);
		}
		public bool Show()
		{
			return _menu.Show();
		}
		public bool IsIgnore { get { return _menu.Key.Is(KeyCode.D1) || _menu.Selected >= 0 && _menu.Items[_menu.Selected] == _itemIgnore; } }
		public bool IsIgnoreAll { get { return _menu.Key.Is(KeyCode.D2) || _menu.Selected >= 0 && _menu.Items[_menu.Selected] == _itemIgnoreAll; } }
		public bool IsAddToDictionary { get { return _menu.Key.Is(KeyCode.D3) || _menu.Selected >= 0 && _menu.Items[_menu.Selected] == _itemAddToDictionary; } }
		public string Word { get { return _menu.Selected < 0 ? string.Empty : _menu.Items[_menu.Selected].Text; } }
	}
}
