/*
FAR.NET plugin for Far Manager
Copyright (c) 2005-2008 FAR.NET Team
*/

#pragma once

namespace FarNet
{;
ref class AnyEditor : IAnyEditor
{
public: DEF_EVENT(Closed, _Closed);
public: DEF_EVENT(GotFocus, _GotFocus);
public: DEF_EVENT(LosingFocus, _LosingFocus);
public: DEF_EVENT(Opened, _Opened);
public: DEF_EVENT(Saving, _Saving);
public: DEF_EVENT_ARGS(OnKey, _OnKey, KeyEventArgs);
public: DEF_EVENT_ARGS(OnMouse, _OnMouse, MouseEventArgs);
public: DEF_EVENT_ARGS(OnRedraw, _OnRedraw, RedrawEventArgs);
public:
	virtual property String^ WordDiv { String^ get(); void set(String^ value); }
	virtual String^ EditText(String^ text, String^ title);
};
}
