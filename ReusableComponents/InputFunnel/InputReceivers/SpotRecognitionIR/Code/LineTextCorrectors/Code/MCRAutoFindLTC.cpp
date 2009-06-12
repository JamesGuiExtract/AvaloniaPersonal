// MCRAutoFindLTC.cpp : Implementation of CMCRAutoFindLTC
#include "stdafx.h"
#include "LineTextCorrectors.h"
#include "MCRAutoFindLTC.h"
#include "..\\..\\..\\..\\..\\InputValidators\\LandRecordsIV\\Code\\InputTypes.h"
#include "..\..\..\..\HighlightedTextIR\Code\InputFinders\Code\MCRTextFinderEngine.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <COMUtils.h>
#include <ComponentLicenseIDs.h>

#include <algorithm>
using namespace std;

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMCRAutoFindLTC::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ILineTextCorrector,
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
// CMCRAutoFindLTC
//-------------------------------------------------------------------------------------------------
CMCRAutoFindLTC::CMCRAutoFindLTC()
{
	m_ipMCRInputFinder.CoCreateInstance(__uuidof(MCRTextInputFinder));
}

//-------------------------------------------------------------------------------------------------
// ILineTextCorrector
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMCRAutoFindLTC::raw_CorrectText(BSTR strInputText, BSTR strInputType, BSTR * pstrOutputText)
{
	try
	{
		// Check license
		validateLicense();

		if (!m_ipMCRInputFinder)
		{
			throw UCLIDException("ELI03075", "MCR Text Input Finder not created successfully!");
		}

		if (pstrOutputText == NULL)
			return E_POINTER;

		// convert strInputText to std::string
		string stdstrInputText = asString( strInputText );

		// convert strInputType to std::string
		string stdstrInputType = asString( strInputType );

		// if the input type is not one of the ones this object knows how to handle, then
		// return immediately
		if (!isKnowledgeableOfInputType(stdstrInputType))
		{
			*pstrOutputText = get_bstr_t( strInputText ).copy();
			return S_OK;
		}

		// before auto finding MCR Text, use MCRTextCorrector correct the text
		CComQIPtr<ILineTextCorrector> ipMCRTextCorrector;
		ipMCRTextCorrector.CoCreateInstance(__uuidof(MCRTextCorrector));
		_bstr_t _bstrOutputText(ipMCRTextCorrector->CorrectText(strInputText, strInputType));
		
		// to avoid doing the string comparisions inside the loop, let's determine
		// the expected input type right now.
		enum EExpectedInputType
		{
			kDirectionIsExpected,
			kBearingIsExpected,
			kDistanceIsExpected,
			kAngleIsExpected,
			kTextIsExpected
		};

		EExpectedInputType eExpectedInputType = kTextIsExpected;
		
		if (stdstrInputType == gstrDirection_INPUT_TYPE)
		{
			eExpectedInputType = kDirectionIsExpected;
		}
		else if (stdstrInputType == gstrBEARING_INPUT_TYPE)
		{
			eExpectedInputType = kBearingIsExpected;
		}
		else if (stdstrInputType == gstrDISTANCE_INPUT_TYPE)
		{
			eExpectedInputType = kDistanceIsExpected;
		}
		else if (stdstrInputType == gstrANGLE_INPUT_TYPE)
		{
			eExpectedInputType = kAngleIsExpected;
		}


		// a string to store the output..by default the output = 'corrected' input
		string stdstrOutputText(_bstrOutputText);

		// If the expected input type is other, then
		// we don't need to do all the work, as this line text corrector only knows how
		// to find bearings, distances, angles, and directions from input text
		if (eExpectedInputType != kTextIsExpected)
		{
			// ask the input finder to parse the text and find MCR text
			UCLID_COMUTILSLib::IIUnknownVectorPtr ipTokenPositions = m_ipMCRInputFinder->ParseString( get_bstr_t( strInputText ));

			// iterate through the tokens if tokens are available, and determine
			// which token to return
			long lNumTokens = ipTokenPositions->Size();
			if (lNumTokens > 0)
			{
				// We want the longest token
				long	lLongestTokenLength = 0;

				for (int i = 0; i < lNumTokens; i++)
				{
					// get the token
					CComQIPtr<IToken> ipToken;
					ipToken = ipTokenPositions->At(i);

					// get the token information
					long		lStartPos;
					long		lEndPos;
					CComBSTR	bstrType;
					ipToken->GetTokenInfo(&lStartPos, &lEndPos, &bstrType, NULL);
					bool bFound = false;

					// Determine specific type
					long	lInfoTag = -1;
					_bstr_t	strType(bstrType);

					if (strType == _bstr_t( "Angle" ))
					{
						lInfoTag = kAngle;
					}
					else if (strType == _bstr_t( "Bearing" ))
					{
						lInfoTag = kBearing;
					}
					else if (strType == _bstr_t( "Distance" ))
					{
						lInfoTag = kDistance;
					}
					else if (strType == _bstr_t( "Number" ))
					{
						lInfoTag = kNumber;
					}

					switch (lInfoTag)
					{
					case kAngle: 
						if (eExpectedInputType == kDirectionIsExpected || 
							eExpectedInputType == kAngleIsExpected ||
							eExpectedInputType == kDistanceIsExpected) // 1299.32' is a valid angle according to our current filters!
							bFound = true;
						break;
					
					case kBearing:
						if (eExpectedInputType == kBearingIsExpected ||
							eExpectedInputType == kDirectionIsExpected)
							bFound = true;
						break;
					
					case kDistance:
					case kNumber:
						if (eExpectedInputType == kDistanceIsExpected)
							bFound = true;
						break;
					
					default:
						break;
					}

					// If we found what we are looking for, determine 
					// the token length and check against max
					if (bFound)
					{
						if ((lEndPos - lStartPos + 1) > lLongestTokenLength)
						{
							// Store new max length
							lLongestTokenLength = (lEndPos - lStartPos + 1);

							string strSubStr(stdstrInputText.substr(lStartPos, lEndPos - lStartPos + 1));
							// If the sub string length is more than a certain percentage
							// of the original string length, then overwrite the original 
							// string with the sub string
							if (strSubStr.length() >= stdstrInputText.length()/3)
							{
								stdstrOutputText = strSubStr;
							}
						}
					}			// end if found token of appropriate type
				}				// end for each token
			}					// end if numTokens > 0
		}						// end if NOT TextExpected

		// return the corrected text to the user
		_bstrOutputText = stdstrOutputText.c_str();
		*pstrOutputText = _bstrOutputText.copy();
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03076");
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMCRAutoFindLTC::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	try
	{
		validateLicense();
		// if validateLicense doesn't throw any exception, then pbValue is true
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper methods
//-------------------------------------------------------------------------------------------------
bool CMCRAutoFindLTC::isKnowledgeableOfInputType(const std::string& strInputType)
{
	static bool ls_bInitialized = false;
	static vector<string> ls_vecKnownInputTypes;
	if (!ls_bInitialized)
	{
		ls_vecKnownInputTypes.push_back(gstrDirection_INPUT_TYPE);
		ls_vecKnownInputTypes.push_back(gstrBEARING_INPUT_TYPE);
		ls_vecKnownInputTypes.push_back(gstrDISTANCE_INPUT_TYPE);
		ls_vecKnownInputTypes.push_back(gstrANGLE_INPUT_TYPE);
		ls_bInitialized = true;
	}

	vector<string>::iterator iter;
	iter = find(ls_vecKnownInputTypes.begin(), ls_vecKnownInputTypes.end(), strInputType);
	return iter != ls_vecKnownInputTypes.end();
}
//-------------------------------------------------------------------------------------------------
void CMCRAutoFindLTC::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnICOMAP_CORE_OBJECTS;

	VALIDATE_LICENSE( THIS_COMPONENT_ID, "ELI03077", 
		"MCR Auto Find LTC" );
}
//-------------------------------------------------------------------------------------------------
