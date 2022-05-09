// FAMTagManager.cpp : Implementation of CFAMTagManager

#include "stdafx.h"
#include "FAMTagManager.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>
#include <MRUList.h>
#include <IConfigurationSettingsPersistenceMgr.h>
#include <ADOUtils.h>

//--------------------------------------------------------------------------------------------------
// Tag names
//--------------------------------------------------------------------------------------------------
const string strSOURCE_DOC_NAME_TAG = "<SourceDocName>";
const string strFPS_FILE_DIR_TAG = "<FPSFileDir>";
const string strFPS_FILENAME_TAG = "<FPSFileName>";
const string strCOMMON_COMPONENTS_DIR_TAG = "<CommonComponentsDir>";
const string strDATABASE_SERVER_TAG = "<DatabaseServer>";
const string strDATABASE_NAME_TAG = "<DatabaseName>";
const string strDATABASE_ACTION_TAG = "<ActionName>";
const string strWORKFLOW_TAG = "<Workflow>";
const string strMRU_MUTEX_NAME = "ExtractContextMRURegistryMutex";
const string strMETADATA = "Metadata";
const string strMETADATA_FORMATTED = "$Metadata(file, field name)";
const string strATTRIBUTE = "Attribute";
const string strATTRIBUTE_FORMATTED = "$Attribute(file, set name, path)";
const string strPAGINATION_PARENT = "PaginationParent";
const string strPAGINATION_PARENT_FORMATTED = "$PaginationParent(file)";

//--------------------------------------------------------------------------------------------------
// Statics
//--------------------------------------------------------------------------------------------------
string CFAMTagManager::ms_strFPSDir;
string CFAMTagManager::ms_strFPSFileName;
CCriticalSection CFAMTagManager::ms_criticalsection;
IContextTagProviderPtr CFAMTagManager::ms_ipContextTagProvider;
map<stringCSIS, map<stringCSIS, stringCSIS>> CFAMTagManager::ms_mapWorkflowContextTags;

//--------------------------------------------------------------------------------------------------
// CFAMTagManager
//--------------------------------------------------------------------------------------------------
CFAMTagManager::CFAMTagManager()
	: m_ipMiscUtils(CLSID_MiscUtils)
	, m_bAlwaysShowDatabaseTags(false)
	, m_mutexMRU(FALSE, strMRU_MUTEX_NAME.c_str())
	, m_strWorkflow("")
, m_ipFAMDB(__nullptr)
{
	ASSERT_RESOURCE_ALLOCATION("ELI35226", m_ipMiscUtils != __nullptr);

	// ms_ipContextTagProvider is not created until the first instance of FAMTagManager is
	// created.
	CSingleLock lock(&ms_criticalsection, TRUE);
	if (ms_ipContextTagProvider == __nullptr)
	{
		ms_ipContextTagProvider.CreateInstance("Extract.Utilities.ContextTags.ContextTagProvider");
		ASSERT_RESOURCE_ALLOCATION("ELI37903", ms_ipContextTagProvider != __nullptr);
	}

	m_upUserCfgMgr.reset(new RegistryPersistenceMgr(HKEY_CURRENT_USER,
		gstrCOM_COMPONENTS_REG_PATH + "\\UCLIDFileProcessing"));
		
	m_upContextMRUList.reset(new MRUList(m_upUserCfgMgr.get(), "\\ContextsMRUList", "File_%d", 12));
}
//--------------------------------------------------------------------------------------------------
CFAMTagManager::~CFAMTagManager()
{
	try
	{
		m_ipMiscUtils = __nullptr;
		m_ipFAMDB = __nullptr;
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
		&IID_IFAMTagManager,
		&IID_ITagUtility,
		&IID_ICopyableObject
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

		CSingleLock lock(&ms_criticalsection, TRUE);
		*strFPSDir = _bstr_t(ms_strFPSDir.c_str()).Detach();

		return S_OK;		
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14383");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::put_FPSFileDir(BSTR strFPSDir)
{
	try
	{
		// Check license
		validateLicense();

		CSingleLock lock(&ms_criticalsection, TRUE);
		ms_strFPSDir = asString(strFPSDir);

		ms_ipContextTagProvider->ContextPath = ms_strFPSDir.c_str();

		// Want to always refresh the tags from the database, even when 
		// the folder has not changed
		// https://extract.atlassian.net/browse/ISSUE-13068
		// https://extract.atlassian.net/browse/ISSUE-13078
		refreshContextTags();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14384");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::get_FPSFileName(BSTR *strFPSFileName)
{
	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI36124", strFPSFileName != __nullptr);

		CSingleLock lock(&ms_criticalsection, TRUE);
		*strFPSFileName = _bstr_t(ms_strFPSFileName.c_str()).Detach();

		return S_OK;		
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36125");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::put_FPSFileName(BSTR strFPSFileName)
{
	try
	{
		// Check license
		validateLicense();

		CSingleLock lock(&ms_criticalsection, TRUE);
		ms_strFPSFileName = asString(strFPSFileName);

		string strPath = getDirectoryFromFullPath(ms_strFPSFileName);
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36126");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::raw_ExpandTags(BSTR bstrInput, BSTR bstrSourceDocName, IUnknown *pData,
											BSTR *pbstrOutput)
{
	try
	{
		ASSERT_ARGUMENT("ELI20470", pbstrOutput != __nullptr);

		// Check license
		validateLicense();

		// The code is used to expand tags, currently it supports <SourceDocName> 
		// and <FPSFile>
		string strInput = asString(bstrInput);
		string strSourceDocName = asString(bstrSourceDocName);

		expandTags(strInput, strSourceDocName);

		*pbstrOutput = _bstr_t(strInput.c_str()).Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14389");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::ExpandTags(BSTR bstrInput, BSTR bstrSourceName, BSTR *pbstrOutput)
{
	try
	{
		ASSERT_ARGUMENT("ELI34849", pbstrOutput != __nullptr);

		// Check license
		validateLicense();

		// The code is used to expand tags, currently it support <SourceDocName> 
		// and <FPSFile>
		string strInput = asString(bstrInput);
		string strSourceDocName = asString(bstrSourceName);

		expandTags(strInput, strSourceDocName);

		*pbstrOutput = _bstr_t(strInput.c_str()).Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38201");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::raw_ExpandTagsAndFunctions(BSTR bstrInput, BSTR bstrSourceDocName,
														IUnknown *pData, BSTR *pbstrOutput)
{
	try
	{
		ASSERT_ARGUMENT("ELI35227", pbstrOutput != __nullptr);

		// Check license
		validateLicense();

		ITagUtilityPtr ipThis(this);
		ASSERT_RESOURCE_ALLOCATION("ELI35228", ipThis != __nullptr);

		_bstr_t bstrOutput = m_ipMiscUtils->ExpandTagsAndFunctions(
			bstrInput, ipThis, bstrSourceDocName, pData);

		*pbstrOutput = bstrOutput.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI35229");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::ExpandTagsAndFunctions(BSTR bstrInput, BSTR bstrSourceName, BSTR *pbstrOutput)
{
	try
	{
		ASSERT_ARGUMENT("ELI35230", pbstrOutput != __nullptr);

		// Check license
		validateLicense();

		ITagUtilityPtr ipThis(this);
		ASSERT_RESOURCE_ALLOCATION("ELI35231", ipThis != __nullptr);

		_bstr_t bstrOutput = m_ipMiscUtils->ExpandTagsAndFunctions(
			bstrInput, ipThis, bstrSourceName, __nullptr);

		*pbstrOutput = bstrOutput.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI35232");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::raw_GetBuiltInTags(IVariantVector* *ppTags)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		IVariantVectorPtr ipVec(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI14396", ipVec != __nullptr);

		// Push the current tags into vector. Currently:
		// <SourceDocName>, <FPSFileDir> <FPSFileName> and <CommonComponentsDir>
		ipVec->PushBack(get_bstr_t(strSOURCE_DOC_NAME_TAG));
		ipVec->PushBack(get_bstr_t(strFPS_FILE_DIR_TAG));
		ipVec->PushBack(get_bstr_t(strFPS_FILENAME_TAG));
		ipVec->PushBack(get_bstr_t(strCOMMON_COMPONENTS_DIR_TAG));
		ipVec->PushBack(get_bstr_t(strWORKFLOW_TAG));

		if (m_bAlwaysShowDatabaseTags)
		{
			IVariantVectorPtr ipContextTagNames = ms_ipContextTagProvider->GetTagNames();
			ASSERT_RESOURCE_ALLOCATION("ELI38075", ipContextTagNames != __nullptr);

			if (!ipContextTagNames->Contains(strDATABASE_SERVER_TAG.c_str()))
			{
				ipVec->PushBack(strDATABASE_SERVER_TAG.c_str());
			}
			
			if (!ipContextTagNames->Contains(strDATABASE_NAME_TAG.c_str()))
			{
				ipVec->PushBack(strDATABASE_NAME_TAG.c_str());
			}

			if (!ipContextTagNames->Contains(strDATABASE_ACTION_TAG.c_str()))
			{
				ipVec->PushBack(strDATABASE_ACTION_TAG.c_str());
			}
		}

		// Report any programmatically added tags.
		CSingleLock lock(&m_criticalSectionAddedTags, TRUE);
		for (map<string, string>::iterator iter = m_mapAddedTags.begin();
			 iter != m_mapAddedTags.end();
			 iter++)
		{
			ipVec->PushBack(get_bstr_t(iter->first));
		}

		*ppTags = ipVec.Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14390");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::raw_GetCustomFileTags(IVariantVector* *ppTags)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		IVariantVectorPtr ipVec(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI19432", ipVec != __nullptr);

		CSingleLock lock(&ms_criticalsection, TRUE);
		IVariantVectorPtr ipTagNames = ms_ipContextTagProvider->GetTagNames();
		ASSERT_RESOURCE_ALLOCATION("ELI37904", ipTagNames != __nullptr);

		*ppTags = ipTagNames.Detach();	
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14391");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::raw_GetAllTags(IVariantVector* *ppTags)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		ITagUtilityPtr ipThis(this);
		ASSERT_RESOURCE_ALLOCATION("ELI14397", ipThis != __nullptr);

		// Call GetBuiltInTags() and GetCustomFileTags()
		IVariantVectorPtr ipVec1 = ipThis->GetBuiltInTags();
		IVariantVectorPtr ipVec2 = ipThis->GetCustomFileTags();
		// Build a vector of all tags
		ipVec1->Append(ipVec2);

		*ppTags = ipVec1.Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14392");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::raw_GetFunctionNames(IVariantVector** ppFunctionNames)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI35233", ppFunctionNames != __nullptr);

		ITagUtilityPtr ipTagUtility(m_ipMiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI35234", ipTagUtility != __nullptr);

		IVariantVectorPtr ipFunctions = ipTagUtility->GetFunctionNames();
		ASSERT_RESOURCE_ALLOCATION("ELI35235", ipFunctions != __nullptr);

		ipFunctions->PushBack(strMETADATA.c_str());
		ipFunctions->PushBack(strATTRIBUTE.c_str());
		ipFunctions->PushBack(strPAGINATION_PARENT.c_str());

		*ppFunctionNames = ipFunctions.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI35236");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::raw_GetFormattedFunctionNames(IVariantVector** ppFunctionNames)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI35237", ppFunctionNames != __nullptr);

		ITagUtilityPtr ipTagUtility(m_ipMiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI35238", ipTagUtility != __nullptr);

		IVariantVectorPtr ipFunctions = ipTagUtility->GetFormattedFunctionNames();
		ASSERT_RESOURCE_ALLOCATION("ELI35239", ipFunctions != __nullptr);

		ipFunctions->PushBack(strMETADATA_FORMATTED.c_str());
		ipFunctions->PushBack(strATTRIBUTE_FORMATTED.c_str());
		ipFunctions->PushBack(strPAGINATION_PARENT_FORMATTED.c_str());

		*ppFunctionNames = ipFunctions.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI35240");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::raw_EditCustomTags(long hParentWindow)
{
	try
	{
		// Check license
		validateLicense();

		if (ms_strFPSDir.empty())
		{
			throw UCLIDException("ELI38048",
				"FPSFileDir must be defined to allow editing of context-specific tags.");
		}

		ms_ipContextTagProvider->EditTags(hParentWindow);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38049");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::raw_AddTag(BSTR bstrTagName, BSTR bstrTagValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI38088", bstrTagName != __nullptr);

		validateLicense();

		string strTag = asString(bstrTagName);
		if (strTag.substr(0, 1) != "<")
		{
			strTag = "<" + strTag;
		}
		if (strTag.substr(strTag.length() - 1, 1) != ">")
		{
			strTag += ">";
		}

		CSingleLock lock(&m_criticalSectionAddedTags, TRUE);
		m_mapAddedTags[strTag] = asString(bstrTagValue);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38089");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::raw_GetAddedTags(IIUnknownVector **ppStringPairTags)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI43284", ppStringPairTags != __nullptr);

		IIUnknownVectorPtr ipStringPairTags(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI43285", ipStringPairTags != __nullptr);

		CSingleLock lock(&m_criticalSectionAddedTags, TRUE);

		for (auto iter = m_mapAddedTags.begin(); iter != m_mapAddedTags.end(); iter++)
		{
			IStringPairPtr ipStringPair(CLSID_StringPair);
			ipStringPair->StringKey = iter->first.c_str();
			ipStringPair->StringValue = iter->second.c_str();
			ipStringPairTags->PushBack(ipStringPair);
		}
		*ppStringPairTags = ipStringPairTags.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI43286");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::raw_ExpandFunction(BSTR bstrFunctionName, IVariantVector *pArgs,
	BSTR bstrSourceDocName, IUnknown *pData, BSTR *pbstrOutput)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI43492", pArgs != __nullptr);
		ASSERT_ARGUMENT("ELI43493", pbstrOutput != __nullptr);

		validateLicense();

		IVariantVectorPtr ipArgs(pArgs);
		ASSERT_RESOURCE_ALLOCATION("ELI43494", ipArgs != __nullptr);

		UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipFAMDB = getFAMDB();


		string strFunctionName = asString(bstrFunctionName);
		if (_strcmpi(strFunctionName.c_str(), strMETADATA.c_str()) == 0)
		{
			ASSERT_RUNTIME_CONDITION("ELI43495", ipArgs->Size == 2,
				"The $" + strMETADATA + " path tag function requires two arguments");

			long nFileID = ipFAMDB->GetFileID(ipArgs->Item[0].bstrVal);
			_bstr_t bstrOutput = ipFAMDB->GetMetadataFieldValue(nFileID, ipArgs->Item[1].bstrVal);
			*pbstrOutput = bstrOutput.Detach();
		}
		else if(_strcmpi(strFunctionName.c_str(), strATTRIBUTE.c_str()) == 0)
		{
			ASSERT_RUNTIME_CONDITION("ELI50006", ipArgs->Size == 3,
				"The $" + strATTRIBUTE + " path tag function requires three arguments.");

			_bstr_t bstrOutput = ipFAMDB->GetAttributeValue(
				ipArgs->Item[0].bstrVal, ipArgs->Item[1].bstrVal, ipArgs->Item[2].bstrVal);
			*pbstrOutput = bstrOutput.Detach();
		}
		else if (_strcmpi(strFunctionName.c_str(), strPAGINATION_PARENT.c_str()) == 0)
		{
			ASSERT_RUNTIME_CONDITION("ELI53419", ipArgs->Size == 1,
				"The $" + strPAGINATION_PARENT + " path tag function requires one argument.");
			long nFileID = ipFAMDB->GetFileID(ipArgs->Item[0].bstrVal);
			string strQuery = "  SELECT TOP(1) [FAMFile].[FileName] FROM Pagination "
				" INNER JOIN FAMFile ON FAMFile.id = Pagination.OriginalFileID AND DestFileID = "
				+ asString(nFileID) + " AND Pagination.DestPage = 1";	

			auto ipRecords = ipFAMDB->GetResultsForQuery( strQuery.c_str());
			if (!asCppBool(ipRecords->adoEOF))
			{
				*pbstrOutput = get_bstr_t(getStringField(ipRecords->Fields, "FileName")).Detach();
			}
			else
			{
				*pbstrOutput = _bstr_t(ipArgs->Item[0].bstrVal).Detach();
			}
		}
		else
		{
			*pbstrOutput = _bstr_t().Detach();
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI43497");
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
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14395");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::get_AlwaysShowDatabaseTags(VARIANT_BOOL *pbValue)
{
	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI38076", pbValue != __nullptr);

		*pbValue = asVariantBool(m_bAlwaysShowDatabaseTags);

		return S_OK;		
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38077");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::put_AlwaysShowDatabaseTags(VARIANT_BOOL bValue)
{
	try
	{
		// Check license
		validateLicense();

		m_bAlwaysShowDatabaseTags = asCppBool(bValue);

		string strPath = getDirectoryFromFullPath(ms_strFPSFileName);
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38078");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::ValidateConfiguration(BSTR bstrDatabaseServer, BSTR bstrDatabaseName,
												   BSTR* pbstrWarning)
{
	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI38105", bstrDatabaseServer != __nullptr);
		ASSERT_ARGUMENT("ELI38106", bstrDatabaseName != __nullptr);
		ASSERT_ARGUMENT("ELI38107", pbstrWarning != __nullptr);

		IVariantVectorPtr ipUndefinedTags = ms_ipContextTagProvider->GetUndefinedTags(m_strWorkflow.c_str());
		ASSERT_RESOURCE_ALLOCATION("ELI38097", ipUndefinedTags != __nullptr);

		long nUndefinedTagCount = ipUndefinedTags->Size;
		if (nUndefinedTagCount > 0)
		{
			UCLIDException ue("ELI38098", 
				"There are custom tag(s) that have not been defined in the current context.");
			ue.addDebugInfo("Context", asString(ms_ipContextTagProvider->ActiveContext));
			for (int i = 0; i < nUndefinedTagCount; i++)
			{
				ue.addDebugInfo("Undefined tag", asString(ipUndefinedTags->Item[i].bstrVal));
			}
			throw ue;
		}

		IVariantVectorPtr ipContextTagNames = ms_ipContextTagProvider->GetTagNames();
		ASSERT_RESOURCE_ALLOCATION("ELI38108", ipContextTagNames != __nullptr);

		string strDBServer = asString(bstrDatabaseServer);
		string strDBName = asString(bstrDatabaseName);
		bool bConflictingTags = false;

		long nCountTagNames = ipContextTagNames->Size;
		for (int i = 0; i < nCountTagNames; i++)
		{
			if (asString(ipContextTagNames->Item[i].bstrVal) == strDATABASE_SERVER_TAG &&
				strDBServer != strDATABASE_SERVER_TAG)
			{
				bConflictingTags = true;
				break;
			}

			if (asString(ipContextTagNames->Item[i].bstrVal) == strDATABASE_NAME_TAG &&
				strDBName != strDATABASE_NAME_TAG)
			{
				bConflictingTags = true;
				break;
			}
		}

		if (bConflictingTags)
		{
			string strWarning = "Since <DatabaseServer> and/or <DatabaseName> are defined they "
				"should be used  to specify the server on the database tab. Otherwise, the "
				"specified database of " + strDBServer + "/" + strDBName + " may conflict and "
				"override the defined values of these tags depending on the context at the time of "
				"execution.";

			*pbstrWarning = _bstr_t(strWarning.c_str()).Detach();
		}
		else
		{
			*pbstrWarning = __nullptr;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38096");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::get_ActiveContext(BSTR *strActiveContext)
{
	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI38070", strActiveContext != __nullptr);

		*strActiveContext = _bstr_t(ms_ipContextTagProvider->ActiveContext).Detach();

		return S_OK;		
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38071");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::get_DatabaseServer(BSTR *strDatabaseServer)
{
	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI38303", strDatabaseServer != __nullptr);

		CSingleLock lock(&ms_criticalsection, TRUE);
		*strDatabaseServer = _bstr_t(m_strDatabaseServer.c_str()).Detach();

		return S_OK;		
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38305");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::put_DatabaseServer(BSTR strDatabaseServer)
{
	try
	{
		// Check license
		validateLicense();

		CSingleLock lock(&ms_criticalsection, TRUE);
		m_strDatabaseServer = asString(strDatabaseServer);

		m_ipFAMDB = __nullptr;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38309");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::get_DatabaseName(BSTR *strDatabaseName)
{
	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI38304", strDatabaseName != __nullptr);

		CSingleLock lock(&ms_criticalsection, TRUE);
		*strDatabaseName = _bstr_t(m_strDatabaseName.c_str()).Detach();
		
		return S_OK;		
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38310");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::put_DatabaseName(BSTR strDatabaseName)
{
	try
	{
		// Check license
		validateLicense();

		CSingleLock lock(&ms_criticalsection, TRUE);
		m_strDatabaseName = asString(strDatabaseName);

		m_ipFAMDB = __nullptr;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38311");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::get_ActionName(BSTR *strActionName)
{
	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI38312", strActionName != __nullptr);

		CSingleLock lock(&ms_criticalsection, TRUE);
		*strActionName = _bstr_t(m_strDatabaseAction.c_str()).Detach();
		
		return S_OK;		
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38313");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::put_ActionName(BSTR strActionName)
{
	try
	{
		// Check license
		validateLicense();

		CSingleLock lock(&ms_criticalsection, TRUE);
		m_strDatabaseAction = asString(strActionName);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38314");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::RefreshContextTags()
{
	try
	{
		// Check license
		validateLicense();

		refreshContextTags();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI39273");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::get_Workflow(BSTR *strWorkflow)
{
	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI43223", strWorkflow != __nullptr);

		CSingleLock lock(&ms_criticalsection, TRUE);
		*strWorkflow = _bstr_t(m_strWorkflow.c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI43224");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::put_Workflow(BSTR strWorkflow)
{
	try
	{
		// Check license
		validateLicense();
		string strNewWorkflow = asString(strWorkflow);
		{
			CSingleLock lock(&ms_criticalsection, TRUE);
			m_strWorkflow = strNewWorkflow;
		}
		
		CSingleLock lock2(&m_criticalSectionAddedTags, TRUE);
		// add the workflow to the map
		m_mapAddedTags[strWORKFLOW_TAG] = strNewWorkflow;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI43225");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::GetFAMTagManagerWithWorkflow(BSTR bstrWorkflow, IFAMTagManager **ppFAMTagManager)
{
	try
	{
		ICopyableObjectPtr ipThis(this);
		ASSERT_RESOURCE_ALLOCATION("ELI43295", ipThis != __nullptr);

		UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr ipNew = ipThis->Clone();
		ipNew->FAMDB = m_ipFAMDB;
		ipNew->Workflow = _bstr_t(bstrWorkflow);
		
		*ppFAMTagManager = (IFAMTagManager *) ipNew.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI43294");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::get_FAMDB(IFileProcessingDB** ppFAMDB)
{
	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI43485", ppFAMDB != __nullptr);

		CSingleLock lock(&ms_criticalsection, TRUE);
		*ppFAMDB = (IFileProcessingDB*)m_ipFAMDB.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI43486");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::put_FAMDB(IFileProcessingDB* pFAMDB)
{
	try
	{
		// Check license
		validateLicense();

		CSingleLock lock(&ms_criticalsection, TRUE);
		m_ipFAMDB = pFAMDB;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI43487");
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
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// validate license first
		validateLicense();

		UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI43288", ipSource != __nullptr);
		
		string strNewWorkflow = asString(ipSource->Workflow);

		{
			CSingleLock lock(&ms_criticalsection, TRUE);
			m_bAlwaysShowDatabaseTags = asCppBool(ipSource->AlwaysShowDatabaseTags);
			m_strDatabaseServer = asString(ipSource->DatabaseServer);
			m_strDatabaseName = asString(ipSource->DatabaseName);
			m_strDatabaseAction = asString(ipSource->ActionName);
			m_strWorkflow = strNewWorkflow;
		}

		ITagUtilityPtr ipTagUtility = ipSource;
		ASSERT_RESOURCE_ALLOCATION("ELI43289", ipTagUtility != __nullptr);

		IIUnknownVectorPtr ipAddedTags = ipTagUtility->GetAddedTags();

		CSingleLock lock2(&m_criticalSectionAddedTags, TRUE);
		long nSize = ipAddedTags->Size();
		m_mapAddedTags.clear();
		for (long i = nSize; i < nSize; i++)
		{
			IStringPairPtr ipStringPair = ipAddedTags->At(i);
			m_mapAddedTags[asString(ipStringPair->StringKey)] = asString(ipStringPair->StringValue);
		}
		// This will probably already be in the tags because of the previous loop but just in case
		m_mapAddedTags[strWORKFLOW_TAG] = strNewWorkflow;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI43290");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMTagManager::raw_Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI43291", pObject != __nullptr);

		// Validate license first
		validateLicense();

		// Create another instance of this object
		ICopyableObjectPtr ipObjCopy(CLSID_FAMTagManager);
		ASSERT_RESOURCE_ALLOCATION("ELI43292", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk(this);
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI43293");

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
void CFAMTagManager::expandTags(string &rstrInput, const string &strSourceDocName)
{
	// In the case that the database name or server tags are present, attempt to resolve them
	// before evaluating path tags so that the currently connected database overrides any definition
	// of database server or name in CustomTags.sdf.
	bool bDatabaseServerTagFound = rstrInput.find(strDATABASE_SERVER_TAG) != string::npos;
	bool bDatabaseNameTagFound = rstrInput.find(strDATABASE_NAME_TAG) != string::npos;
	if (bDatabaseServerTagFound && !m_strDatabaseServer.empty())
	{
		replaceVariable(rstrInput, strDATABASE_SERVER_TAG, m_strDatabaseServer);
	}

	if (bDatabaseNameTagFound && !m_strDatabaseName.empty())
	{
		replaceVariable(rstrInput, strDATABASE_NAME_TAG, m_strDatabaseName);
	}

	// As long as ms_strFPSDir is specified, apply any defined environment-specific path tags.
	// Expand these before the built-in tags so that built-in tags can be used within custom
	// environment tags.
	CSingleLock lock(&ms_criticalsection, TRUE);
	if (!ms_mapWorkflowContextTags.empty())
	{
		// Workflow being asked for may not have entries in the context tag provider so change to default values(workflow="")
		stringCSIS workflowToUse(m_strWorkflow, false);
		if (ms_mapWorkflowContextTags.find(workflowToUse) == ms_mapWorkflowContextTags.end())
		{
			workflowToUse.assign("");
		}
		
		long nTagCount = ms_mapWorkflowContextTags[workflowToUse].size();
		
		// In order to allow custom tags to contain other custom tags (e.g., <OutputImage> could be
		// defined as <OutputPath>\$FileOf(<SourceDocName>)), continue iterate tag expansion as long
		// as the expanded path keeps changing, up to the number of custom tags defined.
		long nIterations = min(nTagCount, 10);
		for (long i = 0; i < nIterations; i++)
		{
			string strOriginal = rstrInput;

			for each (pair<stringCSIS, stringCSIS> contextTag in ms_mapWorkflowContextTags[workflowToUse])
			{
				replaceVariable(rstrInput, contextTag.first, contextTag.second);
			}

			// If no tags were replaced, we can break out of the loop now.
			if (rstrInput == strOriginal)
			{
				break;
			}
		}
	}

	bool bSourceDocNameTagFound = rstrInput.find(strSOURCE_DOC_NAME_TAG) != string::npos;
	bool bFPSFileDirTagFound = rstrInput.find(strFPS_FILE_DIR_TAG) != string::npos;
	bool bFPSFileNameTagFound = rstrInput.find(strFPS_FILENAME_TAG) != string::npos;
	bool bCommonComponentsDirFound = rstrInput.find(strCOMMON_COMPONENTS_DIR_TAG) != string::npos;
	// In case database tags were used in a custom tag, try again to evaluate the tags here.
	bDatabaseServerTagFound = rstrInput.find(strDATABASE_SERVER_TAG) != string::npos;
	bDatabaseNameTagFound = rstrInput.find(strDATABASE_NAME_TAG) != string::npos;
	bool bDatabaseActionTagFound = rstrInput.find(strDATABASE_ACTION_TAG) != string::npos;

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
			ue.addDebugInfo("strInput", rstrInput);
			throw ue;
		}
		replaceVariable(rstrInput, strSOURCE_DOC_NAME_TAG, strSourceDocName);
	}

	if (bFPSFileDirTagFound)
	{
		if (ms_strFPSDir == "")
		{
			string strMsg = "There is no FPS file directory available to expand the ";
			strMsg += strFPS_FILE_DIR_TAG;
			strMsg += " tag!";
			UCLIDException ue("ELI14388", strMsg);
			ue.addDebugInfo("strInput", rstrInput);
			throw ue;
		}
		replaceVariable(rstrInput, strFPS_FILE_DIR_TAG, ms_strFPSDir);
	}

	if (bFPSFileNameTagFound)
	{
		if (ms_strFPSFileName == "")
		{
			string strMsg = "There is no FPS filename available to expand the ";
			strMsg += strFPS_FILENAME_TAG;
			strMsg += " tag!";
			UCLIDException ue("ELI36127", strMsg);
			ue.addDebugInfo("strInput", rstrInput);
			throw ue;
		}
		replaceVariable(rstrInput, strFPS_FILENAME_TAG, ms_strFPSFileName);
	}

	if (bCommonComponentsDirFound)
	{
		const string strCommonComponentsDir = getModuleDirectory("BaseUtils.dll");

		// Replace the common components dir tag
		replaceVariable(rstrInput, strCOMMON_COMPONENTS_DIR_TAG, strCommonComponentsDir);
	}

	if (bDatabaseServerTagFound && !m_strDatabaseServer.empty())
	{
		replaceVariable(rstrInput, strDATABASE_SERVER_TAG, m_strDatabaseServer);
	}

	if (bDatabaseNameTagFound && !m_strDatabaseName.empty())
	{
		replaceVariable(rstrInput, strDATABASE_NAME_TAG, m_strDatabaseName);
	}

	if (bDatabaseActionTagFound && !m_strDatabaseAction.empty())
	{
		replaceVariable(rstrInput, strDATABASE_ACTION_TAG, m_strDatabaseAction);
	}

	// Expand any programmatically added tags.
	CSingleLock lock2(&m_criticalSectionAddedTags, TRUE);
	for (map<string, string>::iterator iter = m_mapAddedTags.begin();
			iter != m_mapAddedTags.end();
			iter++)
	{
		string strTag = iter->first;
		string strValue = iter->second;

		replaceVariable(rstrInput, strTag, strValue);
	}
}
//-------------------------------------------------------------------------------------------------
void CFAMTagManager::refreshContextTags()
{
	ms_ipContextTagProvider->RefreshTags();

	// https://extract.atlassian.net/browse/ISSUE-13528
	// Update the context MRU based upon whether there is a valid context at FPSFileDir
	string strContextPath = asString(ms_ipContextTagProvider->ContextPath);
	if (!strContextPath.empty())
	{
		// Lock the mutex while the registry is being updated this is a named mutex so all processes
		// on the computer will stop for this
		CSingleLock lock(&m_mutexMRU, TRUE);

		m_upContextMRUList->readFromPersistentStore();

		if (ms_ipContextTagProvider->ActiveContext.length() == 0)
		{
			m_upContextMRUList->removeItem(strContextPath);
		}
		else
		{
			m_upContextMRUList->addItem(strContextPath);
		}

		m_upContextMRUList->writeToPersistentStore();
	}

	CSingleLock lock(&ms_criticalsection, TRUE);

	// https://extract.atlassian.net/browse/ISSUE-13001
	// Cache the context the tag values to avoid frequent COM calls which also tend to leak
	// memory since the returned VariantVector type is not currently supported by the
	// ReportMemoryUsage framework.
	ms_mapWorkflowContextTags.clear();

	// Get the workflows that have values
	IVariantVectorPtr ipWorkflows =  ms_ipContextTagProvider->GetWorkflowsThatHaveValues();

	long nNumberOfWorkflows = ipWorkflows->Size;
	for (long n = 0; n < nNumberOfWorkflows; n++)
	{
		variant_t vtWorkflow = ipWorkflows->Item[n];

		stringCSIS strWorkflow(asString(vtWorkflow.bstrVal), false);
		
		IStrToStrMapPtr ipContextNameValuePairs = ms_ipContextTagProvider->GetTagValuePairsForWorkflow(vtWorkflow.bstrVal);

		IIUnknownVectorPtr ipContextPairs = ipContextNameValuePairs->GetAllKeyValuePairs();
		long nTagCount = ipContextPairs->Size();
		for (long i = 0; i < nTagCount; i++)
		{
			IStringPairPtr ipPair = ipContextPairs->At(i);
			
			stringCSIS strName(asString(ipPair->StringKey), false);
			stringCSIS strValue(asString(ipPair->StringValue), false);

			ms_mapWorkflowContextTags[strWorkflow][strName] = strValue;
		}
	}
}
//-------------------------------------------------------------------------------------------------
UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr CFAMTagManager::getFAMDB()
{
	if (m_ipFAMDB == __nullptr)
	{
		m_ipFAMDB.CreateInstance(CLSID_FileProcessingDB);
		ASSERT_RESOURCE_ALLOCATION("ELI43496", m_ipFAMDB != __nullptr);

		string strServer = m_strDatabaseServer;
		expandTags(strServer, "");
		m_ipFAMDB->DatabaseServer = strServer.c_str();

		string strDatabase = m_strDatabaseName;
		expandTags(strDatabase, "");
		m_ipFAMDB->DatabaseName = strDatabase.c_str();
	}

	return m_ipFAMDB;
}
//-------------------------------------------------------------------------------------------------
void CFAMTagManager::validateLicense()
{
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI14394", "FAM Tag Manager" );
}
//-------------------------------------------------------------------------------------------------