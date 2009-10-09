#include "stdafx.h"

#include "ExtractPdfManager.h"

#include <UCLIDException.h>

using namespace Extract;
using namespace Extract::Imaging::Utilities;
using namespace Extract::Licensing;
using namespace System::Windows::Forms;

//--------------------------------------------------------------------------------------------------
// Internal functions
//--------------------------------------------------------------------------------------------------
// PURPOSE: To Convert .net string to stl string
// NOTE:	This method is used internally and is placed here so the header file can
//			be used in projects not compiled with the /clr switch
//
// This function is based on code from Stan Lippman's Blog on MSDN:
// http://blogs.msdn.com/slippman/archive/2004/06/02/147090.aspx
string asString(String^ netString)
{
	char* zData = NULL;

	// Default the return string to empty
	string strSTL = "";

	if (!String::IsNullOrEmpty(netString))
	{
		try
		{
			// Compute the length needed to hold the converted string
			int length = ((netString->Length+1) * 2);

			// Declare a char array to hold the converted string
			zData = new char[length];
			ExtractException::Assert("ELI27940", "Failed array allocation!", zData != NULL);

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
				ExtractException^ ex = gcnew ExtractException("ELI27941",
					"Unable to convert wide string to std::string!");
				ex->AddDebugData("Error code", result, false);
				throw ex;
			}

			// Store the result in the return string
			strSTL = zData;
		}
		catch(Exception^ ex)
		{
			ExtractException::Log("ELI27942", ex);
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
//--------------------------------------------------------------------------------------------------
String^ AsSystemString(const string& stdString)
{
	return gcnew String(stdString.c_str());
}

//--------------------------------------------------------------------------------------------------
// Public methods
//--------------------------------------------------------------------------------------------------
ExtractPdfManager::ExtractPdfManager(System::String ^fileName, bool fileUsedAsInput)
{
	try
	{
		// Validate the license
		_licenseCache->Validate("ELI27943");

		ExtractException::Assert("ELI28065", "File name cannot be null or empty",
			!String::IsNullOrEmpty(fileName));

		// Set the internal PDF manager
		_pdfFile = new PDFInputOutputMgr(asString(fileName), fileUsedAsInput);

		_fileName = AsSystemString(_pdfFile->getFileName());
		_fileNameInformation = AsSystemString(_pdfFile->getFileNameInformationString());

		_inputFile = fileUsedAsInput;
	}
	catch(UCLIDException& uex)
	{
		ExtractException^ ee = gcnew ExtractException("ELI28018",
			"Unable to create new ExtractPdfManager.",
			AsSystemString(uex.asStringizedByteStream()));
		throw ee;
	}
	catch(Exception^ ex)
	{
		// Wrap all exceptions as an ExtractException
		ExtractException^ ee = ExtractException::AsExtractException("ELI28019", ex);
		throw ee;
	}
}
//--------------------------------------------------------------------------------------------------
ExtractPdfManager::~ExtractPdfManager()
{
	// Release the PDF manager
	if (_pdfFile != NULL)
	{
		try
		{
			delete _pdfFile;
			_pdfFile = NULL;
		}
		catch(Exception^ ex)
		{
			// Log any exception thrown by delete
			ExtractException::Log("ELI27939", ex);
		}
	}
}
//--------------------------------------------------------------------------------------------------
bool ExtractPdfManager::IsInputFile()
{
	try
	{
		return _inputFile;
	}
	catch(Exception^ ex)
	{
		// Wrap all exceptions as an ExtractException
		ExtractException^ ee = ExtractException::AsExtractException("ELI27946", ex);
		throw ee;
	}
}
//--------------------------------------------------------------------------------------------------
String^ ExtractPdfManager::FileName::get()
{
	try
	{
		return _fileName;
	}
	catch(Exception^ ex)
	{
		// Wrap all exceptions as an ExtractException
		ExtractException^ ee = ExtractException::AsExtractException("ELI27949", ex);
		throw ee;
	}
}
//--------------------------------------------------------------------------------------------------
String^ ExtractPdfManager::FileNameInformationString::get()
{
	try
	{
		return _fileNameInformation;
	}
	catch(Exception^ ex)
	{
		// Wrap all exceptions as an ExtractException
		ExtractException^ ee = ExtractException::AsExtractException("ELI27952", ex);
		throw ee;
	}
}

//--------------------------------------------------------------------------------------------------
// Private methods
//--------------------------------------------------------------------------------------------------
ExtractPdfManager::!ExtractPdfManager()
{
	if (_pdfFile != NULL)
	{
		try
		{
			delete _pdfFile;
		}
		catch(...)
		{
			// Just eat exceptions in the finalizer
		}
	}
}
//--------------------------------------------------------------------------------------------------