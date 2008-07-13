/*
FAR.NET plugin for Far Manager
Copyright (c) 2005-2008 FAR.NET Team
*/

#pragma once

namespace FarNet
{;
ref class AnyViewer : IAnyViewer
{
public: DEF_EVENT(Closed, _Closed);
public: DEF_EVENT(GotFocus, _GotFocus);
public: DEF_EVENT(LosingFocus, _LosingFocus);
public: DEF_EVENT(Opened, _Opened);
public:
	virtual void ViewText(String^ text, String^ title, OpenMode mode);
};
}
