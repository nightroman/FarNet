/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
*/

#pragma once
#include "Editor.h"

namespace FarNet
{;
ref class EditorAsync : public Editor, public IEditorAsync
{
public:
	virtual void BeginAsync();
	virtual void EndAsync();
internal:
	EditorAsync();
private:
};
}
