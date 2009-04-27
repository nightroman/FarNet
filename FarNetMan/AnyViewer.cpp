/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
*/

#include "StdAfx.h"
#include "AnyViewer.h"
#include "Far.h"
#include "Viewer.h"

namespace FarNet
{;
/*
Issue [_090219_121638] On switching to editor the temp file is not deleted;
?? Editor in READ event can check existing viewer with DeleteSource::File,
drop the flag for editor and propagate this option to itself.
*/
void AnyViewer::ViewText(String^ text, String^ title, OpenMode mode)
{
	String^ tmpfile = Far::Instance->TempName();
	File::WriteAllText(tmpfile, text, Encoding::Unicode);

	Viewer viewer;
	viewer.DeleteSource = FarNet::DeleteSource::File; // yes, File - we can control it
	viewer.DisableHistory = true;
	viewer.FileName = tmpfile;
	viewer.Switching = Switching::Enabled;
	viewer.Title = title;
	viewer.Open(mode);
}
}
