/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
*/

#pragma once
#include "AnyViewer.h"

namespace FarNet
{;
ref class Viewer : public AnyViewer, public IViewer
{
public:
	virtual property bool DisableHistory { bool get(); void set(bool value); }
	virtual property bool HexMode { bool get(); void set(bool value); }
	virtual property bool IsOpened { bool get(); }
	virtual property bool WrapMode { bool get(); void set(bool value); }
	virtual property bool WordWrapMode { bool get(); void set(bool value); }
	virtual property DeleteSource DeleteSource { FarNet::DeleteSource get(); void set(FarNet::DeleteSource value); }
	virtual property int CodePage { int get(); void set(int value); }
	virtual property int Id { int get(); }
	virtual property Int64 FileSize { Int64 get(); }
	virtual property Place Window { Place get(); void set(Place value); }
	virtual property Point WindowSize { Point get(); }
	virtual property String^ FileName { String^ get(); void set(String^ value); }
	virtual property String^ Title { String^ get(); void set(String^ value); }
	virtual property Switching Switching { FarNet::Switching get(); void set(FarNet::Switching value); }
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
	void AssertClosed();
internal:
	int _id;
	String^ _FileName;
private:
	FarNet::DeleteSource _DeleteSource;
	FarNet::Switching _Switching;
	bool _DisableHistory;
	Place _Window;
	String^ _Title;
	int _CodePage;
};
}
