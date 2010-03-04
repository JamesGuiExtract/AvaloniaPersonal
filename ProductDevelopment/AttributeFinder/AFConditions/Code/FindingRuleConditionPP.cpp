// FindingRuleConditionPP.cpp : Implementation of CFindingRuleConditionPP

#include "stdafx.h"
#include "FindingRuleConditionPP.h"
#include "..\..\AFCore\Code\AFCategories.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CFindingRuleConditionPP
//-------------------------------------------------------------------------------------------------
CFindingRuleConditionPP::CFindingRuleConditionPP() :
	m_ipCategoryManager(NULL),
	m_ipRuleMap(NULL)
{
	m_dwTitleID = IDS_TITLEFINDINGRULECONDITIONPP;
	m_dwHelpFileID = IDS_HELPFILEFINDINGRULECONDITIONPP;
	m_dwDocStringID = IDS_DOCSTRINGFINDINGRULECONDITIONPP;
}
//-------------------------------------------------------------------------------------------------
CFindingRuleConditionPP::~CFindingRuleConditionPP()
{
	try
	{
		m_ipCategoryManager = NULL;
		m_ipRuleMap = NULL;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI18242");
}
//-------------------------------------------------------------------------------------------------
HRESULT CFindingRuleConditionPP::FinalConstruct()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
void CFindingRuleConditionPP::FinalRelease()
{
}

//-------------------------------------------------------------------------------------------------
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CFindingRuleConditionPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Obtain interface pointer to the IFindingRuleCondition class
		UCLID_AFCONDITIONSLib::IFindingRuleConditionPtr ipFindingRuleCondition = m_ppUnk[0];
		ASSERT_RESOURCE_ALLOCATION("ELI18266", ipFindingRuleCondition != NULL);

		// Initialize controls
		m_cmbRules = GetDlgItem(IDC_COMBO_OBJ);
		m_txtMustBeConfigured = GetDlgItem(IDC_STATIC_CONFIGURE);
		m_btnConfig = GetDlgItem(IDC_BTN_CONFIGURE);

		long nCount = getRuleMap()->Size;

		// If no objects are found that meet the specified criteria, throw an
		// exception
		if (nCount == 0)
		{
			throw UCLIDException("ELI18251", "No qualifying rule objects found!");
		}

		// Populate the combo box
		for (long i = 0; i < nCount; i++)
		{	
			CComBSTR bstrName, bstrProgID;
			getRuleMap()->GetKeyValue(i, &bstrName, &bstrProgID);

			m_cmbRules.AddString(asString(bstrName).c_str());
		}

		// Select the currently configure rule, if there is one.
		_bstr_t bstrRuleName;
		if (ipFindingRuleCondition->AFRule != NULL)
		{
			ICategorizedComponentPtr ipComponent = ipFindingRuleCondition->AFRule;
			ASSERT_RESOURCE_ALLOCATION("ELI18267", ipComponent != NULL);

			bstrRuleName = ipComponent->GetComponentDescription();
			m_cmbRules.SelectString(-1, asString(bstrRuleName).c_str());

			// Update configuation status
			updateRequiresConfig(ipFindingRuleCondition->AFRule);
		}
		else
		{
			// Select first rule by default
			m_cmbRules.SetCurSel(0);

			// Get a pointer to the selected rule object
			IAttributeFindingRulePtr ipNewRule = getSelectedAFRule();
			ASSERT_RESOURCE_ALLOCATION("ELI18310", ipNewRule != NULL);

			// Update configuration message/button appropriately
			updateRequiresConfig(ipNewRule);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18252");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CFindingRuleConditionPP::OnSelChangeCombo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Get a pointer to the selected rule object
		IAttributeFindingRulePtr ipNewRule = getSelectedAFRule();
		ASSERT_RESOURCE_ALLOCATION("ELI18311", ipNewRule != NULL);

		// Update configuration message/button appropriately
		updateRequiresConfig(ipNewRule);

		// Don't assign the object for now... as soon as we do, the user loses configuration settings
		// they had configured for a previously selected object.  Wait until it is absolutely
		// necessary before assigning the selected object (Apply or OnConfigure)
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18285");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CFindingRuleConditionPP::OnConfigure(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Obtain interface pointer to the IFindingRuleCondition class
		UCLID_AFCONDITIONSLib::IFindingRuleConditionPtr ipFindingRuleCondition = m_ppUnk[0];
		ASSERT_RESOURCE_ALLOCATION("ELI18290", ipFindingRuleCondition != NULL);

		// Create the ObjectPropertiesUI object
		IObjectPropertiesUIPtr ipProperties(CLSID_ObjectPropertiesUI);
		ASSERT_RESOURCE_ALLOCATION("ELI18286", ipProperties != NULL);

		// Create a copy of the object for configuration
		ICopyableObjectPtr ipCopyObj(getSelectedAFRule(ipFindingRuleCondition));
		ASSERT_RESOURCE_ALLOCATION("ELI18287", ipCopyObj != NULL);
		ICategorizedComponentPtr ipCopy = ipCopyObj->Clone();
		ASSERT_RESOURCE_ALLOCATION("ELI18314", ipCopy);

		string strComponentDesc = asString(ipCopy->GetComponentDescription());
		string strTitle = string( "Configure " ) + strComponentDesc;

		if(asCppBool(ipProperties->DisplayProperties1(ipCopy, strTitle.c_str())))
		{
			// Store the object now the user has applied configuration settings
			IAttributeFindingRulePtr ipConfiguredRule(ipCopy);
			ASSERT_RESOURCE_ALLOCATION("ELI18292", ipConfiguredRule != NULL);

			ipFindingRuleCondition->AFRule = ipConfiguredRule;

			// Check configuration state of the component
			updateRequiresConfig(ipConfiguredRule);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18293");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFindingRuleConditionPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLTRACE(_T("CFindingRuleConditionPP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			// Obtain interface pointer to the IFindingRuleCondition class
			UCLID_AFCONDITIONSLib::IFindingRuleConditionPtr ipFindingRuleCondition = m_ppUnk[i];
			ASSERT_RESOURCE_ALLOCATION("ELI18268", ipFindingRuleCondition != NULL);
			
			IAttributeFindingRulePtr ipNewRule = getSelectedAFRule(ipFindingRuleCondition);
			ASSERT_RESOURCE_ALLOCATION("ELI18313", ipNewRule != NULL);

			// Check configuration state of the component
			if (updateRequiresConfig(ipNewRule) == true)
			{
				MessageBox("The selected rule has not been configured completely.  Please specify all required properties.", "Configuration");

				// Return S_FALSE to prevent apply being committed if the selected object 
				// has not been configured
				return S_FALSE;
			}

			// Assign the selected rule
			ipFindingRuleCondition->AFRule = ipNewRule;
		}

		SetDirty(FALSE);

		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18269");

	// If we reached here, it's because of an exception
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFindingRuleConditionPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI18246", pbValue != NULL);

		try
		{
			// check the license
			validateLicense();

			// If no exception, then pbValue is true
			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18247");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
ICategoryManagerPtr CFindingRuleConditionPP::getCategoryManager()
{
	if (!m_ipCategoryManager)
	{
		// create category manager object
		m_ipCategoryManager.CreateInstance(CLSID_CategoryManager);
		ASSERT_RESOURCE_ALLOCATION("ELI18249", m_ipCategoryManager != NULL);
	}

	return m_ipCategoryManager;
}
//-------------------------------------------------------------------------------------------------
IStrToStrMapPtr CFindingRuleConditionPP::getRuleMap()
{
	if (!m_ipRuleMap)
	{
		// Create array of required interfaces (just IAttributeFindingRule in this case)
		static const long nIIDCount = 3;
		IID pIIDs[nIIDCount];

		pIIDs[0] = IID_IAttributeFindingRule;
		pIIDs[1] = IID_ICopyableObject;
		pIIDs[2] = IID_IPersistStream;

		// Query category manager for registered value finders which implement IAttributeFindingRule
		m_ipRuleMap = getCategoryManager()->GetDescriptionToProgIDMap2(AFAPI_VALUE_FINDERS_CATEGORYNAME.c_str(),
			nIIDCount, pIIDs);

		ASSERT_RESOURCE_ALLOCATION("ELI18250", m_ipRuleMap != NULL);
	}

	return m_ipRuleMap;
}
//-------------------------------------------------------------------------------------------------
IAttributeFindingRulePtr CFindingRuleConditionPP::getSelectedAFRule(
	UCLID_AFCONDITIONSLib::IFindingRuleConditionPtr ipFindingRuleCondition/*= NULL*/)
{
	if (ipFindingRuleCondition == NULL)
	{
		// If no IFindingRuleConditionPtr was provided, obtain one
		ipFindingRuleCondition = m_ppUnk[0];
		ASSERT_RESOURCE_ALLOCATION("ELI18289", ipFindingRuleCondition != NULL);
	}

	// Get the currently selected rule
	int nIndex = m_cmbRules.GetCurSel();
	if (nIndex < 0)
	{
		UCLIDException ue("ELI18282", "No attribute finding rule is selected!");
		throw ue;
	}

	// Get the selected rules's description
	int nLen = m_cmbRules.GetLBTextLen(nIndex);
	char* buf = new char[nLen+1];
	m_cmbRules.GetLBText(nIndex, buf);
	string strRuleName = buf;
	delete buf;

	// Check to see if the existing AFRule matches the requested description
	// If so, just return the object we already have so that we don't throw
	// away any configuration settings the user may have already applied.
	ICategorizedComponentPtr ipCurrentComponent = ipFindingRuleCondition->AFRule;
	if (ipCurrentComponent != NULL && 
		strRuleName == asString(ipCurrentComponent->GetComponentDescription()))
	{
		return ipFindingRuleCondition->AFRule;
	}

	// Retrieve the Prog ID string
	_bstr_t bstrProgID = m_ipRuleMap->GetValue(get_bstr_t(strRuleName));

	// Create the object
	IAttributeFindingRulePtr ipAFRule((const char *)bstrProgID);
	ASSERT_RESOURCE_ALLOCATION("ELI18262", ipAFRule != NULL);

	return ipAFRule;
}
//-------------------------------------------------------------------------------------------------
bool CFindingRuleConditionPP::updateRequiresConfig(IAttributeFindingRulePtr ipRule)
{
	ASSERT_ARGUMENT("ELI18315", ipRule != NULL);

	// Enable/Disable the configure button as necessary
	ISpecifyPropertyPagesPtr ipPP(ipRule);
	m_btnConfig.EnableWindow(ipPP ? TRUE : FALSE);

	// Check configuration status
	IMustBeConfiguredObjectPtr ipRuleConfig(ipRule);
	if (ipRuleConfig != NULL && ipRuleConfig->IsConfigured() == VARIANT_FALSE)
	{
		// Show message to indicate the object needs configuration
		m_txtMustBeConfigured.ShowWindow(SW_SHOW);	
		return true;
	}
	else
	{
		// Object doesn't need configuration or is configured
		m_txtMustBeConfigured.ShowWindow(SW_HIDE);
		return false;
	}
}
//-------------------------------------------------------------------------------------------------
void CFindingRuleConditionPP::validateLicense()
{
	VALIDATE_LICENSE(gnFLEXINDEX_IDSHIELD_CORE_OBJECTS, "ELI18245", "Finding Rule Condition PP");
}
//-------------------------------------------------------------------------------------------------
