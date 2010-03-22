/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once

#ifdef _DEBUG
#define assert(e) Log::Assert(e)
#else
#define assert(e)
#endif

#define Debug stop_Debug
#define Trace stop_Trace

/*
- Do use 'if <level>' before any other expressions to avoid evaluation for nothing.
*/
#define LOG_3(VALUE) if (Log::Switch->TraceInfo) Log::WriteLine((VALUE));
#define LOG_4(VALUE) if (Log::Switch->TraceVerbose) Log::WriteLine((VALUE));
#define LOG_AUTO(LEVEL, INFO) Log^ log = Log::Switch->Trace##LEVEL ? gcnew Log(INFO) : nullptr; try
#define LOG_END finally { delete log; }
