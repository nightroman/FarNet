/*
FAR.NET plugin for Far Manager
Copyright (c) 2005-2009 FAR.NET Team
*/

#pragma once
class CStr;

namespace FarNet
{;
//???
ref class Message
{
internal:
	static int Show(String^ body, String^ header, MessageOptions options, array<String^>^ buttons, String^ helpTopic);
private:
	bool Show();
	int Amount();
	CStr* CreateBlock();
	static void Add(StringCollection^ strings, CStr* result, int& index);
	static int ShowDialog(Message^ msg, array<String^>^ buttons, int width);
	static void FormatMessageLines(StringCollection^ lines, String^ message, int width, int height);
private:
	int _flags;
	int _selected;
	String^ _header;
	String^ _helpTopic;
	StringCollection _body;
	StringCollection _buttons;
};
}
