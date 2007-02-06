#pragma once
class CStr;

namespace FarManagerImpl
{;
public ref class Message : public IMessage
{
public:
	virtual bool Show();
	virtual property bool IsError { bool get(); void set(bool value); }
	virtual property bool IsWarning { bool get(); void set(bool value); }
	virtual property bool KeepBackground { bool get(); void set(bool value); }
	virtual property bool LeftAligned { bool get(); void set(bool value); }
	virtual property int Selected { int get(); void set(int value); }
	virtual property String^ Header { String^ get(); void set(String^ value); }
	virtual property StringCollection^ Body { StringCollection^ get(); }
	virtual property StringCollection^ Buttons { StringCollection^ get(); }
	virtual void Reset();
internal:
	Message();
private:
	int Amount();
	int Flags();
	CStr* CreateBlock();
	static void Add(StringCollection^ strings, CStr* result, int& index);
private:
	bool _isError;
	bool _isWarning;
	bool _keepBackground;
	bool _leftAligned;
	int _selected;
	String^ _header;
	StringCollection^ _body;
	StringCollection^ _buttons;
};
}
