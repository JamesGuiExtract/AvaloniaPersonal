// ReformatPersonNames.cpp : Implementation of CReformatPersonNames
#include "stdafx.h"
#include "AFOutputHandlers.h"
#include "ReformatPersonNames.h"

#include <LicenseMgmt.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <comutils.h>
#include <ComponentLicenseIDs.h>

#include <map>
#include <vector>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CReformatPersonNames
//-------------------------------------------------------------------------------------------------
CReformatPersonNames::CReformatPersonNames()
: m_bReformatPersonSubAttributes(true)
{
	try
	{
		m_ipAFUtility.CreateInstance(CLSID_AFUtility);
		ASSERT_RESOURCE_ALLOCATION("ELI09569", m_ipAFUtility != NULL);
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI09570")
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReformatPersonNames::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IReformatPersonNames,
		&IID_IOutputHandler,
		&IID_IPersistStream,
		&IID_ICategorizedComponent,
		&IID_ISpecifyPropertyPages,
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
// IRemoveSubAttributes
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReformatPersonNames::get_PersonAttributeQuery(/*[out, retval]*/ BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Validate argument
		ASSERT_ARGUMENT("ELI09571", pVal != NULL );

		// Return the Attribute Name
		*pVal = _bstr_t( m_strQuery.c_str() ).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09572")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReformatPersonNames::put_PersonAttributeQuery(/*[in]*/ BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Make local copy of Name
		string strTmp = asString( newVal );

		// Attribute Name must not be empty
		if (strTmp.empty())
		{
			UCLIDException ue("ELI09573", "The Attribute query must not be empty!");
			throw ue;
		}

		// make sure the string is a valid query
		if(m_ipAFUtility->IsValidQuery(newVal) == VARIANT_FALSE)
		{
			UCLIDException ue( "ELI10437", "Invalid Attribute Query!");
			throw ue;
		}

		// Store the Attribute Name
		m_strQuery = strTmp;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09574")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReformatPersonNames::get_ReformatPersonSubAttributes(/*[out, retval]*/ VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Validate argument
		ASSERT_ARGUMENT( "ELI09597", pVal != NULL );

		*pVal = m_bReformatPersonSubAttributes ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09598")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReformatPersonNames::put_ReformatPersonSubAttributes(/*[in]*/ VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		m_bReformatPersonSubAttributes = newVal == VARIANT_TRUE;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09599")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReformatPersonNames::get_FormatString(/*[out, retval]*/ BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Validate argument
		ASSERT_ARGUMENT("ELI09593", pVal != NULL );

		// Return the Attribute Name
		*pVal = _bstr_t( m_strFormat.c_str() ).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09594")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReformatPersonNames::put_FormatString(/*[in]*/ BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Make local copy of Name
		string strTmp = asString( newVal );

		// Format String must not be empty
		if (strTmp.empty())
		{
			UCLIDException ue("ELI09595", "The Format String must not be empty!");
			throw ue;
		}

		validateFormatString(strTmp);

		// Store the Format String
		m_strFormat = strTmp;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09596")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IOutputHandler
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReformatPersonNames::raw_ProcessOutput(IIUnknownVector* pAttributes,
													 IAFDocument *pAFDoc,
													 IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		IIUnknownVectorPtr ipAttributes(pAttributes);
		ASSERT_RESOURCE_ALLOCATION("ELI09575", ipAttributes != NULL);

		// Query the attributes
		IIUnknownVectorPtr ipFoundAttributes = m_ipAFUtility->QueryAttributes(ipAttributes, 
			_bstr_t(m_strQuery.c_str()), VARIANT_FALSE);

		int i;
		for (i = 0; i < ipFoundAttributes->Size(); i++)
		{
			IAttributePtr ipAttr = ipFoundAttributes->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI09577", ipAttr != NULL);

			reformatAttribute(ipAttr);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09578")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReformatPersonNames::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CReformatPersonNames::raw_IsConfigured(VARIANT_BOOL * pbValue)
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

		// This object is considered configured 
		// if a query is specified
		*pbValue = m_strQuery.empty() ? VARIANT_FALSE : VARIANT_TRUE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09579");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReformatPersonNames::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19550", pstrComponentDescription != NULL)

		*pstrComponentDescription = _bstr_t("Reformat person names").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09580")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReformatPersonNames::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_AFOUTPUTHANDLERSLib::IReformatPersonNamesPtr ipSource = pObject;
		ASSERT_RESOURCE_ALLOCATION("ELI09581", ipSource != NULL );
		
		m_strQuery = asString( ipSource->PersonAttributeQuery );
		
		m_bReformatPersonSubAttributes = ipSource->ReformatPersonSubAttributes == VARIANT_TRUE;
		
		m_strFormat = asString( ipSource->FormatString );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12817");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReformatPersonNames::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		// Create another instance of this object
		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance( CLSID_ReformatPersonNames );
		ASSERT_RESOURCE_ALLOCATION("ELI09582", ipObjCopy != NULL );

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom( ipUnk );
	
		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09583");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReformatPersonNames::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pClassID = CLSID_ReformatPersonNames;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReformatPersonNames::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReformatPersonNames::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

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
			UCLIDException ue( "ELI09584", 
				"Unable to load newer Reformat Person Names Output Handler!" );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}
		// load data here
		dataReader >> m_strQuery;
		dataReader >> m_bReformatPersonSubAttributes;
		dataReader >> m_strFormat;

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09585");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReformatPersonNames::Save(IStream *pStream, BOOL fClearDirty)
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
		dataWriter << m_strQuery;
		dataWriter << m_bReformatPersonSubAttributes;
		dataWriter << m_strFormat;

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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09586");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReformatPersonNames::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CReformatPersonNames::validateLicense()
{
	static const unsigned long REFORMAT_PERSON_NAMES_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE( REFORMAT_PERSON_NAMES_ID, "ELI09587", 
		"Reformat Person Names Output Handler" );
}
//-------------------------------------------------------------------------------------------------
void CReformatPersonNames::reformatAttribute(IAttributePtr ipAttribute)
{
	if (ipAttribute == NULL)
	{
		THROW_LOGIC_ERROR_EXCEPTION("ELI09603");
	}

	// This will hold all the variables which will be available to the format
	map<string, vector<ISpatialStringPtr> > mapVars;
	
	IIUnknownVectorPtr ipSubAttributes = ipAttribute->SubAttributes;
	int i;
	for (i = 0; i < ipSubAttributes->Size(); i++)
	{
		IAttributePtr ipAttr = ipSubAttributes->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI09602", ipAttr != NULL);

		// if we are supposed to process PersonSubAttributes
		// then we recurse on each sub attribute
		if (m_bReformatPersonSubAttributes)
		{
			reformatAttribute(ipAttr);
		}

		// Add this sub attribute to the map of available variables
		string strName = asString(ipAttr->Name);
		mapVars[strName].push_back(ipAttr->Value);
	}

	// In order to maintain this as the tree grows
	// all that needs to happen is that attributes which
	// have valid names should be added here
	string strAttributeName = asString(ipAttribute->Name);
	if (	strAttributeName == "Person" ||
		strAttributeName == "PersonAlias" ||
		strAttributeName == "Agent" ||
		strAttributeName == "AIF" ||
		strAttributeName == "Authority")
	{
		// Reformat the name
		ISpatialStringPtr ipNewValue = getReformattedName(m_strFormat, mapVars);
		ipAttribute->Value = ipNewValue;
	}
}
//-------------------------------------------------------------------------------------------------
ISpatialStringPtr CReformatPersonNames::getReformattedName(	string strFormat, VariableMap& varmap)
{
	ISpatialStringPtr ipNewName(CLSID_SpatialString);
	ASSERT_RESOURCE_ALLOCATION("ELI09608", ipNewName != NULL);

	unsigned int ui;
	unsigned int uiLength = strFormat.length();

	for (ui = 0; ui < uiLength; ui++)
	{
		char c = strFormat.at(ui);
		if (c == '<')
		{
			// a new scope is being opened
			// to process it we will get the entire scope as a substring 
			// and recurse adding the sub string wherever it is supposed to go
			long nClosePos = getCloseScopePos(strFormat, ui, '<', '>');
			if (nClosePos == string::npos)
			{
				UCLIDException ue("ELI09620", "Unmatched \'<\' in name formatting pattern.");
				throw ue;
			}
			// nClosePos is the index that immediately follows the closing > of
			// the scope and we want the string between < and > exclusive
			long nStart = ui+1;
			long nLength = (nClosePos - 1) - nStart;
			string strNewFormat = strFormat.substr(ui+1, nLength);
			ISpatialStringPtr ipScopeStr = getReformattedName(strNewFormat, varmap);
			if (ipScopeStr != NULL)
			{
				ipNewName->Append(ipScopeStr);
			}
			// -1 because ui will auto increment (for loop) 
			ui = nClosePos - 1;
		}
		else if (c == '%')
		{
			// % must be followed by a valid identifier that is the name of an attribute
			// it must be either %First, %Last, %Middle... 
			// there can optionally be a number between % and the identifier which is the 
			// number of characters to use
			long nNumChars = -1;
			unsigned long ulIdentStartPos = ui + 1;
			unsigned long ulIdentEndPos = std::string::npos;

			// if character after starting "%" is digit extract the number from the identifier
			if (ulIdentStartPos < uiLength && isDigitChar(strFormat[ulIdentStartPos]))
			{
				unsigned long ulNumberEndPos = strFormat.find_first_not_of("0123456789", ulIdentStartPos);

				string strNum = strFormat.substr( ulIdentStartPos, ulNumberEndPos - ulIdentStartPos );
				nNumChars = asLong( strNum.c_str() );
				ulIdentStartPos = ulNumberEndPos;
			}

			ulIdentEndPos = getIdentifierEndPos(strFormat, ulIdentStartPos);
			
			// confirm that a valid identifer name was found
			if (ulIdentEndPos == std::string::npos)
			{
				UCLIDException ue("ELI13022", "Invalid identifier name specified.");
				ue.addDebugInfo("Identifier", strFormat.substr(ulIdentStartPos));
				throw ue;
			}

			unsigned long ulLength = ulIdentEndPos - ulIdentStartPos;
			string strIdent = strFormat.substr(ulIdentStartPos, ulLength);

			// Get the value of the %variable
			// if the %variable is available
			ISpatialStringPtr ipText = NULL;
			VariableMap::iterator it = varmap.find(strIdent);
			if (it != varmap.end())
			{
				if (it->second.size() < 1)
				{
					THROW_LOGIC_ERROR_EXCEPTION("ELI09621");
				}
				// For now we will always use the first instance of any 
				// variable i.e. if there are 2 last names %Last always
				// means the first one
				ipText = it->second[0];
			}

			if (ipText == NULL)
			{
				// If the %var has no value we can either ignore it and continue
				// or invalidate the enitire scope
				return NULL;
			}
			else
			{
				if (nNumChars > 0)
				{
					long nLength = ipText->Size;
					if (nNumChars < nLength)
					{
						ipText = ipText->GetSubString(0, nNumChars-1);
					}
				}
				ipNewName->Append(ipText);
			}
			// don't forget to jump ahead in the string
			ui = ulIdentEndPos - 1;
		}
		else
		{
			// Add this character to the string
			string str;
			str += c;
			ipNewName->AppendString(_bstr_t(str.c_str()));
		}
	}
	return ipNewName;
}
//-------------------------------------------------------------------------------------------------
void CReformatPersonNames::validateFormatString(string strFormat)
{
	unsigned int ui;
	for (ui = 0; ui < strFormat.length(); ui++)
	{
		char c = strFormat.at(ui);
		if (c == '<')
		{
			// a new scope is being opened
			// to process it we will get the entire scope as a substring 
			// and recurse adding the sub string wherever it is supposed to go
			unsigned long ulClosePos = getCloseScopePos(strFormat, ui, '<', '>');
			if (ulClosePos == string::npos)
			{
				UCLIDException ue("ELI09630", "Unmatched \'<\' in name formatting pattern.");
				throw ue;
			}
			// nClosePos is the index that immediately follows the closing > of
			// the scope and we want the string between < and > exclusive
			long nStart = ui + 1;
			long nLength = (ulClosePos - 1) - nStart;
			string strNewFormat = strFormat.substr(ui + 1, nLength);
			validateFormatString(strNewFormat);
			ui = ulClosePos - 1;
		}
		else if (c == '%')
		{
			// % must be followed by a valid identifier that is the name of an attribute
			// it must be either %First, %Last, %Middle... 
			long nNumberEnd = strFormat.find_first_not_of("0123456789", ui + 1);
			long nIdentStart = ui + 1;
			if (nNumberEnd != string::npos)
			{
				nIdentStart = nNumberEnd;
			}

			unsigned long ulEndPos = getIdentifierEndPos(strFormat, nIdentStart);
			if (ulEndPos == string::npos)
			{
				UCLIDException ue("ELI09631", "Invalid Identifier in Format String.");
				ue.addDebugInfo("Identifier", strFormat.substr(ui));
				throw ue;
			}
			long nStart = nIdentStart;
			long nLength = ulEndPos - nStart;
			string strIdent = strFormat.substr(nStart, nLength);
			// Check if the identifier is valid one for a name
			if(!(strIdent == "First" || 
				strIdent == "Last" || 
				strIdent == "Middle" || 
				strIdent == "Title" || 
				strIdent == "Suffix"))
			{
				UCLIDException ue("ELI09643", "Invalid variable in Format String.");
				ue.addDebugInfo("Identifier", strIdent);
				throw ue;
			}
			// don't forget to jump ahead in the string
			ui = ulEndPos - 1;
		}
		else
		{
			// just a character
		}
	}
}
//-------------------------------------------------------------------------------------------------
