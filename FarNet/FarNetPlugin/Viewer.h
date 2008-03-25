/*
FAR.NET plugin for Far Manager
Copyright (c) 2005-2008 FAR.NET Team
*/

#pragma once

namespace FarNet
{;
ref class BaseViewer : IAnyViewer
{
public: DEF_EVENT(Closed, _Closed);
public: DEF_EVENT(Opened, _Opened);
public:
	virtual void ViewText(String^ text, String^ title, OpenMode mode);
};

ref class Viewer : public BaseViewer, public IViewer
{
public:
	virtual property bool DisableHistory { bool get(); void set(bool value); }
	virtual property bool EnableSwitch { bool get(); void set(bool value); }
	virtual property bool HexMode { bool get(); void set(bool value); }
	virtual property bool IsOpened { bool get(); }
	virtual property bool WrapMode { bool get(); void set(bool value); }
	virtual property bool WordWrapMode { bool get(); void set(bool value); }
	virtual property FarManager::DeleteSource DeleteSource { FarManager::DeleteSource get(); void set(FarManager::DeleteSource value); }
	virtual property int Id { int get(); }
	virtual property Int64 FileSize { Int64 get(); }
	virtual property Place Window { Place get(); void set(Place value); }
	virtual property Point WindowSize { Point get(); }
	virtual property String^ FileName { String^ get(); void set(String^ value); }
	virtual property String^ Title { String^ get(); void set(String^ value); }
	virtual property ViewFrame Frame { ViewFrame get(); void set(ViewFrame value); }
public:
	virtual void Open();
	virtual void Open(OpenMode mode);
	virtual Int64 SetFrame(Int64 pos, int left, ViewFrameOptions options);
	virtual void Close();
	virtual void Redraw();
	virtual void Select(Int64 symbolStart, int symbolCount);
internal:
	Viewer();
private:
	[CA_USED]
	void AssertClosed();
internal:
	int _id;
	String^ _FileName;
private:
	FarManager::DeleteSource _DeleteSource;
	bool _DisableHistory;
	bool _EnableSwitch;
	Place _Window;
	String^ _Title;
};
}
