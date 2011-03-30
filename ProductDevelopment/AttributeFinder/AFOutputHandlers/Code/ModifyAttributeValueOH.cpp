// ModifyAttributeValueOH.cpp : Implementation of CModifyAttributeValueOH
#include "stdafx.h"
#include "AFOutputHandlers.h"
#include "ModifyAttributeValueOH.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <COMUtils.h>
#include <ComponentLicenseIDs.h>
#include <cpputil.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 4;

//-------------------------------------------------------------------------------------------------
// CModifyAttributeValueOH
//-------------------------------------------------------------------------------------------------
CModifyAttributeValueOH::CModifyAttributeValueOH()
: m_bDirty(false),
  m_bSetAttributeName(false),
  m_bSetAttributeValue(false),
  m_bSetAttributeType(false),
  m_ipAFUtil(NULL),
  m_bCreateSubAttribute(false)
{
	m_ipAFUtil.CreateInstance(CLSID_AFUtility);
	ASSERT_RESOURCE_ALLOCATION("ELI08744", m_ipAFUtil != __nullptr);
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CModifyAttributeValueOH::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IModifyAttributeValueOH,
		&IID_IOutputHandler,
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
// IModifyAttributeValue
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CModifyAttributeValueOH::get_AttributeQuery(/*[out, retval]*/ BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Validate argument
		ASSERT_ARGUMENT( "ELI08533", pVal != __nullptr );

		// Return the Attribute Name
		*pVal = _bstr_t( m_strAttributeQuery.c_str() ).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08534")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CModifyAttributeValueOH::put_AttributeQuery(/*[in]*/ BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Make local copy of Name
		string strQuery = asString( newVal );

		// Attribute Name must not be empty
		if (strQuery.empty())
		{
			UCLIDException ue( "ELI08537", "The Attribute Query must not be empty!");
			throw ue;
		}

		// make sure the string is a valid query
		if(m_ipAFUtil->IsValidQuery(newVal) == VARIANT_FALSE)
		{
			UCLIDException ue( "ELI10436", "Invalid Attribute Query!");
			throw ue;
		}

		// Store the Attribute Name
		m_strAttributeQuery = strQuery;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08538")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CModifyAttributeValueOH::get_AttributeName(/*[out, retval]*/ BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Validate argument
		ASSERT_ARGUMENT( "ELI10010", pVal != __nullptr );

		// Return the Attribute Name
		*pVal = _bstr_t(m_strAttributeName.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10009")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CModifyAttributeValueOH::put_AttributeName(/*[in]*/ BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		string strNewVal = asString(newVal);

		// Validate the attribute name
		if (!isValidIdentifier(strNewVal))
		{
			UCLIDException uex("ELI28574", "Invalid attribute name.");
			uex.addDebugInfo("Invalid Name", strNewVal);
			throw uex;
		}

		// Store the Attribute Name
		m_strAttributeName = strNewVal;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10011")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CModifyAttributeValueOH::get_AttributeValue(/*[out, retval]*/ BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Validate argument
		ASSERT_ARGUMENT( "ELI08535", pVal != __nullptr );

		// Return the Attribute Value
		*pVal = _bstr_t( m_strAttributeValue.c_str() ).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08536")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CModifyAttributeValueOH::put_AttributeValue(/*[in]*/ BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Store the Attribute Value
		m_strAttributeValue = asString( newVal );
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08539")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CModifyAttributeValueOH::get_AttributeType(/*[out, retval]*/ BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Validate argument
		ASSERT_ARGUMENT( "ELI08733", pVal != __nullptr );

		// Return the Attribute Type
		*pVal = _bstr_t( m_strAttributeType.c_str() ).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08734")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CModifyAttributeValueOH::put_AttributeType(/*[in]*/ BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		string strNewVal = asString(newVal);

		// Validate the attribute type
		if (!strNewVal.empty() && !isValidIdentifier(strNewVal))
		{
			UCLIDException uex("ELI28575", "Invalid attribute type.");
			uex.addDebugInfo("Invalid Type", strNewVal);
			throw uex;
		}

		// Store the Attribute type
		m_strAttributeType = strNewVal;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08735")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CModifyAttributeValueOH::get_SetAttributeName(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Retrieve setting
		*pVal = asVariantBool(m_bSetAttributeName);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10012");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CModifyAttributeValueOH::put_SetAttributeName(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Store setting
		m_bSetAttributeName = asCppBool(newVal);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10013");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CModifyAttributeValueOH::get_SetAttributeValue(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Retrieve setting
		*pVal = asVariantBool(m_bSetAttributeValue);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08736");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CModifyAttributeValueOH::put_SetAttributeValue(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Store setting
		m_bSetAttributeValue = asCppBool(newVal);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08737");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CModifyAttributeValueOH::get_SetAttributeType(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Retrieve setting
		*pVal = asVariantBool(m_bSetAttributeType);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08738");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CModifyAttributeValueOH::put_SetAttributeType(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Store setting
		m_bSetAttributeType = asCppBool(newVal);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08739");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CModifyAttributeValueOH::get_CreateSubAttribute(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Retrieve setting
		*pVal = asVariantBool(m_bCreateSubAttribute);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10266");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CModifyAttributeValueOH::put_CreateSubAttribute(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Store setting
		m_bCreateSubAttribute = asCppBool(newVal);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10267");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IOutputHandler
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CModifyAttributeValueOH::raw_ProcessOutput(IIUnknownVector* pAttributes,
														IAFDocument *pAFDoc,
														IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// Local copy of original Attributes
		IIUnknownVectorPtr ipOrigAttributes( pAttributes );
		ASSERT_ARGUMENT("ELI08544", ipOrigAttributes != __nullptr);

		// query the vector for desired attributes
		IIUnknownVectorPtr ipQueriedAttributes = m_ipAFUtil->QueryAttributes(
				ipOrigAttributes, _bstr_t(m_strAttributeQuery.c_str()), VARIANT_FALSE);

		if (ipQueriedAttributes != __nullptr)
		{
			long nMatchedNum = ipQueriedAttributes->Size();
			for (long n=0; n<nMatchedNum; n++)
			{
				IAttributePtr ipMatchedAttr = ipQueriedAttributes->At(n);
				ASSERT_RESOURCE_ALLOCATION("ELI08542", ipMatchedAttr != __nullptr);

				// This is a pointer to the attribute that will be updated with
				// new info 
				// we will default it to the selected attribute
				IAttributePtr ipUpdateAttr = ipMatchedAttr;

				// I the create subAttr
				if (m_bCreateSubAttribute)
				{
					ipUpdateAttr = __nullptr;
					ipUpdateAttr.CreateInstance(CLSID_Attribute);
					ASSERT_RESOURCE_ALLOCATION("ELI10268", ipUpdateAttr != __nullptr);
				}
				// modify attribute value or type
				if (m_bSetAttributeName)
				{
					ipUpdateAttr->Name = m_strAttributeName.c_str();
				}
				else
				{
					// If we are creating a new sub-attribute the name must be set
					if(m_bCreateSubAttribute)
					{
						UCLIDException ue("ELI10270", "No name specified for new sub-attribute.");
						throw ue;
					}
				}

				if (m_bSetAttributeValue)
				{
					// replace the old attribute value with the new attribute value
					ipUpdateAttr->Value = m_ipAFUtil->ExpandFormatString(ipMatchedAttr, m_strAttributeValue.c_str());
				}

				if (m_bSetAttributeType)
				{
					ipUpdateAttr->Type = m_strAttributeType.c_str();
				}

				if (m_bCreateSubAttribute)
				{
					IIUnknownVectorPtr ipSub = ipMatchedAttr->SubAttributes;
					ASSERT_RESOURCE_ALLOCATION( "ELI15525", ipSub != __nullptr );
					ipSub->PushBack(ipUpdateAttr);
				}
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08541")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CModifyAttributeValueOH::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CModifyAttributeValueOH::raw_IsConfigured(VARIANT_BOOL * pbValue)
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

		// This object is considered configured 
		//	if the Attribute Name is specified
		// The Attribute Value is allowed to be empty
		bool bIsConfigured = true;

		if (m_strAttributeQuery.empty())
		{
			bIsConfigured = false;
		}

		if(!m_bSetAttributeName && !m_bSetAttributeType && !m_bSetAttributeValue)
		{
			bIsConfigured = false;
		}	
		*pbValue = asVariantBool(bIsConfigured);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08545");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CModifyAttributeValueOH::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19544", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Modify attributes").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08546")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CModifyAttributeValueOH::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		
		UCLID_AFOUTPUTHANDLERSLib::IModifyAttributeValueOHPtr ipSource = pObject;
		ASSERT_RESOURCE_ALLOCATION( "ELI08547", ipSource != __nullptr );
		
		// Copy Attribute Name and Value
		m_strAttributeQuery = asString( ipSource->AttributeQuery );
		
		m_strAttributeName = asString( ipSource->AttributeName );
		m_strAttributeValue = asString( ipSource->AttributeValue );
		m_strAttributeType = asString( ipSource->AttributeType );
		
		// Copy checkbox settings
		m_bSetAttributeName = asCppBool(ipSource->SetAttributeName);
		m_bSetAttributeValue = asCppBool(ipSource->SetAttributeValue);
		m_bSetAttributeType = asCppBool(ipSource->SetAttributeType);
		
		m_bCreateSubAttribute = asCppBool(ipSource->CreateSubAttribute);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12814");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CModifyAttributeValueOH::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		// Create another instance of this object
		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance( CLSID_ModifyAttributeValueOH );
		ASSERT_RESOURCE_ALLOCATION( "ELI08548", ipObjCopy != __nullptr );

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom( ipUnk );
	
		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08549");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CModifyAttributeValueOH::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pClassID = CLSID_ModifyAttributeValueOH;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CModifyAttributeValueOH::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
// NOTES about versions:
// Version 1:
//   * Saved: 
//            Attribute Name
//            Attribute Value
// Version 2:
//   * Additionally saved:
//            Attribute Type
//            Set Attribute Value
//            Set Attribute Type
STDMETHODIMP CModifyAttributeValueOH::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Reset member variables
		m_strAttributeName = "";
		m_strAttributeValue = "";
		m_bSetAttributeValue = false;
		m_bSetAttributeType = false;

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
			UCLIDException ue( "ELI08550", 
				"Unable to load newer Modify Attribute Value Output Handler" );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		if (nDataVersion >= 1)
		{
			// Read the Attribute Name
			dataReader >> m_strAttributeQuery;

			// Read the Attribute Value
			dataReader >> m_strAttributeValue;
		}

		// Is Type string and checkbox information available
		if (nDataVersion >= 2)
		{
			// Read the Attribute Type
			dataReader >> m_strAttributeType;

			// Read the Value checkbox setting
			dataReader >> m_bSetAttributeValue;

			// Read the Type checkbox setting
			dataReader >> m_bSetAttributeType;
		}
		else
		{
			// Default to modification of Value
			m_bSetAttributeValue = true;
		}

		if(nDataVersion >= 3)
		{
			// Read the Attribute Name
			dataReader >> m_strAttributeName;

			// Read the Name checkbox setting
			dataReader >> m_bSetAttributeName;
		}

		if(nDataVersion >= 4)
		{
			dataReader >> m_bCreateSubAttribute;
		}
		else
		{
			m_bCreateSubAttribute = false;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08551");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CModifyAttributeValueOH::Save(IStream *pStream, BOOL fClearDirty)
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
		dataWriter << m_strAttributeQuery;
		dataWriter << m_strAttributeValue;

		dataWriter << m_strAttributeType;
		dataWriter << m_bSetAttributeValue;
		dataWriter << m_bSetAttributeType;

		dataWriter << m_strAttributeName;
		dataWriter << m_bSetAttributeName;

		dataWriter << m_bCreateSubAttribute;

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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08552");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CModifyAttributeValueOH::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CModifyAttributeValueOH::validateLicense()
{
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI08540", 
		"Modify Attribute Value Output Handler" );
}
//-------------------------------------------------------------------------------------------------
