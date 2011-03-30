// ValueAfterClue.cpp : Implementation of CValueAfterClue
#include "stdafx.h"
#include "AFValueFinders.h"
#include "ValueAfterClue.h"

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
// CValueAfterClue
//-------------------------------------------------------------------------------------------------
CValueAfterClue::CValueAfterClue()
: m_bCaseSensitive(false),
  m_bClueAsRegExpr(false),
  m_bClueToStringAsRegExpr(false),
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
  m_bDirty(false)
{
	try
	{
		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI13047", m_ipMiscUtils != __nullptr );
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI13049")
}
//-------------------------------------------------------------------------------------------------
CValueAfterClue::~CValueAfterClue()
{
	try
	{
		m_ipMiscUtils = __nullptr;
		m_ipClues = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16350");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueAfterClue::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IAttributeFindingRule,
		&IID_ICategorizedComponent,
		&IID_IValueAfterClue,
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
STDMETHODIMP CValueAfterClue::raw_ParseText(IAFDocument* pAFDoc, IProgressStatus *pProgressStatus,
											IIUnknownVector **pAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		IIUnknownVectorPtr ipAttributes(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI05876", ipAttributes != __nullptr);

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

		IRegularExprParserPtr ipParser = m_ipMiscUtils->GetNewRegExpParserInstance("ValueAfterClue");
		ASSERT_RESOURCE_ALLOCATION("ELI13048", ipParser != __nullptr);

		// find all clues in the input string
		ipParser->IgnoreCase = asVariantBool(!m_bCaseSensitive);
		ipParser->Pattern = _bstrCluesPattern;
		IIUnknownVectorPtr ipVecFoundClues(ipParser->Find(_bstrText, VARIANT_FALSE, 
			VARIANT_FALSE));

		long nNumOfCluesFound = ipVecFoundClues->Size();
		for (n = 0; n < nNumOfCluesFound; n++)
		{
			// each item in the ipVecFoundClues is of type IObjectPair
			IObjectPairPtr ipObjPair = ipVecFoundClues->At(n);
			// Token is the first object in the object pair
			ITokenPtr ipToken = ipObjPair->Object1;
			if (ipToken)
			{
				// get the end position of the found clue
				long nEnd = ipToken->EndPosition;
				long nInputTextSize = ipInputText->Size;
				if (nInputTextSize-1<nEnd+1)
				{
					continue;
				}

				// get everything after the found clue text
				ISpatialStringPtr ipValue = ipInputText->GetSubString(nEnd+1, nInputTextSize-1);
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
						// get everything after the clue text 
						// on the same line as the clue text
						// look for new line chars in the substr
						ipParser->Pattern = _bstr_t("[\n\r]+");
						IIUnknownVectorPtr ipNewLines = ipParser->Find(_bstrStrValue, 
							VARIANT_FALSE, VARIANT_FALSE);
						long nNumOfNewLines = ipNewLines->Size();
						if (nNumOfNewLines>=1)
						{
							// get the first new line position
							IObjectPairPtr ipObjPair = ipNewLines->At(0);
							// Token is the first object in the object pair
							ITokenPtr ipNewLine = ipObjPair->Object1;
							if (ipNewLine)
							{
								long nStartNewLinePos = ipNewLine->StartPosition;
								if (nStartNewLinePos < 1)
								{
									ipValue->Clear();
								}
								else
								{
									// get everything in between the clue text to the first newline char
									ipValue = ipValue->GetSubString(0, nStartNewLinePos-1);
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

						// get all words after clue text
						IIUnknownVectorPtr ipAllWordsInfo 
							= ipParser->Find(_bstrStrValue, VARIANT_FALSE, VARIANT_FALSE);
						if (ipAllWordsInfo)
						{
							long nNumOfWords = ipAllWordsInfo->Size();
							if (nNumOfWords==0) continue;

							if (nNumOfWords > m_nNumOfWords)
							{
								// if we found more words than limited by user
								nNumOfWords = m_nNumOfWords;
							}

							// get the last word position
							IObjectPairPtr ipObjPair = ipAllWordsInfo->At(nNumOfWords-1);
							// Token is the first object in the object pair
							ITokenPtr ipWordInfo = ipObjPair->Object1;
							if (ipWordInfo)
							{
								long nEndWordPos = ipWordInfo->EndPosition;
								
								// if user wants to get words only until the first
								// Stop char is encountered, even though the
								// word count might not be the maximum
								if (m_bStopAtNewLine || m_bStopForOther)
								{
									// Start Pattern with Stop characters
									string strStopPattern = "[" + m_strStops;
									if (m_bStopAtNewLine)
									{
										// Add carriage-return and line-feed characters
										strStopPattern = strStopPattern + "\r\n";
									}
									// Finish and apply the Pattern
									strStopPattern = strStopPattern + "]+";
									ipParser->Pattern = _bstr_t(strStopPattern.c_str());

									IIUnknownVectorPtr ipStopChars = 
										ipParser->Find(_bstrStrValue, VARIANT_FALSE, 
										VARIANT_FALSE);
									long nNumOfStops = ipStopChars->Size();
									if (nNumOfStops >= 1)
									{
										// get the first Stop position
										IObjectPairPtr ipObjPair = ipStopChars->At(0);
										// Token is the first object in the object pair
										ITokenPtr ipStop = ipObjPair->Object1;
										if (ipStop)
										{
											long nStartStopPos = ipStop->StartPosition;

											// if Stop char is before the last word
											if (nEndWordPos > nStartStopPos)
											{
												nEndWordPos = nStartStopPos - 1;
											}
										}
									}
								}
								
								if (nEndWordPos < 0)
								{
									ipValue->Clear();
								}
								else
								{
									ipValue = ipValue->GetSubString(0, nEndWordPos);
									// trim off any leading/trailing word separator(s), such as space, etc.
									ipValue->Trim(_bstr_t(m_strPunctuations.c_str()), 
										_bstr_t(m_strPunctuations.c_str()));
								}
							}
						}
					}
					break;
				case kUptoXLines:
					{
						// find all new line chars
						ipParser->Pattern = _bstr_t("[\n\r]+");
						IIUnknownVectorPtr ipNewLines = 
							ipParser->Find(_bstrStrValue, VARIANT_FALSE, VARIANT_FALSE);
						long nNumOfNewLines = ipNewLines->Size();
						// if there's only clue line
						if (nNumOfNewLines == 0)
						{
							// if clue line is included, then
							// the _bstrStrValue is good enough
							if (m_bIncludeClueLine)
							{
								break;
							}
							// and clue line text is not included, 
							// then there's no value to return
							else 
							{
								continue;
							}
						}

						// start position of the substring we want to extract
						long nStartPos=0;
						// get the first new line position
						IObjectPairPtr ipObjPair = ipNewLines->At(0);
						// Token is the first object in the object pair
						ITokenPtr ipNewLine = ipObjPair->Object1;
						if (ipNewLine)
						{
							long nEndFirstNewLinePos = ipNewLine->EndPosition;

							if (!m_bIncludeClueLine)
							{
								// if clue line is not included, start pos
								// will be the first char after first new line char(s)
								nStartPos = nEndFirstNewLinePos + 1;
							}
						}

						long nIndex = m_nNumOfLines - 1;
						if (m_bIncludeClueLine)
						{
							if (nNumOfNewLines+1 <= m_nNumOfLines) break;
						}
						else
						{
							if (nNumOfNewLines <= m_nNumOfLines)
							{
								if (ipValue->Size-1 < nStartPos)
								{
									ipValue->Clear();
								}
								else
								{
									// get the string from nStartPos till the end of the string
									ipValue = ipValue->GetSubString(nStartPos, ipValue->Size-1);
								}
								break;
							}

							nIndex = m_nNumOfLines;
						}
						
						ipObjPair = ipNewLines->At(nIndex);
						ipNewLine = ipObjPair->Object1;
						if (ipNewLine)
						{
							long nStartMaxNewLinePos = ipNewLine->StartPosition;
							if (nStartMaxNewLinePos-1 < nStartPos)
							{
								ipValue->Clear();
							}
							else
							{
								ipValue = ipValue->GetSubString(nStartPos, nStartMaxNewLinePos-1);
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
						IIUnknownVectorPtr ipVecFoundStrings(
							ipParser->Find(_bstrStrValue, VARIANT_FALSE, VARIANT_FALSE));
						
						if (ipVecFoundStrings->Size() == 0)
						{
							continue;
						}
						
						IObjectPairPtr ipObjPair = ipVecFoundStrings->At(0);
						// Token is the first object in the object pair
						ITokenPtr ipFound = ipObjPair->Object1;
						if (ipFound)
						{
							// get the first occurrence info
							long nStart = ipFound->StartPosition;
							if (nStart < 1)
							{
								ipValue->Clear();
							}
							else
							{
								ipValue = ipValue->GetSubString(0, nStart-1);
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
				// store the found string value
				ipAttribute->Value = ipValue;
				ipAttributes->PushBack(ipAttribute);
			}
		}

		*pAttributes = ipAttributes.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04176");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueAfterClue::raw_IsConfigured(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		*pbValue = m_ipClues->Size > 0 ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04858");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueAfterClue::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19585", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Value after clue rule").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04185")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueAfterClue::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CValueAfterClue::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		UCLID_AFVALUEFINDERSLib::IValueAfterCluePtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI08262", ipSource != __nullptr);

		m_bCaseSensitive = (ipSource->GetIsCaseSensitive()==VARIANT_TRUE) ? true : false;
		m_bClueAsRegExpr = (ipSource->GetClueAsRegExpr()==VARIANT_TRUE) ? true : false;
		m_bClueToStringAsRegExpr = (ipSource->GetClueToStringAsRegExpr()==VARIANT_TRUE) ? true : false;
		
		ICopyableObjectPtr ipCopyObj = ipSource->GetClues();
		ASSERT_RESOURCE_ALLOCATION("ELI08263", ipCopyObj != __nullptr);
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08264");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueAfterClue::raw_Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy(CLSID_ValueAfterClue);
		ASSERT_RESOURCE_ALLOCATION("ELI08350", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04489");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IValueAfterClue
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueAfterClue::get_ClueAsRegExpr(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_bClueAsRegExpr ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04896")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueAfterClue::put_ClueAsRegExpr(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bClueAsRegExpr = newVal == VARIANT_TRUE;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04897")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueAfterClue::get_IsCaseSensitive(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_bCaseSensitive ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04535")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueAfterClue::put_IsCaseSensitive(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bCaseSensitive = newVal == VARIANT_TRUE;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04536")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueAfterClue::get_Clues(IVariantVector **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		IVariantVectorPtr ipShallowCopy = m_ipClues;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04539")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueAfterClue::put_Clues(IVariantVector *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_ipClues = newVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04540")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueAfterClue::get_RefiningType(ERuleRefiningType *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = (ERuleRefiningType)m_eRefiningType;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04516")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueAfterClue::SetNoRefiningType()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_eRefiningType = UCLID_AFVALUEFINDERSLib::kNoRefiningType;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04519")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueAfterClue::SetClueLineType()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_eRefiningType = UCLID_AFVALUEFINDERSLib::kClueLine;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04520")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueAfterClue::GetUptoXWords(long *nNumOfWords, 
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04521")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueAfterClue::SetUptoXWords(long nNumOfWords, 
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04528")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueAfterClue::GetUptoXLines(long *nNumOfLines, VARIANT_BOOL *bIncludeClueLine)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*nNumOfLines = m_nNumOfLines;
		*bIncludeClueLine = m_bIncludeClueLine?VARIANT_TRUE:VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04522")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueAfterClue::SetUptoXLines(long nNumOfLines, VARIANT_BOOL bIncludeClueLine)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04523")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueAfterClue::GetClueToString(BSTR *strString)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*strString = _bstr_t(m_strLimitingString.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04524")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueAfterClue::SetClueToString(BSTR strString)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_eRefiningType = UCLID_AFVALUEFINDERSLib::kClueToString;
		m_strLimitingString = asString( strString );

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04525")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueAfterClue::get_ClueToStringAsRegExpr(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		*pVal = m_bClueToStringAsRegExpr ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05746")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueAfterClue::put_ClueToStringAsRegExpr(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		m_bClueToStringAsRegExpr = newVal == VARIANT_TRUE;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05785")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueAfterClue::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_ValueAfterClue;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueAfterClue::IsDirty(void)
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
				throw UCLIDException("ELI04791", "Object does not support persistence!");
			}
			
			hr = ipPersistStream->IsDirty();
		}

		return hr;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04792");

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
STDMETHODIMP CValueAfterClue::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// clear the variables first
		m_bCaseSensitive = false;
		m_bClueAsRegExpr = false;
		m_eRefiningType = UCLID_AFVALUEFINDERSLib::kNoRefiningType;
		m_nNumOfWords = 0;
		m_strPunctuations = "";
		m_strStops = "";
		m_bStopAtNewLine = false;;
		m_bStopForOther = false;;
		m_nNumOfLines = 0;
		m_bIncludeClueLine = false;
		m_strLimitingString = "";
//		m_strLimitingRegExpr = "";
		m_bClueToStringAsRegExpr = false;
		m_ipClues = __nullptr;

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
			UCLIDException ue( "ELI07648", "Unable to load newer ValueAfterClue Finder." );
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
		::readObjectFromStream(ipObj, pStream, "ELI09967");
		if (ipObj == __nullptr)
		{
			throw UCLIDException("ELI04697", "Clue list could not be read from stream!");
		}
		else
		{
			m_ipClues = ipObj;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04674");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
// Version == 2
//   Added m_bStopForOther
//   Added m_strStops
// Version == 3
//   Added m_bClueToStringAsRegExpr
//   Removed m_strLimitingRegExpr
STDMETHODIMP CValueAfterClue::Save(IStream *pStream, BOOL fClearDirty)
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
		if (ipObj == __nullptr)
		{
			throw UCLIDException( "ELI04701", 
				"Clues collection does not support persistence!" );
		}
		else
		{
			::writeObjectToStream( ipObj, pStream, "ELI09922", fClearDirty );
		}

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04675");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueAfterClue::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CValueAfterClue::validateLicense()
{
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI04188", "Value After Clue Rule" );
}
//-------------------------------------------------------------------------------------------------
