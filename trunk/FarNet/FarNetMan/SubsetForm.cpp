/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#include "StdAfx.h"
#include "SubsetForm.h"
#include "DialogControls.h"

namespace FarNet
{;
#define DLG_XSIZE 78
#define DLG_YSIZE 22

FarSubsetForm::FarSubsetForm()
{
	Title = "Select";
	_Dialog->KeyPressed += gcnew EventHandler<KeyPressedEventArgs^>(this, &FarSubsetForm::OnKeyPressed); 

	// "available" and "selected" list
	const int L1 = 4;
	const int R2 = DLG_XSIZE - 5;
	const int R1 = (L1 + R2) / 2;
	const int L2 = R1 + 1;
	const int BB = DLG_YSIZE - 4;

	_ListBox1 = (FarListBox^)_Dialog->AddListBox(L1, 2, R1, BB, "Available");
	_ListBox1->NoClose = true;
	_ListBox1->MouseClicked += gcnew EventHandler<MouseClickedEventArgs^>(this, &FarSubsetForm::OnListBox1Clicked);

	_ListBox2 = (FarListBox^)_Dialog->AddListBox(L2, 2, R2, BB, "Selected");
	_ListBox2->NoClose = true;
	_ListBox2->MouseClicked += gcnew EventHandler<MouseClickedEventArgs^>(this, &FarSubsetForm::OnListBox2Clicked);

	// buttons
	const int yButton = DLG_YSIZE - 3;
	IButton^ button;
	
	button = _Dialog->AddButton(0, yButton, "Add");
	button->CenterGroup = true;
	button->ButtonClicked += gcnew EventHandler<ButtonClickedEventArgs^>(this, &FarSubsetForm::OnAddButtonClicked);
	
	button = _Dialog->AddButton(0, yButton, "Remove");
	button->CenterGroup = true;
	button->ButtonClicked += gcnew EventHandler<ButtonClickedEventArgs^>(this, &FarSubsetForm::OnRemoveButtonClicked);
	
	button = _Dialog->AddButton(0, yButton, "Up");
	button->CenterGroup = true;
	button->ButtonClicked += gcnew EventHandler<ButtonClickedEventArgs^>(this, &FarSubsetForm::OnUpButtonClicked);
	
	button = _Dialog->AddButton(0, yButton, "Down");
	button->CenterGroup = true;
	button->ButtonClicked += gcnew EventHandler<ButtonClickedEventArgs^>(this, &FarSubsetForm::OnDownButtonClicked);
	
	button = _Dialog->AddButton(0, yButton, "OK");
	button->CenterGroup = true;
	_Dialog->Default = button;
	
	button = _Dialog->AddButton(0, yButton, "Cancel");
	button->CenterGroup = true;
	_Dialog->Cancel = button;
}

String^ FarSubsetForm::DoItemToString(Object^ value)
{
	if (ItemToString)
		return ItemToString(value);
	
	if (!value)
		return String::Empty;

	return value->ToString();
}

bool FarSubsetForm::Show()
{
	// no job
	if (!_Items || _Items->Length == 0)
		return false;
	
	// drop items, Show() may be called several times
	_ListBox1->Items->Clear();
	_ListBox2->Items->Clear();

	// fill both lists
	if (_Indexes && _Indexes->Length > 0)
	{
		for(int index1 = 0; index1 < _Items->Length; ++index1)
		{
			FarItem^ item;
			if (Array::IndexOf(_Indexes, index1) < 0)
				item = _ListBox1->Add(DoItemToString(_Items[index1]));
			else
				item = _ListBox2->Add(DoItemToString(_Items[index1]));
			item->Data = index1;
		}
	}
	else
	{
		for(int index1 = 0; index1 < _Items->Length; ++index1)
			_ListBox1->Add(DoItemToString(_Items[index1]))->Data = index1;
	}

	// the last fake selected item for inserting to the end
	_ListBox2->Add(String::Empty)->Data = -1;
	_ListBox2->SelectLast = true;

	// go!
	if (!FarForm::Show())
		return false;

	// collect and reset selected indexes
	List<int> r = gcnew List<int>;
	for each(FarItem^ item in _ListBox2->Items)
	{
		int index = (int)item->Data;
		if (index < 0)
			break;
		r.Add(index);
	}
	_Indexes = r.ToArray();
	return true;
}

void FarSubsetForm::DoAdd()
{
	int selected1 = _ListBox1->Selected;
	if (selected1 >= 0)
	{
		FarItem^ item = _ListBox1->Items[selected1];
		_ListBox1->Items->RemoveAt(selected1);
		int selected2 = _ListBox2->Selected;
		_ListBox2->Items->Insert(selected2, item);
		_ListBox2->Selected = selected2 + 1;
	}
}

void FarSubsetForm::DoRemove()
{
	int selected2 = _ListBox2->Selected;
	if (selected2 >= 0 && selected2 < _ListBox2->Items->Count - 1)
	{
		FarItem^ item = _ListBox2->Items[selected2];
		int index2 = (int)item->Data;
		_ListBox2->Items->RemoveAt(selected2);
		for(int i = 0; i < _ListBox1->Items->Count; ++i)
		{
			int index1 = (int)_ListBox1->Items[i]->Data;
			if (index2 < index1)
			{
				_ListBox1->Items->Insert(i, item);
				return;
			}
		}
		_ListBox1->Items->Add(item);
	}
}

void FarSubsetForm::DoUp()
{
	int selected2 = _ListBox2->Selected;
	if (selected2 > 0 && selected2 < _ListBox2->Items->Count - 1)
	{
		FarItem^ item = _ListBox2->Items[selected2];
		FarItem^ prev = _ListBox2->Items[selected2 - 1];
		_ListBox2->Items[selected2] = prev;
		_ListBox2->Items[selected2 - 1] = item;
		_ListBox2->Selected = selected2 - 1;
	}
}

void FarSubsetForm::DoDown()
{
	int selected2 = _ListBox2->Selected;
	if (selected2 >= 0 && selected2 < _ListBox2->Items->Count - 2)
	{
		FarItem^ item = _ListBox2->Items[selected2];
		FarItem^ next = _ListBox2->Items[selected2 + 1];
		_ListBox2->Items[selected2] = next;
		_ListBox2->Items[selected2 + 1] = item;
		_ListBox2->Selected = selected2 + 1;
	}
}

//! Do not add Close() on Enter, Enter is called on ButtonClick (why?)
void FarSubsetForm::OnKeyPressed(Object^ /*sender*/, KeyPressedEventArgs^ e)
{
	switch(e->Code)
	{
	case KeyMode::Ctrl | KeyCode::Up:
		e->Ignore = true;
		_Dialog->SetFocus(_ListBox2->Id);
		DoUp();
		return;
	case KeyMode::Ctrl | KeyCode::Down:
		e->Ignore = true;
		_Dialog->SetFocus(_ListBox2->Id);
		DoDown();
		return;
	case KeyCode::Tab:
		if (_Dialog->Focused == _ListBox2)
		{
			e->Ignore = true;
			_Dialog->SetFocus(_ListBox1->Id);
			return;
		}
		break;
	case KeyCode::Enter:
	case KeyCode::Space:
		if (_Dialog->Focused == _ListBox1)
		{
			e->Ignore = true;
			DoAdd();
			return;
		}
		else if (_Dialog->Focused == _ListBox2)
		{
			e->Ignore = true;
			DoRemove();
			return;
		}
		break;
	}
}

array<Object^>^ FarSubsetForm::Items::get()
{
	return _Items;
}

void FarSubsetForm::Items::set(array<Object^>^ value)
{
	_Items = value;
}

array<int>^ FarSubsetForm::Indexes::get()
{
	return _Indexes;
}

void FarSubsetForm::Indexes::set(array<int>^ value)
{
	_Indexes = value;
}

void FarSubsetForm::OnListBox1Clicked(Object^ /*sender*/, MouseClickedEventArgs^ e)
{
	if (e->Mouse.Action == MouseAction::DoubleClick)
	{
		e->Ignore = true;
		DoAdd();
	}
}

void FarSubsetForm::OnListBox2Clicked(Object^ /*sender*/, MouseClickedEventArgs^ e)
{
	if (e->Mouse.Action == MouseAction::DoubleClick)
	{
		e->Ignore = true;
		DoRemove();
	}
}

void FarSubsetForm::OnAddButtonClicked(Object^ /*sender*/, ButtonClickedEventArgs^ e)
{
	e->Ignore = true;
	DoAdd();
}

void FarSubsetForm::OnRemoveButtonClicked(Object^ /*sender*/, ButtonClickedEventArgs^ e)
{
	e->Ignore = true;
	DoRemove();
}

void FarSubsetForm::OnUpButtonClicked(Object^ /*sender*/, ButtonClickedEventArgs^ e)
{
	e->Ignore = true;
	DoUp();
}

void FarSubsetForm::OnDownButtonClicked(Object^ /*sender*/, ButtonClickedEventArgs^ e)
{
	e->Ignore = true;
	DoDown();
}

}
