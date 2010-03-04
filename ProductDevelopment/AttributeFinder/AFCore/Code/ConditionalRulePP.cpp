// ConditionalRulePP.cpp : Implementation of CConditionalRulePP
#include "stdafx.h"
#include "AFCore.h"
#include "ConditionalRulePP.h"

#include "EditorLicenseID.h"
#include <UCLIDException.h>
#include <cpputil.h>
#include <COMUtils.h>
#include <LicenseMgmt.h>

using std::string;

// add license management function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// CConditionalRulePP
//-------------------------------------------------------------------------------------------------
CConditionalRulePP::CConditionalRulePP() 
{
	try
	{
		m_dwTitleID = IDS_TITLEConditionalRulePP;
		m_dwHelpFileID = IDS_HELPFILEConditionalRulePP;
		m_dwDocStringID = IDS_DOCSTRINGConditionalRulePP;
	
		m_ipCategoryMgr.CreateInstance(CLSID_CategoryManager);
		ASSERT_RESOURCE_ALLOCATION("ELI10759", m_ipCategoryMgr != NULL);
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI10758");
}
//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalRulePP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLTRACE(_T("CConditionalRulePP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_AFCORELib::IConditionalRulePtr ipRule = m_ppUnk[i];
			ASSERT_RESOURCE_ALLOCATION("ELI10769", ipRule != NULL);
			
			// ensure the Condition is configured
			if (objectMustBeConfigured(m_ipCondition))
			{
				MessageBox("Condition object has not been configured completely.  Please specify all required properties.", "Configuration");
				// the object hasn't been configured yet, show the configuration
				// dialog to the user 
				BOOL bTmp;
				OnClickedBtnConfigCondition(0, 0, 0, bTmp);
	
				// do not close the dialog
				return S_FALSE;
			}

			// ensure the rule object is configured
			if (objectMustBeConfigured(m_ipRule))
			{
				MessageBox("Rule object has not been configured completely.  Please specify all required properties.", "Configuration");
				// the object hasn't been configured yet, show the configuration
				// dialog to the user 
				BOOL bTmp;
				OnClickedBtnConfigRule(0, 0, 0, bTmp);
	
				// do not close the dialog
				return S_FALSE;
			}

			// Save the condition
			ipRule->SetCondition(m_ipCondition);
			// Save the Rule
			ipRule->SetRule(m_ipRule);

			m_cmbTrueFalse = GetDlgItem(IDC_CMB_TRUE_FALSE);
			int nCurrentIndex = m_cmbTrueFalse.GetCurSel();
			ipRule->InvertCondition = (nCurrentIndex == 0) ? VARIANT_FALSE : VARIANT_TRUE;

		}

		SetDirty(FALSE);
		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10746")

	// if we reached here, it's because of an exception
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CConditionalRulePP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, 
									 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_AFCORELib::IConditionalRulePtr ipRule = m_ppUnk[0];
		if (ipRule != NULL)
		{
			m_cmbTrueFalse = GetDlgItem(IDC_CMB_TRUE_FALSE);
			m_cmbCondition = GetDlgItem(IDC_CMB_CONDITION);
			m_cmbRule = GetDlgItem(IDC_CMB_RULE);
			m_btnConfigCondition = GetDlgItem(IDC_BTN_CONFIG_CONDITION);
			m_btnConfigRule = GetDlgItem(IDC_BTN_CONFIG_RULE);
		
			m_cmbTrueFalse.InsertString(0, _bstr_t("true"));
			m_cmbTrueFalse.InsertString(1, _bstr_t("false"));
			m_cmbTrueFalse.SetCurSel(ipRule->InvertCondition == VARIANT_TRUE ? 1 : 0);

			string strCategoryName = ipRule->GetCategoryName();

			m_ipConditionMap = populateCombo(m_cmbCondition, AFAPI_CONDITIONS_CATEGORYNAME);
			m_ipCondition = ipRule->GetCondition();
			if(m_ipCondition != NULL)
			{
				ICategorizedComponentPtr ipObject(m_ipCondition);
				ASSERT_RESOURCE_ALLOCATION("ELI10763", ipObject != NULL);
				_bstr_t bstrName = ipObject->GetComponentDescription();
				m_cmbCondition.SelectString( -1, bstrName );	
			}
			else
			{
					m_cmbCondition.SetCurSel(0);
					m_ipCondition = createSelectedObject(m_cmbCondition, m_ipConditionMap);
			}

			m_ipRuleMap = populateCombo(m_cmbRule, strCategoryName);
			m_ipRule = ipRule->GetRule();
			if(m_ipRule != NULL)
			{
				ICategorizedComponentPtr ipObject(m_ipRule);
				ASSERT_RESOURCE_ALLOCATION("ELI10771", ipObject != NULL);
				_bstr_t bstrName = ipObject->GetComponentDescription();
				m_cmbRule.SelectString( -1, bstrName );	
			}
			else
			{
				m_cmbRule.SetCurSel(0);
				m_ipRule = createSelectedObject(m_cmbRule, m_ipRuleMap);
			}
			showReminder();
			toggleConfigButtons();
		}

		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10747");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CConditionalRulePP::OnClickedBtnConfigCondition(WORD wNotifyCode, 
													 WORD wID, HWND hWndCtl, 
													 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ICategorizedComponentPtr ipComponent = m_ipCondition;
		ASSERT_RESOURCE_ALLOCATION("ELI10767", ipComponent != NULL);
		configureComponent(ipComponent);
		m_ipCondition = ipComponent;
		showReminder();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10752");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CConditionalRulePP::OnClickedBtnConfigRule(WORD wNotifyCode, 
													 WORD wID, HWND hWndCtl, 
													 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ICategorizedComponentPtr ipComponent = m_ipRule;
		ASSERT_RESOURCE_ALLOCATION("ELI10768", ipComponent != NULL);
		configureComponent(ipComponent);
		m_ipRule = ipComponent;
		showReminder();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10753");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CConditionalRulePP::OnSelChangeCmbCondition(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{

		// Create a new object
		m_ipCondition = createSelectedObject(m_cmbCondition, m_ipConditionMap);
		showReminder();
		toggleConfigButtons();

		// Set dirty flag
		SetDirty( TRUE );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10407");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CConditionalRulePP::OnSelChangeCmbRule(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{

		// Create a new object
		m_ipRule = createSelectedObject(m_cmbRule, m_ipRuleMap);
		showReminder();
		toggleConfigButtons();

		// Set dirty flag
		SetDirty( TRUE );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19125");

	return 0;
}
//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
IStrToStrMapPtr CConditionalRulePP::populateCombo(ATLControls::CComboBox& m_cmbObjects, const std::string& strCategoryName)
{	
	// clear the list
	m_cmbObjects.ResetContent();

	IStrToStrMapPtr ipMap = m_ipCategoryMgr->GetDescriptionToProgIDMap1(get_bstr_t(strCategoryName));

	// Fill the object combo with the object names
	// Get names from map
	CString	zName;
	long nNumEntries = ipMap->GetSize();
	UCLID_COMUTILSLib::IVariantVectorPtr ipKeys = ipMap->GetKeys();
	ASSERT_RESOURCE_ALLOCATION("ELI10760", ipKeys != NULL);
	int i;
	for (i = 0; i < nNumEntries; i++)
	{
		// Add name to combo box
		zName = (char *)_bstr_t( ipKeys->GetItem( i ) );
		m_cmbObjects.AddString( zName );
	}
	return ipMap;
}
//-------------------------------------------------------------------------------------------------
ICategorizedComponentPtr CConditionalRulePP::createSelectedObject(ATLControls::CComboBox& m_cmbObjects, IStrToStrMapPtr ipMap)
{
	// get the objects description
	long nIndex = m_cmbObjects.GetCurSel();
	long nLen = m_cmbObjects.GetLBTextLen(nIndex);
	char* buf = new char[nLen+1];
	m_cmbObjects.GetLBText(nIndex, buf);
	string strName = buf;
	delete buf;

	// create and object with the appropriate progid
	ICategorizedComponentPtr ipObject = createObjectFromName(strName, ipMap);
	return ipObject;
}
//-------------------------------------------------------------------------------------------------
ICategorizedComponentPtr CConditionalRulePP::createObjectFromName(std::string strName, IStrToStrMapPtr ipMap)
{
	// Check for the Prog ID in the map
	_bstr_t	bstrName( strName.c_str() );
	if (ipMap->Contains( bstrName ) == VARIANT_TRUE)
	{
		// Retrieve the Prog ID string
		_bstr_t	bstrProgID = ipMap->GetValue( bstrName );
		
		// Create the object
		UCLID_COMUTILSLib::ICategorizedComponentPtr ipComponent( 
			bstrProgID.operator const char *() );

		// if the object is privately licensed, then initialize
		// the private license
		IPrivateLicensedComponentPtr ipPLComponent = ipComponent;
		if (ipPLComponent != NULL)
		{
			_bstr_t _bstrKey = LICENSE_MGMT_PASSWORD.c_str();
			ipPLComponent->InitPrivateLicense(_bstrKey);
		}

		return ipComponent;
	}
	
	// Not found in map, just return NULL
	return NULL;
}
//-------------------------------------------------------------------------------------------------
void CConditionalRulePP::showReminder()
{
	ATLControls::CStatic txtConfigure = GetDlgItem( IDC_TEXT_MUST_CONFIG );

	bool bConfigCondition = objectMustBeConfigured(m_ipCondition);
	bool bConfigRule = objectMustBeConfigured(m_ipRule);
	string strMessage;
	if (bConfigCondition && bConfigRule)
	{
		strMessage = "Condition and Rule objects both must be configured";
	}
	else if (bConfigCondition)
	{
		strMessage = "Condition object must be configured";
	}
	else if (bConfigRule)
	{
		strMessage = "Rule object must be configured";
	}

	txtConfigure.SetWindowText(_bstr_t(strMessage.c_str()));
	txtConfigure.ShowWindow( SW_SHOW );
}
//-------------------------------------------------------------------------------------------------
void CConditionalRulePP::toggleConfigButtons()
{
	ISpecifyPropertyPagesPtr ipPP( m_ipCondition );
	BOOL bEnable = FALSE;
	if (ipPP != NULL) 
	{
		bEnable = TRUE;
	}
	m_btnConfigCondition.EnableWindow(bEnable);

	ipPP = m_ipRule;
	bEnable = FALSE;
	if (ipPP != NULL) 
	{
		bEnable = TRUE;
	}
	m_btnConfigRule.EnableWindow(bEnable);
}
//-------------------------------------------------------------------------------------------------
void CConditionalRulePP::configureComponent(ICategorizedComponentPtr& ipObject)
{
	// Must have a combo box selection
	if (ipObject != NULL)
	{
		// Create the ObjectPropertiesUI object
		IObjectPropertiesUIPtr	ipProperties( CLSID_ObjectPropertiesUI );
		ASSERT_RESOURCE_ALLOCATION("ELI10764", ipProperties != NULL);

		UCLID_COMUTILSLib::ICopyableObjectPtr ipCopyObj = ipObject;
		ASSERT_RESOURCE_ALLOCATION("ELI10765", ipCopyObj != NULL);
		UCLID_COMUTILSLib::ICategorizedComponentPtr ipCopy = ipCopyObj->Clone();
		ASSERT_RESOURCE_ALLOCATION("ELI10766", ipCopy != NULL);

		string strComponentDesc = ipCopy->GetComponentDescription();
		string strTitle = string( "Configure " ) + strComponentDesc;
		_bstr_t	bstrTitle( strTitle.c_str() );

		if(asCppBool(ipProperties->DisplayProperties1(ipCopy, bstrTitle)))
		{
			ipObject = ipCopy;
		}
	}

	// Set dirty flag
	SetDirty( TRUE );
}
//-------------------------------------------------------------------------------------------------
bool CConditionalRulePP::objectMustBeConfigured(IUnknownPtr ipObject)
{
	UCLID_COMUTILSLib::IMustBeConfiguredObjectPtr ipConfiguredObj(ipObject);
	if (ipConfiguredObj)
	{
		// Has object been configured yet?
		if (ipConfiguredObj->IsConfigured() == VARIANT_FALSE)
		{
			return true;
		}
	}
	return false;
}