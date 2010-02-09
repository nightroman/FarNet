/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#include "StdAfx.h"
#include "Viewer0.h"
#include "Viewer.h"
#include "Far.h"

namespace FarNet
{;
//! See Editor0::Editors().
array<IViewer^>^ Viewer0::Viewers()
{
	array<IViewer^>^ r = gcnew array<IViewer^>(_viewers.Count);
	int i = 0;
	for each(Viewer^ it in _viewers.Values)
		r[i++] = it;
	return r;
}

// Viewer must not be registered yet
void Viewer0::Register(Viewer^ viewer, const ViewerInfo& vi)
{
	_viewers.Add(vi.ViewerID, viewer);
	viewer->_id = vi.ViewerID;
	viewer->_FileName = gcnew String(vi.FileName);
}

//! For exturnal use.
Viewer^ Viewer0::GetCurrentViewer()
{
	// get current ID
	ViewerInfo vi; ViewerControl_VCTL_GETINFO(vi, true);
	if (vi.ViewerID < 0)
		return nullptr;

	// get viewer by ID
	//! It may be not yet registered: CtrlQ panel started with Far or CtrlQ of folder (there was no VE_READ event)
	Viewer^ viewer;
	if (!_viewers.TryGetValue(vi.ViewerID, viewer))
	{
		// CtrlQ of folder
		if (vi.FileName[0] == 0)
			return nullptr;

		// create and register
		viewer = gcnew Viewer;
		Register(viewer, vi);
	}
	return viewer;
}

int Viewer0::AsProcessViewerEvent(int type, void* param)
{
	switch(type)
	{
	case VE_READ:
		{
			LOG_AUTO(3, "VE_READ");

			// get info
			ViewerInfo vi; vi.StructSize = sizeof(vi);
			Info.ViewerControl(VCTL_GETINFO, &vi);

			// take waiting, existing or create new
			//! It really may exist on 'Add', 'Subtract' in a viewer open a file in the *same* viewer.
			Viewer^ viewer;
			if (_viewerWaiting)
			{
				viewer = _viewerWaiting;
				_viewerWaiting = nullptr;
				Register(viewer, vi);
			}
			else if (!_viewers.TryGetValue(vi.ViewerID, viewer))
			{
				viewer = gcnew Viewer;
				Register(viewer, vi);
			}
			else
			{
				// new file is opened in the same viewer -- update file name
				viewer->_FileName = gcnew String(vi.FileName);
			}

			// event
			if (_anyViewer._Opened)
				_anyViewer._Opened(viewer, nullptr);
			if (viewer->_Opened)
				viewer->_Opened(viewer, nullptr);
		}
		break;
	case VE_CLOSE:
		{
			LOG_AUTO(3, "VE_CLOSE");

			// get registered, close and unregister
			int id = *((int*)param);
			Viewer^ viewer;
			//! not found if CtrlQ on dots is closed (because READ was not called)
			//! fixed in 1.71.2335 but let it stay for a while. ??
			if (!_viewers.TryGetValue(id, viewer))
				break;
			viewer->_id = -2;
			_viewers.Remove(id);

			// event, after the above
			if (_anyViewer._Closed)
				_anyViewer._Closed(viewer, nullptr);
			if (viewer->_Closed)
				viewer->_Closed(viewer, nullptr);

			// delete the file after all
			DeleteSourceOptional(viewer->_FileName, viewer->DeleteSource);
		}
		break;
	case VE_GOTFOCUS:
		{
			LOG_AUTO(4, "VE_GOTFOCUS");

			// get registered
			int id = *((int*)param);
			Viewer^ viewer;
			if (!_viewers.TryGetValue(id, viewer))
				break;

			// event
			if (_anyViewer._GotFocus)
				_anyViewer._GotFocus(viewer, nullptr);
			if (viewer->_GotFocus)
				viewer->_GotFocus(viewer, nullptr);
		}
		break;
	case VE_KILLFOCUS:
		{
			LOG_AUTO(4, "VE_KILLFOCUS");

			// get registered
			int id = *((int*)param);
			Viewer^ viewer;
			if (!_viewers.TryGetValue(id, viewer))
				break;

			// event
			if (_anyViewer._LosingFocus)
				_anyViewer._LosingFocus(viewer, nullptr);
			if (viewer->_LosingFocus)
				viewer->_LosingFocus(viewer, nullptr);
		}
		break;
	}
	return 0;
}
}
