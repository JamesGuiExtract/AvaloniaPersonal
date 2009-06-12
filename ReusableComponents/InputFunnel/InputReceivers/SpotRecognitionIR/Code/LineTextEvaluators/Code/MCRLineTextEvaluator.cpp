
#include "stdafx.h"
#include "LineTextEvaluators.h"
#include "MCRLineTextEvaluator.h"

#include <comdef.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <UCLIDException.h>
#include <comutils.h>
#include <ComponentLicenseIDs.h>

using namespace std;

//-------------------------------------------------------------------------------------------------
// CMCRLineTextEvaluator
//-------------------------------------------------------------------------------------------------
CMCRLineTextEvaluator::CMCRLineTextEvaluator()
: m_ipMCRTextPositions(NULL),
  m_ipTokenPosition(NULL),
  m_ipMCRInputFinder(__uuidof(MCRTextInputFinder))
{
	try
	{
		if (m_ipMCRInputFinder == NULL)
		{
			throw UCLIDException("ELI03488", "Unable to create MCRInputFinder");
		}

		// get bearing, distance, angle type name
		ICategorizedComponentPtr ipBearingIV(__uuidof(BearingInputValidator));
		if (ipBearingIV == NULL)
		{
			throw UCLIDException("ELI03485", "Unable to create BearingInputValidator");
		}
		m_bstrBearingInputType = ipBearingIV->GetComponentDescription();

		ICategorizedComponentPtr ipDistanceIV(__uuidof(DistanceInputValidator));
		if (ipDistanceIV == NULL)
		{
			throw UCLIDException("ELI03486", "Unable to create DistanceInputValidator");
		}
		m_bstrDistanceInputType = ipDistanceIV->GetComponentDescription();

		ICategorizedComponentPtr ipAngleIV(__uuidof(AngleInputValidator));
		if (ipAngleIV == NULL)
		{
			throw UCLIDException("ELI03487", "Unable to create AngleInputValidator");
		}
		m_bstrAngleInputType = ipAngleIV->GetComponentDescription();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03484");
}
//-------------------------------------------------------------------------------------------------
CMCRLineTextEvaluator::~CMCRLineTextEvaluator()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20400");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMCRLineTextEvaluator::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ILineTextEvaluator,
		&IID_ILicensedComponent,
		&IID_ITestableComponent
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMCRLineTextEvaluator::raw_GetTextScore(BSTR strLineText, BSTR strInputType, LONG *plScore)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState())

		// ensure that this component is licensed.
		validateLicense();

		if (plScore == NULL)
			return E_POINTER;
		
		long lScore = 0;

		if (_bstr_t(strInputType) == m_bstrBearingInputType)
		{
			lScore = getTextScore(kBearing, strLineText);
		}
		else if (_bstr_t(strInputType) == m_bstrDistanceInputType)
		{
			lScore = getTextScore(kDistance, strLineText);
		}
		else if (_bstr_t(strInputType) == m_bstrAngleInputType)
		{
			lScore = getTextScore(kAngle, strLineText);
		}

		// return the number of numeric characters in the text as its score
		*plScore = lScore;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02665")
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMCRLineTextEvaluator::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	try
	{
		if (pbValue == NULL)
			return E_POINTER;

		// try to ensure that this component is licensed.
		validateLicense();

		// if the above method call does not throw an exception, then this
		// component is licensed.
		*pbValue = VARIANT_TRUE;
	}
	catch (...)
	{
		// if we caught some exception, then this component is not licensed.
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
long CMCRLineTextEvaluator::getNumOfChars(const std::string& strInput)
{
	long nTotalNum = 0;
	for (unsigned long ul = 0; ul < strInput.size(); ul++)
	{
		// skip any space
		if (strInput[ul] == ' ' || strInput[ul] == '\t')
		{
			continue;
		}

		nTotalNum++;
	}

	return nTotalNum;
}
//-------------------------------------------------------------------------------------------------
long CMCRLineTextEvaluator::getTextScore(EMCRType eMCRType, BSTR bstrInputText)
{
	m_ipMCRTextPositions = m_ipMCRInputFinder->ParseString(bstrInputText);
	
	long nTotalScore = 0;

	// get the input text in STL string format
	string strInputText = asString( bstrInputText );

	if (m_ipMCRTextPositions)
	{
		// scores for each type
		long nBearingScore = 0;
		long nDistanceScore = 0;
		long nAngleScore = 0;
		long nNumberScore = 0;

		// start, end and token info
		long		nStartPos;
		long		nEndPos;

		for (long i = 0; i < m_ipMCRTextPositions->Size(); i++)
		{
			// Retrieve this token
			m_ipTokenPosition = m_ipMCRTextPositions->At(i);
			if (m_ipTokenPosition)
			{
				CComBSTR bstrType, bstrText;
				// Retrieve details
				m_ipTokenPosition->GetTokenInfo( &nStartPos, &nEndPos, 
					&bstrType, &bstrText );

				// get this token MCR Text
				string strMCRText = _bstr_t(bstrText);
				string strType = _bstr_t(bstrType);

				// Get type information
				long	lInfoTag = -1;

				if (strType == "Angle")
				{
					lInfoTag = kAngle;
				}
				else if (strType == "Bearing")
				{
					lInfoTag = kBearing;
				}
				else if (strType == "Distance")
				{
					lInfoTag = kDistance;
				}
				else if (strType == "Number")
				{
					lInfoTag = kNumber;
				}

				// Update the score
				switch (lInfoTag)
				{
				case kBearing:		// Bearing
					nBearingScore += getNumOfChars(strMCRText) * 10;
					break;
				case kDistance:		// Distance
					nDistanceScore += getNumOfChars(strMCRText) * 10;
					break;
				case kAngle:		// Angle
					nAngleScore += getNumOfChars(strMCRText) * 10;
					break;
				case kNumber:		// Number
					nNumberScore = getNumOfChars(strMCRText);
					break;
				default:
					{
						nBearingScore = 0;
						nDistanceScore = 0;
						nAngleScore = 0;
						nNumberScore = 0;
					}
				}
			}
		}

		// calculate the total score according to the actual input type
		switch (eMCRType)
		{
		case kBearing:
			nTotalScore = nBearingScore + nAngleScore + nNumberScore;
			break;
		case kAngle:
			nTotalScore = nAngleScore + nNumberScore * 10;
			break;
		case kDistance:
			nTotalScore = nAngleScore + nDistanceScore + nNumberScore * 10;
			break;
		}
	}

	return nTotalScore;
}
//-------------------------------------------------------------------------------------------------
void CMCRLineTextEvaluator::validateLicense()
{
	static const unsigned long ulTHIS_COMPONENT_ID = gnICOMAP_CORE_OBJECTS;

	VALIDATE_LICENSE( ulTHIS_COMPONENT_ID, "ELI02776", 
		"MCR Line Text Evaluator" );
}
//-------------------------------------------------------------------------------------------------
