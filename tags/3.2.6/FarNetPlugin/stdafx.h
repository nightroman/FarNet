#pragma once

#define WIN32_LEAN_AND_MEAN	// Exclude rarely-used stuff from Windows headers
#define NOTEXTMETRIC // fixes pack 2 linking problem

#pragma warning(push,3)
#include <vcclr.h>
#include "plugin.hpp"
#pragma warning(pop)

using namespace FarManager::Impl;
using namespace FarManager;
using namespace System::Collections::Specialized;
using namespace System::Collections::Generic;
using namespace System;

extern PluginStartupInfo Info;
