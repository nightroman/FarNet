
#pragma once
#include <memory>
#include <vector>
#include <vcclr.h>
#include "plugin.hpp"

// Deny .NET
#define Console stop_Console

// Deny Far
#define ACTL_GETWINDOWINFO stop_ACTL_GETWINDOWINFO
#define DM_GETDLGITEM stop_DM_GETDLGITEM
#define DM_SETDLGITEM stop_DM_SETDLGITEM
#define ECTL_GETINFO stop_ECTL_GETINFO
#define FCTL_GETCMDLINESELECTEDTEXT stop_FCTL_GETCMDLINESELECTEDTEXT // Far bug: gets 1 char less; there is another way
#define FCTL_GETCURRENTPANELITEM stop_FCTL_GETCURRENTPANELITEM
#define FCTL_GETPANELINFO stop_FCTL_GETPANELINFO
#define FCTL_GETPANELITEM stop_FCTL_GETPANELITEM
#define FCTL_GETSELECTEDPANELITEM stop_FCTL_GETSELECTEDPANELITEM
#define FCTL_SETPANELDIRECTORY stop_FCTL_SETPANELDIRECTORY
#define FCTL_GETUSERSCREEN stop_FCTL_GETUSERSCREEN
#define FCTL_SETUSERSCREEN stop_FCTL_SETUSERSCREEN

using namespace FarNet::Forms;
using namespace FarNet;
using namespace System::Collections::Generic;
using namespace System::Collections::ObjectModel;
using namespace System::Collections::Specialized;
using namespace System::Diagnostics;
using namespace System::Globalization;
using namespace System::IO;
using namespace System::Reflection;
using namespace System::Resources;
using namespace System::Text::RegularExpressions;
using namespace System::Text;
using namespace System::Threading;
using namespace System::Threading::Tasks;
using namespace System;

#undef CreateDialog
#undef GetEnvironmentVariable

#ifdef _DEBUG
// remove if not found
#include <Test1.h>
#endif

extern PluginStartupInfo Info;
extern const GUID MainGuid;
extern const GUID ColorerGuid;

#include "Log.h"
#include "Utils.h"
