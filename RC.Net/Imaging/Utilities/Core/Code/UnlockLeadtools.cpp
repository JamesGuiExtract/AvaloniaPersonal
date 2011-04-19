#include "stdafx.h"

#include "UnlockLeadtools.h"

#include <UCLIDException.h>

#include <string>
using namespace std;

using namespace Extract;
using namespace Extract::Imaging::Utilities;
using namespace Extract::Licensing;
using namespace Leadtools;
using namespace System;

#pragma unmanaged
//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
// To reconstruct the license strings take every fourth character
// Example: string "abcdefghijklmno" translates to "dhl"
static string _DOCUMENT_SUPPORT_KEY = "m6$HzSMbB:tQ4}cRC&r9xeANV|lScf1X;U9QPC[3bUv";
static string _PDF_SAVE_SUPPORT_KEY = "hu,tefav9wl4*ZUCg[sJkqms1FYaSk05H27aNW0JY3H";
static string _PDF_READ_SUPPORT_KEY = "R,qWTs8vAVfuaBpzg=[2QF=WsjoC:yk3D:nrdw*XuH6";

//--------------------------------------------------------------------------------------------------
// Unmanaged methods
//--------------------------------------------------------------------------------------------------
// Unmangles the license string and returns it
string getUnmangledString(const string& mangledString)
{
	string returnVal;
	for (size_t i=3; i < mangledString.length(); i += 4)
	{
		returnVal += mangledString.at(i);
	}

	return returnVal;
}
//--------------------------------------------------------------------------------------------------
// Returns the document support key
string A()
{
	return getUnmangledString(_DOCUMENT_SUPPORT_KEY);
}
//--------------------------------------------------------------------------------------------------
// Returns the pdf read key
string B()
{
	return getUnmangledString(_PDF_READ_SUPPORT_KEY);
}
//--------------------------------------------------------------------------------------------------
// Returns the pdf write key
string C()
{
	return getUnmangledString(_PDF_SAVE_SUPPORT_KEY);
}

#pragma managed

//--------------------------------------------------------------------------------------------------
// Public methods
//--------------------------------------------------------------------------------------------------
ExtractException^ UnlockLeadtools::UnlockDocumentSupport(bool returnExceptionIfUnlicensed)
{
	try
	{
		// Only unlock support if the support is licensed and it is not already unlocked
		if (LicenseUtilities::IsLicensed(LicenseIdName::AnnotationFeature))
		{
			if (RasterSupport::IsLocked(RasterSupportType::Document))
			{
				// Unlock document (ie. annotations) support
				RasterSupport::Unlock(RasterSupportType::Document,
					gcnew String(A().c_str()));
			}

			// If failed to unlock document support, return an exception
			if (RasterSupport::IsLocked(RasterSupportType::Document))
			{
				ExtractException^ ee = gcnew ExtractException("ELI28057",
					"Unable to unlock document support.");
				return ee;
			}
		}
		else if(returnExceptionIfUnlicensed)
		{
			return gcnew ExtractException("ELI28058", "Document support is not licensed.");
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
		if (LicenseUtilities::IsLicensed(LicenseIdName::PdfReadWriteFeature))
		{
			if(RasterSupport::IsLocked(RasterSupportType::PdfRead))
			{
				// Unlock pdf read support
				RasterSupport::Unlock(RasterSupportType::PdfRead,
					gcnew String(B().c_str()));
			}
			if (RasterSupport::IsLocked(RasterSupportType::PdfSave))
			{
				// Unlock pdf write support
				RasterSupport::Unlock(RasterSupportType::PdfSave,
					gcnew String(C().c_str()));
			}

			// Ensure pdf support was unlocked
			bool pdfReadLocked = RasterSupport::IsLocked(RasterSupportType::PdfRead);
			bool pdfWriteLocked = RasterSupport::IsLocked(RasterSupportType::PdfSave);
			if (pdfReadLocked || pdfWriteLocked)
			{
				ExtractException^ ee = gcnew ExtractException("ELI28060",
					"Unable to unlock pdf support. Pdf support will be limited.");
				ee->AddDebugData("Pdf reading",
					pdfReadLocked ? "Locked" : "Unlocked", false);
				ee->AddDebugData("Pdf writing",
					pdfWriteLocked ? "Locked" : "Unlocked", false);
				return ee;
			}
		}
		else if (returnExceptionIfUnlicensed)
		{
			return gcnew ExtractException("ELI28061", "PDF support is not licensed.");
		}

		return nullptr;
	}
	catch(Exception^ ex)
	{
		throw ExtractException::AsExtractException("ELI28062", ex);
	}
}
//--------------------------------------------------------------------------------------------------