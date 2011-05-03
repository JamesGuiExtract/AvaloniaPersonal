// RunObjectOnQuery.cpp : Implementation of CRunObjectOnQuery
#include "stdafx.h"
#include "AFOutputHandlers.h"
#include "RunObjectOnQuery.h"

#include <AFCategories.h>
#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>
#include <cpputil.h>
#include <VectorOperations.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 4;

//-------------------------------------------------------------------------------------------------
// CRunObjectOnQuery
//-------------------------------------------------------------------------------------------------
CRunObjectOnQuery::CRunObjectOnQuery()
: m_bDirty(false),
  m_ipObject(NULL),
  m_objectIID(IID_IAttributeModifyingRule)
{
	try
	{
		m_ipAFUtility.CreateInstance(CLSID_AFUtility);
		ASSERT_RESOURCE_ALLOCATION("ELI10384", m_ipAFUtility != __nullptr);
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI10433");
}
//-------------------------------------------------------------------------------------------------
CRunObjectOnQuery::~CRunObjectOnQuery()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16318");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRunObjectOnQuery::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IRunObjectOnQuery,
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
// IRunObjectOnQuery
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRunObjectOnQuery::get_AttributeQuery(/*[out, retval]*/ BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Validate argument
		ASSERT_ARGUMENT( "ELI10379", pVal != __nullptr );

		// Return the Attribute Name
		*pVal = get_bstr_t( m_strAttributeQuery).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10381")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRunObjectOnQuery::put_AttributeQuery(/*[in]*/ BSTR newVal)
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
			UCLIDException ue( "ELI10380", "The Attribute Query must not be empty!");
			throw ue;
		}

		// make sure the string is a valid query
		if(m_ipAFUtility->IsValidQuery(newVal) == VARIANT_FALSE)
		{
			UCLIDException ue( "ELI10434", "Invalid Attribute Query!");
			throw ue;
		}

		// Store the Attribute Name
		m_strAttributeQuery = strQuery;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10382")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRunObjectOnQuery::GetObjectAndIID(IID *pIID, ICategorizedComponent **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// set the IID for return
		*pIID = m_objectIID;

		// Set the Object for return
		CComQIPtr<ICategorizedComponent> ipTemp = m_ipObject;
		ipTemp.CopyTo(pObject);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10400")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRunObjectOnQuery::SetObjectAndIID(IID newIID, ICategorizedComponent *newObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();


		// The IID must be one of these interfaces
		if( newIID != IID_IAttributeModifyingRule && 
			newIID != IID_IAttributeSplitter &&
			newIID != IID_IOutputHandler )
		{
			UCLIDException ue("ELI10401", "Invalid Object Type.");
		//	ue.addDebugInfo("Type", (long)newVal);
			throw ue;
		}
		
		ICategorizedComponentPtr ipTmp(newObject);
		ASSERT_RESOURCE_ALLOCATION("ELI10414", ipTmp != __nullptr);

		IUnknown* pUnknown;
		ipTmp->QueryInterface(newIID, (void**)&pUnknown);
		if(pUnknown == NULL)
		{
			UCLIDException ue("ELI10428", "The specified Object does not implement the specified interface.");
			throw ue;
		}
		// Save the object
		m_ipObject = ipTmp;

		// save the IID
		m_objectIID = newIID;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10412")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IOutputHandler
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRunObjectOnQuery::raw_ProcessOutput(IIUnknownVector* pAttributes, IAFDocument *pAFDoc,
												  IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// Local copy of original Attributes
		IIUnknownVectorPtr ipOrigAttributes( pAttributes );
		ASSERT_ARGUMENT("ELI10383", ipOrigAttributes != __nullptr);

		IAFDocumentPtr ipAFDoc(pAFDoc);
		ASSERT_RESOURCE_ALLOCATION("ELI10423", ipAFDoc != __nullptr);

		// use a smart pointer for the progress status object
		IProgressStatusPtr ipProgressStatus = pProgressStatus;

		// calculate the progress items count
		const long lPROGRESS_ITEMS_PER_OBJECT_QUERY = 1;
		const long lPROGRESS_ITEMS_PER_OBJECT = 1;
		const long lTOTAL_PROGRESS_ITEMS = lPROGRESS_ITEMS_PER_OBJECT_QUERY +
			lPROGRESS_ITEMS_PER_OBJECT;

		// initialize the progress status if the caller requested it
		if (ipProgressStatus)
		{
			ipProgressStatus->InitProgressStatus("Initializing run on object query",
				0, lTOTAL_PROGRESS_ITEMS, VARIANT_FALSE);

			ipProgressStatus->StartNextItemGroup("Running query for object",
				lPROGRESS_ITEMS_PER_OBJECT_QUERY); 
		}

		// query the vector for desired attributes
		IIUnknownVectorPtr ipQueriedAttributes = m_ipAFUtility->QueryAttributes(
				ipOrigAttributes, get_bstr_t(m_strAttributeQuery), VARIANT_FALSE);

		// update the progress status if it exists
		if (ipProgressStatus)
		{
			ipProgressStatus->StartNextItemGroup("Running object", lPROGRESS_ITEMS_PER_OBJECT);
		}

		// Run the object in the appropriate way depending on what type of 
		// object it is
		if(m_objectIID == IID_IAttributeModifyingRule)
		{
			runAttributeModifier(ipQueriedAttributes, ipAFDoc);
		}
		else if(m_objectIID == IID_IAttributeSplitter)
		{
			runAttributeSplitter(ipQueriedAttributes, ipAFDoc);
		}
		else if(m_objectIID == IID_IOutputHandler)
		{
			runOutputHandler(ipOrigAttributes, ipQueriedAttributes, ipAFDoc);
		}
		else
		{
			THROW_LOGIC_ERROR_EXCEPTION("ELI10422");
		}
		
		// mark progress status as complete if it exists
		if (ipProgressStatus)
		{
			ipProgressStatus->CompleteCurrentItemGroup();
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10385")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRunObjectOnQuery::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CRunObjectOnQuery::raw_IsConfigured(VARIANT_BOOL * pbValue)
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
		bool bIsConfigured = true;

		// TODO check for config
		if(m_strAttributeQuery.empty())
		{
			bIsConfigured = false;
		}
		else if(m_ipObject == __nullptr)
		{
			bIsConfigured = false;
		}
		*pbValue = asVariantBool(bIsConfigured);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10386");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRunObjectOnQuery::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19554", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Run object on query").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10387")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRunObjectOnQuery::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_AFOUTPUTHANDLERSLib::IRunObjectOnQueryPtr ipSource = pObject;
		ASSERT_RESOURCE_ALLOCATION( "ELI10388", ipSource != __nullptr );
		
		// Copy Attribute query
		m_strAttributeQuery = asString( ipSource->AttributeQuery );
		
		// Copy the IID and the Object
		ICopyableObjectPtr ipCopyObj(ipSource->GetObjectAndIID(&m_objectIID));
		if(ipCopyObj != __nullptr)
		{
			m_ipObject = ipCopyObj->Clone();
		}
		else
		{
			m_ipObject = __nullptr;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12820");
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRunObjectOnQuery::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		// Create another instance of this object
		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance( CLSID_RunObjectOnQuery );
		ASSERT_RESOURCE_ALLOCATION( "ELI10390", ipObjCopy != __nullptr );

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom( ipUnk );
	
		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10389");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRunObjectOnQuery::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pClassID = CLSID_RunObjectOnQuery;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRunObjectOnQuery::IsDirty(void)
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
STDMETHODIMP CRunObjectOnQuery::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Reset member variables
		m_strAttributeQuery = "";

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
			UCLIDException ue( "ELI10391", 
				"Unable to load newer Run Object On Query Output Handler!" );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		dataReader >> m_strAttributeQuery;
		dataReader >> m_objectIID.Data1;
		dataReader >> m_objectIID.Data2;
		dataReader >> m_objectIID.Data3;
		int i;
		for(i = 0; i < 8; i++)
		{
			dataReader >> m_objectIID.Data4[i];
		}


		// read the object
		IPersistStreamPtr ipObj;
		::readObjectFromStream(ipObj, pStream, "ELI09983");
		m_ipObject = ipObj;

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10392");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRunObjectOnQuery::Save(IStream *pStream, BOOL fClearDirty)
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
		dataWriter << m_objectIID.Data1;
		dataWriter << m_objectIID.Data2;
		dataWriter << m_objectIID.Data3;
		int i;
		for(i = 0; i < 8; i++)
		{
			dataWriter << m_objectIID.Data4[i];
		}
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write( &nDataLength, sizeof(nDataLength), NULL );
		pStream->Write( data.getData(), nDataLength, NULL );

		// write the object out
		IPersistStreamPtr ipObj = m_ipObject;
		ASSERT_RESOURCE_ALLOCATION("ELI10417", ipObj != __nullptr);
		writeObjectToStream(ipObj, pStream, "ELI10418", fClearDirty);

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10393");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRunObjectOnQuery::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CRunObjectOnQuery::validateLicense()
{
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI10394", 
		"Run Object On Query Output Handler" );
}
//-------------------------------------------------------------------------------------------------
void CRunObjectOnQuery::runAttributeModifier(IIUnknownVectorPtr ipAttributes, IAFDocumentPtr ipAFDoc)
{
	IAttributeModifyingRulePtr ipModifier(m_ipObject);
	ASSERT_RESOURCE_ALLOCATION("ELI10419", ipModifier != __nullptr);

	long nNum = ipAttributes->Size();
	int i;
	for (i = 0; i < nNum; i++)
	{
		IAttributePtr ipAttr = ipAttributes->At(i);
		ipModifier->ModifyValue(ipAttr, ipAFDoc, NULL);
	}
}
//-------------------------------------------------------------------------------------------------
void CRunObjectOnQuery::runAttributeSplitter(IIUnknownVectorPtr ipAttributes, IAFDocumentPtr ipAFDoc)
{
	IAttributeSplitterPtr ipSplitter(m_ipObject);
	ASSERT_RESOURCE_ALLOCATION("ELI10420", ipSplitter != __nullptr);

	long nNum = ipAttributes->Size();
	int i;
	for (i = 0; i < nNum; i++)
	{
		IAttributePtr ipAttr = ipAttributes->At(i);
		ipSplitter->SplitAttribute(ipAttr, ipAFDoc, NULL);
	}
}
//-------------------------------------------------------------------------------------------------
void CRunObjectOnQuery::runOutputHandler(IIUnknownVectorPtr ipAttributesOut, 
										 IIUnknownVectorPtr ipAttributesSelected, IAFDocumentPtr ipAFDoc)
{
	IOutputHandlerPtr ipOH(m_ipObject);
	ASSERT_RESOURCE_ALLOCATION("ELI10421", ipOH != __nullptr);

	// Save the original contents of the ipAttributesSelected so removed or added attributes can
	// be added or removed from the ipAttributesOut vector
	vector<IAttributePtr> vecOriginalSelectedAttributes;
	long lSize = ipAttributesSelected->Size();
	for (long l = 0; l < lSize; l++)
	{
		vecOriginalSelectedAttributes.push_back(ipAttributesSelected->At(l));
	}

	// Run the Output handler
	ipOH->ProcessOutput(ipAttributesSelected, ipAFDoc, NULL);

	// Need to add or remove attributes from the ipAttributesOut that have
	// been added or removed from the ipAttributesSelected
	// After this loop, the vecOriginalSelectedAttributes will contain all the attributes that
	// have been removed by the Output handler and will need to be removed from the output
	lSize = ipAttributesSelected->Size();
	for (long i = 0; i < lSize; i++ )
	{
		// get the attribute
		IAttributePtr iAttr = ipAttributesSelected->At(i);
		
		// If the attribute is not in the vecOriginalSelectedAttributes it will need to 
		// be added to the output
		if (!vectorContainsElement(vecOriginalSelectedAttributes, iAttr))
		{
			ipAttributesOut->PushBack(iAttr);
		}
		else
		{
			// The attribute is in both the Original selected and the selected that has 
			// been modified by the output handler so remove it from the vecOriginalSelectedAttributes
			eraseFromVector(vecOriginalSelectedAttributes, iAttr);
		}
	}

	// Remove any remaining items in vecOriginalSelectedAttributes from ipAttributesOut vector
	vector<IAttributePtr>::iterator iterAttribute = vecOriginalSelectedAttributes.begin();
	while (iterAttribute != vecOriginalSelectedAttributes.end())
	{
		// Remove the attribute from the output vector
		ipAttributesOut->RemoveValue(*iterAttribute);
		iterAttribute++;
	}
}
//-------------------------------------------------------------------------------------------------
