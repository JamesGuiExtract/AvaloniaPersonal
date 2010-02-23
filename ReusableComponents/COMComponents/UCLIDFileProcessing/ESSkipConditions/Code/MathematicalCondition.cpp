// MathematicalCondition.cpp : Implementation of CMathematicalCondition

#include "stdafx.h"
#include "MathematicalCondition.h"
#include "ESSkipConditions.h"

#include <ComUtils.h>
#include <ComponentLicenseIDs.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <UCLIDException.h>

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
// current version
const unsigned long gnCurrentVersion = 1;

//--------------------------------------------------------------------------------------------------
// CMathematicalCondition
//--------------------------------------------------------------------------------------------------
CMathematicalCondition::CMathematicalCondition()
:m_bDirty(false),
m_bConsiderConditionMet(true),
m_ipConditionChecker(NULL)
{
}
//--------------------------------------------------------------------------------------------------
CMathematicalCondition::~CMathematicalCondition()
{
	try
	{
		// Ensure the condition checker is set to NULL
		if (m_ipConditionChecker != NULL)
		{
			m_ipConditionChecker = NULL;
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI27138");
}
//--------------------------------------------------------------------------------------------------
void CMathematicalCondition::FinalRelease()
{
	try
	{
		// Ensure condition checker is set to NULL
		m_ipConditionChecker = NULL;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI27139");
}

//--------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMathematicalCondition::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ILicensedComponent,
		&IID_IMathematicalFAMCondition,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_IClipboardCopyable,
		&IID_IPersistStream,
		&IID_IMustBeConfiguredObject,
		&IID_IFAMCondition
	};

	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//--------------------------------------------------------------------------------------------------
// IMathematicalConditionFAMCondition
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMathematicalCondition::get_ConsiderMet(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI27140", pVal != NULL);

		*pVal = asVariantBool(m_bConsiderConditionMet);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27141");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMathematicalCondition::put_ConsiderMet(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		m_bConsiderConditionMet = asCppBool(newVal);
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27142");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMathematicalCondition::get_MathematicalCondition(IMathConditionChecker** ppCondition)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI27143", ppCondition != NULL);

		IMathConditionCheckerPtr ipShallowCopy = m_ipConditionChecker;

		*ppCondition = (IMathConditionChecker*)ipShallowCopy.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27144");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMathematicalCondition::put_MathematicalCondition(IMathConditionChecker* pCondition)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		// Copy the new value into the object (NULL is acceptable)
		m_ipConditionChecker = pCondition;
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27145");
}

//--------------------------------------------------------------------------------------------------
// ILicensedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMathematicalCondition::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI27146", pbValue != NULL);

		try
		{
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27147");
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMathematicalCondition::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI27148", pstrComponentDescription != NULL)

		*pstrComponentDescription = _bstr_t("Match based upon mathematical condition").Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27149")
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMathematicalCondition::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		EXTRACT_FAMCONDITIONSLib::IMathematicalFAMConditionPtr ipCopyThis(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI27150", ipCopyThis != NULL);
		
		// Copy the values from another object
		m_bConsiderConditionMet = asCppBool(ipCopyThis->ConsiderMet);

		// Get the math condition object as a copyable object
		ICopyableObjectPtr ipConditionCopier = ipCopyThis->MathematicalCondition;

		// Clone the math condition object
		m_ipConditionChecker = ipConditionCopier != NULL ? ipConditionCopier->Clone() : NULL;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27151");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMathematicalCondition::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		ASSERT_ARGUMENT("ELI27152", pObject != NULL);

		ICopyableObjectPtr ipCopy(CLSID_MathematicalCondition);
		ASSERT_RESOURCE_ALLOCATION("ELI27153", ipCopy != NULL);

		IUnknownPtr ipUnk = this;
		ipCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipCopy.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27154");
}

//-------------------------------------------------------------------------------------------------
// IClipboardCopyable
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMathematicalCondition::raw_NotifyCopiedFromClipboard()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate the license
		validateLicense();

		// Get the math condition as a clipboard copyable object
		IClipboardCopyablePtr ipClip = m_ipConditionChecker;
		if (ipClip != NULL)
		{
			// If the condition object implements the interface, call the notify method
			ipClip->NotifyCopiedFromClipboard();
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27255");
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMathematicalCondition::raw_IsConfigured(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI27155", pbValue != NULL);

		*pbValue = isConfigured();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27156");
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMathematicalCondition::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	*pClassID = CLSID_MathematicalCondition;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMathematicalCondition::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMathematicalCondition::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// reset member variables
		m_bConsiderConditionMet = true;
		m_ipConditionChecker = NULL;

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
			UCLIDException ue("ELI27157", "Unable to load newer mathematical FAM condition component.");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		// Load the variables 
		dataReader >> m_bConsiderConditionMet;

		// Read the math condition from the stream
		IPersistStreamPtr ipMathCondition;
		readObjectFromStream(ipMathCondition, pStream, "ELI27158");
		m_ipConditionChecker = ipMathCondition;

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27159");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMathematicalCondition::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		// save the variables
		dataWriter << gnCurrentVersion;
		dataWriter << m_bConsiderConditionMet;
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		// Write the condition checker object to the stream
		IPersistStreamPtr ipConditionStream = m_ipConditionChecker;
		writeObjectToStream(ipConditionStream, pStream, "ELI27160", fClearDirty);

		// Clear the flag as specified
		if (fClearDirty == TRUE)
		{
			m_bDirty = false;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27161");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMathematicalCondition::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// IFAMCondition
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMathematicalCondition::raw_FileMatchesFAMCondition(BSTR bstrFile, 
	IFileProcessingDB* pFPDB, long lFileID, long lActionID, IFAMTagManager* pFAMTM, 
	VARIANT_BOOL* pRetVal)
{	
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// Check the arguments
		ASSERT_ARGUMENT("ELI27163", pRetVal != NULL);

		// Ensure the object has been configured
		if (isConfigured() == VARIANT_FALSE)
		{
			UCLIDException ue("ELI27164", "The condition object has not been configured!");
			throw ue;
		}

		// Check the condition
		bool bConditionMet = 
			asCppBool(m_ipConditionChecker->CheckCondition(bstrFile, lFileID, lActionID));

		// Reverse result if considering the condition not met
		if (!m_bConsiderConditionMet)
		{
			bConditionMet = !bConditionMet;
		}

		// Return the result
		*pRetVal = asVariantBool(bConditionMet);
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27165");
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CMathematicalCondition::validateLicense()
{
	VALIDATE_LICENSE(gnFLEXINDEX_IDSHIELD_CORE_OBJECTS, "ELI27166", "Mathematical FAM Condition");
}
//-------------------------------------------------------------------------------------------------
VARIANT_BOOL CMathematicalCondition::isConfigured()
{
	try
	{
		// Get the condition as a must be configured object
		IMustBeConfiguredObjectPtr ipConfigure = m_ipConditionChecker;
		
		// Return the result of the IsConfigured if ipConfigure is not null, otherwise return false
		return (ipConfigure != NULL ? ipConfigure->IsConfigured() : VARIANT_FALSE);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27167");
}
//-------------------------------------------------------------------------------------------------