
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2013 Roman Kuzmin
*/

#include "StdAfx.h"
#include "ListItemCollection.h"
#include "Dialog.h"
#include "DialogControls.h"

namespace FarNet
{;
ListItemCollection::ListItemCollection(FarBaseList^ box)
: _box(box)
{
}

void ListItemCollection::SetBox(FarBaseList^ value)
{
	_box = value;
}

void ListItemCollection::ClearItems()
{
	if (_box && _box->_dialog->_hDlg != INVALID_HANDLE_VALUE)
	{
		FarListDelete arg = {sizeof(arg)};
		arg.Count = 0;
		arg.StartIndex = 0;
		if (!Info.SendDlgMessage(_box->_dialog->_hDlg, DM_LISTDELETE, _box->Id, &arg))
			throw gcnew InvalidOperationException(__FUNCTION__);
	}

	Collection<FarItem^>::ClearItems();
}

// Bug [_090208_040000] combos crash in here after Clear().
// Listbox used to crash, too, fixed in Far.
void ListItemCollection::InsertItem(int index, FarItem^ item)
{
	if (index < 0 || index > Count)
		throw gcnew ArgumentOutOfRangeException("index");

	if (_box && _box->_dialog->_hDlg != INVALID_HANDLE_VALUE)
	{
		PIN_ES(pinText, item->Text);
		FarListInsert arg = {sizeof(arg)};
		FarBaseList::InitFarListItemShort(arg.Item, item);
		arg.Item.Text = pinText;
		arg.Index = index;

		if (!Info.SendDlgMessage(_box->_dialog->_hDlg, DM_LISTINSERT, _box->Id, &arg))
			throw gcnew InvalidOperationException(__FUNCTION__);
	}

	Collection<FarItem^>::InsertItem(index, item);
}

void ListItemCollection::RemoveItem(int index)
{
	if (index < 0 || index >= Count)
		throw gcnew ArgumentOutOfRangeException("index");

	if (_box && _box->_dialog->_hDlg != INVALID_HANDLE_VALUE)
	{
		FarListDelete d = {sizeof(d)};
		d.Count = 1;
		d.StartIndex = index;
		if (!Info.SendDlgMessage(_box->_dialog->_hDlg, DM_LISTDELETE, _box->Id, &d))
			throw gcnew InvalidOperationException(__FUNCTION__);
	}

	Collection<FarItem^>::RemoveItem(index);
}

void ListItemCollection::SetItem(int index, FarItem^ item)
{
	RemoveItem(index);
	InsertItem(index, item);
}

}
