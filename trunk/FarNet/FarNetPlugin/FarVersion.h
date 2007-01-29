#pragma once

namespace FarManagerImpl
{;
public ref class FarVersion : public IVersion
{
public:
	virtual String^ ToString() override;
	virtual property int Major { int get(); }
	virtual property int Minor { int get(); }
	virtual property int Build { int get(); }
internal:
	FarVersion(int major, int minor, int build);
private:
	int _major, _minor, _build;
};
}
