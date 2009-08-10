/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
*/

#pragma once
#include "Form.h"

namespace FarNet
{;
ref class Far;
ref class FarDialog;
ref class ListItemCollection;

ref class FarSubsetForm : FarForm, ISubsetForm
{
public:
	virtual bool Show() override;
public:
	virtual property array<Object^>^ Items { array<Object^>^ get(); void set(array<Object^>^ value); }
	virtual property array<int>^ Indexes { array<int>^ get(); void set(array<int>^ value); }
	virtual property Func<Object^, String^>^ ItemToString;
internal:
	FarSubsetForm();
private:
	void DoAdd();
	void DoRemove();
	void DoUp();
	void DoDown();
	void OnListBox1Clicked(Object^ sender, MouseClickedEventArgs^ e);
	void OnListBox2Clicked(Object^ sender, MouseClickedEventArgs^ e);
	void OnAddButtonClicked(Object^ sender, ButtonClickedEventArgs^ e);
	void OnRemoveButtonClicked(Object^ sender, ButtonClickedEventArgs^ e);
	void OnUpButtonClicked(Object^ sender, ButtonClickedEventArgs^ e);
	void OnDownButtonClicked(Object^ sender, ButtonClickedEventArgs^ e);
	void OnKeyPressed(Object^ sender, KeyPressedEventArgs^ e);
	String^ DoItemToString(Object^ value);
private:
	array<Object^>^ _Items;
	array<int>^ _Indexes;
	FarListBox^ _ListBox1;
	FarListBox^ _ListBox2;
};

}
