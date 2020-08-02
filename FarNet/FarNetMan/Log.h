
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

#pragma once

#ifdef _DEBUG
#define assert(e) Debug::Assert(e)
#else
#define assert(e)
#endif

#define Trace stop_Trace
