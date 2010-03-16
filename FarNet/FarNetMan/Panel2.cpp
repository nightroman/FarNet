/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#include "StdAfx.h"
#include "Panel2.h"
#include "Panel0.h"
#include "Shelve.h"

namespace FarNet
{;
Panel2::Panel2()
: Panel1(true)
, _files(gcnew List<FarFile^>)
, _ActiveInfo(ShelveInfoPanel::CreateActiveInfo(false))
{}

void Panel2::AssertOpen()
{
	if (Index <= 0)
		throw gcnew InvalidOperationException("Expected opened module panel.");
}

/*
?? It works only for panels that have the current mode defined,
because Far does not provide this info and we do not want to hack
Far:\Panel\ViewModes\ModeX, though it should work, more likely.
For now we just do nothing for not defined modes.
To submit a wish?
*/
void Panel2::SwitchFullScreen()
{
	// get
	PanelViewMode iViewMode = ViewMode;
	PanelModeInfo^ mode = Info->GetMode(iViewMode);
	if (!mode)
	{
		String^ sColumnTypes;
		{
			int size = ::Info.Control(Handle, FCTL_GETCOLUMNTYPES, 0, NULL);
			CBox buf(size);
			::Info.Control(Handle, FCTL_GETCOLUMNTYPES, size, (LONG_PTR)(wchar_t*)buf);
			sColumnTypes = gcnew String(buf);
		}
		String^ sColumnWidths;
		{
			int size = ::Info.Control(Handle, FCTL_GETCOLUMNWIDTHS, 0, NULL);
			CBox buf(size);
			::Info.Control(Handle, FCTL_GETCOLUMNWIDTHS, size, (LONG_PTR)(wchar_t*)buf);
			sColumnWidths = gcnew String(buf);
		}

		array<String^>^ types = sColumnTypes->Split(',');
		array<String^>^ widths = sColumnWidths->Split(',');
		if (types->Length != widths->Length)
			throw gcnew InvalidOperationException("Different numbers of column types and widths.");

		mode = gcnew PanelModeInfo;
		mode->Columns = gcnew array<FarColumn^>(types->Length);
		for(int iType = 0; iType < types->Length; ++iType)
		{
			SetColumn^ column = gcnew SetColumn();
			mode->Columns[iType] = column;
			column->Kind = types[iType];
			
			if (widths[iType]->EndsWith("%"))
				column->Width = - ParseInt(widths[iType]->Substring(0, widths[iType]->Length - 1), 0);
			else if (types[iType] == "N" || types[iType] == "Z" || types[iType] == "O")
				column->Width = 0;
			else
				column->Width = ParseInt(widths[iType], 0);
		}
	}

	// switch
	mode->IsFullScreen = !mode->IsFullScreen;

	// set
	Info->SetMode(iViewMode, mode);
	Redraw();
}

bool Panel2::IsOpened::get()
{
	return Index > 0;
}

IList<FarFile^>^ Panel2::Files::get()
{
	return _files;
}

void Panel2::Files::set(IList<FarFile^>^ value)
{
	if (value == nullptr)
		throw gcnew ArgumentNullException("value");

	_files = value;
}

bool Panel2::IsPlugin::get()
{
	return true;
}

Guid Panel2::TypeId::get()
{
	return _TypeId;
}

void Panel2::TypeId::set(Guid value)
{
	if (_TypeId != Guid::Empty)
		throw gcnew InvalidOperationException("TypeId must not change.");

	_TypeId = value;
}

//! see remark for Panel1::CurrentFile::get()
FarFile^ Panel2::CurrentFile::get()
{
	AssertOpen();

	PanelInfo pi;
	GetPanelInfo(Handle, pi);

	if (pi.ItemsNumber == 0)
		return nullptr;

	AutoPluginPanelItem item(Handle, pi.CurrentItem, ShownFile);
	int fi = (int)(INT_PTR)item.Get().UserData;
	if (fi < 0)
		return nullptr;

	// 090411 Extra sanity test and watch.
	// See State::GetPanelInfo - this approach fixes the problem, but let's watch for a while.
	if (fi >= _files->Count)
	{
		assert(0);
		return nullptr;
	}

	return _files[fi];
}

IList<FarFile^>^ Panel2::ShownFiles::get()
{
	AssertOpen();

	PanelInfo pi;
	GetPanelInfo(Handle, pi);

	List<FarFile^>^ r = gcnew List<FarFile^>(pi.ItemsNumber);
	for(int i = 0; i < pi.ItemsNumber; ++i)
	{
		AutoPluginPanelItem item(Handle, i, ShownFile);
		int fi = (int)(INT_PTR)item.Get().UserData;
		if (fi >= 0)
			r->Add(_files[fi]);
	}

	return r;
}

IList<FarFile^>^ Panel2::SelectedFiles::get()
{
	AssertOpen();

	PanelInfo pi;
	GetPanelInfo(Handle, pi);

	List<FarFile^>^ r = gcnew List<FarFile^>(pi.SelectedItemsNumber);
	for(int i = 0; i < pi.SelectedItemsNumber; ++i)
	{
		AutoPluginPanelItem item(Handle, i, SelectedFile);
		int fi = (int)(INT_PTR)item.Get().UserData;
		if (fi >= 0)
			r->Add(_files[fi]);
	}

	return r;
}

FarFile^ Panel2::GetFile(int index, FileType type)
{
	AutoPluginPanelItem item(Handle, index, type);
	int fi = (int)(INT_PTR)item.Get().UserData;
	if (fi >= 0)
		// plugin file
		return _files[fi];
	else
		// 090823 dots, not null
		return ItemToFile(item.Get());
}

String^ Panel2::Path::get()
{
	return _info.CurrentDirectory;
}

void Panel2::Path::set(String^ value)
{
	if (value == nullptr)
		throw gcnew ArgumentNullException("value");

	if (!_SettingDirectory)
	{
		// _090929_061740 Directory::Exists gets false for long paths
		if (value->Length < 260 && !Directory::Exists(value))
			throw gcnew ArgumentException("Directory '" + value + "' does not exist.");
		
		Close(value);
		return;
	}

	SettingDirectoryEventArgs e(value, OperationModes::None);
	_SettingDirectory(this, %e);
	if (!e.Ignore)
	{
		Update(false);
		Redraw();
	}
}

String^ Panel2::ActivePath::get()
{
	return _ActiveInfo ? _ActiveInfo->Path : String::Empty;
}

IPanel^ Panel2::AnotherPanel::get()
{
	return Panel0::GetPanel2(this);
}

void Panel2::Open(IPanel^ oldPanel)
{
	if (!oldPanel)
		throw gcnew ArgumentNullException("oldPanel");

	Panel0::ReplacePluginPanel((Panel2^)oldPanel, this);
}

void Panel2::Open()
{
	if (Index > 0)
		throw gcnew InvalidOperationException("Cannot open the panel because it is already opened.");

	Panel0::OpenPluginPanel(this);
	if (_Pushed)
	{
		Works::ShelveInfo::Stack->Remove(_Pushed);
		_skipGettingData = true;
		_Pushed = nullptr;
	}
}

void Panel2::Push()
{
	Panel0::PushPluginPanel(this);
}

// close and restore the shelved
void Panel2::Close()
{
	LOG_AUTO(Info, __FUNCTION__)
	{
		if (_ActiveInfo)
			_ActiveInfo->Pop();
		else
			Panel1::Close();
	}
	LOG_END;
}

}
