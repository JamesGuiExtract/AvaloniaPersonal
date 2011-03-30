// MoveAndModifyAttributes.cpp : Implementation of CMoveAndModifyAttributes
#include "stdafx.h"
#include "AFOutputHandlers.h"
#include "MoveAndModifyAttributes.h"

#include <LicenseMgmt.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>

#include <vector>
using std::vector;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 2;

//-------------------------------------------------------------------------------------------------
// CMoveAndModifyAttributes
//-------------------------------------------------------------------------------------------------
CMoveAndModifyAttributes::CMoveAndModifyAttributes() :
 m_strQuery(""),
 m_eMoveAttributeLevel(kMoveToRoot),
 m_eOverwriteAttributeName(kDoNotOverwrite),
 m_strSpecifiedName(""),
 m_bRetainCurrType(true),
 m_bAddRootOrParentType(false),
 m_bAddNameToType(false),
 m_bAddSpecified(false),
 m_strSpecifiedType(""),
 m_bDeleteRootOrParentIfAllChildrenMoved(true),
 m_bDirty(false)
{
	try
	{
		m_ipAFUtility.CreateInstance(CLSID_AFUtility);
		ASSERT_RESOURCE_ALLOCATION("ELI09516", m_ipAFUtility != __nullptr);
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI09565")
}
//-------------------------------------------------------------------------------------------------
CMoveAndModifyAttributes::~CMoveAndModifyAttributes()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16313");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMoveAndModifyAttributes::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IMoveAndModifyAttributes,
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
// IMoveAndModifyAttributes
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMoveAndModifyAttributes::get_AttributeQuery(/*[out, retval]*/ BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Validate argument
		ASSERT_ARGUMENT( "ELI09410", pVal != __nullptr );

		// Return the Attribute Name
		*pVal = _bstr_t( m_strQuery.c_str() ).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09411")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMoveAndModifyAttributes::put_AttributeQuery(/*[in]*/ BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Make local copy of Name
		string strTmp = asString( newVal );

		// Attribute Name must not be empty
		if (strTmp.empty())
		{
			UCLIDException ue( "ELI09412", "The Attribute query must not be empty!");
			throw ue;
		}

		// make sure the string is a valid query
		if(m_ipAFUtility->IsValidQuery(newVal) == VARIANT_FALSE)
		{
			UCLIDException ue( "ELI10435", "Invalid Attribute Query!");
			throw ue;
		}

		long nMinDepth = m_ipAFUtility->GetMinQueryDepth(_bstr_t(strTmp.c_str()));
		if (nMinDepth <= 1)
		{
			UCLIDException ue("ELI09497", "The Attribute query must not query top-level (root) attributes!");
			throw ue;
		}

		// Store the Attribute Name
		m_strQuery = strTmp;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09413")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMoveAndModifyAttributes::get_MoveAttributeLevel(EMoveAttributeLevel *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		*pVal = m_eMoveAttributeLevel;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10200")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMoveAndModifyAttributes::put_MoveAttributeLevel(EMoveAttributeLevel newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		m_eMoveAttributeLevel = newVal;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10201")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMoveAndModifyAttributes::get_OverwriteAttributeName(/*[out, retval]*/ EOverwriteAttributeName *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Validate argument
		ASSERT_ARGUMENT( "ELI09414", pVal != __nullptr );

		*pVal = m_eOverwriteAttributeName;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09415")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMoveAndModifyAttributes::put_OverwriteAttributeName(/*[in]*/ EOverwriteAttributeName newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		m_eOverwriteAttributeName = newVal;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09416")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMoveAndModifyAttributes::get_SpecifiedAttributeName(/*[out, retval]*/ BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Validate argument
		ASSERT_ARGUMENT( "ELI09418", pVal != __nullptr );

		// Return the Attribute Name
		*pVal = _bstr_t( m_strSpecifiedName.c_str() ).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09417")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMoveAndModifyAttributes::put_SpecifiedAttributeName(/*[in]*/ BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Make local copy of Name
		string strTmp = asString( newVal );

		// Attribute Name must not be empty
		if (strTmp.empty())
		{
			UCLIDException ue( "ELI09419", "The Specified Attribute Name must not be empty!");
			throw ue;
		}

		// Validate the attribute name
		if (!isValidIdentifier(strTmp))
		{
			UCLIDException uex("ELI28576", "Invalid attribute name.");
			uex.addDebugInfo("Invalid Name", strTmp);
			throw uex;
		}

		// Store the Attribute Name
		m_strSpecifiedName = strTmp;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09420")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMoveAndModifyAttributes::get_RetainAttributeType(/*[out, retval]*/ VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Validate argument
		ASSERT_ARGUMENT( "ELI09421", pVal != __nullptr );

		*pVal = m_bRetainCurrType ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09422")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMoveAndModifyAttributes::put_RetainAttributeType(/*[in]*/ VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		m_bRetainCurrType = newVal == VARIANT_TRUE;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09423")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMoveAndModifyAttributes::get_AddRootOrParentAttributeType(/*[out, retval]*/ VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Validate argument
		ASSERT_ARGUMENT( "ELI09424", pVal != __nullptr );

		*pVal = m_bAddRootOrParentType ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09425")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMoveAndModifyAttributes::put_AddRootOrParentAttributeType(/*[in]*/ VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		m_bAddRootOrParentType = newVal == VARIANT_TRUE;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09426")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMoveAndModifyAttributes::get_AddSpecifiedAttributeType(/*[out, retval]*/ VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Validate argument
		ASSERT_ARGUMENT( "ELI09427", pVal != __nullptr );

		*pVal = m_bAddSpecified ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09428")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMoveAndModifyAttributes::put_AddSpecifiedAttributeType(/*[in]*/ VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		m_bAddSpecified = newVal == VARIANT_TRUE;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09429")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMoveAndModifyAttributes::get_SpecifiedAttributeType(/*[out, retval]*/ BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Validate argument
		ASSERT_ARGUMENT( "ELI09430", pVal != __nullptr );

		// Return the Attribute Name
		*pVal = _bstr_t( m_strSpecifiedType.c_str() ).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09431")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMoveAndModifyAttributes::put_SpecifiedAttributeType(/*[in]*/ BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Make local copy of Type
		string strTmp = asString( newVal );

		// Attribute Type must not be empty
		if (strTmp.empty())
		{
			UCLIDException ue( "ELI09432", "The Specified Attribute Type must not be empty!");
			throw ue;
		}

		// Validate the type
		if (!isValidIdentifier(strTmp))
		{
			UCLIDException uex("ELI28577", "Invalid attribute type.");
			uex.addDebugInfo("Invalid Type", strTmp);
			throw uex;
		}

		// Store the Attribute Type
		m_strSpecifiedType = strTmp;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09433")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMoveAndModifyAttributes::get_DeleteRootOrParentIfAllChildrenMoved(/*[out, retval]*/ VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Validate argument
		ASSERT_ARGUMENT( "ELI19138", pVal != __nullptr );

		*pVal = m_bDeleteRootOrParentIfAllChildrenMoved ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19139")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMoveAndModifyAttributes::put_DeleteRootOrParentIfAllChildrenMoved(/*[in]*/ VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		m_bDeleteRootOrParentIfAllChildrenMoved = newVal == VARIANT_TRUE;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19140")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMoveAndModifyAttributes::get_AddAttributeNameToType(/*[out, retval]*/ VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Validate argument
		ASSERT_ARGUMENT( "ELI19141", pVal != __nullptr );

		*pVal = m_bAddNameToType ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09501")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMoveAndModifyAttributes::put_AddAttributeNameToType(/*[in]*/ VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		m_bAddNameToType = newVal == VARIANT_TRUE;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09502")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IOutputHandler
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMoveAndModifyAttributes::raw_ProcessOutput(IIUnknownVector* pAttributes,
														IAFDocument *pAFDoc,
														IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// all root level attributes
		IIUnknownVectorPtr ipRootLevelAttributes(pAttributes);
		ASSERT_RESOURCE_ALLOCATION("ELI09456", ipRootLevelAttributes != __nullptr);

		// Query the attributes
		IIUnknownVectorPtr ipFoundAttributes = m_ipAFUtility->QueryAttributes(ipRootLevelAttributes, 
			_bstr_t(m_strQuery.c_str()), VARIANT_FALSE);

		// Before moving these attributes, modify their type and name accordingly
		modifyAttributeTypes(ipRootLevelAttributes, ipFoundAttributes);
		modifyAttributeNames(ipRootLevelAttributes, ipFoundAttributes);

		// Store vector of root or parent attributes of found attributes 
		// before the found attributes are moved
		vector<IAttributePtr> vecRootOrParentAttributesOfFoundAttributes;
		if (m_bDeleteRootOrParentIfAllChildrenMoved)
		{
			// Get all the root or parent nodes that will be affected by moving 
			// the sub attributes
			int i; 
			for (i = 0; i < ipFoundAttributes->Size(); i++)
			{
				IAttributePtr ipAttr = ipFoundAttributes->At(i);
				ASSERT_RESOURCE_ALLOCATION("ELI09472", ipAttr != __nullptr);
				IAttributePtr ipRootOrParent 
					= getRootOrParentAttribute(ipRootLevelAttributes, ipAttr);
				if (ipRootOrParent == __nullptr)
				{
					continue;
				}
				// store the attribute if only it can't be found in the vector
				::vectorPushBackIfNotContained(vecRootOrParentAttributesOfFoundAttributes, ipRootOrParent);
			}
		}

		// Now, move found attributes
		moveAttributes(ipRootLevelAttributes, ipFoundAttributes);

		// Remove any affected childless root or parent attribute if the flag is on
		if (m_bDeleteRootOrParentIfAllChildrenMoved)
		{
			// check every root node that was affected by moving attributes
			// and if those root nodes no longer have any children delete them
			unsigned int ui;
			for (ui = 0; ui < vecRootOrParentAttributesOfFoundAttributes.size(); ui++)
			{
				// Retrieve this root node
				IAttributePtr ipAttr = vecRootOrParentAttributesOfFoundAttributes[ui];
				ASSERT_RESOURCE_ALLOCATION("ELI09473", ipAttr != __nullptr);

				// Check count of sub-attributes
				IIUnknownVectorPtr ipSub = ipAttr->SubAttributes;
				ASSERT_RESOURCE_ALLOCATION("ELI15526", ipSub != __nullptr);
				if (ipSub->Size() == 0)
				{
					// No sub-attributes so remove this attribute
					m_ipAFUtility->RemoveAttribute(ipRootLevelAttributes, ipAttr);
				}
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09398")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMoveAndModifyAttributes::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CMoveAndModifyAttributes::raw_IsConfigured(VARIANT_BOOL * pbValue)
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
		// if a query is specified
		*pbValue = m_strQuery.empty() ? VARIANT_FALSE : VARIANT_TRUE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09471");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMoveAndModifyAttributes::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19545", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Move and modify attributes").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09407")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMoveAndModifyAttributes::raw_CopyFrom(IUnknown *pObject)
{

	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		// Check licensing
		validateLicense();

		UCLID_AFOUTPUTHANDLERSLib::IMoveAndModifyAttributesPtr ipSource = pObject;
		ASSERT_RESOURCE_ALLOCATION( "ELI09401", ipSource != __nullptr );

		m_strQuery = asString( ipSource->GetAttributeQuery() );

		m_eMoveAttributeLevel = (EMoveAttributeLevel)ipSource->MoveAttributeLevel;

		m_eOverwriteAttributeName = (EOverwriteAttributeName)ipSource->GetOverwriteAttributeName();
		m_strSpecifiedName = asString( ipSource->GetSpecifiedAttributeName() );

		m_bRetainCurrType = ipSource->GetRetainAttributeType() == VARIANT_TRUE;
		m_bAddRootOrParentType = ipSource->GetAddRootOrParentAttributeType() == VARIANT_TRUE;
		m_bAddNameToType = ipSource->GetAddAttributeNameToType() == VARIANT_TRUE;
		m_bAddSpecified = ipSource->GetAddSpecifiedAttributeType() == VARIANT_TRUE;
		m_strSpecifiedType = asString( ipSource->GetSpecifiedAttributeType() );

		m_bDeleteRootOrParentIfAllChildrenMoved = ipSource->DeleteRootOrParentIfAllChildrenMoved == VARIANT_TRUE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09567");
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMoveAndModifyAttributes::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		// Create another instance of this object
		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance( CLSID_MoveAndModifyAttributes );
		ASSERT_RESOURCE_ALLOCATION( "ELI09402", ipObjCopy != __nullptr );

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom( ipUnk );
	
		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09403");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMoveAndModifyAttributes::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pClassID = CLSID_MoveAndModifyAttributes;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMoveAndModifyAttributes::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMoveAndModifyAttributes::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// set default
		m_eMoveAttributeLevel = kMoveToRoot;

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
			UCLIDException ue( "ELI09406", 
				"Unable to load newer Move and Modify Attribute Output Handler!" );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		dataReader >> m_strQuery;

		long nTmp;
		if (nDataVersion >= 2)
		{
			dataReader >> nTmp;
			m_eMoveAttributeLevel = (EMoveAttributeLevel)nTmp;
		}

		dataReader >> nTmp;
		m_eOverwriteAttributeName = (EOverwriteAttributeName)nTmp;
		dataReader >> m_strSpecifiedName;

		dataReader >> m_bRetainCurrType;
		dataReader >> m_bAddRootOrParentType;
		dataReader >> m_bAddNameToType;
		dataReader >> m_bAddSpecified;
		dataReader >> m_strSpecifiedType;

		dataReader >> m_bDeleteRootOrParentIfAllChildrenMoved;

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09405");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMoveAndModifyAttributes::Save(IStream *pStream, BOOL fClearDirty)
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
		dataWriter << m_strQuery;

		dataWriter << (long)m_eMoveAttributeLevel;

		dataWriter << (long)m_eOverwriteAttributeName;
		dataWriter << m_strSpecifiedName;

		dataWriter << m_bRetainCurrType;
		dataWriter << m_bAddRootOrParentType;
		dataWriter << m_bAddNameToType;
		dataWriter << m_bAddSpecified;
		dataWriter << m_strSpecifiedType;

		dataWriter << m_bDeleteRootOrParentIfAllChildrenMoved;

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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09404");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMoveAndModifyAttributes::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
IAttributePtr CMoveAndModifyAttributes::getRootOrParentAttribute(IIUnknownVectorPtr ipAttributes, 
																 IAttributePtr ipChild)
{
	IAttributePtr ipRootOrParent(NULL);

	switch (m_eMoveAttributeLevel)
	{
	case kMoveToRoot:
		// get attribute's root
		ipRootOrParent = m_ipAFUtility->GetAttributeRoot(ipAttributes, ipChild);
		break;
	case kMoveToParent:
		// get attribute's parent
		ipRootOrParent = m_ipAFUtility->GetAttributeParent(ipAttributes, ipChild);
		break;
	default:
		THROW_LOGIC_ERROR_EXCEPTION("ELI10205");
		break;
	}

	return ipRootOrParent;
}
//-------------------------------------------------------------------------------------------------
void CMoveAndModifyAttributes::modifyAttributeNames(IIUnknownVectorPtr ipTrees, IIUnknownVectorPtr ipAttributes)
{
	if (m_eOverwriteAttributeName == kDoNotOverwrite)
	{
		return;
	}
	else if (m_eOverwriteAttributeName == kOverwriteWithRootOrParentName)
	{
		// This will hold the new names as we form them
		// we hold the new names instead of assigning the to the attributes
		// right away so we don't write an attributes names then query it later 
		// in this method for use in another attributes name
		// Note: this only matters when we are using other attributes names 
		// (e.g. the parent or root)
		vector<string> vecNewNames;
		// We will cache the attrbiutes in a local stl vector to
		// prevent more Com calls in the second loop through the vector
		vector<IAttributePtr> vecAttrs;
		unsigned int ui;
		for (ui = 0; ui < (unsigned int)ipAttributes->Size(); ui++)
		{
			IAttributePtr ipAttribute = ipAttributes->At(ui);
			ASSERT_RESOURCE_ALLOCATION("ELI09457", ipAttribute != __nullptr);
			vecAttrs.push_back(ipAttribute);

			IAttributePtr ipRootOrParent = getRootOrParentAttribute(ipTrees, ipAttribute);
			string strName;
			if (ipRootOrParent != __nullptr)
			{
				strName = asString( ipRootOrParent->Name);
			}
			vecNewNames.push_back(strName);
		}
		for (ui = 0; ui < vecAttrs.size(); ui++)
		{
			vecAttrs[ui]->Name = get_bstr_t(vecNewNames[ui]);
		}
	}
	else if (m_eOverwriteAttributeName == kOverwriteWithSpecifiedName)
	{
		int i;
		for (i = 0; i < ipAttributes->Size(); i++)
		{
			IAttributePtr ipAttribute = ipAttributes->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI09524", ipAttribute != __nullptr);
			
			ipAttribute->Name = _bstr_t(m_strSpecifiedName.c_str());
		}
	}
	else
	{
		THROW_LOGIC_ERROR_EXCEPTION("ELI09458");
	}
}
//-------------------------------------------------------------------------------------------------
void CMoveAndModifyAttributes::modifyAttributeTypes(IIUnknownVectorPtr ipTrees, IIUnknownVectorPtr ipAttributes)
{
	// This will hold the new types as we form them
	// we hold the new types instead of assigning the to the attributes
	// right away so we don't write an attributes type then query it later 
	// in this method for use in another attributes type
	vector<string> vecNewTypes;
	// Wee will cache the attrbiutes in a local stl vector to
	// prevent more Com calls in the second loop through the vector
	vector<IAttributePtr> vecAttrs;
	unsigned int ui;
	for (ui = 0; ui < (unsigned int)ipAttributes->Size(); ui++)
	{
		string strNewType;

		IAttributePtr ipAttribute = ipAttributes->At(ui);
		ASSERT_RESOURCE_ALLOCATION("ELI09525", ipAttribute != __nullptr);
		vecAttrs.push_back(ipAttribute);
		
		if (m_bRetainCurrType)
		{
			strNewType += asString(ipAttribute->Type);
		}

		if (m_bAddRootOrParentType)
		{
			IAttributePtr ipRootOrParent = getRootOrParentAttribute(ipTrees, ipAttribute);
			if (ipRootOrParent != __nullptr)
			{

				_bstr_t _bstrType = ipRootOrParent->Type;
				if(!strNewType.empty() && _bstrType.length() > 0)
				{
					strNewType += '+';
				}
				strNewType += _bstrType;
			}
		}
		if (m_bAddNameToType)
		{
			_bstr_t _bstrType = ipAttribute->Name;
			if(!strNewType.empty() && _bstrType.length() > 0)
			{
				strNewType += '+';
			}
			strNewType += _bstrType;
		}
		if (m_bAddSpecified)
		{
			_bstr_t _bstrType = m_strSpecifiedType.c_str();
			if(!strNewType.empty() && _bstrType.length() > 0)
			{
				strNewType += '+';
			}
			strNewType += _bstrType;
		}
		vecNewTypes.push_back(strNewType);
	}

	for (ui = 0; ui < vecAttrs.size(); ui++)
	{
		vecAttrs[ui]->Type = get_bstr_t(vecNewTypes[ui]);
	}
}
//-------------------------------------------------------------------------------------------------
void CMoveAndModifyAttributes::moveAttributes(IIUnknownVectorPtr ipRootLevelAttributes, 
											  IIUnknownVectorPtr ipFoundAttributes)
{
	// whether move found attributes to top most level or only up one level
	// all depends on the the value of m_eMoveAttributeLevel
	switch (m_eMoveAttributeLevel)
	{
	case kMoveToRoot:
		{
			// Remove all the found attributes from thier current position in the tree
			m_ipAFUtility->RemoveAttributes(ipRootLevelAttributes, ipFoundAttributes);
			// Append all the found attributes to the top (root) level
			ipRootLevelAttributes->Append(ipFoundAttributes);
		}
		break;
	case kMoveToParent:
		{
			vector<IIUnknownVectorPtr> vecGrandParentsSubAttrs;
			vector<IAttributePtr> vecAttrs;
			long nSize = ipFoundAttributes->Size();
			long n;
			for (n = 0; n < nSize; n++)
			{
				IAttributePtr ipFoundAttribute = ipFoundAttributes->At(n);
				ASSERT_RESOURCE_ALLOCATION("ELI10211", ipFoundAttribute != __nullptr);
				vecAttrs.push_back(ipFoundAttribute);

				// get individual found attribute's parent
				IAttributePtr ipParent = m_ipAFUtility->GetAttributeParent(
										ipRootLevelAttributes, ipFoundAttribute);
				ASSERT_RESOURCE_ALLOCATION("ELI10212", ipParent != __nullptr);

				// get the parent's parent
				IAttributePtr ipGrandParent = m_ipAFUtility->GetAttributeParent(
										ipRootLevelAttributes, ipParent);
				// if the ipParent is at root level already, ipGrandParent could be null
				// get the vector of sub attributes of the grand parent
				IIUnknownVectorPtr ipGrandParentSubAttributes 
					= ipGrandParent != __nullptr ? ipGrandParent->SubAttributes : ipRootLevelAttributes;

				vecGrandParentsSubAttrs.push_back(ipGrandParentSubAttributes);
			}
			for (n = 0; n < nSize; n++)
			{
					// remove found attribute from its original place
				m_ipAFUtility->RemoveAttribute(ipRootLevelAttributes, vecAttrs[n]);

				// now, append the found attribute to the vector
				(vecGrandParentsSubAttrs[n])->PushBack( vecAttrs[n] );
			}
		}
		break;
	default:
		{
			THROW_LOGIC_ERROR_EXCEPTION("ELI10210");
		}
		break;
	}
}
//-------------------------------------------------------------------------------------------------
void CMoveAndModifyAttributes::validateLicense()
{
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI09408", 
		"Move and Modify Attributes Output Handler" );
}
//-------------------------------------------------------------------------------------------------
