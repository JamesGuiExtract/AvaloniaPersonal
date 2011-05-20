// COMUCLIDException.cpp : Implementation of CCOMUCLIDException
#include "stdafx.h"
#include "UCLIDExceptionMgmt.h"
#include "COMUCLIDException.h"

#include <comdef.h>
#include <UCLIDException.h>
#include <UCLIDExceptionDlg.h>
#include <LicenseMgmt.h>
#include <COMUtils.h>

//-------------------------------------------------------------------------------------------------
// CCOMUCLIDException
//-------------------------------------------------------------------------------------------------
CCOMUCLIDException::CCOMUCLIDException() : m_upException(new UCLIDException())
{
}
//-------------------------------------------------------------------------------------------------
CCOMUCLIDException::~CCOMUCLIDException()
{
	try
	{
		if (m_upException.get() != __nullptr)
		{
			m_upException.reset();
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20395");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCOMUCLIDException::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ICOMUCLIDException
	};

	for (int i = 0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
		{
			return S_OK;
		}
	}

	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ICOMUCLIDException
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCOMUCLIDException::CreateWithInnerException(BSTR strELICode, BSTR strText, 
														  ICOMUCLIDException *pInnerException)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{	
		// If there is an inner exception create a new exception object with an inner exception
		if (pInnerException != __nullptr)
		{
			// Convert the inner exception and make sure the memory gets released.
			unique_ptr<UCLIDException> upueInner(createUCLIDException(pInnerException));

			// Create a new exception object with the inner exception
			m_upException.reset(new UCLIDException(asString(strELICode), asString(strText), *upueInner));
		}
		else
		{
			// Create a new exception object without an inner exception.
			m_upException.reset(new UCLIDException(asString(strELICode), asString(strText)));
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI21237");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCOMUCLIDException::AddStackTraceEntry(BSTR strStackTrace)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{	
		// Add the stack trace entry to the encapsulated exception object.
		m_upException->addStackTraceEntry(asString(strStackTrace));

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI21238");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCOMUCLIDException::GetStackTraceEntry(long nIndex, BSTR *pstrStackTrace)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{	
		ASSERT_ARGUMENT("ELI25447", pstrStackTrace != __nullptr);

		// Get the stack trace from the UCLIDException object.
		const vector<string>& rvecStackTrace = m_upException->getStackTrace();

		// Check to make sure the index in within the bounds of the stack trace vector.
		if ( nIndex < 0 || nIndex >= (long)rvecStackTrace.size())
		{
			UCLIDException ue("ELI21267", "Index out of bounds!");
			ue.addDebugInfo("Index", nIndex);
			ue.addDebugInfo("Size", rvecStackTrace.size());
			throw ue;
		}
		
		// Return the stack trace entry at the indexed position.
		*pstrStackTrace = get_bstr_t(rvecStackTrace[nIndex]).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI21239");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCOMUCLIDException::GetStackTraceCount(long *pnIndex)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{	
		ASSERT_RESOURCE_ALLOCATION("ELI21263", pnIndex != __nullptr);

		// Set to the size of the StackTrace vector
		*pnIndex = m_upException->getStackTrace().size();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI21280");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCOMUCLIDException::GetDebugInfo(long nIndex, BSTR* pbstrKeyName, BSTR* pbstrStringizedValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{	
		ASSERT_ARGUMENT("ELI21286", pbstrKeyName != __nullptr);
		ASSERT_ARGUMENT("ELI21287", pbstrStringizedValue != __nullptr);

		// Get the debug info from the UCLIDException object.
		const vector<NamedValueTypePair>& rvecDebugInfo = m_upException->getDebugVector();

		// Check to make sure the index in within the bounds of the debug info vector.
		if ( nIndex < 0 || nIndex >= (long)rvecDebugInfo.size())
		{
			UCLIDException ue("ELI21284", "Index out of bounds!");
			ue.addDebugInfo("Index", nIndex);
			ue.addDebugInfo("Size", rvecDebugInfo.size());
			throw ue;
		}
		
		// Return the Debug Info entry at the indexed position.
		const NamedValueTypePair& pair = rvecDebugInfo[nIndex];
		*pbstrStringizedValue = _bstr_t(pair.GetPair().getValueAsString().c_str()).Detach();
		*pbstrKeyName = _bstr_t(pair.GetName().c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI21283");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCOMUCLIDException::GetDebugInfoCount(long *pnIndex)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{	
		ASSERT_RESOURCE_ALLOCATION("ELI21281", pnIndex != __nullptr);

		// Set to the size of the debug vector
		*pnIndex = m_upException->getDebugVector().size();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI21279");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCOMUCLIDException::GetInnerException(ICOMUCLIDException **ppInnerException)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{	
		ASSERT_ARGUMENT("ELI21339", ppInnerException != __nullptr);

		// Get the inner exception from the exception object
		const UCLIDException *exInner = m_upException->getInnerException();

		// If it is not null, convert to UCLID COM Exception.
		if (exInner != __nullptr)
		{
			UCLID_EXCEPTIONMGMTLib::ICOMUCLIDExceptionPtr ipCOMUCLIDException(CLSID_COMUCLIDException);
			ASSERT_RESOURCE_ALLOCATION("ELI21240", ipCOMUCLIDException != __nullptr);
			ipCOMUCLIDException->CreateFromString("ELI21241", exInner->asStringizedByteStream().c_str());
		
			*ppInnerException = (ICOMUCLIDException *)ipCOMUCLIDException.Detach();
		}
		else
		{
			// Return NULL if there is no inner exception.
			*ppInnerException = __nullptr;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI21235");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCOMUCLIDException::Display()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{	
		// Display the exception
		UCLIDExceptionDlg dlg(CWnd::GetActiveWindow());

		if ( m_upException.get() != __nullptr )
		{	
			m_upException->log("", true, true);
			dlg.display(*m_upException);
		}
		else
		{
			UCLIDException ue("ELI17159", "Encapsulated exception object is NULL!");
			ue.log("", true, true);
			dlg.display(ue);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI01680");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCOMUCLIDException::AddDebugInfo(BSTR bstrKeyName, BSTR bstrStringizedValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{	
		string strKeyName = asString( bstrKeyName );
		string strStringizedValue = asString( bstrStringizedValue );
		
		m_upException->addDebugInfo(strKeyName, strStringizedValue);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01681");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCOMUCLIDException::CreateFromString(BSTR bstrELICode, BSTR bstrData)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		// get the data into a string object
		string strData = asString( bstrData );
		string strELICode = asString( bstrELICode );
		
		// create a new exception object from the given string
		m_upException.reset(new UCLIDException());
		m_upException->createFromString(strELICode, strData);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01686");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCOMUCLIDException::AsStringizedByteStream(BSTR *pbstrData)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		ASSERT_ARGUMENT("ELI25446", pbstrData != __nullptr);

		*pbstrData = _bstr_t(m_upException->asStringizedByteStream().c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03125");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCOMUCLIDException::GetTopELICode(BSTR *pbstrCode)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI25448", pbstrCode != __nullptr);

		// Retrieve top ELI code from UCLID Exception member
		*pbstrCode = _bstr_t(m_upException->getTopELI().c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03068");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCOMUCLIDException::GetTopText(BSTR *pbstrText)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI25449", pbstrText != __nullptr);

		// Retrieve top text from UCLID Exception member
		_bstr_t bstrText = m_upException->getTopText().c_str();
		*pbstrText = bstrText.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03069");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCOMUCLIDException::Log()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_upException->log();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03785");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCOMUCLIDException::SaveTo(BSTR strFullFileName, VARIANT_BOOL bAppend)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		string strFile = asString( strFullFileName );
		m_upException->saveTo(strFile, asCppBool(bAppend));

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03786");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCOMUCLIDException::GetApplicationName(BSTR* pbstrAppName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI32589", pbstrAppName != __nullptr);
		*pbstrAppName = _bstr_t(UCLIDException::getApplication().c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32590");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCOMUCLIDException::LogWithSpecifiedInfo(BSTR bstrMachineName, BSTR bstrUserName,
	long nDateTimeUtc, long nPid, BSTR bstrAppName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		string strMachineName = asString(bstrMachineName);
		string strUserName = asString(bstrUserName);
		string strAppName = asString(bstrAppName);

		// Log the exception with the specified data
		m_upException->log("", true, false,
			strMachineName.empty() ? __nullptr : strMachineName.c_str(),
			strUserName.empty() ? __nullptr : strUserName.c_str(),
			nDateTimeUtc > 0 ? nDateTimeUtc : -1,
			nPid > 0 ? nPid : -1,
			strAppName.empty() ? __nullptr : strAppName.c_str());

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32591");
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
UCLIDException *CCOMUCLIDException::createUCLIDException(
	UCLID_EXCEPTIONMGMTLib::ICOMUCLIDExceptionPtr ipException)
{
	ASSERT_RESOURCE_ALLOCATION("ELI21236", ipException != __nullptr);
	
	// Allocate memory and use unique pointer to cleanup if exception is thrown.
	unique_ptr<UCLIDException> upException;

	// Get the eli code of the exception to convert.
	string strELI = asString(ipException->GetTopELICode());

	// Get the Text description of the exception to convert
	string strDescription = asString(ipException->GetTopText());

	// Get the inner exception
	UCLID_EXCEPTIONMGMTLib::ICOMUCLIDExceptionPtr ipInner = ipException->GetInnerException();

	// if there is an inner exception, convert the inner exception returned to UCLIDException.
	if (ipInner != __nullptr)
	{
		unique_ptr<UCLIDException> upueInner(createUCLIDException(ipInner));
		
		// Create new UCLIDException with inner exception.
		upException.reset(new UCLIDException(strELI, strDescription, *upueInner));
	}
	else
	{
		// Create UCLIDException without an inner exception.
		upException.reset(new UCLIDException(strELI, strDescription));
	}

	// Add the stack trace
	long nStackEntryCount = ipException->GetStackTraceCount();
	for (long n = 0; n < nStackEntryCount; n++)
	{
		string strStackEntry = asString(ipException->GetStackTraceEntry(n));
		upException->addStackTraceEntry(strStackEntry);
	}

	// Add the debug info
	long nDebugInfoCount = ipException->GetDebugInfoCount();
	for (long n = 0; n < nDebugInfoCount; n++)
	{
		_bstr_t bstrKey, bstrValue;
		ipException->GetDebugInfo(n, &bstrKey.GetBSTR(), &bstrValue.GetBSTR());
		upException->addDebugInfo(asString(bstrKey), asString(bstrValue));
	}

	return upException.release();
}
//-------------------------------------------------------------------------------------------------
