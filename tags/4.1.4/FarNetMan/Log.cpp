/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
*/

#include "StdAfx.h"
#include "Log.h"
#include "Utils.h"

#undef Debug
#undef Trace

namespace FarNet
{;
#ifdef _DEBUG
void Log::Assert(bool expression)
{
	Debug::Assert(expression);
}
#endif

// return: null if not written or formatted error info
String^ Log::TraceException(Exception^ error)
{
	// no job?
	if (!error || !Switch->TraceError)
		return nullptr;

	// find the last dot
	String^ type = error->GetType()->FullName;
	int i = type->LastIndexOf('.');
	
	// system error: trace as error
	String^ r = nullptr;
	if (i >= 0 && type->Substring(0, i) == "System")
	{
		r = FormatException(error);
		Trace::TraceError(r);
	}
	// other error: trace as warning
	else if (Switch->TraceWarning)
	{
		r = FormatException(error);
		Trace::TraceWarning(r);
	}

	return r;
}

void Log::TraceError(String^ error)
{
	Trace::TraceError(error);
}

void Log::WriteLine(String^ info)
{
	Trace::WriteLine(info);
}

String^ Log::Format(MethodInfo^ method)
{
	assert(method != nullptr);
	return method->ReflectedType->FullName + "." + method->Name;
}

String^ Log::FormatException(Exception^ e)
{
	//?? Regex is used to fix bad PS V1 strings; check V2
	Regex re("[\r\n]+");
	String^ info = re.Replace(e->Message, "\r\n") + "\r\n";

	// search for an error record recursively
	Object^ errorRecord = nullptr;
	for(Exception^ ex = e; ex != nullptr; ex = ex->InnerException)
	{
		if (ex->GetType()->FullName->StartsWith("System.Management.Automation."))
		{
			errorRecord = Property(ex, "ErrorRecord");
			break;
		}
	}

	// enough?
	if (!errorRecord)
		return info;

	// process the error record
	Object^ ii = Property(errorRecord, "InvocationInfo");
	if (ii != nullptr)
	{
		Object^ pm = Property(ii, "PositionMessage");
		if (pm != nullptr)
			//?? 090517 Added Trim(), because a position message starts with an empty line
			info += re.Replace(pm->ToString()->Trim(), "\r\n") + "\r\n";
	}

	return info;
}

}
