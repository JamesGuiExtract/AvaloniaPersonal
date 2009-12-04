#pragma once

#include "LicenseIdName.h"

using namespace System;

namespace Extract
{
	namespace Licensing
	{
		private ref class LicenseStateCache sealed
		{
		public:
			// Contructs a new instance of the LicenseStateCache object
			// id - The ID that this LicenseStateCache will check
			// componentName - The name of the component to check licensing for
			LicenseStateCache(LicenseIdName id);

			// Performs the validation check with the specified ELI code
			// eliCode - The ELI code to use if the license fails validation
			void Validate(String^ eliCode, String^ componentName);

			// Returns true if this ID is temporarily licensed and false otherwise.
			bool IsTemporaryLicense();

			// Resets the tick count value to enforce license validation on next call.
			void ResetCache();

			// Gets the expiration date for the temporary licensed object.
			// If the object is not temporarily licensed this method will throw an exception.
			property DateTime ExpirationDate
			{
				DateTime get();
			}


		private:
			// The id to license
			LicenseIdName _id;
			unsigned int _valueToCheck;

			// Whether this is a temporary licensed object or not
			volatile bool _temporaryLicenseChecked;
			volatile bool _temporaryLicense;

			// Stores the last tick count
			volatile int _lastTickCount;

			volatile bool _expirationDateValid;
			volatile DateTime _expirationDate;

			// PURPOSE: To return a DateTime object containing the expiration date for this
			//			license ID
			DateTime InternalGetExpirationDate();

			// Sets the internal license ID value
			void SetInternalValue();
		}; // end class LicenseStateCache
	}; // end namespace Licensing
}; // end namespace Extract