
/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#include "StdAfx.h"
#include "Viewer0.h"
#include "Viewer.h"

namespace FarNet
{;
//! See Editor0::Editors().
array<IViewer^>^ Viewer0::Viewers()
{
	return _viewers.ToArray();
}

// Viewer must not be registered yet
void Viewer0::Register(Viewer^ viewer, const ViewerInfo& vi)
{
	_viewers.Insert(0, viewer);
	viewer->_id = vi.ViewerID;
	viewer->_TimeOfOpen = DateTime::Now;
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
	for(int i = 0; i < _viewers.Count; ++i)
	{
		if (_viewers[i]->_id == vi.ViewerID)
			return _viewers[i];
	}

	//! It may be not yet registered: CtrlQ panel started with Far or CtrlQ of folder (there was no VE_READ event)
	// CtrlQ of folder
	if (vi.FileName[0] == 0)
		return nullptr;

	// create and register
	Viewer^ viewer = gcnew Viewer;
	Register(viewer, vi);
	return viewer;
}

int Viewer0::AsProcessViewerEvent(int type, void* param)
{
	switch(type)
	{
	case VE_READ:
		{
			Log::Source->TraceInformation("VE_READ");

			// get info
			ViewerInfo vi; vi.StructSize = sizeof(vi);
			Info.ViewerControl(VCTL_GETINFO, &vi);

			// take waiting, existing or create new
			//! It really may exist on 'Add', 'Subtract' in a viewer open a file in the *same* viewer.
			Viewer^ viewer = nullptr;
			if (_viewerWaiting)
			{
				viewer = _viewerWaiting;
				_viewerWaiting = nullptr;
				Register(viewer, vi);
			}
			else
			{
				// find registered
				for(int i = 0; i < _viewers.Count; ++i)
				{
					if (_viewers[i]->_id == vi.ViewerID)
					{
						// new file is opened in the same viewer -- update file name
						viewer = _viewers[i];
						viewer->_FileName = gcnew String(vi.FileName);
						break;
					}
				}

				// not yet
				if (viewer == nullptr)
				{
					viewer = gcnew Viewer;
					Register(viewer, vi);
				}
			}

			// event
			if (_anyViewer._Opened)
			{
				Log::Source->TraceInformation("Opened");
				_anyViewer._Opened(viewer, nullptr);
			}
			if (viewer->_Opened)
			{
				Log::Source->TraceInformation("Opened");
				viewer->_Opened(viewer, nullptr);
			}
		}
		break;
	case VE_CLOSE:
		{
			Log::Source->TraceInformation("VE_CLOSE");

			// get registered, close and unregister
			int id = *((int*)param);
			Viewer^ viewer = nullptr;
			for(int i = 0; i < _viewers.Count; ++i)
			{
				if (_viewers[i]->_id == id)
				{
					viewer = _viewers[i];
					viewer->_id = -2;
					_viewers.RemoveAt(i);
					break;
				}
			}

			//! not found if CtrlQ on dots is closed (because READ was not called)
			//! fixed in 1.71.2335 but let it stay for a while. ??
			if (viewer == nullptr)
				break;

			// event, after the above
			if (_anyViewer._Closed)
			{
				Log::Source->TraceInformation("Closed");
				_anyViewer._Closed(viewer, nullptr);
			}
			if (viewer->_Closed)
			{
				Log::Source->TraceInformation("Closed");
				viewer->_Closed(viewer, nullptr);
			}

			// delete the file after all
			DeleteSourceOptional(viewer->_FileName, viewer->DeleteSource);
		}
		break;
	case VE_GOTFOCUS:
		{
			Log::Source->TraceEvent(TraceEventType::Verbose, 0, "VE_GOTFOCUS");

			// get registered
			int id = *((int*)param);
			Viewer^ viewer = nullptr;
			for(int i = 0; i < _viewers.Count; ++i)
			{
				if (_viewers[i]->_id == id)
				{
					viewer = _viewers[i];
					if (i > 0)
					{
						_viewers.RemoveAt(i);
						_viewers.Insert(0, viewer);
					}
					break;
				}
			}

			if (viewer == nullptr)
				break;

			// event
			if (_anyViewer._GotFocus)
			{
				Log::Source->TraceEvent(TraceEventType::Verbose, 0, "GotFocus");
				_anyViewer._GotFocus(viewer, nullptr);
			}
			if (viewer->_GotFocus)
			{
				Log::Source->TraceEvent(TraceEventType::Verbose, 0, "GotFocus");
				viewer->_GotFocus(viewer, nullptr);
			}
		}
		break;
	case VE_KILLFOCUS:
		{
			Log::Source->TraceEvent(TraceEventType::Verbose, 0, "VE_KILLFOCUS");

			// get registered
			int id = *((int*)param);
			Viewer^ viewer = nullptr;
			for(int i = 0; i < _viewers.Count; ++i)
			{
				if (_viewers[i]->_id == id)
				{
					viewer = _viewers[i];
					break;
				}
			}

			if (viewer == nullptr)
				break;

			// event
			if (_anyViewer._LosingFocus)
			{
				Log::Source->TraceEvent(TraceEventType::Verbose, 0, "LosingFocus");
				_anyViewer._LosingFocus(viewer, nullptr);
			}
			if (viewer->_LosingFocus)
			{
				Log::Source->TraceEvent(TraceEventType::Verbose, 0, "LosingFocus");
				viewer->_LosingFocus(viewer, nullptr);
			}
		}
		break;
	}
	return 0;
}
}
