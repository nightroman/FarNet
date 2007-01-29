#pragma once

namespace FarManagerImpl
{;
public ref class VisibleEditorCursor : public ICursor
{
public:
	virtual property int LeftPos { int get(); void set(int value); }
	virtual property int Line { int get(); void set(int value); }
	virtual property int Pos { int get(); void set(int value); }
	virtual property int TabPos { int get(); void set(int value); }
	virtual property int TopLine { int get(); void set(int value); }
	virtual void Assign(ICursor^ cursor);
	virtual void Set(int pos, int line);
internal:
	VisibleEditorCursor();
private:
	static void PutEsp(const EditorSetPosition& esp);
};
}
