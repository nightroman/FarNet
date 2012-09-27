
/*
FarNet plugin for Far Manager
Copyright (c) 2005-2012 FarNet Team
*/

#pragma once

namespace FarNet
{;
ref class AnyViewer : IAnyViewer
{
public: DEF_EVENT_IMP(Closed, _Closed);
public: DEF_EVENT_IMP(GotFocus, _GotFocus);
public: DEF_EVENT_IMP(LosingFocus, _LosingFocus);
public: DEF_EVENT_IMP(Opened, _Opened);
public:
	virtual void ViewText(String^ text, String^ title, OpenMode mode) override;
};

ref class Viewer : IViewer
{
public: DEF_EVENT_IMP(Closed, _Closed);
public: DEF_EVENT_IMP(GotFocus, _GotFocus);
public: DEF_EVENT_IMP(LosingFocus, _LosingFocus);
public: DEF_EVENT_IMP(Opened, _Opened);
public:
	virtual property bool DisableHistory { bool get() override; void set(bool value) override; }
	virtual property ViewerViewMode ViewMode { ViewerViewMode get() override; void set(ViewerViewMode value) override; }
	virtual property bool WrapMode { bool get() override; void set(bool value) override; }
	virtual property bool WordWrapMode { bool get() override; void set(bool value) override; }
	virtual property DateTime TimeOfOpen { DateTime get() override; }
	virtual property FarNet::DeleteSource DeleteSource { FarNet::DeleteSource get() override; void set(FarNet::DeleteSource value) override; }
	virtual property int CodePage { int get() override; void set(int value) override; }
	virtual property IntPtr Id { IntPtr get() override; }
	virtual property Int64 FileSize { Int64 get() override; }
	virtual property Place Window { Place get() override; void set(Place value) override; }
	virtual property Point WindowSize { Point get() override; }
	virtual property String^ FileName { String^ get() override; void set(String^ value) override; }
	virtual property String^ Title { String^ get() override; void set(String^ value) override; }
	virtual property FarNet::Switching Switching { FarNet::Switching get() override; void set(FarNet::Switching value) override; }
	virtual property ViewFrame Frame { ViewFrame get() override; void set(ViewFrame value) override; }
public:
	virtual void Activate() override;
	virtual void Open(OpenMode mode) override;
	virtual Int64 SetFrame(Int64 pos, int left, ViewFrameOptions options) override;
	virtual void Close() override;
	virtual void Redraw() override;
	virtual void SelectText(Int64 symbolStart, int symbolCount) override;
internal:
	Viewer();
internal:
	intptr_t _id;
	String^ _FileName;
	DateTime _TimeOfOpen;
private:
	property bool IsOpened { bool get(); }
	void AssertClosed();
private:
	FarNet::DeleteSource _DeleteSource;
	FarNet::Switching _Switching;
	bool _DisableHistory;
	Place _Window;
	String^ _Title;
	intptr_t _CodePage;
};
}
