/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
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
	virtual IWindowInfo^ GetInfoAt(int index, bool full) override;
	virtual void SetCurrentAt(int index) override;
	virtual WindowKind GetKindAt(int index) override;
internal:
	static Window Instance;
private:
	Window() {}
};

}
