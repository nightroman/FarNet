#pragma once

namespace FarNet
{;
ref class Window sealed : IWindow
{
public:
	virtual property bool IsModal { bool get() override; }
	virtual property int Count { int get() override; }
	virtual property WindowKind Kind { WindowKind get() override; }
public:
	virtual IFace^ GetAt(int index) override;
	virtual IntPtr GetIdAt(int index) override;
	virtual String^ GetNameAt(int index) override;
	virtual void SetCurrentAt(int index) override;
	virtual WindowKind GetKindAt(int index) override;
internal:
	static Window Instance;
private:
	Window() {}
};

}
