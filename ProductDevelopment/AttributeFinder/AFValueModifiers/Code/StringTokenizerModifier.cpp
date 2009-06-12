// StringTokenizer.cpp : Implementation of CStringTokenizer
#include "stdafx.h"
#include "AFValueModifiers.h"
#include "StringTokenizerModifier.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <StringTokenizer.h>
#include <cpputil.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CStringTokenizerModifier
//-------------------------------------------------------------------------------------------------
CStringTokenizerModifier::CStringTokenizerModifier()
: m_bDirty(false),
  m_strDelimiter(""),
  m_strResultExpression(""),
  m_strTextInBetween(""),
  m_eNumOfTokensType(kAnyNumber),
  m_ipSyntexValidator(NULL),
  m_nNumOfTokensRequired(-1)
{
	try
	{
		IMiscUtilsPtr ipMiscUtils(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI13059", ipMiscUtils != NULL );

		m_ipRegExpr = ipMiscUtils->GetNewRegExpParserInstance("StringTokenizerModifier");
		ASSERT_RESOURCE_ALLOCATION("ELI05367", m_ipRegExpr!=NULL);
		// this pattern will search for any %d and %d-%d
		m_ipRegExpr->Pattern = "(\\\\*)%(\\d+)(-%(\\d+))?";
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI05366")
}
//-------------------------------------------------------------------------------------------------
CStringTokenizerModifier::~CStringTokenizerModifier()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16365");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerModifier::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IStringTokenizerModifier,
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
// IStringTokenizerModifier
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerModifier::get_Delimiter(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = get_bstr_t(m_strDelimiter.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05322");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerModifier::put_Delimiter(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		string strDelim = asString( newVal );
		
		if (strDelim.empty() || strDelim.size() > 1)
		{
			throw UCLIDException("ELI05808", "Please specify non-empty single character delimiter.");
		}

		m_strDelimiter = strDelim;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05323");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerModifier::get_ResultExpression(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = _bstr_t(m_strResultExpression.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05324");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerModifier::put_ResultExpression(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		string strExpr = asString( newVal );

		if (strExpr.empty())
		{
			throw UCLIDException("ELI05809", "Please specify non-empty result expression.");
		}

		// validate the expression
		if (!validateExpression(strExpr))
		{
			UCLIDException ue("ELI05813", "Invalid result expression.");
			ue.addDebugInfo("ResultExpression", strExpr);
			throw ue;
		}

		m_strResultExpression = strExpr;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05325");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerModifier::get_NumberOfTokensType(ENumOfTokensType *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_eNumOfTokensType;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05326");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerModifier::put_NumberOfTokensType(ENumOfTokensType newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_eNumOfTokensType = newVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05327");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerModifier::get_NumberOfTokensRequired(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_nNumOfTokensRequired;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05328");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerModifier::put_NumberOfTokensRequired(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (m_eNumOfTokensType > 0 && newVal < 0)
		{
			UCLIDException ue("ELI05810", "Please specify a positive number of tokens.");
			ue.addDebugInfo("Number of tokens", newVal);
			throw ue;
		}

		m_nNumOfTokensRequired = newVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05329");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerModifier::get_TextInBetween(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = _bstr_t(m_strTextInBetween.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05447");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerModifier::put_TextInBetween(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_strTextInBetween = asString( newVal );

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05448");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IAttributeModifyingRule
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerModifier::raw_ModifyValue(IAttribute* pAttribute, IAFDocument* pOriginInput,
													   IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		IAttributePtr	ipAttribute(pAttribute);
		ASSERT_RESOURCE_ALLOCATION( "ELI09292", ipAttribute != NULL );

		ISpatialStringPtr ipInputText = ipAttribute->Value;
		ASSERT_RESOURCE_ALLOCATION( "ELI09293", ipInputText != NULL);
		
		// tokenize the value
		IIUnknownVectorPtr ipTokens = ipInputText->Tokenize(_bstr_t(m_strDelimiter.c_str()));

		// check the number of token requirement
		long nActualNumOfTokens = ipTokens->Size();
		switch (m_eNumOfTokensType)
		{
		case kEqualNumber:
			if (nActualNumOfTokens != m_nNumOfTokensRequired)
			{
				// no change for the original input text
				return S_OK;
			}
			break;
		case kGreaterThanNumber:
			if (nActualNumOfTokens <= m_nNumOfTokensRequired)
			{
				// no change for the original input text
				return S_OK;
			}
			break;
		case kGreaterThanEqualNumber:
			if (nActualNumOfTokens < m_nNumOfTokensRequired)
			{
				// no change for the original input text
				return S_OK;
			}
			break;
		}

		// find all token place holders (i.e. %d or %d-%d) in the result expression
		IIUnknownVectorPtr ipTokenPlaceHolders = 
			m_ipRegExpr->Find(_bstr_t(m_strResultExpression.c_str()), VARIANT_FALSE, VARIANT_TRUE);
		ASSERT_RESOURCE_ALLOCATION("ELI05375", ipTokenPlaceHolders!=NULL);

		long nPlaceHoldersSize  = ipTokenPlaceHolders->Size();
		// if there is no place holder found in the result expression
		if (nPlaceHoldersSize <= 0)
		{
			// set the value equal to the result expression
			if (ipInputText->HasSpatialInfo() == VARIANT_TRUE)
			{
				ipInputText->ReplaceAndDowngradeToHybrid(m_strResultExpression.c_str());
			}
			else
			{
				ipInputText->ReplaceAndDowngradeToNonSpatial(m_strResultExpression.c_str());
			}

			return S_OK;
		}

		// clear the contents of the input
		ipInputText->Clear();

		unsigned long ulCurrentPos=0, ulStart, ulEnd;
		for (long n = 0; n < nPlaceHoldersSize; n++)
		{
			IObjectPairPtr ipMatches = ipTokenPlaceHolders->At(n);
			ITokenPtr ipTopMatch = ipMatches->Object1;
			if (ipTopMatch)
			{
				ulStart = ipTopMatch->StartPosition;
				ulEnd = ipTopMatch->EndPosition;
								
				// now replace any place holders with actual tokens
				IIUnknownVectorPtr ipSubMatches(ipMatches->Object2);
				ASSERT_RESOURCE_ALLOCATION("ELI06636", ipSubMatches != NULL);
				// only interested in submatch 2 and submatch 4
				string strSubMatch2("");
				string strSubMatch4("");
				
				// if the match has an odd number of slash (\) right ahead of it,
				// then this match shall be skipped
				ITokenPtr ipSubMatch1(ipSubMatches->At(0));
				ASSERT_RESOURCE_ALLOCATION("ELI06637", ipSubMatch1 != NULL);
				// check submatch 1 if there are slashes
				// If the number of slashes is an odd number, it means that
				// the slashes are actually the escape characters
				if (ipSubMatch1->Value.length()%2 == 1)
				{
					// append this part of string to the spatial string
					string strPart = m_strResultExpression.substr( 
						ulCurrentPos, ulEnd - ulCurrentPos + 1 );
					ipInputText->AppendString(strPart.c_str());

					ulCurrentPos = ulEnd + 1;
					continue;
				}
				
				// if the found position is not at the current position,
				// the string in between the current position and the found
				// position is the non place holder string, which needs to be
				// extracted and stored in the returning string
				if (ulStart > ulCurrentPos)
				{
					string strPart = m_strResultExpression.substr( 
						ulCurrentPos, ulStart - ulCurrentPos );
					ipInputText->AppendString(strPart.c_str());
				}

				// If there was an even number of slashes are escaped slashes and they need
				// to be added to the string
				long nNumSlashes = ipSubMatch1->Value.length()/2;
				string strSlashes = "";
				int i;
				for (i = 0; i < nNumSlashes; i++)
				{
					strSlashes += '\\';
				}
				if (strSlashes.size() > 0)
				{
					ipInputText->AppendString(get_bstr_t(strSlashes));
				}
				
				// each sub match is stored in a Token object
				ITokenPtr ipSubMatch2(ipSubMatches->At(1));
				ASSERT_RESOURCE_ALLOCATION("ELI06638", ipSubMatch2 != NULL);
				strSubMatch2 = ipSubMatch2->Value;
				
				ITokenPtr ipSubMatch4(ipSubMatches->At(3));
				ASSERT_RESOURCE_ALLOCATION("ELI06639", ipSubMatch4 != NULL);
				strSubMatch4 = ipSubMatch4->Value;
				
				ISpatialStringPtr ipReplacedStr = getReplacement(ipTokens, strSubMatch2, strSubMatch4);
				// only append the string if it's not null
				if (ipReplacedStr)
				{
					ipInputText->Append(ipReplacedStr);
				}
				
				// set current pos right pass the end of the current place holder
				ulCurrentPos = ulEnd + 1;
			}
		}
		
		string strRemainString("");
		if (ulCurrentPos < m_strResultExpression.size())
		{
			// get rest of the string (if any) after last place holder
			strRemainString = m_strResultExpression.substr( ulCurrentPos );

			// append the remaining string to the existing string
			ipInputText->AppendString(strRemainString.c_str());
		}

		// remove any escape sequence char
		_bstr_t _bstrEscapeChars("\\\\(%|-|\\\\)");
		// replace any \\ with \, \% with %, \- with -
		ipInputText->Replace(_bstrEscapeChars, "$1", VARIANT_FALSE, 0, 
			getSyntexValidator());
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05314");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerModifier::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		UCLID_AFVALUEMODIFIERSLib::IStringTokenizerModifierPtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI08287", ipSource != NULL);
	
		m_strDelimiter = asString(ipSource->GetDelimiter());
		m_strResultExpression = asString(ipSource->GetResultExpression());
		m_strTextInBetween = asString(ipSource->GetTextInBetween());

		m_eNumOfTokensType = (ENumOfTokensType)ipSource->GetNumberOfTokensType();

		m_nNumOfTokensRequired = ipSource->GetNumberOfTokensRequired();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08288");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerModifier::raw_Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy(CLSID_StringTokenizerModifier);
		ASSERT_RESOURCE_ALLOCATION("ELI08361", ipObjCopy != NULL);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05316");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerModifier::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19606", pstrComponentDescription != NULL)

		*pstrComponentDescription = _bstr_t("String tokenizer").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05317");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerModifier::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CStringTokenizerModifier::raw_IsConfigured(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		*pbValue = VARIANT_FALSE;

		if (!m_strDelimiter.empty() && !m_strResultExpression.empty())
		{
			if (m_eNumOfTokensType == kAnyNumber
				|| (m_eNumOfTokensType > kAnyNumber && m_nNumOfTokensRequired >= 0))
			{
				*pbValue = VARIANT_TRUE;
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05318")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerModifier::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_StringTokenizerModifier;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerModifier::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerModifier::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();
		
		m_strDelimiter = "";
		m_strResultExpression = "";
		m_strTextInBetween = "";
		m_eNumOfTokensType = kAnyNumber;
		m_nNumOfTokensRequired = -1;

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
			UCLIDException ue( "ELI07661", "Unable to load newer StringTokenizer Modifier." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		if (nDataVersion >= 1)
		{
			dataReader >> m_strDelimiter;
			dataReader >> m_strResultExpression;
			dataReader >> m_strTextInBetween;
			long nNumOfTokensType = 0;
			dataReader >> nNumOfTokensType;
			m_eNumOfTokensType = (ENumOfTokensType)nNumOfTokensType;
			dataReader >> m_nNumOfTokensRequired;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05319");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerModifier::Save(IStream *pStream, BOOL fClearDirty)
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
		dataWriter << m_strDelimiter;
		dataWriter << m_strResultExpression;
		dataWriter << m_strTextInBetween;
		dataWriter << (long)m_eNumOfTokensType;
		dataWriter << m_nNumOfTokensRequired;
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05320");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerModifier::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
ISpatialStringPtr CStringTokenizerModifier::getReplacement(IIUnknownVectorPtr ipTokens,
												const string& strToBeReplacedStart, 
												const string& strToBeReplacedEnd)
{
	ISpatialStringPtr ipReturnString(NULL);

	// convert strToBeReplacedStart and strToBeReplacedEnd into numbers
	// Note: strToBeReplacedStart and strToBeReplacedEnd are 1-based
	// whereas the vecTokens is 0-based
	long nStart = asLong(strToBeReplacedStart) - 1;
	long nEnd = strToBeReplacedEnd.empty() ? nStart : asLong(strToBeReplacedEnd)-1;

	for (long n=nStart; n<=nEnd; n++)
	{
		if (ipReturnString == NULL)
		{
			ipReturnString.CreateInstance(CLSID_SpatialString);
			ASSERT_RESOURCE_ALLOCATION("ELI06632", ipReturnString != NULL);
		}

		if (n < ipTokens->Size())
		{
			if (!m_strTextInBetween.empty() && n > nStart)
			{
				ipReturnString->AppendString(m_strTextInBetween.c_str());
			}

			ISpatialStringPtr ipToken = ipTokens->At(n);
			ipReturnString->Append(ipToken);
		}
	}

	return ipReturnString;
}
//-------------------------------------------------------------------------------------------------
IRegularExprParserPtr CStringTokenizerModifier::getSyntexValidator()
{
	if (m_ipSyntexValidator == NULL)
	{
		IMiscUtilsPtr ipMiscUtils(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI13060", ipMiscUtils != NULL );

		m_ipSyntexValidator = ipMiscUtils->GetNewRegExpParserInstance("StringTokenizerModifier");
		ASSERT_RESOURCE_ALLOCATION("ELI13061", m_ipSyntexValidator != NULL);
	}

	return m_ipSyntexValidator;
}
//-------------------------------------------------------------------------------------------------
void CStringTokenizerModifier::validateLicense()
{
	static const unsigned long STRING_TOKENIZER_MODIFIER_COMPONENT_ID = 
		gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE( STRING_TOKENIZER_MODIFIER_COMPONENT_ID, "ELI05321", 
		"StringTokenizer Modifying Rule" );
}
//-------------------------------------------------------------------------------------------------
bool CStringTokenizerModifier::validateExpression(const string& strExpr)
{
	// set pattern
	getSyntexValidator()->Pattern = "(\\\\*)(%|-|\\\\)";
	IIUnknownVectorPtr ipFound = 
		getSyntexValidator()->Find(_bstr_t(strExpr.c_str()), VARIANT_FALSE, VARIANT_TRUE);
	if (ipFound != NULL)
	{
		long nSize = ipFound->Size();
		for (long n=0; n<nSize; n++)
		{
			IObjectPairPtr ipObjectPair = ipFound->At(n);
			ITokenPtr ipTopMatch = ipObjectPair->Object1;
			long nEndPos = ipTopMatch->EndPosition;
			IIUnknownVectorPtr ipSubMatches = ipObjectPair->Object2;
			if (ipSubMatches != NULL && ipSubMatches->Size() > 0)
			{
				// the token for slashes
				ITokenPtr ipSlashes = ipSubMatches->At(0);
				// the token for percent sign or dash or slash
				ITokenPtr ipPercent = ipSubMatches->At(1);
				// what's the actual found char
				string strPercent = ipPercent->Value;

				// what's the actual found slashes
				string strSlashes = ipSlashes->Value;

				if (strPercent == "%")
				{
					string strAfterPercentSign = strExpr.substr(nEndPos+1);
					if ( strSlashes.length()%2 == 0 && 
						(strAfterPercentSign.empty() || !::isdigit((unsigned char) strAfterPercentSign[0])) )
					{
						return false;
					}
				}
				else if (strPercent == "-" || strPercent == "\\")
				{
					string strAfterPercent = strExpr.substr(nEndPos+1, 1);
					// if there is a percent sign right after the dash (-),
					// then it is valid
					if (strPercent == "-" && strAfterPercent == "%")
					{
						continue;
					}

					if (strSlashes.length()%2 == 0)
					{
						return false;
					}
				}
			}
		}
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
