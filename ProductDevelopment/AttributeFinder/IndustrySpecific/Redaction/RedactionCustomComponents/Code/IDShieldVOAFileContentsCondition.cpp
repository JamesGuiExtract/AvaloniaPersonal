// IDShieldVOAFileContentsCondition.cpp : Implementation of CIDShieldVOAFileContentsCondition

#include "stdafx.h"
#include "IDShieldVOAFileContentsCondition.h"
#include "RedactionCCUtils.h"

#include <AttributeTester.h>
#include <ComponentLicenseIDs.h>
#include <ComUtils.h>
#include <LicenseMgmt.h>
#include <UCLIDException.h>

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
// current version
const unsigned long gnCurrentVersion = 4;
// attribute names
const string gstrATTRIBUTE_HCDATA	= "HCData";
const string gstrATTRIBUTE_MCDATA	= "MCData";
const string gstrATTRIBUTE_LCDATA	= "LCData";
const string gstrATTRIBUTE_MANUAL	= "Manual";
const string gstrATTRIBUTE_CLUES	= "Clues";

//--------------------------------------------------------------------------------------------------
// CIDShieldVOAFileContentsCondition
//--------------------------------------------------------------------------------------------------
CIDShieldVOAFileContentsCondition::CIDShieldVOAFileContentsCondition() : 
	m_bDirty(false),
	m_bCheckDataContents(false),
	m_bLookForHCData(false),
	m_bLookForMCData(false),
	m_bLookForLCData(false),
	m_bLookForManualData(false),
	m_bLookForClues(false),
	m_bCheckDocType(false),
	m_strDocCategory(""),
	m_strTargetFileName(gstrDEFAULT_TARGET_FILENAME),
	m_eMissingFileBehavior(UCLID_REDACTIONCUSTOMCOMPONENTSLib::kThrowError),
	m_eAttributeQuantifier(UCLID_REDACTIONCUSTOMCOMPONENTSLib::kAny),
	m_bConfigureConditionsOnly(false)
{
	m_setDocTypes.clear();
}
//--------------------------------------------------------------------------------------------------
CIDShieldVOAFileContentsCondition::~CIDShieldVOAFileContentsCondition()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI17386");
}
//--------------------------------------------------------------------------------------------------
HRESULT CIDShieldVOAFileContentsCondition::FinalConstruct()
{
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
void CIDShieldVOAFileContentsCondition::FinalRelease()
{
}

//--------------------------------------------------------------------------------------------------
// IIDShieldVOAFileContentsCondition
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldVOAFileContentsCondition::get_CheckDataContents(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI17549", pVal != NULL);

		*pVal = asVariantBool(m_bCheckDataContents);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17428");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldVOAFileContentsCondition::put_CheckDataContents(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		m_bCheckDataContents = asCppBool(newVal);

		setDirty(true);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17429");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldVOAFileContentsCondition::get_LookForHCData(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI17550", pVal != NULL);

		*pVal = asVariantBool(m_bLookForHCData);
		
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17426");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldVOAFileContentsCondition::put_LookForHCData(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		m_bLookForHCData = asCppBool(newVal);

		setDirty(true);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17427");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldVOAFileContentsCondition::get_LookForMCData(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI17594", pVal != NULL);

		*pVal = asVariantBool(m_bLookForMCData);
		
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17595");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldVOAFileContentsCondition::put_LookForMCData(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		m_bLookForMCData = asCppBool(newVal);

		setDirty(true);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17596");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldVOAFileContentsCondition::get_LookForLCData(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI17597", pVal != NULL);

		*pVal = asVariantBool(m_bLookForLCData);
		
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17598");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldVOAFileContentsCondition::put_LookForLCData(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		m_bLookForLCData = asCppBool(newVal);

		setDirty(true);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17599");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldVOAFileContentsCondition::get_LookForManualData(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI17600", pVal != NULL);

		*pVal = asVariantBool(m_bLookForManualData);
		
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17601");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldVOAFileContentsCondition::put_LookForManualData(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		m_bLookForManualData = asCppBool(newVal);

		setDirty(true);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17602");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldVOAFileContentsCondition::get_LookForClues(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI17603", pVal != NULL);

		*pVal = asVariantBool(m_bLookForClues);
		
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17604");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldVOAFileContentsCondition::put_LookForClues(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		m_bLookForClues = asCppBool(newVal);

		setDirty(true);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17605");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldVOAFileContentsCondition::get_CheckDocType(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		ASSERT_ARGUMENT("ELI17547", pVal != NULL);

		*pVal = asVariantBool(m_bCheckDocType);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17453");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldVOAFileContentsCondition::put_CheckDocType(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		m_bCheckDocType = asCppBool(newVal);

		setDirty(true);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17452");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldVOAFileContentsCondition::get_DocCategory(BSTR* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		ASSERT_ARGUMENT("ELI17551", pVal != NULL);

		*pVal = _bstr_t(m_strDocCategory.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17451");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldVOAFileContentsCondition::put_DocCategory(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		m_strDocCategory = asString(newVal);

		setDirty(true);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17450");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldVOAFileContentsCondition::get_DocTypes(IVariantVector** pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		ASSERT_ARGUMENT("ELI17552", pVal != NULL);
		
		IVariantVectorPtr ipDocTypes(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI17448", ipDocTypes != NULL);

		for (set<string>::iterator i = m_setDocTypes.begin(); i != m_setDocTypes.end(); i++)
		{
			ipDocTypes->PushBack((*i).c_str());
		}
		*pVal = ipDocTypes.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17449");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldVOAFileContentsCondition::put_DocTypes(IVariantVector* newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();
		
		m_setDocTypes.clear();
		if (newVal != NULL)
		{
			IVariantVectorPtr ipDocTypes(newVal);
			ASSERT_RESOURCE_ALLOCATION("ELI17446", ipDocTypes != NULL);

			unsigned long nSize = ipDocTypes->Size;
			for (unsigned long ul = 0; ul < nSize; ul++)
			{
				string strValue = asString(_bstr_t(ipDocTypes->GetItem(ul)));
				m_setDocTypes.insert(strValue);
			}
		}

		setDirty(true);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17447");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldVOAFileContentsCondition::get_TargetFileName(BSTR* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		ASSERT_ARGUMENT("ELI17553", pVal != NULL);

		*pVal = _bstr_t(m_strTargetFileName.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17520");
	
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldVOAFileContentsCondition::put_TargetFileName(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_strTargetFileName = asString(newVal);

		setDirty(true);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17521");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldVOAFileContentsCondition::get_MissingFileBehavior(EMissingVOAFileBehavior* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI17624", pVal != NULL);

		*pVal = (EMissingVOAFileBehavior)m_eMissingFileBehavior;
		
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17625");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldVOAFileContentsCondition::put_MissingFileBehavior(EMissingVOAFileBehavior newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		m_eMissingFileBehavior = (UCLID_REDACTIONCUSTOMCOMPONENTSLib::EMissingVOAFileBehavior) newVal;

		setDirty(true);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17626");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldVOAFileContentsCondition::get_AttributeQuantifier(EAttributeQuantifier* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI25130", pVal != NULL);

		*pVal = (EAttributeQuantifier)m_eAttributeQuantifier;
		
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25131");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldVOAFileContentsCondition::put_AttributeQuantifier(EAttributeQuantifier newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		m_eAttributeQuantifier = (UCLID_REDACTIONCUSTOMCOMPONENTSLib::EAttributeQuantifier) newVal;

		setDirty(true);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25132");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldVOAFileContentsCondition::get_ConfigureConditionsOnly(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		ASSERT_ARGUMENT("ELI28670", pVal != NULL);

		*pVal = asVariantBool(m_bConfigureConditionsOnly);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28671");	
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldVOAFileContentsCondition::put_ConfigureConditionsOnly(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		m_bConfigureConditionsOnly = asCppBool(newVal);

		setDirty(true);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28672");
}

//--------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldVOAFileContentsCondition::InterfaceSupportsErrorInfo(REFIID riid)
{
	try
	{
		static const IID* arr[] = 
		{
			&IID_IIDShieldVOAFileContentsCondition,
			&IID_ILicensedComponent,
			&IID_ICategorizedComponent,
			&IID_ICopyableObject,
			&IID_IPersistStream,
			&IID_IMustBeConfiguredObject,
			&IID_IFAMCondition
		};

		for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
		{
			if (InlineIsEqualGUID(*arr[i],riid))
				return S_OK;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17501")

	return S_FALSE;
}

//--------------------------------------------------------------------------------------------------
// ILicensedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldVOAFileContentsCondition::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		if (pbValue == NULL)
		{
			return E_POINTER;
		}

		// validate license
		validateLicense();

		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldVOAFileContentsCondition::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19628", pstrComponentDescription != NULL)

		*pstrComponentDescription = _bstr_t("ID Shield data file contents condition").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17387")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldVOAFileContentsCondition::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		UCLID_REDACTIONCUSTOMCOMPONENTSLib::IIDShieldVOAFileContentsConditionPtr ipCopyThis(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI17390", ipCopyThis != NULL);
		
		// copy member variables
		m_bCheckDataContents		= asCppBool(ipCopyThis->CheckDataContents);
		m_bLookForHCData			= asCppBool(ipCopyThis->LookForHCData);
		m_bLookForMCData			= asCppBool(ipCopyThis->LookForMCData);
		m_bLookForLCData			= asCppBool(ipCopyThis->LookForLCData);
		m_bLookForManualData		= asCppBool(ipCopyThis->LookForManualData);
		m_bLookForClues				= asCppBool(ipCopyThis->LookForClues);
		m_bCheckDocType				= asCppBool(ipCopyThis->CheckDocType);
		m_strDocCategory			= asString(ipCopyThis->DocCategory);
		m_strTargetFileName			= asString(ipCopyThis->TargetFileName);
		m_eMissingFileBehavior		= ipCopyThis->MissingFileBehavior;
		m_eAttributeQuantifier      = ipCopyThis->AttributeQuantifier;
		m_bConfigureConditionsOnly  = asCppBool(ipCopyThis->ConfigureConditionsOnly);

		// load m_setDocTypes
		getThisAsCOMPtr()->DocTypes = ipCopyThis->DocTypes;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17391");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldVOAFileContentsCondition::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance(CLSID_IDShieldVOAFileContentsCondition);
		ASSERT_RESOURCE_ALLOCATION("ELI17388", ipObjCopy != NULL);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17389");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldVOAFileContentsCondition::raw_IsConfigured(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		bool bConfigured = false;

		// Check that at least one data type or doc type is selected
		if (m_bCheckDataContents == true)
		{
			if (m_bLookForHCData == true ||
				m_bLookForMCData == true ||
				m_bLookForLCData == true ||
				m_bLookForManualData == true ||
				m_bLookForClues	== true)
			{
			bConfigured = true;
			}
		}
		else if (m_bCheckDocType == true && m_setDocTypes.size() > 0)
		{
			bConfigured = true;
		}
		
		*pbValue = asVariantBool(bConfigured);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17392");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldVOAFileContentsCondition::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pClassID = CLSID_IDShieldVOAFileContentsCondition;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldVOAFileContentsCondition::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldVOAFileContentsCondition::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// reset member variables
		m_bCheckDataContents = false;
		m_bLookForHCData = false;
		m_bLookForMCData = false;
		m_bLookForLCData = false;
		m_bLookForManualData = false;
		m_bLookForClues	= false;
		m_bCheckDocType = false;
		m_strDocCategory = "";
		m_setDocTypes.clear();
		m_strTargetFileName = gstrDEFAULT_TARGET_FILENAME;
		m_eMissingFileBehavior = UCLID_REDACTIONCUSTOMCOMPONENTSLib::kThrowError;
		m_eAttributeQuantifier = UCLID_REDACTIONCUSTOMCOMPONENTSLib::kAny;
		m_bConfigureConditionsOnly = false;
		m_apTester.reset();

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
			UCLIDException ue("ELI17393", "Unable to load newer file existence FAM condition component.");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		// Read member variables
		dataReader >> m_bCheckDataContents;
		dataReader >> m_bLookForHCData;
		dataReader >> m_bLookForMCData;
		dataReader >> m_bLookForLCData;
		dataReader >> m_bLookForManualData;
		dataReader >> m_bLookForClues;
		dataReader >> m_bCheckDocType;
		dataReader >> m_strDocCategory;
		dataReader >> m_strTargetFileName;

		// If nDataVersion >= 2, read m_eMissingFileBehavior setting
		if (nDataVersion >= 2)
		{
			long lMissingFileBehavior;
			dataReader >> lMissingFileBehavior;
			m_eMissingFileBehavior = 
				(UCLID_REDACTIONCUSTOMCOMPONENTSLib::EMissingVOAFileBehavior) lMissingFileBehavior;
		}

		// Read attribute quantifier
		if (nDataVersion >= 3)
		{
			long lAttributeQuantifier;
			dataReader >> lAttributeQuantifier;
			m_eAttributeQuantifier = 
				(UCLID_REDACTIONCUSTOMCOMPONENTSLib::EAttributeQuantifier) lAttributeQuantifier;
		}

		// Modified as per [FlexIDSCore #3971]
		// Read the configure conditions only value
		if (nDataVersion >= 4)
		{
			dataReader >> m_bConfigureConditionsOnly;
		}

		// Read doc types
		unsigned long ulSize;
		dataReader >> ulSize;
		m_setDocTypes.clear();
		for (unsigned long ul = 0; ul < ulSize; ul++)
		{
			string strValue;
			dataReader >> strValue;
			m_setDocTypes.insert(strValue);
		}

		// Clear the dirty flag as we've loaded a fresh object
		setDirty(false);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17394");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldVOAFileContentsCondition::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		dataWriter << gnCurrentVersion;

		// Save member variables
		dataWriter << m_bCheckDataContents;
		dataWriter << m_bLookForHCData;
		dataWriter << m_bLookForMCData;
		dataWriter << m_bLookForLCData;
		dataWriter << m_bLookForManualData;
		dataWriter << m_bLookForClues;
		dataWriter << m_bCheckDocType;
		dataWriter << m_strDocCategory;
		dataWriter << m_strTargetFileName;
		dataWriter << (long)m_eMissingFileBehavior;
		dataWriter << (long)m_eAttributeQuantifier;
		dataWriter << m_bConfigureConditionsOnly;

		// Save doc types
		unsigned long ulSize = m_setDocTypes.size();
		dataWriter << ulSize;

		for (set<string>::iterator i = m_setDocTypes.begin(); i != m_setDocTypes.end(); i++)
		{
			dataWriter << *i;
		}

		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		// Clear the flag as specified
		if (fClearDirty)
		{
			setDirty(false);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17395");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldVOAFileContentsCondition::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// IFAMCondition
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldVOAFileContentsCondition::raw_FileMatchesFAMCondition(BSTR bstrFile, IFileProcessingDB *pFPDB, 
												BSTR bstrAction, IFAMTagManager *pFAMTM, VARIANT_BOOL* pRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	INIT_EXCEPTION_AND_TRACING("MLI02005");
	try
	{
		ASSERT_ARGUMENT("ELI17606", pFAMTM != NULL);
		ASSERT_ARGUMENT("ELI17607", pRetVal != NULL);

		// default to condition failed, set to TRUE once condition has been satisfied
		*pRetVal = VARIANT_FALSE;

		// Call ExpandTagsAndTFE() to convert bstrFile by expanding tags and functions
		string strSource = asString(bstrFile);
		string strTarget = CRedactionCustomComponentsUtils::ExpandTagsAndTFE(pFAMTM, m_strTargetFileName, strSource);
		_lastCodePos = "10";

		// Check for existence of strTarget file
		if (isFileOrFolderValid(strTarget) == false)
		{
			_lastCodePos = "20";
			// If voa file is missing, handle according to user specification
			if (m_eMissingFileBehavior == UCLID_REDACTIONCUSTOMCOMPONENTSLib::kConsiderSatisfied)
			{
				*pRetVal = VARIANT_TRUE;
				return S_OK;
			}
			else if (m_eMissingFileBehavior == UCLID_REDACTIONCUSTOMCOMPONENTSLib::kConsiderUnsatisfied)
			{
				*pRetVal = VARIANT_FALSE;
				return S_OK;
			}
			else
			{
				// Add info to existing exception and throw
				UCLIDException ue("ELI17638", "ID Shield Data File Contents Condition: data file is missing.");
				ue.addDebugInfo("Source Filename", strSource);
				ue.addDebugInfo("Data Filename", strTarget);
				throw ue;
			}
		}
		_lastCodePos = "30";

		// Read the file into a vector
		IIUnknownVectorPtr	ipAttributes(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI17440", ipAttributes != NULL);
		ipAttributes->LoadFrom(get_bstr_t(strTarget.c_str()), VARIANT_FALSE);
		_lastCodePos = "40";

		// Ensure the attribute tester is up to date
		updateAttributeTester();

		long lAttributeCount = ipAttributes->Size();
		_lastCodePos = "50";
		for (long l = 0; l < lAttributeCount; l++)
		{
			// Get the loop count as a string
			string strCount = asString(l);

			// Get next attribute
			IAttributePtr ipAttribute = ipAttributes->At(l);
			ASSERT_RESOURCE_ALLOCATION("ELI17441", ipAttribute != NULL);

			// Get attribute name
			string strName = asString(ipAttribute->GetName());
			_lastCodePos = "50_A_" + strCount;
			
			// Get attribute value
			ISpatialStringPtr ipValue = ipAttribute->Value;
			ASSERT_RESOURCE_ALLOCATION("ELI15606", ipValue != NULL);
			string strValue = asString(ipValue->String);
			_lastCodePos = "50_B_" + strCount;

			// Test to see if this attribute fulfills the condition
			if (m_apTester->test(strName, strValue))
			{
				// No need to test any further attributes
				break;
			}
		}

		*pRetVal = asVariantBool( m_apTester->getResult() );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17396");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
UCLID_REDACTIONCUSTOMCOMPONENTSLib::IIDShieldVOAFileContentsConditionPtr CIDShieldVOAFileContentsCondition::getThisAsCOMPtr()
{
	UCLID_REDACTIONCUSTOMCOMPONENTSLib::IIDShieldVOAFileContentsConditionPtr ipThis = this;
	ASSERT_RESOURCE_ALLOCATION("ELI17560", ipThis != NULL);
	return ipThis;
}
//-------------------------------------------------------------------------------------------------
void CIDShieldVOAFileContentsCondition::setDirty(bool bIsDirty)
{
	m_bDirty = bIsDirty;
	
	if (m_bDirty)
	{
		m_apTester.reset();
	}
}
//-------------------------------------------------------------------------------------------------
void CIDShieldVOAFileContentsCondition::updateAttributeTester()
{
	// Check if the tester is already initialized
	if (m_apTester.get() != NULL)
	{
		// Reset the tester
		m_apTester->reset();
		return;
	}

	// Create an attribute tester
	m_apTester.reset(new AttributeTester());

	try
	{
		// Check if the attribute name is being tested
		if (m_bCheckDataContents)
		{
			// Create the set of data types to use
			set<string> setDataTypes;
			if (m_bLookForHCData)
			{
				setDataTypes.insert(gstrATTRIBUTE_HCDATA);
			}
			if (m_bLookForMCData)
			{
				setDataTypes.insert(gstrATTRIBUTE_MCDATA);
			}
			if (m_bLookForLCData)
			{
				setDataTypes.insert(gstrATTRIBUTE_LCDATA);
			}
			if (m_bLookForManualData)
			{
				setDataTypes.insert(gstrATTRIBUTE_MANUAL);
			}
			if (m_bLookForClues)
			{
				setDataTypes.insert(gstrATTRIBUTE_CLUES);
			}

			// Add a data type tester
			IAttributeTester* pTester = NULL;
			switch (m_eAttributeQuantifier)
			{
			case kNone:
				pTester = new NoneDataTypeAttributeTester(setDataTypes);
				break;

			case kAny:
				pTester = new AnyDataTypeAttributeTester(setDataTypes);
				break;

			case kOneOfEach:
				pTester = new OneOfEachDataTypeAttributeTester(setDataTypes);
				break;

			case kOnlyAny:
				pTester = new OnlyAnyDataTypeAttributeTester(setDataTypes);
				break;

			default:
				UCLIDException ue("ELI25147", "Unexpected attribute quantifier.");
				ue.addDebugInfo("Attribute quantifier number", m_eAttributeQuantifier);
				throw ue;
			}

			m_apTester->addTester(pTester);
		}

		// Check if the document type is being tested
		if (m_bCheckDocType)
		{
			m_apTester->addTester(new DocTypeAttributeTester(m_setDocTypes));
		}
	}
	catch (...)
	{
		// Reset the partially constructed Attribute Tester
		m_apTester.reset();

		throw;
	}
}
//-------------------------------------------------------------------------------------------------
void CIDShieldVOAFileContentsCondition::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnIDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI17397", "ID Shield VOA File Contents Condition");
}
//-------------------------------------------------------------------------------------------------