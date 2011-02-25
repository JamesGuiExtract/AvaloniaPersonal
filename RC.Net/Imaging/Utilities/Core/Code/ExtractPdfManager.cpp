#include "stdafx.h"

#include "ExtractPdfManager.h"
#include "StringHelperFunctions.h"

#include <UCLIDException.h>

using namespace Extract;
using namespace Extract::Imaging::Utilities;
using namespace Extract::Licensing;
using namespace System::Windows::Forms;

//--------------------------------------------------------------------------------------------------
// Public methods
//--------------------------------------------------------------------------------------------------
ExtractPdfManager::ExtractPdfManager(System::String ^fileName, bool fileUsedAsInput)
	: _pdfFile(__nullptr)
{
	try
	{
		// Validate the license
		LicenseUtilities::ValidateLicense(LicenseIdName::ExtractCoreObjects, "ELI27943",
			"Extract Pdf Manager");

		ExtractException::Assert("ELI28065", "File name cannot be null or empty",
			!String::IsNullOrEmpty(fileName));

		// Set the internal PDF manager
		_pdfFile = new PDFInputOutputMgr(StringHelpers::AsSTLString(fileName), fileUsedAsInput);

		_fileName = StringHelpers::AsSystemString(_pdfFile->getFileName());
		_fileNameInformation = StringHelpers::AsSystemString(_pdfFile->getFileNameInformationString());

		_inputFile = fileUsedAsInput;
	}
	catch(UCLIDException& uex)
	{
		if (_pdfFile != __nullptr)
		{
			delete _pdfFile;
			_pdfFile = __nullptr;
		}

		ExtractException^ ee = gcnew ExtractException("ELI28018",
			"Unable to create new ExtractPdfManager.",
			StringHelpers::AsSystemString(uex.asStringizedByteStream()));
		throw ee;
	}
	catch(Exception^ ex)
	{
		if (_pdfFile != __nullptr)
		{
			delete _pdfFile;
			_pdfFile = __nullptr;
		}

		// Wrap all exceptions as an ExtractException
		ExtractException^ ee = ExtractException::AsExtractException("ELI28019", ex);
		throw ee;
	}
}
//--------------------------------------------------------------------------------------------------
ExtractPdfManager::~ExtractPdfManager()
{
	// Release the PDF manager
	if (_pdfFile != __nullptr)
	{
		try
		{
			delete _pdfFile;
			_pdfFile = __nullptr;
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
	if (_pdfFile != __nullptr)
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