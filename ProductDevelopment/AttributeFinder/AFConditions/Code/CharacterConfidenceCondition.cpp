// CharacterConfidenceCondition.cpp : Implementation of CCharacterConfidenceCondition

#include "stdafx.h"
#include "CharacterConfidenceCondition.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CCharacterConfidenceCondition
//-------------------------------------------------------------------------------------------------
CCharacterConfidenceCondition::CCharacterConfidenceCondition() :
	m_bDirty(false),
	m_eAggregateFunction(kAverage),
	m_eFirstScoreCondition(kEQ),
	m_lFirstScoreToCompare(0),	m_bIsSecondCondition(false),
	m_eSecondScoreCondition(kEQ),	m_lSecondScoreToCompare(0),
	m_bAndConditions(true), m_bIsMet(true)
{
}
//-------------------------------------------------------------------------------------------------
HRESULT CCharacterConfidenceCondition::FinalConstruct()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
void CCharacterConfidenceCondition::FinalRelease()
{
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceCondition::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ICharacterConfidenceCondition,
		&IID_IAFCondition,
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
	return S_FALSE;
}
//-------------------------------------------------------------------------------------------------
// IAFCondition
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceCondition::raw_ProcessCondition(IAFDocument *pAFDoc, VARIANT_BOOL* pbRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		IAFDocumentPtr ipDoc(pAFDoc);

		// Assert parameters and resources
		ASSERT_ARGUMENT("ELI29352", ipDoc != __nullptr);
		ASSERT_ARGUMENT("ELI29353", pbRetVal != __nullptr);
		
		long nConfidence;
		ISpatialStringPtr ipValue = ipDoc->Text;
		ASSERT_RESOURCE_ALLOCATION("ELI29451", ipValue != __nullptr);

		// Get the Confidence
		switch (m_eAggregateFunction)
		{
		case kAverage:
			ipValue->GetCharConfidence(__nullptr, __nullptr, &nConfidence);
			break;
		case kMinimum:
			ipValue->GetCharConfidence(&nConfidence, __nullptr, __nullptr);
			break;
		case kMaximum:
			ipValue->GetCharConfidence(__nullptr, &nConfidence, __nullptr);
			break;
		default:
			THROW_LOGIC_ERROR_EXCEPTION("ELI29439");
		}

		// Evaluate score for first condition
		bool bSelect = evaluateCondition(m_eFirstScoreCondition, nConfidence, m_lFirstScoreToCompare);

		// Check if there is a second condition
		if (m_bIsSecondCondition)
		{
			// Evaluate the second condition
			bool bSecondSelect = 
				evaluateCondition(m_eSecondScoreCondition, nConfidence, m_lSecondScoreToCompare);

			// Combine with the first condition by ANDing or ORing
			if (m_bAndConditions)
			{
				bSelect = bSelect && bSecondSelect;
			}
			else
			{
				bSelect = bSelect || bSecondSelect;
			}
		}
		
		*pbRetVal = asVariantBool(m_bIsMet == bSelect);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29355")
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceCondition::raw_IsConfigured(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		// Check parameter
		ASSERT_ARGUMENT("ELI29356", pbValue != __nullptr);

		// Always configured
		*pbValue = VARIANT_TRUE;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29357");
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceCondition::raw_GetComponentDescription(BSTR * pbstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI29358", pbstrComponentDescription != __nullptr)

		*pbstrComponentDescription = _bstr_t("Character confidence condition").Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29359")
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceCondition::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license
		validateLicense();

		UCLID_AFCONDITIONSLib::ICharacterConfidenceConditionPtr ipCopyThis = pObject;
		ASSERT_ARGUMENT("ELI29360", ipCopyThis != __nullptr);
		
		// Copy the properties
		m_eFirstScoreCondition = ipCopyThis->FirstScoreCondition;
		m_lFirstScoreToCompare = ipCopyThis->FirstScoreToCompare;
		m_bIsSecondCondition = asCppBool(ipCopyThis->IsSecondCondition);
		m_eSecondScoreCondition = ipCopyThis->SecondScoreCondition;
		m_lSecondScoreToCompare = ipCopyThis->SecondScoreToCompare;
		m_bAndConditions = asCppBool(ipCopyThis->AndSecondCondition);

		m_eAggregateFunction = (EAggregateFunctions)ipCopyThis->AggregateFunction;
		m_bIsMet = asCppBool(ipCopyThis->IsMet);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29361");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceCondition::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license
		validateLicense();

		ASSERT_ARGUMENT("ELI29362", pObject != __nullptr);

		// Create another instance of this object
		ICopyableObjectPtr ipObjCopy(CLSID_CharacterConfidenceCondition);
		ASSERT_RESOURCE_ALLOCATION("ELI29363", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);
	
		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29364");
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceCondition::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI29365", pClassID != __nullptr);

		*pClassID = CLSID_CharacterConfidenceCondition;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29366");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceCondition::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		return m_bDirty ? S_OK : S_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29367");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceCondition::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		ASSERT_ARGUMENT("ELI29368", pStream != __nullptr);
		
		// Set the properties to default values
		m_eFirstScoreCondition = kEQ;
		m_lFirstScoreToCompare = 0;
		m_bIsSecondCondition = false;
		m_eSecondScoreCondition = kEQ;
		m_lSecondScoreToCompare = 0;
		m_bAndConditions = true;
		m_eAggregateFunction = kAverage;
		m_bIsMet = true;

		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		pStream->Read(&nDataLength, sizeof(nDataLength), __nullptr);
		ByteStream data(nDataLength);
		pStream->Read(data.getData(), nDataLength, __nullptr);
		ByteStreamManipulator dataReader(ByteStreamManipulator::kRead, data);

		// Read the individual data items from the bytestream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;

		// Load the data
		long nTmp;
		dataReader >> nTmp;
		m_eFirstScoreCondition = (EConditionalOp)nTmp;
		dataReader >> m_lFirstScoreToCompare;
		dataReader >> m_bIsSecondCondition;
		dataReader >> nTmp;
		m_eSecondScoreCondition = (EConditionalOp)nTmp;
		dataReader >> m_lSecondScoreToCompare;
		dataReader >> m_bAndConditions;
		dataReader >> nTmp;
		m_eAggregateFunction = (EAggregateFunctions)nTmp;
		dataReader >> m_bIsMet;

		// Check for newer version
		if (nDataVersion > gnCurrentVersion)
		{
			// Throw exception
			UCLIDException ue("ELI29369", "Unable to load newer character confidence condition!");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}


		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29370");

}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceCondition::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		ASSERT_ARGUMENT("ELI29371", pStream != __nullptr);

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		dataWriter << gnCurrentVersion;

		// Save the data to the stream
		dataWriter << (long)m_eFirstScoreCondition;
		dataWriter << m_lFirstScoreToCompare;
		dataWriter << m_bIsSecondCondition;
		dataWriter << (long)m_eSecondScoreCondition;
		dataWriter << m_lSecondScoreToCompare;
		dataWriter << m_bAndConditions;
		dataWriter << (long)m_eAggregateFunction;
		dataWriter << m_bIsMet;

		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), __nullptr);
		pStream->Write(data.getData(), nDataLength, __nullptr);


		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29372");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceCondition::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceCondition::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI29373", pbValue != __nullptr);

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
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29374");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICharacterConfidenceCondition
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceCondition::get_AggregateFunction(EAggregateFunctions *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// validate License
		validateLicense();
		
		ASSERT_ARGUMENT("ELI29411", pVal != __nullptr);

		*pVal = m_eAggregateFunction;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29412");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceCondition::put_AggregateFunction(EAggregateFunctions newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// validate License
		validateLicense();

		if (m_eAggregateFunction != newVal)
		{
			m_eAggregateFunction = newVal;
			m_bDirty = true;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29434");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceCondition::get_FirstScoreCondition(EConditionalOp* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		// Make sure the argument is valid
		ASSERT_ARGUMENT("ELI29413", pVal != __nullptr);

		*pVal = m_eFirstScoreCondition;

		return S_OK;
	}			
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29414");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceCondition::put_FirstScoreCondition(EConditionalOp newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		// Set dirty flag if the new value is different from old value or already dirty
		m_bDirty = m_bDirty || (m_eFirstScoreCondition != newVal);

		m_eFirstScoreCondition = newVal;

		return S_OK;
	}			
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29415");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceCondition::get_FirstScoreToCompare(long* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		// Make sure the argument is valid
		ASSERT_ARGUMENT("ELI29416", pVal != __nullptr);

		*pVal = m_lFirstScoreToCompare;

		return S_OK;
	}			
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29417");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceCondition::put_FirstScoreToCompare(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();
		
		// Set dirty flag if the new value is different from old value or already dirty
		m_bDirty = m_bDirty || (m_lFirstScoreToCompare != newVal);

		m_lFirstScoreToCompare = newVal;

		return S_OK;
	}			
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29418");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceCondition::get_IsSecondCondition(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		// Make sure the argument is valid
		ASSERT_ARGUMENT("ELI29419", pVal != __nullptr);

		*pVal = asVariantBool(m_bIsSecondCondition);

		return S_OK;
	}			
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29420");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceCondition::put_IsSecondCondition( VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();
		
		// Set dirty flag if the new value is different from old value or already dirty
		m_bDirty = m_bDirty || (m_bIsSecondCondition != asCppBool(newVal));

		m_bIsSecondCondition = asCppBool(newVal);

		return S_OK;
	}			
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29421");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceCondition::get_SecondScoreCondition(EConditionalOp* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		// Make sure the argument is valid
		ASSERT_ARGUMENT("ELI29422", pVal != __nullptr);

		*pVal = m_eSecondScoreCondition;

		return S_OK;
	}			
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29423");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceCondition::put_SecondScoreCondition(EConditionalOp newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		// Set dirty flag if the new value is different from old value or already dirty
		m_bDirty = m_bDirty || (m_eSecondScoreCondition != newVal);

		m_eSecondScoreCondition = newVal;

		return S_OK;
	}			
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29424");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceCondition::get_SecondScoreToCompare(long* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();
		
		// Make sure the argument is valid
		ASSERT_ARGUMENT("ELI29425", pVal != __nullptr);

		*pVal = m_lSecondScoreToCompare;

		return S_OK;
	}			
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29426");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceCondition::put_SecondScoreToCompare(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		// Set dirty flag if the new value is different from old value or already dirty
		m_bDirty = m_bDirty || (m_lSecondScoreToCompare != newVal);

		m_lSecondScoreToCompare = newVal;

		return S_OK;
	}			
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29427");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceCondition::get_AndSecondCondition(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		// Make sure the argument is valid
		ASSERT_ARGUMENT("ELI29428", pVal != __nullptr);

		*pVal = asVariantBool(m_bAndConditions);

		return S_OK;
	}			
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29429");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceCondition::put_AndSecondCondition(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		// Set dirty flag if the new value is different from old value or already dirty
		m_bDirty = m_bDirty || (m_bAndConditions != asCppBool(newVal));

		m_bAndConditions = asCppBool(newVal);

		return S_OK;
	}			
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29430");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceCondition::get_IsMet(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		// Make sure the argument is valid
		ASSERT_ARGUMENT("ELI29431", pVal != __nullptr);

		*pVal = asVariantBool(m_bIsMet);

		return S_OK;
	}			
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29432");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceCondition::put_IsMet( VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();
		
		// Set dirty flag if the new value is different from old value or already dirty
		m_bDirty = m_bDirty || (m_bIsMet != asCppBool(newVal));

		m_bIsMet = asCppBool(newVal);

		return S_OK;
	}			
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29433");
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void CCharacterConfidenceCondition::validateLicense()
{
	VALIDATE_LICENSE(gnFLEXINDEX_IDSHIELD_CORE_OBJECTS, "ELI29375", "Character Confidence Condition");
}
//-------------------------------------------------------------------------------------------------
bool CCharacterConfidenceCondition::evaluateCondition(const EConditionalOp eOp, 
													  const long lCalculatedConfidence, 
													  const long lConditionConfidence)
{
	// Determine the comparison to make
	switch (eOp)
	{
	case kEQ:
		return lCalculatedConfidence == lConditionConfidence;
	case kNEQ:
		return lCalculatedConfidence != lConditionConfidence;
	case kLT:
		return lCalculatedConfidence < lConditionConfidence;
	case kGT:
		return lCalculatedConfidence > lConditionConfidence;
	case kLEQ:
		return lCalculatedConfidence <= lConditionConfidence;
	case kGEQ:
		return lCalculatedConfidence >= lConditionConfidence;
	default:
		THROW_LOGIC_ERROR_EXCEPTION("ELI29440");
	}
}
//-------------------------------------------------------------------------------------------------
