#pragma once

#include "LicenseIdName.h"

using namespace System;

namespace Extract
{
	namespace Licensing
	{
		public ref class LicenseStateCache sealed
		{
		public:
			// Contructs a new instance of the LicenseStateCache object
			// id - The ID that this LicenseStateCache will check
			// componentName - The name of the component to check licensing for
			LicenseStateCache(LicenseIdName id, String^ componentName);

			~LicenseStateCache() {};

			// Performs the validation check with the specified ELI code
			// eliCode - The ELI code to use if the license fails validation
			void Validate(String^ eliCode);

		private:
			// The component name to license
			String^ _componentName;

			// The id to license
			LicenseIdName _id;

			// Stores the last tick count
			volatile int _lastTickCount;

		}; // end class LicenseStateCache
	}; // end namespace Licensing
}; // end namespace Extract