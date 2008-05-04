/*
FAR.NET plugin for Far Manager
Copyright (c) 2005-2008 FAR.NET Team
*/

#include "StdAfx.h"
#include "ListItemCollection.h"
#include "Dialog.h"

namespace FarNet
{;
ListItemCollection::ListItemCollection(FarBaseList^ box)
: _box(box)
{
}

void ListItemCollection::ClearItems()
{
	if (_box && _box->_dialog->_hDlg)
	{
		FarListDelete arg;
		arg.Count = 0;
		arg.StartIndex = 0;
		if (!Info.SendDlgMessage(_box->_dialog->_hDlg, DM_LISTDELETE, _box->Id, (LONG_PTR)&arg))
			throw gcnew OperationCanceledException;
	}

	Collection<IMenuItem^>::ClearItems();
}

void ListItemCollection::InsertItem(int index, IMenuItem^ item)
{
	if (index < 0 || index > Count)
		throw gcnew ArgumentOutOfRangeException("index");

	if (_box && _box->_dialog->_hDlg)
	{
		FarListInsert arg;
		arg.Index = index;
		FarBaseList::InitFarListItem(arg.Item, item);

		if (!Info.SendDlgMessage(_box->_dialog->_hDlg, DM_LISTINSERT, _box->Id, (LONG_PTR)&arg))
			throw gcnew OperationCanceledException;
	}

	Collection<IMenuItem^>::InsertItem(index, item);
}

void ListItemCollection::RemoveItem(int index)
{
	if (index < 0 || index >= Count)
		throw gcnew ArgumentOutOfRangeException("index");

	if (_box && _box->_dialog->_hDlg)
	{
		FarListDelete d;
		d.Count = 1;
		d.StartIndex = index;
		if (!Info.SendDlgMessage(_box->_dialog->_hDlg, DM_LISTDELETE, _box->Id, (LONG_PTR)&d))
			throw gcnew OperationCanceledException;
	}

	Collection<IMenuItem^>::RemoveItem(index);
}

void ListItemCollection::SetItem(int index, IMenuItem^ item)
{
	RemoveItem(index);
	InsertItem(index, item);
}

}
