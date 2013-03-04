// This is the main DLL file.

#include "stdafx.h"
#include <string>
#include <UCLIDException.h>
#include "ExceptionUtilities.h"


using namespace std;
using namespace Extract;
using namespace Extract::ExceptionUtilities;
using namespace System::Runtime::InteropServices;
using namespace System::Runtime::CompilerServices;

#pragma managed

[assembly:RuntimeCompatibilityAttribute(WrapNonExceptionThrows = true)]; 

String ^ ExceptionData::GetDebugValue( String ^ data )
{
	try
	{
		// Convert the .NET string to stl string
		const char* chars = (const char*)(Marshal::StringToHGlobalAnsi(data)).ToPointer();
		string strData = chars;
		Marshal::FreeHGlobal(IntPtr((void*)chars));
	
		// Call method that will decrypt encrypted data if allowed
		strData = UCLIDException::sGetDataValue(strData);

		// Convert the stl string to .NET string
		String^ rtnValue = gcnew String(strData.c_str());
		return rtnValue;
	}
	catch(Exception^ ex)
	{
		throw ExtractException::AsExtractException("ELI35416", ex);
	}
}

