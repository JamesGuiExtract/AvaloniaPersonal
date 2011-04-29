
// RuleExecutionEnv.cpp : Implementation of CRuleExecutionEnv

#include "stdafx.h"
#include "AFCore.h"
#include "RuleExecutionEnv.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <ComUtils.h>

// allocate static variables
map<DWORD, stack<string> > CRuleExecutionEnv::m_mapThreadIDToRSDFileStack;
map<DWORD, string> CRuleExecutionEnv::m_mapThreadIDToFKBVersion;
CMutex CRuleExecutionEnv::m_mutex;

//-------------------------------------------------------------------------------------------------
// CRuleExecutionEnv
//-------------------------------------------------------------------------------------------------
CRuleExecutionEnv::CRuleExecutionEnv()
{
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

		// return the stack size
		*pnStackSize = rThisThreadRSDFileStack.size();
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
		// get the current thread's RSD file stack
		stack<string>& rThisThreadRSDFileStack = getCurrentStack(true);

		// the entry was found - return the corresponding string
		*pstrFileName = get_bstr_t(rThisThreadRSDFileStack.top()).Detach();
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
		// get the current thread's RSD file stack
		stack<string>& rThisThreadRSDFileStack = getCurrentStack(true);

		// the entry was found - return the directory associated
		// with the corresponding RSD file
		string strRSDFile = rThisThreadRSDFileStack.top();
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

		// If this it the top-level rule, apply the FKB version (no questions asked)
		if (getCurrentStack().size() == 1)
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
stack<string>& CRuleExecutionEnv::getCurrentStack(bool bThrowExceptionIfStackEmpty)
{
	// protect the stack against simultaneous access
	CSingleLock lg( &m_mutex, TRUE );

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
	CSingleLock lg( &m_mutex, TRUE );

	// Get the FKB version string for the current thread.
	DWORD dwThreadID = GetCurrentThreadId();
	string& rFKBVersion = m_mapThreadIDToFKBVersion[dwThreadID];

	return rFKBVersion;
}
//-------------------------------------------------------------------------------------------------
