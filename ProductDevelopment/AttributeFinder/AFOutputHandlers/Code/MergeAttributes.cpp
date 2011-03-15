// MergeAttributes.cpp : Implementation of CMergeAttributes

#include "stdafx.h"
#include "MergeAttributes.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
// Version 2: Added CreateMergedRegion. Default is false, however when version 1 files are loaded it
// will default to true to maintain behavior for version 1 rule objects.
const unsigned long gnCurrentVersion			= 2;
const unsigned long gnDEFAULT_OVERLAP_PERCENT	= 75;

//--------------------------------------------------------------------------------------------------
// CMergeAttributes
//--------------------------------------------------------------------------------------------------
CMergeAttributes::CMergeAttributes()
: m_bDirty(false)
, m_ipAttributeMerger(__nullptr)
{
	try
	{
		reset();

		m_ipAFUtility.CreateInstance(CLSID_AFUtility);
		ASSERT_RESOURCE_ALLOCATION("ELI22876", m_ipAFUtility != __nullptr);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI22730");
}
//--------------------------------------------------------------------------------------------------
CMergeAttributes::~CMergeAttributes()
{
	try
	{
		m_ipAFUtility = __nullptr;
		m_ipNameMergePriority = __nullptr;
		m_ipAttributeMerger = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI22731");
}
//-------------------------------------------------------------------------------------------------
HRESULT CMergeAttributes::FinalConstruct()
{
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
void CMergeAttributes::FinalRelease()
{
	try
	{
		m_ipAFUtility = __nullptr;
		m_ipNameMergePriority = __nullptr;
		m_ipAttributeMerger = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI32139");
}

//--------------------------------------------------------------------------------------------------
// IMergeAttributes
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::get_AttributeQuery(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22809", pVal != __nullptr);

		validateLicense();
		
		*pVal = get_bstr_t(m_strAttributeQuery).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22810")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::put_AttributeQuery(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22941", !asString(newVal).empty());

		validateLicense();

		m_strAttributeQuery = asString(newVal);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22811")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::get_OverlapPercent(double *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22814", pVal != __nullptr);

		validateLicense();
		
		*pVal = m_dOverlapPercent;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22815")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::put_OverlapPercent(double newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22915", newVal >= 0 && newVal <= 100);

		validateLicense();

		m_dOverlapPercent = newVal;

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22816")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::get_NameMergeMode(EFieldMergeMode *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22817", pVal != __nullptr);

		validateLicense();
		
		*pVal = m_eNameMergeMode;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22818")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::put_NameMergeMode(EFieldMergeMode newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22931", newVal == kSpecifyField || newVal == kPreserveField);

		validateLicense();

		m_eNameMergeMode = newVal;

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22819")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::get_TypeMergeMode(EFieldMergeMode *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22820", pVal != __nullptr);

		validateLicense();
		
		*pVal = m_eTypeMergeMode;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22821")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::put_TypeMergeMode(EFieldMergeMode newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22930", newVal == kSpecifyField || newVal == kCombineField);

		validateLicense();

		m_eTypeMergeMode = newVal;

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22822")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::get_SpecifiedName(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22823", pVal != __nullptr);

		validateLicense();
		
		*pVal = get_bstr_t(m_strSpecifiedName).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22824")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::put_SpecifiedName(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_strSpecifiedName = asString(newVal);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22825")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::get_SpecifiedType(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22826", pVal != __nullptr);

		validateLicense();
		
		*pVal = get_bstr_t(m_strSpecifiedType).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22827")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::put_SpecifiedType(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_strSpecifiedType = asString(newVal);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22828")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::get_SpecifiedValue(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22905", pVal != __nullptr);

		validateLicense();
		
		*pVal = get_bstr_t(m_strSpecifiedValue).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22906")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::put_SpecifiedValue(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22914", !asString(newVal).empty());

		validateLicense();

		m_strSpecifiedValue = asString(newVal);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22907")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::get_NameMergePriority(IVariantVector **ppVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22829", ppVal != __nullptr);

		validateLicense();
		
		IVariantVectorPtr ipShallowCopy = m_ipNameMergePriority;
		*ppVal = ipShallowCopy.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22830")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::put_NameMergePriority(IVariantVector *pNewVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22842", pNewVal != __nullptr);

		validateLicense();

		m_ipNameMergePriority = pNewVal;

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22831")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::get_PreserveAsSubAttributes(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22911", pVal != __nullptr);

		validateLicense();
		
		*pVal = asVariantBool(m_bPreserveAsSubAttributes);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22912")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::put_PreserveAsSubAttributes(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bPreserveAsSubAttributes = asCppBool(newVal);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22913")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::get_CreateMergedRegion(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI30839", pVal != __nullptr);

		validateLicense();
		
		*pVal = asVariantBool(m_bCreateMergedRegion);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI30840")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::put_CreateMergedRegion(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bCreateMergedRegion = asCppBool(newVal);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI30841")
}

//--------------------------------------------------------------------------------------------------
// IOutputHandler
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::raw_ProcessOutput(IIUnknownVector* pAttributes, IAFDocument *pAFDoc,
												 IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		IIUnknownVectorPtr ipAttributes(pAttributes);
		ASSERT_ARGUMENT("ELI22877", ipAttributes != __nullptr);

		IAFDocumentPtr ipAFDocument(pAFDoc);
		ASSERT_ARGUMENT("ELI25276", ipAFDocument != __nullptr);

		// Obtain the set of attributes eligible for merging
		IIUnknownVectorPtr ipTargetAttributes = 
			m_ipAFUtility->QueryAttributes(ipAttributes, m_strAttributeQuery.c_str(), VARIANT_FALSE);
		ASSERT_RESOURCE_ALLOCATION("ELI22883", ipTargetAttributes != __nullptr);

		ISpatialAttributeMergeUtilsPtr ipAttributeMerger = getAttributeMerger();

		// Perform the merge
		ipAttributeMerger->FindQualifiedMerges(ipTargetAttributes, ipAFDocument->Text);
		ipAttributeMerger->ApplyMerges(ipAttributes);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22732")
}

//--------------------------------------------------------------------------------------------------
// IPersistStream
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI22733", pClassID != __nullptr);

		*pClassID = CLSID_MergeAttributes;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22734");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		return m_bDirty ? S_OK : S_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22736");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		ASSERT_ARGUMENT("ELI22737", pStream != __nullptr);

		// Reset data members
		reset();
		
		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		pStream->Read(&nDataLength, sizeof(nDataLength), __nullptr);
		ByteStream data(nDataLength);
		pStream->Read(data.getData(), nDataLength, __nullptr);
		ByteStreamManipulator dataReader(ByteStreamManipulator::kRead, data);

		// Read the individual data items from the bytestream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;

		// Check for newer version
		if (nDataVersion > gnCurrentVersion)
		{
			// Throw exception
			UCLIDException ue("ELI22738", 
				"Unable to load newer merge attributes rule!");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		// Read data members from the stream
		dataReader >> m_strAttributeQuery;
		dataReader >> m_dOverlapPercent;
		long nTemp;
		dataReader >> nTemp;
		m_eNameMergeMode = (EFieldMergeMode) nTemp;
		dataReader >> nTemp;
		m_eTypeMergeMode = (EFieldMergeMode) nTemp;
		dataReader >> m_strSpecifiedName;
		dataReader >> m_strSpecifiedType;
		dataReader >> m_strSpecifiedValue;
		dataReader >> m_bPreserveAsSubAttributes;
		if (nDataVersion == 1)
		{
			// For old version of the rule, set m_bCreateMergedRegion = true so that the rule's
			// behavior is not changed.
			m_bCreateMergedRegion = true;
		}
		else
		{
			dataReader >> m_bCreateMergedRegion;
		}

		// Clone the NameMergePriority member
		IPersistStreamPtr ipNameMergePriority;
		readObjectFromStream(ipNameMergePriority, pStream, "ELI22844");
		m_ipNameMergePriority = ipNameMergePriority;

		// Reset m_ipAttributeMerger to force it to be re-loaded next time the rule is run.
		m_ipAttributeMerger = __nullptr;

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22739");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		ASSERT_ARGUMENT("ELI22740", pStream != __nullptr);

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		// Write the current version
		dataWriter << gnCurrentVersion;

		// Write the data members to the stream
		dataWriter << m_strAttributeQuery;
		dataWriter << m_dOverlapPercent;
		dataWriter << (long) m_eNameMergeMode;
		dataWriter << (long) m_eTypeMergeMode;
		dataWriter << m_strSpecifiedName;
		dataWriter << m_strSpecifiedType;
		dataWriter << m_strSpecifiedValue;
		dataWriter << m_bPreserveAsSubAttributes;

		dataWriter << m_bCreateMergedRegion;

		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), __nullptr);
		pStream->Write(data.getData(), nDataLength, __nullptr);

		// Write the NameMergePriority list to the stream
		IPersistStreamPtr ipNameMergePriority(m_ipNameMergePriority);
		ASSERT_RESOURCE_ALLOCATION("ELI22846", ipNameMergePriority != __nullptr);
		writeObjectToStream(ipNameMergePriority, pStream, "ELI22847", fClearDirty);

		// Clear the flag as specified
		if (fClearDirty)
		{
			// Reset m_ipAttributeMerger to force it to be re-loaded next time the rule is run.
			m_ipAttributeMerger = __nullptr;

			m_bDirty = false;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22741");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return E_NOTIMPL;
}

//--------------------------------------------------------------------------------------------------
// ICategorizedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::raw_GetComponentDescription(BSTR *pbstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI22742", pbstrComponentDescription != __nullptr)

		*pbstrComponentDescription = _bstr_t("Merge attributes").Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22743")
}

//--------------------------------------------------------------------------------------------------
// ILicensedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI22744", pbValue != __nullptr);

		try
		{
			// check the license
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22745");
}

//--------------------------------------------------------------------------------------------------
// ICopyableObject
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license
		validateLicense();

		UCLID_AFOUTPUTHANDLERSLib::IMergeAttributesPtr ipCopyThis = pObject;
		ASSERT_ARGUMENT("ELI22746", ipCopyThis != __nullptr);

		// Copy members
		m_strAttributeQuery			= asString(ipCopyThis->AttributeQuery);
		m_dOverlapPercent			= ipCopyThis->OverlapPercent;
		m_eNameMergeMode			= (EFieldMergeMode) ipCopyThis->NameMergeMode;
		m_eTypeMergeMode			= (EFieldMergeMode) ipCopyThis->TypeMergeMode;
		m_strSpecifiedName			= asString(ipCopyThis->SpecifiedName);
		m_strSpecifiedType			= asString(ipCopyThis->SpecifiedType);
		m_strSpecifiedValue			= asString(ipCopyThis->SpecifiedValue);
		m_bPreserveAsSubAttributes	= asCppBool(ipCopyThis->PreserveAsSubAttributes);
		m_bCreateMergedRegion		= asCppBool(ipCopyThis->CreateMergedRegion);

		// Clone the NameMergePriority member
		ICopyableObjectPtr ipCopyableNameMergePriority = ipCopyThis->NameMergePriority;
		ASSERT_RESOURCE_ALLOCATION("ELI22836", ipCopyableNameMergePriority != __nullptr);
		m_ipNameMergePriority = ipCopyableNameMergePriority->Clone();
		ASSERT_RESOURCE_ALLOCATION("ELI22837", m_ipNameMergePriority != __nullptr);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22747");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license
		validateLicense();

		ASSERT_ARGUMENT("ELI22748", pObject != __nullptr);

		// Create another instance of this object
		ICopyableObjectPtr ipObjCopy(CLSID_MergeAttributes);
		ASSERT_RESOURCE_ALLOCATION("ELI22749", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);
	
		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22750");
}

//--------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::raw_IsConfigured(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Check parameter
		ASSERT_ARGUMENT("ELI22751", pbValue != __nullptr);

		*pbValue = VARIANT_TRUE;

		if (m_strAttributeQuery.empty())
		{
			// An attribute query must be specified.
			*pbValue =  VARIANT_FALSE;
		}
		else if (m_strSpecifiedValue.empty())
		{
			// The value for merge results must be specified.
			*pbValue =  VARIANT_FALSE;
		}
		else if (m_dOverlapPercent < 0 || m_dOverlapPercent > 100)
		{
			// The overlap percent must be between 0 and 100
			*pbValue =  VARIANT_FALSE;
		}
		else if (m_eNameMergeMode != kSpecifyField && m_eNameMergeMode != kPreserveField)
		{
			// The name can be specified only via direct specification, or via preservation.
			*pbValue =  VARIANT_FALSE;
		}
		else if (m_eTypeMergeMode != kSpecifyField && m_eTypeMergeMode != kCombineField)
		{
			// The type can be specified only via direct specification, or combination.
			*pbValue =  VARIANT_FALSE;
		}
		else if (m_eNameMergeMode == kSpecifyField && m_strSpecifiedName.empty())
		{
			// If using name specification, the name specification field must be filled.
			*pbValue =  VARIANT_FALSE;
		}
		else if (m_eNameMergeMode == kPreserveField && m_ipNameMergePriority->Size == 0)
		{
			// If using name preservation, at least one entry must be in the name preservation list.
			*pbValue =  VARIANT_FALSE;
		}
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22752");
}

//---------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//---------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::InterfaceSupportsErrorInfo(REFIID riid)
{
	try
	{
		static const IID* arr[] = 
		{
			&IID_IMergeAttributes,
			&IID_IOutputHandler,
			&IID_IPersistStream,
			&IID_ICategorizedComponent,
			&IID_ISpecifyPropertyPages,
			&IID_ICopyableObject,
			&IID_IMustBeConfiguredObject,
			&IID_ILicensedComponent
		};

		for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
		{
			if (InlineIsEqualGUID(*arr[i],riid))
				return S_OK;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22753")
}

//--------------------------------------------------------------------------------------------------
// Private Methods
//--------------------------------------------------------------------------------------------------
void CMergeAttributes::reset()
{
	// Reset all the rules settings.
	m_strAttributeQuery.clear();
	m_dOverlapPercent = gnDEFAULT_OVERLAP_PERCENT;
	m_eNameMergeMode = kSpecifyField;
	m_eTypeMergeMode = kSpecifyField;
	m_strSpecifiedName.clear();
	m_strSpecifiedType.clear();
	m_strSpecifiedValue = "000-00-0000";
	m_bPreserveAsSubAttributes = true;
	m_bCreateMergedRegion = false;

	m_ipNameMergePriority.CreateInstance(CLSID_VariantVector);
	ASSERT_RESOURCE_ALLOCATION("ELI22840", m_ipNameMergePriority != __nullptr);
}
//--------------------------------------------------------------------------------------------------
ISpatialAttributeMergeUtilsPtr CMergeAttributes::getAttributeMerger()
{
	if (m_ipAttributeMerger == __nullptr || m_bDirty)
	{
		// Create an AttributeMergeUtils instance to perform the merge.
		m_ipAttributeMerger.CreateInstance(CLSID_SpatialAttributeMergeUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI32064", m_ipAttributeMerger != __nullptr);

		// Apply the merge settings.
		m_ipAttributeMerger->OverlapPercent = m_dOverlapPercent;
		m_ipAttributeMerger->NameMergeMode = m_eNameMergeMode;
		m_ipAttributeMerger->TypeMergeMode = m_eTypeMergeMode;
		m_ipAttributeMerger->SpecifiedName = m_strSpecifiedName.c_str();
		m_ipAttributeMerger->SpecifiedType = m_strSpecifiedType.c_str();
		m_ipAttributeMerger->SpecifiedValue = m_strSpecifiedValue.c_str();
		m_ipAttributeMerger->NameMergePriority = m_ipNameMergePriority;
		m_ipAttributeMerger->PreserveAsSubAttributes = asVariantBool(m_bPreserveAsSubAttributes);
		m_ipAttributeMerger->CreateMergedRegion = asVariantBool(m_bCreateMergedRegion);

		// Use the largest percentage of mutual overlap between the two. [FlexIDSCore #3509]
		m_ipAttributeMerger->UseMutualOverlap = VARIANT_FALSE;
	}

	return m_ipAttributeMerger;
}
//--------------------------------------------------------------------------------------------------
void CMergeAttributes::validateLicense()
{
	VALIDATE_LICENSE(gnFLEXINDEX_IDSHIELD_CORE_OBJECTS, "ELI22754", 
		"Merge attributes output handler");
}
//--------------------------------------------------------------------------------------------------