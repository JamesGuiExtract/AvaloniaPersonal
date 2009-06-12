
#include "stdafx.h"
#include "ParagraphTextCorrectors.h"
#include "LegalDescriptionTextCorrector.h"

#include <cpputil.h>
#include <LicenseMgmt.h>
#include <UCLIDException.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CLegalDescriptionTextCorrector
//-------------------------------------------------------------------------------------------------
CLegalDescriptionTextCorrector::CLegalDescriptionTextCorrector()
{
}
//-------------------------------------------------------------------------------------------------
CLegalDescriptionTextCorrector::~CLegalDescriptionTextCorrector()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20406");
}
//-------------------------------------------------------------------------------------------------
inline bool charIsPartOfNumber(char cChar)
{
	return (cChar >= '0' && cChar <= '9') || cChar == '.';
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLegalDescriptionTextCorrector::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IParagraphTextCorrector,
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
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLegalDescriptionTextCorrector::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CLegalDescriptionTextCorrector::raw_CorrectText(ISpatialString *pTextToCorrect)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState())

		// ensure that this component is licensed.
		validateLicense();

		ISpatialStringPtr ipTextToCorrect(pTextToCorrect);
		ASSERT_RESOURCE_ALLOCATION("ELI19484", ipTextToCorrect != NULL);

		cleanupOCRedParagraphText(ipTextToCorrect);
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02773")
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CLegalDescriptionTextCorrector::removeWhiteSpaceInWord(ISpatialStringPtr ipText, 
															const string& strWord)
{
	long nTextLen = ipText->Size;
	if (nTextLen == 0)
	{
		return;
	}

	// make a copy of the input text, and convert the copy into
	// uppercase for sake of subsequent comparision
	ICopyableObjectPtr ipTemp = ipText;
	ASSERT_RESOURCE_ALLOCATION("ELI06534", ipTemp != NULL);
	ISpatialStringPtr ipUpperCaseText = ipTemp->Clone();
	ASSERT_RESOURCE_ALLOCATION("ELI06535", ipUpperCaseText != NULL);
	ipUpperCaseText->ToUpperCase();

	// make an uppercase copy of strWord for case-insensitive
	// comparision
	string strUpperCaseWord = strWord;
	makeUpperCase(strUpperCaseWord);

	// find an instance of the first char of the word we are looking for
	long lastPos;
	long currPos = ipUpperCaseText->FindFirstInstanceOfChar(strUpperCaseWord[0], 0);
	while (currPos != -1)
	{
		unsigned int ui = 1;
		int iNumSpaces = 0;
		do
		{
			if (currPos + iNumSpaces + (int)ui >= nTextLen)
			{
				break;
			}

			char cNextChar = (char)ipUpperCaseText->GetChar(currPos + iNumSpaces + ui);
			while (cNextChar == ' ' || cNextChar == '\t' || cNextChar == '\n' || cNextChar == '\r')
			{
				iNumSpaces++;
				
				if (currPos + iNumSpaces + (int)ui >= nTextLen)
				{
					break;
				}

				cNextChar = (char)ipUpperCaseText->GetChar(currPos + iNumSpaces + ui);
			}

			if (cNextChar == strUpperCaseWord[ui])
			{
				// we have found a match for the ith character.  Let's increment ui and try
				// to find a match for the next character in the word
				ui++;
			}
			else
			{
				// we have not found a match - exit this loop.
				break;
			}
		}
		while (ui < strUpperCaseWord.length());
		
		if (ui == strUpperCaseWord.length())
		{
			for (int i = 0; i < iNumSpaces; i++)
			{
				if (currPos >= nTextLen-1)
				{
					break;
				}
				
				int j = currPos + 1;
				char cNextChar = (char)ipUpperCaseText->GetChar(j);
				while (cNextChar != ' ' && cNextChar != '\t' && cNextChar != '\n' && cNextChar != '\r')
				{
					j++;
					
					cNextChar = (char)ipUpperCaseText->GetChar(j);
				}
				
				ipText->Remove(j, j);
				ipUpperCaseText->Remove(j, j);
				// update text len
				nTextLen = ipText->Size;
			}
		}

		if (currPos >= nTextLen - 1)
		{
			break;
		}
		// find the next instance of the first char of the word that we
		// are looking for
		lastPos = currPos;
		currPos = ipUpperCaseText->FindFirstInstanceOfChar(strUpperCaseWord[0], 
			lastPos + 1);
	}
}
//-------------------------------------------------------------------------------------------------
void CLegalDescriptionTextCorrector::cleanupOCRedParagraphText(ISpatialStringPtr ipText)
{
	long lstrLength = ipText->Size;
	if (lstrLength == 0)
	{
		return;
	}

	// replace all tabs with spaces
	_bstr_t _bstrSpace(" ");
	ipText->Replace("\t", _bstrSpace, VARIANT_TRUE, 0, NULL);

	// replace all consequtive instances of whitespace with a single space char
	ipText->ConsolidateChars(_bstrSpace, VARIANT_TRUE);

	// get rid of all leading and trailing spaces
	ipText->Trim(_bstrSpace, _bstrSpace);

	// first find all instances of spaces and see if the spaces
	// should be eliminated
	lstrLength = ipText->Size;
	if (lstrLength == 0)
	{
		// if there's no text at all, return
		return;
	}

	long lastPos;
	long currPos = ipText->FindFirstInstanceOfChar(' ', 0);
	while (currPos != -1)
	{
		// NOTE: because we have eliminated all leading whitespace earlier, and also got rid
		// of all multiple-spaces, we are guaranteed that whenever we find a space character, 
		// the previous or next character is not a space.

		bool bPreviousCharPartOfNumber = false;
		bool bNextCharPartOfNumber = false;

		// determine if the previous character is part of a number
		if (currPos > 0)
		{
			char cChar = (char)ipText->GetChar(currPos - 1);
			bPreviousCharPartOfNumber = charIsPartOfNumber(cChar);
		}

		// determine if the next character is part of a number
		if (currPos < lstrLength - 1)
		{
			char cChar = (char)ipText->GetChar(currPos + 1);
			bNextCharPartOfNumber = charIsPartOfNumber(cChar);
		}

		// if both the previous and next char is part of a number, then get rid of this
		// space
		if (bPreviousCharPartOfNumber && bNextCharPartOfNumber)
		{
			ipText->Remove(currPos, currPos);
			// update the actual string length
			lstrLength = ipText->Size;
		}

		// if current position is last char of the string, break out of the loop
		if (currPos == lstrLength - 1)
		{
			break;
		}

		// find the next space
		lastPos = currPos;
		currPos = ipText->FindFirstInstanceOfChar(' ', lastPos + 1);
	}

	// replace any 'S' character preceeded with a quote symbol and succeeded by a digit char
	// with a 5.  This is a common recognition problem.
	currPos = ipText->FindFirstInstanceOfChar('\'', 0);
	while (currPos != -1)
	{
		bool bNextCharIsAnS = false;
		bool bNextNextCharIsADigit = false;

		// determine if the next character is an 'S'
		if (currPos < lstrLength - 1)
		{
			char cChar = (char)ipText->GetChar(currPos + 1);
			bNextCharIsAnS = (cChar == 'S' || cChar == 's');
		}

		// determine if the next to next character is a digit
		if (currPos < lstrLength - 2)
		{
			char cChar = (char)ipText->GetChar(currPos + 2);
			bNextNextCharIsADigit = (cChar >= '0' && cChar <= '9');
		}

		// if the next char is an S and the next-next char is a digit, then
		// convert the S into a 5
		if (bNextCharIsAnS && bNextNextCharIsADigit)
		{
			ipText->SetChar(currPos + 1, '5');
		}

		// if current position is last char of the string, break out of the loop
		if (currPos == lstrLength - 1)
		{
			break;
		}

		// find the next space
		lastPos = currPos;
		currPos = ipText->FindFirstInstanceOfChar('\'', lastPos + 1);
	}

	// replace any character O's next to a digit by a zero
	currPos = ipText->FindFirstInstanceOfChar('O', 0); // find uppper-case O
	while (currPos != -1)
	{
		bool bNextCharIsADigit = false;
		bool bPrevCharIsADigit = false;

		// determine if the next character is a digit
		if (currPos < lstrLength - 1)
		{
			char cChar = (char)ipText->GetChar(currPos + 1);
			bNextCharIsADigit = (cChar >= '0' && cChar <= '9');
		}

		// determine if the previous char is a digit
		if (currPos > 0)
		{
			char cChar = (char)ipText->GetChar(currPos - 1);
			bPrevCharIsADigit = (cChar >= '0' && cChar <= '9');
		}

		// if the next char is an S and the next-next char is a digit, then
		// convert the S into a 5
		if (bNextCharIsADigit || bPrevCharIsADigit)
		{
			ipText->SetChar(currPos, '0');
		}

		// if current position is last char of the string, break out of the loop
		if (currPos == lstrLength - 1)
		{
			break;
		}

		// find the next space
		lastPos = currPos;
		currPos = ipText->FindFirstInstanceOfChar('O', lastPos + 1); // find uppper-case O
	}

	// replace any character T's next to a digit by a 7
	currPos = ipText->FindFirstInstanceOfChar('T', 0); // find uppper-case T
	while (currPos != string::npos)
	{
		bool bNextCharIsADigit = false;
		bool bPrevCharIsADigit = false;

		// Determine if the next character is a digit
		if (currPos < lstrLength - 1)
		{
			char cChar = (char)ipText->GetChar(currPos + 1);
			bNextCharIsADigit = isDigitChar(cChar);
		}

		// Determine if the previous char is a digit
		if (currPos > 0)
		{
			char cChar = (char)ipText->GetChar(currPos - 1);
			bPrevCharIsADigit = isDigitChar(cChar);
		}

		// Do the conversion
		if (bNextCharIsADigit || bPrevCharIsADigit)
		{
			ipText->SetChar(currPos, '7');
		}

		// if current position is last char of the string, break out of the loop
		if (currPos == lstrLength - 1)
		{
			break;
		}

		// find the next T
		lastPos = currPos;
		currPos = ipText->FindFirstInstanceOfChar('T', lastPos + 1); // find uppper-case T
	}

	removeWhiteSpaceInWord(ipText, "East");
	removeWhiteSpaceInWord(ipText, "West");
	removeWhiteSpaceInWord(ipText, "South");
	removeWhiteSpaceInWord(ipText, "North");
}
//-------------------------------------------------------------------------------------------------
void CLegalDescriptionTextCorrector::validateLicense()
{
	static const unsigned long ulTHIS_COMPONENT_ID = gnICOMAP_CORE_OBJECTS;

	VALIDATE_LICENSE( ulTHIS_COMPONENT_ID, "ELI02777",
		"Legal Description Text Corrector" );
}
//-------------------------------------------------------------------------------------------------
