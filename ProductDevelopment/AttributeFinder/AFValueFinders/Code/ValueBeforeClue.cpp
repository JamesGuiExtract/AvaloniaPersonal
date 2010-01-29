// ValueBeforeClue.cpp : Implementation of CValueBeforeClue
#include "stdafx.h"
#include "AFValueFinders.h"
#include "ValueBeforeClue.h"

#include <UCLIDException.h>
#include <COMUtils.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 3;

//-------------------------------------------------------------------------------------------------
// CValueBeforeClue
//-------------------------------------------------------------------------------------------------
CValueBeforeClue::CValueBeforeClue()
: m_bCaseSensitive(false),
  m_bClueAsRegExpr(false),
  m_eRefiningType(UCLID_AFVALUEFINDERSLib::kNoRefiningType),
  m_nNumOfWords(0),
  m_strPunctuations(" \t\n\r"),  // default to space, tab and newline
  m_bStopAtNewLine(false),
  m_bStopForOther(false),
  m_strStops(""),  // default to nothing
  m_nNumOfLines(0),
  m_bIncludeClueLine(true),
  m_strLimitingString(""),
  m_ipClues(CLSID_VariantVector),
  m_bDirty(false),
  m_bClueToStringAsRegExpr(false)
{
	try
	{
		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI13051", m_ipMiscUtils != NULL );
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI13053")
}
//-------------------------------------------------------------------------------------------------
CValueBeforeClue::~CValueBeforeClue()
{
	try
	{
		m_ipMiscUtils = NULL;
		m_ipClues = NULL;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16351");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueBeforeClue::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IAttributeFindingRule,
		&IID_ICategorizedComponent,
		&IID_IValueBeforeClue,
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
// IAttributeFindingRule
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueBeforeClue::raw_ParseText(IAFDocument* pAFDoc, IProgressStatus *pProgressStatus,
											 IIUnknownVector **pAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		IIUnknownVectorPtr ipAttributes(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI05877", ipAttributes != NULL);

		IAFDocumentPtr ipAFDoc(pAFDoc);
		// Get the text out from the spatial string
		ISpatialStringPtr ipInputText(ipAFDoc->Text);
		// get the input string from the spatial string
		_bstr_t _bstrText(ipInputText->String);

		// put all clues into one string pattern
		_bstr_t _bstrCluesPattern("");
		long nSize = m_ipClues->Size;
		if (nSize <= 0)
		{
			// return an empty vec
			*pAttributes = ipAttributes.Detach();
			return S_OK;
		}

		long n;
		for (n = 0; n < nSize; n++)
		{
			if (n > 0)
			{
				_bstrCluesPattern += "|";
			}

			string strClue = asString(_bstr_t(m_ipClues->GetItem(n)));
			// reform special chars if all clues text are not
			// used as regular expressions
			if (!m_bClueAsRegExpr)
			{
				convertStringToRegularExpression(strClue);
			}

			_bstrCluesPattern += _bstr_t(strClue.c_str());
		}

		// Get the regex parser
		IRegularExprParserPtr ipParser = m_ipMiscUtils->GetNewRegExpParserInstance("ValueBeforeClue");
		ASSERT_RESOURCE_ALLOCATION("ELI13052", ipParser != NULL);

		// find all clues in the input string
		ipParser->IgnoreCase = asVariantBool(!m_bCaseSensitive);
		ipParser->Pattern = _bstrCluesPattern;
		IIUnknownVectorPtr ipVecFoundClues(ipParser->Find(_bstrText, VARIANT_FALSE, VARIANT_FALSE));

		long nNumOfCluesFound = ipVecFoundClues->Size();
		for (n = 0; n < nNumOfCluesFound; n++)
		{
			IObjectPairPtr ipObjPair = ipVecFoundClues->At(n);
			// Token is the first object in the object pair
			ITokenPtr ipToken = ipObjPair->Object1;
			if (ipToken)
			{
				long nStart = ipToken->StartPosition;
				if (nStart < 1)
				{
					continue;
				}

				// get everything before a found clue text
				ISpatialStringPtr ipValue = ipInputText->GetSubString(0, nStart-1);
				// go to next found value if this value is empty
				if (ipValue->IsEmpty() == VARIANT_TRUE)
				{
					continue;
				}

				// get the string value out from the found string
				_bstr_t _bstrStrValue = ipValue->String;
				switch (m_eRefiningType)
				{
				case kClueLine:
					{
						// get everything before the clue text 
						// on the same line as the clue text
						// look for new line chars in the substr
						ipParser->Pattern = _bstr_t("[\n\r]+");
						IIUnknownVectorPtr ipNewLines = ipParser->Find(_bstrStrValue,
							VARIANT_FALSE, VARIANT_FALSE);
						long nNumOfNewLines = ipNewLines->Size();
						// if there's no line break, simply take the strValue;
						// if there's at least one line break, do the following:
						if (nNumOfNewLines>=1) 
						{
							// get the last new line position
							IObjectPairPtr ipObjPair = ipNewLines->At(nNumOfNewLines-1);
							// Token is the first object in the object pair
							ITokenPtr ipNewLine = ipObjPair->Object1;
							if (ipNewLine)
							{
								long nEndNewLinePos = ipNewLine->EndPosition;
								if (ipValue->Size-1 < nEndNewLinePos+1)
								{
									ipValue->Clear();
								}
								else
								{
									// get everything in between the last new line char to the clue text
									ipValue = ipValue->GetSubString(nEndNewLinePos+1, ipValue->Size-1);
								}
							}
						}
					}
					break;
				case kUptoXWords:
					{
						// look for all words separated by punctuations specified
						string strWordPattern = "[^" + m_strPunctuations + "]+";
						ipParser->Pattern = _bstr_t(strWordPattern.c_str());
						
						// get all words before clue text
						IIUnknownVectorPtr ipAllWordsInfo(ipParser->Find(_bstrStrValue, 
							VARIANT_FALSE, VARIANT_FALSE));
						if (ipAllWordsInfo)
						{
							long nNumOfWords = ipAllWordsInfo->Size();
							if (nNumOfWords==0) continue;

							// start from the clue text going left till the limit reaches
							long nFirstWordIndex = 0;
							if (nNumOfWords > m_nNumOfWords)
							{
								// if we found more words than limited by user
								nFirstWordIndex = nNumOfWords - m_nNumOfWords;
							}

							long nStartWordPos, nEndWordPos;
							// get the first word position
							IObjectPairPtr ipObjPair = ipAllWordsInfo->At(nFirstWordIndex);
							// Token is the first object in the object pair
							ITokenPtr ipWordInfo = ipObjPair->Object1;
							if (ipWordInfo)
							{
								nStartWordPos = ipWordInfo->StartPosition;
								nEndWordPos = ipWordInfo->EndPosition;
								
								// if user wants to get words before the first
								// Stop char is encountered, even though the
								// word count might not be the maximum
								if (m_bStopAtNewLine || m_bStopForOther)
								{
									// Start Pattern with Stop characters
									string strStopPattern = "[" + m_strStops;
									if (m_bStopAtNewLine)
									{
										// Add carriage-return and line-feed characters
										strStopPattern = strStopPattern + "\n\r";
									}
									// Finish and apply the Pattern
									strStopPattern = strStopPattern + "]+";
									ipParser->Pattern = _bstr_t( strStopPattern.c_str() );

									IIUnknownVectorPtr ipStopChars 
										= ipParser->Find(_bstrStrValue, VARIANT_FALSE, 
										VARIANT_FALSE);
									long nNumOfStops = ipStopChars->Size();
									if (nNumOfStops >= 1)
									{
										// get the last Stop position
										IObjectPairPtr ipObjPair = ipStopChars->At(nNumOfStops-1);
										// Token is the first object in the object pair
										ITokenPtr ipStop = ipObjPair->Object1;
										if (ipStop)
										{
											long nStartStopPos = ipStop->StartPosition;
											long nEndStopPos = ipStop->EndPosition;
											// if Stop char is after the first word
											if (nEndWordPos < nStartStopPos)
											{
												nStartWordPos = nEndStopPos + 1;
											}
										}
									}
								}
								
								if (ipValue->Size-1 < nStartWordPos)
								{
									ipValue->Clear();
								}
								else
								{
									ipValue = ipValue->GetSubString(nStartWordPos, ipValue->Size-1);
									// trim off any leading/trailing word seperator(s), such as space, etc.
									ipValue->Trim(_bstr_t(m_strPunctuations.c_str()), _bstr_t(m_strPunctuations.c_str()));
								}
							}
						}
					}
					break;
				case kUptoXLines:
					{
						// find all new line chars
						ipParser->Pattern = _bstr_t("[\n\r]+");
						IIUnknownVectorPtr ipNewLines = ipParser->Find(_bstrStrValue, 
							VARIANT_FALSE, VARIANT_FALSE);
						long nNumOfNewLines = ipNewLines->Size();
						// if there's only clue line
						if (nNumOfNewLines == 0)
						{
							// if clue line is included, then
							// the strValue is good enough
							if (m_bIncludeClueLine) break;
							// and clue line text is not included, 
							// then there's no value to return
							else continue;
						}

						// end position of the substring we want to extract
						long nEndPos = ipValue->Size-1;
						// get the last new line position
						IObjectPairPtr ipObjPair = ipNewLines->At(nNumOfNewLines-1);
						// Token is the first object in the object pair
						ITokenPtr ipNewLine = ipObjPair->Object1;
						if (ipNewLine)
						{
							long nStartLastNewLinePos = ipNewLine->StartPosition;
							if (!m_bIncludeClueLine)
							{
								// if clue line is not included, end pos
								// will be the first char before the last new line char(s)
								nEndPos = nStartLastNewLinePos - 1;
							}
						}

						long nIndex = 0;
						if (m_bIncludeClueLine)
						{
							if (nNumOfNewLines+1 <= m_nNumOfLines) break;

							nIndex = nNumOfNewLines - m_nNumOfLines;
						}
						else
						{
							if (nNumOfNewLines <= m_nNumOfLines)
							{
								if (nEndPos < 0)
								{
									ipValue->Clear();
								}
								else
								{
									ipValue = ipValue->GetSubString(0, nEndPos);
								}
								break;
							}

							nIndex = nNumOfNewLines - 1 - m_nNumOfLines;;
						}
						
						ipObjPair = ipNewLines->At(nIndex);
						ipNewLine = ipObjPair->Object1;
						if (ipNewLine)
						{
							long nEndMaxNewLinePos = ipNewLine->EndPosition;
							if (nEndPos < nEndMaxNewLinePos+1)
							{
								ipValue->Clear();
							}
							else
							{
								ipValue = ipValue->GetSubString(nEndMaxNewLinePos+1, nEndPos);
							}
						}
					}
					break;
				case kClueToString:
					{
						// Local pattern string
						string	strLimit = m_strLimitingString;

						if (!m_bClueToStringAsRegExpr)
						{
							// Convert regular expression-style text to characters
							convertStringToRegularExpression( strLimit );
						}

						// the pattern string to be searched in the strValue
						_bstr_t _bstrLimitPattern( strLimit.c_str() );
						
						// Check for limiting strings
						ipParser->Pattern = _bstrLimitPattern;
						IIUnknownVectorPtr ipVecFoundStrings(ipParser->Find(_bstrStrValue, 
							VARIANT_FALSE, VARIANT_FALSE));
						
						long nFoundSize = ipVecFoundStrings->Size();
						if (nFoundSize == 0)
						{
							continue;
						}
						
						// get last found match
						IObjectPairPtr ipObjPair = ipVecFoundStrings->At(nFoundSize-1);
						// Token is the first object in the object pair
						ITokenPtr ipFound = ipObjPair->Object1;
						if (ipFound)
						{
							// get the last occurrence info
							long nEnd = ipFound->EndPosition;
							if (ipValue->Size-1 < nEnd+1)
							{
								ipValue->Clear();
							}
							else
							{
								ipValue = ipValue->GetSubString(nEnd+1, ipValue->Size-1);
							}
						}
					}
					break;
				}

				if (ipValue->IsEmpty() == VARIANT_TRUE) 
				{
					continue;
				}

				// create an attribute to store the value
				IAttributePtr ipAttribute(CLSID_Attribute);
				ipAttribute->Value = ipValue;
				ipAttributes->PushBack(ipAttribute);
			}
		}

		*pAttributes = ipAttributes.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04177");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueBeforeClue::raw_IsConfigured(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		*pbValue = m_ipClues->Size > 0 ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04859");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueBeforeClue::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19586", pstrComponentDescription != NULL)

		*pstrComponentDescription = _bstr_t("Value before clue rule").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04184")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueBeforeClue::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueBeforeClue::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		UCLID_AFVALUEFINDERSLib::IValueBeforeCluePtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI08265", ipSource != NULL);

		m_bCaseSensitive = (ipSource->GetIsCaseSensitive()==VARIANT_TRUE) ? true : false;
		m_bClueAsRegExpr = (ipSource->GetClueAsRegExpr()==VARIANT_TRUE) ? true : false;
		m_bClueToStringAsRegExpr = (ipSource->GetClueToStringAsRegExpr()==VARIANT_TRUE) ? true : false;
		
		ICopyableObjectPtr ipCopyObj = ipSource->GetClues();
		ASSERT_RESOURCE_ALLOCATION("ELI08266", ipCopyObj != NULL);
		m_ipClues = ipCopyObj->Clone();

		m_eRefiningType = ipSource->GetRefiningType();

		CComBSTR bstrPunc, bstrStopChars;
		VARIANT_BOOL bStopAtNewLine;
		long nNumOfWords;
		ipSource->GetUptoXWords(&nNumOfWords, &bstrPunc, &bStopAtNewLine, &bstrStopChars);
		m_nNumOfWords = nNumOfWords;
		m_strPunctuations = asString(bstrPunc);
		m_strStops = asString(bstrStopChars);
		m_bStopAtNewLine = (bStopAtNewLine==VARIANT_TRUE) ? true : false;


		long nNumOfLines;
		VARIANT_BOOL bIncludeClueLine;
		ipSource->GetUptoXLines(&nNumOfLines, &bIncludeClueLine);
		m_nNumOfLines = nNumOfLines;
		m_bIncludeClueLine = (bIncludeClueLine==VARIANT_TRUE) ? true : false;

		m_strLimitingString = asString(ipSource->GetClueToString());
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08267");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueBeforeClue::raw_Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy(CLSID_ValueBeforeClue);
		ASSERT_RESOURCE_ALLOCATION("ELI08351", ipObjCopy != NULL);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04492");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
// IValueAfterClue
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueBeforeClue::get_ClueAsRegExpr(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_bClueAsRegExpr ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04898")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueBeforeClue::put_ClueAsRegExpr(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bClueAsRegExpr = newVal == VARIANT_TRUE;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04899")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueBeforeClue::get_IsCaseSensitive(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_bCaseSensitive ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04537")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueBeforeClue::put_IsCaseSensitive(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bCaseSensitive = newVal == VARIANT_TRUE;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04538")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueBeforeClue::get_Clues(IVariantVector **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		IVariantVectorPtr ipShallowCopy = m_ipClues;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04541")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueBeforeClue::put_Clues(IVariantVector *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (m_ipClues) m_ipClues = NULL;

		m_ipClues = newVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04542")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueBeforeClue::get_RefiningType(ERuleRefiningType *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = (ERuleRefiningType)m_eRefiningType;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04503")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueBeforeClue::SetNoRefiningType()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_eRefiningType = UCLID_AFVALUEFINDERSLib::kNoRefiningType;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04506")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueBeforeClue::SetClueLineType()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_eRefiningType = UCLID_AFVALUEFINDERSLib::kClueLine;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04507")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueBeforeClue::GetUptoXWords(long *nNumOfWords, 
											BSTR *pstrPunctuations,
											VARIANT_BOOL *bStopAtNewLine, 
											BSTR *pstrStops)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Provide settings
		*nNumOfWords = m_nNumOfWords;
		*pstrPunctuations = _bstr_t(m_strPunctuations.c_str()).copy();
		*bStopAtNewLine = m_bStopAtNewLine ? VARIANT_TRUE : VARIANT_FALSE;
		*pstrStops = _bstr_t( m_strStops.c_str() ).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04508")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueBeforeClue::SetUptoXWords(long nNumOfWords, 
											BSTR strPunctuations,
											VARIANT_BOOL bStopAtNewLine, 
											BSTR strStops)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Store settings
		m_eRefiningType = UCLID_AFVALUEFINDERSLib::kUptoXWords;
		m_nNumOfWords = nNumOfWords;
		m_strPunctuations = asString( strPunctuations );
		m_bStopAtNewLine = (bStopAtNewLine == VARIANT_TRUE);
		m_strStops = asString( strStops );
		m_bStopForOther = (m_strStops.length() > 0) ? true : false;

		// Set Dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04509")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueBeforeClue::GetUptoXLines(long *nNumOfLines, VARIANT_BOOL *bIncludeClueLine)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*nNumOfLines = m_nNumOfLines;
		*bIncludeClueLine = m_bIncludeClueLine?VARIANT_TRUE:VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04510")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueBeforeClue::SetUptoXLines(long nNumOfLines, VARIANT_BOOL bIncludeClueLine)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_eRefiningType = UCLID_AFVALUEFINDERSLib::kUptoXLines;
		m_nNumOfLines = nNumOfLines;
		m_bIncludeClueLine = bIncludeClueLine==VARIANT_TRUE;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04511")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueBeforeClue::GetClueToString(BSTR *strString)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*strString = _bstr_t(m_strLimitingString.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04512")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueBeforeClue::SetClueToString(BSTR strString)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_eRefiningType = UCLID_AFVALUEFINDERSLib::kClueToString;
		m_strLimitingString = asString( strString );

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04513")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueBeforeClue::get_ClueToStringAsRegExpr(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		*pVal = m_bClueToStringAsRegExpr ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05999")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueBeforeClue::put_ClueToStringAsRegExpr(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		m_bClueToStringAsRegExpr = newVal == VARIANT_TRUE;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06644")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueBeforeClue::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_ValueBeforeClue;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueBeforeClue::IsDirty(void)
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
			IPersistStreamPtr ipPersistStream(m_ipClues);
			if (ipPersistStream==NULL)
			{
				throw UCLIDException("ELI04793", "Object does not support persistence!");
			}
			
			hr = ipPersistStream->IsDirty();
		}

		return hr;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04794");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
// NOTES about versions:
// Version 1:
//   * Saved: 
//            data version,
//            case-sensitivity flag,
//            clue is regular expression flag,
//            refining type enumeration,
//            number of words,
//            string of punctuation characters
//            stop at new line flag,
//            number of lines,
//            include clue line flag,
//            limiting string,
//            limiting regular expression
// Version 2:
//   * Additionally saved:
//            stop for other characters flag
//            string of other stop characters
//   * NOTE:
//            two new items located immediately after Stop At New Line flag
// Version 3:
//   * Additionally saved:
//            clue to string is regular expression flag,
//   * Removed:
//            limiting regular expression
//   * NOTE:
//            new item located immediately after limiting string
STDMETHODIMP CValueBeforeClue::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Clear the variables first
		m_bCaseSensitive = false;
		m_bClueAsRegExpr = false;
		m_eRefiningType = UCLID_AFVALUEFINDERSLib::kNoRefiningType;
		m_nNumOfWords = 0;
		m_strPunctuations = "";
		m_bStopAtNewLine = false;;
		m_nNumOfLines = 0;
		m_bIncludeClueLine = false;
		m_strLimitingString = "";
//		m_strLimitingRegExpr = "";
		m_bClueToStringAsRegExpr = false;
		m_ipClues = NULL;

		// Local variable to replace m_strLimitingRegExpr removed for Version 3
		string strLimitingRegExpr( "" );

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
			UCLIDException ue( "ELI07649", "Unable to load newer ValueBeforeClue Finder." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		if (nDataVersion >= 1)
		{
			dataReader >> m_bCaseSensitive;
			dataReader >> m_bClueAsRegExpr;
			long lTemp = (long)UCLID_AFVALUEFINDERSLib::kNoRefiningType;
			dataReader >> lTemp;
			m_eRefiningType = (UCLID_AFVALUEFINDERSLib::ERuleRefiningType)lTemp;
			dataReader >> m_nNumOfWords;
			dataReader >> m_strPunctuations;
			dataReader >> m_bStopAtNewLine;

			// Check for other Stop characters if Version >= 2
			if (nDataVersion >= 2)
			{
				dataReader >> m_bStopForOther;
				dataReader >> m_strStops;
			}

			dataReader >> m_nNumOfLines;
			dataReader >> m_bIncludeClueLine;
			dataReader >> m_strLimitingString;

			// Check for Limiting Regular Expression if Version < 3
			if (nDataVersion < 3)
			{
				dataReader >> strLimitingRegExpr;

				// Store this in Limiting String and set new Reg Exp flag
				if (strLimitingRegExpr.length() > 0)
				{
					m_strLimitingString = strLimitingRegExpr;
					m_bClueToStringAsRegExpr = true;
				}
			}
			// Check for Clue To String is Regular Expression if Version >= 3
			else
			{
				dataReader >> m_bClueToStringAsRegExpr;
			}
		}

		// Read the clue list
		IPersistStreamPtr ipObj;
		::readObjectFromStream( ipObj, pStream, "ELI09968" );
		if (ipObj == NULL)
		{
			throw UCLIDException( "ELI04703", 
				"Clues collection could not be read from stream!" );
		}
		else
		{
			m_ipClues = ipObj;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04676");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
// Version == 2
//   Added m_bStopForOther
//   Added m_strStops
// Version == 3
//   Added m_bClueToStringAsRegExpr
//   Removed m_strLimitingRegExpr
STDMETHODIMP CValueBeforeClue::Save(IStream *pStream, BOOL fClearDirty)
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
		dataWriter << m_bClueAsRegExpr;
		dataWriter << (long)m_eRefiningType;
		dataWriter << m_nNumOfWords;
		dataWriter << m_strPunctuations;
		dataWriter << m_bStopAtNewLine;

		dataWriter << m_bStopForOther;
		dataWriter << m_strStops;

		dataWriter << m_nNumOfLines;
		dataWriter << m_bIncludeClueLine;
		dataWriter << m_strLimitingString;
//		dataWriter << m_strLimitingRegExpr;

		dataWriter << m_bClueToStringAsRegExpr;

		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write( &nDataLength, sizeof(nDataLength), NULL );
		pStream->Write( data.getData(), nDataLength, NULL );

		// Separately write the clue list to the IStream object
		IPersistStreamPtr ipObj( m_ipClues );
		if (ipObj == NULL)
		{
			throw UCLIDException( "ELI04704", 
				"Clues collection does not support persistence!" );
		}
		else
		{
			::writeObjectToStream( ipObj, pStream, "ELI09923", fClearDirty );
		}

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04677");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueBeforeClue::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CValueBeforeClue::validateLicense()
{
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI04189", "Value Before Clue Rule" );
}
//-------------------------------------------------------------------------------------------------
