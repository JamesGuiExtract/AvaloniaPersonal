#include "stdafx.h"

#include "LicenseStateCache.h"
#include "LicenseUtilities.h"

using namespace Extract;
using namespace Extract::Licensing;

//--------------------------------------------------------------------------------------------------
// Public methods
//--------------------------------------------------------------------------------------------------
LicenseStateCache::LicenseStateCache(LicenseIdName id, String^ componentName) :
_id(id),
_componentName(componentName),
_lastTickCount(0)
{
}
//--------------------------------------------------------------------------------------------------
void LicenseStateCache::Validate(String^ eliCode)
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
			// Perform the license validation
			LicenseUtilities::ValidateLicense(_id, eliCode, _componentName);

			// Store the last tick count
			_lastTickCount = tickCount;
		}
	}
	catch(Exception^ ex)
	{
		// Throw all exception as Extract exceptions
		throw ExtractException::AsExtractException("ELI27102", ex);
	}
}
//--------------------------------------------------------------------------------------------------