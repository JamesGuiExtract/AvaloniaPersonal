// RunObjectOnQueryPP.cpp : Implementation of CRunObjectOnQueryPP
#include "stdafx.h"
#include "AFOutputHandlers.h"
#include "RunObjectOnQueryPP.h"
#include "..\..\AFCore\Code\EditorLicenseID.h"
#include "..\..\AFCore\Code\AFCategories.h"

#include <LicenseMgmt.h>
#include <UCLIDException.h>
#include <StringTokenizer.h>
#include <cpputil.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>

// add license management function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// CRunObjectOnQueryPP
//-------------------------------------------------------------------------------------------------
CRunObjectOnQueryPP::CRunObjectOnQueryPP()
: m_ipCategoryMgr(NULL),
  m_ipObjectMap(NULL),
  m_ipObject(NULL)
{
	try
	{
		m_dwTitleID = IDS_TITLERunObjectOnQueryPP;
		m_dwHelpFileID = IDS_HELPFILERunObjectOnQueryPP;
		m_dwDocStringID = IDS_DOCSTRINGRunObjectOnQueryPP;	

		m_ipCategoryMgr.CreateInstance(CLSID_CategoryManager);
		ASSERT_RESOURCE_ALLOCATION("ELI10404", m_ipCategoryMgr != __nullptr);
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI10405");
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRunObjectOnQueryPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLTRACE(_T("CRunObjectOnQueryPP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			// Get the output handler object
			UCLID_AFOUTPUTHANDLERSLib::IRunObjectOnQueryPtr ipObject = m_ppUnk[i];

			// Check Name edit box for text
			CComBSTR bstrAttributeQuery;
			GetDlgItemText( IDC_EDIT_QUERY, bstrAttributeQuery.m_str );
			_bstr_t _bstrQuery( bstrAttributeQuery );
			if (_bstrQuery.length() == 0)
			{
				throw UCLIDException( "ELI10395", "Please specify an Attribute Query!" );
			}


			// ensure the object is configured
			UCLID_COMUTILSLib::IMustBeConfiguredObjectPtr ipConfiguredObj(m_ipObject);
			if (ipConfiguredObj)
			{
				// Has object been configured yet?
				if (ipConfiguredObj->IsConfigured() == VARIANT_FALSE)
				{
					MessageBox("Object has not been configured completely.  Please specify all required properties.", "Configuration");
					// the object hasn't been configured yet, show the configuration
					// dialog to the user 
					BOOL bTmp;
					OnClickedBtnConfigure(0, 0, 0, bTmp);
		
					// do not close the dialog
					return S_FALSE;
				}
			}

			// Store the Query
			ipObject->AttributeQuery = _bstrQuery;

			// Store the Object and Type
			m_cmbObjectType = GetDlgItem( IDC_CMB_OBJECT_TYPE );
			long nCurSel = m_cmbObjectType.GetCurSel();
			ipObject->SetObjectAndIID(getObjectType(nCurSel), m_ipObject);
		}

		SetDirty(FALSE);

		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10396")

	// if we reached here, it's because of an exception
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRunObjectOnQueryPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
LRESULT CRunObjectOnQueryPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, 
											  BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_AFOUTPUTHANDLERSLib::IRunObjectOnQueryPtr ipObject = m_ppUnk[0];
		if (ipObject)
		{
			m_editAttributeQuery = GetDlgItem( IDC_EDIT_QUERY );

			// Retrieve text settings
			string strQuery = ipObject->AttributeQuery;

			// Initialize edit boxes
			m_editAttributeQuery.SetWindowText( strQuery.c_str() );

			// Set focus to the Name editbox
			m_editAttributeQuery.SetSel( 0, -1 );
			m_editAttributeQuery.SetFocus();

			// set up the configure button
			m_btnConfigure = GetDlgItem( IDC_BTN_CONFIGURE );

			// Popluate the Object Type Combo Box
			long nIndex;
			m_cmbObjectType = GetDlgItem( IDC_CMB_OBJECT_TYPE );
			nIndex = m_cmbObjectType.AddString("Attribute Modifier");
			m_cmbObjectType.SetItemDataPtr(nIndex, (void*)&IID_IAttributeModifyingRule);
			nIndex = m_cmbObjectType.AddString("Output Handler");
			m_cmbObjectType.SetItemDataPtr(nIndex, (void*)&IID_IOutputHandler);
			nIndex = m_cmbObjectType.AddString("Attribute Splitter");
			m_cmbObjectType.SetItemDataPtr(nIndex, (void*)&IID_IAttributeSplitter);
			m_cmbObjectType.SetCurSel(1);

			// Get the IID and Object
			IID riid;
			m_ipObject = ipObject->GetObjectAndIID(&riid);

			// Select the appropriate Object type in the combo box
			selectType(riid);
			
			// Fill the object combo box with the appropriate objects for the set type
			populateObjectCombo();

			// If object we are loading has an object already (it this is not a new RunObjectOnQuery object)
			// select the appropriate object in the combo box
			m_cmbObject = GetDlgItem( IDC_CMB_OBJECT );
			if(m_ipObject != __nullptr)
			{
				_bstr_t	bstrCompDesc = m_ipObject->GetComponentDescription();
				std::string	strActualName( bstrCompDesc );

				// Set the combo box selection
				m_cmbObject.SelectString( -1, strActualName.c_str() );	

				// does the object need to be configured
				// this should only happen when an object requires additional
				// configuration in a later version
				showReminder();

				// show or hide the configure button
				updateConfigureButton();
			}
			else
			{
				m_cmbObject.SetCurSel(0);
				createSelectedObject();
			}
		}
	
		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10397");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRunObjectOnQueryPP::OnClickedBtnConfigure(WORD wNotifyCode, WORD wID, 
														   HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Must have a combo box selection
		if (m_ipObject != __nullptr)
		{
			// Create the ObjectPropertiesUI object
			IObjectPropertiesUIPtr	ipProperties( CLSID_ObjectPropertiesUI );
			ASSERT_RESOURCE_ALLOCATION("ELI10409", ipProperties != __nullptr);

			UCLID_COMUTILSLib::ICopyableObjectPtr ipCopyObj = m_ipObject;
			ASSERT_RESOURCE_ALLOCATION("ELI10410", ipCopyObj != __nullptr);
			UCLID_COMUTILSLib::ICategorizedComponentPtr ipCopy = ipCopyObj->Clone();

			string strComponentDesc = ipCopy->GetComponentDescription();
			string strTitle = string( "Configure " ) + strComponentDesc;
			_bstr_t	bstrTitle( strTitle.c_str() );

			if(asCppBool(ipProperties->DisplayProperties1(ipCopy, bstrTitle)))
			{
				m_ipObject = ipCopy;
			}

			// Check configured state of Component
			showReminder();
		}

		// Set dirty flag
		SetDirty( TRUE );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10398");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRunObjectOnQueryPP::OnSelChangeCmbObjectType(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// change the list of objects in the object combo based on the 
		// new object type
		populateObjectCombo();
		m_cmbObject = GetDlgItem( IDC_CMB_OBJECT);
		m_cmbObject.SetCurSel(0);
		createSelectedObject();

		// Set dirty flag
		SetDirty( TRUE );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10406");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CRunObjectOnQueryPP::OnSelChangeCmbObject(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Create a new object
		createSelectedObject();

		// Set dirty flag
		SetDirty( TRUE );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19191");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CRunObjectOnQueryPP::validateLicense()
{
	// Property Page requires Editor license
	VALIDATE_LICENSE( gnRULESET_EDITOR_UI_OBJECT, "ELI10399", 
		"RunObjectOnQuery PP" );
}
//-------------------------------------------------------------------------------------------------
IID CRunObjectOnQueryPP::getObjectType(int nIndex)
{
	return *((IID*)m_cmbObjectType.GetItemDataPtr(nIndex));
}
//-------------------------------------------------------------------------------------------------
void CRunObjectOnQueryPP::populateObjectCombo()
{
	// Get the object type combo box
	m_cmbObjectType = GetDlgItem( IDC_CMB_OBJECT_TYPE );
	int curSel = m_cmbObjectType.GetCurSel();
	IID riid = getObjectType(curSel);

	// get the object combo box
	m_cmbObject = GetDlgItem( IDC_CMB_OBJECT );

	// clear the list
	m_cmbObject.ResetContent();

	// if the type = "None" nothing will be added to the list
	if(riid == GUID_NULL )
	{
		return;
	}

	// Get the object name
	string strCategoryName = getCategoryName(riid);
	m_ipObjectMap = m_ipCategoryMgr->GetDescriptionToProgIDMap1(get_bstr_t(strCategoryName));

	// Fill the object combo with the object names
	// Get names from map
	CString	zName;
	long nNumEntries = m_ipObjectMap->GetSize();
	UCLID_COMUTILSLib::IVariantVectorPtr ipKeys = m_ipObjectMap->GetKeys();
	ASSERT_RESOURCE_ALLOCATION("ELI19192", ipKeys != __nullptr);
	int i;
	for (i = 0; i < nNumEntries; i++)
	{
		// Add name to combo box
		zName = (char *)_bstr_t( ipKeys->GetItem( i ) );
		m_cmbObject.AddString( zName );
	}
}
//-------------------------------------------------------------------------------------------------
std::string CRunObjectOnQueryPP::getCategoryName(IID& riid)
{
	if (riid == IID_IAttributeModifyingRule)
	{
		return AFAPI_VALUE_MODIFIERS_CATEGORYNAME;
	}
	else if (riid == IID_IAttributeSplitter)
	{
		return AFAPI_ATTRIBUTE_SPLITTERS_CATEGORYNAME;
	}
	else if (riid == IID_IOutputHandler)
	{
		return AFAPI_OUTPUT_HANDLERS_CATEGORYNAME;
	}
	else
	{
		THROW_LOGIC_ERROR_EXCEPTION("ELI10416");
	}
}
//-------------------------------------------------------------------------------------------------
void CRunObjectOnQueryPP::createSelectedObject()
{
	m_cmbObject = GetDlgItem( IDC_CMB_OBJECT );

	// get the objects description
	long nIndex = m_cmbObject.GetCurSel();
	long nLen = m_cmbObject.GetLBTextLen(nIndex);
	char* buf = new char[nLen+1];
	m_cmbObject.GetLBText(nIndex, buf);
	string strName = buf;
	delete buf;

	// create and object with the appropriate progid
	m_ipObject = createObjectFromName(strName);

	// display the must configure message if appropriate
	showReminder();
	// show or hide the configure button
	updateConfigureButton();
}
//-------------------------------------------------------------------------------------------------
UCLID_COMUTILSLib::ICategorizedComponentPtr CRunObjectOnQueryPP::createObjectFromName(std::string strName)
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
void CRunObjectOnQueryPP::showReminder() 
{
	ATLControls::CStatic txtConfigure = GetDlgItem( IDC_TXT_MUST_CONFIGURE );
	// Check configured state of object
	UCLID_COMUTILSLib::IMustBeConfiguredObjectPtr ipConfiguredObj(m_ipObject);
	if (ipConfiguredObj)
	{
		// Has object been configured yet?
		if (ipConfiguredObj->IsConfigured() == VARIANT_FALSE)
		{
			// Else Component IS NOT configured and
			// label should be shown
			txtConfigure.ShowWindow( SW_SHOW );
			return;
		}
	}

	txtConfigure.ShowWindow( SW_HIDE );
}
//-------------------------------------------------------------------------------------------------
void CRunObjectOnQueryPP::selectType(IID& riid)
{
	m_cmbObjectType = GetDlgItem( IDC_CMB_OBJECT_TYPE );
	int i;
	for(i = 0; i < m_cmbObjectType.GetCount(); i++)
	{
		IID eTmp = *((IID*)m_cmbObjectType.GetItemDataPtr(i));
		if(eTmp == riid)
		{
			m_cmbObjectType.SetCurSel(i);
			return;
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CRunObjectOnQueryPP::updateConfigureButton()
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
