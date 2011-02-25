#include "stdafx.h"

#include "PdfLock.h"
#include "StringHelperFunctions.h"

#include <UCLIDException.h>

using namespace Extract;
using namespace Extract::Imaging::Utilities;
using namespace System;

//--------------------------------------------------------------------------------------------------
// Public methods
//--------------------------------------------------------------------------------------------------
PdfLock::PdfLock() : pLoadLocker(__nullptr)
{
	try
	{
		pLoadLocker = new LeadToolsPDFLoadLocker(true);
	}
	catch(UCLIDException& uex)
	{
		// Ensure the memory is cleaned up
		if (pLoadLocker != __nullptr)
		{
			delete pLoadLocker;
			pLoadLocker = __nullptr;
		}

		ExtractException^ ee = gcnew ExtractException("ELI29716", "Unable to create new PdfLock.",
			StringHelpers::AsSystemString(uex.asStringizedByteStream()));
		throw ee;
	}
	catch(Exception^ ex)
	{
		// Ensure the memory is cleaned up
		if (pLoadLocker != __nullptr)
		{
			delete pLoadLocker;
			pLoadLocker = __nullptr;
		}

		// Wrap all exceptions as an ExtractException
		ExtractException^ ee = ExtractException::AsExtractException("ELI29717", ex);
		throw ee;
	}
}
//--------------------------------------------------------------------------------------------------
PdfLock::PdfLock(bool lock) : pLoadLocker(__nullptr)
{
	if (lock)
	{
		try
		{
			pLoadLocker = new LeadToolsPDFLoadLocker(true);
		}
		catch(UCLIDException& uex)
		{
			// Ensure the memory is cleaned up
			if (pLoadLocker != __nullptr)
			{
				delete pLoadLocker;
				pLoadLocker = __nullptr;
			}

			ExtractException^ ee = gcnew ExtractException("ELI29712", "Unable to create new PdfLock.",
				StringHelpers::AsSystemString(uex.asStringizedByteStream()));
			throw ee;
		}
		catch(Exception^ ex)
		{
			// Ensure the memory is cleaned up
			if (pLoadLocker != __nullptr)
			{
				delete pLoadLocker;
				pLoadLocker = __nullptr;
			}

			// Wrap all exceptions as an ExtractException
			ExtractException^ ee = ExtractException::AsExtractException("ELI29713", ex);
			throw ee;
		}
	}
}
//--------------------------------------------------------------------------------------------------
PdfLock::~PdfLock()
{
	try
	{
		if (pLoadLocker != __nullptr)
		{
			delete pLoadLocker;
			pLoadLocker = __nullptr;
		}
	}
	catch(Exception^ ex)
	{
		// Log any exception thrown by delete
		ExtractException::Log("ELI29714", ex);
	}
}

//--------------------------------------------------------------------------------------------------
// Private methods
//--------------------------------------------------------------------------------------------------
PdfLock::!PdfLock()
{
	try
	{
		if (pLoadLocker != __nullptr)
		{
			delete pLoadLocker;
			pLoadLocker = __nullptr;
		}
	}
	catch(...)
	{
		// Just eat any exceptions in the finalizer
	}
}
