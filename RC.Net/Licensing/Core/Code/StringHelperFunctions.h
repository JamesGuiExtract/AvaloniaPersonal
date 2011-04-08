#pragma once

#include <vcclr.h>
#include <string>

#include <msclr\marshal_cppstd.h>

using namespace System;

namespace Extract
{
	namespace Licensing
	{
		private ref class StringHelpers sealed
		{
		public:
			//--------------------------------------------------------------------------------------
			// Internal functions
			//--------------------------------------------------------------------------------------
			// These marshal functions are based on the list from:
			// http://msdn.microsoft.com/en-us/library/bb384865.aspx
			//
			// PURPOSE: To Convert .net string to stl string
			// NOTE:	This method is used internally and is placed here so the header file can
			//			be used in projects not compiled with the /clr switch
			static std::string AsSTLString(String^ netString)
			{
				return msclr::interop::marshal_as<std::string>(netString);
			}
			//--------------------------------------------------------------------------------------
			static String^ AsSystemString(std::string stdString)
			{
				return msclr::interop::marshal_as<String^>(stdString);
			}

		private:
            //--------------------------------------------------------------------------------------
			// PURPOSE:	Added to remove FxCop warning
			//			Microsoft.Performance::CA1812 - Declare a private constructor for
			//			static internal classes
			StringHelpers(void) {};
            //--------------------------------------------------------------------------------------

		};
	};
};