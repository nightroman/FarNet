/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once

namespace FarNet
{;
ref class EditorLineSelection : public ILineSelection
{
public:
	virtual property String^ Text { String^ get(); void set(String^ value); }
	virtual property int End { int get(); }
	virtual property int Length { int get(); }
	virtual property int Start { int get(); }
	virtual String^ ToString() override;
internal:
	EditorLineSelection(int no);
private:
	int _no;
};
}
