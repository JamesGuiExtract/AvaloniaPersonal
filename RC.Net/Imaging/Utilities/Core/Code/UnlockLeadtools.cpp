#include "stdafx.h"

#define pointer_safety Pointer_safety
#define errc Errc
#define io_errc IO_errc

#include "UnlockLeadtools.h"
#include <UCLIDException.h>
#include <MiscLeadUtils.h>
#include <LicenseMgmt.h>
#include <LeadToolsLicensing.h>

#include <string>

#include <msclr\marshal_cppstd.h>
#include <msclr\marshal_windows.h>

using namespace std;

using namespace System;
using namespace System::IO;
using namespace System::Reflection;
using namespace Extract;
using namespace Extract::Imaging::Utilities;
using namespace Extract::Licensing;
using namespace Leadtools;
using namespace msclr::interop;

#pragma managed

//--------------------------------------------------------------------------------------------------
// Public methods
//--------------------------------------------------------------------------------------------------
void UnlockLeadtools::UnlockLeadToolsSupport()
{
	try
	{
		if (LicenseInitialized)
		{
			return;
		}

		// Initialize license for C++
		InitLeadToolsLicense();

		// InitializeLicence for .Net
		// Building this path here instead of using the FileSystemsMethods because of issues adding that as a Reference to this assembly
		System::String^ ExtractCommonPath = 
			(gcnew Uri(Path::GetDirectoryName(Assembly::GetExecutingAssembly()->CodeBase)))->LocalPath;

		System::String^ LicenseFileName;
		System::String^ LicenseKey;

		bool isPDFLicense = LicenseManagement::isPDFLicensed();
		if (isPDFLicense)
		{
			LicenseFileName = Path::Combine(ExtractCommonPath, "LEADTOOLS_PDF.OCL");
			LicenseKey = marshal_as<String^>(gstrLEADTOOLS_DEVELOPER_PDF_KEY);
		}
		else if (LicenseManagement::isPDFReadLicensed())
		{
			LicenseFileName = Path::Combine(ExtractCommonPath, "LEADTOOLS_PDF_READ.OCL");
			LicenseKey = marshal_as<String^>(gstrLEADTOOLS_DEVELOPER_PDF_READ_KEY);
		}
		else
		{
			LicenseFileName = Path::Combine(ExtractCommonPath, "LEADTOOLS.OCL");
			LicenseKey = marshal_as<String^>(gstrLEADTOOLS_DEVELOPER_KEY);
		}

		if (File::Exists(LicenseFileName))
		{
			RasterSupport::SetLicense(LicenseFileName, LicenseKey);
			LicenseInitialized = true;
		}
	}
	catch (Exception^ ex)
	{
		throw ExtractException::AsExtractException("ELI46649", ex);
	}
}
//--------------------------------------------------------------------------------------------------
ExtractException^ UnlockLeadtools::UnlockDocumentSupport(bool returnExceptionIfUnlicensed)
{
	try
	{
		if (RasterSupport::IsLocked(RasterSupportType::Basic))
		{
			UnlockLeadToolsSupport();
		}

		return nullptr;
	}
	catch(Exception^ ex)
	{
		throw ExtractException::AsExtractException("ELI28059", ex);
	}
}
//--------------------------------------------------------------------------------------------------
ExtractException^ UnlockLeadtools::UnlockPdfSupport(bool returnExceptionIfUnlicensed)
{
	try
	{
		if (RasterSupport::IsLocked(RasterSupportType::Basic))
		{
			UnlockLeadToolsSupport();
		}

		return nullptr;
	}
	catch(Exception^ ex)
	{
		throw ExtractException::AsExtractException("ELI28062", ex);
	}
}
//--------------------------------------------------------------------------------------------------