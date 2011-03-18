
#include "stdafx.h"
#include "COMUtils.h"
#include "cpputil.h"
#include "EncryptedFileManager.h"
#include "UCLIDException.h"

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
			// description.  This is infact a requirement for calling this
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
			ipObj = NULL;

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
	/* This was the original reason this function was used it is no longer required
	Per Andrew email 12-13-2004
	bstr_t get_bstr_t(string str);
	_bstr_t get_bstr_t(const char* szStr);

	Why is this important?
	- It has been discovered that _bstr_t cannot (for some unknown and
	unexplained my microsoft reason) handle conversion of very long character
	arrays.  We have thousands of calls in our code that look like this:

	string strText;
	... strText is assigned to some huge string ...
	ipComponent->SomeComMethod(_bstr_t(strText.c_str()), bOtherParams);

	If strText is large, for instance one of our XML Files, the constructor
	throws an exception.  An exception is also thrown in cases like the
	following.

	string strText;
	... strText is assigned to some huge string ...
	_bstr_t _bstr;
	_bstr = strText;

	>From now on ALWAYS use get_bstr_t(xxx) when you need to convert to a BSTR or
	_bstr_t.
	In addition we will need to systematically make this change everywhere in
	our software (roughly 2700 places).

	long nLen = strlen(szStr)+1;
	WCHAR *wstr = new WCHAR[nLen]; // This was changed from auto_ptr since pointers to arrays should not be using auto_ptr's
	MultiByteToWideChar(CP_ACP, 0, szStr, nLen, wstr, nLen );
	_bstr_t _bstr(wstr);
	
	delete [] wstr;
	return _bstr;
	*/
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
_bstr_t EXPORT_BaseUtils writeObjectToBSTR(IPersistStreamPtr& ipObj, BOOL bClearDirty)
{
	// Create a stream
	IStreamPtr ipStream = NULL;
	CreateStreamOnHGlobal(NULL, TRUE, &ipStream);
	if (ipStream == NULL)
	{
		throw UCLIDException("ELI06691", "Unable to create stream object!");
	}

	// Save the object to the stream
	ASSERT_RESOURCE_ALLOCATION("ELI11062", ipObj != NULL);
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
	// All BSTR's contain an even nuber of bytes because each "character" is unicode i.e. 2 bytes long.
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
	IStreamPtr ipStream = NULL;
	CreateStreamOnHGlobal(NULL, TRUE, &ipStream);

	if (ipStream == NULL)
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
	ASSERT_ARGUMENT("ELI20263", ipObj != NULL);

	// create a stream object
	IStreamPtr ipStream;
	CreateStreamOnHGlobal(NULL, TRUE, &ipStream);
	ASSERT_RESOURCE_ALLOCATION("ELI20262", ipStream != NULL);

	// clear the dirty flag by saving the object to the stream
	HANDLE_HRESULT(ipObj->Save(ipStream, TRUE), "ELI20260", "Unable to clear dirty flag.", ipObj, 
		IID_IPersistStream);

	// ignore the stream result
}
//-------------------------------------------------------------------------------------------------
HRESULT EXPORT_BaseUtils waitForStgFileAccess(BSTR strFileName, IStorage** ppStorage)
{
	// Ensure the file name is not empty string
	ASSERT_ARGUMENT("ELI23899", strFileName != NULL && _bstr_t(strFileName, true).length() > 0);

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
		IStoragePtr ipStorage = NULL;
		STGOPTIONS options = {gSTORAGE_VERSION};
		StgOpenStorageEx(strFileName, gdwSTORAGE_ACCESS_MODE, STGFMT_DOCFILE, 0, &options, 
			0, IID_IStorage, (void**)&ipStorage);

		// Check the return value
		bRtnValue = hr == S_OK;

		// If the file was successfully opened AND pStorage != NULL then
		// set *ppStorage to the open storage object
		if (bRtnValue && ppStorage != NULL)
		{
			*ppStorage = ipStorage.Detach();
		}
		else
		{
			ipStorage = NULL;
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
	ASSERT_ARGUMENT("ELI24711", strFileName != NULL && _bstr_t(strFileName, true).length() > 0);
	ASSERT_ARGUMENT("ELI24712", ppStorage != NULL);

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
		IStoragePtr ipStorage = NULL;
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
			ipStorage = NULL;
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
	ipStream = NULL;
	ipStorage = NULL;
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
	ASSERT_ARGUMENT("ELI25409", ppStream != NULL);
	ASSERT_ARGUMENT("ELI25410", ipStorage != NULL);

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
				  bool bClearDirty, string strSignature)
{
	try
	{
		ASSERT_ARGUMENT("ELI25598", ipObject != NULL);

		// Create a directory for the file if necessary
		string strFileName = asString(bstrFileName);
		createDirectory( getDirectoryFromFullPath(strFileName) );

		// Create the file storage object
		IStoragePtr ipStorage;
		HRESULT hr = waitForStgFileCreate(bstrFileName, &ipStorage, gdwSTORAGE_CREATE_MODE);
		if (ipStorage == NULL || FAILED(hr))
		{
			UCLIDException ue("ELI25588", "Unable to create file storage object.");
			ue.addDebugInfo("File name", strFileName);
			ue.addHresult(hr);
			throw ue;
		}

		try
		{
			// Create a stream within the storage object to store the object
			IStreamPtr ipStream;
			hr = ipStorage->CreateStream(bstrObjectName, gdwSTREAM_CREATE_MODE, 0, 0, &ipStream);
			if (ipStream == NULL || FAILED(hr))
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
			ipStream = NULL;
			ipStorage = NULL;
		}
		catch (...)
		{
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

//-------------------------------------------------------------------------------------------------
// Unexported functions
//-------------------------------------------------------------------------------------------------
void readStorageFromUnencryptedFile(IStorage** ppStorage, BSTR bstrFileName)
{
	ASSERT_ARGUMENT("ELI25411", ppStorage != NULL);

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
	ASSERT_ARGUMENT("ELI25412", ppStorage != NULL);

	string strFileName = asString(bstrFileName);

	// Create the EncryptedFileManager
	EncryptedFileManager efm;
	unsigned long ulByteCount = 0;
	unsigned long ulWritten = 0;
	unsigned char* pucDecryptedBytes = NULL;
	
	// Ensure the decrypted bytes are cleaned up
	try
	{
		// Decrypt the encrypted file
		pucDecryptedBytes = efm.decryptBinaryFile(strFileName, &ulByteCount);

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
		if (pucDecryptedBytes != NULL)
		{
			delete [] pucDecryptedBytes;
			pucDecryptedBytes = NULL;
		}

		throw;
	}

	// Clear the memory from the file decryption
	if (pucDecryptedBytes != NULL)
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
