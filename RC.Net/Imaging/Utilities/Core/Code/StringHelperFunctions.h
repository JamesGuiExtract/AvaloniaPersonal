#pragma once

#include <vcclr.h>
#include <string>

namespace Extract
{
	namespace Imaging
	{
		namespace Utilities
		{
			using namespace System;

			private ref class StringHelpers sealed
			{
			public:
				// This function is based on code from Stan Lippman's Blog on MSDN:
				// http://blogs.msdn.com/slippman/archive/2004/06/02/147090.aspx
				static std::string AsSTLString(String^ netString)
				{
					char* zData = NULL;

					// Default the return string to empty
					std::string strSTL = "";

					if (!String::IsNullOrEmpty(netString))
					{
						try
						{
							// Compute the length needed to hold the converted string
							int length = ((netString->Length+1) * 2);

							// Declare a char array to hold the converted string
							zData = new char[length];
							ExtractException::Assert("ELI29719", "Failed array allocation!", zData != NULL);

							// Scope for pin_ptr
							int result;
							{
								// Get the pointer from the System::String
								pin_ptr<const wchar_t> wideData = PtrToStringChars(netString);

								// Convert the wide character string to multibyte
								result = wcstombs_s(NULL, zData, length, wideData, length);
							}

							// If result != 0 then an error occurred
							if (result != 0)
							{
								ExtractException^ ex = gcnew ExtractException("ELI29720",
									"Unable to convert wide string to std::string!");
								ex->AddDebugData("Error code", result, false);
								throw ex;
							}

							// Store the result in the return string
							strSTL = zData;
						}
						catch(Exception^ ex)
						{
							ExtractException::Log("ELI29721", ex);
						}
						finally
						{
							// Ensure the memory is cleaned up
							if(zData != NULL)
							{
								delete [] zData;
							}
						}
					}

					// Return the STL string
					return strSTL;
				}
				//--------------------------------------------------------------------------------------
				static String^ AsSystemString(std::string stdString)
				{
					return gcnew String(stdString.c_str());
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
};
