#pragma once

#define WIN32_LEAN_AND_MEAN	// Exclude rarely-used stuff from Windows headers
#define NOTEXTMETRIC // Fix pack 2 linking problem

#pragma warning(push,3)
#include <vcclr.h>
#include "plugin.hpp"
#include "farcolor.hpp"
#include "farkeys.hpp"
#pragma warning(pop)

#define ACTL_GETWINDOWINFO stop_ACTL_GETWINDOWINFO
#define DM_ENABLEREDRAW stop_DM_ENABLEREDRAW
#define DM_GETDLGITEM stop_DM_GETDLGITEM
#define DM_SETDLGITEM stop_DM_SETDLGITEM
#define ECTL_GETINFO stop_ECTL_GETINFO
#define FCTL_GETCURRENTPANELITEM stop_FCTL_GETCURRENTPANELITEM
#define FCTL_GETPANELINFO stop_FCTL_GETPANELINFO
#define FCTL_GETPANELITEM stop_FCTL_GETPANELITEM
#define FCTL_GETSELECTEDPANELITEM stop_FCTL_GETSELECTEDPANELITEM

using namespace FarNet::Forms;
using namespace FarNet::Works;
using namespace FarNet;
using namespace System::Collections::Generic;
using namespace System::Collections::ObjectModel;
using namespace System::Collections::Specialized;
using namespace System::Configuration;
using namespace System::Diagnostics;
using namespace System::Globalization;
using namespace System::IO;
using namespace System::Reflection;
using namespace System::Resources;
using namespace System::Text::RegularExpressions;
using namespace System::Text;
using namespace System;

#ifdef _DEBUG
// remove if not found
#include <Test1.h>
#endif

extern PluginStartupInfo Info;

#include "Log.h"
#include "Utils.h"
