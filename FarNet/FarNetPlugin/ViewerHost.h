/*
FAR.NET plugin for Far Manager
Copyright (c) 2005-2008 FAR.NET Team
*/

#pragma once
#include "Viewer.h"

namespace FarNet
{;
ref class ViewerHost
{
	ViewerHost() {}
internal:
	static array<IViewer^>^ Viewers();
	static Viewer^ GetCurrentViewer();
	static int AsProcessViewerEvent(int type, void* param);
internal:
	// Viewer waiting for ID
	static Viewer^ _viewerWaiting;
	// Any viewer object
	static AnyViewer _anyViewer;
private:
	static void Register(Viewer^ viewer, const ViewerInfo& vi);
private:
	// Registered opened viewers
	static SortedList<int, Viewer^> _viewers;
};
}
