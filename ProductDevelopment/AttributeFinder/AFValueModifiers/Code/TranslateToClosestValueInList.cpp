// TranslateToClosestValueInList.cpp : Implementation of CTranslateToClosestValueInList
#include "stdafx.h"
#include "AFValueModifiers.h"
#include "TranslateToClosestValueInList.h"

#include <UCLIDException.h>
#include <CommentedTextFileReader.h>
#include <cpputil.h>
#include <COMUtils.h>
#include <LicenseMgmt.h>
#include <LevenshteinDistance.h>
#include <ComponentLicenseIDs.h>

#include <fstream>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 2;

// Minimum match score to allow automatic replacement
const double gMATCH_THRESHOLD = 70;

//-------------------------------------------------------------------------------------------------
// CTranslateToClosestValueInList
//-------------------------------------------------------------------------------------------------
CTranslateToClosestValueInList::CTranslateToClosestValueInList()
: m_ipClosestValuesList(CLSID_VariantVector), 
  m_bCaseSensitive(false),
  m_bForceMatch(false),
  m_bDirty(false)
{
	try
	{
		ASSERT_RESOURCE_ALLOCATION("ELI19312", m_ipClosestValuesList!=NULL);
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI06641")
}
//-------------------------------------------------------------------------------------------------
CTranslateToClosestValueInList::~CTranslateToClosestValueInList()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16367");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateToClosestValueInList::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ITranslateToClosestValueInList,
		&IID_IAttributeModifyingRule,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_ILicensedComponent,
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
// IAttributeModifyingRule
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateToClosestValueInList::raw_ModifyValue(IAttribute* pAttribute, 
															 IAFDocument* pOriginInput, 
															 IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		IAttributePtr	ipAttribute(pAttribute);
		ASSERT_RESOURCE_ALLOCATION( "ELI09294", ipAttribute != __nullptr );

		ISpatialStringPtr ipInputText = ipAttribute->GetValue();
		ASSERT_RESOURCE_ALLOCATION("ELI06642", ipInputText != __nullptr);

		LevenshteinDistance editDistance;
		editDistance.SetFlags(false, m_bCaseSensitive);

		// Get a list of values that includes values from any specified files.
		IVariantVectorPtr ipExpandedValuesList =
				m_cachedListLoader.expandList(m_ipClosestValuesList, pOriginInput);
			ASSERT_RESOURCE_ALLOCATION("ELI30069", ipExpandedValuesList != __nullptr)
		
		string strInput = asString(ipInputText->String);
		string strToMatch = asString(ipExpandedValuesList->GetItem(0).bstrVal);
		double dLeastDifference = editDistance.GetPercent(strInput, strToMatch);
		string strWithLeastDifference(strToMatch);

		// Check other values if this was not a perfect match
		if (dLeastDifference != 0)
		{
			// Iterate through all available values
			long nSize = ipExpandedValuesList->Size;
			
			for (long n=1; n<nSize; n++)
			{
				strToMatch = asString(ipExpandedValuesList->GetItem(n).bstrVal);

				// Get the edit distance
				double dPercentDifference = editDistance.GetPercent(strInput, strToMatch);
				if (dPercentDifference < dLeastDifference)
				{
					// Only store the closest match
					dLeastDifference = dPercentDifference;
					strWithLeastDifference = strToMatch;
					
					// Have we found a perfect match?
					if (dLeastDifference == 0)
					{
						// Can stop checking remainder of list
						break;
					}
				}
			}
		}
		
		// Check to see if match is close enough or should be forced
		if (m_bForceMatch || (dLeastDifference <= gMATCH_THRESHOLD))
		{
			ipInputText->Replace(strInput.c_str(), strWithLeastDifference.c_str(), 
				VARIANT_TRUE, 0, NULL);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04212");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateToClosestValueInList::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19607", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Translate to closest value in list").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04213");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ITranslateToClosestValueInList
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateToClosestValueInList::get_IsCaseSensitive(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_bCaseSensitive ? VARIANT_TRUE: VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04228");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateToClosestValueInList::put_IsCaseSensitive(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bCaseSensitive = newVal==VARIANT_TRUE;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04229");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateToClosestValueInList::get_ClosestValueList(IVariantVector **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		IVariantVectorPtr ipShallowCopy = m_ipClosestValuesList;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04328");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateToClosestValueInList::put_ClosestValueList(IVariantVector *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_ipClosestValuesList = newVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04329");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateToClosestValueInList::get_IsForcedMatch(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Provide value
		*pVal = m_bForceMatch ? VARIANT_TRUE: VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04930");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateToClosestValueInList::put_IsForcedMatch(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Set value
		m_bForceMatch = (newVal == VARIANT_TRUE);

		// Set Dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05013");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateToClosestValueInList::LoadValuesFromFile(BSTR strFileFullName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// reset content of map
		m_ipClosestValuesList->Clear();
		
		string strFileName = asString(strFileFullName);
		ifstream ifs(strFileName.c_str());
		CommentedTextFileReader fileReader(ifs, "//", true);
		
		while (!ifs.eof())
		{
			string strLine("");
			strLine = fileReader.getLineText();
			if (!strLine.empty())
			{				
				m_ipClosestValuesList->PushBack(_bstr_t(strLine.c_str()));
			}
		};

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04330");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateToClosestValueInList::SaveValuesToFile(BSTR strFileFullName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		string strFileToSave = asString(strFileFullName);

		// always overwrite if the file exists
		ofstream ofs(strFileToSave.c_str(), ios::out | ios::trunc);
		
		// iterate through the vector
		string strValue("");
		long nSize = m_ipClosestValuesList->Size;
		for (long n=0; n<nSize; n++)
		{
			strValue = asString(_bstr_t(m_ipClosestValuesList->GetItem(n)));
			// save the value to the file
			ofs << strValue << endl;
		}

		ofs.close();
		waitForFileToBeReadable(strFileToSave);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05574");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateToClosestValueInList::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		UCLID_AFVALUEMODIFIERSLib::ITranslateToClosestValueInListPtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI08289", ipSource != __nullptr);
		
		// Copy list of strings
		ICopyableObjectPtr ipCopyObj = ipSource->ClosestValueList;
		if (ipCopyObj)
		{
			m_ipClosestValuesList = ipCopyObj->Clone();
		}

		// Copy case-sensitive flag
		m_bCaseSensitive = (ipSource->GetIsCaseSensitive()==VARIANT_TRUE) ? true : false;
	
		// Copy forced-match flag
		m_bForceMatch = (ipSource->GetIsForcedMatch()==VARIANT_TRUE) ? true : false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08290");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateToClosestValueInList::raw_Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy(CLSID_TranslateToClosestValueInList);
		ASSERT_RESOURCE_ALLOCATION("ELI08362", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04462");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateToClosestValueInList::raw_IsConfigured(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		*pbValue = m_ipClosestValuesList->Size > 0 ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04845")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateToClosestValueInList::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateToClosestValueInList::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_TranslateToClosestValueInList;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateToClosestValueInList::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// check m_bDirty flag first, if it's not dirty then
		// check all objects owned by this object
		HRESULT hr = m_bDirty ? S_OK : S_FALSE;
		if (!m_bDirty)
		{
			IPersistStreamPtr ipPersistStream(m_ipClosestValuesList);
			if (ipPersistStream==NULL)
			{
				throw UCLIDException("ELI04799", "Object does not support persistence!");
			}
			
			hr = ipPersistStream->IsDirty();
		}

		return hr;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04800");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
// NOTES about versions:
// Version 1:
//   * Saved: 
//            data version,
//            case-sensitivity flag,
//            collection of Closest Value strings
// Version 2:
//   * Additionally saved:
//            forced match flag
//   * NOTE:
//            new flag located immediately after Case Sensitive flag
STDMETHODIMP CTranslateToClosestValueInList::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Clear the variables first
		m_bCaseSensitive = false;
		m_ipClosestValuesList = __nullptr;

		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		pStream->Read( &nDataLength, sizeof(nDataLength), NULL );
		ByteStream data( nDataLength );
		pStream->Read( data.getData(), nDataLength, NULL );
		ByteStreamManipulator dataReader( ByteStreamManipulator::kRead, data );

		// Read the individual data items from the bytestream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;

		// Check for newer version
		if (nDataVersion > gnCurrentVersion)
		{
			// Throw exception
			UCLIDException ue( "ELI07662", "Unable to load newer TranslateToClosestValueInList Modifier." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		if (nDataVersion >= 1)
		{
			dataReader >> m_bCaseSensitive;
		}

		// Forced Match setting only in versions >= 2
		if (nDataVersion >= 2)
		{
			dataReader >> m_bForceMatch;
		}

		// Separately read in the closest value list
		IPersistStreamPtr ipObj;
		::readObjectFromStream(ipObj, pStream, "ELI09971");
		if (ipObj == __nullptr)
		{
			throw UCLIDException( "ELI04720", 
				"Closest value list could not be read from stream!" );
		}

		m_ipClosestValuesList = ipObj;

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04718");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
// Version 2:
//    Added m_bForceMatch
STDMETHODIMP CTranslateToClosestValueInList::Save(IStream *pStream, BOOL fClearDirty)
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
		dataWriter << m_bCaseSensitive;
		dataWriter << m_bForceMatch;
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write( &nDataLength, sizeof(nDataLength), NULL );
		pStream->Write( data.getData(), nDataLength, NULL );

		// Separately write the Closest Values list to the IStream object
		IPersistStreamPtr ipObj( m_ipClosestValuesList );
		if (ipObj == __nullptr)
		{
			throw UCLIDException( "ELI04721", 
				"Closest value list object does not support persistence!" );
		}
		else
		{
			::writeObjectToStream(ipObj, pStream, "ELI09926", fClearDirty);
		}

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04719");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateToClosestValueInList::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CTranslateToClosestValueInList::validateLicense()
{
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI04214", "Translate To Closest Value" );
}
//-------------------------------------------------------------------------------------------------
