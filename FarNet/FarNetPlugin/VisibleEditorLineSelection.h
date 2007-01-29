#pragma once

namespace FarManagerImpl
{;
public ref class VisibleEditorLineSelection : public ILineSelection
{
public:
	virtual property String^ Text { String^ get(); void set(String^ value); }
	virtual property int End { int get(); }
	virtual property int Length { int get(); }
	virtual property int Start { int get(); }
	virtual String^ ToString() override;
internal:
	VisibleEditorLineSelection(int no);
private:
	int _no;
};
}
