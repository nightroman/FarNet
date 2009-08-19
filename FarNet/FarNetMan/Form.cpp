/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
*/

#include "StdAfx.h"
#include "Form.h"

namespace FarNet
{;
#define DLG_XSIZE 78
#define DLG_YSIZE 22

FarForm::FarForm()
: _Dialog(gcnew FarDialog(-1, -1, DLG_XSIZE, DLG_YSIZE))
{
	_Box = _Dialog->AddBox(3, 1, 0, 0, String::Empty);
}

String^ FarForm::Title::get()
{
	return _Box->Text;
}

void FarForm::Title::set(String^ value)
{
	_Box->Text = value;
}

bool FarForm::Show()
{
	return _Dialog->Show();
}
}
