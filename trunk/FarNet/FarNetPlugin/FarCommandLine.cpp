#include "StdAfx.h"
#include "FarCommandLine.h"
#include "Utils.h"

namespace FarManagerImpl
{;
FarCommandLine::FarCommandLine()
{
}

String^ FarCommandLine::Text::get()
{
	char sCmd[1024];
	if (!Info.Control(INVALID_HANDLE_VALUE, FCTL_GETCMDLINE, sCmd))
		throw gcnew OperationCanceledException();
	return OemToStr(sCmd);
}

void FarCommandLine::Text::set(String^ value)
{
	CStr sCmd(value);
	if (!Info.Control(INVALID_HANDLE_VALUE, FCTL_SETCMDLINE, sCmd))
		throw gcnew OperationCanceledException();
}

void FarCommandLine::Insert(String^ text)
{
	CStr sText(text);
	if (!Info.Control(INVALID_HANDLE_VALUE, FCTL_INSERTCMDLINE, sText))
		throw gcnew OperationCanceledException();
}

String^ FarCommandLine::ToString()
{
	return Text;
}
}
