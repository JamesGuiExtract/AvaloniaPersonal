#include "StdAfx.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>

#include <vcclr.h>
#include <string>
#include <map>

#include "LicenseUtilities.h"
#include "MapLicenseIdsToComponentIds.h"

using namespace Extract;
using namespace Extract::Licensing;
using namespace Extract::Utilities;

#pragma unmanaged
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;
#pragma managed

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
			ExtractException::Assert("ELI21825", "Failed array allocation!", zData != NULL);

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
				ExtractException^ ex = gcnew ExtractException("ELI21826",
					"Unable to convert wide string to std::string!");
				ex->AddDebugData("Error code", result, false);
				throw ex;
			}

			// Store the result in the return string
			strSTL = zData;
		}
		catch(Exception^ ex)
		{
			ExtractException::Log("ELI21827", ex);
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
String^ AsSystemString(string stdString)
{
	return gcnew String(stdString.c_str());
}

//--------------------------------------------------------------------------------------------------
// Public methods
//--------------------------------------------------------------------------------------------------
[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId="mapLabel")]
void LicenseUtilities::LoadLicenseFilesFromFolder(int licenseType, MapLabel^ mapLabel)
{
	try
	{
		if (!_licensesLoaded)
		{
			// Ensure the calling assembly has been signed by Extract
			if (!CheckData(Assembly::GetCallingAssembly()))
			{
				throw gcnew ExtractException("ELI21705", "Failed to initialize license data!");
			}

			// Load the license file(s)
			LicenseManagement::sGetInstance().loadLicenseFilesFromFolder(
				LICENSE_MGMT_PASSWORD, licenseType);

			_licensesLoaded = true;
		}
	}
	catch(UCLIDException& uex)
	{
		throw gcnew ExtractException("ELI21944", "Failed loading license from folder!",
			AsSystemString(uex.asStringizedByteStream()));
	}
	catch(Exception^ ex)
	{
		// Wrap all exceptions as an ExtractException
		throw gcnew ExtractException("ELI21709", "Failed loading license from folder!", ex);
	}
}
//--------------------------------------------------------------------------------------------------
bool LicenseUtilities::IsLicensed(LicenseIdName id)
{
	try
	{
		// Check if the particular component ID is licensed
		return LicenseManagement::sGetInstance().isLicensed(
			ValueMapConversion::getConversion(static_cast<unsigned int>(id)));
	}
	catch(UCLIDException& uex)
	{
		ExtractException^ ee = gcnew ExtractException("ELI21945",
			"Failed checking licensed state for component!",
			AsSystemString(uex.asStringizedByteStream()));
		ee->AddDebugData("License Id Name", id.ToString("G"), true);
		throw ee;
	}
	catch(Exception^ ex)
	{
		// Wrap all exceptions as an ExtractException
		ExtractException^ ee = ExtractException::AsExtractException("ELI21937", ex);
		ee->AddDebugData("License Id Name", id.ToString("G"), true);
		throw ee;
	}
}
//--------------------------------------------------------------------------------------------------
bool LicenseUtilities::IsTemporaryLicense(LicenseIdName id)
{
	try
	{
		// Check if the specified component id is temporarily licensed
		return LicenseManagement::sGetInstance().isTemporaryLicense(
			ValueMapConversion::getConversion(static_cast<unsigned int>(id)));
	}
	catch(UCLIDException& uex)
	{
		ExtractException^ ee = gcnew ExtractException("ELI23061",
			"Failed checking for temporary license state!",
			AsSystemString(uex.asStringizedByteStream()));
		ee->AddDebugData("License Id Name", id.ToString("G"), true);
		throw ee;
	}
	catch(Exception^ ex)
	{
		ExtractException^ ee = ExtractException::AsExtractException("ELI23062", ex);
		ee->AddDebugData("License Id Name", id.ToString("G"), true);
		throw ee;
	}
}
//--------------------------------------------------------------------------------------------------
DateTime LicenseUtilities::GetExpirationDate(LicenseIdName id)
{
	try
	{
		// Get the expiration date
		CTime time = InternalGetExpirationDate(id);

		// Return the expiration date as a DateTime object
		return DateTime(time.GetYear(), time.GetMonth(), time.GetDay(), time.GetHour(),
			time.GetMinute(), time.GetSecond());
	}
	catch(UCLIDException& uex)
	{
		ExtractException^ ee = gcnew ExtractException("ELI23095",
			"Failed getting license expiration date!",
			AsSystemString(uex.asStringizedByteStream()));
		ee->AddDebugData("License Id Name", id.ToString("G"), true);
		throw ee;
	}
	catch(Exception^ ex)
	{
		ExtractException^ ee = ExtractException::AsExtractException("ELI23096", ex);
		ee->AddDebugData("License Id Name", id.ToString("G"), true);
		throw ee;
	}
}
//--------------------------------------------------------------------------------------------------
void LicenseUtilities::ValidateLicense(LicenseIdName id, String^ eliCode, String^ componentName)
{
	try
	{
		// Validate the license ID
		LicenseManagement::sGetInstance().validateLicense(
			ValueMapConversion::getConversion(static_cast<unsigned int>(id)),
			asString(eliCode), asString(componentName));
	}
	catch(UCLIDException& uex)
	{
		ExtractException^ ee = gcnew ExtractException("ELI21946",
			"License validation failed!",
			AsSystemString(uex.asStringizedByteStream()));
		ee->AddDebugData("License Id Name", id.ToString("G"), true);
		ee->AddDebugData("Component name", componentName, false);
		throw ee;
	}
	catch(Exception^ ex)
	{
		// Wrap all exceptions as an ExtractException
		ExtractException^ ee = gcnew ExtractException("ELI21938",
			"License validation failed!", ex);
		ee->AddDebugData("License Id Name", id.ToString("G"), true);
		ee->AddDebugData("Component name", componentName, false);
		throw ee;
	}
}
//--------------------------------------------------------------------------------------------------
void LicenseUtilities::ResetCache()
{
	try
	{
		LicenseManagement::sGetInstance().resetCache();
	}
	catch(UCLIDException& uex)
	{
		throw gcnew ExtractException("ELI21947",
			"Failed resetting the license cache!", 
			AsSystemString(uex.asStringizedByteStream()));
	}
	catch(Exception^ ex)
	{
		// Wrap all exceptions as an ExtractException
		throw gcnew ExtractException("ELI21939", "Failed resetting the license cache!", ex);
	}
}
//--------------------------------------------------------------------------------------------------
void LicenseUtilities::EnableAll()
{
	try
	{
		LicenseManagement::sGetInstance().enableAll();
	}
	catch(UCLIDException& uex)
	{
		throw gcnew ExtractException("ELI21948",
			"Failed enabling licenses!",
			AsSystemString(uex.asStringizedByteStream()));
	}
	catch(Exception^ ex)
	{
		// Wrap all exceptions as an ExtractException
		throw gcnew ExtractException("ELI21940", "Failed enabling licenses!", ex);
	}
}
//--------------------------------------------------------------------------------------------------
void LicenseUtilities::EnableId(LicenseIdName id)
{
	try
	{
		LicenseManagement::sGetInstance().enableId(
			ValueMapConversion::getConversion(static_cast<unsigned long>(id)));
	}
	catch(UCLIDException& uex)
	{
		ExtractException^ ee = gcnew ExtractException("ELI21949",
			"Failed enabling license!", 
			AsSystemString(uex.asStringizedByteStream()));
		ee->AddDebugData("License Id name", id.ToString("G"), true);
		throw ee;
	}
	catch(Exception^ ex)
	{
		// Wrap all exceptions as an ExtractException
		ExtractException^ ee = gcnew ExtractException("ELI21941",
			"Failed enabling license!", ex);
		ee->AddDebugData("License Id name", id.ToString("G"), true);
		throw ee;
	}
}
//--------------------------------------------------------------------------------------------------
void LicenseUtilities::DisableAll()
{
	try
	{
		LicenseManagement::sGetInstance().disableAll();
	}
	catch(UCLIDException& uex)
	{
		throw gcnew ExtractException("ELI21950",
			"Failed disabling licenses!",
			AsSystemString(uex.asStringizedByteStream()));
	}
	catch(Exception^ ex)
	{
		// Wrap all exceptions as an ExtractException
		throw gcnew ExtractException("ELI21942", "Failed disabling licenses!", ex);
	}
}
//--------------------------------------------------------------------------------------------------
void LicenseUtilities::DisableId(LicenseIdName id)
{
	try
	{
		LicenseManagement::sGetInstance().disableId(
			ValueMapConversion::getConversion(static_cast<unsigned long>(id)));
	}
	catch(UCLIDException& uex)
	{
		ExtractException^ ee = gcnew ExtractException("ELI21955",
			"Failed disabling license!",
			AsSystemString(uex.asStringizedByteStream()));
		ee->AddDebugData("License Id name", id.ToString("G"), true);
		throw ee;
	}
	catch(Exception^ ex)
	{
		// Wrap all exceptions as an ExtractException
		ExtractException^ ee = gcnew ExtractException("ELI21943",
			"Failed disabling license!", ex);
		ee->AddDebugData("License Id name", id.ToString("G"), true);
		throw ee;
	}
}
//--------------------------------------------------------------------------------------------------
[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId="mapLabel")]
String^ LicenseUtilities::GetMapLabelValue(MapLabel^ mapLabel)
{
	try
	{
		// Ensure the calling assembly has been signed by Extract
		if (!CheckData(Assembly::GetCallingAssembly()))
		{
			throw gcnew ExtractException("ELI22043", "Error while retrieving map label value!");
		}

		// Return the LicenseManagement Password as a System::String
		return AsSystemString(LICENSE_MGMT_PASSWORD);
	}
	catch(UCLIDException& uex)
	{
		throw gcnew ExtractException("ELI22044", "Failed getting map label value!",
			AsSystemString(uex.asStringizedByteStream()));
	}
	catch(Exception^ ex)
	{
		throw gcnew ExtractException("ELI22045", "Failed getting map label value!", ex);
	}
}

//--------------------------------------------------------------------------------------------------
// Private methods
//--------------------------------------------------------------------------------------------------
array<Byte>^ LicenseUtilities::CreateInternalArray()
{
	try
	{
		return StringMethods::ConvertHexStringToBytes(Constants::ExtractPublicKey);
	}
	catch(Exception^ ex)
	{
		throw gcnew ExtractException("ELI21783", "Failed while initializing LicenseUtilities!", ex);
	}
}
//--------------------------------------------------------------------------------------------------
CTime LicenseUtilities::InternalGetExpirationDate(LicenseIdName id)
{
	// Get the license ID to check
	unsigned int licenseId = ValueMapConversion::getConversion(static_cast<unsigned int>(id));

	// Ensure the component is temporarily licensed
	ExtractException::Assert("ELI23094",
		"Component is not temporarily licensed, cannot get expiration date!",
		LicenseManagement::sGetInstance().isTemporaryLicense(licenseId));

	// Get the expiration date
	return LicenseManagement::sGetInstance().getExpirationDate(licenseId);
}
//--------------------------------------------------------------------------------------------------
bool LicenseUtilities::CheckData(Assembly^ assembly)
{
	ExtractException::Assert("ELI21704", "Assembly is null!", assembly != nullptr);

	// Get the name from the assembly
	AssemblyName^ assemblyName = assembly->GetName();

	if (assemblyName != nullptr)
	{
		// Get the public key for this assembly
		array<Byte>^ publicKey = assemblyName->GetPublicKey();

		if (publicKey != nullptr && publicKey->Length != 0)
		{
			// Build a StrongNameMembershipCondition from the assembly
			// and our internal public key
			StrongNameMembershipCondition^ mc =
				gcnew StrongNameMembershipCondition(gcnew StrongNamePublicKeyBlob(_myArray),
				assemblyName->Name, assemblyName->Version);

			// Check the StrongNameMembershipCondition against this assembly
			// (if the assembly has a different public key than our internal
			// public key then this check will return false)
			return mc->Check(assembly->Evidence);
		}
	}

	// assemblyName is null or publicKey is either null or empty
	return false;
}
