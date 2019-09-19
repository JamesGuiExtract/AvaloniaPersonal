// FileNamePattern.cpp : Implementation of CFileNamePattern

#include "stdafx.h"
#include "FileNamePattern.h"
#include "ESSkipConditions.h"
#include "..\..\..\..\..\ProductDevelopment\AttributeFinder\AFCore\Code\Common.h"
#include "SkipConditionUtils.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <Misc.h>
#include <ComponentLicenseIDs.h>

#include <io.h>

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
// current version
const unsigned long gnCurrentVersion = 1;

// Default filename to be tested (P13 #4781)
static const string gstrDEFAULT_FILENAME = "<SourceDocName>";

//--------------------------------------------------------------------------------------------------
// CFileNamePattern
//--------------------------------------------------------------------------------------------------
CFileNamePattern::CFileNamePattern()
:m_bDirty(false),
m_strFileName(gstrDEFAULT_FILENAME),
m_ipMiscUtils(NULL),
m_bDoesDoesNot(true),
m_bContainMatch(true),
m_bCaseSensitive(false),
m_bIsRegExpFromFile(false),
m_cachedRegExLoader(gstrAF_AUTO_ENCRYPT_KEY_PATH.c_str())
{
	try
	{
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI13648")
}
//--------------------------------------------------------------------------------------------------
CFileNamePattern::~CFileNamePattern()
{
	try
	{
		m_ipMiscUtils = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16561");
}

//--------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFileNamePattern::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ILicensedComponent,
		&IID_IFileNamePatternFAMCondition,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_IPersistStream,
		&IID_IMustBeConfiguredObject,
		&IID_IFAMCondition,
		&IID_IAccessRequired
	};

	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//--------------------------------------------------------------------------------------------------
// IFileNamePatternFAMCondition
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFileNamePattern::get_DoesContainOrMatch(VARIANT_BOOL* pRetVal)
{
	try
	{
		// Check license
		validateLicense();

		*pRetVal = asVariantBool(m_bDoesDoesNot);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13614");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFileNamePattern::put_DoesContainOrMatch(VARIANT_BOOL newVal)
{
	try
	{
		// Check license
		validateLicense();

		m_bDoesDoesNot = (newVal==VARIANT_TRUE);
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13615");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFileNamePattern::get_ContainMatch(VARIANT_BOOL* pRetVal)
{
	try
	{
		// Check license
		validateLicense();

		*pRetVal = asVariantBool(m_bContainMatch);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13621");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFileNamePattern::put_ContainMatch(VARIANT_BOOL newVal)
{
	try
	{
		// Check license
		validateLicense();

		m_bContainMatch = (newVal==VARIANT_TRUE);
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13622");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFileNamePattern::get_IsCaseSensitive(VARIANT_BOOL* pRetVal)
{
	try
	{
		// Check license
		validateLicense();

		*pRetVal = asVariantBool(m_bCaseSensitive);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13617");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFileNamePattern::put_IsCaseSensitive(VARIANT_BOOL newVal)
{
	try
	{
		// Check license
		validateLicense();

		m_bCaseSensitive = (newVal==VARIANT_TRUE);
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13618");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFileNamePattern::get_IsRegFromFile(VARIANT_BOOL* pRetVal)
{
	try
	{
		// Check license
		validateLicense();

		*pRetVal = asVariantBool(m_bIsRegExpFromFile);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13619");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFileNamePattern::put_IsRegFromFile(VARIANT_BOOL newVal)
{
	try
	{
		// Check license
		validateLicense();

		m_bIsRegExpFromFile = (newVal==VARIANT_TRUE);
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13620");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFileNamePattern::get_FileString(BSTR *strFileString)
{
	try
	{
		// Check license
		validateLicense();

		*strFileString = _bstr_t(m_strFileName.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13624");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFileNamePattern::put_FileString(BSTR strFileString)
{
	try
	{
		// Check license
		validateLicense();

		string strFile = asString(strFileString);

		// Create a local IFAMTagManagerPtr object
		UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr ipFAMTagManager;
		ipFAMTagManager.CreateInstance(CLSID_FAMTagManager);
		ASSERT_RESOURCE_ALLOCATION("ELI14421", ipFAMTagManager != __nullptr);

		// make sure the file exists
		// or that it contains valid string tags
		if (ipFAMTagManager->StringContainsInvalidTags(strFile.c_str()) == VARIANT_TRUE)
		{

			UCLIDException ue("ELI14422", "The FAM condition filename contains invalid tags!");
			ue.addDebugInfo("File", strFile);
			throw ue;
		}

		m_strFileName = strFile;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13625");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFileNamePattern::get_RegExpFileName(BSTR *strFileString)
{
	try
	{
		// Check license
		validateLicense();

		*strFileString = _bstr_t(m_strRegFileName.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13627");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFileNamePattern::put_RegExpFileName(BSTR strFileString)
{
	try
	{
		// Check license
		validateLicense();

		string strFile = asString( strFileString );

		// Create a local IFAMTagManagerPtr object
		UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr ipFAMTagManager;
		ipFAMTagManager.CreateInstance(CLSID_FAMTagManager);
		ASSERT_RESOURCE_ALLOCATION("ELI14423", ipFAMTagManager != __nullptr);

		// make sure the file exists
		// or that it contains valid string tags
		if (ipFAMTagManager->StringContainsInvalidTags(strFile.c_str()) == VARIANT_TRUE)
		{

			UCLIDException ue("ELI14424", "The regular expression path contains invalid tags!");
			ue.addDebugInfo("File", strFile);
			throw ue;
		}
		else if (ipFAMTagManager->StringContainsTags(strFile.c_str()) == VARIANT_FALSE)
		{
			if (!isAbsolutePath(strFile))
			{
				UCLIDException ue("ELI14425", "Specification of a relative path is not allowed!");
				ue.addDebugInfo("File", strFile);
				throw ue;
			}

			// Validate the existence of the regular expression file
			validateFileOrFolderExistence(strFile);
		}

		m_strRegFileName = strFile;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13631");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFileNamePattern::get_RegPattern(BSTR *strFileString)
{
	try
	{
		// Check license
		validateLicense();

		*strFileString = _bstr_t(m_strRegPattern.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13632");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFileNamePattern::put_RegPattern(BSTR strFileString)
{
	try
	{
		// Check license
		validateLicense();

		string strPattern = asString( strFileString );

		if (strPattern.empty())
		{
			throw UCLIDException("ELI13634", "Please provide non-empty regular expression.");
		}

		// Put the pattern string to m_strRegPattern
		m_strRegPattern = strPattern;
		m_bDirty = true;

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13633");

	return S_OK;
}

//--------------------------------------------------------------------------------------------------
// ILicensedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFileNamePattern::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	if (pbValue == NULL)
		return E_POINTER;

	try
	{
		// validate license
		validateLicense();
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileNamePattern::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19637", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("File name pattern condition").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13635")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileNamePattern::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		EXTRACT_FAMCONDITIONSLib::IFileNamePatternFAMConditionPtr ipCopyThis(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI13636", ipCopyThis != __nullptr);
		
		// Copy the variables from another object
		m_bDoesDoesNot = ((ipCopyThis->DoesContainOrMatch)==VARIANT_TRUE);
		m_bContainMatch = ((ipCopyThis->ContainMatch)==VARIANT_TRUE);
		m_bCaseSensitive = ((ipCopyThis->IsCaseSensitive)==VARIANT_TRUE);
		m_bIsRegExpFromFile = ((ipCopyThis->IsRegFromFile)==VARIANT_TRUE);

		m_strFileName = asString(ipCopyThis->FileString);
		m_strRegFileName = asString(ipCopyThis->RegExpFileName);
		m_strRegPattern = asString(ipCopyThis->RegPattern);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13637");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileNamePattern::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance(CLSID_FileNamePattern);
		ASSERT_RESOURCE_ALLOCATION("ELI13638", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13639");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileNamePattern::raw_IsConfigured(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		bool bConfigured = true;

		// if m_strFileName is empty 
		// if we select regular expression from file and the file name is empty
		// if we select regular expression from text box and the text box is empty
		if ( m_strFileName.empty() ||
			( (m_bIsRegExpFromFile && m_strRegFileName.empty()) ||
			(!m_bIsRegExpFromFile && m_strRegPattern.empty()) ) )
		{
			bConfigured = false;
		}

		*pbValue = asVariantBool(bConfigured);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13640");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileNamePattern::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_FileNamePattern;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileNamePattern::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileNamePattern::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// reset member variables
		m_bDoesDoesNot = true;
		m_bContainMatch = true;
		m_bCaseSensitive = false;
		m_bIsRegExpFromFile = false;
		m_strFileName = "";
		m_strRegFileName = "";
		m_strRegPattern = "";

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
			UCLIDException ue("ELI13641", "Unable to load newer file name pattern FAM condition component.");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		// Load the variables 
		dataReader >> m_bDoesDoesNot;
		dataReader >> m_bContainMatch;
		dataReader >> m_bCaseSensitive;
		dataReader >> m_bIsRegExpFromFile;
		dataReader >> m_strFileName;
		if (m_bIsRegExpFromFile)
		{
			dataReader >> m_strRegFileName;
		}
		else
		{
			dataReader >> m_strRegPattern;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13642");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileNamePattern::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		// save the variables
		dataWriter << gnCurrentVersion;
		dataWriter << m_bDoesDoesNot;
		dataWriter << m_bContainMatch;
		dataWriter << m_bCaseSensitive;
		dataWriter << m_bIsRegExpFromFile;
		dataWriter << m_strFileName;
		if (m_bIsRegExpFromFile)
		{
			dataWriter << m_strRegFileName;
		}
		else
		{
			dataWriter << m_strRegPattern;
		}

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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13643");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileNamePattern::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// IFAMCondition
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileNamePattern::raw_FileMatchesFAMCondition(IFileRecord* pFileRecord, IFileProcessingDB* pFPDB, 
	long lActionID, IFAMTagManager* pFAMTM, VARIANT_BOOL* pRetVal)
{	
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		IFileRecordPtr ipFileRecord(pFileRecord);
		ASSERT_ARGUMENT("ELI31358", ipFileRecord != __nullptr);

		// String contain the current source file name
		string strSourceFileName = asString(ipFileRecord->Name); //e.g. strSourceFileName = "C:\123.tif"
		// String contain the regular expression
		string strRegExp = "";

		// Get a regex parser
		IRegularExprParserPtr ipParser = getMiscUtils()->GetNewRegExpParserInstance("FileNamePattern");
		ASSERT_RESOURCE_ALLOCATION("ELI13647", ipParser != __nullptr);

		// Get the FAMTagManager smart Pointer
		IFAMTagManagerPtr ipTag = IFAMTagManagerPtr(pFAMTM);
		ASSERT_RESOURCE_ALLOCATION("ELI14405", ipTag != __nullptr);
		
		// Get the regular expression
		if (m_bIsRegExpFromFile)
		{
			// Call ExpandTagsAndTFE() to expand tags and utility functions
			string strRegExprFile = CFAMConditionUtils::ExpandTagsAndTFE(pFAMTM, m_strRegFileName, strSourceFileName);

			// [FlexIDSCore:3643] Load the regular expression from disk if necessary.
			m_cachedRegExLoader.loadObjectFromFile(strRegExprFile);

			// Retrieve the pattern
			strRegExp = (string)m_cachedRegExLoader.m_obj;
			
			ipParser->Pattern = strRegExp.c_str();
		}
		else
		{
			if (m_strRegPattern.length() == 0)
			{
				throw UCLIDException("ELI19283", "No regular expression pattern set!");
			}

			string strRootFolder = "";
			// we will try to use the FPS file dir as the root folder
			// but if there is no FPS file dir we will pass in an empty root 
			// folder
			try
			{
				string rootTag = "<FPSFileDir>";
				strRootFolder = asString( ipTag->ExpandTags(_bstr_t(rootTag.c_str()), strSourceFileName.c_str()) );
			}
			catch(...)
			{
			}

			// Get regular expression from text
			strRegExp = getRegExpFromText(m_strRegPattern, strRootFolder, true, gstrAF_AUTO_ENCRYPT_KEY_PATH);
			ipParser->Pattern = strRegExp.c_str();
		}

		// Call ExpandTagsAndTFE() to expand tags and utility functions
		string strFAMFile = CFAMConditionUtils::ExpandTagsAndTFE(pFAMTM, m_strFileName, strSourceFileName);

		// Set the default return value to false (i.e. do not match)
		*pRetVal = VARIANT_FALSE;

		// Set whether the regular expression parser is case sensitive or not
		ipParser->IgnoreCase = asVariantBool(!m_bCaseSensitive);

		// There are four cases due to the combination of "does", "does not", "contain" and
		// "exactly match", consider each of them separately.
		if(m_bContainMatch)
		{
			// If current selection is "contain"
			IIUnknownVectorPtr ipFound = ipParser->Find(get_bstr_t(strFAMFile), 
				VARIANT_TRUE, VARIANT_FALSE, VARIANT_FALSE);
			if(ipFound->Size() > 0 && m_bDoesDoesNot == true)
			{
				*pRetVal = VARIANT_TRUE;
			}
			else if (ipFound->Size() <= 0 && m_bDoesDoesNot == false)
			{
				*pRetVal = VARIANT_TRUE;
			}
		}
		else
		{
			// If current selection is "exactly match"
			bool bFound = ipParser->StringMatchesPattern(get_bstr_t(strFAMFile)) == VARIANT_TRUE;

			if (m_bDoesDoesNot == true && bFound)
			{
				*pRetVal = VARIANT_TRUE;	
			}
			else if (m_bDoesDoesNot == false && !bFound)
			{
				*pRetVal = VARIANT_TRUE;
			}
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13674");
}

//-------------------------------------------------------------------------------------------------
// IAccessRequired interface implementation
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileNamePattern::raw_RequiresAdminAccess(VARIANT_BOOL* pbResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI31207", pbResult != __nullptr);

		*pbResult = VARIANT_FALSE;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31208");
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CFileNamePattern::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI13664", "File Name Pattern FAM Condition");
}
//-------------------------------------------------------------------------------------------------
IMiscUtilsPtr CFileNamePattern::getMiscUtils()
{
	if (m_ipMiscUtils == __nullptr)
	{
		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI13649", m_ipMiscUtils != __nullptr );
	}
	
	return m_ipMiscUtils;
}
//-------------------------------------------------------------------------------------------------
