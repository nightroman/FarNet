/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

#pragma once

#ifdef _DEBUG
#define assert(e) Log::Assert(e);
#else
#define assert(e)
#endif

namespace FarNet
{;
ref class Log
{
public:
	static property int IndentLevel { int get() { return Trace::IndentLevel; } void set(int value) { Trace::IndentLevel = value; } }
	static String^ TraceException(Exception^ error);
	static void TraceError(String^ error);
	static void TraceWarning(String^ info) { Trace::TraceWarning(info); }
	static void WriteLine(String^ info);
	static String^ Format(MethodInfo^ method);
	static String^ FormatException(Exception^ e);
#ifdef _DEBUG
	static void Assert(bool expression);
#endif
public:
	static TraceSwitch^ Switch = gcnew TraceSwitch("FarNet.Trace", "FarNet trace switch.");
};
};

#define Debug stop_Debug
#define Trace stop_Trace

/*
- Do not pass any data into constructor to avoid wasted evaluation.
- Use IndentLevel, not Indent/Unindent - to ensure the original level restored.
*/
class LogAuto
{
public:
	// Only makes a dummy
	LogAuto() : IndentLevel(-1)
	{}
	// Begin log
	void Log(String^ info)
	{
		// begin
		IndentLevel = Log::IndentLevel;

		// trace
		Log::WriteLine(info + " {");
		
		// indent
		Log::IndentLevel = IndentLevel + 1;
	}
	// End log
	~LogAuto()
	{
		// bedan?
		if (IndentLevel >= 0)
		{
			// unindent
			Log::IndentLevel = IndentLevel;

			// trace end
			Log::WriteLine("}");
		}
	}
private:
	int IndentLevel;
};

/*
- Do use 'if <level>' before any other expressions to avoid evaluation for nothing.
*/
#define LOG_AUTO(LEVEL, INFO) LogAuto logAuto; if ((LEVEL) <= (int)Log::Switch->Level) logAuto.Log((INFO));
#define LOG_3(VALUE) if (Log::Switch->TraceInfo) Log::WriteLine((VALUE));
#define LOG_4(VALUE) if (Log::Switch->TraceVerbose) Log::WriteLine((VALUE));
