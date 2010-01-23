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

String^ Line::Eol::get()
{
	return String::Empty;
}

void Line::Eol::set(String^ /*value*/)
{
}

}
