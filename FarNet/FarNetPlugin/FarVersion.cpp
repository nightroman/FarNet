#include "StdAfx.h"
#include "FarVersion.h"

namespace FarManagerImpl
{;
FarVersion::FarVersion(int major, int minor, int build)
: _major(major), _minor(minor), _build(build)
{
}

System::String^ FarVersion::ToString()
{
	return String::Empty + _major + "." + _minor + "." + _build;
}

int FarVersion::Major::get()
{
	return _major;
}

int FarVersion::Minor::get()
{
	return _minor;
}

int FarVersion::Build::get()
{
	return _build;
}
}
