
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2015 Roman Kuzmin
*/

#pragma once

namespace FarNet
{;
ref class InputBox : public IInputBox
{
public:
	virtual bool Show();
	virtual property bool ButtonsAreVisible;
	virtual property bool EmptyEnabled;
	virtual property bool ExpandEnvironmentVariables;
	virtual property bool IsPath;
	virtual property bool IsPassword;
	virtual property bool UseLastHistory;
	virtual property int MaxLength { int get(); void set(int value); }
	virtual property String^ HelpTopic { String^ get(); void set(String^ value); }
	virtual property String^ History;
	virtual property String^ Prompt;
	virtual property String^ Text;
	virtual property String^ Title;
internal:
	InputBox();
private:
	int Flags();
private:
	int _maxLength;
	String^ _HelpTopic;
};
}
