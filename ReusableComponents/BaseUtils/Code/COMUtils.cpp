
#include "stdafx.h"
#include "COMUtils.h"
#include "cpputil.h"
#include "EncryptedFileManager.h"
#include "UCLIDException.h"
#include "TemporaryFileName.h"
#include "SafeArrayAccessGuard.h"

#include <AtlBase.h>
#include <objBase.h>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
static const long g_nSTREAM_READ_BLOCK_SIZE = 1024;

// Version that document files are stored as (corresponds to Windows 2000 or later)
static const USHORT gSTORAGE_VERSION = 1;

// Access mode for opening storage objects
static const DWORD gdwSTORAGE_ACCESS_MODE = STGM_SHARE_DENY_WRITE | STGM_DIRECT | STGM_READ;

// Access mode for creating storage objects
static const DWORD gdwSTORAGE_CREATE_MODE = STGM_SHARE_EXCLUSIVE | STGM_DIRECT | STGM_CREATE | STGM_WRITE;

// Access mode for opening streams
static const DWORD gdwSTREAM_ACCESS_MODE = STGM_SHARE_EXCLUSIVE | STGM_DIRECT | STGM_READ;

// Access mode for creating streams
static const DWORD gdwSTREAM_CREATE_MODE = gdwSTORAGE_CREATE_MODE;

//-------------------------------------------------------------------------------------------------
// Local functions
//-------------------------------------------------------------------------------------------------
void readStorageFromUnencryptedFile(IStorage** ppStorage, BSTR bstrFileName);
void readStorageFromEncryptedFile(IStorage** ppStorage, BSTR bstrFileName);

//-------------------------------------------------------------------------------------------------
// Exported functions
//-------------------------------------------------------------------------------------------------
void createCOMCategory(CATID categoryID, const string& strCategoryName)
{
	CComPtr<ICatRegister> ptrCR;
	HRESULT hr = ptrCR.CoCreateInstance(CLSID_StdComponentCategoriesMgr, NULL,
		CLSCTX_ALL);
	if (SUCCEEDED(hr))
	{
		CATEGORYINFO rgcc;
		rgcc.catid = categoryID;
		rgcc.lcid = 0x409;
		_bstr_t _bstrCategoryName = strCategoryName.c_str();
		wcscpy_s(rgcc.szDescription, _bstrCategoryName);
		if (FAILED(ptrCR->RegisterCategories(1, &rgcc)))
		{
			UCLIDException ue("ELI04250", "Unable to register COM category!");
			ue.addDebugInfo("categoryID", asString(categoryID));
			ue.addDebugInfo("strCategoryName", strCategoryName);
			throw ue;
		}
	}
	else
	{
		UCLIDException ue("ELI02156", "Unable to create instance of Component Category Manager!");
		ue.addDebugInfo("categoryID", asString(categoryID));
		ue.addDebugInfo("strCategoryName", strCategoryName);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void registerCOMComponentInCategory(CLSID componentID, CATID categoryID)
{
	CComPtr<ICatRegister> ptrCR;
	HRESULT hr = ptrCR.CoCreateInstance(CLSID_StdComponentCategoriesMgr, NULL,
		CLSCTX_ALL);
	if (SUCCEEDED(hr))
	{
		CATID rgcid;
		rgcid = categoryID;
		if (FAILED(ptrCR->RegisterClassImplCategories(componentID, 1, &rgcid)))
		{
			UCLIDException ue("ELI04251", "Unable to register class in specified category!");
			ue.addDebugInfo("categoryID", asString(categoryID));
			ue.addDebugInfo("componentID", asString(componentID));
			throw ue;
		}
	}
	else
	{
		UCLIDException ue("ELI19273", "Unable to create instance of Component Category Manager!");
		ue.addDebugInfo("categoryID", asString(categoryID));
		ue.addDebugInfo("componentID", asString(componentID));
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
vector<string> getComponentProgIDsInCategory(const string& strCategoryName)
{
	vector<string> vecComponentsProgIDs;

	_bstr_t _bstrCategoryNameToSearch(strCategoryName.c_str());

	// create an instance of the Category Information manager
	CComQIPtr<ICatInformation> ptrCatInfo;
	ptrCatInfo.CoCreateInstance(CLSID_StdComponentCategoriesMgr, 0,
		CLSCTX_INPROC_SERVER);
	
	// get all the available categories
	CComQIPtr<IEnumCATEGORYINFO> ptrEnumCatInfo;
	HRESULT hr = ptrCatInfo->EnumCategories(0x409, &ptrEnumCatInfo);
	if (FAILED(hr)) return vecComponentsProgIDs;
		
	// iterate through all the available categories and find matches
	CATEGORYINFO catInfo;
	ULONG numFetched = 0;
	while (SUCCEEDED(ptrEnumCatInfo->Next(1, &catInfo, &numFetched))) 
	{
		if (numFetched == 0) break;
		
		_bstr_t _bstrCurrCategoryName(catInfo.szDescription);
		if (_bstrCurrCategoryName == _bstrCategoryNameToSearch)
		{
			// get classes that implement the specified category
			CATID catID;
			catID = catInfo.catid;
			
			CComQIPtr<IEnumCLSID> ptrEnumCLSID;
			hr = ptrCatInfo->EnumClassesOfCategories(1, &catID, -1, NULL, &ptrEnumCLSID);
			if (FAILED(hr))	return vecComponentsProgIDs;
			
			// for each class that implements the component category
			// get its description, and add it to the return result collection
			CLSID clsid;
			numFetched = 0;
			while (SUCCEEDED(ptrEnumCLSID->Next(1, &clsid, &numFetched)))
			{
				if (numFetched == 0) break;
				
				try
				{
					LPOLESTR progID;
					// get prog id of the component
					if (FAILED(ProgIDFromCLSID(clsid, &progID)))
					{
						::CoTaskMemFree(progID);
						continue;
					}
					CString zProgID(progID);
					string strProgID((LPCTSTR)zProgID);
					// release memory
					::CoTaskMemFree(progID);
					vecComponentsProgIDs.push_back(strProgID);
				}
				catch (...)
				{
					// this component can't be instantiated, then skip it and go
					// to next component
				}
			}
			
			// we can assume that there is only one category with the given
			// description.  This is, in fact, a requirement for calling this
			// method.  Since we have found the one category, we can break out
			// of the while loop.
			break;
		}
	}

	return vecComponentsProgIDs;
}
//-------------------------------------------------------------------------------------------------
void writeObjectToStream(IPersistStreamPtr& ipObj, IStream *pStream, string strELI, BOOL bClearDirty)
{
	string strObjectName = "Unknown";
	try
	{
		try
		{
			// get the class ID of the object
			CLSID clsID;
			if (FAILED(ipObj->GetClassID(&clsID)))
				throw UCLIDException("ELI04620", "Unable to get class ID!");
			
			strObjectName = classIDToProgID(clsID);

			// write class ID to the stream
			if (FAILED(WriteClassStm(pStream, clsID)))
				throw UCLIDException("ELI04621", "Unable to write class ID to stream!");

			// save the object contents to the stream
			HRESULT hr = ipObj->Save(pStream, bClearDirty);
			if (FAILED(hr))
			{
				// check if the object supports error info...if so, throw a rich
				// exception object.  If not, throw a plain exception object indicating
				// that save failed
				ISupportErrorInfoPtr ipSupportErrorInfo = ipObj;
				if (ipSupportErrorInfo)
				{
					IErrorInfoPtr ipErrorInfo;
					HRESULT hr2 = GetErrorInfo(NULL, &ipErrorInfo);
					if (FAILED(hr2))
					{
						UCLIDException ue("ELI05054", "Unable to retrieve error information!");
						UCLIDException uexOuter("ELI05055", "Unable to save object to stream!", ue);
						uexOuter.addHresult(hr);
						uexOuter.addHresult(hr2);
						throw uexOuter;
					}

					CComBSTR bstrDescription;
					hr2 = ipErrorInfo->GetDescription(&bstrDescription);
					if (FAILED(hr2))
					{
						UCLIDException ue("ELI05056", "Unable to retrieve error description!");
						UCLIDException uexOuter("ELI05057", "Unable to save object to stream!", ue);
						uexOuter.addHresult(hr);
						uexOuter.addHresult(hr2);
						throw uexOuter;
					}
					
					_bstr_t _bstrDescription(bstrDescription);
					char *pszDescription = _bstrDescription;

					UCLIDException ue;
					ue.createFromString("ELI05058", pszDescription);
					UCLIDException uexOuter("ELI05059", "Unable to save object to stream!", ue);
					uexOuter.addHresult(hr);
					throw uexOuter;
				}

				UCLIDException ue("ELI04622", "Unable to save object to stream!");
				ue.addHresult(hr);
				throw ue;
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI09899");
	}
	catch(UCLIDException ue)
	{
		string strMsg;
		strMsg = "Unable to save \'";
		strMsg += strObjectName;
		strMsg += "\' object!";
		UCLIDException uexOuter(strELI, strMsg, ue);
		throw uexOuter;
	}
}
//-------------------------------------------------------------------------------------------------
void readObjectFromStream(IPersistStreamPtr& ipObj, IStream *pStream, string strELI)
{
	string strObjectName = "Unknown";
	try
	{
		try
		{
			// reset the object
			ipObj = __nullptr;

			// read class ID from the stream
			CLSID clsID;
			if (FAILED(ReadClassStm(pStream, &clsID)))
				throw UCLIDException("ELI04623", "Unable to read class ID from stream!");

			strObjectName = classIDToProgID(clsID);

			// create the object
			if (FAILED(ipObj.CreateInstance(clsID)))
				throw UCLIDException("ELI04624", "Unable to create object from stream!");

			// save the object contents to the stream
			HRESULT hr = ipObj->Load(pStream);
			if (FAILED(hr))
			{
				// check if the object supports error info...if so, throw a rich
				// exception object.  If not, throw a plain exception object indicating
				// that load failed
				ISupportErrorInfoPtr ipSupportErrorInfo = ipObj;
				if (ipSupportErrorInfo)
				{
					IErrorInfoPtr ipErrorInfo;
					HRESULT hr2 = GetErrorInfo(NULL, &ipErrorInfo);
					if (FAILED(hr2))
					{
						UCLIDException ue("ELI05049", "Unable to retrieve error information!");
						UCLIDException uexOuter("ELI04625", "Unable to load object from stream!", ue);
						uexOuter.addHresult(hr);
						uexOuter.addHresult(hr2);
						throw uexOuter;
					}

					CComBSTR bstrDescription;
					hr2 = ipErrorInfo->GetDescription(&bstrDescription);
					if (FAILED(hr2))
					{
						UCLIDException ue("ELI05052", "Unable to retrieve error description!");
						UCLIDException uexOuter("ELI05053", "Unable to load object from stream!", ue);
						uexOuter.addHresult(hr);
						uexOuter.addHresult(hr2);
						throw uexOuter;
					}
					
					_bstr_t _bstrDescription(bstrDescription);
					char *pszDescription = _bstrDescription;

					UCLIDException ue;
					ue.createFromString("ELI05050", pszDescription);
					UCLIDException uexOuter("ELI05016", "Unable to load object from stream!", ue);
					uexOuter.addHresult(hr);
					throw uexOuter;
				}

				UCLIDException ue("ELI05051", "Unable to load object from stream!");
				ue.addHresult(hr);
				throw ue;
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI09940");
	}
	catch(UCLIDException ue)
	{
		string strMsg;
		strMsg = "Unable to load \'";
		strMsg += strObjectName;
		strMsg += "\' object!";
		UCLIDException uexOuter(strELI, strMsg, ue);
		throw uexOuter;
	}
}
//-------------------------------------------------------------------------------------------------
std::string classIDToProgID(CLSID clsid)
{
	LPOLESTR progID;
	if (FAILED(ProgIDFromCLSID(clsid, &progID)))
	{
		throw UCLIDException("ELI09942", "Unable to get ProgID from class ID!");
	}
	CString zProgID(progID);
	string s((LPCTSTR)zProgID);
	::CoTaskMemFree(progID);
	return s;
}
//-------------------------------------------------------------------------------------------------
_bstr_t EXPORT_BaseUtils get_bstr_t(string str)
{
	return get_bstr_t(str.c_str());
}
//-------------------------------------------------------------------------------------------------
_bstr_t EXPORT_BaseUtils get_bstr_t(const char* szStr)
{	
	// If the input string is null return an empty _bstr_t
	if ( szStr == NULL )
	{
		return _bstr_t ( "" );
	}
	// This now will work with very large strings
	return _bstr_t(szStr);
}
//-------------------------------------------------------------------------------------------------
_bstr_t EXPORT_BaseUtils get_bstr_t(BSTR strVal)
{	
	if (strVal == NULL)
	{
		return _bstr_t("");
	}
	else
	{
		return _bstr_t(strVal);
	}
}
//-------------------------------------------------------------------------------------------------
std::string EXPORT_BaseUtils asString(BSTR strVal)
{
	string strTemp = get_bstr_t(strVal);
	return strTemp;
}
//-------------------------------------------------------------------------------------------------
long EXPORT_BaseUtils asLong(BSTR strValue)
{
	auto value = asString(strValue);
	return asLong(value);
}
//-------------------------------------------------------------------------------------------------
_bstr_t EXPORT_BaseUtils writeObjectToBSTR(IPersistStreamPtr& ipObj, BOOL bClearDirty)
{
	// Create a stream
	IStreamPtr ipStream;
	ipStream.Attach(SHCreateMemStream(__nullptr, 0));
	if (ipStream == __nullptr)
	{
		throw UCLIDException("ELI06691", "Unable to create stream object!");
	}

	// Save the object to the stream
	ASSERT_RESOURCE_ALLOCATION("ELI11062", ipObj != __nullptr);
	writeObjectToStream(ipObj, ipStream, "ELI11076", bClearDirty);

	// Set the stream seek pointer back to the beginning of the stream
	LARGE_INTEGER disp;
	disp.QuadPart = 0;
	ULARGE_INTEGER newPos;
	ipStream->Seek(disp, STREAM_SEEK_SET, &newPos);

	// Read all the bytes from the stream into a vector
	long nTotalNumBytes = 0;
	vector<OLECHAR> vecChars;
	char buf[g_nSTREAM_READ_BLOCK_SIZE];
	ULONG numBytesRead;
	// We will read the bytes in 1k chunks
	while (1)
	{
		HRESULT hr = ipStream->Read((void*)buf, g_nSTREAM_READ_BLOCK_SIZE, &numBytesRead);
		if (hr != S_OK)
		{
			break;
		}

		// Only add new characters if new chars exist
		if (numBytesRead > 0)
		{
			long nStart = vecChars.size();
			vecChars.resize(vecChars.size() + ((numBytesRead+1) / 2));
			memcpy(&vecChars[nStart], buf, numBytesRead);
			nTotalNumBytes += numBytesRead;
		}

		// break if the end of stream has been passed
		if (numBytesRead < g_nSTREAM_READ_BLOCK_SIZE)
		{
			break;
		}
	}
	
	// here we add an extra wchar at the beginning of the vector which
	// represents whether or not the bytestream had an odd number of bytes
	// 1 = odd
	// 0 = even
	// All BSTR's contain an even number of bytes because each "character" is unicode i.e. 2 bytes long.
	// since we are passing a stream of bytes as a BSTR the number of bytes passed will always be even,
	// even in the case where the original stream had an odd number of bytes
	// When decoding the stream we will need to know whether there is an extra byte on the end
	// that should be removed
	if (nTotalNumBytes % 2 != 0)
	{
		vecChars.insert(vecChars.begin(), 1);
	}
	else
	{
		vecChars.insert(vecChars.begin(), 0);
	}

	// prepend the unicode character array with the length of 
	// the array in bytes to make it a BSTR
	long nSize = vecChars.size()*2;
	vecChars.insert(vecChars.begin(), 0);
	vecChars.insert(vecChars.begin(), 0);
	memcpy(&vecChars[0], &nSize, 2*sizeof(OLECHAR));

	// return the _bstr_t
	// Note that &vecChars[2] is used.  A BSTR is an unsigned short*
	// That points to the first character of the string
	// The four bytes previous to the pointer contain the length of the 
	// BSTR in bytes.  We have the length stored in vecChars[0] and vecChars[1].
	// thus the beginning of the string is vecChars[0]
	_bstr_t _bstr((BSTR)&vecChars[2], true);
	return _bstr;
}
//-------------------------------------------------------------------------------------------------
void EXPORT_BaseUtils readObjectFromBSTR(IPersistStreamPtr& ipObj, _bstr_t _bstr)
{
	IStreamPtr ipStream;
	ipStream.Attach(SHCreateMemStream(__nullptr, 0));
	if (ipStream == __nullptr)
	{
		throw UCLIDException("ELI19322", "Unable to create stream object!");
	}

	// get the length of the bstr (in number of OLECHAR's)
	long nLength = _bstr.length();
	const wchar_t* str = _bstr;
	long nOdd = str[0];
	
	// calculate the number of bytes we want to write to the stream
	// 2 for each unicode character minus the OLECHAR reserved for the odd flag
	// minus 1 char if the original stream had an odd length
	ULONG nLen = 2*nLength - sizeof(OLECHAR) - nOdd;


	// this step of conversion into a vector is probably unnecessary
	vector<char> vecChars;
	vecChars.resize(nLen);
	memcpy((void*)&vecChars[0], (void*)&str[1], nLen*sizeof(char));

	// write the data out to a stream
	ULONG nNumWritten = 0;
	ipStream->Write((void*)&vecChars[0], nLen, &nNumWritten);

	// set the stream pointer back to the beginning
	LARGE_INTEGER disp;
	disp.QuadPart = 0;
	ULARGE_INTEGER newPos;
	ipStream->Seek(disp, STREAM_SEEK_SET, &newPos);

	// read the object in from the stream
	readObjectFromStream(ipObj, ipStream, "ELI11077");
}
//-------------------------------------------------------------------------------------------------
void EXPORT_BaseUtils clearDirtyFlag(IPersistStreamPtr& ipObj)
{
	// validate argument
	ASSERT_ARGUMENT("ELI20263", ipObj != __nullptr);

	// create a stream object
	IStreamPtr ipStream;
	ipStream.Attach(SHCreateMemStream(__nullptr, 0));
	ASSERT_RESOURCE_ALLOCATION("ELI20262", ipStream != __nullptr);

	// clear the dirty flag by saving the object to the stream
	HANDLE_HRESULT(ipObj->Save(ipStream, TRUE), "ELI20260", "Unable to clear dirty flag.", ipObj, 
		IID_IPersistStream);

	// ignore the stream result
}
//-------------------------------------------------------------------------------------------------
HRESULT EXPORT_BaseUtils waitForStgFileAccess(BSTR strFileName, IStorage** ppStorage)
{
	// Ensure the file name is not empty string
	ASSERT_ARGUMENT("ELI23899", strFileName != __nullptr && _bstr_t(strFileName, true).length() > 0);

	// Get the timeout and retry count
	int iTimeout = -1;
	int iRetryCount = -1;
	getFileAccessRetryCountAndTimeout(iRetryCount, iTimeout);

	// See if the file has the requested access
	int iRetries = 0;
	bool bRtnValue = false;
	HRESULT hr = S_OK;
	do
	{
		// Open the file storage
		IStoragePtr ipStorage = __nullptr;
		STGOPTIONS options = {gSTORAGE_VERSION};
		hr = StgOpenStorageEx(strFileName, gdwSTORAGE_ACCESS_MODE, STGFMT_DOCFILE, 0, &options, 
			0, IID_IStorage, (void**)&ipStorage);

		// Check the return value
		bRtnValue = hr == S_OK;

		// If the file was successfully opened AND pStorage != __nullptr then
		// set *ppStorage to the open storage object
		if (bRtnValue && ppStorage != __nullptr)
		{
			*ppStorage = ipStorage.Detach();
		}
		else
		{
			ipStorage = __nullptr;
		}

		// if file is not accessible check retry count 
		if ( !bRtnValue )
		{
			// If retry count has been exceeded log exception
			if ( iRetries > iRetryCount)
			{
				// Have checked the accessibility the required number of times so log exception
				UCLIDException ue("ELI24210", "File cannot be accessed with requested access!");
				ue.addDebugInfo("File Name", asString(strFileName));
				ue.addDebugInfo("Number of retries", iRetries);
				ue.addHresult(hr);

				// Just log the exception since this function is to give OS time 
				// to release the file.
				ue.log();
				break;
			}

			// increment the number of retries and wait for the timeout period
			iRetries++;
			Sleep(iTimeout);
		}
	}
	while (!bRtnValue);

	return hr;
}
//-------------------------------------------------------------------------------------------------
HRESULT EXPORT_BaseUtils waitForStgFileCreate(BSTR strFileName, IStorage** ppStorage, DWORD dwMode)
{
	// Ensure the file name is not empty string
	ASSERT_ARGUMENT("ELI24711", strFileName != __nullptr && _bstr_t(strFileName, true).length() > 0);
	ASSERT_ARGUMENT("ELI24712", ppStorage != __nullptr);

	// Get the timeout and retry count
	int iTimeout = -1;
	int iRetryCount = -1;
	getFileAccessRetryCountAndTimeout(iRetryCount, iTimeout);

	// See if the file has the requested access
	int iRetries = 0;
	bool bRtnValue;
	HRESULT hr = S_OK;
	do
	{
		// Open the file storage
		IStoragePtr ipStorage = __nullptr;
		hr = StgCreateStorageEx(strFileName, dwMode, STGFMT_DOCFILE, 0, NULL, 0, IID_IStorage, 
			(void**)&ipStorage);

		// Check the return value
		bRtnValue = hr == S_OK;

		// If the file was successfully opened
		// set *ppStorage to the open storage object
		if (bRtnValue)
		{
			*ppStorage = ipStorage.Detach();
		}
		else
		{
			ipStorage = __nullptr;
		}

		// if file is not accessible check retry count 
		if ( !bRtnValue )
		{
			// If retry count has been exceeded log exception
			if ( iRetries > iRetryCount)
			{
				// Have checked the accessibility the required number of times so log exception
				UCLIDException ue("ELI24743",
					"Storage object cannot be created for specified file!");
				ue.addDebugInfo("File To Open", asString(strFileName));
				ue.addDebugInfo("Number of retries", iRetries);
				ue.addHresult(hr);

				// Just log the exception since this function is to give OS time 
				// to release the file.
				ue.log();
				break;
			}

			// increment the number of retries and wait for the timeout period
			iRetries++;
			Sleep(iTimeout);
		}
	}
	while (!bRtnValue);

	return hr;
}
//-------------------------------------------------------------------------------------------------
void EXPORT_BaseUtils readObjectFromFile(IPersistStreamPtr ipObject, BSTR bstrFileName, 
										 BSTR bstrObjectName, bool bEncrypted, string strSignature)
{
	// [LRCAU #5987] Validate file existence
	string strFileName = asString(bstrFileName);
	validateFileOrFolderExistence(strFileName, "ELI32154");

	// Read the file storage
	IStoragePtr ipStorage;
	readStorageFromFile(&ipStorage, bstrFileName, bEncrypted);

	// Open the stream
	IStreamPtr ipStream;
	readStreamFromStorage(&ipStream, ipStorage, bstrObjectName);

	// Read signature from stream and ensure that it is correct
	if (!strSignature.empty())
	{
		CComBSTR bstrSignature;
		bstrSignature.ReadFromStream(ipStream);
		string strSignatureFromFile = CString(bstrSignature);
		if (strSignatureFromFile != strSignature.c_str())
		{
			UCLIDException ue("ELI25405", "Invalid signature.");
			ue.addDebugInfo("File name", strFileName);
			ue.addDebugInfo("Stream name", asString(bstrObjectName));
			ue.addDebugInfo("Expected signature", strSignature, bEncrypted);
			ue.addDebugInfo("Actual signature", strSignatureFromFile, bEncrypted);
			throw ue;
		}
	}

	// Load the object from the stream
	HRESULT hr = ipObject->Load(ipStream);
	if (FAILED(hr))
	{
		string strErrorMessage = "Unable to load " + asString(bstrObjectName) + ".";
		HANDLE_HRESULT(hr, "ELI25395", strErrorMessage, ipObject, IID_IPersistStream);
	}

	// Ensure the stream and storage are closed [LRCAU #5078]
	ipStream = __nullptr;
	ipStorage = __nullptr;
}
//-------------------------------------------------------------------------------------------------
void readStorageFromFile(IStorage** ppStorage, BSTR bstrFileName, bool bEncrypted)
{
	// [LRCAU #5987] Validate file existence
	validateFileOrFolderExistence(asString(bstrFileName), "ELI31841");
	if (bEncrypted)
	{
		readStorageFromEncryptedFile(ppStorage, bstrFileName);
	}
	else
	{
		readStorageFromUnencryptedFile(ppStorage, bstrFileName);
	}
}
//-------------------------------------------------------------------------------------------------
void readStreamFromStorage(IStream** ppStream, IStoragePtr ipStorage, BSTR bstrStreamName)
{
	ASSERT_ARGUMENT("ELI25409", ppStream != __nullptr);
	ASSERT_ARGUMENT("ELI25410", ipStorage != __nullptr);

	HRESULT hr = ipStorage->OpenStream(bstrStreamName, 0, gdwSTREAM_ACCESS_MODE, 0, ppStream);
	if (*ppStream == NULL || FAILED(hr))
	{
		UCLIDException ue("ELI25406", "Unable to open stream object.");
		ue.addDebugInfo("Stream name", asString(bstrStreamName));
		ue.addHresult(hr);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void writeObjectToFile(IPersistStreamPtr ipObject, BSTR bstrFileName, BSTR bstrObjectName, 
				  bool bClearDirty, string strSignature, bool bWriteDirectlyToDestination)
{
	TemporaryFileName *pTempOutFile = __nullptr;
	IStreamPtr ipStream(__nullptr);
	IStoragePtr ipStorage(__nullptr);

	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI25598", ipObject != __nullptr);

			// Create a directory for the file if necessary
			string strPermanentFileName = asString(bstrFileName);
			string strDirectory = getDirectoryFromFullPath(strPermanentFileName);
			createDirectory(strDirectory);

			// [LegacyRCAndUtils:6255]
			// To avoid corrupting existing files, if writing to an existing file, save first to a
			// temporary filename, then overwrite the original only if the write is successful.
			string strFileName;
			if (!bWriteDirectlyToDestination && isValidFile(strPermanentFileName))
			{
				pTempOutFile = new TemporaryFileName(true);
				strFileName = pTempOutFile->getName();
			}
			else
			{
				strFileName = strPermanentFileName;
			}

			// Create the file storage object
			HRESULT hr = waitForStgFileCreate(get_bstr_t(strFileName), &ipStorage, gdwSTORAGE_CREATE_MODE);
			if (ipStorage == __nullptr || FAILED(hr))
			{
				UCLIDException ue("ELI25588", "Unable to create file storage object.");
				ue.addDebugInfo("File name", strFileName);
				ue.addHresult(hr);
				throw ue;
			}

			try
			{
				// Create a stream within the storage object to store the object
				hr = ipStorage->CreateStream(bstrObjectName, gdwSTREAM_CREATE_MODE, 0, 0, &ipStream);
				if (ipStream == __nullptr || FAILED(hr))
				{
					UCLIDException ue("ELI25589", "Unable to create stream object.");
					ue.addDebugInfo("File name", strFileName);
					ue.addDebugInfo("Stream name", asString(bstrObjectName));
					ue.addHresult(hr);
					throw ue;
				}

				// Write file signature to stream if specified
				if (!strSignature.empty())
				{
					CComBSTR bstrSignature(strSignature.c_str());
					bstrSignature.WriteToStream(ipStream);
				}

				// Save the object to the stream
				hr = ipObject->Save(ipStream, asMFCBool(bClearDirty));
				if (FAILED(hr))
				{
					string strMessage = "Unable to save " + asString(bstrObjectName) + ".";
					HANDLE_HRESULT(hr, "ELI25590", strMessage, ipObject, IID_IPersistStream);
				}

				// Ensure the stream and storage are closed [LRCAU #5078]
				ipStream = __nullptr;
				ipStorage = __nullptr;

				// If we wrote to a temporary file, now the write succeeded, overwrite the original
				// file with the temp copy.
				if (pTempOutFile != __nullptr)
				{
					try
					{
						try
						{
							copyFile(strFileName, strPermanentFileName);
						}
						CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI34201");
					}
					catch (UCLIDException& ue)
					{
						throw UCLIDException("ELI34202",
							"Unable to save " + strPermanentFileName + ".", ue);
					}

					delete pTempOutFile;
					pTempOutFile = __nullptr;
				}
			}
			catch (...)
			{
				// Ensure the stream and storage are closed [LRCAU #5078]
				ipStream = __nullptr;
				ipStorage = __nullptr;

				// The object could not be streamed successfully. Delete the output file.
				try
				{
					if (isValidFile(strFileName))
					{
						deleteFile(strFileName);
					}
				}
				CATCH_AND_LOG_ALL_EXCEPTIONS("ELI27023")

				throw;
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25597")
	}
	catch (UCLIDException &ue)
	{
		if (pTempOutFile != __nullptr)
		{
			delete pTempOutFile;
		}

		throw ue;
	}
}

//-------------------------------------------------------------------------------------------------
// Unexported functions
//-------------------------------------------------------------------------------------------------
void readStorageFromUnencryptedFile(IStorage** ppStorage, BSTR bstrFileName)
{
	ASSERT_ARGUMENT("ELI25411", ppStorage != __nullptr);

	STGOPTIONS options = {gSTORAGE_VERSION};
	HRESULT hr = StgOpenStorageEx(bstrFileName, gdwSTORAGE_ACCESS_MODE, STGFMT_DOCFILE, 0, 
		&options, 0, IID_IStorage, (void**)ppStorage);
	if (hr == STG_E_SHAREVIOLATION)
	{
		// If open failed due to share violation, wait and retry [LRCAU #5090] 
		hr = waitForStgFileAccess(bstrFileName, ppStorage);
	}
	if (*ppStorage == NULL || FAILED(hr))
	{
		UCLIDException ue("ELI25392", "Unable to open file storage object.");
		ue.addDebugInfo("File name", asString(bstrFileName));
		ue.addDebugInfo("Pointer Value", *ppStorage == NULL ? "Null" : "Not Null");
		ue.addHresult(hr);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void readStorageFromEncryptedFile(IStorage** ppStorage, BSTR bstrFileName)
{
	ASSERT_ARGUMENT("ELI25412", ppStorage != __nullptr);

	string strFileName = asString(bstrFileName);

	// Create the EncryptedFileManager
	MapLabelManager encryptedFileManager;
	unsigned long ulByteCount = 0;
	unsigned long ulWritten = 0;
	unsigned char* pucDecryptedBytes = NULL;
	
	// Ensure the decrypted bytes are cleaned up
	try
	{
		// Decrypt the encrypted file
		pucDecryptedBytes = encryptedFileManager.getMapLabel(strFileName, &ulByteCount);

		// Create the LockBytes object
		ILockBytesPtr ipLockBytes;
		HRESULT hr = CreateILockBytesOnHGlobal(NULL, TRUE, &ipLockBytes);
		if (FAILED(hr))
		{
			UCLIDException ue("ELI25401", "Unable to allocate memory for storage object.");
			ue.addDebugInfo("File name", strFileName);
			ue.addHresult(hr);
			throw ue;
		}

		// Write the decrypted file bytes into the memory allocated to the LockBytes object
		ULARGE_INTEGER ulOffset;
		ulOffset.QuadPart = 0;
		hr = ipLockBytes->WriteAt(ulOffset, pucDecryptedBytes, ulByteCount, &ulWritten);
		if (FAILED(hr))
		{
			UCLIDException ue("ELI25402", "Unable to write memory for storage object.");
			ue.addDebugInfo("File name", strFileName);
			ue.addHresult(hr);
			throw ue;
		}

		// Open the storage object on the LockBytes
		hr = StgOpenStorageOnILockBytes(ipLockBytes, NULL, gdwSTORAGE_ACCESS_MODE, NULL, 0, ppStorage);
		if (*ppStorage == NULL || FAILED(hr))
		{
			UCLIDException ue("ELI25403", "Failed opening storage object.");
			ue.addDebugInfo("File name", strFileName);
			ue.addHresult(hr);
			throw ue;
		}
	}
	catch (...)
	{
		if (pucDecryptedBytes != __nullptr)
		{
			delete [] pucDecryptedBytes;
			pucDecryptedBytes = NULL;
		}

		throw;
	}

	// Clear the memory from the file decryption
	if (pucDecryptedBytes != __nullptr)
	{
		delete [] pucDecryptedBytes;
		pucDecryptedBytes = NULL;
	}

	// Check to be sure we did read and write the same # of bytes
	if (ulWritten != ulByteCount)
	{
		UCLIDException ue("ELI25404", "Failed opening encrypted file.");
		ue.addDebugInfo("File name", strFileName);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
IPersistStreamPtr EXPORT_BaseUtils readObjFromSAFEARRAY(SAFEARRAY *psaData)
{
	ASSERT_ARGUMENT("ELI37203", psaData != __nullptr);

	// Verify that the SAFEARRAY is of appropriate type
	if (psaData->cbElements != sizeof(BYTE))
	{
		UCLIDException ue("ELI37201", "SAFEARRAY must have element size of 1.");
		ue.addDebugInfo("ElementSize", psaData->cbElements);
		throw ue;
	}
	if (psaData->cDims != 1)
	{
		UCLIDException ue("ELI37202", "SAFEARRAY must have only 1 dimension.");
		ue.addDebugInfo("Dimensions", psaData->cDims);
		throw ue;
	}

	SafeArrayAccessGuard<BYTE> saGuard(psaData);
	BYTE *pBytes = saGuard.AccessData();

	long lowerBound, upperBound;
	unsigned long ulCount;
	SafeArrayGetLBound(psaData, 1, &lowerBound);
	SafeArrayGetUBound(psaData, 1, &upperBound);
	ulCount = upperBound - lowerBound + 1;

	// create a temporary IStream object
	IStreamPtr ipStream;
	HANDLE_HRESULT(CreateStreamOnHGlobal(NULL, TRUE, &ipStream),
		"ELI37184", "Unable to create stream object!", ipStream, IID_IStream);

	// Write the buffer to the stream
	ipStream->Write(&(pBytes[lowerBound]), ulCount, NULL);

	// Reset the stream current position to the beginning of the stream
	LARGE_INTEGER zeroOffset;
	zeroOffset.QuadPart = 0;
	ipStream->Seek(zeroOffset, STREAM_SEEK_SET, NULL);

	// Stream the object out of the IStream
	IPersistStreamPtr ipPersistObj;
	readObjectFromStream(ipPersistObj, ipStream, "ELI37187");

	ipStream = __nullptr;

	// Done with the data pointer
	saGuard.UnaccessData();

	// Return the object
	return ipPersistObj;
}
//-------------------------------------------------------------------------------------------------
LPSAFEARRAY EXPORT_BaseUtils writeObjToSAFEARRAY(IPersistStreamPtr ipObj)
{
	// create a temporary IStream object
	IStreamPtr ipStream;
	HANDLE_HRESULT(CreateStreamOnHGlobal(NULL, TRUE, &ipStream),
		"ELI37189", "Unable to create stream object!", ipStream, IID_IStream);

	writeObjectToStream(ipObj, ipStream, "ELI37190", FALSE);

	// Set the stream seek pointer back to the beginning of the stream
	LARGE_INTEGER disp;
	disp.QuadPart = 0;
	ULARGE_INTEGER newPos;
	ULARGE_INTEGER dataSize;
	
	// Get the size of the stream in newPos;
	ipStream->Seek(disp, STREAM_SEEK_END, &dataSize);

	CComSafeArray<BYTE> saData(dataSize.LowPart, 1);

	ipStream->Seek(disp, STREAM_SEEK_SET, &newPos);

	SafeArrayAccessGuard<BYTE> saGuard(*(saData.GetSafeArrayPtr()));
	BYTE *pBytes = saGuard.AccessData();

	ULONG ulBytesRead;
	ipStream->Read(pBytes, dataSize.LowPart, &ulBytesRead);

	if (dataSize.LowPart == ulBytesRead)
	{
		saGuard.UnaccessData();
		return saData.Detach();
	}

	// The wrong number of bytes was read from the stream
	UCLIDException ueRead("ELI37191", "Unexpected number of bytes read.");
	ueRead.addDebugInfo("Expected", dataSize.LowPart);
	ueRead.addDebugInfo("ActualRead", ulBytesRead);
	throw ueRead;
}
//-------------------------------------------------------------------------------------------------
void initRegisteredObjectsBase(long objectCode)
{
	if (m_sapnRegisteredObjectKey)
	{
		// If init is called when already initialized, it is an indication that registerObjectBase
		// has been called from somewhere other than LicenseManagement::registerObject. Corrupt the
		// license state of the software.
		m_snRegisteredObjectCount = -1;
		THROW_LOGIC_ERROR_EXCEPTION("ELI38727");
	}

	m_sapnRegisteredObjectKey.reset(new long(objectCode));
	m_sapnRegisteredObject.reset(nullptr);
}
//-------------------------------------------------------------------------------------------------
void registerObjectBase(long objectCode)
{
	m_snRegisteredObjectCount++;

	if (!m_sapnRegisteredObjectKey)
	{
		// If registerObjectBase is called when not initialized, it is an indication that it has
		// been called from somewhere other than LicenseManagement::registerObject. Corrupt the
		// license state of the software.
		m_snRegisteredObjectCount = -1;
		THROW_LOGIC_ERROR_EXCEPTION("ELI38728");
	}

	// The objectCode will have been disguised with m_sapnRegisteredObjectKey.
	m_sapnRegisteredObject.reset(new long(objectCode));
}
//-------------------------------------------------------------------------------------------------
void validateObjectRegistration(long objectCode)
{
	try
	{
		try
		{
			if (!m_sapnRegisteredObjectKey)
			{
				// If validateObjectRegistration is called when not initialized, it is an indication
				// that it has been called from somewhere other than
				// LicenseManagement::registerObject. Corrupt the license state of the software.
				m_snRegisteredObjectCount = -1;
				THROW_LOGIC_ERROR_EXCEPTION("ELI38729");
			}

			// So that 3rd party code can't simply call registerObjectBase to impersonate a trusted
			// SecureObjectCreator, m_sapnRegisteredObject will have been XOR'd with the low and
			// high longs from an encrypted day code (LICENSE_MGMT_PASSWORD).
			long key = *m_sapnRegisteredObjectKey ^ objectCode;
			bool valid = (m_sapnRegisteredObject && *m_sapnRegisteredObject == key);
			if (!valid)
			{
				throw UCLIDException("ELI38730", "Object validation failed.");
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI38731");
	}
	catch (UCLIDException &ue)
	{
		// It is expected that in response to an error, SecureObjectCreator (via 
		// CREATE_VERIFIED_OBJECT) will trigger initRegisteredObjectsBase.
		m_sapnRegisteredObjectKey = nullptr;
		
		// Prevent brute-force attempts at cracking m_sapnRegisteredObjectKey.
		Sleep(1000);

		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------

