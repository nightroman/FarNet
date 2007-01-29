#pragma once

namespace FarManagerImpl
{;
public ref class InputBox : public IInputBox
{
public:
	virtual bool Show();
	virtual bool Show(String^ prompt, String^ text, String^ title, String^ history);
	virtual property bool ButtonsAreVisible { bool get(); void set(bool value); }
	virtual property bool EmptyEnabled { bool get(); void set(bool value); }
	virtual property bool EnvExpanded { bool get(); void set(bool value); }
	virtual property bool IsPassword { bool get(); void set(bool value); }
	virtual property bool UseLastHistory { bool get(); void set(bool value); }
	virtual property int MaxLength { int get(); void set(int value); }
	virtual property String^ History { String^ get(); void set(String^ value); }
	virtual property String^ Prompt { String^ get(); void set(String^ value); }
	virtual property String^ Text { String^ get(); void set(String^ value); }
	virtual property String^ Title { String^ get(); void set(String^ value); }
internal:
	InputBox();
private:
	int Flags();
private:
	String ^_text, ^_title, ^_prompt, ^_history;
	bool _emptyEnabled;
	bool _isPassword;
	bool _envExpanded;
	bool _useLastHistory;
	bool _buttonsAreVisible;
	int _maxLength;
};
}
