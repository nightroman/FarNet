
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
	static int Show(MessageArgs^ args);
	static int ShowGui(String^ body, String^ header, MessageOptions options);
private:
	bool Show();
	int ShowDialog(int maxTextWidth, bool needButtonList);
	CStr* CreateBlock(int& outNbItems);
	static bool NeedButtonList(array<String^>^ buttons, int width);
private:
	int _flags;
	int _selected;
	String^ _header;
	String^ _helpTopic;
	List<String^> _body;
	array<String^>^ _buttons;
	Nullable<Point> _position;
};
}
