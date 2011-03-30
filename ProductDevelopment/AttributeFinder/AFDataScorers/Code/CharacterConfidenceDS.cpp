// CharacterConfidenceDS.cpp : Implementation of CCharacterConfidenceDS

#include "stdafx.h"
#include "CharacterConfidenceDS.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <COMUtils.h>
#include <cpputil.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CCharacterConfidenceDS
//-------------------------------------------------------------------------------------------------
CCharacterConfidenceDS::CCharacterConfidenceDS()
: m_eAggregateFunction(kAverage)
{
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceDS::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ICharacterConfidenceDS,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_IPersistStream,
		&IID_ILicensedComponent,
		&IID_IDataScorer,
		&IID_IMustBeConfiguredObject
	};

	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IDataScorer
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceDS::raw_GetDataScore1(IAttribute * pAttribute, LONG * pScore)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// validate License
		validateLicense();
		
		// Make sure the arguments are valid
		ASSERT_ARGUMENT("ELI29338", pScore != __nullptr );
		IAttributePtr ipAttribute(pAttribute);
		ASSERT_ARGUMENT("ELI29335", ipAttribute != __nullptr);

		// Get the score
		*pScore = getAttributeScore(ipAttribute);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29453")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceDS::raw_GetDataScore2(IIUnknownVector * pAttributes, LONG * pScore)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// validate License
		validateLicense();
		
		// Make sure the arguments are valid
		ASSERT_ARGUMENT("ELI29339", pScore != __nullptr );
		IIUnknownVectorPtr ipAttributes(pAttributes);
		ASSERT_ARGUMENT("ELI29340", ipAttributes != __nullptr);

		// Calculate the score
		//		if kAverage then score will be average of each attributes score
		//		if kMinimum then score will be the minimum score for all attributes
		//		if kMaximum then score will be the maximum score for all attributes
		long nTmpScore = 0;
		long nScore = 0;
		long nSize = ipAttributes->Size();
		for (long n=0; n < nSize; n++)
		{
			IAttributePtr ipAttribute = ipAttributes->At(n);
			ASSERT_ARGUMENT("ELI29341", ipAttribute != __nullptr);

			// Get the score for this attribute
			nTmpScore = getAttributeScore(ipAttribute);

			switch (m_eAggregateFunction)
			{
			case kAverage:
				nScore += nTmpScore;
				break;
			case kMinimum:
				if (n == 0 || nTmpScore < nScore)
				{
					nScore = nTmpScore;
				}
				break;
			case kMaximum:
				if (n == 0 || nTmpScore > nScore)
				{
					nScore = nTmpScore;
				}
				break;
			}
		}

		// If looking for the average will return the average of the average of all attributes
		if (m_eAggregateFunction == kAverage)
		{
			nScore = nScore / nSize;
		}

		*pScore = nScore;
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29318")
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceDS::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI29319", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Character confidence data scorer").Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29320")
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceDS::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	*pClassID = CLSID_CharacterConfidenceDS;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceDS::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceDS::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// validate License
		validateLicense();

		ASSERT_ARGUMENT("ELI29345",  pStream != __nullptr);
		
		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		pStream->Read(&nDataLength, sizeof(nDataLength), NULL);
		ByteStream data(nDataLength);
		pStream->Read(data.getData(), nDataLength, NULL);
		ByteStreamManipulator dataReader(ByteStreamManipulator::kRead, data);

		// Read the individual data items from the bytestream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;
		
		long nTmp;
		
		dataReader >> nTmp;
		m_eAggregateFunction = (EAggregateFunctions)nTmp;

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29312");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceDS::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// validate License
		validateLicense();

		ASSERT_ARGUMENT("ELI29344", pStream != __nullptr);
		
		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter( ByteStreamManipulator::kWrite, data );
		dataWriter << gnCurrentVersion;

		dataWriter << (long)m_eAggregateFunction;

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

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29313");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceDS::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceDS::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// validate License
		validateLicense();

		UCLID_AFDATASCORERSLib::ICharacterConfidenceDSPtr ipFrom(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI29324", ipFrom != __nullptr);

		m_eAggregateFunction = (EAggregateFunctions)ipFrom->AggregateFunction;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29314");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceDS::raw_Clone(IUnknown **ppObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// validate License
		validateLicense();
		
		ASSERT_ARGUMENT("ELI29342", ppObject != __nullptr);

		// create a new instance of the EntityNameDataScorer
		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance(CLSID_CharacterConfidenceDS);
		ASSERT_RESOURCE_ALLOCATION("ELI29315", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*ppObject = ipObjCopy.Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29316");
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceDS::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// ICharacterConfidenceDS
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceDS::get_AggregateFunction(EAggregateFunctions *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// validate License
		validateLicense();
		
		ASSERT_ARGUMENT("ELI29323", pVal != __nullptr);

		*pVal = m_eAggregateFunction;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29321");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceDS::put_AggregateFunction(EAggregateFunctions newVal)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29322");
}
//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCharacterConfidenceDS::raw_IsConfigured(VARIANT_BOOL * pbConfigured)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate the license
		validateLicense();

		ASSERT_ARGUMENT("ELI29343", pbConfigured != __nullptr);

		// Make sure the argument is valid
		*pbConfigured = VARIANT_TRUE;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29327");
}
//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void CCharacterConfidenceDS::validateLicense()
{
	static const unsigned long CHARACTER_CONFIDENCE_DATA_SCORER_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE( CHARACTER_CONFIDENCE_DATA_SCORER_ID, "ELI29317", "Character confidence Data Scorer" );
}
//-------------------------------------------------------------------------------------------------
long CCharacterConfidenceDS::getAttributeScore(IAttributePtr ipAttr)
{
	long nScore;

	// Get the value
	ISpatialStringPtr ipValue = ipAttr->Value;
	ASSERT_RESOURCE_ALLOCATION("ELI29336", ipValue != __nullptr);

	// Get the score
	switch (m_eAggregateFunction)
	{
	case kAverage:
		ipValue->GetCharConfidence(NULL, NULL, &nScore);
		break;
	case kMinimum:
		ipValue->GetCharConfidence(&nScore, NULL, NULL);
		break;
	case kMaximum:
		ipValue->GetCharConfidence(NULL, &nScore, NULL);
		break;
	default:
		THROW_LOGIC_ERROR_EXCEPTION("ELI29337");
	}

	return nScore;
}
//-------------------------------------------------------------------------------------------------
