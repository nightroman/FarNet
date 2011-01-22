/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once
#include "Panel1.h"
#include "PanelInfo.h"

namespace FarNet
{;
ref class ShelveInfoPanel;
ref class ShelveInfoPlugin;

ref class Panel2 : public Panel1, IPanel
{
public: // Panel1
	virtual property bool IsPlugin { bool get() override; }
	virtual property Guid TypeId { Guid get(); void set(Guid value); }
	virtual property FarFile^ CurrentFile { FarFile^ get() override; }
	virtual property IList<FarFile^>^ ShownFiles { IList<FarFile^>^ get() override; }
	virtual property IList<FarFile^>^ SelectedFiles { IList<FarFile^>^ get() override; }
	virtual property String^ Path { String^ get() override; void set(String^ value) override; }
	virtual property String^ ActivePath { String^ get(); }
public: // IPanel
	virtual property bool AddDots;
	virtual property bool IdleUpdate;
	virtual property bool IsOpened { bool get(); }
	virtual property bool IsPushed { bool get() { return _Pushed != nullptr; } }
	virtual property Getter^ DataId;
	virtual property IList<FarFile^>^ Files { IList<FarFile^>^ get(); void set(IList<FarFile^>^ value); }
	virtual property IPanel^ AnotherPanel { IPanel^ get(); }
	virtual property IPanelInfo^ Info { IPanelInfo^ get() { return %_info; } }
	virtual property Object^ Host;
	virtual property String^ DotsDescription;
	virtual property System::Collections::Hashtable^ Data { System::Collections::Hashtable^ get(); }
	virtual void Close() override;
	virtual void Open();
	virtual void Open(IPanel^ oldPanel);
	virtual void PostData(Object^ data) { _postData = data; }
	virtual void PostFile(FarFile^ file) { _postFile = file; }
	virtual void PostName(String^ name) { _postName = name; }
	virtual void Push() override;
public: DEF_EVENT(Closed, _Closed);
public: DEF_EVENT(CtrlBreakPressed, _CtrlBreakPressed);
public: DEF_EVENT(GettingInfo, _GettingInfo);
public: DEF_EVENT(GotFocus, _GotFocus);
public: DEF_EVENT(Idled, _Idled);
public: DEF_EVENT(LosingFocus, _LosingFocus);
public: DEF_EVENT_ARGS(Closing, _Closing, PanelEventArgs);
public: DEF_EVENT_ARGS(DeletingFiles, _DeletingFiles, FilesEventArgs);
public: DEF_EVENT_ARGS(Escaping, _Escaping, PanelEventArgs);
public: DEF_EVENT_ARGS(Executing, _Executing, ExecutingEventArgs);
public: DEF_EVENT_ARGS(GettingData, _GettingData, PanelEventArgs);
public: DEF_EVENT_ARGS(GettingFiles, _GettingFiles, GettingFilesEventArgs);
public: DEF_EVENT_ARGS(KeyPressed, _KeyPressed, PanelKeyEventArgs);
public: DEF_EVENT_ARGS(KeyPressing, _KeyPressing, PanelKeyEventArgs);
public: DEF_EVENT_ARGS(MakingDirectory, _MakingDirectory, MakingDirectoryEventArgs);
public: DEF_EVENT_ARGS(PuttingFiles, _PuttingFiles, PuttingFilesEventArgs);
public: DEF_EVENT_ARGS(Redrawing, _Redrawing, PanelEventArgs);
public: DEF_EVENT_ARGS(SettingDirectory, _SettingDirectory, SettingDirectoryEventArgs);
public: DEF_EVENT_ARGS(ViewModeChanged, _ViewModeChanged, ViewModeChangedEventArgs);
internal:
	Panel2();
	void AssertOpen();
	void SwitchFullScreen();
	virtual FarFile^ GetFile(int index, FileType type) override;
internal:
	ShelveInfoPlugin^ _Pushed;
	bool _skipGettingData;
	bool _voidGettingData;
	FarPanelInfo _info;
	Object^ _postData;
	FarFile^ _postFile;
	String^ _postName;
	array<int>^ _postSelected;
	ShelveInfoPanel^ _ActiveInfo;
private:
	Guid _TypeId;
	IList<FarFile^>^ _files;
	System::Collections::Hashtable^ _Data;
};
}
