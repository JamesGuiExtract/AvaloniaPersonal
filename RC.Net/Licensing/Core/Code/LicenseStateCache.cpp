#include "stdafx.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>

#include "LicenseStateCache.h"
#include "LicenseUtilities.h"
#include "MapLicenseIdsToComponentIds.h"
#include "StringHelperFunctions.h"

using namespace Extract;
using namespace Extract::Licensing;

//--------------------------------------------------------------------------------------------------
// Public methods
//--------------------------------------------------------------------------------------------------
LicenseStateCache::LicenseStateCache(LicenseIdName id) :
_id(id),
_lastTickCount(-1),
_temporaryLicenseChecked(false),
_temporaryLicense(false),
_expirationDate(),
_expirationDateValid(false)
{
	SetInternalValue();
}
//--------------------------------------------------------------------------------------------------
void LicenseStateCache::Validate(String^ eliCode, String^ componentName)
{
	try
	{
		// Get the current tick count and divide by 10000
		// (gives us snapshots in 10 second resolution)
		int tickCount = Environment::TickCount / 10000;

		// If tick count is different (10 seconds have elapsed)
		// perform the license validation
		if (tickCount != _lastTickCount)
		{
			// Validate the license ID
			LicenseManagement::validateLicense(_valueToCheck,
				StringHelpers::AsSTLString(eliCode), StringHelpers::AsSTLString(componentName));

			// Store the last tick count
			_lastTickCount = tickCount;
		}
	}
	catch(UCLIDException& uex)
	{
		ExtractException^ ee = gcnew ExtractException("ELI28752",
			"License validation failed!",
			StringHelpers::AsSystemString(uex.asStringizedByteStream()));
		ee->AddDebugData("License Id Name", _id.ToString("G"), true);
		ee->AddDebugData("Component name", componentName, false);
		throw ee;
	}
	catch(Exception^ ex)
	{
		// Wrap all exceptions as an ExtractException
		ExtractException^ ee = gcnew ExtractException("ELI28753",
			"License validation failed!", ex);
		ee->AddDebugData("License Id Name", _id.ToString("G"), true);
		ee->AddDebugData("Component name", componentName, false);
		throw ee;
	}
}
//--------------------------------------------------------------------------------------------------
bool LicenseStateCache::IsTemporaryLicense()
{
	if (!_temporaryLicenseChecked)
	{
		try
		{
			// Check if the specified component id is temporarily licensed
			_temporaryLicense = LicenseManagement::isTemporaryLicense(_valueToCheck);
			_temporaryLicenseChecked = true;
		}
		catch(UCLIDException& uex)
		{
			ExtractException^ ee = gcnew ExtractException("ELI28754",
				"Failed checking for temporary license state!",
				StringHelpers::AsSystemString(uex.asStringizedByteStream()));
			ee->AddDebugData("License Id Name", _id.ToString("G"), true);
			throw ee;
		}
		catch(Exception^ ex)
		{
			ExtractException^ ee = ExtractException::AsExtractException("ELI28755", ex);
			ee->AddDebugData("License Id Name", _id.ToString("G"), true);
			throw ee;
		}
	}

	return _temporaryLicense;
}
//--------------------------------------------------------------------------------------------------
DateTime LicenseStateCache::ExpirationDate::get()
{
	try
	{
		// Return the expiration date as a DateTime object
		return InternalGetExpirationDate();
	}
	catch(Exception^ ex)
	{
		throw ExtractException::AsExtractException("ELI28758", ex);
	}
}
//--------------------------------------------------------------------------------------------------
void LicenseStateCache::ResetCache()
{
	_lastTickCount = -1;
}

//--------------------------------------------------------------------------------------------------
// Private methods
//--------------------------------------------------------------------------------------------------
DateTime LicenseStateCache::InternalGetExpirationDate()
{
	try
	{
		if (!_expirationDateValid)
		{
			// Ensure the component is temporarily licensed
			ExtractException::Assert("ELI28759",
				"Component is not temporarily licensed, cannot get expiration date!",
				IsTemporaryLicense());

			// Get the expiration date
			CTime time = LicenseManagement::getExpirationDate(_valueToCheck);

			// Convert the date to a DateTime object
			_expirationDate = DateTime(time.GetYear(), time.GetMonth(), time.GetDay(), time.GetHour(),
				time.GetMinute(), time.GetSecond());
			_expirationDateValid = true;
		}

		// Return the expiration date
		return _expirationDate;
	}
	catch(UCLIDException& uex)
	{
		ExtractException^ ee = gcnew ExtractException("ELI28756",
			"Failed getting license expiration date!",
			StringHelpers::AsSystemString(uex.asStringizedByteStream()));
		ee->AddDebugData("License Id Name", _id.ToString("G"), true);
		throw ee;
	}
	catch(Exception^ ex)
	{
		ExtractException^ ee = ExtractException::AsExtractException("ELI28757", ex);
		ee->AddDebugData("License Id Name", _id.ToString("G"), true);
		throw ee;
	}
}
//--------------------------------------------------------------------------------------------------
void LicenseStateCache::SetInternalValue()
{
	_valueToCheck = ValueMapConversion::getConversion(static_cast<unsigned int>(_id));
}
//--------------------------------------------------------------------------------------------------