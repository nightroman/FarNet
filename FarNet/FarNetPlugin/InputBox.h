/*
FAR.NET plugin for Far Manager
Copyright (c) 2005-2008 FAR.NET Team
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
	virtual property bool EnvExpanded;
	virtual property bool IsPassword;
	virtual property bool NoLastHistory;
	virtual property int MaxLength { int get(); void set(int value); }
	virtual property String^ HelpTopic { String^ get(); void set(String^ value); }
	virtual property String^ History;
	virtual property String^ Prompt;
	virtual property String^ Text;
	virtual property String^ Title;
internal:
	InputBox();
internal:
	const char* _oemHelpTopic;
private:
	int Flags();
private:
	int _maxLength;
	String^ _HelpTopic;
};
}
