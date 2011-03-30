// CreateValue.cpp : Implementation of CCreateValue
#include "stdafx.h"
#include "AFValueFinders.h"
#include "CreateValue.h"
#include "..\\..\\AFCore\\Code\\Common.h"

#include <UCLIDException.h>
#include <COMUtils.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <CommentedTextFileReader.h>
#include <EncryptedFileManager.h>
#include <ByteStream.h>
#include <ByteStreamManipulator.h>
#include <Misc.h>
#include <ComponentLicenseIDs.h>

#include <io.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CCreateValue
//-------------------------------------------------------------------------------------------------
CCreateValue::CCreateValue()
{
	try
	{
		m_ipAFUtility.CreateInstance( CLSID_AFUtility );
		ASSERT_RESOURCE_ALLOCATION( "ELI09837", m_ipAFUtility != __nullptr );
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI09838")
}
//-------------------------------------------------------------------------------------------------
CCreateValue::~CCreateValue()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16341");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCreateValue::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ICreateValue,
		&IID_IAttributeFindingRule,
		&IID_IPersistStream,
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
// ICreateValue
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCreateValue::get_ValueString(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		*pVal = _bstr_t(m_strValue.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09839")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCreateValue::put_ValueString(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		string strValue = asString( newVal );

		if (strValue.empty())
		{
			throw UCLIDException("ELI09840", "Please provide non-empty Value.");
		}

		m_strValue = strValue;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09841")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCreateValue::get_TypeString(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		*pVal = _bstr_t(m_strType.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09842")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCreateValue::put_TypeString(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		string strType = asString( newVal );

		// Validate non-blank types [FlexIDSCore #3810]
		if (!strType.empty() && !isValidIdentifier(strType))
		{
			UCLIDException ue("ELI28572", "Specified type contains invalid characters.");
			ue.addDebugInfo("Invalid Type", strType);
			throw ue;
		}

		m_strType = strType;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09843")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
// IAttributeFindingRule
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCreateValue::raw_ParseText(IAFDocument* pAFDoc, IProgressStatus *pProgressStatus,
										 IIUnknownVector **pAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// check arguments
		if (pAttributes == NULL)
		{
			return E_POINTER;
		}
			
		// validate license
		validateLicense();

		IIUnknownVectorPtr ipAttributes(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI09844", ipAttributes != __nullptr);

		IAFDocumentPtr ipAFDoc(pAFDoc);

		IAttributePtr ipAttribute(CLSID_Attribute);
		ASSERT_RESOURCE_ALLOCATION("ELI09845", ipAttribute != __nullptr);
		// Expand any tags in the string
		// If any tags don't expand we will not add the attribute
		bool bAdd = true;
		string strExpandedValue;
		try
		{
			strExpandedValue = asString(m_ipAFUtility->ExpandTags(_bstr_t(m_strValue.c_str()), ipAFDoc));
		}
		catch(...)
		{
			bAdd = false;
		}

		if(bAdd)
		{
			ISpatialStringPtr ipSS(CLSID_SpatialString);
			ASSERT_RESOURCE_ALLOCATION("ELI09846", ipSS != __nullptr);
			ipSS->CreateNonSpatialString(strExpandedValue.c_str(), "");

			ipAttribute->Value = ipSS;
			ipAttribute->Type = _bstr_t(m_strType.c_str());

			ipAttributes->PushBack(ipAttribute);
		}
		// return the vector
		*pAttributes = ipAttributes.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09847");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCreateValue::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_CreateValue;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCreateValue::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCreateValue::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		pStream->Read(&nDataLength, sizeof(nDataLength), NULL);
		ByteStream data(nDataLength);
		pStream->Read(data.getData(), nDataLength, NULL);
		ByteStreamManipulator dataReader(ByteStreamManipulator::kRead, data);

		// Read the individual data items from the bytestream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;

		dataReader >> m_strValue;
		dataReader >> m_strType;

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09848");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCreateValue::Save(IStream *pStream, BOOL fClearDirty)
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

		dataWriter << m_strValue;
		dataWriter << m_strType;
		
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09849");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCreateValue::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}
//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCreateValue::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19577", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Create value").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09850");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCreateValue::raw_IsConfigured(VARIANT_BOOL * pbValue)
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

		bool bConfigured = true;

		if(m_strValue.size() == 0)
		{
			bConfigured = false;
		}
		
		*pbValue = bConfigured ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09851");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCreateValue::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCreateValue::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		UCLID_AFVALUEFINDERSLib::ICreateValuePtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI09853", ipSource != __nullptr);

		m_strValue = ipSource->ValueString;
		m_strType = ipSource->TypeString;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09852");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCreateValue::raw_Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy(CLSID_CreateValue);
		ASSERT_RESOURCE_ALLOCATION("ELI09854", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09855");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CCreateValue::validateLicense()
{
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI09856", "Create Value Rule" );
}
//-------------------------------------------------------------------------------------------------
