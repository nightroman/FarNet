
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

#pragma once

namespace FarNet
{
// Works::Far2::Api instance.
ref class Far2 : Works::Far2
{
public:
	virtual void RegisterProxyCommand(IModuleCommand^ info) override;
	virtual void RegisterProxyDrawer(IModuleDrawer^ info) override;
	virtual void RegisterProxyEditor(IModuleEditor^ info) override;
	virtual void RegisterProxyTool(IModuleTool^ info) override;
	virtual void UnregisterProxyAction(IModuleAction^ action) override;
	virtual void UnregisterProxyTool(IModuleTool^ tool) override;
	virtual void InvalidateProxyCommand() override;
public:
	virtual FarNet::Works::IPanelWorks^ CreatePanel(Panel^ panel, Explorer^ explorer) override;
	virtual Task^ WaitSteps() override;
	virtual WaitHandle^ PostMacroWait(String^ macro) override;
	virtual ValueTuple<IntPtr, int> IEditorLineText(IntPtr id, int line) override;
	virtual void IEditorLineText(IntPtr id, int line, IntPtr p, int n) override;
	virtual ValueTuple<IntPtr, int> ILineText(ILine^ line) override;
	virtual void ILineText(ILine^ line, IntPtr p, int n) override;
};
}
