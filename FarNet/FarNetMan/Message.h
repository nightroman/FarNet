
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

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
	std::vector<CStr> CreateBlock();
	static int GetButtonLineLength(array<String^>^ buttons);
private:
	MessageArgs^ _args;
	int _flags;
	int _selected;
	String^ _header;
	List<String^> _body;
	array<String^>^ _buttons;
	int _buttonLineLength;
};
}
