/*
FAR.NET plugin for Far Manager
Copyright (c) 2005-2009 FAR.NET Team
*/

#pragma once
class CStr;

namespace FarNet
{;
ref class Message : public IMessage
{
public:
	virtual bool Show();
	virtual property bool IsError { bool get(); void set(bool value); }
	virtual property bool IsWarning { bool get(); void set(bool value); }
	virtual property bool KeepBackground { bool get(); void set(bool value); }
	virtual property bool LeftAligned { bool get(); void set(bool value); }
	virtual property int Selected { int get(); void set(int value); }
	virtual property MessageOptions Options { MessageOptions get(); void set(MessageOptions value); }
	virtual property String^ Header;
	virtual property String^ HelpTopic;
	virtual property StringCollection^ Body { StringCollection^ get(); }
	virtual property StringCollection^ Buttons { StringCollection^ get(); }
internal:
	Message();
	static int Show(String^ body, String^ header, MessageOptions options, array<String^>^ buttons, String^ helpTopic);
private:
	int Amount();
	CStr* CreateBlock();
	static void Add(StringCollection^ strings, CStr* result, int& index);
	static int ShowDialog(Message^ msg, array<String^>^ buttons, int width);
	static void FormatMessageLines(StringCollection^ lines, String^ message, int width, int height);
private:
	int _flags;
	int _selected;
	StringCollection^ _body;
	StringCollection^ _buttons;
};
}
