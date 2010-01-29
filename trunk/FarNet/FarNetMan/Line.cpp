/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#include "StdAfx.h"
#include "Line.h"

namespace FarNet
{;
int Line::No::get()
{
	return -1;
}

String^ Line::EndOfLine::get()
{
	return String::Empty;
}

void Line::EndOfLine::set(String^ /*value*/)
{
}

}
