// ExtractLine.cpp : Implementation of CExtractLine
#include "stdafx.h"
#include "AFValueFinders.h"
#include "ExtractLine.h"

#include <UCLIDException.h>
#include <COMUtils.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CExtractLine
//-------------------------------------------------------------------------------------------------
CExtractLine::CExtractLine()
: m_bDirty(false),
  m_bEachLineAsUniqueValue(true),
  m_bIncludeLineBreak(false),
  m_strLineNumbers("")
{
	try
	{
		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI13040", m_ipMiscUtils != NULL );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05427")
}
//-------------------------------------------------------------------------------------------------
CExtractLine::~CExtractLine()
{
	try
	{
		m_ipMiscUtils = NULL;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16343");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CExtractLine::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IAttributeFindingRule,
		&IID_ICategorizedComponent,
		&IID_IExtractLine,
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
STDMETHODIMP CExtractLine::raw_ParseText(IAFDocument* pAFDoc, IProgressStatus *pProgressStatus,
										 IIUnknownVector **pAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// Create vector for resulting Attributes
		IIUnknownVectorPtr ipAttributes( CLSID_IUnknownVector );

		// Create local Document pointer
		IAFDocumentPtr ipAFDoc( pAFDoc );

		// Get the text out from the spatial string
		ISpatialStringPtr ipInputText( ipAFDoc->Text );

		// get the input string from the spatial string
		_bstr_t _bstrText( ipInputText->String );

		// divide input string into individual lines including 
		// the line break at the end of each line
		IRegularExprParserPtr ipParser = getParser("[\\s\\S]*?(\\r\\n|\\n|\\r|(?=\\r*$))");

		// all lines are stored separated in the vector
		IIUnknownVectorPtr ipLines = ipParser->Find( _bstrText, VARIANT_FALSE, VARIANT_FALSE );
		long nNumOfLines = ipLines->Size();

		long nStart, nEnd;
		if (m_bEachLineAsUniqueValue)
		{
			// Store each line as the value for current attribute
			for (long n = 0; n < nNumOfLines; n++)
			{
				// Retrieve this line
				IObjectPairPtr ipObjPair( ipLines->At(n) );
				ITokenPtr ipSingleItem( ipObjPair->Object1 );
				if (ipSingleItem)
				{
					// Get the token information
					nStart = ipSingleItem->StartPosition;
					nEnd = ipSingleItem->EndPosition;

					// Get the associated ISpatialString
					ISpatialStringPtr	ipSingleLine = ipInputText->GetSubString( nStart, nEnd );
					ASSERT_RESOURCE_ALLOCATION( "ELI06585", ipSingleLine != NULL );

					// Remove any line breaks
					ipSingleLine->Replace("\n", "", VARIANT_FALSE, 0, NULL);
					ipSingleLine->Replace("\r", "", VARIANT_FALSE, 0, NULL);

					// Create an attribute to store the value
					IAttributePtr ipAttribute( CLSID_Attribute );
					ASSERT_RESOURCE_ALLOCATION( "ELI06586", ipAttribute != NULL );
					ipAttribute->Value = ipSingleLine;
					// Add the Attribute to the vector
					ipAttributes->PushBack( ipAttribute );
				}
			}
		}
		else
		{
			// combine all specified lines into one value with line breaks first
			ISpatialStringPtr	ipCombined( CLSID_SpatialString );
			ASSERT_RESOURCE_ALLOCATION( "ELI06587", ipCombined != NULL );

			for (unsigned int ui = 0; ui < m_vecLineNumbers.size(); ui++)
			{
				// note that line numbers are 1-based, translate it into 0-based
				long nLineNumber = m_vecLineNumbers[ui] - 1;

				// make sure the line number from the vec doesn't 
				// exceed the total number of lines
				if (nLineNumber >= nNumOfLines)
				{
					break;
				}

				// Retrieve this line
				IObjectPairPtr ipObjPair( ipLines->At( nLineNumber ) );
				ITokenPtr ipEachItem( ipObjPair->Object1 );
				if (ipEachItem)
				{
					nStart = ipEachItem->StartPosition;
					nEnd = ipEachItem->EndPosition;

					// Get the associated ISpatialString
					ISpatialStringPtr	ipEachLine = ipInputText->GetSubString( nStart, nEnd );
					ASSERT_RESOURCE_ALLOCATION( "ELI06588", ipEachLine != NULL );

					// Append this SpatialString to the others
					ipCombined->Append( ipEachLine );
				}
			}

			// Remove any line breaks
			if (!m_bIncludeLineBreak)
			{
				ipCombined->Replace("\n", "", VARIANT_FALSE, 0, NULL);
				ipCombined->Replace("\r", "", VARIANT_FALSE, 0, NULL);
			}

			// Only create attribute if ipCombined has a size > 0
			// FlexIDSCore #3181
			if (ipCombined->Size > 0)
			{
				// Create an Attribute to store the value
				IAttributePtr ipAttribute( CLSID_Attribute );
				ASSERT_RESOURCE_ALLOCATION( "ELI06589", ipAttribute != NULL );
				ipAttribute->Value = ipCombined;

				// Add the Attribute to the vector
				ipAttributes->PushBack( ipAttribute );
			}
		}

		// Provide the collected Attributes to the caller
		*pAttributes = ipAttributes.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05405");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CExtractLine::raw_IsConfigured(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		bool bConfigured = true;

		if (!m_bEachLineAsUniqueValue)
		{
			if (m_vecLineNumbers.size() == 0)
			{
				bConfigured = false;
			}
		}

		*pbValue = bConfigured ? VARIANT_TRUE: VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05406");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CExtractLine::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19578", pstrComponentDescription != NULL)

		*pstrComponentDescription = _bstr_t("Extract lines").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05407")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CExtractLine::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CExtractLine::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		UCLID_AFVALUEFINDERSLib::IExtractLinePtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI08247", ipSource!=NULL);

		m_bEachLineAsUniqueValue = (ipSource->GetEachLineAsUniqueValue()==VARIANT_TRUE) ? true : false;

		m_bIncludeLineBreak = (ipSource->GetIncludeLineBreak()==VARIANT_TRUE) ? true : false;

		m_strLineNumbers = ipSource->GetLineNumbers();

		// Parse line numbers
		if (!m_strLineNumbers.empty())
		{
			parseLineNumbers( m_strLineNumbers );
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08248");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CExtractLine::raw_Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy(CLSID_ExtractLine);
		ASSERT_RESOURCE_ALLOCATION("ELI08344", ipObjCopy != NULL);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05409");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CExtractLine::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_ExtractLine;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CExtractLine::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CExtractLine::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		m_bEachLineAsUniqueValue = true;
		m_bIncludeLineBreak = false;
		m_strLineNumbers = "";
		
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
			UCLIDException ue( "ELI07645", "Unable to load newer ExtractLine Finder." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		if (nDataVersion >= 1)
		{
			dataReader >> m_bEachLineAsUniqueValue;
			dataReader >> m_bIncludeLineBreak;
			dataReader >> m_strLineNumbers;

			// parse the string of line numbers into a vector
			parseLineNumbers(m_strLineNumbers);
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05411");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CExtractLine::Save(IStream *pStream, BOOL fClearDirty)
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
		dataWriter << m_bEachLineAsUniqueValue;
		dataWriter << m_bIncludeLineBreak;
		dataWriter << m_strLineNumbers;
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05410");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CExtractLine::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// IExtractLine
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CExtractLine::get_EachLineAsUniqueValue(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_bEachLineAsUniqueValue ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05413");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CExtractLine::put_EachLineAsUniqueValue(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bEachLineAsUniqueValue = newVal == VARIANT_TRUE;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05414");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CExtractLine::get_LineNumbers(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = _bstr_t(m_strLineNumbers.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05415");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CExtractLine::put_LineNumbers(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		string strLineNumbers = asString( newVal );

		// parse the string into individual line numbers 
		// and store in the vector
		parseLineNumbers(strLineNumbers);

		m_strLineNumbers = strLineNumbers;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05416");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CExtractLine::get_IncludeLineBreak(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_bIncludeLineBreak ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05428");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CExtractLine::put_IncludeLineBreak(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bIncludeLineBreak = newVal == VARIANT_TRUE;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05429");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CExtractLine::parseLineNumbers(const string& strLineNumbers)
{
	if (m_bEachLineAsUniqueValue)
	{
		return;
	}

	if (strLineNumbers.empty())
	{
		throw UCLIDException("ELI05833", "Please specify at least one number to represent "
			"the line you wish to extract.");
	}

	// pattern for any invalid character sets
	IRegularExprParserPtr ipParser = getParser(
				   "((^|\\D)-\\d+)"		// any negative numbers
				  "|(\\d+(,|-)(?=\\r*$))"		// any digits followed by , or -
				  "|(^|,|-)(,|-)"		// consecutive - or ,
				  "|(-\\d+-)"			// any digits with - on both sides
				  "|[^0-9,-]"			// any non-digit characters excluding , and -
				  "|\\b0");				// any word starts with 0

	IIUnknownVectorPtr ipNonDigitChars = 
		ipParser->Find(_bstr_t(strLineNumbers.c_str()), VARIANT_FALSE, VARIANT_FALSE);
	if (ipNonDigitChars != NULL && ipNonDigitChars->Size() > 0)
	{
		UCLIDException ue("ELI05834", "Invalid line numbers are defined.");
		ue.addDebugInfo("Invalid line numbers", strLineNumbers);
		throw ue;
	}

	m_vecLineNumbers.clear();
	ipParser->Pattern = "(\\d+)(\\s?-\\s?(\\d+))?";
	// get top matches, which might be some numbers and some number ranges
	IIUnknownVectorPtr ipTopMatches = 
		ipParser->Find(_bstr_t(strLineNumbers.c_str()), VARIANT_FALSE, VARIANT_TRUE);
	if (ipTopMatches != NULL)
	{
		// look at each top match's sub matches
		long nNumOfTopMatches = ipTopMatches->Size();
		for (long n=0; n<nNumOfTopMatches; n++)
		{
			IObjectPairPtr ipObjectPair = ipTopMatches->At(n);
			IIUnknownVectorPtr ipSubMatches = ipObjectPair->Object2;
			if (ipSubMatches)
			{
				// take a look at each sub match value, we are only
				// interested in sub match #1 and sub match #3
				// strStartLineNumber is the starting line number for the 
				// line number range. strEndLineNumber is the end line number
				// for the range, this string could be empty
				string strStartLineNumber(""), strEndLineNumber("");
				ITokenPtr ipSubMatch1(ipSubMatches->At(0));
				if (ipSubMatch1)
				{
					strStartLineNumber = ipSubMatch1->Value;
				}
				ITokenPtr ipSubMatch3(ipSubMatches->At(2));
				if (ipSubMatch3)
				{
					strEndLineNumber = ipSubMatch3->Value;
					if (strEndLineNumber.empty())
					{
						strEndLineNumber = strStartLineNumber;
					}
				}
				
				// in case the range is actually in a reverse order, 
				// for example, 2-4, 4-2 are both valid
				long nStartLineNumber = asLong(strStartLineNumber);
				long nEndLineNumber = asLong(strEndLineNumber);
				long m = nStartLineNumber;
				while (true)
				{
					// store line numbers in the vec
					m_vecLineNumbers.push_back(m);
					
					if (nStartLineNumber >= nEndLineNumber)
					{
						m--;
						if (m < nEndLineNumber)
						{
							break;
						}
					}
					else
					{
						m++;
						if (m > nEndLineNumber)
						{
							break;
						}
					}
					
				}
			}
		}
	}
}
//-------------------------------------------------------------------------------------------------
IRegularExprParserPtr CExtractLine::getParser(const string& strPattern)
{
	try
	{
		// Get the parser and set the pattern
		IRegularExprParserPtr ipParser = m_ipMiscUtils->GetNewRegExpParserInstance("BoxFinder");
		ASSERT_RESOURCE_ALLOCATION("ELI22433", ipParser);

		ipParser->Pattern = strPattern.c_str();

		return ipParser;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29474");
}
//-------------------------------------------------------------------------------------------------
void CExtractLine::validateLicense()
{
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI05392", "Extract Line Rule" );
}
//-------------------------------------------------------------------------------------------------
