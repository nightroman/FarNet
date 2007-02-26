#include "StdAfx.h"
#include "Collections.h"

namespace FarManagerImpl
{;
String^ EditorStringCollection::Text::get()
{
	StringBuilder sb;
	String^ eol = String::Empty;
	for each(ILine^ line in _lines)
	{
		sb.Append(eol + (_selected ? line->Selection->Text : line->Text));
		eol = line->Eol;
		if (eol->Length == 0)
			eol = _CRLF;
	}
	return sb.ToString();
}

void EditorStringCollection::Text::set(String^ value)
{
	if (value == nullptr)
		throw gcnew ArgumentNullException("value");

	if (_selected)
	{
		_lines->Text = value;
	}
	else
	{
		// Notes:
		// *) this method preserves empty last line
		// *) Split() gives 1+ lines even if value is empty

		Clear();
		value = value->Replace(_CRLF, _CR)->Replace('\n', '\r');
		bool ell = value->EndsWith(_CR);
		array<String^>^ arr = value->Split('\r');
		for (int i = 0, last = arr->Length - 1; ; ++i)
		{
			if (i < last)
			{
				Add(arr[i]);
				continue;
			}
			if (!ell)
			{
				this[i] = arr[i];
			}
			break;
		}
	}
}
}
