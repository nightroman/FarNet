
/*
FarNet plugin for Far Manager
Copyright (c) 2005-2012 FarNet Team
*/

#include "StdAfx.h"
#include "PanelFileCollection.h"
#include "Panel1.h"

namespace FarNet
{;
PanelFileEnumerator::PanelFileEnumerator(Panel1^ panel, FileType type, int count)
: _Panel(panel)
, _Type(type)
, _Count(count)
, _Index(-1)
{
}

FarFile^ PanelFileEnumerator::Current::get()
{
	return _File;
}

bool PanelFileEnumerator::MoveNext()
{
	if (_Index >= _Count)
		return false;

	++_Index;
	if (_Index >= _Count)
	{
		_File = nullptr;
		return false;
	}

	_File = _Panel->GetFile(_Index, _Type);
	return true;
}

void PanelFileEnumerator::Reset()
{
	_File = nullptr;
	_Index = -1;
}

PanelFileCollection::PanelFileCollection(Panel1^ panel, FileType type)
: _Panel(panel)
, _Type(type)
, _Count(type == ShownFile ? panel->GetShownFileCount() : panel->GetSelectedFileCount())
{
}

FarFile^ PanelFileCollection::default::get(int index)
{
	if (index < 0 || index >= _Count)
		throw gcnew IndexOutOfRangeException("Invalid panel item index.");

	return _Panel->GetFile(index, _Type);
}

void PanelFileCollection::CopyTo(array<FarFile^>^ array, int arrayIndex)
{
	if (!array)
		throw gcnew ArgumentNullException("array");

	for(int i = 0; i < _Count; ++i)
		array[arrayIndex + i] = this[i];
}

}
