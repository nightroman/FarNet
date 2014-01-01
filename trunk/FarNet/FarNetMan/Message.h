
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2014 Roman Kuzmin
*/

#pragma once
class CStr;

namespace FarNet
{;
ref class Message
{
internal:
	static int Show(String^ body, String^ header, MessageOptions options, array<String^>^ buttons, String^ helpTopic);
	static int ShowGui(String^ body, String^ header, MessageOptions options);
private:
	bool Show();
	int ShowDialog(int width);
	CStr* CreateBlock(int& outNbItems);
private:
	int _flags;
	int _selected;
	String^ _header;
	String^ _helpTopic;
	List<String^> _body;
	array<String^>^ _buttons;
};
}
