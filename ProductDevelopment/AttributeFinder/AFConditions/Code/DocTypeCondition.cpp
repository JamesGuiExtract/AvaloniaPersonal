// DocTypeCondition.cpp : Implementation of CDocTypeCondition
#include "stdafx.h"
#include "AFConditions.h"
#include "DocTypeCondition.h"

#include <AFCategories.h>
#include <SpecialStringDefinitions.h>
#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ByteStream.h>
#include <ByteStreamManipulator.h>
#include <cpputil.h>
#include <ComponentLicenseIDs.h>
#include <SpecialStringDefinitions.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 3;

//-------------------------------------------------------------------------------------------------
// CDocTypeCondition
//-------------------------------------------------------------------------------------------------
CDocTypeCondition::CDocTypeCondition()
{
	try
	{
		clear();

		m_ipAFUtility.CreateInstance(CLSID_AFUtility);
		ASSERT_RESOURCE_ALLOCATION("ELI10826", m_ipAFUtility != NULL);
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI10772");
}
//-------------------------------------------------------------------------------------------------
CDocTypeCondition::~CDocTypeCondition()
{
	try
	{
		m_ipTypes = NULL;
		m_ipAFUtility = NULL;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16296");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocTypeCondition::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IDocTypeCondition,
		&IID_IAFCondition,
		&IID_IPersistStream,
		&IID_ICategorizedComponent,
		&IID_ISpecifyPropertyPages,
		&IID_ICopyableObject,
		&IID_IMustBeConfiguredObject,
		&IID_ILicensedComponent
	};

	for (int i = 0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
		{
			return S_OK;
		}
	}

	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IDocTypeCondition
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocTypeCondition::get_Types(IVariantVector **ppVec)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI26040", ppVec != NULL);

		// Check licensing
		validateLicense();

		IVariantVectorPtr ipTemp = m_ipTypes;
		*ppVec = ipTemp.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10795")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocTypeCondition::put_Types(IVariantVector *pVec)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();
		m_ipTypes = pVec;
		ASSERT_RESOURCE_ALLOCATION("ELI10802", m_ipTypes != NULL);

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10796")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocTypeCondition::get_AllowTypes(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();
		*pVal = asVariantBool(m_bAllowTypes);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10797")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocTypeCondition::put_AllowTypes(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();
		m_bAllowTypes = asCppBool(newVal);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10798")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocTypeCondition::get_MinConfidence(EDocumentConfidenceLevel* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI26062", pVal != NULL);

		// Check licensing
		validateLicense();
		*pVal = m_eMinConfidence;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11059")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocTypeCondition::put_MinConfidence(EDocumentConfidenceLevel newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();
		m_eMinConfidence = newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11060")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocTypeCondition::get_Category(BSTR* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI26063", pVal != NULL);

		// Check licensing
		validateLicense();

		*pVal = _bstr_t( m_strCategory.c_str() ).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11892")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocTypeCondition::put_Category(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// Validate string length
		string strNewVal = asString(newVal);
		if (strNewVal.empty())
		{
			UCLIDException ue("ELI11891", "Cannot specify an empty category name.");
			throw ue;
		}

		m_strCategory = strNewVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11893")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IAFCondition
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocTypeCondition::raw_ProcessCondition(IAFDocument *pAFDoc, VARIANT_BOOL* pbRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		IAFDocumentPtr ipAFDoc(pAFDoc);
		ASSERT_RESOURCE_ALLOCATION("ELI10825", ipAFDoc != NULL);

		// Check the document probability
		EDocumentConfidenceLevel eConfidence = kSureLevel;
		IStrToStrMapPtr ipStrMap = ipAFDoc->GetStringTags();

		if (ipStrMap->Contains(DOC_PROBABILITY.c_str()) == VARIANT_TRUE)
		{
			// Retrieve the value
			string strValue = asString(ipStrMap->GetValue(DOC_PROBABILITY.c_str()));
			long lValue = asLong( strValue );
			eConfidence = (EDocumentConfidenceLevel)lValue;
		}

		bool bInList = false;

		// only bother to check the document types if the probability is close
		if (eConfidence >= m_eMinConfidence)
		{
			IStrToObjectMapPtr ipDocTags(ipAFDoc->ObjectTags);
			ASSERT_RESOURCE_ALLOCATION("ELI30096", ipDocTags != NULL);

			// get the vector of document type names
			IVariantVectorPtr ipVecDocTypes = ipDocTags->TryGetValue(DOC_TYPE.c_str());
			ASSERT_RESOURCE_ALLOCATION("ELI30097", ipVecDocTypes != NULL);

			// See if any of the document types are in our list
			long nDocTypeCount = ipVecDocTypes->Size;
			for (long i = 0; !bInList && i < nDocTypeCount; i++)
			{
				string strDocType = asString(ipVecDocTypes->GetItem(i).bstrVal);

				long nCount = m_ipTypes->Size;
				for (long j = 0; j < nCount; j++)
				{	
					_variant_t varType = m_ipTypes->GetItem(j);
					_bstr_t bstrType = varType.bstrVal;
					string strListDocType = asString(bstrType);

					if (strDocType == strListDocType ||
						(nDocTypeCount == 1 && strListDocType == gstrSPECIAL_ANY_UNIQUE) ||
						(nDocTypeCount > 1 && strListDocType == gstrSPECIAL_MULTIPLE_CLASS))
					{
						bInList = true;
						break;
					}
				}
			}
		}
		else if(eConfidence == kZeroLevel)
		{
			long nCount = m_ipTypes->Size;

			for (long i = 0; i < nCount; i++)
			{	
				_variant_t varType = m_ipTypes->GetItem(i);
				_bstr_t bstrType = varType.bstrVal;
				string strListDocType = asString(bstrType);

				if(strListDocType == gstrSPECIAL_UNKNOWN)
				{
					bInList = true;
					break;
				}
			}
		}

		// Does the user want the doc to match or not
		if(!m_bAllowTypes)
		{
			bInList = !bInList;
		}

		*pbRetVal = bInList ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10773")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocTypeCondition::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		if (pbValue == NULL)
		{
			return E_POINTER;
		}

		// Check license
		validateLicense();

		// If no exception, then pbValue is true
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocTypeCondition::raw_IsConfigured(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI26064", pbValue != NULL);

		// Check license
		validateLicense();
		bool bIsConfigured = true;

		if(m_ipTypes == NULL || m_ipTypes->Size <= 0)
		{
			bIsConfigured = false;
		}

		*pbValue = asVariantBool(bIsConfigured);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10774");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocTypeCondition::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19590", pstrComponentDescription != NULL)

		*pstrComponentDescription = _bstr_t("Document type condition").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10775")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocTypeCondition::raw_CopyFrom(IUnknown *pObject)
{

	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_AFCONDITIONSLib::IDocTypeConditionPtr ipSource = pObject;
		ASSERT_RESOURCE_ALLOCATION( "ELI10776", ipSource != NULL );
		
		m_ipTypes = ipSource->Types;
		
		m_bAllowTypes = asCppBool(ipSource->AllowTypes);
		
		m_eMinConfidence = ipSource->MinConfidence;
		
		m_strCategory = asString(ipSource->Category);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12854");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocTypeCondition::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		// Create another instance of this object
		ICopyableObjectPtr ipObjCopy(CLSID_DocTypeCondition);
		ASSERT_RESOURCE_ALLOCATION( "ELI10777", ipObjCopy != NULL );

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom( ipUnk );
	
		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10778");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocTypeCondition::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI26065", pClassID != NULL);
		*pClassID = CLSID_DocTypeCondition;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12856");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocTypeCondition::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		return m_bDirty ? S_OK : S_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12857");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocTypeCondition::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// clear current data
		clear();

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
		if (nDataVersion > gnCurrentVersion)
		{
			// Throw exception
			UCLIDException ue( "ELI10779", 
				"Unable to load newer Doc Type Condition." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		dataReader >> m_bAllowTypes;

		if (nDataVersion > 1)
		{
			long nTmp;
			dataReader >> nTmp;
			m_eMinConfidence = (EDocumentConfidenceLevel)nTmp;
		}

		// Load Category string
		if (nDataVersion > 2)
		{
			dataReader >> m_strCategory;
		}

		// Load collection of document types
		IPersistStreamPtr ipObj;
		readObjectFromStream(ipObj, pStream, "ELI10823");
		m_ipTypes = ipObj;

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10780");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocTypeCondition::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter( ByteStreamManipulator::kWrite, data );
		dataWriter << gnCurrentVersion;
		dataWriter << m_bAllowTypes;
		dataWriter << (long)m_eMinConfidence;
		dataWriter << m_strCategory;

		dataWriter.flushToByteStream();
	
		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write( &nDataLength, sizeof(nDataLength), NULL );
		pStream->Write( data.getData(), nDataLength, NULL );

		// Write the document types directly to the stream
		IPersistStreamPtr ipObj = m_ipTypes;
		ASSERT_RESOURCE_ALLOCATION("ELI10822", ipObj != NULL);
		writeObjectToStream(ipObj, pStream, "ELI19123", fClearDirty);

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10781");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocTypeCondition::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CDocTypeCondition::validateLicense()
{
	static const unsigned long DOC_TYPE_CONDITION_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE( DOC_TYPE_CONDITION_ID, "ELI10782", 
		"Doc Type Condition" );
}
//-------------------------------------------------------------------------------------------------
void CDocTypeCondition::clear()
{
	m_bAllowTypes = true;
	m_ipTypes = NULL;
	m_eMinConfidence = kMaybeLevel;
	m_strCategory = "";
}
//-------------------------------------------------------------------------------------------------
