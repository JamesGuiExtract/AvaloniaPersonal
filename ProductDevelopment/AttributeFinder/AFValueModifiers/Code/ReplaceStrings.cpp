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
  m_lCurrentAttributeNumber(1),
  m_lAttributeCount(1)
{
	try
	{
		ASSERT_RESOURCE_ALLOCATION("ELI06628", m_ipReplaceInfos != __nullptr);

		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI13085", m_ipMiscUtils != __nullptr );
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI06629")
}
//-------------------------------------------------------------------------------------------------
CReplaceStrings::~CReplaceStrings()
{
	try
	{
		m_ipMiscUtils = __nullptr;
		m_ipReplaceInfos = __nullptr;
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
		ASSERT_RESOURCE_ALLOCATION( "ELI09291", ipAttribute != __nullptr );

		ISpatialStringPtr ipInputText = ipAttribute->GetValue();
		ASSERT_ARGUMENT("ELI06631", ipInputText != __nullptr);

		// Update the progress status if it exists
		if (m_ipProgressStatus)
		{
			m_ipProgressStatus->StartNextItemGroup(
				get_bstr_t( "Replacing string " + asString(m_lCurrentAttributeNumber) + 
				" of " + asString(m_lAttributeCount) ), 
				ms_lPROGRESS_ITEMS_PER_REPLACEMENT);
		}

		// Call local method
		modifyValue(ipInputText, pOriginInput, NULL);
		
		if (m_ipProgressStatus)
		{
			// Increment the string number
			m_lCurrentAttributeNumber++;
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
		ASSERT_RESOURCE_ALLOCATION( "ELI08697", ipAFUtility != __nullptr );

		// Use Attributes as smart pointer
		IIUnknownVectorPtr ipAttributes( pAttributes );
		ASSERT_RESOURCE_ALLOCATION("ELI08698", ipAttributes != __nullptr);

		// Store the progress status object
		m_ipProgressStatus = pProgressStatus;

		// Check if progress status object exists
		if (m_ipProgressStatus)
		{
			// Initialize the string number counter
			m_lCurrentAttributeNumber = 1;

			// Default count to 0
			m_lAttributeCount = 0;

			// Compute the total attribute count (sum of the size of each attribute)
			// [FlexIDSCore #3566]
			long lSize = ipAttributes->Size();
			for (long i=0; i < lSize; i++)
			{
				IAttributePtr ipAttr = ipAttributes->At(i);
				ASSERT_RESOURCE_ALLOCATION("ELI26470", ipAttr != __nullptr);

				m_lAttributeCount += ipAttr->GetAttributeSize();
			}

			// Initialize progress status object
			m_ipProgressStatus->InitProgressStatus("Replacing strings", 0, 
				m_lAttributeCount * ms_lPROGRESS_ITEMS_PER_REPLACEMENT, VARIANT_TRUE);
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
		ASSERT_ARGUMENT("ELI19605", pstrComponentDescription != __nullptr)

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
		ASSERT_RESOURCE_ALLOCATION("ELI08285", ipSource != __nullptr);

		ICopyableObjectPtr ipCopyObj = ipSource->GetReplacements();
		ASSERT_RESOURCE_ALLOCATION("ELI08286", ipCopyObj != __nullptr);

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
		ASSERT_RESOURCE_ALLOCATION("ELI08360", ipObjCopy != __nullptr);

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
		m_ipReplaceInfos = __nullptr;

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
		if (ipObj == __nullptr)
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
		if (ipObj == __nullptr)
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
		ASSERT_RESOURCE_ALLOCATION("ELI07437", ipInputDoc != __nullptr);

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
	ASSERT_RESOURCE_ALLOCATION("ELI14638", ipAFDoc != __nullptr);

	ISpatialStringPtr ipInputText(pText);
	ASSERT_RESOURCE_ALLOCATION( "ELI09305", ipInputText != __nullptr );

	IIUnknownVectorPtr ipExpandedReplaceList = m_cachedListLoader.expandTwoColumnList(
		m_ipReplaceInfos, ';', ipAFDoc);
	ASSERT_RESOURCE_ALLOCATION("ELI30068", ipExpandedReplaceList != __nullptr);

	replaceValue(ipInputText, ipExpandedReplaceList, pProgressStatus);
}
//-------------------------------------------------------------------------------------------------
void CReplaceStrings::replaceValue(ISpatialStringPtr ipInputText, IIUnknownVectorPtr ipReplaceInfos, 
								   IProgressStatus* pProgressStatus)
{
	// ensure valid arguments
	ASSERT_ARGUMENT("ELI14560", ipInputText != __nullptr);
	ASSERT_ARGUMENT("ELI14561", ipReplaceInfos != __nullptr);

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
		ASSERT_RESOURCE_ALLOCATION("ELI14562", ipKeyValuePair != __nullptr);
		
		// Assign the key and value of the pair
		_bstr_t _bstrToBeReplace = ipKeyValuePair->StringKey;
		_bstr_t _bstrReplacement = ipKeyValuePair->StringValue;

		IRegularExprParserPtr ipParser = __nullptr;
		if (m_bAsRegExpr)
		{
			ipParser = m_ipMiscUtils->GetNewRegExpParserInstance("ReplaceStrings");
			ASSERT_RESOURCE_ALLOCATION("ELI06627", ipParser != __nullptr);
		}
		ipInputText->Replace(_bstrToBeReplace, _bstrReplacement,
			asVariantBool(m_bIsCaseSensitive), 0, ipParser);
	}

	// mark progress status as complete if it exists
	if (ipProgressStatus)
	{
		ipProgressStatus->CompleteCurrentItemGroup();
	}
}
//-------------------------------------------------------------------------------------------------
void CReplaceStrings::validateLicense()
{
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI04211", "Replace Strings" );
}
//-------------------------------------------------------------------------------------------------
