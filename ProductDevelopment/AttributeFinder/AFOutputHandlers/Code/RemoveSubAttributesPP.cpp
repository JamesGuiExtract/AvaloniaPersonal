// RemoveSubAttributesPP.cpp : Implementation of CRemoveSubAttributesPP
#include "stdafx.h"
#include "AFOutputHandlers.h"
#include "RemoveSubAttributesPP.h"
#include "..\..\AFCore\Code\AFCategories.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <comutils.h>
#include <ComponentLicenseIDs.h>

// add license management function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// CRemoveSubAttributesPP
//-------------------------------------------------------------------------------------------------
CRemoveSubAttributesPP::CRemoveSubAttributesPP()
:	m_cmbAttributeSelectors(0),
	m_ipCategoryMgr(NULL),
	m_ipObjectMap(NULL),
	m_ipObject(NULL),
	m_btnConfigure(0)
{
	try
	{
		m_dwTitleID = IDS_TITLERemoveSubAttributesPP;
		m_dwHelpFileID = IDS_HELPFILERemoveSubAttributesPP;
		m_dwDocStringID = IDS_DOCSTRINGRemoveSubAttributesPP;

		m_ipDataScorer = __nullptr;

		m_ipCategoryMgr.CreateInstance(CLSID_CategoryManager);
		ASSERT_RESOURCE_ALLOCATION("ELI13303", m_ipCategoryMgr != __nullptr);
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI13302");

}
//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveSubAttributesPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		ATLTRACE(_T("CRemoveSubAttributesPP::Apply\n"));
		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_AFOUTPUTHANDLERSLib::IRemoveSubAttributesPtr ipRSA = m_ppUnk[i];
			if (ipRSA == __nullptr)
			{
				THROW_LOGIC_ERROR_EXCEPTION("ELI09560");
			}
			
			IAttributeSelectorPtr ipAS = m_ipObject;
			ASSERT_RESOURCE_ALLOCATION("ELI13355", ipAS != __nullptr );

			IMustBeConfiguredObjectPtr ipCfgObj ( m_ipObject );
			if ( ipCfgObj != __nullptr )
			{
				if ( ipCfgObj->IsConfigured() == VARIANT_FALSE )
				{
					UCLIDException ue ("ELI13349", "Attribute Selector must be configured.");
					throw ue;
				}
			}
			// Set the attribute selector object
			ipRSA->AttributeSelector = ipAS;

			if (m_chkRemoveIfScore.GetCheck() == 1)
			{
				ipRSA->ConditionalRemove = VARIANT_TRUE;
				if (m_ipDataScorer->GetObject() == NULL)
				{
					m_butChooseDataScorer.SetFocus();
					throw UCLIDException( "ELI09799", "Please select a Data Scorer!" );
				}
				ipRSA->DataScorer = m_ipDataScorer;
				ipRSA->ScoreCondition = (EConditionalOp)m_cmbCondition.GetCurSel();
				
				CComBSTR bstrScore;
				m_editScore.GetWindowText(&bstrScore);
				string strScore = asString(bstrScore);
				if(strScore.length() == 0)
				{
					m_editScore.SetFocus();
					throw UCLIDException( "ELI09801", "Please specify a score." );
				}
				long nScore = asLong(strScore);
				if (nScore < 0 || nScore > 100)
				{
					m_editScore.SetFocus();
					throw UCLIDException( "ELI09800", "Please specify a score between 0 and 100!" );
				}
				ipRSA->ScoreToCompare = asLong(strScore);
				
			}
			else
			{
				ipRSA->ConditionalRemove = VARIANT_FALSE;
			}

		}
		m_bDirty = FALSE;
		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09562")
	// if we reached here, it's because of an exception
	return S_FALSE;
}
//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRemoveSubAttributesPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		if (pbValue == NULL)
		{
			return E_POINTER;
		}

		// Check license
		validateLicense();

		// If no exception, then pbValue is true
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CRemoveSubAttributesPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, 
											  BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_AFOUTPUTHANDLERSLib::IRemoveSubAttributesPtr ipRSA = m_ppUnk[0];
		if (ipRSA != __nullptr)
		{
			// "Create" all the controls
			m_cmbAttributeSelectors = GetDlgItem( IDC_COMBO_ATTRIBUTE_SELECTOR );
			m_btnConfigure = GetDlgItem( IDC_BUTTON_CONFIGURE_SELECTOR );

			m_butChooseDataScorer = GetDlgItem( IDC_BUTTON_CHOOSE_DATA_SCORER );
			m_chkRemoveIfScore = GetDlgItem( IDC_CHECK_REMOVE_IF_SCORE );

			m_editDataScorerName = GetDlgItem( IDC_EDIT_DATA_SCORER_NAME );
			m_cmbCondition = GetDlgItem( IDC_COMBO_CONDITION );
			m_cmbCondition.InsertString(kEQ, "=");
			m_cmbCondition.InsertString(kNEQ, "!=");
			m_cmbCondition.InsertString(kLT, "<");
			m_cmbCondition.InsertString(kGT, ">");
			m_cmbCondition.InsertString(kLEQ, "<=");
			m_cmbCondition.InsertString(kGEQ, ">=");

			m_editScore = GetDlgItem( IDC_EDIT_SCORE );

			// Initialize the UI to the state of the current IRemove AttributesPtr
			m_cmbAttributeSelectors.SetFocus();			
			m_ipObject = ipRSA->AttributeSelector;

			m_chkRemoveIfScore.SetCheck(ipRSA->ConditionalRemove == VARIANT_TRUE ? 1 : 0);
			int nTmp;
			OnClickedRemoveIfScore(0, 0, 0, nTmp);

			m_ipDataScorer = ipRSA->DataScorer;
			ASSERT_RESOURCE_ALLOCATION("ELI09798", m_ipDataScorer != __nullptr);
	
			// set the description of the data scorer object
			string strDesc = m_ipDataScorer->Description;
			m_editDataScorerName.SetWindowText(_bstr_t(strDesc.c_str()));
			m_cmbCondition.SetCurSel(ipRSA->ScoreCondition);

			string strScore = asString(ipRSA->ScoreToCompare);
			m_editScore.SetWindowText(strScore.c_str());

			populateObjectCombo();
		}
	
		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19190");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRemoveSubAttributesPP::OnClickedChooseDataScorer(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Create a copy of the DataScorer object-with-description
		ICopyableObjectPtr ipCopyableObj = m_ipDataScorer;
		ASSERT_RESOURCE_ALLOCATION( "ELI09791", ipCopyableObj != __nullptr );
		
		IObjectWithDescriptionPtr ipDataScorerCopy = ipCopyableObj->Clone();
		ASSERT_RESOURCE_ALLOCATION( "ELI09792", ipDataScorerCopy != __nullptr );

		// Create the IObjectSelectorUI object
		IObjectSelectorUIPtr ipObjSelect( CLSID_ObjectSelectorUI );
		ASSERT_RESOURCE_ALLOCATION( "ELI09793", ipObjSelect != __nullptr );

		// initialize private license for the object
		IPrivateLicensedComponentPtr ipPLComponent = ipObjSelect;
		ASSERT_RESOURCE_ALLOCATION("ELI10315", ipPLComponent != __nullptr);
		_bstr_t _bstrKey = LICENSE_MGMT_PASSWORD.c_str();
		ipPLComponent->InitPrivateLicense(_bstrKey);

		// Prepare the prompts for object selector
		_bstr_t	bstrTitle("Data Scorer");
		_bstr_t	bstrDesc("Data Scorer description");
		_bstr_t	bstrSelect("Select Data Scorer");
		_bstr_t	bstrCategory( AFAPI_DATA_SCORERS_CATEGORYNAME.c_str() );

		// Show the UI
		VARIANT_BOOL vbResult = ipObjSelect->ShowUI1(bstrTitle, bstrDesc, 
			bstrSelect, bstrCategory, ipDataScorerCopy, VARIANT_TRUE);

		// If the user clicks the OK button
		if (vbResult == VARIANT_TRUE)
		{
			// Store the updated DataScorer
			m_ipDataScorer = ipDataScorerCopy;

			// Update the DataScorer description
			string strDesc = m_ipDataScorer->Description;
			m_editDataScorerName.SetWindowText(_bstr_t(strDesc.c_str()));
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09790");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRemoveSubAttributesPP::OnClickedRemoveIfScore(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	if(m_chkRemoveIfScore.GetCheck() == 1)
	{
		m_butChooseDataScorer.EnableWindow(TRUE);
		m_cmbCondition.EnableWindow(TRUE);
		m_editScore.EnableWindow(TRUE);
	}
	else
	{
		m_butChooseDataScorer.EnableWindow(FALSE);
		m_cmbCondition.EnableWindow(FALSE);
		m_editScore.EnableWindow(FALSE);
	}
	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRemoveSubAttributesPP::OnBnClickedButtonConfigureSelector(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Must have a combo box selection
		if (m_ipObject != __nullptr)
		{
			// Create the ObjectPropertiesUI object
			IObjectPropertiesUIPtr	ipProperties( CLSID_ObjectPropertiesUI );
			ASSERT_RESOURCE_ALLOCATION("ELI13306", ipProperties != __nullptr);

			UCLID_COMUTILSLib::ICopyableObjectPtr ipCopyObj = m_ipObject;
			ASSERT_RESOURCE_ALLOCATION("ELI13305", ipCopyObj != __nullptr);
			UCLID_COMUTILSLib::ICategorizedComponentPtr ipCopy = ipCopyObj->Clone();

			string strComponentDesc = ipCopy->GetComponentDescription();
			string strTitle = string( "Configure " ) + strComponentDesc;
			_bstr_t	bstrTitle( strTitle.c_str() );

			if(asCppBool(ipProperties->DisplayProperties1(ipCopy, bstrTitle)))
			{
				m_ipObject = ipCopy;
			}
		}

		// Set dirty flag
		SetDirty( TRUE );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13304");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRemoveSubAttributesPP::OnCbnSelchangeComboAttributeSelector(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{

		// Create a new object
		createSelectedObject();

		// Set dirty flag
		SetDirty( TRUE );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13307");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CRemoveSubAttributesPP::validateLicense()
{
	// Property Page requires Editor license
	VALIDATE_LICENSE( gnRULESET_EDITOR_UI_OBJECT, "ELI09559", 
		"Remove SubAttributes PP" );
}
//-------------------------------------------------------------------------------------------------
void CRemoveSubAttributesPP::populateObjectCombo()
{
	// clear the list
	m_cmbAttributeSelectors.ResetContent();

	// Get the object name
	m_ipObjectMap = m_ipCategoryMgr->GetDescriptionToProgIDMap1(get_bstr_t(AFAPI_ATTRIBUTE_SELECTORS_CATEGORYNAME));

	// Fill the object combo with the object names
	// Get names from map
	CString	zName;
	long nNumEntries = m_ipObjectMap->GetSize();
	UCLID_COMUTILSLib::IVariantVectorPtr ipKeys = m_ipObjectMap->GetKeys();
	ASSERT_RESOURCE_ALLOCATION("ELI10403", ipKeys != __nullptr);
	int i;
	for (i = 0; i < nNumEntries; i++)
	{
		// Add name to combo box
		zName = (char *)_bstr_t( ipKeys->GetItem( i ) );
		m_cmbAttributeSelectors.AddString( zName );
	}
	updateComboSelection();
}
//-------------------------------------------------------------------------------------------------
void CRemoveSubAttributesPP::updateComboSelection()
{
	if ( m_ipObject == __nullptr )
	{
		m_cmbAttributeSelectors.SetCurSel(0);
		createSelectedObject();
	}
	else
	{
		string strDesc = asString(m_ipObject->GetComponentDescription());
		int iSel = m_cmbAttributeSelectors.FindString( 0, strDesc.c_str());
		if ( iSel >= 0 )
		{
			m_cmbAttributeSelectors.SetCurSel(iSel);
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CRemoveSubAttributesPP::updateConfigureButton()
{
	ISpecifyPropertyPagesPtr ipPP( m_ipObject );
	BOOL bEnable = FALSE;
	if (ipPP) 
	{
		bEnable = TRUE;
	}
	m_btnConfigure.EnableWindow(bEnable);
}
//-------------------------------------------------------------------------------------------------
void CRemoveSubAttributesPP::createSelectedObject()
{
	// get the objects description
	long nIndex = m_cmbAttributeSelectors.GetCurSel();
	long nLen = m_cmbAttributeSelectors.GetLBTextLen(nIndex);
	char* buf = new char[nLen+1];
	m_cmbAttributeSelectors.GetLBText(nIndex, buf);
	string strName = buf;
	delete buf;

	// create and object with the appropriate progid
	m_ipObject = createObjectFromName(strName);

	// show or hide the configure button
	updateConfigureButton();
}
//-------------------------------------------------------------------------------------------------
UCLID_COMUTILSLib::ICategorizedComponentPtr CRemoveSubAttributesPP::createObjectFromName(std::string strName)
{
	// Check for the Prog ID in the map
	_bstr_t	bstrName( strName.c_str() );
	if (m_ipObjectMap->Contains( bstrName ) == VARIANT_TRUE)
	{
		// Retrieve the Prog ID string
		_bstr_t	bstrProgID = m_ipObjectMap->GetValue( bstrName );
		
		// Create the object
		UCLID_COMUTILSLib::ICategorizedComponentPtr ipComponent( 
			bstrProgID.operator const char *() );

		// if the object is privately licensed, then initialize
		// the private license
		IPrivateLicensedComponentPtr ipPLComponent = ipComponent;
		if (ipPLComponent)
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
