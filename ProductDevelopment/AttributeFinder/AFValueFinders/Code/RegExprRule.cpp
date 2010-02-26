
#include "stdafx.h"
#include "AFValueFinders.h"
#include "RegExprRule.h"
#include "..\\..\\AFCore\\Code\\Common.h"

#include <UCLIDException.h>
#include <COMUtils.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <CommentedTextFileReader.h>
#include <EncryptedFileManager.h>
#include <ByteStream.h>
#include <ByteStreamManipulator.h>
#include <Misc.h>
#include <AFTagManager.h>
#include <ComponentLicenseIDs.h>

#include <io.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 4;

//-------------------------------------------------------------------------------------------------
// CRegExprRule
//-------------------------------------------------------------------------------------------------
CRegExprRule::CRegExprRule()
:	m_bCaseSensitive(false), 
	m_bDirty(false), 
	m_strPattern(""),
	m_bIsRegExpFromFile(false),
	m_strRegExpFileName(""),
	m_ipAFUtility(NULL),
	m_ipMiscUtils(NULL),
	m_bAddCapturesAsSubAttributes(false),
	m_cachedRegExLoader(gstrAF_AUTO_ENCRYPT_KEY_PATH.c_str())
{
	try
	{
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI04224")
}
//-------------------------------------------------------------------------------------------------
CRegExprRule::~CRegExprRule()
{
	try
	{
		// Set COM objects to NULL
		m_ipAFUtility = NULL;
		m_ipMiscUtils = NULL;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16347");
}
//--------------------------------------------------------------------------------------------------
HRESULT CRegExprRule::FinalConstruct()
{
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
void CRegExprRule::FinalRelease()
{
	try
	{
		// Set COM objects to NULL
		m_ipAFUtility = NULL;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI27092");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprRule::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IRegExprRule,
		&IID_IAttributeFindingRule,
		&IID_IAttributeModifyingRule,
		&IID_IPersistStream,
		&IID_ICategorizedComponent,
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
// IRegExprRule
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprRule::get_Pattern(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		*pVal = _bstr_t(m_strPattern.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04498")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprRule::put_Pattern(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		string strPattern = asString( newVal );

		if (strPattern.empty())
		{
			throw UCLIDException("ELI19316", "Please provide non-empty regular expression.");
		}

		m_strPattern = strPattern;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04499")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprRule::get_IsCaseSensitive(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_bCaseSensitive ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04500")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprRule::put_IsCaseSensitive(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bCaseSensitive = newVal == VARIANT_TRUE;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04501")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprRule::get_IsRegExpFromFile(BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();

		*pVal = m_bIsRegExpFromFile ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07521");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprRule::put_IsRegExpFromFile(BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();

		m_bIsRegExpFromFile = newVal == VARIANT_TRUE;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07522");
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprRule::get_RegExpFileName(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = _bstr_t(m_strRegExpFileName.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08456");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprRule::put_RegExpFileName(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// checking or not checking RDT license all depends
		// on the file type.
		string strFile = asString( newVal );

		// make sure the file exists
		// or if the file name contains valid <DocType> strings
		if (getAFUtility()->StringContainsInvalidTags(strFile.c_str()) == VARIANT_TRUE)
		{
			UCLIDException ue("ELI07533", "The rules file contains invalid tags!");
			ue.addDebugInfo("File", strFile);
			throw ue;
		}
		else if (getAFUtility()->StringContainsTags(strFile.c_str()) == VARIANT_FALSE)
		{
			if (!isAbsolutePath(strFile))
			{
				UCLIDException ue("ELI07534", "Specification of a relative path to the DAT file is not allowed!");
				ue.addDebugInfo("File", strFile);
				throw ue;
			}
			else if (!isValidFile(strFile))
			{
				UCLIDException ue("ELI07535", "The specified regular expression file does not exist!");
				ue.addDebugInfo("File", strFile);
				ue.addWin32ErrorInfo();
				throw ue;
			}
		}
		m_strRegExpFileName = strFile;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19335");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprRule::get_CreateSubAttributesFromNamedMatches(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI23028", pVal != NULL);

		*pVal = asVariantBool(m_bAddCapturesAsSubAttributes);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI23027");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprRule::put_CreateSubAttributesFromNamedMatches(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_bAddCapturesAsSubAttributes = asCppBool(newVal);
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI23026");
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprRule::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_RegExprRule;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprRule::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
// Version 4: Added m_bAddCapturesAsSubAttributes
STDMETHODIMP CRegExprRule::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Reset all the member variables
		m_bCaseSensitive = false;
		m_strPattern = "";
		m_bIsRegExpFromFile = false;
		m_strRegExpFileName = "";
		m_bAddCapturesAsSubAttributes = false;

		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		pStream->Read(&nDataLength, sizeof(nDataLength), NULL);
		ByteStream data(nDataLength);
		pStream->Read(data.getData(), nDataLength, NULL);
		ByteStreamManipulator dataReader(ByteStreamManipulator::kRead, data);

		// Read the individual data items from the bytestream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;
		if (nDataVersion >= 1)
		{
			dataReader >> m_bCaseSensitive;
			dataReader >> m_strPattern;
			if ( nDataVersion >= 2)
			{
				dataReader >> m_bIsRegExpFromFile;
				dataReader >> m_strRegExpFileName;
			}
			if ( nDataVersion >= 4)
			{
				dataReader >> m_bAddCapturesAsSubAttributes;
			}
		}
		// in the newer version all whitespace is removed so
		// \s or \r\n or \x20 must be used to indicate whitespace
		// since the older versions allowed whitespace in these versions
		// the whitespace must be replaced with the appropriate characters
		// (i.e. " " with "\x20"
		if (nDataVersion < 3)
		{
			unsigned int ui;
			for (ui = 0; ui < m_strPattern.size(); ui++)
			{
				char c = m_strPattern.at( ui );
				string str = "";
				switch(c)
				{
				case ' ':
					str = "\\x20";
					break;
				case '\r':
					str = "\\r";
					break;
				case '\n':
					str = "\\n";
					break;
				case '\t':
					str = "\\t";
					break;
				case '\f':
					str = "\\x0C";
					break;
				case '\v':
					str = "\\x0B";
					break;
				}
				if (str != "")
				{
					m_strPattern.replace(ui, 1, str);
					ui = ui + str.size();
					// ui will be incremented at the top of the loop
					ui--;
				}
			}
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04578");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprRule::Save(IStream *pStream, BOOL fClearDirty)
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
		dataWriter << m_strPattern;

		dataWriter << m_bIsRegExpFromFile;
		dataWriter << m_strRegExpFileName;
		dataWriter << m_bAddCapturesAsSubAttributes;
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04579");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprRule::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// IAttributeFindingRule
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprRule::raw_ParseText(IAFDocument* pAFDoc, IProgressStatus *pProgressStatus,
										 IIUnknownVector **pAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		ASSERT_ARGUMENT("ELI29409", pAttributes != NULL);

		IAFDocumentPtr ipAFDoc(pAFDoc);
		ASSERT_ARGUMENT("ELI29410", ipAFDoc != NULL);

		IIUnknownVectorPtr ipAttributes = parseText(ipAFDoc);
		ASSERT_RESOURCE_ALLOCATION("ELI29408", ipAttributes != NULL);

		// return the vector
		*pAttributes = ipAttributes.Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04217");
}

//-------------------------------------------------------------------------------------------------
// IAttributeModifyingRule
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprRule::raw_ModifyValue(IAttribute* pAttribute, IAFDocument* pOriginInput,
										   IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// make up an AFDocument
		IAttributePtr	ipAttr( pAttribute );
		ASSERT_RESOURCE_ALLOCATION("ELI09302", ipAttr != NULL);

		ISpatialStringPtr ipAttrValue = ipAttr->Value;
		ASSERT_RESOURCE_ALLOCATION("ELI06862", ipAttrValue != NULL);

		IAFDocumentPtr ipAFDoc(CLSID_AFDocument);
		ASSERT_RESOURCE_ALLOCATION("ELI06863", ipAFDoc != NULL);
		ipAFDoc->Text = ipAttrValue;

		IIUnknownVectorPtr ipAttributes = parseText(ipAFDoc);
		if (ipAttributes != NULL && ipAttributes->Size() > 0)
		{
			// if more than one value is found, return the first match
			IAttributePtr ipAttribute(ipAttributes->At(0));
			ASSERT_RESOURCE_ALLOCATION("ELI25956", ipAttribute != NULL);

			ISpatialStringPtr ipModifiedValue = ipAttribute->Value;
			ASSERT_RESOURCE_ALLOCATION("ELI25957", ipModifiedValue != NULL);

			ICopyableObjectPtr ipCopier = ipAttrValue;
			ASSERT_RESOURCE_ALLOCATION("ELI25958", ipCopier != NULL);
			ipCopier->CopyFrom(ipModifiedValue);

			// Add any new sub attributes that were added for the value
			IIUnknownVectorPtr ipSubAttributes = ipAttr->SubAttributes;
			IIUnknownVectorPtr ipNewSubAttributes = ipAttribute->SubAttributes;

			if ( ipNewSubAttributes == NULL || ipNewSubAttributes->Size() == 0 )
			{
				// No new sub attributes so just return with the original
				return S_OK;
			}
			if (ipSubAttributes == NULL || ipSubAttributes->Size() == 0)
			{
				// There were no sub attributes for the original attribute so set the 
				// sub attributes to the new sub attributes.
				ipAttr->SubAttributes = ipNewSubAttributes;
				return S_OK;
			}

			// Add any new sub attributes
			ipSubAttributes->Append(ipNewSubAttributes);

			return S_OK;
		}

		// if no value is found, set the spatial string to empty
		ipAttrValue->Clear();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04218");
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprRule::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19582", pstrComponentDescription != NULL)

		*pstrComponentDescription = _bstr_t("Regular expression rule").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04219");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprRule::raw_IsConfigured(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI29407", pbValue != NULL);

		// This object is considered configured 
		//	if m_bIsRegExpFromFile is true and m_strRegExpFileName is not empty
		//	or m_bIsRegExpFromFile is false and strPattern is not empty

		bool bConfigured = false;
		if (( m_bIsRegExpFromFile && !m_strRegExpFileName.empty())
			|| (!m_bIsRegExpFromFile && !m_strPattern.empty()))
		{
			bConfigured = true;
		}

		*pbValue = asVariantBool(bConfigured);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19310");
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprRule::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CRegExprRule::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		UCLID_AFVALUEFINDERSLib::IRegExprRulePtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI08256", ipSource != NULL);

		m_bCaseSensitive = (ipSource->GetIsCaseSensitive()==VARIANT_TRUE) ? true : false;
		m_bIsRegExpFromFile = (ipSource->GetIsRegExpFromFile()==VARIANT_TRUE) ? true : false;

		m_strRegExpFileName = ipSource->GetRegExpFileName();
		m_strPattern = ipSource->GetPattern();
		m_bAddCapturesAsSubAttributes = asCppBool(ipSource->CreateSubAttributesFromNamedMatches);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08257");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRegExprRule::raw_Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy(CLSID_RegExprRule);
		ASSERT_RESOURCE_ALLOCATION("ELI08347", ipObjCopy != NULL);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04486");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CRegExprRule::validateLicense()
{
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI04221", "Regular Expression Rule" );
}
//-------------------------------------------------------------------------------------------------
IAFUtilityPtr CRegExprRule::getAFUtility()
{
	if (m_ipAFUtility == NULL)
	{
		m_ipAFUtility.CreateInstance( CLSID_AFUtility );
		ASSERT_RESOURCE_ALLOCATION( "ELI07520", m_ipAFUtility != NULL );
	}
	
	return m_ipAFUtility;
}
//-------------------------------------------------------------------------------------------------
IMiscUtilsPtr CRegExprRule::getMiscUtils()
{
	if (m_ipMiscUtils == NULL)
	{
		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI07639", m_ipMiscUtils != NULL );
	}

	return m_ipMiscUtils;
}
//-------------------------------------------------------------------------------------------------
string CRegExprRule::getRegularExpr(IAFDocumentPtr ipAFDoc)
{
	// String to contain the return value
	string strRegExp = "";
	if ( !m_bIsRegExpFromFile )
	{
		// ensure that a pattern has been specified
		if (m_strPattern.length() == 0)
		{
			throw UCLIDException("ELI04222", "No regular expression pattern set!");
		}

		string strRootFolder = "";
		// we will try to use the rsd file dir as the root folder
		// but if there is no rsd file dir we will pass in an empty root 
		// folder
		try
		{
			string rootTag = "<RSDFileDir>";
			strRootFolder = getAFUtility()->ExpandTags(rootTag.c_str(), ipAFDoc );
		}
		catch(...)
		{
		}

		strRegExp = getRegExpFromText(m_strPattern, strRootFolder,  true, gstrAF_AUTO_ENCRYPT_KEY_PATH);
	}
	else
	{
		// Expand any tags in the file name
		string strRegExpFile = getRegExpFileName( ipAFDoc );

		// [FlexIDSCore:3642] Load the regular expression from disk if necessary.
		m_cachedRegExLoader.loadObjectFromFile(strRegExpFile);

		strRegExp = (string)m_cachedRegExLoader.m_obj;
	}
	return strRegExp;
}
//-------------------------------------------------------------------------------------------------
std::string CRegExprRule::getRegExpFileName(IAFDocumentPtr ipAFDoc)
{
	string strOut = m_strRegExpFileName;
	AFTagManager tagMgr;
	strOut = tagMgr.expandTagsAndFunctions(strOut, ipAFDoc);
	return strOut;
}
//-------------------------------------------------------------------------------------------------	
IAttributePtr CRegExprRule::createAttribute(ITokenPtr ipToken, ISpatialStringPtr ipInput)
{
	ASSERT_ARGUMENT("ELI22916", ipToken != NULL);
	ASSERT_ARGUMENT("ELI22917", ipInput != NULL);

	long nStart, nEnd;
	ipToken->GetTokenInfo(&nStart, &nEnd, NULL, NULL);

	// create an attribute to store the value
	IAttributePtr ipAttribute(CLSID_Attribute);
	ASSERT_RESOURCE_ALLOCATION("ELI22926", ipAttribute != NULL);

	ISpatialStringPtr ipMatch;
	if ( nStart != -1 && nEnd != -1 )
	{
		// create a spatial string representing the match
		ipMatch = ipInput->GetSubString(nStart, nEnd);
		ASSERT_RESOURCE_ALLOCATION("ELI22479", ipMatch != NULL);
	}
	else
	{
		ipMatch.CreateInstance(CLSID_SpatialString);
		ASSERT_RESOURCE_ALLOCATION("ELI22485", ipMatch != NULL);
		ipMatch->CreateNonSpatialString(ipToken->Value, "");
	}

	// set the match as the value of the attribute, and add
	// the attribute to the result vector
	ipAttribute->Value = ipMatch;
	
	// If the name property of the token is not of zero length set the attribute name to the name
	// given in the token.
	if ( ipToken->Name.length() > 0)
	{
		// Handle the sub matches that are numbered
		// eventually this will not be required
		// TODO: Remove this if only using named sub matches
		string strName = asString(ipToken->Name);
		if ( isDigitChar( strName[0] ))
		{
			// Add capture prefix
			strName = "Capture" + strName;
		}
		// Get the name from the token
		ipAttribute->Name = strName.c_str();
	}

	return ipAttribute;
}
//-------------------------------------------------------------------------------------------------	
IIUnknownVectorPtr CRegExprRule::parseText(IAFDocumentPtr ipAFDoc)
{
	try
	{
		// Get the parser
		IRegularExprParserPtr ipParser = getParser();
		ASSERT_RESOURCE_ALLOCATION("ELI29403", ipParser != NULL);

		IIUnknownVectorPtr ipAttributes(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI04733", ipAttributes != NULL);

		ISpatialStringPtr ipInputText = ipAFDoc->Text;
		ASSERT_RESOURCE_ALLOCATION("ELI29406", ipInputText != NULL);

		// Set the pattern and case values for the regular expression parser
		ipParser->Pattern = getRegularExpr(ipAFDoc).c_str();
		ipParser->IgnoreCase = asVariantBool(!m_bCaseSensitive);

		// Use the regular expression engine to parse the text and find attribute values
		// matching the specified regular expression
		IIUnknownVectorPtr ipMatches = ipParser->Find(ipInputText->String, VARIANT_FALSE, 
			asVariantBool(m_bAddCapturesAsSubAttributes));
		ASSERT_RESOURCE_ALLOCATION("ELI29404", ipMatches != NULL);

		long nNumMatches = ipMatches->Size();
		if (nNumMatches > 0)
		{
			// iterate through the matches and populate the return vector
			for (int i = 0; i < nNumMatches; i++)
			{
				// each item in the ipMatches is of type IObjectPair
				IObjectPairPtr ipObjPair = ipMatches->At(i);

				// Token is the first object in the object pair
				ITokenPtr ipToken = ipObjPair->Object1;
				if (ipToken)
				{	
					// create an attribute to store the value
					IAttributePtr ipAttribute = createAttribute(ipToken, ipInputText);
					ASSERT_RESOURCE_ALLOCATION("ELI06547", ipAttribute != NULL);

					IIUnknownVectorPtr ipSubMatches = ipObjPair->Object2;
					if ( m_bAddCapturesAsSubAttributes && ipSubMatches != NULL )
					{
						// Make any sub matches into sub attributes
						IIUnknownVectorPtr ipSubAttributes(CLSID_IUnknownVector);
						ASSERT_RESOURCE_ALLOCATION("ELI22476", ipSubAttributes != NULL);
						
						// Get number of sub matches
						int iCount = ipSubMatches->Size();
						for ( int s = 0; s < iCount; s++)
						{
							ITokenPtr ipSubToken = ipSubMatches->At(s);
							ASSERT_RESOURCE_ALLOCATION("ELI22477", ipSubToken != NULL);
							
							// Don't create attributes if name begins with a number or is empty.
							string strName = asString(ipSubToken->Name);
							if ( !isDigitChar( strName[0] ) && !asString(ipSubToken->Value).empty() )
							{
								IAttributePtr ipSubAttribute = createAttribute(ipSubToken, ipInputText);
								ASSERT_RESOURCE_ALLOCATION("ELI22478", ipSubAttribute != NULL);

								// Put the sub attribute on the list
								ipSubAttributes->PushBack(ipSubAttribute);
							}
						}
						
						// Add the sub attribute list to the attribute
						ipAttribute->SubAttributes = ipSubAttributes;
					}

					// Put the found attribute on the attributes list
					ipAttributes->PushBack(ipAttribute);
				}
			}
		}

		return ipAttributes;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29405");
}
//-------------------------------------------------------------------------------------------------	
IRegularExprParserPtr CRegExprRule::getParser()
{
	try
	{
		// create the regular expression parser object
		IRegularExprParserPtr ipParser = getMiscUtils()->GetNewRegExpParserInstance("RegExprRule");
		ASSERT_RESOURCE_ALLOCATION("ELI04223",ipParser != NULL);

		return ipParser;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29486");
}
//-------------------------------------------------------------------------------------------------	
