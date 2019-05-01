#pragma once

#include "MapLabel.h"
#include "LicenseIdName.h"
#include "LicenseStateCache.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Reflection;
using namespace System::Security::Permissions;
using namespace System::Security::Policy;
using namespace System::Runtime::CompilerServices;
using namespace System::Runtime::InteropServices;

namespace Extract
{
	namespace Licensing
	{
        //------------------------------------------------------------------------------------------
		public ref class LicenseUtilities sealed
		{
		public:
            //--------------------------------------------------------------------------------------
			// Public methods
            //--------------------------------------------------------------------------------------
			// PURPOSE: To load Extract license files of a particular type
			//
			// ARGS:	licenseType - the type of license files to load
			//						0 - Default Extract passwords
			//						1 - IcoMap passwords
			//						2 - Simple rule writing passwords
			//
			//			mapLabel	- just used to prevent using LoadLicenseFilesFromFolder
			//						  as a delegate or event handler which could potentially
			//						  circumvent the check of whether the calling assembly
			//						  was signed by Extract Systems
			//
			// PROMISE:	All license files in the CommonComponents folder will be loaded
			static void LoadLicenseFilesFromFolder(int licenseType, MapLabel^ mapLabel);
            //--------------------------------------------------------------------------------------
			// PURPOSE: To check if a particular component is licensed
			//
			// ARGS:	id - the LicenseIdName enum of the particular component to check
			static bool IsLicensed(LicenseIdName id);
            //--------------------------------------------------------------------------------------
			// PURPOSE: To check if a particular component is temporarily licensed
			//
			// ARGS:	id - the LicenseIDName enum of the particular component to check
			static bool IsTemporaryLicense(LicenseIdName id);
            //--------------------------------------------------------------------------------------
			// PURPOSE: To get the expiration date of a particular component ID
			//
			// ARGS:	id - the LicenseIdName enum of the particular component to check
			//
			// REQUIRE:	id is a temporarily licensed component
			static DateTime GetExpirationDate(LicenseIdName id);
            //--------------------------------------------------------------------------------------
			// PURPOSE: To validate if a particular type of object is licensed
			//
			// ARGS:	id				- The LicenseIdName enum of the particular
			//							  component to validate
			//			eliCode			- The ELI code to associate with the exception that
			//							  will be thrown if the component is not licensed
			//			componentName	- The name of the component whose license is being
			//							  validated
			static void ValidateLicense(LicenseIdName id, String^ eliCode, String^ componentName);
            //--------------------------------------------------------------------------------------
			// PURPOSE: To reset the cache of license states
			static void ResetCache(); 
            //--------------------------------------------------------------------------------------
			// PURPOSE: To enable all currently licensed component IDs
			// Added as per [DotNetRCAndUtils #122]
			static void EnableAll();
            //--------------------------------------------------------------------------------------
			// PURPOSE: To enable the specified licensed component ID
			// Added as per [DotNetRCAndUtils #122]
			static void EnableId(LicenseIdName id);
            //--------------------------------------------------------------------------------------
			// PURPOSE: To disable all currently licensed component IDs
			// Added as per [DotNetRCAndUtils #122]
			static void DisableAll();
            //--------------------------------------------------------------------------------------
			// PURPOSE: To disable the specified licensed component ID
			// Added as per [DotNetRCAndUtils #122]
			static void DisableId(LicenseIdName id);
            //--------------------------------------------------------------------------------------
			// PURPOSE: To retrieve the encrypted day code value needed to initialize
			//			any privately licensed objects.
			static String^ GetMapLabelValue(MapLabel^ mapLabel);
            //--------------------------------------------------------------------------------------
            // PURPOSE: To verify that the assembly is an extract assembly.
            static bool VerifyAssemblyData(Assembly^ assembly);
			//--------------------------------------------------------------------------------------
			// PURPOSE: A special purpose function that should be called only via
			// Extract.Interop.SecureObjectCreator in order to validate that the SecureObjectCreator
			// implementation being used is ours.
			// In particular, this triggers the LicenseManagement class to initialize an encrypted
			// day code (LICENSE_MGMT_PASSWORD) to use when validating SecureObjectCreator instances
			// have properly registered themselves.
			static void InitRegisteredObjects(MapLabel^ mapLabel);
            //--------------------------------------------------------------------------------------
			// PURPOSE: A special purpose function that should be called only via
			// Extract.Interop.SecureObjectCreator in order to validate that the SecureObjectCreator
			// implementation being used is ours.
			// In particular, this encodes the specified objectID using an encrypted day code
			// (LICENSE_MGMT_PASSWORD).
			static void RegisterObject(int objectId, MapLabel^ mapLabel);

		private:
            //--------------------------------------------------------------------------------------
			// Private variables
            //--------------------------------------------------------------------------------------
			// The array used to store the public key for this assembly
			static cli::array<System::Byte>^ _myArray = CreateInternalArray();

			// The dictionary of license ID's to LicenseStateCache objects
			static Dictionary<LicenseIdName, LicenseStateCache^>^ _licenseCache
				= CreateLicenseCacheCollection();

			// boolean flag to indicate whether licenses have been loaded from folder yet
			static bool _licensesLoaded;

            //--------------------------------------------------------------------------------------
			// Private methods
            //--------------------------------------------------------------------------------------
			// PURPOSE: To create an array containing the public key data for this assembly
			static cli::array<System::Byte>^ CreateInternalArray();
            //--------------------------------------------------------------------------------------
			static Dictionary<LicenseIdName, LicenseStateCache^>^ CreateLicenseCacheCollection();
            //--------------------------------------------------------------------------------------
			// PURPOSE: To check the given assemblies public key against the public key for this
			//			assembly.  Returns true if they match or false otherwise.
			//
			// ARGS:	assembly - the Assembly whose public key will be checked.
			static bool CheckData(Assembly^ assembly);
            //--------------------------------------------------------------------------------------
			// PURPOSE: To call the ResetCache method for each LicenseStateCache object in the
			//			_licenseCache collection.
			static void ResetLicenseCache();
            //--------------------------------------------------------------------------------------
			// PURPOSE: To call the ResetCache method for a particular license ID.
			static void ResetLicenseCache(LicenseIdName id);
            //--------------------------------------------------------------------------------------
			// PURPOSE:	Added to remove FxCop error - http://msdn.microsoft.com/en-us/ms182169.aspx
			//			Microsoft.Design::CA1053 - Static holder types should not have constructors
			LicenseUtilities(void) {};
            //--------------------------------------------------------------------------------------

		}; // end class LicenseUtilities
	}; // end namespace Licensing
}; // end namespace Extract
