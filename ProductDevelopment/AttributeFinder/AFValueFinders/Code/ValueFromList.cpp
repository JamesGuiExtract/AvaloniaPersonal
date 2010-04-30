// ValueFromList.cpp : Implementation of CValueFromList
#include "stdafx.h"
#include "AFValueFinders.h"
#include "ValueFromList.h"

#include <UCLIDException.h>
#include <CommentedTextFileReader.h>
#include <cpputil.h>
#include <COMUtils.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

#include <fstream>
#include <algorithm>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CValueFromList
//-------------------------------------------------------------------------------------------------
CValueFromList::CValueFromList()
: m_bCaseSensitive(false),
  m_ipValueList(CLSID_VariantVector),
  m_bDirty(false)
{
}
//-------------------------------------------------------------------------------------------------
CValueFromList::~CValueFromList()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16352");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueFromList::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IAttributeFindingRule,
		&IID_IInputValidator,
		&IID_ICategorizedComponent,
		&IID_IValueFromList,
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
STDMETHODIMP CValueFromList::raw_ParseText(IAFDocument* pAFDoc, IProgressStatus *pProgressStatus,
										   IIUnknownVector **pAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// Create a vector for storing all attribute values
		IIUnknownVectorPtr ipAttributes( CLSID_IUnknownVector );
		ASSERT_RESOURCE_ALLOCATION( "ELI04562", ipAttributes != NULL );

		// Create local Document pointer
		IAFDocumentPtr ipAFDoc( pAFDoc );

		// Get the text out from the spatial string
		ISpatialStringPtr ipInputText( ipAFDoc->Text );

		// get the input string from the spatial string
		string strInputText = ipInputText->String;

		// Only continue if string is non-empty
		if (!strInputText.empty())
		{
			// Change input text to upper case if not case-sensitive
			if (!m_bCaseSensitive) 
			{
				::makeUpperCase( strInputText );
			}

			// vector of found positions for sorting purpose
			vector<int> vecPositions;
			string strItem("");

			// Get a list of values that includes values from any specified files.
			IVariantVectorPtr ipExpandedList = m_cachedListLoader.expandList(m_ipValueList, ipAFDoc);
			ASSERT_RESOURCE_ALLOCATION("ELI30066", ipExpandedList != NULL);

			// Check each item in list
			long nSize = ipExpandedList->Size;
			for (long n = 0; n < nSize; n++)
			{
				// Retrieve this list item
				_bstr_t _bstrItem( ipExpandedList->GetItem( n ) );
				if (_bstrItem.length() == 0)
				{
					continue;
				}
				string strItem = asString(_bstrItem);

				// Change text to upper case if not case-sensitive
				if (!m_bCaseSensitive)
				{
					// if case-insensitive
					::makeUpperCase( strItem );
				}

				// Search the input text for this item
				int nFoundPos = strInputText.find( strItem );
				if (nFoundPos != string::npos)
				{
					// Item was found, find its proper position in vector of found positions
					unsigned long nIndex = vecPositions.size();
					for (unsigned int ui = 0; ui < vecPositions.size(); ui++)
					{
						if (nFoundPos < vecPositions[ui])
						{
							nIndex = ui;
							break;
						}
					}

					// Store this position
					vecPositions.push_back( nFoundPos );

					// Sort the vector ascendingly
					sort( vecPositions.begin(), vecPositions.end() );
					
					// Create an Attribute to store the value
					IAttributePtr ipAttribute( CLSID_Attribute );

					// Get the associated ISpatialString based on start and end positions
					// from the original ISpatialString
					ISpatialStringPtr	ipText = ipInputText->GetSubString( nFoundPos, 
						nFoundPos + strItem.length() - 1 );
					ASSERT_RESOURCE_ALLOCATION( "ELI06590", ipText != NULL );
					ipAttribute->Value = ipText;

					// Put the new Attribute into the return vector
					ipAttributes->Insert( nIndex, ipAttribute );
				}
			}
		}

		// Provide the collected Attributes to the caller
		*pAttributes = ipAttributes.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04179");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueFromList::raw_IsConfigured(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		*pbValue = m_ipValueList->Size > 0 ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04860");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueFromList::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19587", pstrComponentDescription != NULL)

		*pstrComponentDescription = _bstr_t("Value from list").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04181")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IInputValidator
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueFromList::raw_ValidateInput(ITextInput * pTextInput, 
											   VARIANT_BOOL * pbSuccessful)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Retrieve text input string
		string strInput = asString(pTextInput->GetText());
		if (!m_bCaseSensitive) ::makeUpperCase(strInput);

		// if the input text string is one of the values in the list
		if (m_ipValueList)
		{
			// iterate through all elements in the vector
			long nSize = m_ipValueList->Size;
			string strValue("");
			for (long n=0; n<nSize; n++)
			{
				strValue = asString(_bstr_t(m_ipValueList->GetItem(n)));
				if (!m_bCaseSensitive) ::makeUpperCase(strValue);

				if (strInput == strValue)
				{
					*pbSuccessful = VARIANT_TRUE;
					return S_OK;
				}
			}
		}

		*pbSuccessful = VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04182")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueFromList::raw_GetInputType(BSTR * pstrInputType)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return raw_GetComponentDescription(pstrInputType);
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueFromList::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CValueFromList::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		UCLID_AFVALUEFINDERSLib::IValueFromListPtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI08268", ipSource != NULL);

		m_bCaseSensitive = (ipSource->GetIsCaseSensitive()==VARIANT_TRUE) ? true : false;

		ICopyableObjectPtr ipCopyObj = ipSource->GetValueList();
		ASSERT_RESOURCE_ALLOCATION("ELI08269", ipCopyObj != NULL);
		m_ipValueList = ipCopyObj->Clone();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08270");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueFromList::raw_Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy(CLSID_ValueFromList);
		ASSERT_RESOURCE_ALLOCATION("ELI19352", ipObjCopy != NULL);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04494");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IValueFromList
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueFromList::get_IsCaseSensitive(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_bCaseSensitive ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04544")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueFromList::put_IsCaseSensitive(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bCaseSensitive = newVal == VARIANT_TRUE;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04545")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueFromList::get_ValueList(IVariantVector **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		IVariantVectorPtr ipShallowCopy = m_ipValueList;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04546")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueFromList::put_ValueList(IVariantVector *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (m_ipValueList) m_ipValueList = NULL;

		m_ipValueList = newVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04547")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueFromList::LoadListFromFile(BSTR strFileFullName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		validateLicense();
		
		// reset content of vector
		m_ipValueList->Clear();
		
		string strFileName = asString( strFileFullName );
		ifstream ifs(strFileName.c_str());
		CommentedTextFileReader fileReader(ifs, "//", true);
		string strLine("");
		while (!ifs.eof())
		{
			strLine = fileReader.getLineText();
			if (!strLine.empty())
			{
				_bstr_t _bstrEntry(strLine.c_str());
				// prevent duplicate entries
				if (m_ipValueList->Contains(_bstrEntry) == VARIANT_TRUE)
				{
					CString zMsg("");
					zMsg.Format("The list already has the entry \"%s\"."
						" The duplicate entry will be removed from the list", 
						strLine.c_str());
					::MessageBox(NULL, zMsg, "Duplicate Entries", MB_OK|MB_ICONWARNING);
					continue;
				}

				m_ipValueList->PushBack(_bstrEntry);
			}
		};
		
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04548")
		
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueFromList::SaveListToFile(BSTR strFileFullName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		validateLicense();

		
		string strFileToSave = asString( strFileFullName );
		
		// always overwrite if the file exists
		ofstream ofs(strFileToSave.c_str(), ios::out | ios::trunc);
		
		// iterate through the vector
		string strValue("");
		long nSize = m_ipValueList->Size;
		for (long n=0; n<nSize; n++)
		{
			strValue = asString(_bstr_t(m_ipValueList->GetItem(n)));
			// save the value to the file
			ofs << strValue << endl;
		}
		
		ofs.close();
		waitForFileToBeReadable(strFileToSave);
		
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05591")
		
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueFromList::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_ValueFromList;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueFromList::IsDirty(void)
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
			IPersistStreamPtr ipPersistStream(m_ipValueList);
			if (ipPersistStream==NULL)
			{
				throw UCLIDException("ELI04795", "Object does not support persistence!");
			}
			
			hr = ipPersistStream->IsDirty();
		}

		return hr;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04796");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueFromList::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Clear the variables first
		m_bCaseSensitive = false;
		m_ipValueList = NULL;

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
			UCLIDException ue( "ELI07650", "Unable to load newer ValueFromList Finder." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		if (nDataVersion >= 1)
		{
			dataReader >> m_bCaseSensitive;
		}

		// Read the clue list
		IPersistStreamPtr ipObj;
		::readObjectFromStream(ipObj, pStream, "ELI09969");
		if (ipObj == NULL)
		{
			throw UCLIDException( "ELI04680", 
				"Variant Vector for Value List object could not be read from stream!" );
		}
		else
		{
			m_ipValueList = ipObj;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04678");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueFromList::Save(IStream *pStream, BOOL fClearDirty)
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
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write( &nDataLength, sizeof(nDataLength), NULL );
		pStream->Write( data.getData(), nDataLength, NULL );

		// Separately write the Value list to the IStream object
		IPersistStreamPtr ipPersistentObj( m_ipValueList );
		if (ipPersistentObj == NULL)
		{
			throw UCLIDException( "ELI04681", 
				"Value From List object does not support persistence!" );
		}
		else
		{
			::writeObjectToStream( ipPersistentObj, pStream, "ELI09924", fClearDirty );
		}

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04679");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CValueFromList::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CValueFromList::validateLicense()
{
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI04190", "Value From List" );
}
//-------------------------------------------------------------------------------------------------
