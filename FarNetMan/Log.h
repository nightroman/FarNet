/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
*/

#pragma once

#ifdef _DEBUG
#define assert(e) Debug::Assert(e);
#else
#define assert(e)
#endif

namespace FarNet
{;
ref class Log
{
public:
	static property int IndentLevel { int get() { return Trace::IndentLevel; } void set(int value) { Trace::IndentLevel = value; } }
	static void TraceError(Exception^ error);
	static void TraceWarning(String^ info) { Trace::TraceWarning(info); }
	static void WriteLine(String^ info);
	static String^ Format(MethodInfo^ method);
public:
	static TraceSwitch^ Switch = gcnew TraceSwitch("FarNet.Trace", "FarNet trace switch.");
private:
	static bool IsStarted;
};
};

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
		assert(IndentLevel == -1); // should be called once at most
		assert(level >= 0 && level <= 4);

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
#define LOG_INFO(INFO) if (Log::Switch->TraceInfo) Log::WriteLine((INFO));
