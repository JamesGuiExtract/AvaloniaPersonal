// FAMTagManager.cpp : Implementation of CFAMTagManager

#include "stdafx.h"
#include "FAMTagManager.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>

//--------------------------------------------------------------------------------------------------
// Tag names
//--------------------------------------------------------------------------------------------------
const std::string strSOURCE_DOC_NAME_TAG = "<SourceDocName>";
const std::string strFPS_FILE_DIR_TAG = "<FPSFileDir>";

//--------------------------------------------------------------------------------------------------
// CFAMTagManager
//--------------------------------------------------------------------------------------------------
CFAMTagManager::CFAMTagManager()
{
}
//--------------------------------------------------------------------------------------------------
CFAMTagManager::~CFAMTagManager()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16523");
}

//--------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ILicensedComponent,
		&IID_IFAMTagManager
	};

	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//--------------------------------------------------------------------------------------------------
// IFAMTagManager
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::get_FPSFileDir(BSTR *strFPSDir)
{
	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI24981", strFPSDir != __nullptr);

		*strFPSDir = _bstr_t(m_strFPSDir.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14383");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::put_FPSFileDir(BSTR strFPSDir)
{
	try
	{
		// Check license
		validateLicense();

		m_strFPSDir = asString(strFPSDir);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14384");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::ExpandTags(BSTR bstrInput, BSTR bstrSourceName, BSTR *pbstrOutput)
{
	try
	{
		ASSERT_ARGUMENT("ELI20470", pbstrOutput != __nullptr);

		// Check license
		validateLicense();

		// The code is used to expand tags, currently it support <SourceDocName> 
		// and <FPSFile>
		std::string strInput = asString(bstrInput);
		std::string strSourceDocName = asString(bstrSourceName);

		bool bSourceDocNameTagFound = strInput.find(strSOURCE_DOC_NAME_TAG) != string::npos;
		bool bFPSFileDirTagFound = strInput.find(strFPS_FILE_DIR_TAG) != string::npos;

		// expand the strSOURCE_DOC_NAME_TAG tag with the appropriate value
		if (bSourceDocNameTagFound)
		{
			// if there is no current sourcedoc file, this tag cannot
			// be expanded.
			if (strSourceDocName == "")
			{
				string strMsg = "There is no source document available to expand the ";
				strMsg += strSOURCE_DOC_NAME_TAG;
				strMsg += " tag!";
				UCLIDException ue("ELI14387", strMsg);
				ue.addDebugInfo("strInput", strInput);
				throw ue;
			}
			replaceVariable(strInput, strSOURCE_DOC_NAME_TAG, strSourceDocName);
		}

		if (bFPSFileDirTagFound)
		{
			if (m_strFPSDir == "")
			{
				string strMsg = "There is no FPS File Name available to expand the ";
				strMsg += strFPS_FILE_DIR_TAG;
				strMsg += " tag!";
				UCLIDException ue("ELI14388", strMsg);
				ue.addDebugInfo("strInput", strInput);
				throw ue;
			}
			replaceVariable(strInput, strFPS_FILE_DIR_TAG, m_strFPSDir);
		}

		*pbstrOutput = _bstr_t(strInput.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14389");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::GetBuiltInTags(IVariantVector* *ppTags)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		IVariantVectorPtr ipVec(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI14396", ipVec != __nullptr);

		// Push the current tags into vector, currrently we
		// only have <SourceDocName> and <FPSFileDir>
		ipVec->PushBack(get_bstr_t(strSOURCE_DOC_NAME_TAG));
		ipVec->PushBack(get_bstr_t(strFPS_FILE_DIR_TAG));

		*ppTags = ipVec.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14390");
	
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::GetINIFileTags(IVariantVector* *ppTags)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		IVariantVectorPtr ipVec(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI19432", ipVec != __nullptr);

		// Currently we do not have any INI file tags

		*ppTags = ipVec.Detach();	
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14391");
	
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::GetAllTags(IVariantVector* *ppTags)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr ipThis(this);
		ASSERT_RESOURCE_ALLOCATION("ELI14397", ipThis != __nullptr);

		// Call GetBuiltInTags() and GetINIFileTags()
		IVariantVectorPtr ipVec1 = ipThis->GetBuiltInTags();
		IVariantVectorPtr ipVec2 = ipThis->GetINIFileTags();
		// Build a vector of all tags
		ipVec1->Append(ipVec2);

		*ppTags = ipVec1.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14392");
	
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::StringContainsInvalidTags(BSTR strInput, VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// get the input as a STL string
		string stdstrInput = asString(strInput);
		
		// get the tags in the string
		vector<string> vecTagNames;
		getTagNames(stdstrInput, vecTagNames);

		// if we reached here, that means that all tags defined were valid.
		// so return true.
		*pbValue = VARIANT_FALSE;
		return S_OK;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI14393");
	
	// if we reached here, its because there were incomplete tags or something
	// else went wrong, which indicates that there were some problems with
	// tag specifications.
	*pbValue = VARIANT_TRUE;
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::StringContainsTags(BSTR strInput, VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();
		// get the input as a STL string
		string stdstrInput = asString(strInput);
		
		// NOTE: getTagNames() is not supposed to be called for a string
		// that contains invalid tag specifications.  The getTagNames() 
		// method may throw exceptions if there is inappropriate usage of
		// tag indicating characters '<' and '>' (such as non-matching
		// pairs of these indicators, etc).  If such an exception is thrown,
		// it's OK for that to reach the outer scope.

		// get the tags in the string
		vector<string> vecTagNames;
		getTagNames(stdstrInput, vecTagNames);

		// return true as long as there's at least one tag
		*pbValue = vecTagNames.empty() ? VARIANT_FALSE : VARIANT_TRUE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14395");
	
	return S_OK;
}

//--------------------------------------------------------------------------------------------------
// ILicensedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// Private functions
//-------------------------------------------------------------------------------------------------
void CFAMTagManager::getTagNames(const string& strInput, 
							 vector<string>& rvecTagNames) const
{
	// clear the result vector
	rvecTagNames.clear();
	
	// get the tag names
	long nSearchStartPos = 0;
	while (true)
	{
		// find the beginning and ending of a tag
		// Currently we don't support nested tags
		// so nested tags will be considered as an exception
		long nTagStartPos = strInput.find_first_of('<', nSearchStartPos);
		long nTagEndPos = strInput.find_first_of('>', nSearchStartPos);
		
		// if there are no more tags, just return
		if (nTagStartPos == string::npos && nTagEndPos == string::npos)
		{
			break;
		}

		// if there is not a matching pair of tags, that's an error condition
		if (nTagStartPos == string::npos || nTagEndPos == string::npos || 
			nTagEndPos < nTagStartPos)
		{
			UCLIDException ue("ELI09810", "Matching pairs of '<' and '>' not found!");
			ue.addDebugInfo("nTagStartPos", nTagStartPos);
			ue.addDebugInfo("nTagEndPos", nTagEndPos);
			throw ue;
		}

		// a matching pair of tags have been found.  Get the tag name and
		// ensure that it is not empty
		string strTagName = strInput.substr(nTagStartPos,
			nTagEndPos - nTagStartPos + 1);
		if (strTagName.empty())
		{
			throw UCLIDException("ELI09811", "An empty tag name cannot be expanded!");
		}

		// continue searching at the next position
		rvecTagNames.push_back(strTagName);
		nSearchStartPos = nTagEndPos + 1;
	}
}
//-------------------------------------------------------------------------------------------------
void CFAMTagManager::validateLicense()
{
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI14394", "FAM Tag Manager" );
}
//-------------------------------------------------------------------------------------------------