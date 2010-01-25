// DataScorerBasedAS.cpp : Implementation of CDataScorerBasedAS

#include "stdafx.h"
#include "DataScorerBasedAS.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <COMUtils.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CDataScorerBasedAS
//-------------------------------------------------------------------------------------------------
CDataScorerBasedAS::CDataScorerBasedAS():
	m_ipDataScorer(NULL), m_eFirstScoreCondition(kEQ),
	m_lFirstScoreToCompare(0),	m_bIsSecondCondition(false),
	m_eSecondScoreCondition(kEQ),	m_lSecondScoreToCompare(0),
	m_bAndConditions(true)
{
}
CDataScorerBasedAS::~CDataScorerBasedAS()
{
	try
	{
		m_ipDataScorer = NULL;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI29332");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataScorerBasedAS::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IDataScorerBasedAS,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_IPersistStream,
		&IID_ILicensedComponent,
		&IID_IAttributeSelector
	};

	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataScorerBasedAS::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	*pClassID = CLSID_DataScorerBasedAS;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataScorerBasedAS::IsDirty()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataScorerBasedAS::Load(IStream * pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// validate license
		validateLicense();
		
		// Make sure there is a vaild argument
		ASSERT_ARGUMENT("ELI29286", pStream != NULL);

		// Set the properties to default values
		m_eFirstScoreCondition = kEQ;
		m_lFirstScoreToCompare = 0;
		m_bIsSecondCondition = false;
		m_eSecondScoreCondition = kEQ;
		m_lSecondScoreToCompare = 0;
		m_bAndConditions = true;

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
			UCLIDException ue( "ELI29246", 
				"Unable to load newer Data Scorer Based Attribute Selector" );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

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
		
		// read the data scorer object
		IPersistStreamPtr ipObj;
		::readObjectFromStream(ipObj, pStream, "ELI29279");
		ASSERT_RESOURCE_ALLOCATION("ELI29280", ipObj != NULL);
		m_ipDataScorer = ipObj;

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29247");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataScorerBasedAS::Save(IStream * pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		// validate license
		validateLicense();
		
		// Make sure the argument is valid
		ASSERT_ARGUMENT("ELI29285", pStream != NULL);

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter( ByteStreamManipulator::kWrite, data );
		dataWriter << gnCurrentVersion;

		// Save the data to the stream
		dataWriter << (long)m_eFirstScoreCondition;
		dataWriter << m_lFirstScoreToCompare;
		dataWriter << m_bIsSecondCondition;
		dataWriter << (long)m_eSecondScoreCondition;
		dataWriter << m_lSecondScoreToCompare;
		dataWriter << m_bAndConditions;

		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write( &nDataLength, sizeof(nDataLength), NULL );
		pStream->Write( data.getData(), nDataLength, NULL );

		// If the datascorer is not set need to create one
		if (m_ipDataScorer == NULL)
		{
			m_ipDataScorer.CreateInstance(CLSID_ObjectWithDescription);
			ASSERT_RESOURCE_ALLOCATION("ELI29284", m_ipDataScorer != NULL);
		}

		// write the data-scorer object to the stream
		IPersistStreamPtr ipObj = m_ipDataScorer;
		if (ipObj == NULL)
		{
			throw UCLIDException("ELI29282", "Data Scorer object does not support persistence.");
		}
		writeObjectToStream(ipObj, pStream, "ELI29283", fClearDirty);
		
		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29248");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataScorerBasedAS::GetSizeMax(_ULARGE_INTEGER * pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// IAttributeSelector Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataScorerBasedAS::raw_SelectAttributes(IIUnknownVector * pAttrIn, IAFDocument * pAFDoc, IIUnknownVector * * pAttrOut)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		// validate license
		validateLicense();

		// Put input vector into smart pointer and validate the arguments that are used.
		IIUnknownVectorPtr ipIn( pAttrIn);
		ASSERT_ARGUMENT("ELI29308", ipIn != NULL);
		ASSERT_ARGUMENT("ELI29309", pAttrOut != NULL);
		ASSERT_RESOURCE_ALLOCATION("ELI29333", m_ipDataScorer != NULL);

		// Create a vector for the found data
		IIUnknownVectorPtr ipFound(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI29307", ipFound != NULL);

		// Create the Data score object to be used to get the score.
		IDataScorerPtr ipDataScorer = m_ipDataScorer->Object;
		ASSERT_RESOURCE_ALLOCATION("ELI29303", ipDataScorer != NULL);

		// Step through the attributes
		long lSize = ipIn->Size();
		for ( long i=0; i < lSize; i++)
		{
			// Assign the current attribute to ICopyableObject so that
			// a clone can be made to use for the scoring 
			ICopyableObjectPtr ipCurrentAttribute = ipIn->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI29304", ipCurrentAttribute != NULL);

			// Need to clone the attribute since data scorer can modify
			IAttributePtr ipAttributeToScore = ipCurrentAttribute->Clone();
			ASSERT_RESOURCE_ALLOCATION("ELI29306", ipAttributeToScore != NULL);

			// Get the score for the attribute
			long nScore = ipDataScorer->GetDataScore1(ipAttributeToScore);

			// Evaluate score for first condition
			bool bSelect = evaluateCondition(m_eFirstScoreCondition, nScore, m_lFirstScoreToCompare);

			// Check if there is a second condition
			if (m_bIsSecondCondition)
			{
				// Evaluate the second condition
				bool bSecondSelect = 
					evaluateCondition(m_eSecondScoreCondition, nScore, m_lSecondScoreToCompare);
				
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

			// If the result of the evaluation of the conditions is true the put attribute on the output
			if (bSelect)
			{
				// Put the Current Attribute on the found list - this must be the same attribute that
				// was in the input not the clone
				ipFound->PushBack(ipCurrentAttribute);
			}
		}
		
		// Detach the found vector from the smart pointer preserving the count for return
		*pAttrOut = ipFound.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29249");
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataScorerBasedAS::raw_IsConfigured(VARIANT_BOOL * pbConfigured)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate the license
		validateLicense();

		// Make sure the argument is valid
		ASSERT_ARGUMENT("ELI29287", pbConfigured != NULL); 

		// Set output to default value
		*pbConfigured = VARIANT_TRUE;

		// Test that the properties are set to valid values
		if (m_ipDataScorer == NULL || m_ipDataScorer->Object == NULL 
			|| m_lFirstScoreToCompare < 0 || m_lFirstScoreToCompare > 100 
			|| (m_bIsSecondCondition && (m_lSecondScoreToCompare < 0 
			|| m_lSecondScoreToCompare > 100)))
		{
			*pbConfigured = VARIANT_FALSE;
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29250");
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataScorerBasedAS::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());	
	
	try
	{
		// Make sure the arguement is valid
		ASSERT_ARGUMENT("ELI29251", pstrComponentDescription != NULL)

		*pstrComponentDescription = _bstr_t("Data scorer based attribute selector").Detach();
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29252");
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataScorerBasedAS::raw_Clone(IUnknown * * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		// validate license
		validateLicense();

		// create a new instance of the DataScorerBasedAS
		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance(CLSID_DataScorerBasedAS);
		ASSERT_RESOURCE_ALLOCATION("ELI29253", ipObjCopy != NULL);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29254");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataScorerBasedAS::raw_CopyFrom(IUnknown * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		// validate license
		validateLicense();

		UCLID_AFSELECTORSLib::IDataScorerBasedASPtr ipFrom(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI29255", ipFrom != NULL );
		
		// Clone the DataScorer object
		ICopyableObjectPtr ipObjCopy = ipFrom->DataScorer;
		ASSERT_RESOURCE_ALLOCATION("ELI29288", ipObjCopy != NULL);
		m_ipDataScorer = ipObjCopy->Clone();

		// Copy the properties
		m_eFirstScoreCondition = ipFrom->FirstScoreCondition;
		m_lFirstScoreToCompare = ipFrom->FirstScoreToCompare;
		m_bIsSecondCondition = asCppBool(ipFrom->IsSecondCondition);
		m_eSecondScoreCondition = ipFrom->SecondScoreCondition;
		m_lSecondScoreToCompare = ipFrom->SecondScoreToCompare;
		m_bAndConditions = asCppBool(ipFrom->AndSecondCondition);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29256");
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataScorerBasedAS::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

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
// IDataScorerBasedAS Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataScorerBasedAS::get_DataScorer(IObjectWithDescription** ppVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		// Make sure the argument is valid
		ASSERT_ARGUMENT("ELI29289", ppVal != NULL);
		
		// if the DataScorer is NULL create a new ObjectWithDescription
		if (m_ipDataScorer == NULL)
		{
			m_ipDataScorer.CreateInstance(CLSID_ObjectWithDescription);
			ASSERT_RESOURCE_ALLOCATION("ELI29300", m_ipDataScorer != NULL);
		}

		// Return the DataScorer ObjectWithDiscription
		IObjectWithDescriptionPtr ipDataScorer(m_ipDataScorer);
		*ppVal = ipDataScorer.Detach();

		return S_OK;
	}			
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29265");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataScorerBasedAS::put_DataScorer(IObjectWithDescription* newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		// Check if this is different
		if (m_ipDataScorer != newVal)
		{
			m_ipDataScorer = newVal;

			// Set the dirty flag
			m_bDirty = true;
		}

		return S_OK;
	}			
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29266");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataScorerBasedAS::get_FirstScoreCondition(EConditionalOp* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		// Make sure the argument is valid
		ASSERT_ARGUMENT("ELI29291", pVal != NULL);

		*pVal = m_eFirstScoreCondition;

		return S_OK;
	}			
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29267");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataScorerBasedAS::put_FirstScoreCondition(EConditionalOp newVal)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29268");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataScorerBasedAS::get_FirstScoreToCompare(long* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		// Make sure the argument is valid
		ASSERT_ARGUMENT("ELI29292", pVal != NULL);

		*pVal = m_lFirstScoreToCompare;

		return S_OK;
	}			
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29269");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataScorerBasedAS::put_FirstScoreToCompare(long newVal)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29270");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataScorerBasedAS::get_IsSecondCondition(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		// Make sure the argument is valid
		ASSERT_ARGUMENT("ELI29293", pVal != NULL);

		*pVal = asVariantBool(m_bIsSecondCondition);

		return S_OK;
	}			
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29271");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataScorerBasedAS::put_IsSecondCondition( VARIANT_BOOL newVal)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29272");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataScorerBasedAS::get_SecondScoreCondition(EConditionalOp* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		// Make sure the argument is valid
		ASSERT_ARGUMENT("ELI29294", pVal != NULL);

		*pVal = m_eSecondScoreCondition;

		return S_OK;
	}			
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29273");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataScorerBasedAS::put_SecondScoreCondition(EConditionalOp newVal)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29274");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataScorerBasedAS::get_SecondScoreToCompare(long* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();
		
		// Make sure the argument is valid
		ASSERT_ARGUMENT("ELI29295", pVal != NULL);

		*pVal = m_lSecondScoreToCompare;

		return S_OK;
	}			
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29275");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataScorerBasedAS::put_SecondScoreToCompare(long newVal)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29276");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataScorerBasedAS::get_AndSecondCondition(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		// Make sure the argument is valid
		ASSERT_ARGUMENT("ELI29296", pVal != NULL);

		*pVal = asVariantBool(m_bAndConditions);

		return S_OK;
	}			
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29277");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataScorerBasedAS::put_AndSecondCondition(VARIANT_BOOL newVal)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29278");
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void CDataScorerBasedAS::validateLicense()
{
	static const unsigned long DATA_SCORER_BASED_AS_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE( DATA_SCORER_BASED_AS_ID, "ELI29257", 
		"Data Scorer Based Attribute Selector" );
}
//-------------------------------------------------------------------------------------------------
bool CDataScorerBasedAS::evaluateCondition(const EConditionalOp eOp, const long lAttributeScore, 
										   const long lConditionScore)
{
	// Determine the comparison to make
	switch (eOp)
	{
	case kEQ:
		return lAttributeScore == lConditionScore;
	case kNEQ:
		return lAttributeScore != lConditionScore;
	case kLT:
		return lAttributeScore < lConditionScore;
	case kGT:
		return lAttributeScore > lConditionScore;
	case kLEQ:
		return lAttributeScore <= lConditionScore;
	case kGEQ:
		return lAttributeScore >= lConditionScore;
	default:
		THROW_LOGIC_ERROR_EXCEPTION("ELI29305");
	}
}
//-------------------------------------------------------------------------------------------------
