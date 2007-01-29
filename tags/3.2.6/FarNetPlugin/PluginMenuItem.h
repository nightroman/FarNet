#pragma once

namespace FarManagerImpl
{;
public ref class PluginMenuItem : public IPluginMenuItem
{
public:
	virtual event EventHandler<OpenPluginMenuItemEventArgs^>^ OnOpen;
	virtual property String^ Name { String^ get(); void set(String^ value); }
	virtual void FireOnOpen(IPluginMenuItem^ sender, OpenFrom from);
private:
	String^ _name;
};
}
