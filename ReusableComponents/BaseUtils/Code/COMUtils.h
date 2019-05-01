
#ifndef COM_UTILS_H
#define COM_UTILS_H

#include "BaseUtils.h"

#include <afxmt.h>
#include <ComDef.h>
#include <ComCat.h>
#include <string>
#include <vector>
#include <memory>

// Name of IMemoryManger implementer to be used to allow IManageableMemory implementers to report
// their memory usage.
#define MEMORY_MANAGER_CLASS	"Extract.Interop.MemoryManager"

// [LegacyRCAndUtils:6460]
// This macro releases the specified IMemoryManagerPtr, but ignores exception 0x8007042b
// (The process terminated unexpectedly). While this issue should be fixed properly at some point,
// in the meantime I can't figure out why this is occurring and if the process has terminated, there
// is no need to report memory usage to the garbage collector.
#define RELEASE_MEMORY_MANAGER(ipMemoryManager, strELI) \
	try \
	{ \
		if (ipMemoryManager != __nullptr) \
		{ \
			ipMemoryManager->ReportUnmanagedMemoryUsage(0); \
			ipMemoryManager = __nullptr; \
		} \
	} \
	catch (...) { }
	// https://extract.atlassian.net/browse/ISSUE-13072
	// It seems that the code attempting to determine if exceptions here could be ignored due to
	// a process in the midst of shutting down was itself encountering exceptions. I think the
	// safest change to make for now is to simply ignore any exception trying to report memory
	// usage. Exceptions here shouldn't have any direct impact on callers (assuming they are not
	// thrown out)-- negative effects would simply be that the .Net garbage collector would not
	// have as accurate data on the amount of unmanaged memory tied up.

// Creates an instance of the COM class indicated by progId into the specified ptr after verifying
// that the COM implementation is provided by an ExtractSystems strong-named .Net assembly. COM
// object implemented in unmanaged code cannot be created via this macro as strong named validation
// cannot be performed.
// This will make 2 attempts a creating our SecureObjectCreator by CLSID and validating that it
// registered itself as expected via LicenseUtilities::RegisterObject. The second attempt is to
// handle the case that InitRegisteredObjects was called on one calendar day but the validation
// didn't occur until the next (will fail due to the LICENSE_MGMT_PASSWORD changing). Once we trust
// the SecureObjectCreator instance, used it to perform strong name validation on the progId's
// implementing assembly before returning an instance of the class it represents.
#define SECURE_CREATE_OBJECT(strELI, ptr, progId) \
	try \
	{ \
		CSingleLock guard(&m_secureObjectLock, TRUE); \
		CLSID clsidSecureObjectCreator = { 0xC49D5019, 0x85A7, 0x476E, { 0xAF, 0x49, 0x39, 0x46, 0x35, 0x80, 0xF8, 0x04 } }; \
		ISecureObjectCreatorPtr ipSecureObjectCreator; \
		try \
		{ \
			ipSecureObjectCreator.CreateInstance(clsidSecureObjectCreator); \
			validateObjectRegistration(ipSecureObjectCreator->InstanceID); \
		} \
		catch (...) \
		{ \
			try \
			{ \
				ipSecureObjectCreator.CreateInstance(clsidSecureObjectCreator); \
				validateObjectRegistration(ipSecureObjectCreator->InstanceID); \
			} \
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI38707"); \
		} \
		ptr = ipSecureObjectCreator->GetObject(progId); \
		ASSERT_RESOURCE_ALLOCATION("ELI38708", ptr != nullptr); \
	} \
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION(strELI)

// Provides synchronization for SECURE_CREATE_OBJECT. 
static CCriticalSection m_secureObjectLock;

using namespace std;

// The registered key to use for registering SecureObjectCreator instances. Based off
// LICENSE_MGMT_PASSWORD.
static unique_ptr<long> m_sapnRegisteredObjectKey(nullptr);

// Represents a SecureObjectCreator instance that is currently registered for use in
// SECURE_CREATE_OBJECT.
static unique_ptr<long> m_sapnRegisteredObject;

// The number of times registerObjectBase has been called. Used to ensure it is not being called
// separate from LicenseManagement::registerObject.
static int m_snRegisteredObjectCount = 0;

//-------------------------------------------------------------------------------------------------
// REQUIRE: the COM environment should already have been initialized before these functions
// are called.  CoInitialize() should already have been called, and CoUninitialize() should
// not yet have been called.
void EXPORT_BaseUtils createCOMCategory(CATID categoryID, const string& strCategoryName);
//-------------------------------------------------------------------------------------------------
void EXPORT_BaseUtils registerCOMComponentInCategory(CLSID componentID, CATID categoryID);
//-------------------------------------------------------------------------------------------------
vector<string> EXPORT_BaseUtils getComponentProgIDsInCategory(const string& strCategoryName);
//-------------------------------------------------------------------------------------------------
void EXPORT_BaseUtils writeObjectToStream(IPersistStreamPtr& ipObj, IStream *pStream, 
										  string strELI, BOOL bClearDirty);
//-------------------------------------------------------------------------------------------------
void EXPORT_BaseUtils readObjectFromStream(IPersistStreamPtr& ipObj, IStream *pStream, 
										  string strELI);
//-------------------------------------------------------------------------------------------------
string EXPORT_BaseUtils classIDToProgID(CLSID clsid);
//-------------------------------------------------------------------------------------------------
_bstr_t EXPORT_BaseUtils get_bstr_t(string str);
_bstr_t EXPORT_BaseUtils get_bstr_t(const char* szStr);
_bstr_t EXPORT_BaseUtils get_bstr_t(BSTR strVal);
//-------------------------------------------------------------------------------------------------
string EXPORT_BaseUtils asString(BSTR strVal);
//-------------------------------------------------------------------------------------------------
long EXPORT_BaseUtils asLong(BSTR strValue);
//-------------------------------------------------------------------------------------------------
_bstr_t EXPORT_BaseUtils writeObjectToBSTR(IPersistStreamPtr& ipObj, BOOL bClearDirty);
//-------------------------------------------------------------------------------------------------
void EXPORT_BaseUtils readObjectFromBSTR(IPersistStreamPtr& ipObj, _bstr_t _bstr);
//-------------------------------------------------------------------------------------------------
void EXPORT_BaseUtils clearDirtyFlag(IPersistStreamPtr& ipObj);
//-------------------------------------------------------------------------------------------------
// This function will wait for read access for Stg objects.  This is nearly identical to
// the cpputils waitForFileAccess but it uses the StgOpen call as opposed to _access
// to check for file access. It will return the result of the StgOpen call (S_OK if it
// was successful and the error code if it is unsuccessful after the retries).
// If ppStorage != __nullptr and the StgOpen is successful, then ppStorage will contain
// the handle to the open storage object.
// REQUIRE: strFileName must not be NULL or empty
HRESULT EXPORT_BaseUtils waitForStgFileAccess(BSTR strFileName, IStorage** ppStorage = NULL);
//-------------------------------------------------------------------------------------------------
// This function will wait for write access for Stg objects. This is nearly identical
// to the cpputils waitForStgFileAccess but it uses StgCreateDocFile call as opposed
// to StgOpen.  It will return the result of the StgCreateDocFile call (S_OK if it
// was successful and the error code if it was unsuccessful after the retries).  Also
// ppStorage will contain the open IStorage handle if the call was successful.
// REQUIRE: ppStorage != __nullptr and strFileName is not NULL or empty
HRESULT EXPORT_BaseUtils waitForStgFileCreate(BSTR strFileName, IStorage** ppStorage, DWORD dwMode);
//-------------------------------------------------------------------------------------------------
// PURPOSE: Reads the object in the specified file into the specified persist stream.
// PARAMS:  ipObject - Will be set to the object read if successful.
//          bstrFileName - The full path to file name to read.
//          bstrObjectName - The name of object to read. Must correspond to how it was stored.
//          bEncrypted - true if the file is encrypted; false if the file is not encrypted.
//          strSignature - The file signature to validate; ignored if empty.
void EXPORT_BaseUtils readObjectFromFile(IPersistStreamPtr ipObject, BSTR bstrFileName, 
	BSTR bstrObjectName, bool bEncrypted = false, string strSignature = "");
//-------------------------------------------------------------------------------------------------
// PURPOSE: Writes the specified persist stream to the specified file.
// PARAMS:  ipObject - The object to write.
//          bstrFileName - The full path to file name to write.
//          bstrObjectName - The name of object to write.
//          bClearDirty - true if the dirty flag should be set to false; false if the dirty flag
//                        should be unchanged.
//          strSignature - The file signature to include with the object; ignored if empty.
void EXPORT_BaseUtils writeObjectToFile(IPersistStreamPtr ipObject, BSTR bstrFileName, 
	BSTR bstrObjectName, bool bClearDirty, string strSignature = "");
//-------------------------------------------------------------------------------------------------
// PURPOSE: Reads the storage object from the specified file.
// PARAMS:  ppStorage - Will be set to the storage object read if successful.
//          bstrFileName - The full path to file name to read.
//          bEncrypted - true if the file is encrypted; false if the file is not encrypted.
void EXPORT_BaseUtils readStorageFromFile(IStorage** ppStorage, BSTR bstrFileName, 
										  bool bEncrypted = false);
//-------------------------------------------------------------------------------------------------
// PURPOSE: Reads a stream from the specified storage object.
// PARAMS:  ppStream - Will be set to the read stream if successful.
//          ipStorage - The storage object from which to read the stream.
//          bstrStreamName - The name of stream to read. Must correspond to how it was stored.
void EXPORT_BaseUtils readStreamFromStorage(IStream** ppStream, IStoragePtr ipStorage, 
	BSTR bstrStreamName);

//-------------------------------------------------------------------------------------------------
// PURPOSE: Reads a IPersistStream object from a SAFEARRAY
// PARAMS:  psaData - pointer to a SAFEARRAY that has one dimension and contains BTYE
IPersistStreamPtr EXPORT_BaseUtils readObjFromSAFEARRAY(SAFEARRAY *psaData);

//-------------------------------------------------------------------------------------------------
// PURPOSE: Writes the ipObj object to a SAFEARRAY
// PARAMS:  ipObj - the object to stream to a SAFEARRAY
// NOTE:	it is the responsibility of the caller to make sure the SAFEARRAY is destroyed
//			when it is done being used
LPSAFEARRAY EXPORT_BaseUtils writeObjToSAFEARRAY(IPersistStreamPtr ipObj);
//-------------------------------------------------------------------------------------------------
// A special purpose function that should be called only via
// Extract.Interop.SecureObjectCreator -> Extract.Licensing.LicenseUtilities.InitRegisteredObjects
//		-> LicenseManagment::initRegisteredObjects
// in order to validate that the SecureObjectCreator implementation being used is ours.
// In particular, this initializes an encrypted day code (LICENSE_MGMT_PASSWORD) to be able to
// validate SecureObjectCreator have registered themselves via Extract.Licenseing as expected.
EXPORT_BaseUtils void initRegisteredObjectsBase(long objectCode);
//-------------------------------------------------------------------------------------------------
// A special purpose function that should be called only via
// Extract.Interop.SecureObjectCreator -> Extract.Licensing.LicenseUtilities.RegisterObject
//		-> LicenseManagment::registerObject
// in order to validate that the SecureObjectCreator implementation being used is ours.
// In particular, this registers a SecureObjectCreator's InstanceID using the previously
// initialized encrypted day code (LICENSE_MGMT_PASSWORD). This prevents 3rd party code from
// being able to use registerObjectBase to do the registration that is expected of our
// SecureObjectCreator implementation.
EXPORT_BaseUtils void registerObjectBase(long objectCode);
//-------------------------------------------------------------------------------------------------
// A special purpose function to allow SECURE_CREATE_OBJECT to verify that a SecureObjectCreator
// has registered itself as expected.
EXPORT_BaseUtils void validateObjectRegistration(long objectCode);
//-------------------------------------------------------------------------------------------------
// Allows the count of registerObjectBase calls to be compared with the count of 
// LicenseManagment::registerObject calls to ensure calls are not being made by 3rd party code.
EXPORT_BaseUtils inline int getRegisteredObjectCount() { return m_snRegisteredObjectCount; }
//-------------------------------------------------------------------------------------------------

#endif // COM_UTILS_H