/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
*/

#include "StdAfx.h"
#include "AnyEditor.h"
#include "EditorHost.h"
#include "Far.h"
#include "SelectionCollection.h"
#include "EditorLine.h"
#include "EditorLineCollection.h"

namespace FarNet
{;
String^ AnyEditor::WordDiv::get()
{
	int size = (int)Info.AdvControl(Info.ModuleNumber, ACTL_GETSYSWORDDIV, 0);
	CBox wd(size);
	Info.AdvControl(Info.ModuleNumber, ACTL_GETSYSWORDDIV, wd);
	return gcnew String(wd);
}

void AnyEditor::WordDiv::set(String^)
{
	throw gcnew NotSupportedException("You may set it only for an editor instance, not globally.");
}

String^ AnyEditor::EditText(String^ text, String^ title)
{
	String^ file = Far::Instance->TempName();
	try
	{
		if (SS(text))
			File::WriteAllText(file, text, Encoding::Default);
		
		IEditor^ edit = Far::Instance->CreateEditor();
		edit->FileName = file;
		edit->DisableHistory = true;
		if (SS(title))
			edit->Title = title;
		edit->Open(OpenMode::Modal);
		
		if (File::Exists(file))
		{
			// read and delete
			String^ r = File::ReadAllText(file, Encoding::Default);
			try
			{
				File::Delete(file);
			}
			catch(IOException^ e)
			{
				Log::TraceError(e);
			}
			return r;
		}
		else
		{
			// no file, e.g. the text was empty and user exits without saving; case 080502
			return String::Empty;
		}
	}
	finally
	{
		File::Delete(file);
	}
}
}
