// TranslateValue.cpp : Implementation of CTranslateValue
#include "stdafx.h"
#include "AFValueModifiers.h"
#include "TranslateValue.h"

#include <UCLIDException.h>
#include <CommentedTextFileReader.h>
#include <StringTokenizer.h>
#include <COMUtils.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <ComponentLicenseIDs.h>

#include <fstream>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 2;

//-------------------------------------------------------------------------------------------------
// CTranslateValue
//-------------------------------------------------------------------------------------------------
CTranslateValue::CTranslateValue()
: m_bCaseSensitive(false),
  m_ipTranslationStringPairs(CLSID_IUnknownVector),
  m_bDirty(false),
  m_eTranslateFieldType(kTranslateValue)
{
}
//-------------------------------------------------------------------------------------------------
CTranslateValue::~CTranslateValue()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16368");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateValue::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ITranslateValue,
		&IID_IAttributeModifyingRule,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_ILicensedComponent,
		&IID_IOutputHandler,
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
STDMETHODIMP CTranslateValue::raw_ModifyValue(IAttribute* pAttribute, IAFDocument* pOriginInput,
											  IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		IAttributePtr	ipAttribute(pAttribute);
		ASSERT_RESOURCE_ALLOCATION("ELI09295", ipAttribute != __nullptr );

		IAFDocumentPtr ipAFDoc(pOriginInput);
		ASSERT_RESOURCE_ALLOCATION("ELI30071", ipAFDoc != __nullptr);

		ISpatialStringPtr ipInputText;
		string strInputText;
		if (m_eTranslateFieldType == kTranslateValue)
		{
			ipInputText = ipAttribute->Value;
			ASSERT_RESOURCE_ALLOCATION("ELI09296", ipInputText != __nullptr);
			strInputText = asString(ipInputText->String);
		}
		else if (m_eTranslateFieldType == kTranslateType)
		{
			strInputText = asString(ipAttribute->Type);
		}
		else
		{
			THROW_LOGIC_ERROR_EXCEPTION("ELI09661");
		}

		if (m_ipTranslationStringPairs)
		{
			if(m_eTranslateFieldType == kTranslateValue)
			{
				if (!m_bCaseSensitive)
				{
					::makeUpperCase(strInputText);
				}
				
				IIUnknownVectorPtr ipExpandedTranslationPairs =
					m_cachedListLoader.expandTwoColumnList(m_ipTranslationStringPairs, ';', ipAFDoc);
				ASSERT_RESOURCE_ALLOCATION("ELI30072", ipExpandedTranslationPairs != __nullptr);

				long nSize = ipExpandedTranslationPairs->Size();
				for (long n=0; n<nSize; n++)
				{
					IStringPairPtr ipStringPair(ipExpandedTranslationPairs->At(n));
					if (ipStringPair)
					{
						// get the string to be translated from
						string strTransFrom = asString(ipStringPair->StringKey);
						// get the translate to string
						string strTransTo = asString(ipStringPair->StringValue);

						// compare input string with translate from string
						if (!m_bCaseSensitive)
						{
							::makeUpperCase(strTransFrom);
						}
						if (strTransFrom == strInputText)
						{
							// This is a match, replace and return
							if (strTransFrom.empty())
							{
								// Matching an empty value. Just set the string.
								if (ipInputText->HasSpatialInfo() == VARIANT_TRUE)
								{
									ipInputText->ReplaceAndDowngradeToHybrid(strTransTo.c_str());
								}
								else
								{
									ipInputText->ReplaceAndDowngradeToNonSpatial(strTransTo.c_str());
								}
							}
							else
							{
								// Replace the existing value.
								ipInputText->Replace(strInputText.c_str(), strTransTo.c_str(),
									asVariantBool(m_bCaseSensitive), 0, NULL);
							}
							break;
						}
					}
				}
			}
			else if(m_eTranslateFieldType == kTranslateType)
			{
				vector<string> vecTokens;
				StringTokenizer::sGetTokens(strInputText, "+", vecTokens);
				string strNewType = "";

				// If no tokens were found, check for the empty string.
				if (vecTokens.size() == 0)
				{
					vecTokens.push_back("");
				}

				unsigned int ui;
				for (ui = 0; ui < vecTokens.size(); ui++)
				{
					string strCmp = vecTokens[ui];
					if (!m_bCaseSensitive)
					{
						::makeUpperCase(strCmp);
					}

					IIUnknownVectorPtr ipExpandedTranslationPairs =
						m_cachedListLoader.expandTwoColumnList(m_ipTranslationStringPairs, ';', ipAFDoc);
					ASSERT_RESOURCE_ALLOCATION("ELI30073", ipExpandedTranslationPairs != __nullptr);

					long nSize = ipExpandedTranslationPairs->Size();
					for (long n = 0; n < nSize; n++)
					{
						IStringPairPtr ipStringPair(ipExpandedTranslationPairs->At(n));
						if (ipStringPair)
						{
							// get the string to be translated from
							string strTransFrom = asString(ipStringPair->StringKey);
							// get the translate to string
							string strTransTo = asString(ipStringPair->StringValue);

							// compare input string with translate from string
							if (!m_bCaseSensitive)
							{
								::makeUpperCase(strTransFrom);
							}

							if(strCmp == strTransFrom)
							{
								vecTokens[ui] = strTransTo;
								break;
							}
						}
					}
					strNewType = strNewType + vecTokens[ui];
					if (ui != vecTokens.size() - 1)
					{
						strNewType = strNewType + "+";
					}
				}
				ipAttribute->Type = _bstr_t(strNewType.c_str());
			}
			else
			{
				THROW_LOGIC_ERROR_EXCEPTION("ELI09660");
			}
		}		
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04203");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateValue::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19608", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Translate values or types").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04204");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateValue::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		// get the str to str map out from the pass-in object
		UCLID_AFVALUEMODIFIERSLib::ITranslateValuePtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI08291", ipSource != __nullptr);
		
		ICopyableObjectPtr ipCopyObj = ipSource->GetTranslationStringPairs();
		ASSERT_RESOURCE_ALLOCATION("ELI08292", ipCopyObj != __nullptr);
		m_ipTranslationStringPairs = ipCopyObj->Clone();
					
		m_bCaseSensitive = (ipSource->GetIsCaseSensitive()==VARIANT_TRUE) ? true : false;

		m_eTranslateFieldType = (ETranslateFieldType)ipSource->TranslateFieldType;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08294");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateValue::raw_Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy(CLSID_TranslateValue);
		ASSERT_RESOURCE_ALLOCATION("ELI08363", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04459");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateValue::raw_IsConfigured(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		*pbValue = m_ipTranslationStringPairs->Size() > 0 ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04844")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateValue::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// ITranslateValue
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateValue::get_TranslationStringPairs(IIUnknownVector **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		IIUnknownVectorPtr ipShallowCopy = m_ipTranslationStringPairs;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04227");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateValue::put_TranslationStringPairs(IIUnknownVector *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		IIUnknownVectorPtr ipNewVal(pVal);

		// Need to validate the type identifiers
		if (m_eTranslateFieldType == kTranslateType)
		{
			if (ipNewVal != __nullptr)
			{
				long nSize = ipNewVal->Size();
				for (long i=0; i < nSize; i++)
				{
					IStringPairPtr ipPair = ipNewVal->At(i);
					ASSERT_RESOURCE_ALLOCATION("ELI28579", ipPair != __nullptr);

					// Only validate the type it is going to
					string strTemp = asString(ipPair->StringValue);
					if (!strTemp.empty() && !isValidIdentifier(strTemp))
					{
						UCLIDException uex("ELI28580", "Invalid attribute type identifier specified.");
						uex.addDebugInfo("Invalid Type", strTemp);
						throw uex;
					}
				}
			}
		}

		if (m_ipTranslationStringPairs != __nullptr)
		{
			m_ipTranslationStringPairs = __nullptr;
		}

		m_ipTranslationStringPairs = pVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04241");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateValue::get_IsCaseSensitive(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_bCaseSensitive ? VARIANT_TRUE: VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19284");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateValue::put_IsCaseSensitive(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bCaseSensitive = newVal==VARIANT_TRUE;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19285");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateValue::LoadTranslationsFromFile(BSTR strFileFullName, BSTR strDelimiter)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// delimiter can only be a single char
		string strDelim = asString(strDelimiter);
		if (strDelim.size() > 1)
		{
			UCLIDException uclidException("ELI04232", "Delimiter shall be one character long.");
			uclidException.addDebugInfo("Delimiter", strDelim);
			throw uclidException;
		}

		loadFromFile(asString(strFileFullName), strDelim.at(0));

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04231");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateValue::SaveTranslationsToFile(BSTR strFileFullName, BSTR cDelimiter)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		string strFileToSave = asString(strFileFullName);
		string strDelimiter = asString(cDelimiter);
	
		// always overwrite if the file exists
		ofstream ofs(strFileToSave.c_str(), ios::out | ios::trunc);
		
		// iterate through the vector
		string strToBeTranslated(""), strTranslation("");
		long nSize = m_ipTranslationStringPairs->Size();
		for (long n=0; n<nSize; n++)
		{
			IStringPairPtr ipStringPair(m_ipTranslationStringPairs->At(n));
			if (ipStringPair)
			{
				strToBeTranslated = ipStringPair->StringKey;
				strTranslation = ipStringPair->StringValue;
				// save the value to the file
				ofs << strToBeTranslated << strDelimiter << strTranslation << endl;
			}
		}

		ofs.close();
		waitForFileToBeReadable(strFileToSave);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05572");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateValue::get_TranslateFieldType(ETranslateFieldType* newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*newVal = m_eTranslateFieldType;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09655");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateValue::put_TranslateFieldType(ETranslateFieldType newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		m_eTranslateFieldType = newVal;

		// Need to validate the type identifiers
		if (m_eTranslateFieldType == kTranslateType)
		{
			if (m_ipTranslationStringPairs != __nullptr)
			{
				long nSize = m_ipTranslationStringPairs->Size();
				for (long i=0; i < nSize; i++)
				{
					IStringPairPtr ipPair = m_ipTranslationStringPairs->At(i);
					ASSERT_RESOURCE_ALLOCATION("ELI28581", ipPair != __nullptr);

					// Only validate the type it is going to
					string strTemp = asString(ipPair->StringValue);
					if (!strTemp.empty() && !isValidIdentifier(strTemp))
					{
						UCLIDException uex("ELI28582", "Invalid attribute type identifier specified.");
						uex.addDebugInfo("Invalid Type", strTemp);
						throw uex;
					}
				}
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09656");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IOutputHandler
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateValue::raw_ProcessOutput(IIUnknownVector * pAttributes, IAFDocument * pDoc, 
												IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Create AFUtility object
		IAFUtilityPtr ipAFUtility( CLSID_AFUtility );
		ASSERT_RESOURCE_ALLOCATION( "ELI09662", ipAFUtility != __nullptr );

		// Use Attributes as smart pointer
		IIUnknownVectorPtr ipAttributes( pAttributes );
		ASSERT_RESOURCE_ALLOCATION("ELI09663", ipAttributes != __nullptr);

		// Apply Attribute Modification
		ipAFUtility->ApplyAttributeModifier( ipAttributes, pDoc, this, VARIANT_TRUE );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09665");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateValue::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_TranslateValue;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateValue::IsDirty(void)
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
			IPersistStreamPtr ipPersistStream(m_ipTranslationStringPairs);
			if (ipPersistStream==NULL)
			{
				throw UCLIDException("ELI04801", "Object does not support persistence!");
			}
			
			hr = ipPersistStream->IsDirty();
		}

		return hr;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04802");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateValue::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Clear the variables first
		m_bCaseSensitive = false;
		m_ipTranslationStringPairs = __nullptr;

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
			UCLIDException ue( "ELI07663", "Unable to load newer TranslateValue Modifier." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		if (nDataVersion >= 1)
		{
			dataReader >> m_bCaseSensitive;
		}

		if (nDataVersion >= 2)
		{
			long nTmp;
			dataReader >> nTmp;
			m_eTranslateFieldType = (ETranslateFieldType)nTmp;
		}

		// Separately read in the translation pairs
		IPersistStreamPtr ipObj;
		::readObjectFromStream(ipObj, pStream, "ELI09972");
		if (ipObj == __nullptr)
		{
			throw UCLIDException( "ELI04724", 
				"Translation pairs could not be read from stream!" );
		}

		m_ipTranslationStringPairs = ipObj;

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04722");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateValue::Save(IStream *pStream, BOOL fClearDirty)
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
		dataWriter << (long)m_eTranslateFieldType;
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write( &nDataLength, sizeof(nDataLength), NULL );
		pStream->Write( data.getData(), nDataLength, NULL );

		// Separately write the Translation pairs to the IStream object
		IPersistStreamPtr ipObj( m_ipTranslationStringPairs );
		if (ipObj == __nullptr)
		{
			throw UCLIDException( "ELI04725", 
				"Translation pairs object does not support persistence!" );
		}
		else
		{
			::writeObjectToStream(ipObj, pStream, "ELI09927", fClearDirty);
		}

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04723");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTranslateValue::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CTranslateValue::loadFromFile(const string& strFileName, const char& cDelimiter)
{
	// reset content of map
	m_ipTranslationStringPairs->Clear();

	ifstream ifs(strFileName.c_str());
	CommentedTextFileReader fileReader(ifs, "//", true);
	
	StringTokenizer tokenizer(cDelimiter);
	while (!ifs.eof())
	{
		string strLine("");
		strLine = fileReader.getLineText();

		if (!strLine.empty())
		{
			// tokenize the line text
			vector<string> vecTokens;
			tokenizer.parse(strLine, vecTokens);
			if (vecTokens.size() != 2)
			{
				UCLIDException uclidException("ELI04233", "This line of text can not be parsed successfully with provided delimiter.");
				uclidException.addDebugInfo("LineText", strLine);
				uclidException.addDebugInfo("Delimiter", cDelimiter);
				throw uclidException;
			}

			IStringPairPtr ipStringPair(CLSID_StringPair);
			ipStringPair->StringKey = vecTokens[0].c_str();
			ipStringPair->StringValue = vecTokens[1].c_str();

			m_ipTranslationStringPairs->PushBack(ipStringPair);
		}
	};
}
//-------------------------------------------------------------------------------------------------
void CTranslateValue::validateLicense()
{
	static const unsigned long TRANSLATE_VALUE_RULE_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE( TRANSLATE_VALUE_RULE_COMPONENT_ID, "ELI04205", "Translate Value Rule" );
}
//-------------------------------------------------------------------------------------------------
