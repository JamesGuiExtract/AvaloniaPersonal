// ReplaceStrings.cpp : Implementation of CReplaceStrings
#include "stdafx.h"
#include "AFValueModifiers.h"
#include "ReplaceStrings.h"
#include "Common.h"

#include <UCLIDException.h>
#include <CommentedTextFileReader.h>
#include <StringTokenizer.h>
#include <COMUtils.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <AFTagManager.h>
#include <ComponentLicenseIDs.h>

#include <fstream>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 2;

//-------------------------------------------------------------------------------------------------
// CReplaceStrings
//-------------------------------------------------------------------------------------------------
CReplaceStrings::CReplaceStrings()
: m_bIsCaseSensitive(false),
  m_bAsRegExpr(false),
  m_ipReplaceInfos(CLSID_IUnknownVector),
  m_bDirty(false),
  m_lCurrentStringNumber(1),
  m_lTotalStringsToReplace(1)
{
	try
	{
		ASSERT_RESOURCE_ALLOCATION("ELI06628", m_ipReplaceInfos != NULL);

		IMiscUtilsPtr ipMiscUtils(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI13085", ipMiscUtils != NULL );

		m_ipRegExpr = ipMiscUtils->GetNewRegExpParserInstance("ReplaceStrings");
		ASSERT_RESOURCE_ALLOCATION("ELI06627", m_ipRegExpr != NULL);

		m_cachedStringListLoader.m_obj == NULL;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI06629")
}
//-------------------------------------------------------------------------------------------------
CReplaceStrings::~CReplaceStrings()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16364");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReplaceStrings::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IReplaceStrings,
		&IID_IAttributeModifyingRule,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_ILicensedComponent,
		&IID_IMustBeConfiguredObject,
		&IID_IDocumentPreprocessor,
		&IID_IOutputHandler
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IAttributeModifyingRule
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReplaceStrings::raw_ModifyValue(IAttribute* pAttribute, IAFDocument* pOriginInput, 
											  IProgressStatus* pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		IAttributePtr	ipAttribute(pAttribute);
		ASSERT_RESOURCE_ALLOCATION( "ELI09291", ipAttribute != NULL );

		ISpatialStringPtr ipInputText = ipAttribute->GetValue();
		ASSERT_ARGUMENT("ELI06631", ipInputText != NULL);

		// Update the progress status if it exists
		if (m_ipProgressStatus)
		{
			m_ipProgressStatus->StartNextItemGroup(
				get_bstr_t( "Replacing string " + asString(m_lCurrentStringNumber) + 
				" of " + asString(m_lTotalStringsToReplace) ), 
				ms_lPROGRESS_ITEMS_PER_REPLACEMENT);
		}

		// Call local method
		modifyValue(ipInputText, pOriginInput, NULL);
		
		if (m_ipProgressStatus)
		{
			// Increment the string number
			m_lCurrentStringNumber++;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04209");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IOutputHandler
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReplaceStrings::raw_ProcessOutput(IIUnknownVector * pAttributes, IAFDocument * pDoc,
												IProgressStatus* pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Create AFUtility object
		IAFUtilityPtr ipAFUtility( CLSID_AFUtility );
		ASSERT_RESOURCE_ALLOCATION( "ELI08697", ipAFUtility != NULL );

		// Use Attributes as smart pointer
		IIUnknownVectorPtr ipAttributes( pAttributes );
		ASSERT_RESOURCE_ALLOCATION("ELI08698", ipAttributes != NULL);

		// Store the progress status object
		m_ipProgressStatus = pProgressStatus;

		// Check if progress status object exists
		if (m_ipProgressStatus)
		{
			// Initialize the string number counter
			m_lCurrentStringNumber = 1;

			// Store the number of strings
			m_lTotalStringsToReplace = ipAttributes->Size();

			// Initialize progress status object
			m_ipProgressStatus->InitProgressStatus("Replacing strings", 0, 
				m_lTotalStringsToReplace * ms_lPROGRESS_ITEMS_PER_REPLACEMENT, VARIANT_TRUE);
		}

		// Apply Attribute Modification
		ipAFUtility->ApplyAttributeModifier(ipAttributes, pDoc, this, VARIANT_TRUE);

		// Notify that the progress status is complete
		if (m_ipProgressStatus)
		{
			m_ipProgressStatus->CompleteCurrentItemGroup();
		}

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08700");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReplaceStrings::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19605", pstrComponentDescription != NULL)

		*pstrComponentDescription = _bstr_t("Replace strings").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04210");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IReplaceStrings
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReplaceStrings::get_IsCaseSensitive(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = asVariantBool(m_bIsCaseSensitive);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04300");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReplaceStrings::put_IsCaseSensitive(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bIsCaseSensitive = asCppBool(newVal);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04301");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReplaceStrings::get_AsRegularExpr(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = asVariantBool(m_bAsRegExpr);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04950");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReplaceStrings::put_AsRegularExpr(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bAsRegExpr = asCppBool(newVal);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04951");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReplaceStrings::get_Replacements(IIUnknownVector **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		IIUnknownVectorPtr ipShallowCopy = m_ipReplaceInfos;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04302");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReplaceStrings::put_Replacements(IIUnknownVector *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// release old address
		if (m_ipReplaceInfos != NULL) m_ipReplaceInfos = NULL;

		m_ipReplaceInfos = newVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04303");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReplaceStrings::LoadReplaceInfoFromFile(BSTR strFileFullName, BSTR cDelimiter)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// delimiter can only be a single char
		string strDelim = asString(cDelimiter);
		CString zFileName = (char*)_bstr_t(strFileFullName);
		if (strDelim.size() > 1)
		{
			UCLIDException uclidException("ELI04313", "Delimiter shall be one character long.");
			uclidException.addDebugInfo("Delimiter", strDelim);
			throw uclidException;
		}
		
		// reset content of map
		m_ipReplaceInfos->Clear();
		
		ifstream ifs(zFileName);
		CommentedTextFileReader fileReader(ifs, "//", true);
		StringTokenizer tokenizer(strDelim[0]);

		while (!ifs.eof())
		{
			string strLine("");
			strLine = fileReader.getLineText();
			if (!strLine.empty())
			{
				// tokenize the line text
				vector<string> vecTokens;
				tokenizer.parse(strLine, vecTokens);
				if (vecTokens.size() != 2)
				{
					UCLIDException uclidException("ELI04315", "This line of text can not be parsed successfully with provided delimiter.");
					uclidException.addDebugInfo("LineText", strLine);
					uclidException.addDebugInfo("Delimiter", strDelim);
					throw uclidException;
				}
				
				IStringPairPtr ipReplacement(CLSID_StringPair);
				ipReplacement->StringKey = _bstr_t(vecTokens[0].c_str());
				ipReplacement->StringValue = _bstr_t(vecTokens[1].c_str());
				m_ipReplaceInfos->PushBack(ipReplacement);
			}
		};

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04314");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReplaceStrings::SaveReplaceInfoToFile(BSTR strFileFullName, BSTR cDelimiter)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		string strFileToSave = asString(strFileFullName);
		string strDelimiter = asString(cDelimiter);
	
		// always overwrite if the file exists
		ofstream ofs(strFileToSave.c_str(), ios::out | ios::trunc);

		// check whether the file has been created successfully
		if ( ofs.fail() )
		{
			AfxMessageBox( "The file can not be created. Please check the path and file name." );
			return S_FALSE;
		}
		
		// iterate through the vector
		string strToBeReplaced(""), strReplacement("");
		long nSize = m_ipReplaceInfos->Size();
		for (long n=0; n<nSize; n++)
		{
			IStringPairPtr ipStringPair(m_ipReplaceInfos->At(n));
			if (ipStringPair)
			{
				strToBeReplaced = ipStringPair->StringKey;
				strReplacement = ipStringPair->StringValue;
				// save the value to the file
				ofs << strToBeReplaced << strDelimiter << strReplacement << endl;
			}
		}

		ofs.close();
		waitForFileToBeReadable(strFileToSave);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05573");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReplaceStrings::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();
		UCLID_AFVALUEMODIFIERSLib::IReplaceStringsPtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI08285", ipSource != NULL);

		ICopyableObjectPtr ipCopyObj = ipSource->GetReplacements();
		ASSERT_RESOURCE_ALLOCATION("ELI08286", ipCopyObj != NULL);

		m_ipReplaceInfos = ipCopyObj->Clone();
		
		m_bIsCaseSensitive = asCppBool( ipSource->GetIsCaseSensitive() );
		m_bAsRegExpr = asCppBool( ipSource->GetAsRegularExpr() );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04455");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReplaceStrings::raw_Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy(CLSID_ReplaceStrings);
		ASSERT_RESOURCE_ALLOCATION("ELI08360", ipObjCopy != NULL);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04456");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReplaceStrings::raw_IsConfigured(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		*pbValue = asVariantBool(m_ipReplaceInfos->Size() > 0);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04846")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReplaceStrings::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReplaceStrings::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_ReplaceStrings;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReplaceStrings::IsDirty(void)
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
			IPersistStreamPtr ipPersistStream(m_ipReplaceInfos);
			if (ipPersistStream==NULL)
			{
				throw UCLIDException("ELI04797", "Object does not support persistence!");
			}
			
			hr = ipPersistStream->IsDirty();
		}

		return hr;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04798");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
// NOTES about versions:
// Version 1:
//   * Saved: 
//            data version,
//            case-sensitivity flag,
//            collection of replacement strings
// Version 2:
//   * Additionally saved:
//            regular expression flag
//   * NOTE:
//            new flag located immediately after Case Sensitive flag
STDMETHODIMP CReplaceStrings::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Clear the variables first
		m_bIsCaseSensitive = false;
		m_bAsRegExpr = false;
		m_ipReplaceInfos = NULL;

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
			UCLIDException ue( "ELI07660", "Unable to load newer ReplaceStrings Modifier." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		if (nDataVersion >= 1)
		{
			dataReader >> m_bIsCaseSensitive;
		}
		
		if (nDataVersion >= 2)
		{
			dataReader >> m_bAsRegExpr;
		}

		// Separately read in the replacement info
		IPersistStreamPtr ipObj;
		::readObjectFromStream(ipObj, pStream, "ELI09970");
		if (ipObj == NULL)
		{
			throw UCLIDException( "ELI04716", 
				"Replacement info could not be read from stream!" );
		}

		m_ipReplaceInfos = ipObj;

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04713");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReplaceStrings::Save(IStream *pStream, BOOL fClearDirty)
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
		dataWriter << m_bIsCaseSensitive;
		// m_bAsRegExpr starts from version 2
		dataWriter << m_bAsRegExpr;

		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write( &nDataLength, sizeof(nDataLength), NULL );
		pStream->Write( data.getData(), nDataLength, NULL );

		// Separately write the Replacements list to the IStream object
		IPersistStreamPtr ipObj( m_ipReplaceInfos );
		if (ipObj == NULL)
		{
			throw UCLIDException( "ELI04717", 
				"Replacement info object does not support persistence!" );
		}
		else
		{
			::writeObjectToStream(ipObj, pStream, "ELI09925", fClearDirty);
		}

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04714");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReplaceStrings::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// IDocumentPreprocessor
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReplaceStrings::raw_Process(IAFDocument* pDocument, IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		IAFDocumentPtr ipInputDoc(pDocument);
		ASSERT_RESOURCE_ALLOCATION("ELI07437", ipInputDoc != NULL);

		// Call local method
		modifyValue(ipInputDoc->Text, pDocument, pProgressStatus);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07436");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CReplaceStrings::modifyValue(ISpatialString* pText, IAFDocument* pDocument, 
								  IProgressStatus* pProgressStatus)
{
	IAFDocumentPtr ipAFDoc(pDocument);
	ASSERT_RESOURCE_ALLOCATION("ELI14638", ipAFDoc != NULL);

	ISpatialStringPtr ipInputText(pText);
	ASSERT_RESOURCE_ALLOCATION( "ELI09305", ipInputText != NULL );

	// Get the string key value pair in the first row of the list box
	IStringPairPtr ipKeyValuePair(m_ipReplaceInfos->At(0));
	ASSERT_RESOURCE_ALLOCATION("ELI14557", ipKeyValuePair != NULL);
	
	//Define an IMiscUtilsPtr object used to get string from file
	IMiscUtilsPtr ipMiscUtils(CLSID_MiscUtils);
	ASSERT_RESOURCE_ALLOCATION("ELI14558", ipMiscUtils != NULL );

	// Remove the header of the string if it is a file name,
	// return the original string if it is not a file name
	_bstr_t bstrFirstEntry = ipKeyValuePair->StringKey;
	_bstr_t bstrAfterRemoveHeader = ipMiscUtils->GetFileNameWithoutHeader(bstrFirstEntry);

	// Expand tags only happens when the string is a file name
	// if it is not, all tags will be treated as common strings [P16: 2118]

	// Compare the new string with the original string
	// If different, which means the header has been removed, we should
	// try to load string pairs from the file
	if (bstrAfterRemoveHeader != bstrFirstEntry)
	{
		string strFileNameWithDelimiter = asString(bstrAfterRemoveHeader);

		// Define a tag manager object to expand tags
		AFTagManager tagMgr;
		// Expand tags and functions in the file name
		strFileNameWithDelimiter = tagMgr.expandTagsAndFunctions(strFileNameWithDelimiter, ipAFDoc);

		// Get the position of the first ";"
		int nIndexEndFileName = strFileNameWithDelimiter.find_first_of(';');
		if (nIndexEndFileName < 0)
		{
			UCLIDException uclidException("ELI14567", "The delimiter and the file name should be separated by ';'.");
			uclidException.addDebugInfo("Separator", ";");
			throw uclidException;
		}

		// Remove the delimiter and return the real file name
		string strFileName = strFileNameWithDelimiter.substr(0, nIndexEndFileName);
		strFileName = trim(strFileName, "", " ");

		// Get the delimiter string
		string strDelimiter = strFileNameWithDelimiter.substr(nIndexEndFileName + 1, string::npos);
		strDelimiter = trim(strDelimiter, " ", "");

		// Check if the delimiter is more than one character
		if (strDelimiter.length() != 1)
		{
			UCLIDException uclidException("ELI14569", "Delimiter shall be one character long.");
			uclidException.addDebugInfo("Delimiter", strDelimiter);
			throw uclidException;
		}

		if (m_cachedStringListLoader.m_obj == NULL)
		{
			m_cachedStringListLoader.m_obj.CreateInstance(CLSID_VariantVector);
			ASSERT_RESOURCE_ALLOCATION("ELI14627", m_cachedStringListLoader.m_obj != NULL );
		}

		// perform any appropriate auto-encrypt actions on the clue list file
		ipMiscUtils->AutoEncryptFile(get_bstr_t(strFileName.c_str()),
			get_bstr_t(gstrAF_AUTO_ENCRYPT_KEY_PATH.c_str()));

		// If the header has been removed, call getStringList() to load the strings inside the file
		IVariantVectorPtr ipDynamicRepInfo = getStringList(strFileName);
		ASSERT_RESOURCE_ALLOCATION("ELI14629", ipDynamicRepInfo != NULL );

		// Create IIUnknownVectorPtr object to put string pairs
		UCLID_COMUTILSLib::IIUnknownVectorPtr ipDynamicVector(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI14566", ipDynamicVector != NULL);

		// Create a tokenizer
		StringTokenizer tokenizer(strDelimiter[0]);

		for (int i = 0; i < ipDynamicRepInfo->Size; i++)
		{
			// Get one line of text of string
			_bstr_t bstrLine = (ipDynamicRepInfo->Item[i]).bstrVal;
			string strLine = bstrLine;

			// Tokenize the line text
			vector<string> vecTokens;
			tokenizer.parse(strLine, vecTokens);

			// If the size of the tokenized vector is not 2
			if (vecTokens.size() != 2)
			{
				UCLIDException uclidException("ELI14570", "This line of text can not be parsed successfully with provided delimiter.");
				uclidException.addDebugInfo("LineText", strLine);
				uclidException.addDebugInfo("Delimiter", strDelimiter);
				throw uclidException;
			}
			
			// Create an IStringPairPtr object
			UCLID_COMUTILSLib::IStringPairPtr ipReplacement(CLSID_StringPair);
			ASSERT_RESOURCE_ALLOCATION("ELI14577", ipReplacement != NULL);

			// Put the tokens into string pair object
			ipReplacement->StringKey = get_bstr_t(vecTokens[0].c_str());
			ipReplacement->StringValue = get_bstr_t(vecTokens[1].c_str());

			// Push the string pair object into IUnknownVector
			ipDynamicVector->PushBack(ipReplacement);
		}
		// If the return IUnknownVector is not empty, which means we have successfully read string pairs out of a file,
		// just do the replacement using this IUnknownVector.
		if (ipDynamicVector->Size() > 0)
		{
			 //Call replaceValue() method to do replacement
			replaceValue(ipInputText, ipDynamicVector, pProgressStatus);
		}
	}
	// If the string is not a file name, treated as a common string to do replacement
	else
	{
		//Call replaceValue() method to do replacement
		replaceValue(ipInputText, m_ipReplaceInfos, pProgressStatus);
	}
}
//-------------------------------------------------------------------------------------------------
void CReplaceStrings::replaceValue(ISpatialStringPtr ipInputText, IIUnknownVectorPtr ipReplaceInfos, 
								   IProgressStatus* pProgressStatus)
{
	// ensure valid arguments
	ASSERT_ARGUMENT("ELI14560", ipInputText != NULL);
	ASSERT_ARGUMENT("ELI14561", ipReplaceInfos != NULL);

	// use a smart pointer for the progress status object
	IProgressStatusPtr ipProgressStatus = pProgressStatus;

	// Get size of replacement info
	long nSize = ipReplaceInfos->Size();

	// initialize progress status object if it exists
	if (pProgressStatus)
	{
		pProgressStatus->InitProgressStatus("Replacing strings", 0, 
			nSize * ms_lPROGRESS_ITEMS_PER_REPLACEMENT, VARIANT_FALSE);
	}

	for (long n = 0; n < nSize; n++)
	{
		// update progress status if progress status was requested
		if (ipProgressStatus)
		{
			ipProgressStatus->StartNextItemGroup(
				get_bstr_t("Replacing string " + asString(n+1) + " of " + asString(nSize)), 
				ms_lPROGRESS_ITEMS_PER_REPLACEMENT);
		}

		// get each string pair out from the vector
		IStringPairPtr ipKeyValuePair(ipReplaceInfos->At(n));
		ASSERT_RESOURCE_ALLOCATION("ELI14562", ipKeyValuePair != NULL);
		
		// Assign the key and value of the pair
		_bstr_t _bstrToBeReplace = ipKeyValuePair->StringKey;
		_bstr_t _bstrReplacement = ipKeyValuePair->StringValue;

		ipInputText->Replace(_bstrToBeReplace, _bstrReplacement,
			asVariantBool(m_bIsCaseSensitive), 0, m_bAsRegExpr ? m_ipRegExpr : NULL);
	}

	// mark progress status as complete if it exists
	if (ipProgressStatus)
	{
		ipProgressStatus->CompleteCurrentItemGroup();
	}
}
//-------------------------------------------------------------------------------------------------
IVariantVectorPtr CReplaceStrings::getStringList(std::string strFile)
{
	// Call loadObjectFromFile() to load the string list from the file
	m_cachedStringListLoader.loadObjectFromFile(strFile);

	// Return IVariantVector which contains string list
	return m_cachedStringListLoader.m_obj;
}
//-------------------------------------------------------------------------------------------------
void CReplaceStrings::validateLicense()
{
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI04211", "Replace Strings" );
}
//-------------------------------------------------------------------------------------------------
