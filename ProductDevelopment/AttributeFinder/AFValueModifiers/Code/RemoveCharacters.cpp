// RemoveCharacters.cpp : Implementation of CRemoveCharacters
#include "stdafx.h"
#include "AFValueModifiers.h"
#include "RemoveCharacters.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <COMUtils.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CRemoveCharacters
//-------------------------------------------------------------------------------------------------
CRemoveCharacters::CRemoveCharacters()
: m_bCaseSensitive(false),
  m_strCharactersDefined(""),
  m_bRemoveAll(true),
  m_bConsolidate(true),
  m_bTrimLeading(true),
  m_bTrimTrailing(true),
  m_bDirty(false)
{
	try
	{
		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI13056", m_ipMiscUtils != __nullptr );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06656")
}
//-------------------------------------------------------------------------------------------------
CRemoveCharacters::~CRemoveCharacters()
{
	try
	{
		m_ipMiscUtils = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16363");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveCharacters::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IRemoveCharacters,
		&IID_IAttributeModifyingRule,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_ILicensedComponent,
		&IID_IMustBeConfiguredObject,
		&IID_IOutputHandler
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
STDMETHODIMP CRemoveCharacters::raw_ModifyValue(IAttribute* pAttribute, IAFDocument* pOriginInput,
												IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		IAttributePtr	ipAttribute(pAttribute);
		ASSERT_RESOURCE_ALLOCATION( "ELI09289", ipAttribute != __nullptr );

		ISpatialStringPtr ipInputText = ipAttribute->GetValue();
		ASSERT_RESOURCE_ALLOCATION( "ELI09290", ipInputText != __nullptr);
		_bstr_t _bstrText(ipInputText->String);
		
		string strCharacters = translateToRegExString(m_strCharactersDefined);
		string strPattern = "[" + strCharacters + "]+";

		// Get a regular expression parser
		IRegularExprParserPtr ipParser =
			m_ipMiscUtils->GetNewRegExpParserInstance("RemoveCharacters");
		ASSERT_RESOURCE_ALLOCATION("ELI06655", ipParser != __nullptr);

		if (m_bRemoveAll)
		{
			// remove all characters defined
			IIUnknownVectorPtr ipVecInfo = findPatternInString(ipParser, _bstrText, 
				_bstr_t(strPattern.c_str()), true);
			while (ipVecInfo != __nullptr && ipVecInfo->Size()> 0)
			{
				// the first match
				IObjectPairPtr ipObjPair = ipVecInfo->At(0);
				ASSERT_RESOURCE_ALLOCATION("ELI06622", ipObjPair != __nullptr);
				ITokenPtr ipToken = ipObjPair->Object1;
				ASSERT_RESOURCE_ALLOCATION("ELI06621", ipToken != __nullptr);
				
				long nStart = ipToken->StartPosition;
				long nEnd = ipToken->EndPosition;
				// remove each string
				ipInputText->Remove(nStart, nEnd);

				_bstrText = ipInputText->String;
				ipVecInfo = findPatternInString(ipParser, _bstrText, 
				_bstr_t(strPattern.c_str()), true);
			}
		}
		else
		{
			if (m_bConsolidate)
			{	
				string strSlashRN("\r\n");
				int nSlashRNPos = m_strCharactersDefined.find(strSlashRN);
				// if no \r\n defined
				if (nSlashRNPos == string::npos)
				{
					ipInputText->ConsolidateChars(_bstr_t(m_strCharactersDefined.c_str()), 
						m_bCaseSensitive ? VARIANT_TRUE : VARIANT_FALSE);
				}
				else
				{
					// Consolidate the \r\n first
					ipInputText->ConsolidateString(_bstr_t(strSlashRN.c_str()),
							m_bCaseSensitive ? VARIANT_TRUE : VARIANT_FALSE);
					// consolidate the remaining chars
					if (m_strCharactersDefined.size() > 2)
					{
						string strRemainingChars(m_strCharactersDefined);
						strRemainingChars.erase(nSlashRNPos, 2);
						if (!strRemainingChars.empty())
						{
							ipInputText->ConsolidateChars(_bstr_t(strRemainingChars.c_str()), 
								m_bCaseSensitive ? VARIANT_TRUE : VARIANT_FALSE);
						}
					}
				}
			}
			
			if (m_bTrimLeading) 
			{
				// update current text
				_bstrText = ipInputText->String;
				// look for all character defined in the input string
				IIUnknownVectorPtr ipVecInfo 
					= findPatternInString(ipParser, _bstrText, _bstr_t(strPattern.c_str()));
				if (ipVecInfo->Size()>0)
				{
					// let's get the very first occurrence of 
					// the character set defined if it's trimming leading
					IObjectPairPtr ipObjPair = ipVecInfo->At(0);
					ASSERT_RESOURCE_ALLOCATION("ELI06623", ipObjPair != __nullptr);
					ITokenPtr ipToken = ipObjPair->Object1;
					ASSERT_RESOURCE_ALLOCATION("ELI06624", ipToken != __nullptr);
					long nStart = ipToken->StartPosition;
					if (nStart == 0)
					{
						long nEnd = ipToken->EndPosition;
						ipInputText->Remove(nStart, nEnd);
					}
				}
			}
			
			if (m_bTrimTrailing)
			{
				// update current text
				_bstrText = ipInputText->String;
				IIUnknownVectorPtr ipVecInfo 
					= findPatternInString(ipParser, _bstrText, _bstr_t(strPattern.c_str()));
				if (ipVecInfo->Size()>0)
				{
					// let's get the very last occurrence of 
					// the character set defined if it's trimming trailing
					long nIndex = ipVecInfo->Size()-1;
					IObjectPairPtr ipObjPair = ipVecInfo->At(nIndex);
					ASSERT_RESOURCE_ALLOCATION("ELI06625", ipObjPair != __nullptr);
					ITokenPtr ipToken = ipObjPair->Object1;
					ASSERT_RESOURCE_ALLOCATION("ELI06626", ipToken != __nullptr);
					long nEnd = ipToken->EndPosition;
					if (nEnd == _bstrText.length()-1)
					{
						long nStart = ipToken->StartPosition;
						ipInputText->Remove(nStart, nEnd);
					}
				}
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04206");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveCharacters::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		UCLID_AFVALUEMODIFIERSLib::IRemoveCharactersPtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI08283", ipSource != __nullptr);

		m_bCaseSensitive = (ipSource->GetIsCaseSensitive()==VARIANT_TRUE) ? true : false;
		m_bRemoveAll = (ipSource->GetRemoveAll()==VARIANT_TRUE) ? true : false;
		m_bConsolidate = (ipSource->GetConsolidate()==VARIANT_TRUE) ? true : false;
		m_bTrimLeading = (ipSource->GetTrimLeading()==VARIANT_TRUE) ? true : false;
		m_bTrimTrailing = (ipSource->GetTrimTrailing()==VARIANT_TRUE) ? true : false;
		m_strCharactersDefined = ipSource->GetCharacters();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08284");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveCharacters::raw_Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy(CLSID_RemoveCharacters);
		ASSERT_RESOURCE_ALLOCATION("ELI08359", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04465");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveCharacters::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19604", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Remove characters").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04207");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IRemoveCharacters
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveCharacters::get_IsCaseSensitive(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_bCaseSensitive ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04341");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveCharacters::put_IsCaseSensitive(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bCaseSensitive = newVal==VARIANT_TRUE;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04342");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveCharacters::get_Characters(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = _bstr_t(m_strCharactersDefined.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04343");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveCharacters::put_Characters(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_strCharactersDefined = asString( newVal );

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04344");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveCharacters::get_RemoveAll(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_bRemoveAll ? VARIANT_TRUE: VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04922");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveCharacters::put_RemoveAll(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bRemoveAll = newVal==VARIANT_TRUE;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04923");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveCharacters::get_Consolidate(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_bConsolidate ? VARIANT_TRUE: VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04924");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveCharacters::put_Consolidate(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bConsolidate = newVal==VARIANT_TRUE;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04925");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveCharacters::get_TrimLeading(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_bTrimLeading ? VARIANT_TRUE: VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04926");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveCharacters::put_TrimLeading(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bTrimLeading = newVal==VARIANT_TRUE;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04927");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveCharacters::get_TrimTrailing(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_bTrimTrailing ? VARIANT_TRUE: VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04928");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveCharacters::put_TrimTrailing(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bTrimTrailing = newVal==VARIANT_TRUE;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04929");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveCharacters::raw_IsConfigured(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		*pbValue = m_strCharactersDefined.empty() ? VARIANT_FALSE : VARIANT_TRUE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04847")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveCharacters::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// IOutputHandler
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveCharacters::raw_ProcessOutput(IIUnknownVector * pAttributes, IAFDocument * pDoc, 
												  IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Create AFUtility object
		IAFUtilityPtr ipAFUtility( CLSID_AFUtility );
		ASSERT_RESOURCE_ALLOCATION( "ELI08716", ipAFUtility != __nullptr );

		// Use Attributes as smart pointer
		IIUnknownVectorPtr ipAttributes( pAttributes );
		ASSERT_RESOURCE_ALLOCATION("ELI08717", ipAttributes != __nullptr);

		// Apply Attribute Modification
		ipAFUtility->ApplyAttributeModifier( ipAttributes, pDoc, this, VARIANT_TRUE );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08719");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveCharacters::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_RemoveCharacters;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveCharacters::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveCharacters::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Clear the variables first
		m_bCaseSensitive = false;
		m_strCharactersDefined = "";
		m_bRemoveAll = true;
		m_bConsolidate = true;
		m_bTrimLeading = true;
		m_bTrimTrailing = true;

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
			UCLIDException ue( "ELI07659", "Unable to load newer RemoveCharacters Modifier." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		if (nDataVersion >= 1)
		{
			dataReader >> m_bCaseSensitive;
			dataReader >> m_strCharactersDefined;
			dataReader >> m_bRemoveAll;
			dataReader >> m_bConsolidate;
			dataReader >> m_bTrimLeading;
			dataReader >> m_bTrimTrailing;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04711");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveCharacters::Save(IStream *pStream, BOOL fClearDirty)
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
		dataWriter << m_strCharactersDefined;
		dataWriter << m_bRemoveAll;
		dataWriter << m_bConsolidate;
		dataWriter << m_bTrimLeading;
		dataWriter << m_bTrimTrailing;
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
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04712");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveCharacters::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CRemoveCharacters::findPatternInString(IRegularExprParserPtr ipParser,
														  _bstr_t _bstrInput, 
														  _bstr_t _bstrPattern,
														  bool bFindFirstMatchOnly)
{
	IIUnknownVectorPtr ipFoundStringInfos(NULL);

	ipParser->Pattern = _bstrPattern;
	ipParser->IgnoreCase = asVariantBool(!m_bCaseSensitive);
	ipFoundStringInfos = ipParser->Find(_bstrInput, asVariantBool(bFindFirstMatchOnly), 
		VARIANT_FALSE);

	return ipFoundStringInfos;
}
//-------------------------------------------------------------------------------------------------
string CRemoveCharacters::translateToRegExString(const string& strInput)
{
	// exclude \t, \r, \n
	string strTemp(strInput);
	int nFoundPos = strTemp.find("\t");
	bool bFoundTab = false, bFoundLineBreak = false;
	if (nFoundPos != string::npos)
	{
		strTemp.erase(nFoundPos, 1);
		bFoundTab = true;
	}

	nFoundPos = strTemp.find("\r\n");
	if (nFoundPos != string::npos)
	{
		strTemp.erase(nFoundPos, 2);
		bFoundLineBreak = true;
	}

	::convertStringToRegularExpression(strTemp);

	// add tab and line break back if any
	if (bFoundTab)
	{
		strTemp = "\t" + strTemp;
	}
	if (bFoundLineBreak)
	{
		strTemp = "\r\n" + strTemp;
	}

	return strTemp;
}
//-------------------------------------------------------------------------------------------------
void CRemoveCharacters::validateLicense()
{
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI04208", "Remove Characters" );
}
//-------------------------------------------------------------------------------------------------
