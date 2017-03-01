
// RuleExecutionEnv.cpp : Implementation of CRuleExecutionEnv

#include "stdafx.h"
#include "AFCore.h"
#include "RuleExecutionEnv.h"
#include "Common.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <ComUtils.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

// allocate static variables
map<DWORD, stack<string> > CRuleExecutionEnv::m_mapThreadIDToRSDFileStack;
map<DWORD, string> CRuleExecutionEnv::m_mapThreadIDToFKBVersion;
map<DWORD, string> CRuleExecutionEnv::m_mapThreadIDToAlternateComponentDataDir;
CCriticalSection CRuleExecutionEnv::m_criticalSection;
map<DWORD, string> CRuleExecutionEnv::m_mapThreadIDToRSDFileBeingEdited;
bool CRuleExecutionEnv::m_bShouldAddAttributeHistory;
bool CRuleExecutionEnv::m_bEnableParallelProcessing;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const string gstrSETTINGS_SECTION = "\\AttributeFinder\\Settings";

//-------------------------------------------------------------------------------------------------
// CRuleExecutionEnv
//-------------------------------------------------------------------------------------------------
CRuleExecutionEnv::CRuleExecutionEnv()
{
	// Setup Registry persistence item
	ma_pSettingsCfgMgr = unique_ptr<IConfigurationSettingsPersistenceMgr>(
		new RegistryPersistenceMgr( HKEY_CURRENT_USER, gstrREG_ROOT_KEY ) );

	updateSettingsFromRegistry();
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleExecutionEnv::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IRuleExecutionEnv
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleExecutionEnv::PushRSDFileName(BSTR strFileName, long *pnStackSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate arguments
		ASSERT_ARGUMENT("ELI11543", pnStackSize != __nullptr);

		// store the specified file as associated with
		// the currently executing thread
		string strFile = asString(strFileName);

		// if strFile is defined, make sure it is an absolute path
		if (!strFile.empty())
		{
			// create a dummy file in the current directory
			string strCurrentDirFile = getCurrentDirectory() + "\\dummy.dat";
			// if strFile has no path before call after call it will have the current Directory as path
			strFile = getAbsoluteFileName(strCurrentDirFile, strFile);
		}

		// get the stack associated with the current thread
		stack<string>& rThisThreadRSDFileStack = getCurrentStack();

		// push the RSD file on the stack associated with the current thread
		rThisThreadRSDFileStack.push(strFile);

		long nStackSize = rThisThreadRSDFileStack.size();

		// return the stack size
		*pnStackSize = nStackSize;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07455")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleExecutionEnv::PopRSDFileName(long *pnStackSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate arguments
		ASSERT_ARGUMENT("ELI11544", pnStackSize != __nullptr);

		// get the stack associated with the current thread
		stack<string>& rThisThreadRSDFileStack = getCurrentStack(true);

		// pop the stack
		rThisThreadRSDFileStack.pop();

		// return the stack size
		*pnStackSize = rThisThreadRSDFileStack.size();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07485")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleExecutionEnv::GetCurrentRSDFileName(BSTR *pstrFileName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		*pstrFileName = get_bstr_t(getCurrentRSDFileName()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07456")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleExecutionEnv::GetCurrentRSDFileDir(BSTR *pstrRSDFileDir)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		string strRSDFile = getCurrentRSDFileName();

		// the entry was found - return the directory associated
		// with the corresponding RSD file
		string strRSDFileDir = getDirectoryFromFullPath(strRSDFile);
		*pstrRSDFileDir = get_bstr_t(strRSDFileDir).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07457")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleExecutionEnv::IsRSDFileExecuting(BSTR bstrFileName,
												   VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// get a ***COPY*** of the current thread's RSD file stack
		stack<string> rThisThreadRSDFileStack = getCurrentStack();

		// Get the file name as a const char*
		string strFileName = asString(bstrFileName);
		const char* pszFileName = strFileName.c_str();

		// keep popping the stack to see if any of the entries
		// match the input file
		while (!rThisThreadRSDFileStack.empty())
		{
			// check the topmost entry in the stack
			const char* pszFile = rThisThreadRSDFileStack.top().c_str();
			if (_strcmpi(pszFile, pszFileName) == 0)
			{
				// we found a match
				*pbValue = VARIANT_TRUE;
				return S_OK;
			}

			// pop the stack
			rThisThreadRSDFileStack.pop();
		}

		// if we reach here, we didn't find a match
		*pbValue = VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07488")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleExecutionEnv::get_FKBVersion(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI32476", pVal != __nullptr);

		*pVal = _bstr_t(getFKBVersionString().c_str()).Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32477")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleExecutionEnv::put_FKBVersion(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		string newFKBVersion = asString(newVal);
		string& rstrFKBVerson = getFKBVersionString();

		// If this it the top-level rule (or if there are no rulesets executing),
		// apply the FKB version (no questions asked)
		if (getCurrentStack().size() < 2)
		{
			rstrFKBVerson = newFKBVersion;
		}
		// If this is a nested ruleset, ensure it is not a different FKB version than that which
		// has already been specified.
		else if (!rstrFKBVerson.empty() &&
				 _strcmpi(newFKBVersion.c_str(), rstrFKBVerson.c_str()) != 0)
		{
			UCLIDException ue("ELI32478", "Conflicting FKB version numbers!");
			ue.addDebugInfo("Original version", rstrFKBVerson);
			ue.addDebugInfo("Conflicting version", newFKBVersion);
			throw ue;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32479")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleExecutionEnv::get_AlternateComponentDataDir(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI35913", pVal != __nullptr);

		*pVal = _bstr_t(getAlternateComponentDataDir().c_str()).Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI35914")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleExecutionEnv::put_AlternateComponentDataDir(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		string& rstrAlternateComponentDataDir = getAlternateComponentDataDir();
		rstrAlternateComponentDataDir = asString(newVal);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI35916")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleExecutionEnv::get_ShouldAddAttributeHistory(VARIANT_BOOL *pbVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		*pbVal = asVariantBool(m_bShouldAddAttributeHistory);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41778")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleExecutionEnv::get_RSDFileBeingEdited(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI41781", pVal != __nullptr);

		*pVal = _bstr_t(getRSDFileBeingEdited().c_str()).Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41782")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleExecutionEnv::put_RSDFileBeingEdited(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		string& rstrRSDFileBeingEdited = getRSDFileBeingEdited();
		rstrRSDFileBeingEdited = asString(newVal);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41783")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleExecutionEnv::get_EnableParallelProcessing(VARIANT_BOOL *pbVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		*pbVal = asVariantBool(m_bEnableParallelProcessing);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI42013")
}
//-------------------------------------------------------------------------------------------------
stack<string>& CRuleExecutionEnv::getCurrentStack(bool bThrowExceptionIfStackEmpty)
{
	// protect the stack against simultaneous access
	CSingleLock lg( &m_criticalSection, TRUE );

	// get the stack associated with the current thread
	// or create a new stack if this is the first time we're in the current thread
	DWORD dwThreadID = GetCurrentThreadId();
	stack<string>& rThisThreadStack = m_mapThreadIDToRSDFileStack[dwThreadID];

	// if there is no stack for the current thread, throw an exception
	if (rThisThreadStack.empty() && bThrowExceptionIfStackEmpty)
	{
		UCLIDException ue("ELI11649", "Rule Execution Stack is empty.");
		ue.addDebugInfo("dwThreadID", dwThreadID);
		throw ue;
	}

	// return the stack
	return rThisThreadStack;
}
//-------------------------------------------------------------------------------------------------
string& CRuleExecutionEnv::getFKBVersionString()
{
	CSingleLock lg( &m_criticalSection, TRUE );

	// Get the FKB version string for the current thread.
	DWORD dwThreadID = GetCurrentThreadId();
	string& rFKBVersion = m_mapThreadIDToFKBVersion[dwThreadID];

	return rFKBVersion;
}
//-------------------------------------------------------------------------------------------------
string& CRuleExecutionEnv::getAlternateComponentDataDir()
{
	CSingleLock lg( &m_criticalSection, TRUE );

	// Get the alternate component data directory for the current thread.
	DWORD dwThreadID = GetCurrentThreadId();
	string& rComponentDataDir = m_mapThreadIDToAlternateComponentDataDir[dwThreadID];

	return rComponentDataDir;
}
//-------------------------------------------------------------------------------------------------
void CRuleExecutionEnv::updateSettingsFromRegistry()
{
	// Only respect the add attribute history setting if the RDT is licensed
	if (LicenseManagement::isLicensed(gnRULE_DEVELOPMENT_TOOLKIT_OBJECTS))
	{
		// Add attribute history
		if (!ma_pSettingsCfgMgr->keyExists(gstrSETTINGS_SECTION, gstrAF_ADD_ATTRIBUTE_HISTORY_KEY))
		{
			ma_pSettingsCfgMgr->createKey(gstrSETTINGS_SECTION,
				gstrAF_ADD_ATTRIBUTE_HISTORY_KEY, gstrAF_DEFAULT_ADD_ATTRIBUTE_HISTORY);

			m_bShouldAddAttributeHistory = asCppBool(gstrAF_DEFAULT_ADD_ATTRIBUTE_HISTORY);
		}
		else
		{
			string strValue = ma_pSettingsCfgMgr->getKeyValue(gstrSETTINGS_SECTION,
				gstrAF_ADD_ATTRIBUTE_HISTORY_KEY, gstrAF_DEFAULT_ADD_ATTRIBUTE_HISTORY);

			m_bShouldAddAttributeHistory = asCppBool(strValue);
		}
	}

	// Enable parallel processing
	if (!ma_pSettingsCfgMgr->keyExists(gstrSETTINGS_SECTION, gstrAF_ENABLE_PARALLEL_PROCESSING_KEY))
	{
		ma_pSettingsCfgMgr->createKey(gstrSETTINGS_SECTION,
			gstrAF_ENABLE_PARALLEL_PROCESSING_KEY, gstrAF_DEFAULT_ENABLE_PARALLEL_PROCESSING);

		m_bEnableParallelProcessing = asCppBool(gstrAF_DEFAULT_ENABLE_PARALLEL_PROCESSING);
	}
	else
	{
		string strValue = ma_pSettingsCfgMgr->getKeyValue(gstrSETTINGS_SECTION,
			gstrAF_ENABLE_PARALLEL_PROCESSING_KEY, gstrAF_DEFAULT_ENABLE_PARALLEL_PROCESSING);

		m_bEnableParallelProcessing = asCppBool(strValue);
	}
}
//-------------------------------------------------------------------------------------------------
string& CRuleExecutionEnv::getRSDFileBeingEdited()
{
	CSingleLock lg( &m_criticalSection, TRUE );

	// Get the RSD file being edited for the current thread.
	DWORD dwThreadID = GetCurrentThreadId();
	string& rRSDFileBeingEdited = m_mapThreadIDToRSDFileBeingEdited[dwThreadID];

	return rRSDFileBeingEdited;
}
//-------------------------------------------------------------------------------------------------
string CRuleExecutionEnv::getCurrentRSDFileName()
{
	string strRSDFile;

	// get the current thread's RSD file stack
	stack<string>& rThisThreadRSDFileStack = getCurrentStack();
	if (rThisThreadRSDFileStack.size() > 0)
	{
		strRSDFile = rThisThreadRSDFileStack.top();
	}
	else
	{
		strRSDFile = getRSDFileBeingEdited();
	}

	ASSERT_RUNTIME_CONDITION("ELI41780", !strRSDFile.empty(),
		"There is no current RSD file");

	return strRSDFile;
}
//-------------------------------------------------------------------------------------------------