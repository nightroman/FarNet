#pragma once
#include "Viewer.h"

namespace FarNet
{
ref class Viewer0
{
	Viewer0() {}
internal:
	static array<IViewer^>^ Viewers();
	static Viewer^ GetCurrentViewer();
	static IViewer^ GetViewer(intptr_t id);
	static int AsProcessViewerEvent(const ProcessViewerEventInfo* info);
internal:
	// Viewer waiting for ID
	static Viewer^ _viewerWaiting;
	// Any viewer object
	static AnyViewer _anyViewer;
private:
	static void Register(Viewer^ viewer, const ViewerInfo& vi);
private:
	// Registered opened viewers
	static List<Viewer^> _viewers;
};
}
