// MultiSkipConditionOR.cpp : Implementation of CMultiFAMConditionOR
#include "stdafx.h"
#include "MultiSkipConditionOR.h"
#include "ESSkipConditions.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <COMUtils.h>
#include <ComponentLicenseIDs.h>
#include <cpputil.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CMultiFAMConditionOR
//-------------------------------------------------------------------------------------------------
CMultiFAMConditionOR::CMultiFAMConditionOR()
: m_bDirty(false)
{
	m_ipGenericMultiFAMCondition = NULL;
	m_ipMultiFAMConditions = NULL;
}
//-------------------------------------------------------------------------------------------------
CMultiFAMConditionOR::~CMultiFAMConditionOR()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16566");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionOR::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IFAMCondition,
		&IID_ILicensedComponent,
		&IID_ICategorizedComponent,
		&IID_ISpecifyPropertyPages,
		&IID_ICopyableObject,
		&IID_IMustBeConfiguredObject,
		&IID_IPersistStream,
		&IID_IMultipleObjectHolder
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IFAMCondition
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionOR::raw_FileMatchesFAMCondition(BSTR bstrFile, 
	IFileProcessingDB* pFPDB, long lFileID, long lActionID, IFAMTagManager* pFAMTM, 
	VARIANT_BOOL* pRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Create m_ipGenericMultiFAMCondition if not exist
		if (m_ipGenericMultiFAMCondition == NULL)
		{
			m_ipGenericMultiFAMCondition.CreateInstance(CLSID_GenericMultiFAMCondition);
			ASSERT_RESOURCE_ALLOCATION("ELI13864", m_ipGenericMultiFAMCondition != NULL);
		}

		// call FileMatchesFAMCondition inside GenericMultiFAMCondition class
		*pRetVal = m_ipGenericMultiFAMCondition->FileMatchesFAMCondition(m_ipMultiFAMConditions, 
			EXTRACT_FAMCONDITIONSLib::kOROperator, bstrFile, pFPDB, lFileID, lActionID, pFAMTM);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13866")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionOR::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pClassID = CLSID_MultiFAMConditionOR;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionOR::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionOR::Load(IStream *pStream)
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

		// Check for newer version
		if (nDataVersion > gnCurrentVersion)
		{
			// Throw exception
			UCLIDException ue( "ELI13867", 
				"Unable to load newer OR Multi FAM Condition Sequence!" );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		// read the IIUnknownVecPtr object 
		IPersistStreamPtr ipObj;
		::readObjectFromStream(ipObj, pStream, "ELI13868");
		m_ipMultiFAMConditions = ipObj;
		ASSERT_RESOURCE_ALLOCATION("ELI13869", m_ipMultiFAMConditions != NULL);

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13870");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionOR::Save(IStream *pStream, BOOL fClearDirty)
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
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write( &nDataLength, sizeof(nDataLength), NULL );
		pStream->Write( data.getData(), nDataLength, NULL );

		// write the vector of FAM conditions to the stream
		IPersistStreamPtr ipObj = m_ipMultiFAMConditions;
		if (ipObj == NULL)
		{
			throw UCLIDException("ELI13871", "IIUnknownVector component does not support persistence!");
		}
		writeObjectToStream(ipObj, pStream, "ELI13872", fClearDirty);

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13873");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionOR::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionOR::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		IMultipleObjectHolderPtr ipObj = pObject;
		ASSERT_RESOURCE_ALLOCATION("ELI13874", ipObj != NULL);

		// get the vector of FAM Condition from the other object
		IIUnknownVectorPtr ipMultiFAMCond = ipObj->ObjectsVector;
		ASSERT_RESOURCE_ALLOCATION("ELI13875", ipMultiFAMCond != NULL);

		// copy the vector of FAM conditions
		m_ipMultiFAMConditions = ipMultiFAMCond->Clone();
		ASSERT_RESOURCE_ALLOCATION("ELI13876", m_ipMultiFAMConditions != NULL);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13877")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionOR::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		// create another instance of this object
		ICopyableObjectPtr ipObjCopy; 
		ipObjCopy.CreateInstance(CLSID_MultiFAMConditionOR);
		ASSERT_RESOURCE_ALLOCATION("ELI13878", ipObjCopy != NULL);

		// Ask the other object to copy itself from this object
		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13879");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMultipleObjectHolder
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionOR::raw_GetObjectCategoryName(BSTR *pstrCategoryName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Get the category name for FAM condition
		_bstr_t _bstrCategoryName = FP_FAM_CONDITIONS_CATEGORYNAME.c_str();
		*pstrCategoryName = _bstrCategoryName.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13880")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionOR::raw_GetObjectType(BSTR *pstrObjectType)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		*pstrObjectType = _bstr_t("FAM Condition").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13881")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionOR::get_ObjectsVector(IIUnknownVector* *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		// Create m_ipMultiFAMConditions if not exist
		if (m_ipMultiFAMConditions == NULL)
		{
			m_ipMultiFAMConditions.CreateInstance(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI13882", m_ipMultiFAMConditions != NULL);
		}

		// return a reference to the internal vector
		IIUnknownVectorPtr ipShallowCopy = m_ipMultiFAMConditions;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13883")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionOR::put_ObjectsVector(IIUnknownVector *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		// convert argument to smart pointer
		IIUnknownVectorPtr ipMultiFAMConditions(newVal);
		ASSERT_RESOURCE_ALLOCATION("ELI13884", ipMultiFAMConditions != NULL);

		// ensure that each of the items in the vector is an object with description
		// where the object is a FAM condition
		long nNumItems = ipMultiFAMConditions->Size();
		for (int i = 0; i < nNumItems; i++)
		{
			IObjectWithDescriptionPtr ipObj = ipMultiFAMConditions->At(i);
			if (ipObj == NULL)
			{
				UCLIDException ue("ELI13885", "Invalid object in vector - expecting vector of IObjectWithDescription objects!");
				ue.addDebugInfo("index", i);
				throw ue;
			}

			IFAMConditionPtr ipFAMCondition = ipObj->Object;
			if (ipFAMCondition == NULL)
			{
				UCLIDException ue("ELI13886", "Invalid object in ObjectWithDescription - expecting IFAMCondition object!");
				ue.addDebugInfo("index", i);
				throw ue;
			}
		}

		// update internal variable
		m_ipMultiFAMConditions = ipMultiFAMConditions;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13887")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionOR::raw_GetRequiredIID(IID *riid)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		*riid = IID_IFAMCondition;

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13888")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionOR::raw_IsConfigured(VARIANT_BOOL * pbValue)
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

		// This object is considered configured if
		// there's at least one object in the vector
		*pbValue = asVariantBool(m_ipMultiFAMConditions != NULL && 
			m_ipMultiFAMConditions->Size() != 0);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13889");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionOR::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19641", pstrComponentDescription != NULL)

		*pstrComponentDescription = _bstr_t("OR multiple conditions").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13890")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionOR::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// Helper functions
//-------------------------------------------------------------------------------------------------
void CMultiFAMConditionOR::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI13891", "OR Multiple FAM Condition");
}
//-------------------------------------------------------------------------------------------------
