// StringTokenizerSplitter.cpp : Implementation of CStringTokenizerSplitter
#include "stdafx.h"
#include "AFSplitters.h"
#include "StringTokenizerSplitter.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <COMUtils.h>
#include <cpputil.h>
#include <ComponentLicenseIDs.h>

#include <algorithm>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 2;

//-------------------------------------------------------------------------------------------------
// CStringTokenizerSplitter
//-------------------------------------------------------------------------------------------------
CStringTokenizerSplitter::CStringTokenizerSplitter()
: m_bDirty(false)
{
	try
	{
		// reset variables
		resetVariablesToDefault();

		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI29469", m_ipMiscUtils != __nullptr);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29470");
}
//-------------------------------------------------------------------------------------------------
CStringTokenizerSplitter::~CStringTokenizerSplitter()
{
	try
	{
		m_ipMiscUtils = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16329");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerSplitter::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IAttributeSplitter,
		&IID_IStringTokenizerSplitter,
		&IID_IPersistStream,
		&IID_ICopyableObject,
		&IID_ICategorizedComponent,
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
// IStringTokenizerSplitter
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerSplitter::get_Delimiter(short *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// validate license
		validateLicense();

		*pVal = m_cDelimiter;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05378")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerSplitter::put_Delimiter(short newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// validate license
		validateLicense();			

		// Accept any printable character including ( š ) (P16 #1950)
		if (!isPrintableChar( (unsigned char)newVal ))
		{
			// Convert delimiter character to hex string
			char pszTemp[3];
			sprintf_s( pszTemp, sizeof(pszTemp), "%02X", (unsigned char)newVal );

			// Create and throw exception
			UCLIDException ue("ELI05380", "Invalid delimiter!");
			ue.addDebugInfo( "Delimiter", pszTemp );
			throw ue;
		}
		
		m_cDelimiter = (char) newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05379")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerSplitter::get_SplitType(EStringTokenizerSplitType *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// validate license
		validateLicense();

		*pVal = m_eSplitType;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05381")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerSplitter::put_SplitType(EStringTokenizerSplitType newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// validate license
		validateLicense();

		m_eSplitType = newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05382")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerSplitter::get_FieldNameExpression(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// validate license
		validateLicense();

		*pVal = _bstr_t(m_strFieldNameExpression.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05383")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerSplitter::put_FieldNameExpression(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// validate license
		validateLicense();

		// validate the field name expression
		string strTemp = asString(newVal);
		if (!containsNonWhitespaceChars(strTemp))
		{
			throw UCLIDException("ELI05794", "The field name expression must contain non-whitespace characters!");
		}
		validateFieldNameExpression(strTemp);

		// update the internal variable
		m_strFieldNameExpression = strTemp;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19177")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerSplitter::get_AttributeNameAndValueExprVector(IIUnknownVector* *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// validate license
		validateLicense();

		// if the vector has not yet been created, create it
		if (m_ipVecNameValuePair == __nullptr)
		{
			m_ipVecNameValuePair.CreateInstance(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI05386", m_ipVecNameValuePair != __nullptr);
		}

		CComQIPtr<IIUnknownVector> ipObj = m_ipVecNameValuePair;
		*pVal = ipObj.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05385")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerSplitter::put_AttributeNameAndValueExprVector(IIUnknownVector *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// validate license
		validateLicense();

		IIUnknownVectorPtr ipVecNameValuePair = newVal;
		ASSERT_RESOURCE_ALLOCATION("ELI05797", ipVecNameValuePair != __nullptr);

		// validate the entries
		long nItems = ipVecNameValuePair->Size();
		for (int i = 0; i < nItems; i++)
		{
			IStringPairPtr ipStringPair = ipVecNameValuePair->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI19178", ipStringPair != __nullptr);

			// ensure that the name contains non-whitespace chars
			string strName = ipStringPair->StringKey;
			try
			{
				validateSubAttributeName(strName);
			}
			catch (UCLIDException& ue)
			{
				ue.addDebugInfo("index", asString(i));
				throw ue;
			}

			// ensure that the sub-attribute-value expression is a valid expression
			string strValue = ipStringPair->StringValue;
			try
			{
				validateSubAttributeValueExpression(strValue);
			}
			catch (UCLIDException& ue)
			{
				ue.addDebugInfo("index", asString(i));
				throw ue;
			}
		}

		// update the internal variable
		m_ipVecNameValuePair = ipVecNameValuePair;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05384")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerSplitter::IsValidSubAttributeValueExpression(BSTR strExpr, VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		string stdstrExpr = asString(strExpr);
		validateSubAttributeValueExpression(stdstrExpr);
		*pbValue = VARIANT_TRUE;
		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05798")

	*pbValue = VARIANT_FALSE;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerSplitter::IsValidSubAttributeName(BSTR strName, VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		string stdstrName = asString(strName);
		validateSubAttributeName(stdstrName);
		*pbValue = VARIANT_TRUE;
		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05800")

	*pbValue = VARIANT_FALSE;

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IAttributeSplitter
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerSplitter::raw_SplitAttribute(IAttribute *pAttribute, 
														  IAFDocument *pAFDoc,
														  IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		IAttributePtr ipAttribute(pAttribute);
		ASSERT_RESOURCE_ALLOCATION("ELI05398", ipAttribute != __nullptr);
		ISpatialStringPtr ipValue = ipAttribute->Value;
		ASSERT_RESOURCE_ALLOCATION("ELI20000", ipValue != __nullptr);
		IIUnknownVectorPtr ipSubs = ipAttribute->SubAttributes;
		ASSERT_RESOURCE_ALLOCATION("ELI15571", ipSubs != __nullptr);

		// tokenize the string
		CString zDelimiter(m_cDelimiter);
		IIUnknownVectorPtr ipTokens = ipValue->Tokenize(_bstr_t(zDelimiter));
		ASSERT_RESOURCE_ALLOCATION("ELI20001", ipTokens != __nullptr);
		long nNumTokens = ipTokens->Size();

		// depending upon the split type, create the sub attributes
		if (m_eSplitType == kEachTokenAsSubAttribute)
		{
			// create each of the tokens as sub-attributes
			for (int i = 0; i < nNumTokens; i++)
			{
				CString zTemp;
				// We want the user to see the sub attribute name
				// as 1-based.
				zTemp.Format(m_strFieldNameExpression.c_str(), i+1);
				string strName = (LPCTSTR) zTemp;				

				// create an IAttribute
				IAttributePtr ipSubAttribute(CLSID_Attribute);
				ASSERT_RESOURCE_ALLOCATION("ELI05397", ipSubAttribute != __nullptr);
				ipSubAttribute->Name = _bstr_t(strName.c_str());
				ISpatialStringPtr ipSubAttrValue = ipTokens->At(i);
				ipSubAttribute->Value = ipSubAttrValue;

				// Add to the sub-attributes
				ipSubs->PushBack(ipSubAttribute);
			}
		}
		else
		{
			// create each of the specified sub-attributes
			long nNumSubAttributes = m_ipVecNameValuePair->Size();

			// Get a regex parser with the pattern we'll be using this object to search for
			IRegularExprParserPtr ipParser = getRegExParser("%\\d+");

			for (int i = 0; i < nNumSubAttributes; i++)
			{
				IStringPairPtr ipStringPair = m_ipVecNameValuePair->At(i);
				ASSERT_RESOURCE_ALLOCATION("ELI20002", ipStringPair != __nullptr);

				// get the value expression string and replace %1 with the first
				// token, %2 with second token, etc.
				string strValue = asString(ipStringPair->StringValue);

				// find all instances of % followed by a number in the value
				// expression string
				IIUnknownVectorPtr ipVecTokens;
				ipVecTokens = ipParser->Find(strValue.c_str(), VARIANT_FALSE, 
					VARIANT_FALSE);
				ASSERT_RESOURCE_ALLOCATION("ELI05400", ipVecTokens != __nullptr);

				// vector to store token indices
				vector<int> vecTokensToBeReplaced;
				long nInstances = ipVecTokens->Size();
				for (int j = 0; j < nInstances; j++)
				{
					IObjectPairPtr ipObjPair = ipVecTokens->At(j);
					ASSERT_RESOURCE_ALLOCATION("ELI20003", ipObjPair != __nullptr);
					ITokenPtr ipToken = ipObjPair->Object1;
					ASSERT_RESOURCE_ALLOCATION("ELI20004", ipToken != __nullptr);
					_bstr_t _bstrTokenValue = ipToken->Value;

					// determine the token number
					string strTokenNumber = _bstrTokenValue;
					replaceVariable(strTokenNumber, "%", "");
					vecTokensToBeReplaced.push_back(asLong(strTokenNumber));
				}

				// sort the tokens to be replaced in descending order
				// to ensure integrity of replacements (i.e. %10 gets replaced
				// before %1)
				sort(vecTokensToBeReplaced.begin(), vecTokensToBeReplaced.end());

				IIUnknownVectorPtr ipVecRasterZones(CLSID_IUnknownVector);
				ASSERT_RESOURCE_ALLOCATION("ELI20208", ipVecRasterZones != __nullptr);
				
				// iterate through all the instances of % followed by a number
				// and do the search-replace with token values
				vector<int>::const_iterator iter = vecTokensToBeReplaced.begin();
				for (; iter != vecTokensToBeReplaced.end(); iter++)
				{
					long nTokenIndex = *iter;
					CString zTemp;
					zTemp.Format("%%%d", nTokenIndex);
					string strToFind = (LPCTSTR) zTemp;
					// all indices contained in vecTokenToBeReplaced are
					// 1-based, we need to convert it to 0-based so that
					// we'll get the correct corresponding token from StringTokenizer
					nTokenIndex--;
					string strToReplaceWith = "";
					
					// if the specified token number is one of the available
					// tokens, then we want to replace the %{number} with
					// the actual token value
					if (nTokenIndex <= nNumTokens - 1)
					{
						ISpatialStringPtr ipReplacement = ipTokens->At(nTokenIndex);
						ASSERT_RESOURCE_ALLOCATION("ELI20005", ipReplacement != __nullptr);
						strToReplaceWith = ipReplacement->String;

						// append the spatial information for this string, if it exists [P16 #2699]
						if(ipReplacement->HasSpatialInfo() == VARIANT_TRUE)
						{
							ipVecRasterZones->Append( ipReplacement->GetOCRImageRasterZones() );
						}
					}

					replaceVariable(strValue, strToFind, strToReplaceWith);
				}
				
				// create a subattribute if and only if a token was found
				if( !strValue.empty() )
				{
					// create an IAttribute
					IAttributePtr ipSubAttribute(CLSID_Attribute);
					ASSERT_RESOURCE_ALLOCATION("ELI05401", ipSubAttribute != __nullptr);
					ipSubAttribute->Name = ipStringPair->StringKey;

					// store the value of the subattribute
					ISpatialStringPtr ipSubValue = ipSubAttribute->Value;
					ASSERT_RESOURCE_ALLOCATION("ELI20006", ipSubValue != __nullptr);

					// check if the original attribute's value is spatial
					if(ipValue->HasSpatialInfo() == VARIANT_TRUE)
					{
						// if the original attribute's value is spatial, the result is hybrid
						ipSubValue->CreateHybridString(ipVecRasterZones, strValue.c_str(), 
							ipValue->SourceDocName, ipValue->SpatialPageInfos);
					}
					else
					{
						// the original attribute has no spatial info, the result is non-spatial
						ipSubValue->CreateNonSpatialString(strValue.c_str(), ipValue->SourceDocName);
					}
				
					// Add to the sub-attributes
					ipSubs->PushBack( ipSubAttribute );
				}
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05306")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerSplitter::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19567", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Split attributes using tokens").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05296")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerSplitter::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		// copy the delimiter
		UCLID_AFSPLITTERSLib::IStringTokenizerSplitterPtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI08236", ipSource != __nullptr);

		m_cDelimiter = (char)ipSource->GetDelimiter();

		// copy the split type
		m_eSplitType = (EStringTokenizerSplitType)ipSource->GetSplitType();

		// copy the rest of the member variables depending upon the split type
		switch(m_eSplitType)
		{
		case kEachTokenAsSubAttribute:
			// copy the field name expression
			m_strFieldNameExpression = asString(ipSource->GetFieldNameExpression());
			break;
		
		case kEachTokenAsSpecified:
			{
				// create a copy of the attribute name-to-value expression object
				// and store in this object
				IIUnknownVectorPtr ipVector = ipSource->GetAttributeNameAndValueExprVector();
				if (ipVector)
				{
					ICopyableObjectPtr ipCopyableObj = ipVector;
					ASSERT_RESOURCE_ALLOCATION("ELI08237", ipCopyableObj != __nullptr);
					m_ipVecNameValuePair = ipCopyableObj->Clone();
				}
				break;
			}
		
		default:
			break;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08239")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerSplitter::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance(CLSID_StringTokenizerSplitter);
		ASSERT_RESOURCE_ALLOCATION("ELI05297", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05298");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerSplitter::raw_IsConfigured(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		bool bConfigured = false;

		if (m_cDelimiter != 0 &&
		   ((m_eSplitType == kEachTokenAsSubAttribute && !m_strFieldNameExpression.empty()) ||
			(m_eSplitType == kEachTokenAsSpecified && m_ipVecNameValuePair != __nullptr && 
			m_ipVecNameValuePair->Size()>0)) )
		{
			bConfigured = true;
		}

		*pbValue = bConfigured ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05769");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerSplitter::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_StringTokenizerSplitter;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerSplitter::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
// NOTES about versions:
// Version 1:
//   * Saved the data version, and that's it.  Was pretty useless.
// Version 2:
//   * Saved the delimiter, the split type, and the field-name-expression (only
//     if the split type was kEachTokenAsSubAttribute), and 
//	   the attribute-name-and-value-expressoin vector (only if the split type was
//	   kEachTokenAsSpecified)
STDMETHODIMP CStringTokenizerSplitter::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Reset all the member variables
		resetVariablesToDefault();

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
			UCLIDException ue( "ELI07637", "Unable to load newer StringTokenizerSplitter." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		if (nDataVersion >= 2)
		{
			unsigned long ulTemp;
			dataReader >> ulTemp;
			m_cDelimiter = (char) ulTemp;
			dataReader >> ulTemp;
			m_eSplitType = (EStringTokenizerSplitType) ulTemp;
			
			// if the current split type is kEachTokenAsSubAttribute then
			// write the field name expression to the bytestream
			if (m_eSplitType == kEachTokenAsSubAttribute)
			{
				dataReader >> m_strFieldNameExpression;
			}
			else
			{
				IPersistStreamPtr ipObj;
				readObjectFromStream(ipObj, pStream, "ELI09962");
				if (ipObj == __nullptr)
				{
					throw UCLIDException( "ELI05396", 
						"Sub-Attribute name-and-value-expression vector could not be read from stream!" );
				}
				m_ipVecNameValuePair = ipObj;
			}
		}
		
		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05299");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerSplitter::Save(IStream *pStream, BOOL fClearDirty)
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
		dataWriter << (unsigned long) m_cDelimiter;
		dataWriter << (unsigned long) m_eSplitType;
		
		// if the current split type is kEachTokenAsSubAttribute then
		// write the field name expression to the bytestream
		if (m_eSplitType == kEachTokenAsSubAttribute)
		{
			dataWriter << m_strFieldNameExpression;
		}

		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write( &nDataLength, sizeof(nDataLength), NULL );
		pStream->Write( data.getData(), nDataLength, NULL );

		// if the current split type is kEachTokenAsSpecified then
		// write the attribute-name and value-expression vector to the stream
		if (m_eSplitType == kEachTokenAsSpecified)
		{
			IPersistStreamPtr ipPersistentObj = m_ipVecNameValuePair;
			if (ipPersistentObj == __nullptr)
			{
				throw UCLIDException("ELI05395", "Sub-Attribute name-and-value-expression vector does not support persistence!");
			}
			writeObjectToStream(ipPersistentObj, pStream, "ELI09917", fClearDirty);
		}

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05300");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerSplitter::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerSplitter::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// private / helper methods
//-------------------------------------------------------------------------------------------------
void CStringTokenizerSplitter::validateLicense()
{
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI05276", "String Tokenizer Splitter");
}
//-------------------------------------------------------------------------------------------------
void CStringTokenizerSplitter::validateSubAttributeName(const string& strName)
{
	if (!containsNonWhitespaceChars(strName))
	{
		UCLIDException ue("ELI05787", "Sub-attribute names must contain non-whitespace characters!");
		ue.addDebugInfo("Sub attribute name", strName);
		throw ue;
	}

	// Validate the sub-attribute name
	validateIdentifier( strName );
}
//-------------------------------------------------------------------------------------------------
void CStringTokenizerSplitter::validateSubAttributeValueExpression(const string& strExpr)
{
	// the sub-attribute value token expression must contain non-whitespace chars
	if (!containsNonWhitespaceChars(strExpr))
	{
		UCLIDException ue("ELI05801", "Token expressions must contain non-whitespace characters!");
		ue.addDebugInfo("Token expression", strExpr);
		throw ue;
	}

	// a % sign followed by anything other than one or more digits is invalid.
	// or followed by 0 is invalid too since externally the token numbers are
	// 1-based instead of 0-based
	const string strInvalidExpr = "%(0|\\D|\\s|(?=\\r*$))";

	// initialize the regex parser object with the
	// pattern for invalid %something expressions
	IRegularExprParserPtr ipParser = getRegExParser(strInvalidExpr);

	// check if any invalid %something expression exists
	if (ipParser->StringContainsPattern(strExpr.c_str()) == VARIANT_TRUE)
	{
		UCLIDException ue("ELI05788", "Invalid token expression.\r\n"
			"\r\n"
			"The only special expression you can use in a token "
			"expression is a percent character followed by some non-zero digits representing "
			"the token number.  Use of %1, %2, etc. is valid, but use of %d for "
			"instance is not valid.");
		ue.addDebugInfo("strExpr", strExpr);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void CStringTokenizerSplitter::validateFieldNameExpression(const string& strFieldNameExpr)
{

	// a "%" at the beginning of the expression, 
	// more than one "%" or "%" sign followed
	// by anything other than a "d" are invaild.
	// form1 = ^% "%" at the beginning of the expression such as "%Field"
	// form2 = %{any number of char)%(any number of char) such as "%d_Field_%d"
	// form3 - %{non-d} such as "%k"
	// strInvalidExpr = form1|form2|form3

	const string strInvalidExpr = "(^%)|(%.*%.*)|(%((?=\\r*$)|\\s))|(%[^d])";

	// Create a local parser object with case sensitive 
	IMiscUtilsPtr ipMiscUtils(CLSID_MiscUtils);
	ASSERT_RESOURCE_ALLOCATION("ELI12963", ipMiscUtils != __nullptr );

	IRegularExprParserPtr ipLocalParser = ipMiscUtils->GetNewRegExpParserInstance("StringTokenizerSplitter");
	ASSERT_RESOURCE_ALLOCATION("ELI12964", ipLocalParser != __nullptr);
	
	ipLocalParser->put_IgnoreCase (false);

	// initialize the regex parser object with the
	// pattern for invalid %something expressions
	ipLocalParser->Pattern = strInvalidExpr.c_str();

	// check if any invalid %something expression exists
	if (ipLocalParser->StringContainsPattern(strFieldNameExpr.c_str()) ==
		VARIANT_TRUE)
	{
		UCLIDException ue("ELI05786", "Invalid field name expression.\r\n"
			"\r\n"
			"The only special expression you can use in a field name is %d, where "
			"%d will be replaced with the token number. %d can not be placed at the" 
			" beginning of the expression and there should be no more than ONE %d in "
			"the expression. Field_%d is an example of a valid expression, "
			"while Field_%3, Field_%d_%d, %d_Field are not valid expressions.");
		ue.addDebugInfo("strFieldNameExpr", strFieldNameExpr);
		throw ue;
	}

	// replace %d with an actual digit and validate the identifier
	CString z1( strFieldNameExpr.c_str() );
	z1.Replace( "%d", "1" );
	validateIdentifier( z1.GetBuffer() );
}
//-------------------------------------------------------------------------------------------------
void CStringTokenizerSplitter::resetVariablesToDefault()
{
	m_cDelimiter = ',';
	m_strFieldNameExpression = "Field_%d";
	m_eSplitType = kEachTokenAsSubAttribute;
	m_ipVecNameValuePair = __nullptr;
}
//-------------------------------------------------------------------------------------------------
IRegularExprParserPtr CStringTokenizerSplitter::getRegExParser(const string& strPattern)
{
	try
	{
		IRegularExprParserPtr ipParser =
			m_ipMiscUtils->GetNewRegExpParserInstance("StringTokenizerSplitter");
		ASSERT_RESOURCE_ALLOCATION("ELI05399", ipParser != __nullptr);

		ipParser->Pattern = strPattern.c_str();

		return ipParser;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29468");
}
//-------------------------------------------------------------------------------------------------
