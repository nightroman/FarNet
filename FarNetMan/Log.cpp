/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
*/

#include "StdAfx.h"
#include "Log.h"

#undef Debug
#undef Trace

namespace FarNet
{;
#define START() //???? if (!IsStarted) { IsStarted = true; Trace::WriteLine(String::Format("\n{0:yyyy-MM-dd HH:mm:ss} FarNet\n", DateTime::Now)); }

void Log::TraceError(Exception^ error)
{
	if (!error || !Switch->TraceError)
		return;

	START();

	String^ type = error->GetType()->FullName;
	int i = type->LastIndexOf('.');
	
	// critical error - trace error
	if (i >= 0 && type->Substring(0, i) == "System")
	{
		Trace::TraceError(error->ToString());
	}
	// other error - trace warning
	else if (Switch->TraceWarning)
	{
		Trace::TraceWarning(error->ToString());
	}
}

void Log::WriteLine(String^ info)
{
	START();

	Trace::WriteLine(info);
}

String^ Log::Format(MethodInfo^ method)
{
	assert(method != nullptr);
	return method->ReflectedType->FullName + "." + method->Name;
}

}
