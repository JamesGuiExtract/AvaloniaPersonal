// AFUtilityTester.cpp : Implementation of CAFUtilityTester
#include "stdafx.h"
#include "AFUtilsTest.h"
#include "AFUtilityTester.h"

#include <UCLIDException.h>

#include <string>
#include <vector>

using namespace std;

//-------------------------------------------------------------------------------------------------
// CAFUtilityTester
//-------------------------------------------------------------------------------------------------
CAFUtilityTester::CAFUtilityTester()
:m_ipAFUtility(CLSID_AFUtility)
{
	try
	{
		ASSERT_RESOURCE_ALLOCATION("ELI06979", m_ipAFUtility != __nullptr);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06980")
}
//-------------------------------------------------------------------------------------------------
CAFUtilityTester::~CAFUtilityTester()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16334");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtilityTester::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ITestableComponent
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ITestableComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtilityTester::raw_RunAutomatedTests(IVariantVector* pParams, BSTR strTCLFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())	

	try
	{
		if (m_ipResultLogger == __nullptr)
		{
			throw UCLIDException("ELI06981", "ResultLogger must be set before running the automated test.");
		}

		test1();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06982")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtilityTester::raw_RunInteractiveTests()
{
	// Do nothing at this time
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtilityTester::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())	

	try
	{
		// Store results logger
		m_ipResultLogger = pLogger;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06983")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFUtilityTester::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	// Do nothing at this time
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CAFUtilityTester::test1()
{
	// test putting and getting String property
	m_ipResultLogger->StartTestCase(_bstr_t("TEST_1"), 
		_bstr_t("Testing GetNameToAttributesMap"), kAutomatedTestCase); 
	
	bool bExceptionCaught = false;
	bool bSuccess = true;
	try
	{
		// create several attributes' name and value
		vector<string> vecNames, vecValues;
		vecNames.push_back("One");
		vecNames.push_back("Two");
		vecNames.push_back("Two");
		vecNames.push_back("Four");

		vecValues.push_back("Paper");
		vecValues.push_back("Book");
		vecValues.push_back("Shelf");
		vecValues.push_back("Clip");

		IIUnknownVectorPtr ipAttributes(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI06984", ipAttributes != __nullptr);
		IStrToObjectMapPtr ipExpectedNameToAttributesMap(CLSID_StrToObjectMap);
		ASSERT_RESOURCE_ALLOCATION("ELI06988", ipExpectedNameToAttributesMap != __nullptr);

		// create attributes
		int nNumOfAttributes = vecNames.size();
		for (int i=0; i<nNumOfAttributes; i++)
		{
			IAttributePtr ipAttr(CLSID_Attribute);
			ASSERT_RESOURCE_ALLOCATION("ELI06985", ipAttr != __nullptr);
			
			_bstr_t _bstrAttrName(vecNames[i].c_str());
			ipAttr->Name = _bstrAttrName;
			ISpatialStringPtr ipValue(CLSID_SpatialString);
			ASSERT_RESOURCE_ALLOCATION("ELI06986", ipValue != __nullptr);
			ipValue->CreateNonSpatialString(vecValues[i].c_str(), "");

			ipAttr->Value = ipValue;
			ipAttributes->PushBack(ipAttr);

			if (ipExpectedNameToAttributesMap->Contains(_bstrAttrName) == VARIANT_TRUE)
			{
				IIUnknownVectorPtr ipAttrsWithSameName 
					= ipExpectedNameToAttributesMap->GetValue(_bstrAttrName);
				if (ipAttrsWithSameName)
				{
					ipAttrsWithSameName->PushBack(ipAttr);
				}
			}
			else
			{
				IIUnknownVectorPtr ipAttrsWithSameName(CLSID_IUnknownVector);
				ASSERT_RESOURCE_ALLOCATION("ELI06989", ipAttrsWithSameName != __nullptr);
				ipAttrsWithSameName->PushBack(ipAttr);
				ipExpectedNameToAttributesMap->Set(_bstrAttrName, ipAttrsWithSameName);
			}
		}

		// get the name to attributes map
		IStrToObjectMapPtr ipNameToAttributesMap 
			= m_ipAFUtility->GetNameToAttributesMap(ipAttributes);

		// first check the number of distinctive attributes
		long nSize = ipNameToAttributesMap->Size;
		CString zTemp("");
		if (nSize != 3)
		{
			bSuccess = false;
			zTemp.Format("The expected size is : 3\r\n"
				"The actual size is : %d", nSize);
			m_ipResultLogger->AddTestCaseMemo("Result", _bstr_t(zTemp));
		}
		else
		{
			IVariantVectorPtr ipExpectedAttrNames = ipExpectedNameToAttributesMap->GetKeys();
			IVariantVectorPtr ipFoundAttrNames = ipNameToAttributesMap->GetKeys();
			long nNumOfAttrNames = ipExpectedAttrNames->Size;
			for (long n=0; n<nNumOfAttrNames; n++)
			{
				_bstr_t _bstrName(ipExpectedAttrNames->GetItem(n));
				if (ipFoundAttrNames->Contains(_bstrName) == VARIANT_FALSE)
				{
					bSuccess = false;
					break;
				}

				// find the name in the map
				IIUnknownVectorPtr ipExpectedAttrs = ipExpectedNameToAttributesMap->GetValue(_bstrName);
				IIUnknownVectorPtr ipFoundAttrs = ipNameToAttributesMap->GetValue(_bstrName);
				if (ipExpectedAttrs->IsOrderFreeEqualTo(ipFoundAttrs) == VARIANT_FALSE)
				{
					bSuccess = false;
					break;
				}
			}
		}
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI06987", m_ipResultLogger, bExceptionCaught, VARIANT_FALSE);

	bSuccess = bSuccess && !bExceptionCaught;
	m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
}
//-------------------------------------------------------------------------------------------------
