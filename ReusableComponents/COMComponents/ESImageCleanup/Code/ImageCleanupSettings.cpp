// ImageCleanupSettings.cpp : Implementation of CImageCleanupSettings

#include "stdafx.h"
#include "ImageCleanupSettings.h"

#include <UCLIDException.h>
#include <COMUtils.h>
#include <ComUtils.h>
#include <cpputil.h>
#include <EncryptedFileManager.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// version 2:
//	- Added string to hold a page range specification
//	- Added a type to indicate what type of range is held in the page range string
const unsigned long gnCurrentVersion = 2;

const string gstrIMAGE_CLEANUP_SETTINGS_SIGNATURE = "Extract Image Cleanup Operations (ICS) File";

//-------------------------------------------------------------------------------------------------
// CImageCleanupSettings
//-------------------------------------------------------------------------------------------------
CImageCleanupSettings::CImageCleanupSettings() :
m_bDirty(false),
m_bIsEncrypted(false),
m_bstrStreamName("ImageCleanupSettings"),
m_strSpecifiedPages(""),
m_eICPageRangeType(ESImageCleanupLib::kCleanAll),
m_ipMiscUtils(NULL)
{
}
//-------------------------------------------------------------------------------------------------
CImageCleanupSettings::~CImageCleanupSettings()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI17116");
}

//-------------------------------------------------------------------------------------------------
// ISupportErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageCleanupSettings::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IImageCleanupSettings,
		&IID_IPersistStream
	};

	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageCleanupSettings::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI17831", pClassID != __nullptr);

		*pClassID = CLSID_ImageCleanupSettings;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17221");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageCleanupSettings::IsDirty()
{
	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageCleanupSettings::Load(IStream * pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI17542", pStream != __nullptr);

		// read the signature from the file
		CComBSTR bstrSignature;
		HRESULT hr = bstrSignature.ReadFromStream(pStream);
		if (FAILED(hr))
		{
			UCLIDException ue("ELI17222", "Failed reading file signature from stream!");
			ue.addHresult(hr);
			throw ue;
		}
		
		// check the file signature
		if (asString(bstrSignature.m_str) != gstrIMAGE_CLEANUP_SETTINGS_SIGNATURE)
		{
			UCLIDException ue("ELI17117", "Invalid Image Cleanup Operations file!");
			ue.addDebugInfo("Signature", asString(bstrSignature.m_str));
			throw ue;
		}

		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		pStream->Read(&nDataLength, sizeof(nDataLength), NULL);
		ByteStream data(nDataLength);
		pStream->Read(data.getData(), nDataLength, NULL);
		ByteStreamManipulator dataReader(ByteStreamManipulator::kRead, data);

		// Read the individual data items from the bytestream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;

		// check the version stored in the file
		if (nDataVersion > gnCurrentVersion)
		{
			UCLIDException ue("ELI17118", "Unable to load newer ImageCleanupSettings.");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		// if version is greater than 1 then load the specified page string and the
		// specified range type
		if (nDataVersion > 1)
		{
			dataReader >> m_strSpecifiedPages;
			long tmp = 0;
			dataReader >> tmp;
			m_eICPageRangeType = (ESImageCleanupLib::EICPageRangeType) tmp;
		}

		// create IPersistStream object and fill from the opened stream
		IPersistStreamPtr ipObj;
		readObjectFromStream(ipObj, pStream, "ELI17119");
		if (ipObj == __nullptr)
		{
			throw UCLIDException("ELI17120", "Image Cleanup Operations could not be read from stream!");
		}

		// set our internal cleanup operations vector
		m_ipImageCleanupOperationsVector = ipObj;
		ASSERT_RESOURCE_ALLOCATION("ELI17224", m_ipImageCleanupOperationsVector != __nullptr);

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17121");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageCleanupSettings::Save(IStream * pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI17777", pStream != __nullptr);

		// write the signature to the file
		CComBSTR bstrSignature(gstrIMAGE_CLEANUP_SETTINGS_SIGNATURE.c_str());
		HRESULT hr = bstrSignature.WriteToStream(pStream);
		if (FAILED(hr))
		{
			UCLIDException ue("ELI17223", "Failed writing file signature to stream!");
			ue.addHresult(hr);
			throw ue;
		}

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		// store the current version number
		dataWriter << gnCurrentVersion;

		// store the other member variables
		dataWriter << m_strSpecifiedPages;
		dataWriter << (long) m_eICPageRangeType;

		// flush the bytestream
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		// get the IPersistStream associated with our cleanup operations vector
		IPersistStreamPtr ipObj = m_ipImageCleanupOperationsVector;
		ASSERT_RESOURCE_ALLOCATION("ELI17122", ipObj != __nullptr);

		// write the cleanup operations to the stream
		writeObjectToStream(ipObj, pStream, "ELI17123", fClearDirty);

		// Clear the flag as specified
		if (asCppBool(fClearDirty))
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17124");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageCleanupSettings::GetSizeMax(_ULARGE_INTEGER * pcbSize)
{
	if (pcbSize == NULL)
		return E_POINTER;
		
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// IImageCleanupSettings
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageCleanupSettings::LoadFrom(BSTR strFullFileName, VARIANT_BOOL bSetDirtyFlagToTrue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// clear the current settings
		clearSettings();

		// Convert the BSTR filename to std::string
		string strFileName = asString(strFullFileName);

		// Get the file extension and check if this file is encrypted
		string strExt = getExtensionFromFullPath(strFileName, true);
		bool bEncrypted = strExt == ".etf";

		// TODO: Implement autoencrypt functionality
		// if (bEncrypted)
		// {
		//   getMiscUtils()->AutoEncryptFile(strFullFileName, 
		//       get_bstr_t(gstrfCI_AUTO_ENCRYPT_KEY_PATH.c_str()));
		// }

		// Get the IPersistStream object
		IPersistStreamPtr ipPersistStream = getThisAsCOMPtr();
		ASSERT_RESOURCE_ALLOCATION("ELI17130", ipPersistStream != __nullptr);

		// Load the settings from the file
		readObjectFromFile(ipPersistStream, strFullFileName, m_bstrStreamName, bEncrypted);
		m_bIsEncrypted = bEncrypted;

		// Set dirty flag
		m_bDirty = asCppBool(bSetDirtyFlagToTrue);

		// Wait for the file to be accessible
		waitForFileAccess(strFileName, giMODE_READ_ONLY);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17132");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageCleanupSettings::SaveTo(BSTR strFullFileName, VARIANT_BOOL bClearDirtyFlag)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		writeObjectToFile(this, strFullFileName, m_bstrStreamName, asCppBool(bClearDirtyFlag));

		if (asCppBool(bClearDirtyFlag))
		{
			m_bDirty = false;
		}

		// Wait until the file is readable
		waitForStgFileAccess(strFullFileName);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17137");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageCleanupSettings::get_IsEncrypted(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		if (pVal == NULL)
		{
			return E_POINTER;
		}

		// return whether the loaded file was encrypted
		*pVal = asVariantBool(m_bIsEncrypted);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17138");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageCleanupSettings::get_ImageCleanupOperations(IIUnknownVector** pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		if (pVal == NULL)
		{
			return E_POINTER;
		}

		// if the operations vector has not been instantiated yet, instantiate it
		if (m_ipImageCleanupOperationsVector == __nullptr)
		{
			m_ipImageCleanupOperationsVector.CreateInstance(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI17139", m_ipImageCleanupOperationsVector != __nullptr);
		}

		// make a copy of the vector and return it
		IIUnknownVectorPtr ipShallowCopy = m_ipImageCleanupOperationsVector;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17140");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageCleanupSettings::put_ImageCleanupOperations(IIUnknownVector* pNewVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		if (pNewVal == NULL)
		{
			return E_POINTER;
		}

		m_ipImageCleanupOperationsVector = pNewVal;
		ASSERT_RESOURCE_ALLOCATION("ELI17141", "Unrecognized vector of cleanup operations!");

		// new operations, set dirty flag to true
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17142");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageCleanupSettings::Clear()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		clearSettings();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17091");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageCleanupSettings::get_SpecifiedPages(BSTR* pstrSpecifiedPages)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI17511", pstrSpecifiedPages != __nullptr);

		*pstrSpecifiedPages = get_bstr_t(m_strSpecifiedPages).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17509");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageCleanupSettings::put_SpecifiedPages(BSTR strSpecifiedPages)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_strSpecifiedPages = asString(strSpecifiedPages);

		// new page range, set dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17510");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageCleanupSettings::get_ICPageRangeType(EICPageRangeType *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI17513", pVal != __nullptr);

		*pVal = (EICPageRangeType) m_eICPageRangeType;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17514");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageCleanupSettings::put_ICPageRangeType(EICPageRangeType newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_eICPageRangeType = (ESImageCleanupLib::EICPageRangeType) newVal;

		// new type of page range, set dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17515");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
ESImageCleanupLib::IImageCleanupSettingsPtr CImageCleanupSettings::getThisAsCOMPtr()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ESImageCleanupLib::IImageCleanupSettingsPtr ipThis(this);
		ASSERT_RESOURCE_ALLOCATION("ELI17143", ipThis != __nullptr);

		return ipThis;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI17543");
}
//-------------------------------------------------------------------------------------------------
IMiscUtilsPtr CImageCleanupSettings::getMiscUtils()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// if the MiscUtils pointer has not been instantiated, instantiate it
		if (m_ipMiscUtils == __nullptr)
		{
			m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
			ASSERT_RESOURCE_ALLOCATION("ELI17145", m_ipMiscUtils != __nullptr);
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI17544");

	// return the MiscUtils pointer
	return m_ipMiscUtils;
}
//-------------------------------------------------------------------------------------------------
void CImageCleanupSettings::clearSettings()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// clear the elements in the vector if the vector exists
		if (m_ipImageCleanupOperationsVector != __nullptr)
		{
			m_ipImageCleanupOperationsVector->Clear();
		}

		// reset the encrypted flag
		m_bIsEncrypted = false;

		// reset the specified page range string
		m_strSpecifiedPages = "";

		// reset the range type to all
		m_eICPageRangeType = ESImageCleanupLib::kCleanAll;

		// reset the dirty flag
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI17545");
}
//-------------------------------------------------------------------------------------------------