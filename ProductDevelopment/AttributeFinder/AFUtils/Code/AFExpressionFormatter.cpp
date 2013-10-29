#include "stdafx.h"
#include "AFUtils.h"
#include "AFExpressionFormatter.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>

//--------------------------------------------------------------------------------------------------
// CAFExpressionFormatter
//--------------------------------------------------------------------------------------------------
const string _strEXPRESSION_DELIMITER = "(?~%";

//--------------------------------------------------------------------------------------------------
// CAFExpressionFormatter
//--------------------------------------------------------------------------------------------------
CAFExpressionFormatter::CAFExpressionFormatter()
: m_ipAFDocument(__nullptr)
, m_ipAFUtility(CLSID_AFUtility)
{
	try
	{
		ASSERT_RESOURCE_ALLOCATION("ELI36203", m_ipAFUtility != __nullptr);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI36204");
}
//--------------------------------------------------------------------------------------------------
CAFExpressionFormatter::~CAFExpressionFormatter()
{
	try
	{
		m_ipAFDocument = __nullptr;
		m_ipAFUtility = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI36205");
}
//-------------------------------------------------------------------------------------------------
HRESULT CAFExpressionFormatter::FinalConstruct()
{
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
void CAFExpressionFormatter::FinalRelease()
{
	try
	{
		m_ipAFDocument = __nullptr;
		m_ipAFUtility = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI36206");
}

//--------------------------------------------------------------------------------------------------
// IAFExpressionFormatter
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CAFExpressionFormatter::get_AFDocument(IAFDocument **ppDoc)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI36207", ppDoc != __nullptr);

		validateLicense();
		
		*ppDoc = m_ipAFDocument;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36208")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CAFExpressionFormatter::put_AFDocument(IAFDocument *pDoc)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_ipAFDocument = pDoc;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36209")
}

//-------------------------------------------------------------------------------------------------
// IExpressionFormatter
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFExpressionFormatter::raw_FormatExpression(BSTR bstrExpression, BSTR* pbstrFormatedExpression)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI36210", pbstrFormatedExpression != __nullptr);

		if (m_ipAFDocument == __nullptr)
		{
			UCLIDException ue("ELI36217", "A document has not been provided as context to allow "
				"for format string expansion.");
			throw ue;
		}

		// Look for any nested format strings that need expansion.
		string strExpression = asString(bstrExpression);
		long nPos = strExpression.find(_strEXPRESSION_DELIMITER);
		while (nPos != string::npos)
		{
			if (nPos + _strEXPRESSION_DELIMITER.length() >= strExpression.length())
			{
				UCLIDException ue("ELI36211", "A format string delimiter was not followed by a "
					"format string.");
				throw ue;
			}

			// Expand the nested format string.
			string strFormatString = strExpression.substr(nPos + _strEXPRESSION_DELIMITER.length());
			long nEndPos = -1;
			ISpatialStringPtr ipFormattedString = m_ipAFUtility->ExpandFormatString(
				m_ipAFDocument->Attribute, strFormatString.c_str(), ')', &nEndPos);
			ASSERT_RESOURCE_ALLOCATION("ELI36212", ipFormattedString != __nullptr);

			string strFormattedString = asString(ipFormattedString->String);

			// Replace the format string with expanded result
			nEndPos = (nEndPos == -1) ? -1 : nEndPos + _strEXPRESSION_DELIMITER.length();
			strExpression.erase(nPos, nEndPos);
			if (nPos >= strExpression.length())
			{
				strExpression += asString(ipFormattedString->String);
			}
			else
			{
				strExpression.insert(nPos, asString(ipFormattedString->String));
			}

			// Then search for the next format string following the inserted result.
			nPos = strExpression.find(_strEXPRESSION_DELIMITER,
				nPos + strFormattedString.length());
		}

		*pbstrFormatedExpression = _bstr_t(strExpression.c_str()).Detach();
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36213");
}


//---------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//---------------------------------------------------------------------------------------------------
STDMETHODIMP CAFExpressionFormatter::InterfaceSupportsErrorInfo(REFIID riid)
{
	try
	{
		static const IID* arr[] = 
		{
			&IID_IAFExpressionFormatter,
			&IID_IExpressionFormatter,
			&IID_ILicensedComponent
		};

		for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
		{
			if (InlineIsEqualGUID(*arr[i],riid))
				return S_OK;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36214")
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CAFExpressionFormatter::validateLicense()
{
	VALIDATE_LICENSE(gnRULE_WRITING_CORE_OBJECTS, "ELI36215", "AFExpressionFormatter");
}