// MCRTextCorrector.cpp : Implementation of CMCRTextCorrector
#include "stdafx.h"
#include "LineTextCorrectors.h"
#include "MCRTextCorrector.h"
#include "..\\..\\..\\..\\..\\InputValidators\\LandRecordsIV\\Code\\InputTypes.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMCRTextCorrector::InterfaceSupportsErrorInfo(REFIID riid)
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
// CMCRTextCorrector
//-------------------------------------------------------------------------------------------------
CMCRTextCorrector::CMCRTextCorrector()
{
}
//-------------------------------------------------------------------------------------------------
CMCRTextCorrector::~CMCRTextCorrector()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20399");
}

//-------------------------------------------------------------------------------------------------
// ILineTextCorrector
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMCRTextCorrector::raw_CorrectText(BSTR strInputText, BSTR strInputType, BSTR * pstrOutputText)
{
	try
	{
		// Check license
		validateLicense();

		if (pstrOutputText == NULL)
			return E_POINTER;
		
		_bstr_t _bstrInputType = strInputType;
		string stdstrInputType = _bstrInputType;

		_bstr_t _bstrInputText = strInputText;
		string stdstrInputText = _bstrInputText;

		// if the input type is not one of the ones this object knows how to handle, then
		// return immediately
		if (!isKnowledgeableOfInputType(stdstrInputType))
		{
			*pstrOutputText = _bstrInputText.copy();
			return S_OK;
		}

		// perform generic pre-processing on MCR input
		preProcessMCRInput(stdstrInputText);

		// perform input-specific pre-processing
		if (stdstrInputType == gstrDirection_INPUT_TYPE)
		{
			// determine what kind of direction it is
			static IDirectionPtr ipDirection(__uuidof(Direction));
			ECartographicDirection eDirection = ipDirection->GetGlobalDirectionType();
			switch (eDirection)
			{
			case kBearingDirection:
				preProcessBearingInput(stdstrInputText);
				break;
			case kPolarAngleDirection:
			case kAzimuthDirection:
				preProcessAngleInput(stdstrInputText);
				break;
			}
		}
		else if (stdstrInputType == gstrBEARING_INPUT_TYPE)
		{
			preProcessBearingInput(stdstrInputText);
		}
		else if (stdstrInputType == gstrDISTANCE_INPUT_TYPE)
		{
			preProcessDistanceInput(stdstrInputText);
		}
		else if (stdstrInputType == gstrANGLE_INPUT_TYPE)
		{
			preProcessAngleInput(stdstrInputText);
		}

		// return the corrected text to the user
		_bstr_t _bstrOutputText = stdstrInputText.c_str();
		*pstrOutputText = _bstrOutputText.copy();
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02779");
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMCRTextCorrector::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
void CMCRTextCorrector::preProcessMCRInput(string& strInput)
{
	// remove all newline and whitespace chars
	replaceVariable(strInput, "\r", "");
	replaceVariable(strInput, "\n", "");
	replaceVariable(strInput, "\t", "");
	replaceVariable(strInput, " ", "");
	replaceVariable(strInput, "|", "1");

	// remove all underscore chars
	replaceVariable(strInput, "_", "");

	// replace any double decimal points with single
	replaceVariable(strInput, "..", ".");

	// find any (bad) leading characters that are periods, or commas, dashes, or tildes, etc
	const char *pszBadLeadingChars = ",.~-;:*";
	int iNumBadLeadingChars = 0;
	unsigned long ulLength = strInput.length();
	char *pszInput = const_cast<char *> (strInput.data());
	unsigned int ui;
	for (ui = 0; ui < ulLength; ui++)
	{
		if (strchr(pszBadLeadingChars, pszInput[ui]) != NULL)
		{
			iNumBadLeadingChars++;
		}
		else
		{
			break;
		}
	}

	// remove any bad leading characters
	if (iNumBadLeadingChars != 0)
	{
		strInput.erase(0, iNumBadLeadingChars);
	}

	// find any trailing bad characters, but allow trailing symbols
	const char *pszBadTrailingChars = ",.;:";
	int iNumBadTrailingChars = 0;
	ulLength = strInput.length();
	pszInput = const_cast<char *> (strInput.data());
	for (ui = ulLength - 1; ui > 0; ui--)
	{
		if (strchr(pszBadTrailingChars, pszInput[ui]) != NULL)
		{
			iNumBadTrailingChars++;
		}
		else
		{
			break;
		}
	}

	// remove any trailing bad characters
	if (iNumBadTrailingChars != 0)
	{
		strInput.erase(ulLength - iNumBadTrailingChars, iNumBadTrailingChars);
	}

	// Make a fresh copy of the cleaned string
	ulLength = strInput.length();
	pszInput = const_cast<char *>(strInput.data());

	// Step through string looking for consecutive symbols
	unsigned int	uj = 0;
	bool	bPrevious = false;
	for (ui = 0; ui < ulLength - uj; ui++)
	{
		if (isSymbolChar( pszInput[ui-uj] ))
		{
			// Was previous character a symbol
			if (bPrevious)
			{
				// Remove this character from the string
				strInput.erase( ui - uj, 1 );

				// Increment erasure counter
				uj++;
			}

			// Set flag
			bPrevious = true;
		}
		else
		{
			bPrevious = false;
		}
	}

	// Make a fresh copy of the cleaned string
	ulLength = strInput.length();
	pszInput = const_cast<char *>(strInput.data());

	// do all expected-type independent corrections first
	{
		// replace all uppercase U's and Q's with zeros if they have a digit 
		// or a symbol on atleast one side of it
		for (ui = 0; ui < ulLength; ui++)
		{
			if (pszInput[ui] == 'U' || pszInput[ui] == 'Q')
			{
				if (ui > 0 && 
					(isDigitChar(pszInput[ui-1]) || isSymbolChar(pszInput[ui-1])))
				{
					pszInput[ui] = '0';
				}
				else if (ui < ulLength - 1 && 
					(isDigitChar(pszInput[ui+1]) || isSymbolChar(pszInput[ui+1])))
				{
					pszInput[ui] = '0';
				}
			}
		}

		// replace all O's with zeros if the O has a digit or a symbol 
		// or another O on at least one side of it
		for (ui = 0; ui < ulLength; ui++)
		{
			if (pszInput[ui] == 'o' || pszInput[ui] == 'O')
			{
				if ((ui > 0) && (pszInput[ui-1] == 'O' || 
					isDigitChar(pszInput[ui-1]) || isSymbolChar(pszInput[ui-1])))
				{
					pszInput[ui] = '0';
				}
				else if (ui < ulLength - 1 && (pszInput[ui+1] == 'O' || 
					isDigitChar(pszInput[ui+1]) || isSymbolChar(pszInput[ui+1])))
				{
					pszInput[ui] = '0';
				}
			}
		}

		// replace all s's with 5's if the s has a digit on atleast one side of it
		for (ui = 0; ui < ulLength; ui++)
		{
			if (pszInput[ui] == 's' || pszInput[ui] == 'S')
			{
				if (ui > 0 && isDigitChar(pszInput[ui-1]))
				{
					pszInput[ui] = '5';
				}
				else if (ui < ulLength - 1 && isDigitChar(pszInput[ui+1]))
				{
					pszInput[ui] = '5';
				}
			}
		}
		
		// replace all I's with 1's if the I has a digit or a symbol 
		// on atleast one side of it
		for (ui = 0; ui < ulLength; ui++)
		{
			if (pszInput[ui] == 'i' || pszInput[ui] == 'I')
			{
				if (ui > 0 && 
					(isDigitChar(pszInput[ui-1]) || isSymbolChar(pszInput[ui-1])))
				{
					pszInput[ui] = '1';
				}
				else if (ui < ulLength - 1 && 
					(isDigitChar(pszInput[ui+1]) || isSymbolChar(pszInput[ui+1])))
				{
					pszInput[ui] = '1';
				}
			}
		}

		// replace all lower-case L's with 1's if the l has a digit 
		// or a symbol on at least one side of it
		for (ui = 0; ui < ulLength; ui++)
		{
			if (pszInput[ui] == 'l')
			{
				if (ui > 0 && 
					(isDigitChar(pszInput[ui-1]) || isSymbolChar(pszInput[ui-1])))
				{
					pszInput[ui] = '1';
				}
				else if (ui < ulLength - 1 && 
					(isDigitChar(pszInput[ui+1]) || isSymbolChar(pszInput[ui+1])))
				{
					pszInput[ui] = '1';
				}
			}
		}

		// replace all uppercase T's with 7's if the T has a digit or a symbol 
		// on at least one side of it
		for (ui = 0; ui < ulLength; ui++)
		{
			if (pszInput[ui] == 'T')
			{
				if (ui > 0 && 
					(isDigitChar(pszInput[ui-1]) || isSymbolChar(pszInput[ui-1])))
				{
					pszInput[ui] = '7';
				}
				else if (ui < ulLength - 1 && 
					(isDigitChar(pszInput[ui+1]) || isSymbolChar(pszInput[ui+1])))
				{
					pszInput[ui] = '7';
				}
			}
		}
	}
}
//--------------------------------------------------------------------------------------------------
void CMCRTextCorrector::preProcessBearingInput(string& strInput)
{
	//////////////////
	// NEW Bearing LTC - 10/11/02 WEL
	//////////////////

	// If the first character of a bearing is a 5, convert it to an S
	if (strInput.find( "5" ) == 0)
	{
		strInput.replace( 0, 1, string("S") );
	}

	// Locate each symbol within the string
	std::vector<long> vecPos;
	locateSymbols( strInput, vecPos );

	// Check for OCR zeros that should be symbols
	if (fixedBadZeros( strInput, vecPos, false ))
	{
		// Flush the vector
		vecPos.clear();

		// Relocate the symbols
		locateSymbols( strInput, vecPos );
	}

	// Make local copy of input string
	unsigned long ulLength = strInput.length();
	char *pszText = const_cast<char *>(strInput.data());

	// Check number of located symbols
	long lCount = vecPos.size();
	switch (lCount)
	{
	case 3:
		// Quick check for a 5 that should be an S
		if ((vecPos[0] > 2) &&
			(pszText[vecPos[0]-3] == '5'))
		{
			// Replace the 5 with an S
			strInput.replace( vecPos[0]-3, 1, string("S") );
		}

		// Check for Dxxyy°zz'D?"
		if ((vecPos[0] > 3) &&
			isDigitChar( pszText[vecPos[0]-1] ) &&
			isDigitChar( pszText[vecPos[0]-2] ) &&
			isDigitChar( pszText[vecPos[0]-3] ) &&
			isDigitChar( pszText[vecPos[0]-4] ))
		{
			// Insert a degree symbol between xx and yy
			strInput.insert( vecPos[0]-2, string("°") );

			// Flush the vector
			vecPos.clear();

			// Relocate the symbols
			locateSymbols( strInput, vecPos );
		}

		// Stamp DMS symbols in appropriate positions
		stampSymbols( strInput, vecPos[0], vecPos[1], vecPos[2] );
		break;

	case 2:
		// Handle Dxx?y'zz"D case
		if (((vecPos[1] - vecPos[0]) == 3) && 
			(vecPos[0] > 4) && 
			isDigitChar(pszText[vecPos[0]-1]) &&
			isDigitChar(pszText[vecPos[0]-3]) &&
			isDigitChar(pszText[vecPos[0]-4]))
		{
			// Get Dxx characters
			string	strNew = strInput.substr( 0, vecPos[0]-2 );

			// Append the degree symbol
			strNew.append( "°" );

			// Append remaining original characters
			strNew.append( strInput.substr( vecPos[0]-2, ulLength - (vecPos[0]-2) ) );

			// Replace original string with modified
			strInput = strNew;

			// Stamp DMS symbols in appropriate positions
			stampSymbols( strInput, vecPos[0]-2, vecPos[0]+1, vecPos[1]+1 );
		}

		// Handle Dxx°y?zz"D case
		else if (((vecPos[1] - vecPos[0]) == 5) && 
			isDigitChar(pszText[vecPos[0]+1]) &&
			isDigitChar(pszText[vecPos[0]+3]) &&
			isDigitChar(pszText[vecPos[0]+4]))
		{
			// Get Dxx°yy characters
			string	strNew = strInput.substr( 0, vecPos[0]+3 );

			// Append the minute symbol
			strNew.append( "'" );

			// Append remaining original characters
			strNew.append( strInput.substr( vecPos[0]+3, ulLength - (vecPos[0]+3) ) );

			// Replace original string with modified
			strInput = strNew;

			// Stamp DMS symbols in appropriate positions
			stampSymbols( strInput, vecPos[0], vecPos[0]+3, vecPos[1]+1 );
		}

		// Handle Dxx°yy?z"D case
		else if (((vecPos[1] - vecPos[0]) == 5) && 
			isDigitChar(pszText[vecPos[0]+1]) &&
			isDigitChar(pszText[vecPos[0]+2]) &&
			isDigitChar(pszText[vecPos[0]+4]))
		{
			// Get Dxx°yy characters
			string	strNew = strInput.substr( 0, vecPos[0]+3 );

			// Append the minute symbol
			strNew.append( "'" );

			// Append remaining original characters
			strNew.append( strInput.substr( vecPos[0]+3, ulLength - (vecPos[0]+3) ) );

			// Replace original string with modified
			strInput = strNew;

			// Stamp DMS symbols in appropriate positions
			stampSymbols( strInput, vecPos[0], vecPos[0]+3, vecPos[1]+1 );
		}

		// Handle Dxx°yyzz.z"D case
		else if (((vecPos[1] - vecPos[0]) == 7) && 
			isDigitChar(pszText[vecPos[0]+1]) &&
			isDigitChar(pszText[vecPos[0]+2]) &&
			isDigitChar(pszText[vecPos[0]+3]) &&
			isDigitChar(pszText[vecPos[0]+4]) &&
			(pszText[vecPos[0]+3] == '.') &&
			isDigitChar(pszText[vecPos[0]+6]))
		{
			// Get Dxx°yy characters
			string	strNew = strInput.substr( 0, vecPos[0]+3 );

			// Append the minute symbol
			strNew.append( "'" );

			// Append remaining original characters
			strNew.append( strInput.substr( vecPos[0]+3, ulLength - (vecPos[0]+3) ) );

			// Replace original string with modified
			strInput = strNew;

			// Stamp DMS symbols in appropriate positions
			stampSymbols( strInput, vecPos[0], vecPos[0]+3, vecPos[1]+1 );
		}

		// Handle Dxx°yy?zz"D case
		else if (((vecPos[1] - vecPos[0]) == 6) && 
			isDigitChar(pszText[vecPos[0]+1]) &&
			isDigitChar(pszText[vecPos[0]+2]) &&
			isDigitChar(pszText[vecPos[0]+4]) &&
			isDigitChar(pszText[vecPos[0]+5]))
		{
			// Replace the goofy character with minute symbol
			strInput.replace( vecPos[0]+3, 1, string("'") );

			// Stamp DMS symbols in appropriate positions
			stampSymbols( strInput, vecPos[0], vecPos[0]+3, vecPos[1] );
		}

		// Handle Dxx°yy?fff"D case
		// NOTE: This regular expression also could match the Dxx°yyzz.z"D case above
		else if (((vecPos[1] - vecPos[0]) == 7) && 
			isDigitChar(pszText[vecPos[0]+1]) &&
			isDigitChar(pszText[vecPos[0]+2]) &&
			!isDigitChar(pszText[vecPos[0]+3]))
		{
			bool	bFound = false;

			// Now must check for two consecutive digits within fff characters
			if (isDigitChar(pszText[vecPos[1]-3]) &&
				isDigitChar(pszText[vecPos[1]-2]))
			{
				// fff is zz?, just remove the last character
				strInput.erase( vecPos[1]-1, 1 );
				bFound = true;
			}
			else if (isDigitChar(pszText[vecPos[1]-2]) &&
				isDigitChar(pszText[vecPos[1]-1]))
			{
				// fff is ?zz, just remove the first character
				strInput.erase( vecPos[1]-3, 1 );
				bFound = true;
			}

			// Was a change made?
			if (bFound)
			{
				// Replace the first goofy character with minute symbol
				strInput.replace( vecPos[0]+3, 1, string("'") );

				// Stamp DMS symbols in appropriate positions
				stampSymbols( strInput, vecPos[0], vecPos[0]+3, vecPos[1]-1 );
			}
		}

		// Handle Dxx°yy'z? case with or without trailing D
		else if (((vecPos[1] - vecPos[0]) == 3) && 
			((long)ulLength > vecPos[1]+2) && 
			isDigitChar(pszText[vecPos[1]+1]))
		{
			// Get Dxx°yy'z? characters
			string	strNew = strInput.substr( 0, vecPos[1]+3 );

			// Append the second symbol
			strNew.append( "\"" );

			// Append any remaining original characters
			if ((long)ulLength > vecPos[1]+3)
			{
				strNew.append( strInput.substr( vecPos[1]+3, ulLength - (vecPos[1]+3) ) );
			}

			// Replace original string with modified
			strInput = strNew;

			// Stamp DMS symbols in appropriate positions
			stampSymbols( strInput, vecPos[0], vecPos[1], vecPos[1]+3 );
		}

		// Handle Dx?yy'zz"D case or Dx?yy'z"D case
		else if ((vecPos[0] > 4) && 
			isDigitChar(pszText[vecPos[0]-1]) &&
			isDigitChar(pszText[vecPos[0]-2]) &&
			isDigitChar(pszText[vecPos[0]-4]))
		{
			// Get Dx? characters
			string	strNew = strInput.substr( 0, vecPos[0]-2 );

			// Append the degree symbol
			strNew.append( "°" );

			// Append remaining original characters
			strNew.append( strInput.substr( vecPos[0]-2, ulLength - (vecPos[0]-2) ) );

			// Replace original string with modified
			strInput = strNew;

			// Stamp DMS symbols in appropriate positions
			stampSymbols( strInput, vecPos[0]-2, vecPos[0]+1, vecPos[1]+1 );
		}
		else
		{
			// No additions or corrections found
			// Just stamp the two symbols
			stampSymbols( strInput, vecPos[0], vecPos[1], -1 );
		}

		break;

	case 1:
		// Handle Dxxyy'zz case --> Dxx°yy'zz"
		if ((vecPos[0] > 4) && 
			(ulLength > 7) && 
			isDigitChar(pszText[vecPos[0]-1]) &&
			isDigitChar(pszText[vecPos[0]-2]) &&
			isDigitChar(pszText[vecPos[0]-3]) &&
			isDigitChar(pszText[vecPos[0]-4]) &&
			isDigitChar(pszText[vecPos[0]+1]) &&
			isDigitChar(pszText[vecPos[0]+2]))
		{
			// Get Dxx characters
			string	strNew = strInput.substr( 0, vecPos[0]-2 );

			// Append the degree symbol
			strNew.append( "°" );

			// Append yy'zz characters
			strNew.append( strInput.substr( vecPos[0]-2, 5 ) );

			// Append the second symbol
			strNew.append( "\"" );

			// Append any remaining original characters
			if (ulLength > 8)
			{
				strNew.append( strInput.substr( vecPos[0]+3, ulLength - (vecPos[0]+3) ) );
			}

			// Replace original string with modified
			strInput = strNew;

			// Stamp DMS symbols in appropriate positions
			stampSymbols( strInput, vecPos[0]-2, vecPos[0]+1, vecPos[0]+4 );
		}

		// Handle Dxx°yyz? case --> Dxx°yy'z?"
		//   also Dxx°yy?z case --> Dxx°yy'?z"
		else if ((vecPos[0] > 2) && 
			(ulLength > 7) && 
			isDigitChar(pszText[vecPos[0]-1]) &&
			isDigitChar(pszText[vecPos[0]-2]) &&
			isDigitChar(pszText[vecPos[0]+1]) &&
			isDigitChar(pszText[vecPos[0]+2]) &&
			
			(isDigitChar(pszText[vecPos[0]+3]) || isDigitChar(pszText[vecPos[0]+4])))
		{
			// Get Dxx°yy characters
			string	strNew = strInput.substr( 0, vecPos[0]+3 );

			// Append the minute symbol
			strNew.append( "'" );

			// Append z? (or ?z) characters
			strNew.append( strInput.substr( vecPos[0]+3, 2 ) );

			// Append the second symbol
			strNew.append( "\"" );

			// Append remaining original characters
			strNew.append( strInput.substr( vecPos[0]+5, ulLength - (vecPos[0]+5) ) );

			// Replace original string with modified
			strInput = strNew;

			// Stamp DMS symbols in appropriate positions
			stampSymbols( strInput, vecPos[0], vecPos[0]+3, vecPos[0]+6 );
		}

		// Handle Dxx°yyzz.z case --> Dxx°yy'zz.z"
		else if ((vecPos[0] > 2) && 
			(ulLength > 9) && 
			isDigitChar(pszText[vecPos[0]-1]) &&
			isDigitChar(pszText[vecPos[0]-2]) &&
			isDigitChar(pszText[vecPos[0]+1]) &&
			isDigitChar(pszText[vecPos[0]+2]) &&
			isDigitChar(pszText[vecPos[0]+3]) &&
			isDigitChar(pszText[vecPos[0]+4]) &&
			(pszText[vecPos[0]+5] == '.') &&
			isDigitChar(pszText[vecPos[0]+6]))
		{
			// Get Dxx°yy characters
			string	strNew = strInput.substr( 0, vecPos[0]+3 );

			// Append the minute symbol
			strNew.append( "'" );

			// Append zz.z characters
			strNew.append( strInput.substr( vecPos[0]+3, 4 ) );

			// Append the second symbol
			strNew.append( "\"" );

			// Append remaining original characters
			strNew.append( strInput.substr( vecPos[0]+7, ulLength - (vecPos[0]+7) ) );

			// Replace original string with modified
			strInput = strNew;

			// Stamp DMS symbols in appropriate positions
			stampSymbols( strInput, vecPos[0], vecPos[0]+3, vecPos[0]+8 );
		}

		// Handle Dxx°yy?zz case --> Dxx°yy'zz"
		else if ((vecPos[0] > 2) && 
			(ulLength > 8) && 
			isDigitChar(pszText[vecPos[0]-1]) &&
			isDigitChar(pszText[vecPos[0]-2]) &&
			isDigitChar(pszText[vecPos[0]+1]) &&
			isDigitChar(pszText[vecPos[0]+2]) &&
			isDigitChar(pszText[vecPos[0]+4]) &&
			isDigitChar(pszText[vecPos[0]+5]))
		{
			// Get Dxx°yy characters
			string	strNew = strInput.substr( 0, vecPos[0]+3 );

			// Append the minute symbol
			strNew.append( "'" );

			// Append zz characters
			strNew.append( strInput.substr( vecPos[0]+4, 2 ) );

			// Append the second symbol
			strNew.append( "\"" );

			// Append any remaining original characters
			strNew.append( strInput.substr( vecPos[0]+6, ulLength - (vecPos[0]+6) ) );

			// Replace original string with modified
			strInput = strNew;

			// Stamp DMS symbols in appropriate positions
			stampSymbols( strInput, vecPos[0], vecPos[0]+3, vecPos[0]+6 );
		}

		// Handle Dxxyyzz" case --> Dxx°yy'zz"
		else if ((vecPos[0] > 6) && 
			(ulLength > 7) && 
			isDigitChar(pszText[vecPos[0]-1]) &&
			isDigitChar(pszText[vecPos[0]-2]) &&
			isDigitChar(pszText[vecPos[0]-3]) &&
			isDigitChar(pszText[vecPos[0]-4]) &&
			isDigitChar(pszText[vecPos[0]-5]) &&
			isDigitChar(pszText[vecPos[0]-6]))
		{
			// Get Dxx characters
			string	strNew = strInput.substr( 0, vecPos[0]-4 );

			// Append the degree symbol
			strNew.append( "°" );

			// Append yy characters
			strNew.append( strInput.substr( vecPos[0]-4, 2 ) );

			// Append the minute symbol
			strNew.append( "'" );

			// Append remaining original characters
			strNew.append( strInput.substr( vecPos[0]-2, ulLength - (vecPos[0]-2) ) );

			// Replace original string with modified
			strInput = strNew;

			// Stamp DMS symbols in appropriate positions
			stampSymbols( strInput, vecPos[0]-4, vecPos[0]-1, vecPos[0]+2 );
		}

		// Handle Dxx?y'zz case --> Dxx°?y'zz"
		else if ((vecPos[0] > 4) && 
			(ulLength > 7) && 
			isDigitChar(pszText[vecPos[0]-1]) &&
			isDigitChar(pszText[vecPos[0]-3]) &&
			isDigitChar(pszText[vecPos[0]-4]) &&
			isDigitChar(pszText[vecPos[0]+1]) &&
			isDigitChar(pszText[vecPos[0]+2]))
		{
			// Get Dxx characters
			string	strNew = strInput.substr( 0, vecPos[0]-2 );

			// Append the degree symbol
			strNew.append( "°" );

			// Append ?y'zz characters
			strNew.append( strInput.substr( vecPos[0]-2, 5 ) );

			// Append the second symbol
			strNew.append( "\"" );

			// Append any remaining original characters
			if (ulLength > 8)
			{
				strNew.append( strInput.substr( vecPos[0]+3, ulLength - (vecPos[0]+3) ) );
			}

			// Replace original string with modified
			strInput = strNew;

			// Stamp DMS symbols in appropriate positions
			stampSymbols( strInput, vecPos[0]-3, vecPos[0]+1, vecPos[0]+4 );
		}

		// Handle Dx?yy'zz case --> Dxx°?y'zz"
		else if ((vecPos[0] > 4) && 
			(ulLength > 7) && 
			isDigitChar(pszText[vecPos[0]-1]) &&
			isDigitChar(pszText[vecPos[0]-2]) &&
			isDigitChar(pszText[vecPos[0]-4]) &&
			isDigitChar(pszText[vecPos[0]+1]) &&
			isDigitChar(pszText[vecPos[0]+2]))
		{
			// Get Dx? characters
			string	strNew = strInput.substr( 0, vecPos[0]-2 );

			// Append the degree symbol
			strNew.append( "°" );

			// Append ?y'zz characters
			strNew.append( strInput.substr( vecPos[0]-2, 5 ) );

			// Append the second symbol
			strNew.append( "\"" );

			// Append any remaining original characters
			if (ulLength > 8)
			{
				strNew.append( strInput.substr( vecPos[0]+3, ulLength - (vecPos[0]+3) ) );
			}

			// Replace original string with modified
			strInput = strNew;

			// Stamp DMS symbols in appropriate positions
			stampSymbols( strInput, vecPos[0]-3, vecPos[0]+1, vecPos[0]+4 );
		}

		// Handle Dxx?yzz" case --> Dxx°?y'zz"
		else if ((vecPos[0] > 6) && 
			(ulLength > 7) && 
			isDigitChar(pszText[vecPos[0]-1]) &&
			isDigitChar(pszText[vecPos[0]-2]) &&
			isDigitChar(pszText[vecPos[0]-3]) &&
			isDigitChar(pszText[vecPos[0]-5]) &&
			isDigitChar(pszText[vecPos[0]-6]))
		{
			// Get Dxx characters
			string	strNew = strInput.substr( 0, vecPos[0]-4 );

			// Append the degree symbol
			strNew.append( "°" );

			// Append ?y characters
			strNew.append( strInput.substr( vecPos[0]-4, 2 ) );

			// Append the minute symbol
			strNew.append( "'" );

			// Append remaining original characters
			strNew.append( strInput.substr( vecPos[0]-2, ulLength - (vecPos[0]-2) ) );

			// Replace original string with modified
			strInput = strNew;

			// Stamp DMS symbols in appropriate positions
			stampSymbols( strInput, vecPos[0]-4, vecPos[0]-1, vecPos[0]+2 );
		}

		break;

	case 0:
		break;

	default:
		// If symbol count > 3, just use the first 3
		if (lCount > 3)
		{
			stampSymbols( strInput, vecPos[0], vecPos[1], vecPos[2] );
		}
		break;
	}

	/////////////////////
	// Partial validation
	/////////////////////
	// Flush the vector
	vecPos.clear();

	// Relocate the symbols
	locateSymbols( strInput, vecPos );
	pszText = const_cast<char *>(strInput.data());

	// Just return if no symbols are present
	if (vecPos.size() < 3)
	{
		return;
	}

	// Check number of minutes for leading 7
	if ((vecPos[1] - vecPos[0] == 3) && 
		isDigitChar( pszText[vecPos[1]-1] ) &&
		( pszText[vecPos[1]-2] == '7' ))
	{
		// Change the 7 to a 2
		strInput.replace( vecPos[1]-2, 1, string( "2" ) );
	}

	// Check number of seconds for leading 7
	if ((vecPos[2] - vecPos[1] == 3) && 
		isDigitChar( pszText[vecPos[2]-1] ) &&
		( pszText[vecPos[2]-2] == '7' ))
	{
		// Change the 7 to a 2
		strInput.replace( vecPos[2]-2, 1, string( "2" ) );
	}

	// Check last direction
	if (((long)ulLength > vecPos[2] + 1) &&
		(pszText[vecPos[2]+1] == 'N'))
	{
		// Change the trailing N to W
		strInput.replace( vecPos[2]+1, 1, string( "W" ) );
	}

	// Check leading direction
	if ((vecPos[0] > 2) &&
		(vecPos.size() > 1) && 
		(pszText[vecPos[0]-3] == 'W'))
	{
		// Change the leading W to N
		strInput.replace( vecPos[0]-3, 1, string( "N" ) );
	}
}
//--------------------------------------------------------------------------------------------------
void CMCRTextCorrector::preProcessDistanceInput(string& strInput)
{
	int iLength = (int) strInput.length();
	char *pszText = const_cast<char *>(strInput.data());

	int ui;
	string strNumber, strCleanedText;

	// checking for "F" or 'f' in character after last number
	// & cleaning "FEET" or "feet"
	for (ui = iLength - 1; ui >= 0; ui--)
	{
		if (isDigitChar(pszText[ui]))
		{
			if (ui < iLength - 1  && pszText[ui + 1] == 'f')
			{
				strNumber = strInput.substr(0, ui + 1);
				strCleanedText = strNumber + "feet";
				break;
			}
			else if (ui < iLength - 1 && pszText[ui + 1] == 'F')
			{
				strNumber = strInput.substr(0, ui + 1);
				strCleanedText = strNumber + "FEET";
				break;
			}
		}
	}

	if (ui > 0)
	{
		strInput.erase();
		strInput.append(strCleanedText);
	}

	// check for commas and periods - if we find a period, then get rid of all commas 
	if (strchr(strInput.c_str(), '.') != NULL)
	{
		bool bContinue = false;

		int iLength = (int) strInput.length();
		for (ui = 0; ui < iLength; ui++)
		{
			if (strInput[ui] == ',')
			{
				strInput.erase(ui, 1);
				iLength--;
				ui--; // re-evaluate the new character that just moved into the position of the comma
			}
		}
	}

	// if a period exists, then we have already got rid of all commas above.  if a comma
	// still exists, that's because a period did not exist in the input string.  If a comma
	// is followed by two decimal digits, then replace that comma with a period
	char *pszComma = (char *)strchr(strInput.c_str(), ',');
	while (pszComma != NULL)
	{
		// Locate comma position within input string
		unsigned long ulCommaPos = pszComma - strInput.c_str();

		// Handle cases: xx,yy xx,yy'  xx,y'
		if ((ulCommaPos > 1) && (strInput.length() > 4))
		{
			// Check for yy digits and subsequent non-digit
			if (isDigitChar(pszComma[1]) && isDigitChar(pszComma[2]) && 
				!isDigitChar(pszComma[3]))
			{
				// Change comma to a decimal point
				pszComma[0] = '.';

				// try to find another comma
				pszComma = (char *)strchr(strInput.c_str(), ',');
			}
			// Check for x y digits and subsequent non-digit
			else if (ulCommaPos > 1 && isDigitChar(pszComma[-1]) && 
				isDigitChar(pszComma[1]) && !isDigitChar(pszComma[2]))
			{
				// Change comma to a decimal point
				pszComma[0] = '.';

				// try to find another comma
				pszComma = (char *)strchr(strInput.c_str(), ',');
			}
			// Check for xx yy digits
			else if (ulCommaPos > 1 && 
				isDigitChar(pszComma[-1]) && 
				isDigitChar(pszComma[-2]) && 
				isDigitChar(pszComma[1]) && 
				isDigitChar(pszComma[2]))
			{
				// Change comma to a decimal point
				pszComma[0] = '.';

				// try to find another comma
				pszComma = (char *)strchr(strInput.c_str(), ',');
			}
			else
			{
				pszComma = NULL;
			}
		}
		// Handle case: xx,y 
		else if ((ulCommaPos > 1) && (strInput.length() == 4))
		{
			// Check for xx y digits
			if (ulCommaPos > 1 && isDigitChar(pszComma[-2]) && 
				isDigitChar(pszComma[-1]) && isDigitChar(pszComma[1]))
			{
				// Change comma to a decimal point
				pszComma[0] = '.';

				// try to find another comma
				pszComma = (char *)strchr(strInput.c_str(), ',');
			}
			else
			{
				pszComma = NULL;
			}
		}
		else if (ulCommaPos < strInput.length() - 3)
		{
			strInput.erase(ulCommaPos, 1);

			// try to find another comma
			pszComma = (char *)strchr(strInput.c_str(), ',');
		}
		else
		{
			pszComma = NULL;
		}
	}

	// TODO: Add registry check for flag to turn this tweak OFF
	// Check for a lack of periods
	if (strchr(strInput.c_str(), '.') == NULL)
	{
		long	lConsecutiveDigits = 0;
		long	lMaxConsecutive = 0;
		long	lLastPos = -1;

		int iLength = (int) strInput.length();
		int ui;
		for (ui = 0; ui < iLength; ui++)
		{
			if (isDigitChar(strInput[ui]))
			{
				// Increment running counter
				lConsecutiveDigits++;

				// Is this the longest string
				if (lConsecutiveDigits > lMaxConsecutive)
				{
					// Save this string length
					lMaxConsecutive = lConsecutiveDigits;

					// Save this last digit position
					lLastPos = ui;
				}
			}
			else
			{
				// Reset running counter
				lConsecutiveDigits = 0;
			}
		}

		// Check for 5+ consecutive digits
		if (lMaxConsecutive > 4)
		{
			// Insert a decimal point before the second-to-last digit
			strInput.insert( lLastPos-1, string(".") );
		}
	}

	// Locate each symbol within the string
	std::vector<long> vecPos;
	locateSymbols( strInput, vecPos );

	// Update the local copy of input string
	pszText = const_cast<char *>(strInput.data());
	iLength = (int) strInput.length();

	if (vecPos.size() > 0)
	{
		int iSize = (int) vecPos.size();
		// Evaluate each symbol, looking for two preceding digits
		for (ui = 0; ui < iSize; ui++)
		{
			// Check string length and preceding characters
			if ((vecPos[ui] > 1) && 
				isDigitChar(pszText[vecPos[ui]-1]) && 
				isDigitChar(pszText[vecPos[ui]-2]))
			{
				// Just change the symbol to a single quote
				strInput.replace( vecPos[ui], 1, string( "'" ) );

				// No need to look at any more symbols
				break;
			}
			// Also allow x.x'
			else if ((vecPos[ui] > 2) && 
				isDigitChar(pszText[vecPos[ui]-1]) && 
				(pszText[vecPos[ui]-2] == '.') && 
				isDigitChar(pszText[vecPos[ui]-3]))
			{
				// Just change the symbol to a single quote
				strInput.replace( vecPos[ui], 1, string( "'" ) );

				// No need to look at any more symbols
				break;
			}
		}
	}
	/*
	/////////////////////////////////////////////////////////////
	// DO NOT add a single quote, since a string without a symbol
	// should be assumed to be in default units - 10/17/02 WEL
	/////////////////////////////////////////////////////////////
	// No symbols, try to add a single quote at the end
	else
	{
		// Look for a decimal point
		char *pszPoint = strchr( strInput.c_str(), '.' );
		if (pszPoint != NULL)
		{
			// Locate point position within input string
			unsigned long ulPointPos = pszPoint - strInput.c_str();

			// Look for two subsequent digits
			if ((ulLength - ulPointPos > 2) && isDigitChar(pszPoint[1]) && 
				isDigitChar(pszPoint[2]))
			{
				// Append a single quote
				strInput.insert( ulPointPos + 3, string( "'" ) );
			}
			// Look for one subsequent digit and one preceding digit
			else if ((ulLength - ulPointPos > 1) && (ulPointPos > 0) && 
				isDigitChar(pszPoint[1]) && isDigitChar(pszPoint[-1]))
			{
				// Append a single quote
				strInput.insert( ulPointPos + 2, string( "'" ) );
			}
		}
		// No decimal point found, insert one after last digit
		else
		{
			for (ui = ulLength-1; ui >= 0; ui++)
			{
				// Is this character a digit
				if (isDigitChar( pszText[ui] ))
				{
					// Append a single quote
					strInput.insert( ui + 1, string( "'" ) );
					break;
				}
			}
		}
	}
	*/
}
//--------------------------------------------------------------------------------------------------
bool CMCRTextCorrector::fixedBadZeros(string& strInput, 
									  std::vector<long>& rvecPos, bool bAngle)
{
	// Check number of symbols
	long	lCount = rvecPos.size();
	if (lCount == 0)
	{
		// No changes can be made
		return false;
	}

	// Make local copy of input string
	unsigned long ulLength = strInput.length();
	char *pszText = const_cast<char *>(strInput.data());

	////////////////////////
	// Handle xx?yy'zz" case
	////////////////////////
	// For Bearings, can't have xxx
	if (!bAngle)
	{
		// Check for min string length, yy digits, xx digits
		if ((rvecPos[0] > 5) && 
			isDigitChar(pszText[rvecPos[0]-1]) &&
			isDigitChar(pszText[rvecPos[0]-2]) &&
			isDigitChar(pszText[rvecPos[0]-4]) &&
			isDigitChar(pszText[rvecPos[0]-5]))
		{
			// Replace the goofy character with a degree symbol
			strInput.replace( rvecPos[0]-3, 1, string("°") );

			// Return
			return true;
		}
	}
	// For Angles, can only have xxx if xxx < 360
	else
	{
		// Check for min string length, yy digits, xx digits
		if ((rvecPos[0] > 4) && 
			isDigitChar(pszText[rvecPos[0]-1]) &&
			isDigitChar(pszText[rvecPos[0]-2]) &&
			isDigitChar(pszText[rvecPos[0]-4]) &&
			isDigitChar(pszText[rvecPos[0]-5]))
		{
			// If goofy character is a zero
			if (pszText[rvecPos[0]-3] == '0')
			{
				// First check if xxx are digits before the zero
				if ((rvecPos[0] > 5) && (isDigitChar(pszText[rvecPos[0]-6])))
				{
					// Degrees > 350 not valid, therefore zero should be replaced
					strInput.replace( rvecPos[0]-3, 1, string("°") );

					// Return
					return true;
				}
				// Just xx are digits and zero is present, check degree value
				else if (asLong( strInput.substr(rvecPos[0]-5, 2) ) > 35)
				{
					// Degrees > 350 not valid, therefore zero should be replaced
					strInput.replace( rvecPos[0]-3, 1, string("°") );

					// Return
					return true;
				}
			}
			// Goofy character is a digit, but not a zero
			else if (isDigitChar(pszText[rvecPos[0]-3]))
			{
				// xx and next char are digits, check degree value
				if (asLong( strInput.substr(rvecPos[0]-5, 3) ) > 359)
				{
					// Degrees > 359 not valid, therefore char should be replaced
					strInput.replace( rvecPos[0]-3, 1, string("°") );

					// Return
					return true;
				}
			}
			// Goofy character is not a digit
			else
			{
				// Just replace it
				strInput.replace( rvecPos[0]-3, 1, string("°") );

				// Return
				return true;
			}
		}
	}

	/////////////////////////
	// Handle xx°yyy'zz" case
	/////////////////////////
	// Check symbol count,  distance between symbols, string length, 
	// yyy digits, 1st z digit
	if ((lCount > 2) && 
		(rvecPos[1] - rvecPos[0] == 4) && 
		((long)ulLength > rvecPos[1]+2) && 
		isDigitChar(pszText[rvecPos[1]-1]) &&
		isDigitChar(pszText[rvecPos[1]-2]) &&
		isDigitChar(pszText[rvecPos[1]-3]) &&
		isDigitChar(pszText[rvecPos[1]+1]))
	{
		// Get xx° characters
		string	strNew = strInput.substr( 0, rvecPos[0]+1 );

		// Append yy'zz" characters
		strNew.append( strInput.substr( rvecPos[0]+2, ulLength - (rvecPos[0]+2) ) );

		// Replace original string with modified
		strInput = strNew;

		// Return
		return true;
	}

	////////////////////////
	// Handle xx°yy?zz" case
	////////////////////////
	// Check symbol count,  distance between symbols, yyy digits, zz digits
	if ((lCount > 1) && 
		(rvecPos[1] - rvecPos[0] == 6) && 
		isDigitChar(pszText[rvecPos[0]+1]) &&
		isDigitChar(pszText[rvecPos[0]+2]) &&
		isDigitChar(pszText[rvecPos[0]+4]) &&
		isDigitChar(pszText[rvecPos[0]+5]))
	{
		// Replace goofy character with minute symbol
		strInput.replace( rvecPos[0]+3, 1, string("'") );

		// Return
		return true;
	}

	// No changes made
	return false;
}
//--------------------------------------------------------------------------------------------------
void CMCRTextCorrector::locateSymbols(const string& strInput, std::vector<long>& rvecPos)
{
	long				lSymbolCount = 0;
	char*				pChar = NULL;

	// Make local copy of input string
	unsigned long	ulLength = strInput.length();
	char *			pszText = const_cast<char *>(strInput.data());
	unsigned long	ulIndex;
	bool			bDigit = false;

	// Check every letter for symbols
	// NOTE: Ignore any symbols without intervening digit(s)
	for (ulIndex = 0; ulIndex < ulLength; ulIndex++)
	{
		// Has a digit been seen since the last symbol?
		if (!bDigit)
		{
			if (isDigitChar( pszText[ulIndex] ))
			{
				bDigit = true;
			}
		}

		// Is this letter a symbol?
		if (isSymbolChar( pszText[ulIndex] ))
		{
			// Should it be added?
			if (bDigit)
			{
				// Add the index to the vector
				rvecPos.push_back( ulIndex );

				// Clear the flag
				bDigit = false;
			}
		}
	}
}

//--------------------------------------------------------------------------------------------------
void CMCRTextCorrector::stampSymbols(string& strInput, long lDegreePos, 
									 long lMinutePos, long lSecondPos)
{
	unsigned long	ulLength = strInput.length();

	// Force the degree symbol if index is valid
	if (lDegreePos != -1 && lDegreePos < (long)ulLength)
	{
		strInput.replace( lDegreePos, 1, string("°") );
	}

	// Force the minute symbol if index is valid
	if (lMinutePos != -1 && lMinutePos < (long)ulLength)
	{
		strInput.replace( lMinutePos, 1, string("'") );
	}

	// Force the second symbol if index is valid
	if (lSecondPos != -1 && lSecondPos < (long)ulLength)
	{
		strInput.replace( lSecondPos, 1, string("\"") );
	}
}
//--------------------------------------------------------------------------------------------------
void CMCRTextCorrector::preProcessAngleInput(string& strInput)
{
	// Angle cleanup is derived from Bearing cleanup - 10/07/02 WEL

	// Replace decimal point followed by single quote with single quote
	replaceVariable(strInput, ".'", "'");

	// Modified to count total symbols {°, ', ", ^, *, ~} instead of 
	// specific symbols - 10/10/02 WEL

	// Locate each symbol within the string
	std::vector<long> vecPos;
	locateSymbols( strInput, vecPos );

	// Check for OCR zeros that should be symbols
	if (fixedBadZeros( strInput, vecPos, true ))
	{
		// Flush the vector
		vecPos.clear();

		// Relocate the symbols
		locateSymbols( strInput, vecPos );
	}

	// Make local copy of input string
	unsigned long ulLength = strInput.length();
	char *pszText = const_cast<char *>(strInput.data());

	// Check number of located symbols
	long lCount = vecPos.size();
	switch (lCount)
	{
	case 3:
		/////////////////////
		// Partial validation
		/////////////////////
		// Check for xx°yy'zzz"
		if (vecPos[2] - vecPos[1] == 4)
		{
			// Check to see that each z is a digit
			bool bBadCharFound = false;
			for (int i = 1; i <= 3; i++)
			{
				if (!isDigitChar( pszText[vecPos[1]+i] ))
				{
					// Just remove this non-digit and keep the other two
					strInput.erase( vecPos[1]+i, 1 );
					bBadCharFound = true;
					break;
				}
			}

			// Check to see if any two of zzz are valid
			if (!bBadCharFound)
			{
				// Check first two digits
				if (asLong( strInput.substr( vecPos[1]+1, 2 ).c_str() ) > 59 )
				{
					// Remove the first z
					strInput.erase( vecPos[1]+1, 1 );
				}
				// Check second two digits
				else if (asLong( strInput.substr( vecPos[1]+2, 2 ).c_str() ) > 59 )
				{
					// Remove the third z
					strInput.erase( vecPos[1]+3, 1 );
				}
				// Check for first z being a zero
				else if( pszText[vecPos[1]+1] == '0' )
				{
					// Remove the zero
					strInput.erase( vecPos[1]+1, 1 );
				}
				// Just remove the last digit
				else
				{
					strInput.erase( vecPos[1]+3, 1 );
				}
			}

			// No matter what happened, the symbol locations are different
			vecPos.clear();
			locateSymbols( strInput, vecPos );
		}

		// Stamp DMS symbols in appropriate positions
		stampSymbols( strInput, vecPos[0], vecPos[1], vecPos[2] );
		break;

	case 2:
		// Handle xxyy'zz" case
		if (((vecPos[1] - vecPos[0]) == 3) && 
			(vecPos[0] > 3) && 
			isDigitChar(pszText[vecPos[0]-1]) &&
			isDigitChar(pszText[vecPos[0]-2]) &&
			isDigitChar(pszText[vecPos[0]-3]) &&
			isDigitChar(pszText[vecPos[0]-4]))
		{
			// Get xx characters
			string	strNew = strInput.substr( 0, vecPos[0]-2 );

			// Append the degree symbol
			strNew.append( "°" );

			// Append remaining original characters
			strNew.append( strInput.substr( vecPos[0]-2, ulLength - (vecPos[0]-2) ) );

			// Replace original string with modified
			strInput = strNew;

			// Stamp DMS symbols in appropriate positions
			stampSymbols( strInput, vecPos[0]-2, vecPos[0]+1, vecPos[1]+1 );
		}

		// Handle xx°yyzz" case
		else if (((vecPos[1] - vecPos[0]) == 5) && 
			isDigitChar(pszText[vecPos[0]+1]) &&
			isDigitChar(pszText[vecPos[0]+2]) &&
			isDigitChar(pszText[vecPos[0]+3]) &&
			isDigitChar(pszText[vecPos[0]+4]))
		{
			// Get xx°yy characters
			string	strNew = strInput.substr( 0, vecPos[0]+3 );

			// Append the minute symbol
			strNew.append( "'" );

			// Append remaining original characters
			strNew.append( strInput.substr( vecPos[0]+3, ulLength - (vecPos[0]+3) ) );

			// Replace original string with modified
			strInput = strNew;

			// Stamp DMS symbols in appropriate positions
			stampSymbols( strInput, vecPos[0], vecPos[0]+3, vecPos[1]+1 );
		}

		// Handle xx°yy'zz case AND xx'yy'zz.z case
		else if (((vecPos[1] - vecPos[0]) == 3) && 
			isDigitChar(pszText[vecPos[1]+1]) &&
			isDigitChar(pszText[vecPos[1]+2]))
		{
			// Get xx°yy'zz characters
			string	strNew = strInput.substr( 0, vecPos[1]+3 );

			// Check for subsequent .z
			if (((long)ulLength > vecPos[1] + 4) && (pszText[vecPos[1]+3] == '.'))
			{
				// Retrieve the decimal point and following digit
				strNew.append( strInput.substr( vecPos[1]+3, 2 ) );

				// Append the second symbol
				strNew.append( "\"" );

				// Replace original string with modified
				strInput = strNew;

				// Stamp DMS symbols in appropriate positions
				stampSymbols( strInput, vecPos[0], vecPos[1], vecPos[1]+5 );
			}
			else
			{
				// Append the second symbol
				strNew.append( "\"" );

				// Replace original string with modified
				strInput = strNew;

				// Stamp DMS symbols in appropriate positions
				stampSymbols( strInput, vecPos[0], vecPos[1], vecPos[1]+3 );
			}
		}
		else
		{
			// No additions or corrections found
			// Just stamp the two symbols
			stampSymbols( strInput, vecPos[0], vecPos[1], -1 );
		}

		break;

	case 1:
		// Handle x°yyzz case AND x°yyzz.z case www
		if ((vecPos[0] > 0) && 
			isDigitChar(pszText[vecPos[0]-1]) &&
			isDigitChar(pszText[vecPos[0]+1]) &&
			isDigitChar(pszText[vecPos[0]+2]) &&
			isDigitChar(pszText[vecPos[0]+3]) &&
			isDigitChar(pszText[vecPos[0]+4]))
		{
			// Get x°yy characters
			string	strNew = strInput.substr( 0, vecPos[0]+3 );

			// Append the minute symbol
			strNew.append( "'" );

			// Append the zz characters
			strNew.append( strInput.substr( vecPos[0]+3, 2 ) );

			// Check for subsequent .z
			if (((long)ulLength > vecPos[0] + 6) && (pszText[vecPos[0]+5] == '.'))
			{
				// Retrieve the decimal point and following digit
				strNew.append( strInput.substr( vecPos[0]+5, 2 ) );

				// Append the second symbol
				strNew.append( "\"" );

				// Replace original string with modified
				strInput = strNew;

				// Stamp DMS symbols in appropriate positions
				stampSymbols( strInput, vecPos[0], vecPos[0]+3, vecPos[0]+8 );
			}
			else
			{
				// Just append the second symbol
				strNew.append( "\"" );

				// Replace original string with modified
				strInput = strNew;

				// Stamp DMS symbols in appropriate positions
				stampSymbols( strInput, vecPos[0], vecPos[0]+3, vecPos[0]+6 );
			}
		}

		break;

	case 0:
		break;

	default:
		// If symbol count > 3, just use the first 3
		if (lCount > 3)
		{
			stampSymbols( strInput, vecPos[0], vecPos[1], vecPos[2] );
		}
		break;
	}
/*
	// In the following routine it is assumed that user will never select two cases together for  
	// bearings + direction
	{
		// Declarations for Symbol positions and counter for number of occurences
		int iSymbolPos1 = -1, iSymbolPos2 = -1, iSymbolPos3 = -1, iCountSymbol = 0;
	
		// Check every letter for degree, minute, second, asterisk symbols
		for (i = 0; i < ulLength; i++)
		{
			// Count and locate symbols
			if ((pszText[i] == '°') ||
				(pszText[i] == '\'') ||
				(pszText[i] == '"') ||
				(pszText[i] == '*'))
			{
				iCountSymbol++;

				if (iCountSymbol == 1)
				{
					iSymbolPos1 = i;
				}
				else if (iCountSymbol == 2)
				{
					iSymbolPos2 = i;
				}
				else if (iCountSymbol == 3)
				{
					iSymbolPos3 = i;
				}
			}
		}

		////////////////////////////
		// Handle symbol count cases
		////////////////////////////
		if (iCountSymbol == 2)
		{
			///////////////////////
			// Check for situation where we need to insert a symbol 
			// before the first symbol
			///////////////////////

			// Case: xxyy'zz"  --> xx°yy'zz" where x, x, are digits
			if (isDigitChar(pszText[iSymbolPos1-3]) && 
				isDigitChar(pszText[iSymbolPos1-4]))
			{
			}
			// Case: xx°yyzz"  --> xx°yy'zz" where y, y, are digits
			else if ()
			{
			}
			// Case: xx°yy'zz  --> xx°yy'zz" where z, z, are digits
			else if ()
			{
			}
			else
			{
				// We don't handle this case right now
				// except for two minutes or two seconds

				// Make the first symbol a degree symbol
				pszText[iSymbolPos1] = '°';

				// Change the second symbol to minute only if it is a degree
				// Otherwise, leave it alone
				if (pszText[iSymbolPos2] == '°')
				{
					pszText[iSymbolPos2] = '\'';
				}
			}
		}

		if (iCountSymbol == 3)
		{
			// Make sure that symbols are in the right order
			// of xx° yy' zz"
			pszText[iSymbolPos1] = '°';
			pszText[iSymbolPos2] = '\'';
			pszText[iSymbolPos3] = '"';
		}
*/
/*
		// Check for degree symbol recognizing as a zero
		if (iCountDegree == 0)
		{
			// Look for a zero three characters before the first minute symbol
			if ((iMinutePos1 > 3) && (pszText[iMinutePos1-3] == '0'))
			{
				// Replace the zero
				pszText[iMinutePos1 - 3] = '°';
				
				// Adjust other items
				iCountDegree = 1;
				iDegreePos1 = iMinutePos1 - 3;
			}
			// Look for a tilde instead of the minute symbol three characters 
			// in front of the second symbol.  Also, a zero three characters 
			// in front of the tilde
			else if ((iCountMinute == 0) && (iCountSecond == 1) && 
				(iSecondPos1 > 7) && (pszText[iSecondPos1-3] == '~') && 
				(pszText[iSecondPos1-6] == '0'))
			{
				// Replace the tilde and the zero
				pszText[iSecondPos1 - 3] = '\'';
				pszText[iSecondPos1 - 6] = '°';

				// Adjust other items
				iCountDegree = 1;
				iCountMinute = 1;
				iMinutePos1 = iSecondPos1 - 3;
				iDegreePos1 = iSecondPos1 - 6;
			}
		}

		// If two second symbols and one minute symbol are found 
		// then adjust such that degree, minute, second is result
		if ((iSecondPos2 != -1) && (iMinutePos1 != -1))
		{
			// Consider xx' yy" zz" case
			if (iSecondPos1 > iMinutePos1)
			{
				pszText[iMinutePos1] = '°';
				pszText[iSecondPos1] = '\'';
			}

			// Consider xx" yy' zz" case
			if (iSecondPos1 < iMinutePos1)
			{
				pszText[iSecondPos1] = '°';
			}
		}
		// If two degree symbols are found and one (minute OR second) 
		// adjust to xx° yy' zz"
		else if ((iDegreePos2 != -1) &&
			((iMinutePos1 != -1) || (iSecondPos1 != -1)))
		{
			// Consider xx° yy° zz" case
			if (iDegreePos2 < iSecondPos1)
			{
				// Just change second degree symbol to minute symbol
				pszText[iDegreePos2] = '\'';
			}

			// Consider xx° yy° zz' case
			else if (iDegreePos2 < iMinutePos1)
			{
				// Change second degree symbol to minute symbol
				// and minute symbol to second symbol
				pszText[iDegreePos2] = '\'';
				pszText[iMinutePos1] = '"';
			}

			// Consider xx' yy° zz° case
			else if (iMinutePos1 < iDegreePos1)
			{
				// Change minute symbol to degree symbol,
				// and first degree symbol to minute symbol
				// and second degree symbol to second symbol
				pszText[iMinutePos1] = '°';
				pszText[iDegreePos1] = '\'';
				pszText[iDegreePos2] = '"';
			}

			// Consider xx" yy° zz° case
			else if (iSecondPos1 < iDegreePos1)
			{
				// Change second symbol to degree symbol,
				// and first degree symbol to minute symbol
				// and second degree symbol to second symbol
				pszText[iSecondPos1] = '°';
				pszText[iDegreePos1] = '\'';
				pszText[iDegreePos2] = '"';
			}
		}
		// If three second symbols are found then replace first one with degree 
		// and second one with minute symbol
		else if (iSecondPos3 != -1)
		{
			pszText[iSecondPos1] = '°';
			pszText[iSecondPos2] = '\'';
		}
		// If two second symbols and one degree symbol are found then 
		// correct them to xx° yy' zz"
		else if ((iSecondPos2 != -1) && (iDegreePos1 != -1))
		{
			// Consider xx° yy" zz" case
			if (iDegreePos1 < iSecondPos1)
			{
				pszText[iSecondPos1] = '\'';
			}

			// Consider xx" yy° zz" case
			else if (iDegreePos1 < iSecondPos2)
			{
				pszText[iSecondPos1] = '°';
				pszText[iDegreePos1] = '\'';
			}

			// Consider xx" yy" zz° case
			else if (iSecondPos2 < iDegreePos1)
			{
				pszText[iSecondPos1] = '°';
				pszText[iSecondPos2] = '\'';
				pszText[iDegreePos1] = '"';
			}
		}
		// If just two second symbols are found then replace first one 
		// with degree symbol 
		else if (iSecondPos2 != -1)
		{
			pszText[iSecondPos1] = '°';
		}
		// If three minute symbols are found then replace first one with degree 
		// and third one with second symbol
		else if (iMinutePos3 != -1)
		{
			pszText[iMinutePos1] = '°';
			pszText[iMinutePos3] = '"';
		}
		// If degree is found and two minute symbols, 
		// then replace second minute symbol with second symbol
		// NOTE: Rearrange symbols as appropriate
		else if (iMinutePos2 != -1 && iDegreePos1 != -1)
		{
			// Consider xx° yy' zz' case
			if (iDegreePos1 < iMinutePos1)
			{
				pszText[iMinutePos2] = '"';	
			}
			// Consider xx' yy' zz° case
			else if (iMinutePos2 < iDegreePos1)
			{
				pszText[iMinutePos1] = '°';	
				pszText[iDegreePos1] = '"';	
			}
			// Consider xx' yy° zz' case
			else if (iMinutePos1 < iDegreePos1)
			{
				pszText[iMinutePos1] = '°';	
				pszText[iDegreePos1] = '\'';	
				pszText[iMinutePos2] = '"';	
			}
		}
		// If just two minute symbols are found, 
		// the xx' yy" and xx° yy" cases are explicitly ignored
		// in favor of the xx° yy' case
		else if (iMinutePos2 != -1 && iDegreePos1 == -1) 
		{
			pszText[iMinutePos1] = '°';	
		}
		// Check for one of each symbol but out of order
		// NOTE: Rearrange symbols as appropriate
		else if ((iDegreePos1 != -1) && (iMinutePos1 != -1) && 
			(iSecondPos1 != -1))
		{
			// Consider xx° yy" zz' case
			if ((iDegreePos1 < iSecondPos1) && 
				(iSecondPos1 < iMinutePos1))
			{
				pszText[iSecondPos1] = '\'';	
				pszText[iMinutePos1] = '"';	
			}

			// Consider xx' yy° zz" case
			if ((iMinutePos1 < iDegreePos1) && 
				(iDegreePos1 < iSecondPos1))
			{
				pszText[iMinutePos1] = '°';	
				pszText[iDegreePos1] = '\'';	
			}

			// Consider xx" yy° zz' case
			if ((iSecondPos1 < iDegreePos1) && 
				(iDegreePos1 < iMinutePos1))
			{
				pszText[iSecondPos1] = '°';	
				pszText[iDegreePos1] = '\'';	
				pszText[iMinutePos1] = '"';	
			}

			// Consider xx' yy" zz° case
			if ((iMinutePos1 < iSecondPos1) && 
				(iSecondPos1 < iDegreePos1))
			{
				pszText[iMinutePos1] = '°';	
				pszText[iSecondPos1] = '\'';	
				pszText[iDegreePos1] = '"';	
			}

			// Consider xx" yy' zz° case
			if ((iSecondPos1 < iMinutePos1) && 
				(iMinutePos1 < iDegreePos1))
			{
				pszText[iSecondPos1] = '°';	
				pszText[iMinutePos1] = '\'';	
				pszText[iDegreePos1] = '"';	
			}
		}
		*/
//	}
}
//-------------------------------------------------------------------------------------------------
bool CMCRTextCorrector::isKnowledgeableOfInputType(const std::string& strInputType)
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
void CMCRTextCorrector::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnICOMAP_CORE_OBJECTS;

	VALIDATE_LICENSE( THIS_COMPONENT_ID, "ELI03078", "MCR Text Corrector" );
}
//-------------------------------------------------------------------------------------------------
