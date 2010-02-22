// FilterIDShieldDataFileTask.cpp : Implementation of CFilterIDShieldDataFileTask
#include "stdafx.h"
#include "RedactionCustomComponents.h"
#include "FilterIDShieldDataFileTask.h"
#include "RedactionCCUtils.h"
#include "RedactionCCConstants.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>
#include <StringTokenizer.h>

#include <string>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCURRENT_VERSION = 1;

//-------------------------------------------------------------------------------------------------
// CFilterIDShieldDataFileTask
//-------------------------------------------------------------------------------------------------
CFilterIDShieldDataFileTask::CFilterIDShieldDataFileTask()
:	m_strVOAFileToRead(""),
	m_strVOAFileToWrite(""),
	m_bDirty(false)
{
}
//-------------------------------------------------------------------------------------------------
CFilterIDShieldDataFileTask::~CFilterIDShieldDataFileTask()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI24783");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFilterIDShieldDataFileTask::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IFilterIDShieldDataFileTask,
		&IID_IFileProcessingTask,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_IMustBeConfiguredObject,
		&IID_ILicensedComponent

	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IFileProcessingTask
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFilterIDShieldDataFileTask::raw_Init(long nActionID, IFAMTagManager* pFAMTM,
	IFileProcessingDB *pDB)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		// nothing to do
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24784");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFilterIDShieldDataFileTask::raw_ProcessFile(BSTR bstrFileFullName, long nFileID,
	long nActionID, IFAMTagManager *pTagManager, IFileProcessingDB *pDB, IProgressStatus *pProgressStatus,
	VARIANT_BOOL bCancelRequested, EFileProcessingResult *pResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI24785", pResult != NULL);

		// Get the tag manager
		IFAMTagManagerPtr ipFamTagManager(pTagManager);
		ASSERT_RESOURCE_ALLOCATION("ELI24787", ipFamTagManager != NULL);

		// Default to successful completion
		*pResult = kProcessingSuccessful;

		// Get source doc
		string strSourceDoc = asString(bstrFileFullName);
		ASSERT_ARGUMENT("ELI24788", !strSourceDoc.empty());

		// Get the VOA file to read and ensure it exists
		string strVOAToRead = CRedactionCustomComponentsUtils::ExpandTagsAndTFE(ipFamTagManager,
			m_strVOAFileToRead, strSourceDoc);
		validateFileOrFolderExistence(strVOAToRead);
		
		// Get the VOA file to write
		string strVOAToWrite = CRedactionCustomComponentsUtils::ExpandTagsAndTFE(ipFamTagManager,
			m_strVOAFileToWrite, strSourceDoc);

		// Ensure that the input and output VOA files are different
		if (_stricmp(strVOAToRead.c_str(), strVOAToWrite.c_str()) == 0)
		{
			UCLIDException uex("ELI24849", "Input and output VOA files must be different files!");
			uex.addDebugInfo("Input VOA", m_strVOAFileToRead);
			uex.addDebugInfo("Output VOA", m_strVOAFileToWrite);
			uex.addDebugInfo("Source Document", strSourceDoc);
			uex.addDebugInfo("Input/Output Expanded", strVOAToRead);
			throw uex;
		}

		// Load the attibutes from the VOA file
		IIUnknownVectorPtr ipAttributesToFilter(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI24789", ipAttributesToFilter != NULL);
		ipAttributesToFilter->LoadFrom(strVOAToRead.c_str(), VARIANT_FALSE);

		// Create a new collection of attributes to hold the filtered set
		IIUnknownVectorPtr ipNewAttributes(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI24790", ipNewAttributes != NULL);

		// Loop through each attribute and add it to the new collection if it matches
		// a data type in the filter
		long lNumberOfAttributes = ipAttributesToFilter->Size();
		for (long i=0; i < lNumberOfAttributes; i++)
		{
			IAttributePtr ipAttribute = ipAttributesToFilter->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI24791", ipAttribute != NULL);

			// Get the type as a lower case string (case insensitive compare)
			string strType = asString(ipAttribute->Type);
			makeLowerCase(strType);

			// Get the name as a lower case string (case insensitive compare)
			string strName = asString(ipAttribute->Name);
			makeLowerCase(strName);
			
			// Add this attribute to the new vector if:
			// A) This attribute type is in the filter list
			// B) This attribute is the document type tag [FlexIDSCore #3451]
			if (m_setDataTypes.find(strType) != m_setDataTypes.end() || strName == "documenttype")
			{
				ipNewAttributes->PushBack(ipAttribute);
			}
		}

		// Now save the filtered attributes
		ipNewAttributes->SaveTo(strVOAToWrite.c_str(), TRUE);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24792")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFilterIDShieldDataFileTask::raw_Cancel()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// nothing to do
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24793");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFilterIDShieldDataFileTask::raw_Close()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// nothing to do
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24794");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFilterIDShieldDataFileTask::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		ASSERT_ARGUMENT("ELI24795", pbValue != NULL);

		try
		{
			// Check license
			validateLicense();

			// If no exception, then pbValue is true
			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24796");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFilterIDShieldDataFileTask::raw_GetComponentDescription(BSTR * pbstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI24797", pbstrComponentDescription);

		*pbstrComponentDescription = _bstr_t("Redaction: Filter ID Shield data file").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24798");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFilterIDShieldDataFileTask::raw_Clone(IUnknown * * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		// create another instance of this object
		ICopyableObjectPtr ipObjCopy(CLSID_FilterIDShieldDataFileTask);
		ASSERT_RESOURCE_ALLOCATION("ELI24799", ipObjCopy != NULL);

		IUnknownPtr ipUnk(this);
		ipObjCopy->CopyFrom(ipUnk);
	
		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24800");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFilterIDShieldDataFileTask::raw_CopyFrom(IUnknown * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();
		UCLID_REDACTIONCUSTOMCOMPONENTSLib::IFilterIDShieldDataFileTaskPtr ipSource(  pObject );
		ASSERT_RESOURCE_ALLOCATION("ELI24801", ipSource != NULL);

		// Copy data from the source
		m_strVOAFileToRead = asString(ipSource->VOAFileToRead);
		fillDataTypeSet(ipSource->DataTypes);
		m_strVOAFileToWrite = asString(ipSource->VOAFileToWrite);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24802");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFilterIDShieldDataFileTask::raw_IsConfigured(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI24803", pbValue != NULL);

		// Configured if:
		// 1. an input file
		// 2. output file
		// 3. at least 1 data type selected
		bool bConfigured = (!m_strVOAFileToRead.empty()
			&& !m_strVOAFileToWrite.empty()
			&& m_setDataTypes.size() > 0);
			
		*pbValue = asVariantBool(bConfigured);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24804");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IFilterIDShieldDataFileTask
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFilterIDShieldDataFileTask::put_VOAFileToRead(BSTR bstrVOAFileName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		validateLicense();
			
		string strFileName = asString(bstrVOAFileName);

		// Create a local IFAMTagManagerPtr object
		UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr ipFAMTagManager(CLSID_FAMTagManager);
		ASSERT_RESOURCE_ALLOCATION("ELI24805", ipFAMTagManager != NULL);

		// Make sure the file name contains valid string tags
		if (ipFAMTagManager->StringContainsInvalidTags(strFileName.c_str()) == VARIANT_TRUE)
		{
			UCLIDException ue("ELI24806", "The voa file name contains invalid tags!");
			ue.addDebugInfo("VOA File", strFileName);
			throw ue;
		}

		// Assign the VOA file to read 
		m_strVOAFileToRead = strFileName;

		// Set the dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24807");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFilterIDShieldDataFileTask::get_VOAFileToRead(BSTR *pbstrVOAFileName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI24808", pbstrVOAFileName != NULL);

		*pbstrVOAFileName = _bstr_t(m_strVOAFileToRead.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24809")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFilterIDShieldDataFileTask::put_DataTypes(IVariantVector* pDataTypes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// Get the data types vector
		IVariantVectorPtr ipDataTypes(pDataTypes);
		ASSERT_RESOURCE_ALLOCATION("ELI24810", ipDataTypes != NULL);

		// Fill the internal set with the data types
		fillDataTypeSet(ipDataTypes);

		// Set the dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24811");
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFilterIDShieldDataFileTask::get_DataTypes(IVariantVector** ppDataTypes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI24812", ppDataTypes != NULL);

		// Get the data types as a variant vector and return it
		*ppDataTypes = getDataTypesAsVariantVector().Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24813");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFilterIDShieldDataFileTask::put_VOAFileToWrite(BSTR bstrVOAFileName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		validateLicense();
			
		string strFileName = asString(bstrVOAFileName);

		// Create a local IFAMTagManagerPtr object
		UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr ipFAMTagManager(CLSID_FAMTagManager);
		ASSERT_RESOURCE_ALLOCATION("ELI24814", ipFAMTagManager != NULL);

		// Make sure the file name contains valid string tags
		if (ipFAMTagManager->StringContainsInvalidTags(strFileName.c_str()) == VARIANT_TRUE)
		{
			UCLIDException ue("ELI24815", "The voa file name contains invalid tags!");
			ue.addDebugInfo("VOA File", strFileName);
			throw ue;
		}

		// Assign the VOA file to write 
		m_strVOAFileToWrite = strFileName;

		// Set the dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24816");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFilterIDShieldDataFileTask::get_VOAFileToWrite(BSTR *pbstrVOAFileName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI24817", pbstrVOAFileName != NULL);

		*pbstrVOAFileName = _bstr_t(m_strVOAFileToWrite.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24818")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFilterIDShieldDataFileTask::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pClassID = CLSID_FilterIDShieldDataFileTask;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFilterIDShieldDataFileTask::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFilterIDShieldDataFileTask::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Reset the object
		m_strVOAFileToRead = "";
		m_setDataTypes.clear();
		m_strVOAFileToWrite = "";

		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		pStream->Read(&nDataLength, sizeof(nDataLength), NULL);
		ByteStream data(nDataLength);
		pStream->Read(data.getData(), nDataLength, NULL);
		ByteStreamManipulator dataReader(ByteStreamManipulator::kRead, data);

		// Read the individual data items from the bytestream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;

		// Check for newer version
		if (nDataVersion > gnCURRENT_VERSION)
		{
			// Throw exception
			UCLIDException ue( "ELI24819", 
				"Unable to load newer Filter ID Shield Data File task!" );
			ue.addDebugInfo( "Current Version", gnCURRENT_VERSION );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		// Get the VOA file to read
		dataReader >> m_strVOAFileToRead;

		// Check if there were data types
		bool bDataTypes;
		dataReader >> bDataTypes;
		if (bDataTypes)
		{
			// Read the data types from the stream and fill the internal set
			string strDataTypes;
			dataReader >> strDataTypes;
			fillDataTypeSet(strDataTypes);
		}

		// Get the VOA file to write
		dataReader >> m_strVOAFileToWrite;

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24820");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFilterIDShieldDataFileTask::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter( ByteStreamManipulator::kWrite, data );
		dataWriter << gnCURRENT_VERSION;

		// Write the VOA file to read
		dataWriter << m_strVOAFileToRead;

		// Write the data filter values
		string strDataTypes = getDataTypesAsString();
		bool bDataTypes = !strDataTypes.empty();
		dataWriter << bDataTypes;
		if (bDataTypes)
		{
			dataWriter << strDataTypes;
		}

		// Write the VOA file to write
		dataWriter << m_strVOAFileToWrite;

		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write( &nDataLength, sizeof(nDataLength), NULL );
		pStream->Write( data.getData(), nDataLength, NULL );

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24821");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFilterIDShieldDataFileTask::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CFilterIDShieldDataFileTask::validateLicense()
{
	VALIDATE_LICENSE( gnIDSHIELD_CORE_OBJECTS, "ELI24822", "Filter ID Shield Data File Task" );
}
//-------------------------------------------------------------------------------------------------
string CFilterIDShieldDataFileTask::getDataTypesAsString()
{
	try
	{
		string strDataTypes = "";
		if (m_setDataTypes.size() > 0)
		{
			set<string>::iterator it = m_setDataTypes.begin();
			strDataTypes = *it;
			for (++it; it != m_setDataTypes.end(); it++)
			{
				strDataTypes += ",";
				strDataTypes += (*it);
			}
		}

		return strDataTypes;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI24823");
}
//-------------------------------------------------------------------------------------------------
IVariantVectorPtr CFilterIDShieldDataFileTask::getDataTypesAsVariantVector()
{
	try
	{
		IVariantVectorPtr ipVarTemp(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI24824", ipVarTemp != NULL);

		// Loop through the set of data types and add them to the variant vector
		for (set<string>::iterator it = m_setDataTypes.begin(); it != m_setDataTypes.end(); it++)
		{
			_variant_t varTemp(it->c_str());
			ipVarTemp->PushBack(varTemp);
		}

		return ipVarTemp;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI24825");
}
//-------------------------------------------------------------------------------------------------
void CFilterIDShieldDataFileTask::fillDataTypeSet(IVariantVectorPtr ipDataTypes)
{
	try
	{
		ASSERT_ARGUMENT("ELI24826", ipDataTypes != NULL);

		// Reset the set of data types
		m_setDataTypes.clear();

		// Loop through the vector and add each value to the internal set
		long lSize = ipDataTypes->Size;
		for (long i=0; i < lSize; i++)
		{
			// Get the string from the vector and make it upper case
			string strDataType = asString(ipDataTypes->Item[i].bstrVal);
			makeLowerCase(strDataType);
			m_setDataTypes.insert(strDataType);
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI24827");
}
//-------------------------------------------------------------------------------------------------
void CFilterIDShieldDataFileTask::fillDataTypeSet(const string& strDataTypes)
{
	try
	{
		// Reset the set of data types
		m_setDataTypes.clear();

		// Get the collection of data types from the comma separated string
		vector<string> vecTokens;
		StringTokenizer::sGetTokens(strDataTypes, ',', vecTokens);

		// Loop through the set of strings and add them to the set
		for (vector<string>::iterator it = vecTokens.begin(); it != vecTokens.end(); it++)
		{
			// Get each string from the vector, make it lower case and add to the set
			string strTemp = *it;
			makeLowerCase(strTemp);
			m_setDataTypes.insert(strTemp);
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI24828");
}
//-------------------------------------------------------------------------------------------------
