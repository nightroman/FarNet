
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

#include "stdafx.h"
#include "Far2.h"
#include "DialogLine.h"
#include "Editor.h"
#include "EditorLine.h"
#include "Far0.h"
#include "Panel2.h"

namespace FarNet
{
void Far2::RegisterProxyCommand(IModuleCommand^ info)
{
	Far0::RegisterProxyCommand(info);
}

void Far2::RegisterProxyDrawer(IModuleDrawer^ info)
{
	Far0::RegisterProxyDrawer(info);
}

void Far2::RegisterProxyEditor(IModuleEditor^ info)
{
	Far0::RegisterProxyEditor(info);
}

void Far2::RegisterProxyTool(IModuleTool^ info)
{
	Far0::RegisterProxyTool(info);
}

void Far2::UnregisterProxyAction(IModuleAction^ action)
{
	Far0::UnregisterProxyAction(action);
}

void Far2::UnregisterProxyTool(IModuleTool^ tool)
{
	Far0::UnregisterProxyTool(tool);
}

void Far2::InvalidateProxyCommand()
{
	Far0::InvalidateProxyCommand();
}

FarNet::Works::IPanelWorks^ Far2::CreatePanel(Panel^ panel, Explorer^ explorer)
{
	return gcnew Panel2(panel, explorer);
}

Task^ Far2::WaitSteps()
{
	return Far0::WaitSteps();
}

WaitHandle^ Far2::PostMacroWait(String^ macro)
{
	return Far0::PostMacroWait(macro);
}

ValueTuple<IntPtr, int> Far2::IEditorLineText(IntPtr id, int line)
{
	return Editor::GetLineText((intptr_t)id, line);
}

void Far2::IEditorLineText(IntPtr id, int line, IntPtr p, int n)
{
	return Editor::SetLineText((intptr_t)id, line, (const wchar_t*)(intptr_t)p, n);
}

ValueTuple<IntPtr, int> Far2::ILineText(ILine^ line)
{
	if (line->WindowKind == WindowKind::Editor)
	{
		auto self = static_cast<EditorLine^>(line);
		return Editor::GetLineText(self->_EditorId, self->_Index);
	}
	else
	{
		auto self = static_cast<DialogLine^>(line);
		return self->GetText();
	}
}

void Far2::ILineText(ILine^ line, IntPtr p, int n)
{
	if (line->WindowKind == WindowKind::Editor)
	{
		auto self = static_cast<EditorLine^>(line);
		Editor::SetLineText(self->_EditorId, self->_Index, (const wchar_t*)(intptr_t)p, n);
	}
	else
	{
		auto self = static_cast<DialogLine^>(line);
		self->SetText((wchar_t*)(intptr_t)p, n);
	}
}
}
