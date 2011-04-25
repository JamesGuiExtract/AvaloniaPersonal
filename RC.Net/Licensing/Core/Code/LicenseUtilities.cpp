#include "StdAfx.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>

#include <map>

#include "LicenseUtilities.h"
#include "MapLicenseIdsToComponentIds.h"
#include "StringHelperFunctions.h"

using namespace Extract;
using namespace Extract::Licensing;

#pragma unmanaged
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;
#pragma managed

//--------------------------------------------------------------------------------------------------
// Public methods
//--------------------------------------------------------------------------------------------------
void LicenseUtilities::LoadLicenseFilesFromFolder(int licenseType, MapLabel^ mapLabel)
{
    try
    {
        // Ensure a map label has been specified
        ASSERT_ARGUMENT("ELI27107", mapLabel != nullptr);

        if (!_licensesLoaded)
        {
            // Ensure the calling assembly has been signed by Extract
            if (!CheckData(Assembly::GetCallingAssembly()))
            {
                throw gcnew ExtractException("ELI21705", "Failed to initialize license data!");
            }

            // Load the license file(s)
            LicenseManagement::loadLicenseFilesFromFolder(
                LICENSE_MGMT_PASSWORD, licenseType);

            _licensesLoaded = true;
        }
    }
    catch(UCLIDException& uex)
    {
        throw gcnew ExtractException("ELI21944", "Failed loading license from folder!",
            StringHelpers::AsSystemString(uex.asStringizedByteStream()));
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
        return LicenseManagement::isLicensed(
            ValueMapConversion::getConversion(static_cast<unsigned int>(id)));
    }
    catch(UCLIDException& uex)
    {
        ExtractException^ ee = gcnew ExtractException("ELI21945",
            "Failed checking licensed state for component!",
            StringHelpers::AsSystemString(uex.asStringizedByteStream()));
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
        LicenseStateCache^ licenseState = nullptr;
        if(_licenseCache->TryGetValue(id, licenseState))
        {
            // Check if the specified component id is temporarily licensed
            return licenseState->IsTemporaryLicense();
        }
        else
        {
            throw gcnew ExtractException("ELI28744", "License id was not found in the collection.");
        }
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
        LicenseStateCache^ licenseState = nullptr;
        if(_licenseCache->TryGetValue(id, licenseState))
        {
            // Check if the specified component id is temporarily licensed
            return licenseState->ExpirationDate;
        }
        else
        {
            throw gcnew ExtractException("ELI28777", "License id was not found in the collection.");
        }
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
        LicenseStateCache^ licenseState = nullptr;
        if(_licenseCache->TryGetValue(id, licenseState))
        {
            // Check if the specified component id is temporarily licensed
            return licenseState->Validate(eliCode, componentName);
        }
        else
        {
            ExtractException^ ee = gcnew ExtractException("ELI28745",
                "License id was not found in the collection.");
            ee->AddDebugData("License ID", id.ToString("G"), true);
            throw ee;
        }
    }
    catch(Exception^ ex)
    {
        // Wrap all exceptions as an ExtractException
        throw ExtractException::AsExtractException("ELI21938", ex);
    }
}
//--------------------------------------------------------------------------------------------------
void LicenseUtilities::ResetCache()
{
    try
    {
        LicenseManagement::resetCache();

        ResetLicenseCache();
    }
    catch(UCLIDException& uex)
    {
        throw gcnew ExtractException("ELI21947",
            "Failed resetting the license cache!", 
            StringHelpers::AsSystemString(uex.asStringizedByteStream()));
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
        LicenseManagement::enableAll();

        ResetLicenseCache();
    }
    catch(UCLIDException& uex)
    {
        throw gcnew ExtractException("ELI21948",
            "Failed enabling licenses!",
            StringHelpers::AsSystemString(uex.asStringizedByteStream()));
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
        LicenseManagement::enableId(
            ValueMapConversion::getConversion(static_cast<unsigned long>(id)));

        ResetLicenseCache(id);
    }
    catch(UCLIDException& uex)
    {
        ExtractException^ ee = gcnew ExtractException("ELI21949",
            "Failed enabling license!", 
            StringHelpers::AsSystemString(uex.asStringizedByteStream()));
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
        LicenseManagement::disableAll();

        ResetLicenseCache();
    }
    catch(UCLIDException& uex)
    {
        throw gcnew ExtractException("ELI21950",
            "Failed disabling licenses!",
            StringHelpers::AsSystemString(uex.asStringizedByteStream()));
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
        LicenseManagement::disableId(
            ValueMapConversion::getConversion(static_cast<unsigned long>(id)));

        ResetLicenseCache(id);
    }
    catch(UCLIDException& uex)
    {
        ExtractException^ ee = gcnew ExtractException("ELI21955",
            "Failed disabling license!",
            StringHelpers::AsSystemString(uex.asStringizedByteStream()));
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
String^ LicenseUtilities::GetMapLabelValue(MapLabel^ mapLabel)
{
    try
    {
        // Ensure a map label has been specified
        ASSERT_ARGUMENT("ELI27108", mapLabel != nullptr);

        // Ensure the calling assembly has been signed by Extract
        if (!CheckData(Assembly::GetCallingAssembly()))
        {
            throw gcnew ExtractException("ELI22043", "Error while retrieving map label value!");
        }

        // Return the LicenseManagement Password as a System::String
        return StringHelpers::AsSystemString(LICENSE_MGMT_PASSWORD);
    }
    catch(UCLIDException& uex)
    {
        throw gcnew ExtractException("ELI22044", "Failed getting map label value!",
            StringHelpers::AsSystemString(uex.asStringizedByteStream()));
    }
    catch(Exception^ ex)
    {
        throw gcnew ExtractException("ELI22045", "Failed getting map label value!", ex);
    }
}
//--------------------------------------------------------------------------------------------------
bool LicenseUtilities::VerifyAssemblyData(Assembly^ assembly)
{
    try
    {
        return CheckData(assembly);
    }
    catch(UCLIDException& uex)
    {
        throw gcnew ExtractException("ELI31147", "Unable to verify assembly data.",
            StringHelpers::AsSystemString(uex.asStringizedByteStream()));
    }
    catch(Exception^ ex)
    {
        throw gcnew ExtractException("ELI31148", "Unable to verify assembly data.", ex);
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
Dictionary<LicenseIdName, LicenseStateCache^>^ LicenseUtilities::CreateLicenseCacheCollection()
{
    try
    {
        Dictionary<LicenseIdName, LicenseStateCache^>^ dictionary =
            gcnew Dictionary<LicenseIdName, LicenseStateCache^>();
        for each(LicenseIdName id in Enum::GetValues(LicenseIdName::typeid))
        {
            dictionary->Add(id, gcnew LicenseStateCache(id));
        }

        return dictionary;
    }
    catch(Exception^ ex)
    {
        throw gcnew ExtractException("ELI28741", "Failed while initializing collection.", ex);
    }
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
//--------------------------------------------------------------------------------------------------
void LicenseUtilities::ResetLicenseCache()
{
    // Reset the cache for each LicenseStateCache object
    for each(LicenseStateCache^ licenseCache in _licenseCache->Values)
    {
        licenseCache->ResetCache();
    }
}
//--------------------------------------------------------------------------------------------------
void LicenseUtilities::ResetLicenseCache(LicenseIdName id)
{
    LicenseStateCache^ licenseState = nullptr;
    if(_licenseCache->TryGetValue(id, licenseState))
    {
        // Reset the cache for this ID
        licenseState->ResetCache();
    }
    else
    {
        ExtractException^ ee = gcnew ExtractException("ELI28748",
            "License id was not found in the collection.");
        ee->AddDebugData("License ID", id.ToString("G"), true);
        throw ee;
    }
}
//--------------------------------------------------------------------------------------------------
