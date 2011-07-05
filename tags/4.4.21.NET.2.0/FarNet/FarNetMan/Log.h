
/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once

#ifdef _DEBUG
#define assert(e) Debug::Assert(e)
#else
#define assert(e)
#endif

#define Trace stop_Trace
