// MultiSkipConditionNONE.cpp : Implementation of CMultiFAMConditionNONE
#include "stdafx.h"
#include "MultiSkipConditionNONE.h"
#include "ESSkipConditions.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <COMUtils.h>
#include <ComponentLicenseIDs.h>
#include <cpputil.h>
#include <FAMHelperFunctions.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CMultiFAMConditionNONE
//-------------------------------------------------------------------------------------------------
CMultiFAMConditionNONE::CMultiFAMConditionNONE()
: m_bDirty(false)
, m_bIsPaginationCondition(false)
{
	m_ipGenericMultiFAMCondition = __nullptr;
	m_ipMultiFAMConditions = __nullptr;
}
//-------------------------------------------------------------------------------------------------
CMultiFAMConditionNONE::~CMultiFAMConditionNONE()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16565");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionNONE::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IFAMCondition,
		&IID_IPaginationCondition,
		&IID_IFAMCancelable,
		&IID_IInitClose,
		&IID_IParallelizableTask,
		&IID_IFAMProcessingResult,
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
STDMETHODIMP CMultiFAMConditionNONE::raw_FileMatchesFAMCondition(IFileRecord* pFileRecord, 
	IFileProcessingDB* pFPDB, long lActionID, IFAMTagManager* pFAMTM, 
	VARIANT_BOOL* pRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// call FileMatchesFAMCondition inside GenericMultiFAMCondition class
		*pRetVal = getGenericMultiFAMCondition()->FileMatchesFAMCondition(m_ipMultiFAMConditions, 
			EXTRACT_FAMCONDITIONSLib::kNONEOperator, false, pFileRecord,
			_bstr_t(""), _bstr_t(""), _bstr_t(""), pFPDB, lActionID, pFAMTM);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13862")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPaginationCondition
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionNONE::get_IsPaginationCondition(VARIANT_BOOL* pbIsPaginationCondition)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI47156", pbIsPaginationCondition != __nullptr);

		*pbIsPaginationCondition = asVariantBool(m_bIsPaginationCondition);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI47157");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionNONE::put_IsPaginationCondition(VARIANT_BOOL pbIsPaginationCondition)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_bIsPaginationCondition = asCppBool(pbIsPaginationCondition);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI47158");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionNONE::raw_FileMatchesPaginationCondition(IFileRecord* pSourceFileRecord,
	BSTR bstrProposedFileName, BSTR bstrDocumentStatus, BSTR bstrSerializedDocumentAttributes,
	IFileProcessingDB* pFPDB, long lActionID, IFAMTagManager* pFAMTagManager,
	VARIANT_BOOL* pRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

		try
	{
		// call FileMatchesFAMCondition inside GenericMultiFAMCondition class
		*pRetVal = getGenericMultiFAMCondition()->FileMatchesFAMCondition(m_ipMultiFAMConditions,
			EXTRACT_FAMCONDITIONSLib::kNONEOperator, true, pSourceFileRecord, bstrProposedFileName,
			bstrDocumentStatus, bstrSerializedDocumentAttributes, pFPDB, lActionID, pFAMTagManager);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI47159")
}
	
//-------------------------------------------------------------------------------------------------
// IAccessRequired interface implementation
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionNONE::raw_RequiresAdminAccess(VARIANT_BOOL* pbResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI31215", pbResult != __nullptr);

		*pbResult = asVariantBool(checkForRequiresAdminAccess(m_ipMultiFAMConditions));

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31216");
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionNONE::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pClassID = CLSID_MultiFAMConditionNONE;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionNONE::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionNONE::Load(IStream *pStream)
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
			UCLIDException ue( "ELI13903", 
				"Unable to load newer NONE Multi FAM Condition Sequence!" );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		// read the IIUnknownVecPtr object 
		IPersistStreamPtr ipObj;
		::readObjectFromStream(ipObj, pStream, "ELI13904");
		m_ipMultiFAMConditions = ipObj;
		ASSERT_RESOURCE_ALLOCATION("ELI13905", m_ipMultiFAMConditions != __nullptr);

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13906");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionNONE::Save(IStream *pStream, BOOL fClearDirty)
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
		if (ipObj == __nullptr)
		{
			throw UCLIDException("ELI13907", "IIUnknownVector component does not support persistence!");
		}
		writeObjectToStream(ipObj, pStream, "ELI13908", fClearDirty);

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13909");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionNONE::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionNONE::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		IMultipleObjectHolderPtr ipObj = pObject;
		ASSERT_RESOURCE_ALLOCATION("ELI13910", ipObj != __nullptr);

		// get the vector of FAM Condition from the other object
		IIUnknownVectorPtr ipMultiFAMCond = ipObj->ObjectsVector;
		ASSERT_RESOURCE_ALLOCATION("ELI13911", ipMultiFAMCond != __nullptr);

		IPaginationConditionPtr ipPaginationCondition(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI50088", ipPaginationCondition != __nullptr);
		m_bIsPaginationCondition = asVariantBool(ipPaginationCondition->IsPaginationCondition);

		// copy the vector of FAM conditions
		m_ipMultiFAMConditions = ipMultiFAMCond->Clone();
		ASSERT_RESOURCE_ALLOCATION("ELI13912", m_ipMultiFAMConditions != __nullptr);

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13913")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionNONE::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		// create another instance of this object
		ICopyableObjectPtr ipObjCopy; 
		ipObjCopy.CreateInstance(CLSID_MultiFAMConditionNONE);
		ASSERT_RESOURCE_ALLOCATION("ELI13914", ipObjCopy != __nullptr);

		// Ask the other object to copy itself from this object
		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13915");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMultipleObjectHolder
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionNONE::raw_GetObjectCategoryName(BSTR *pstrCategoryName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		if (m_bIsPaginationCondition)
		{
			// Get the category name for FAM condition
			_bstr_t _bstrCategoryName = FP_PAGINATION_CONDITION_CATEGORYNAME.c_str();
			*pstrCategoryName = _bstrCategoryName.Detach();
		}
		else
		{
			// Get the category name for FAM condition
			_bstr_t _bstrCategoryName = FP_FAM_CONDITIONS_CATEGORYNAME.c_str();
			*pstrCategoryName = _bstrCategoryName.Detach();
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13916")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionNONE::raw_GetObjectType(BSTR *pstrObjectType)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		if (m_bIsPaginationCondition)
		{
			*pstrObjectType = _bstr_t("Pagination Condition").Detach();
		}
		else
		{
			*pstrObjectType = _bstr_t("FAM Condition").Detach();
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13917")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionNONE::get_ObjectsVector(IIUnknownVector* *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		// Create m_ipMultiFAMConditions if not exist
		if (m_ipMultiFAMConditions == __nullptr)
		{
			m_ipMultiFAMConditions.CreateInstance(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI13918", m_ipMultiFAMConditions != __nullptr);
		}

		// return a reference to the internal vector
		IIUnknownVectorPtr ipShallowCopy = m_ipMultiFAMConditions;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13919")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionNONE::put_ObjectsVector(IIUnknownVector *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		// convert argument to smart pointer
		IIUnknownVectorPtr ipMultiFAMConditions(newVal);
		ASSERT_RESOURCE_ALLOCATION("ELI13920", ipMultiFAMConditions != __nullptr);

		// ensure that each of the items in the vector is an object with description
		// where the object is a FAM condition
		long nNumItems = ipMultiFAMConditions->Size();
		for (int i = 0; i < nNumItems; i++)
		{
			IObjectWithDescriptionPtr ipObj = ipMultiFAMConditions->At(i);
			if (ipObj == __nullptr)
			{
				UCLIDException ue("ELI13921", "Invalid object in vector - expecting vector of IObjectWithDescription objects!");
				ue.addDebugInfo("index", i);
				throw ue;
			}

			if (m_bIsPaginationCondition)
			{
				IPaginationConditionPtr ipPaginationCondition = ipObj->Object;
				if (ipPaginationCondition == __nullptr)
				{
					UCLIDException ue("ELI47177", "Invalid object in ObjectWithDescription - expecting IPaginationCondition object!");
					ue.addDebugInfo("index", i);
					throw ue;
				}
			}
			else
			{
				IFAMConditionPtr ipFAMCondition = ipObj->Object;
				if (ipFAMCondition == __nullptr)
				{
					UCLIDException ue("ELI13922", "Invalid object in ObjectWithDescription - expecting IFAMCondition object!");
					ue.addDebugInfo("index", i);
					throw ue;
				}
			}
		}

		// update internal variable
		m_ipMultiFAMConditions = ipMultiFAMConditions;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13923")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionNONE::raw_GetRequiredIID(IID *riid)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		if (m_bIsPaginationCondition)
		{
			*riid = IID_IPaginationCondition;
		}
		else
		{
			*riid = IID_IFAMCondition;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13924")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionNONE::raw_IsConfigured(VARIANT_BOOL * pbValue)
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
		*pbValue = asVariantBool(m_ipMultiFAMConditions != __nullptr && 
			m_ipMultiFAMConditions->Size() != 0);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13925");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionNONE::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19640", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("NONE in multiple conditions").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13926")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionNONE::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// IFAMCancelable Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionNONE::raw_Cancel()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Pass the cancel to the Generic MultiFAM condition
		IFAMCancelablePtr ipCancelable = getGenericMultiFAMCondition();
		if (ipCancelable != __nullptr)
		{
			ipCancelable->Cancel();
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37692");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionNONE::raw_IsCanceled(VARIANT_BOOL *pvbCanceled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		ASSERT_ARGUMENT("ELI37693", pvbCanceled != __nullptr);

		// Check license
		validateLicense();
		
		*pvbCanceled = VARIANT_FALSE;
		
		// Get the IsCanceled state from the Generic MultiFAM Condition
		IFAMCancelablePtr ipCancelable = getGenericMultiFAMCondition();
		if (ipCancelable != __nullptr)
		{
			*pvbCanceled = ipCancelable->IsCanceled();
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37694");
}

//-------------------------------------------------------------------------------------------------
// IParallelizableTask Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionNONE::raw_ProcessWorkItem(IWorkItemRecord *pWorkItem, long nActionID,
		IFAMTagManager* pFAMTM, IFileProcessingDB* pDB, IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		// Check license
		validateLicense();

		// Nothing to do

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37730");	
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionNONE::get_Parallelize(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		// Get the Parallelize state from the contained objects
		*pVal = getGenericMultiFAMCondition()->Parallelize(m_ipMultiFAMConditions);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37731");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionNONE::put_Parallelize(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		// This is only parallelizable if the contained task is parallelizable

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37732");
}

//-------------------------------------------------------------------------------------------------
// IInitClose Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionNONE::raw_Init(long nActionID, IFAMTagManager* pFAMTM, IFileProcessingDB* pDB,
			IFileRequestHandler* pFileRequestHandler)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Call init on all the FAMConditions that implement InitClose interface
		getGenericMultiFAMCondition()->Init(m_ipMultiFAMConditions, nActionID, pFAMTM, pDB, pFileRequestHandler);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37742");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionNONE::raw_Close()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Call Close on all the FAMConditions that implement InitClose interface
		getGenericMultiFAMCondition()->Close(m_ipMultiFAMConditions);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37743");
}

//-------------------------------------------------------------------------------------------------
// IFAMProcessingResult Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultiFAMConditionNONE::raw_GetResult(EFileProcessingResult* pResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		IFAMProcessingResultPtr ipProcessingResult = getGenericMultiFAMCondition();

		*pResult = kProcessingSuccessful;
		if (ipProcessingResult != __nullptr)
		{
			// Call Close on the task
			*pResult = ipProcessingResult->GetResult();
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37775");
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CMultiFAMConditionNONE::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI13927", "NONE in Multiple FAM Condition");
}
//-------------------------------------------------------------------------------------------------
EXTRACT_FAMCONDITIONSLib::IGenericMultiFAMConditionPtr CMultiFAMConditionNONE::getGenericMultiFAMCondition()
{
	// Create m_ipGenericMultiFAMCondition if not exist
	if (m_ipGenericMultiFAMCondition == __nullptr)
	{
		m_ipGenericMultiFAMCondition.CreateInstance(CLSID_GenericMultiFAMCondition);
		ASSERT_RESOURCE_ALLOCATION("ELI37691", m_ipGenericMultiFAMCondition != __nullptr);
	}
	return m_ipGenericMultiFAMCondition;
}
//-------------------------------------------------------------------------------------------------
