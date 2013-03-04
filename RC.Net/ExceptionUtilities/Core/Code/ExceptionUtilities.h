// ExceptionUtilities.h

#pragma once

using namespace System;
using namespace System::Diagnostics::CodeAnalysis;

// A CLR C++ assembly was needed to wrap a call to sGetDebugValue 
[module: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes",
						 Scope="namespace", Target="Extract.ExceptionUtilities")];
namespace Extract
{
	namespace ExceptionUtilities 
	{
		// Class for static Methods for ExceptionData
		public ref class ExceptionData abstract sealed
		{
		public:
			// Method to call the UCLIDException::sGetDebugValue method from .NET
			static String ^ GetDebugValue( String ^ data ); 
		};
	}
}