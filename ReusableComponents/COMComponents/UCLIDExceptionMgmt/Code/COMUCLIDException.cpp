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
CCOMUCLIDException::CCOMUCLIDException() : m_pException(__nullptr)
{
	m_pException = new UCLIDException();
}
//-------------------------------------------------------------------------------------------------
CCOMUCLIDException::~CCOMUCLIDException()
{
	try
	{
		if (m_pException != __nullptr)
		{
			delete m_pException;
			m_pException = __nullptr;
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
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{	
		if (m_pException != __nullptr)
		{
			delete m_pException;
			m_pException = __nullptr;
		}

		// If there is an inner exception create a new exception object with an inner exception
		if (pInnerException != __nullptr)
		{
			// Convert the inner exception and make sure the memory gets released.
			unique_ptr<UCLIDException> apueInner(createUCLIDException(pInnerException));

			// Create a new exception object with the inner exception
			m_pException = new UCLIDException(asString(strELICode), asString(strText), *apueInner);
		}
		else
		{
			// Create a new exception object without an inner exception.
			m_pException = new UCLIDException(asString(strELICode), asString(strText));
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI21237");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCOMUCLIDException::AddStackTraceEntry(BSTR strStackTrace)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{	
		// Add the stack trace entry to the encapsulated exception object.
		m_pException->addStackTraceEntry(asString(strStackTrace));
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI21238");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCOMUCLIDException::GetStackTraceEntry(long nIndex, BSTR *pstrStackTrace)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{	
		ASSERT_ARGUMENT("ELI25447", pstrStackTrace != __nullptr);

		// Get the stack trace from the UCLIDException object.
		const vector<string>& rvecStackTrace = m_pException->getStackTrace();

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
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI21239");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCOMUCLIDException::GetStackTraceCount(long *pnIndex)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{	
		ASSERT_RESOURCE_ALLOCATION("ELI21263", pnIndex != __nullptr);

		// Set to the size of the StackTrace vector
		*pnIndex = m_pException->getStackTrace().size();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI21280");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCOMUCLIDException::GetDebugInfo(long nIndex, BSTR* pbstrKeyName, BSTR* pbstrStringizedValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{	
		ASSERT_ARGUMENT("ELI21286", pbstrKeyName != __nullptr);
		ASSERT_ARGUMENT("ELI21287", pbstrStringizedValue != __nullptr);

		// Get the debug info from the UCLIDException object.
		const vector<NamedValueTypePair>& rvecDebugInfo = m_pException->getDebugVector();

		// Check to make sure the index in within the bounds of the debug info vector.
		if ( nIndex < 0 || nIndex >= (long)rvecDebugInfo.size())
		{
			UCLIDException ue("ELI21284", "Index out of bounds!");
			ue.addDebugInfo("Index", nIndex);
			ue.addDebugInfo("Size", rvecDebugInfo.size());
			throw ue;
		}
		
		// Return the Debug Info entry at the indexed position.
		*pbstrStringizedValue = get_bstr_t(rvecDebugInfo[nIndex].GetPair().getValueAsString()).Detach();
		*pbstrKeyName = get_bstr_t(rvecDebugInfo[nIndex].GetName()).Detach();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI21283");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCOMUCLIDException::GetDebugInfoCount(long *pnIndex)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{	
		ASSERT_RESOURCE_ALLOCATION("ELI21281", pnIndex != __nullptr);

		// Set to the size of the debug vector
		*pnIndex = m_pException->getDebugVector().size();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI21279");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCOMUCLIDException::GetInnerException(ICOMUCLIDException **ppInnerException)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{	
		ASSERT_ARGUMENT("ELI21339", ppInnerException != __nullptr);

		// Get the inner exception from the exception object
		const UCLIDException *exInner = m_pException->getInnerException();

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
			*ppInnerException = NULL;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI21235");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCOMUCLIDException::Display()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{	
		// Display the exception
		UCLIDExceptionDlg dlg(CWnd::GetActiveWindow());

		if ( m_pException != __nullptr )
		{	
			m_pException->log("", true, true);
			dlg.display(*m_pException);
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
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{	
		string strKeyName = asString( bstrKeyName );
		string strStringizedValue = asString( bstrStringizedValue );
		
		m_pException->addDebugInfo(strKeyName, strStringizedValue);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI01681");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCOMUCLIDException::CreateFromString(BSTR bstrELICode, BSTR bstrData)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		// get the data into a string object
		string strData = asString( bstrData );
		string strELICode = asString( bstrELICode );
		
		// if an exception object exists, delete it
		if (m_pException != __nullptr)
		{
			delete m_pException;
			m_pException = __nullptr;
		}
		
		// create a new exception object from the given string
		m_pException = new UCLIDException();
		m_pException->createFromString(strELICode, strData);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI01686");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCOMUCLIDException::AsStringizedByteStream(BSTR *pbstrData)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		ASSERT_ARGUMENT("ELI25446", pbstrData != __nullptr);

		_bstr_t bstrData = m_pException->asStringizedByteStream().c_str();
		*pbstrData = bstrData.Detach();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03125");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCOMUCLIDException::GetTopELICode(BSTR *pbstrCode)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI25448", pbstrCode != __nullptr);

		// Retrieve top ELI code from UCLID Exception member
		_bstr_t bstrCode = m_pException->getTopELI().c_str();
		*pbstrCode = bstrCode.Detach();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03068");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCOMUCLIDException::GetTopText(BSTR *pbstrText)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI25449", pbstrText != __nullptr);

		// Retrieve top text from UCLID Exception member
		_bstr_t bstrText = m_pException->getTopText().c_str();
		*pbstrText = bstrText.Detach();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03069");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCOMUCLIDException::Log()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_pException->log();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03785");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCOMUCLIDException::SaveTo(BSTR strFullFileName, VARIANT_BOOL bAppend)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		string strFile = asString( strFullFileName );
		m_pException->saveTo(strFile, asCppBool(bAppend));
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03786");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
UCLIDException *CCOMUCLIDException::createUCLIDException(
	UCLID_EXCEPTIONMGMTLib::ICOMUCLIDExceptionPtr ipException)
{
	ASSERT_RESOURCE_ALLOCATION("ELI21236", ipException != __nullptr);
	
	// Allocate memory and use auto pointer to cleanup if exception is thrown.
	unique_ptr<UCLIDException> apException;

	// Get the eli code of the exception to convert.
	string strELI = asString(ipException->GetTopELICode());

	// Get the Text description of the exception to convert
	string strDescription = asString(ipException->GetTopText());

	// Get the inner exception
	UCLID_EXCEPTIONMGMTLib::ICOMUCLIDExceptionPtr ipInner = ipException->GetInnerException();

	// if there is an inner exception, convert the inner exception returned to UCLIDException.
	if (ipInner != __nullptr)
	{
		unique_ptr<UCLIDException> apueInner(createUCLIDException(ipInner));
		
		// Create new UCLIDException with inner exception.
		apException.reset(new UCLIDException(strELI, strDescription, *apueInner));
	}
	else
	{
		// Create UCLIDException without an inner exception.
		apException.reset(new UCLIDException(strELI, strDescription));
	}

	// Add the stack trace
	long nStackEntryCount = ipException->GetStackTraceCount();
	for (long n = 0; n < nStackEntryCount; n++)
	{
		string strStackEntry = asString(ipException->GetStackTraceEntry(n));
		apException->addStackTraceEntry(strStackEntry);
	}

	// Add the debug info
	long nDebugInfoCount = ipException->GetDebugInfoCount();
	for (long n = 0; n < nDebugInfoCount; n++)
	{
		_bstr_t bstrKey, bstrValue;
		ipException->GetDebugInfo(n, &bstrKey.GetBSTR(), &bstrValue.GetBSTR());
		apException->addDebugInfo(asString(bstrKey), asString(bstrValue));
	}

	return apException.release();
}
//-------------------------------------------------------------------------------------------------
