#pragma once

namespace FarManagerImpl
{;
ref class FarCommandLine : ICommandLine
{
public:
	virtual property String^ Text { String^ get(); void set(String^ value); }
	virtual void Insert(String^ text);
public:
	virtual String^ ToString() override;
internal:
	FarCommandLine();
};
}
