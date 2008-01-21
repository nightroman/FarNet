/*
FAR.NET plugin for Far Manager
Copyright (c) 2005-2007 FAR.NET Team
*/

#pragma once

namespace FarNet
{;

ref class Viewer : public IViewer
{
public:
	virtual property bool Async { bool get(); void set(bool value); }
	virtual property bool DeleteOnClose { bool get(); void set(bool value); }
	virtual property bool DeleteOnlyFileOnClose { bool get(); void set(bool value); }
	virtual property bool DisableHistory { bool get(); void set(bool value); }
	virtual property bool EnableSwitch { bool get(); void set(bool value); }
	virtual property bool IsModal { bool get(); void set(bool value); }
	virtual property bool IsOpened { bool get(); }
	virtual property Place Window { Place get(); void set(Place value); }
	virtual property String^ FileName { String^ get(); void set(String^ value); }
	virtual property String^ Title { String^ get(); void set(String^ value); }
public:
	virtual void Open();
	virtual void Open(OpenMode mode);
internal:
	Viewer();
	void GetParams();
private:
	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
	void AssertClosed();
private:
	int _id;
	long _flags;
	Place _window;
	String^ _fileName;
	String^ _title;
};
}
