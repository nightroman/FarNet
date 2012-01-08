
/*
FarNet plugin for Far Manager
Copyright (c) 2005-2012 FarNet Team
*/

#pragma once

namespace FarNet
{;
ref class Window sealed : IWindow
{
public:
	virtual property int Count { int get() override; }
	virtual property WindowKind Kind { WindowKind get() override; }
public:
	virtual bool Commit() override;
	virtual void SetCurrentAt(int index) override;
	virtual WindowKind GetKindAt(int index) override;
	virtual String^ GetKindNameAt(int index) override;
	virtual String^ GetNameAt(int index) override;
internal:
	static Window Instance;
private:
	Window() {}
};

}
