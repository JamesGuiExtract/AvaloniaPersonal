// RemoveSubAttributes.cpp : Implementation of CRemoveSubAttributes
#include "stdafx.h"
#include "AFOutputHandlers.h"
#include "RemoveSubAttributes.h"

#include <SpecialStringDefinitions.h>
#include <Common.h>
#include <UCLIDException.h>
#include <COMUtils.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <CommentedTextFileReader.h>
#include <StringTokenizer.h>
#include <Misc.h>
#include <ComponentLicenseIDs.h>
#include <RuleSetProfiler.h>

using namespace std;
//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// Version 4: Added CIdentifiableObject
const unsigned long gnCurrentVersion = 5;

//-------------------------------------------------------------------------------------------------
// CRemoveSubAttributes
//-------------------------------------------------------------------------------------------------
CRemoveSubAttributes::CRemoveSubAttributes() :
m_ipDataScorer(NULL),
m_eCondition(kEQ),
m_eConditionComparisonType(kValueOf),
m_bConditionalRemove(false),
m_nScoreToCompare(0),
m_ipAS(NULL)
{
	try
	{
		m_ipAFUtility.CreateInstance(CLSID_AFUtility);
		ASSERT_RESOURCE_ALLOCATION("ELI09563", m_ipAFUtility != __nullptr);
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI09564")
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveSubAttributes::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IRemoveSubAttributes,
		&IID_IOutputHandler,
		&IID_IPersistStream,
		&IID_ICategorizedComponent,
		&IID_ISpecifyPropertyPages,
		&IID_ICopyableObject,
		&IID_IMustBeConfiguredObject,
		&IID_ILicensedComponent,
		&IID_IIdentifiableObject
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IRemoveSubAttributes
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveSubAttributes::get_DataScorer(IObjectWithDescription** pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		if(m_ipDataScorer == __nullptr)
		{
			m_ipDataScorer.CreateInstance(CLSID_ObjectWithDescription);
			ASSERT_RESOURCE_ALLOCATION("ELI09797", m_ipDataScorer != __nullptr);
		}

		CComQIPtr<IObjectWithDescription> ipDataScorer(m_ipDataScorer);
		ipDataScorer.CopyTo(pVal);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09783")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveSubAttributes::put_DataScorer(IObjectWithDescription* newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();
		ASSERT_ARGUMENT("ELI09789", newVal != __nullptr);

		m_ipDataScorer = newVal;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09784")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveSubAttributes::get_ScoreCondition(EConditionalOp* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		*pVal = m_eCondition;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09785")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveSubAttributes::put_ScoreCondition(EConditionalOp newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		m_eCondition = newVal;
		m_bDirty = true;

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09786")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveSubAttributes::get_ScoreToCompare(long* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();
		
		*pVal = m_nScoreToCompare;

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09787")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveSubAttributes::put_ScoreToCompare(long newVal){
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();
		m_nScoreToCompare = newVal;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09788")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveSubAttributes::get_ConditionalRemove(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();
		
		*pVal = m_bConditionalRemove ? VARIANT_TRUE : VARIANT_FALSE;

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19145")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveSubAttributes::put_ConditionalRemove(VARIANT_BOOL newVal){
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();
		m_bConditionalRemove = newVal == VARIANT_TRUE ? true : false;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19146")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveSubAttributes::get_AttributeSelector(IAttributeSelector ** pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();
		
		CComQIPtr<IAttributeSelector> ipAS(m_ipAS);
		ipAS.CopyTo(pVal);

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13312")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveSubAttributes::put_AttributeSelector(IAttributeSelector * newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();
		ASSERT_ARGUMENT("ELI13314", newVal != __nullptr);

		m_ipAS = newVal;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13313")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveSubAttributes::get_CompareConditionType(EConditionComparisonType* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		*pVal = m_eConditionComparisonType;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37981")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveSubAttributes::put_CompareConditionType(EConditionComparisonType newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		m_eConditionComparisonType = newVal;
		m_bDirty = true;

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37982")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IOutputHandler
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveSubAttributes::raw_ProcessOutput(IIUnknownVector* pAttributes, IAFDocument *pAFDoc,
													 IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		IIUnknownVectorPtr ipAttributes(pAttributes);
		ASSERT_RESOURCE_ALLOCATION("ELI09544", ipAttributes != __nullptr);
		IIUnknownVectorPtr ipFoundAttributes = __nullptr;

		if(m_bConditionalRemove)
		{
			long nMaxOrMinScore = (m_eConditionComparisonType == kCompareMinimum) ? 100 : 0;
			map<long, long> mapOfScores;

			mapOfScores.clear();

			{
				PROFILE_RULE_OBJECT("", "", m_ipAS, 0);

				// Select the attributes
				ipFoundAttributes = m_ipAS->SelectAttributes( ipAttributes, pAFDoc, ipAttributes );
			}

			// Get the Data scorer object
			IDataScorerPtr ipDataScorer = m_ipDataScorer->GetObject();
			ASSERT_RESOURCE_ALLOCATION("ELI09802", ipDataScorer != __nullptr);

			long lSize = ipFoundAttributes->Size();
			for (long i = 0; i < lSize; i++)
			{
				IAttributePtr ipAttr = ipFoundAttributes->At(i);
				ASSERT_RESOURCE_ALLOCATION("ELI09566", ipAttr != __nullptr);

				long nScore;
				{
					PROFILE_RULE_OBJECT(asString(m_ipDataScorer->Description), "", ipDataScorer, 0);
					
					nScore = ipDataScorer->GetDataScore1(ipAttr, pAFDoc);

				}

				switch (m_eConditionComparisonType)
				{
				case kCompareMinimum:
					if (nMaxOrMinScore > nScore)
					{
						nMaxOrMinScore = nScore;
					}
					mapOfScores[i] = nScore;
					break;
				case kCompareMaximum:
					if (nMaxOrMinScore < nScore)
					{
						nMaxOrMinScore = nScore;
					}
					mapOfScores[i] = nScore;
					break;
				}

			
				// We are going to remove all the attributes in ipFoundAttributes'
				// so if an attribute is to remain it must be taken out of 
				// ipFoundAttributes
				if(m_eConditionComparisonType == kValueOf)
				{
					bool bRemove = compareWithCondition(nScore, m_nScoreToCompare);
					if (!bRemove)
					{
						ipFoundAttributes->Remove(i);
						i--;
						lSize--;
					}
				}
			}

			// if the comparisonType is not kValueOf need to determine which items to to remove
			if (m_eConditionComparisonType != kValueOf)
			{
				// Compare the saved scores in reverse order with the 
				// determined value
				for (long i = mapOfScores.size() - 1; i >= 0; i--)
				{
					// if the attribute is not to be removed it needs to be
					// removed from the ipFoundAttributes vector
					if (!compareWithCondition(mapOfScores[i], nMaxOrMinScore))
					{
						ipFoundAttributes->Remove(i);
					}
				}
			}
		}
		else
		{
			PROFILE_RULE_OBJECT("", "", m_ipAS, 0);

			// Select the attributes
			ipFoundAttributes = m_ipAS->SelectAttributes( ipAttributes, pAFDoc, ipAttributes );
		}
		// if attributes were found(selected) remove them form the source vector
		if ( ipFoundAttributes != __nullptr && ipFoundAttributes->Size() > 0 )
		{
			m_ipAFUtility->RemoveAttributes(ipAttributes, ipFoundAttributes); 
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09545")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveSubAttributes::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CRemoveSubAttributes::raw_IsConfigured(VARIANT_BOOL * pbValue)
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

		if ( m_ipAS != __nullptr )
		{
			IMustBeConfiguredObjectPtr ipCfgObj ( m_ipAS );
			if ( m_ipAS == __nullptr )
			{
				UCLIDException ue("ELI13347", "Attribute Selector Object must support IMustBeConfiguredObject.");
				throw ue;
			}
			*pbValue = ipCfgObj->IsConfigured();
		}
		else
		{
			*pbValue = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09546");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveSubAttributes::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19553", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Remove attributes").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09547")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveSubAttributes::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_AFOUTPUTHANDLERSLib::IRemoveSubAttributesPtr ipSource = pObject;
		ASSERT_RESOURCE_ALLOCATION( "ELI09548", ipSource != __nullptr );
		
		m_bConditionalRemove = ipSource->ConditionalRemove == VARIANT_TRUE ? true : false;
		m_ipDataScorer = ipSource->DataScorer;
		m_eCondition = ipSource->ScoreCondition;
		m_nScoreToCompare = ipSource->ScoreToCompare;
		m_ipAS = ipSource->AttributeSelector;
		m_eConditionComparisonType = ipSource->CompareConditionType;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12819");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveSubAttributes::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		// Create another instance of this object
		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance( CLSID_RemoveSubAttributes );
		ASSERT_RESOURCE_ALLOCATION( "ELI09549", ipObjCopy != __nullptr );

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom( ipUnk );
	
		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09550");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveSubAttributes::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pClassID = CLSID_RemoveSubAttributes;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveSubAttributes::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveSubAttributes::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		m_eCondition = kEQ;
		m_bConditionalRemove = false;
		m_nScoreToCompare = 0;
		m_eConditionComparisonType = kValueOf;

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
			UCLIDException ue( "ELI09551", 
				"Unable to load newer Remove SubAttributes Output Handler." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}
		if ( nDataVersion < 3 )
		{
			// Create a QueryBased
			// load data here
			string strQuery;
			dataReader >> strQuery;
			IQueryBasedASPtr ipQS(CLSID_QueryBasedAS);
			ASSERT_RESOURCE_ALLOCATION("ELI13319", ipQS != __nullptr );

			ipQS->QueryText = strQuery.c_str();
			m_ipAS = ipQS;
			ASSERT_RESOURCE_ALLOCATION("ELI13320", m_ipAS != __nullptr );
		}
		
		if(nDataVersion >= 2)
		{
			dataReader >> m_bConditionalRemove;
			if(m_bConditionalRemove)
			{
				long nTmp;
				dataReader >> nTmp;
				m_eCondition = (EConditionalOp)nTmp;
				dataReader >> m_nScoreToCompare;
			}
		}

		if (nDataVersion >= 5 && m_bConditionalRemove)
		{
			long nTmp = 0;
			dataReader >> nTmp;
			m_eConditionComparisonType = (EConditionComparisonType) nTmp;
		}

		// read the data scorer object
		if (nDataVersion >= 2)
		{
			if(m_bConditionalRemove)
			{
				// read the data scorer object
				IPersistStreamPtr ipObj;
				::readObjectFromStream(ipObj, pStream, "ELI09960");
				ASSERT_RESOURCE_ALLOCATION("ELI09794", ipObj != __nullptr);
				m_ipDataScorer = ipObj;
			}
		}
		if ( nDataVersion >= 3 )
		{
			IPersistStreamPtr ipObj;
			::readObjectFromStream(ipObj, pStream, "ELI13321");
			ASSERT_RESOURCE_ALLOCATION("ELI13322", ipObj != __nullptr);
			m_ipAS = ipObj;
		}

		if (nDataVersion >= 4)
		{
			// Load the GUID for the IIdentifiableObject interface.
			loadGUID(pStream);
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09552");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveSubAttributes::Save(IStream *pStream, BOOL fClearDirty)
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
		dataWriter << m_bConditionalRemove;
		if(m_bConditionalRemove)
		{
			dataWriter << (long)m_eCondition;
			dataWriter << m_nScoreToCompare;
			dataWriter << (long)m_eConditionComparisonType;
		}
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write( &nDataLength, sizeof(nDataLength), NULL );
		pStream->Write( data.getData(), nDataLength, NULL );

		if(m_bConditionalRemove)
		{
			// Make sure DataScorer object-with-description exists
			IObjectWithDescriptionPtr ipObjWithDesc = getThisAsCOMPtr()->DataScorer;
			ASSERT_RESOURCE_ALLOCATION("ELI09795", ipObjWithDesc != __nullptr);
			
			// write the data-scorer object to the stream
			IPersistStreamPtr ipObj = ipObjWithDesc;
			if (ipObj == __nullptr)
			{
				throw UCLIDException("ELI09796", "DataScorer object does not support persistence.");
			}
			writeObjectToStream(ipObj, pStream, "ELI09915", fClearDirty);
		}
		if ( m_ipAS == __nullptr )
		{
			throw UCLIDException("ELI13323", "Attribute Selector is not set.");
		}
		IPersistStreamPtr ipObj = m_ipAS;
		if (ipObj == __nullptr)
		{
			throw UCLIDException("ELI13324", "Attribute Selector object does not support persistence.");
		}
		writeObjectToStream(ipObj, pStream, "ELI13325", fClearDirty);

		// Save the GUID for the IIdentifiableObject interface.
		saveGUID(pStream);
		
		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09553");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveSubAttributes::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// IIdentifiableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveSubAttributes::get_InstanceGUID(GUID *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pVal = getGUID();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33540")
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
UCLID_AFOUTPUTHANDLERSLib::IRemoveSubAttributesPtr CRemoveSubAttributes::getThisAsCOMPtr()
{
	UCLID_AFOUTPUTHANDLERSLib::IRemoveSubAttributesPtr ipThis(this);
	ASSERT_RESOURCE_ALLOCATION("ELI16964", ipThis != __nullptr);
	
	return ipThis;
}
//-------------------------------------------------------------------------------------------------
void CRemoveSubAttributes::validateLicense()
{
	static const unsigned long REMOVE_SUB_ATTRIBUTES_ID = gnRULE_WRITING_CORE_OBJECTS;

	VALIDATE_LICENSE( REMOVE_SUB_ATTRIBUTES_ID, "ELI09554", 
		"Remove SubAttributes Output Handler" );
}
//-------------------------------------------------------------------------------------------------
bool CRemoveSubAttributes::compareWithCondition(long itemScore, long nComparisonScore)
{
	bool bRemove = false;
	switch(m_eCondition)
	{
	case kEQ:
		bRemove = (itemScore == nComparisonScore);
		break;
	case kNEQ:
		bRemove = (itemScore != nComparisonScore);
		break;
	case kLT:
		bRemove = (itemScore < nComparisonScore);
		break;
	case kGT:
		bRemove = (itemScore > nComparisonScore);
		break;
	case kLEQ:
		bRemove = (itemScore <= nComparisonScore);
		break;
	case kGEQ:
		bRemove = (itemScore >= nComparisonScore);
		break;
	}
	return bRemove;
}
