// ObjSelectDlg.cpp : implementation file
//

#include "stdafx.h"
#include "uclidcomutils.h"
#include "ObjSelectDlg.h"
#include "MiscUtils.h"

#include <cpputil.h>
#include <CommentedTextFileReader.h>
#include <StringTokenizer.h>
#include <UCLIDException.h>
#include <TemporaryResourceOverride.h>
#include <io.h>
#include <LicenseMgmt.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// add license management function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

// String used for None in the combo box
const string gstrNone = "<None>";

//-------------------------------------------------------------------------------------------------
// CObjSelectDlg dialog
//-------------------------------------------------------------------------------------------------
CObjSelectDlg::CObjSelectDlg(std::string strTitleAfterSelect, 
							 std::string strDescriptionPrompt, 
							 std::string strSelectPrompt, 
							 std::string strCategoryName, 
							 UCLID_COMUTILSLib::IObjectWithDescriptionPtr ipObj, 
							 bool bAllowNone, long nNumRequiredIIDs, IID pRequiredIIDs[],
							 CWnd* pParent /*=NULL*/,
							 bool bConfigureDescription /*=true*/)
	: CDialog(CObjSelectDlg::IDD, pParent),
	m_strTitleAfterSelect(strTitleAfterSelect),
	m_strCategoryName(strCategoryName),
	m_ipObject(ipObj),
	m_bAllowNone(bAllowNone),
	m_nNumRequiredIIDs(nNumRequiredIIDs),
	m_pRequiredIIDs(pRequiredIIDs),
	m_bConfigureDescription(bConfigureDescription)
{
	//{{AFX_DATA_INIT(CObjSelectDlg)
	m_zDescription = _T("");
	m_zDescLabel = strDescriptionPrompt.c_str();
	m_zSelectLabel = strSelectPrompt.c_str();
	//}}AFX_DATA_INIT

	// Check Category Manager with user-supplied category name
	UCLID_COMUTILSLib::ICategoryManagerPtr ipCategoryMgr( 
		__uuidof(CategoryManager) );
	if (ipCategoryMgr != __nullptr)
	{
		_bstr_t	bstrCategory( m_strCategoryName.c_str() );
		m_ipObjectMap = ipCategoryMgr->GetDescriptionToProgIDMap2( bstrCategory,
			m_nNumRequiredIIDs, m_pRequiredIIDs );
	}

	// if no objects are found that meet the specified criteria, throw an
	// exception
	if (m_ipObjectMap->Size == 0)
	{
		throw UCLIDException("ELI04618", "No objects found!");
	}
}
//-------------------------------------------------------------------------------------------------
CObjSelectDlg::~CObjSelectDlg()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16515");
}
//-------------------------------------------------------------------------------------------------
void CObjSelectDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CObjSelectDlg)
	DDX_Control(pDX, IDC_STATIC_CONFIGURE, m_lblConfigure);
	DDX_Control(pDX, IDC_COMBO_OBJ, m_comboObject);
	DDX_Control(pDX, IDC_BTN_CONFIGURE, m_btnConfigure);
	DDX_Text(pDX, IDC_EDIT_DESC, m_zDescription);
	DDX_Text(pDX, IDC_STATIC_DESC, m_zDescLabel);
	DDX_Text(pDX, IDC_STATIC_SELECT, m_zSelectLabel);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CObjSelectDlg, CDialog)
	//{{AFX_MSG_MAP(CObjSelectDlg)
	ON_BN_CLICKED(IDC_BTN_CONFIGURE, OnBtnConfigure)
	ON_CBN_SELCHANGE(IDC_COMBO_OBJ, OnSelchangeComboObj)
	ON_EN_UPDATE(IDC_EDIT_DESC, OnUpdateEditDesc)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CObjSelectDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL CObjSelectDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	CDialog::OnInitDialog();

	try
	{	
		///////////////////////
		// Set the window title
		///////////////////////
		if (m_strTitleAfterSelect == "")
		{
			// Use default title
			SetWindowText( "Select Object" );
		}
		else
		{
			// Prepend Select to provided title
			string	strFinal = string( "Select " ) + m_strTitleAfterSelect;
			SetWindowText( strFinal.c_str() );
		}
		
		//////////////////////////////////
		// Check default description label
		//////////////////////////////////
		if (m_zDescLabel.IsEmpty())
		{
			// Use default string
			SetDlgItemText( IDC_STATIC_DESC, "Object description" );
		}
		
		////////////////////////////////
		// Check default selection label
		////////////////////////////////
		if (m_zSelectLabel.IsEmpty())
		{
			// Use default string
			SetDlgItemText( IDC_STATIC_SELECT, "Select object" );
		}
		
		///////////////////////////////////
		// Load combo box and set selection
		///////////////////////////////////
		populateCombo();
		setCombo();
		
		///////////////////////////////////////////
		// Set the actual description, if available
		///////////////////////////////////////////
		if (m_ipObject != __nullptr)
		{			
			// Set the description
			string strDescription = m_ipObject->Description;
			// remove appending component description from the string
			int nLeftBracketPos = strDescription.rfind("<");
			if (nLeftBracketPos != string::npos)
			{
				int nRightBracketPos = strDescription.find(">", nLeftBracketPos);
				if (nRightBracketPos != string::npos)
				{
					// remove the component description in the brackets
					strDescription = strDescription.substr(0, nLeftBracketPos);
				}
			}
			
			m_zDescription = strDescription.c_str();
			m_strUserDescription = strDescription.c_str();
			
			// Refresh the display
			UpdateData( FALSE );
		}

		// If we are not allowing configuration of the description, adjust the controls
		// as necessary. Resize the dialog, hide the description controls and position the rest appropriately.
		if (m_bConfigureDescription == false)
		{
			CWnd *pTest = GetDlgItem(IDC_STATIC_SELECT);

			// Get default window position of the description label
			CRect rectDescLabel;
			GetDlgItem(IDC_STATIC_DESC)->GetWindowRect(rectDescLabel);

			// Get default window position of the object selection label
			CRect rectSelectObjectLabel;
			GetDlgItem(IDC_STATIC_SELECT)->GetWindowRect(rectSelectObjectLabel);
			
			// Determine the vertical offset between the two
			int nVerticalOffset = rectSelectObjectLabel.top - rectDescLabel.top;

			// Shorten the dialog by this amount
			CRect rectThis;
			GetWindowRect(rectThis);
			rectThis.bottom -= nVerticalOffset;
			MoveWindow(rectThis);

			// Hide the description label and edit control
			GetDlgItem(IDC_STATIC_DESC)->ShowWindow(SW_HIDE);
			GetDlgItem(IDC_EDIT_DESC)->ShowWindow(SW_HIDE);

			// Shift all controls on the dialog up. (It doesn't matter that this moves the description 
			// controls offscreen since they are hidden anyway)
			for (CWnd *pwndControl = GetWindow(GW_CHILD); 
				 pwndControl != __nullptr; 
				 pwndControl = pwndControl->GetNextWindow())
			{
				CRect rectControl;
				pwndControl->GetWindowRect(rectControl);
				ScreenToClient(&rectControl);
				rectControl.OffsetRect(0,-nVerticalOffset);
				pwndControl->MoveWindow(rectControl);
			}
		}
		
		// set the enabled/disabled state of the configure button
		// to be corresponding to whether the currently selected
		// component supports configuration
		m_btnConfigure.EnableWindow(asMFCBool(CMiscUtils::SupportsConfiguration(m_ipComponent)));
		
		//////////////////////
		// Set Configure label and display if needed
		//////////////////////
		CString	zLabel;
		zLabel.Format( "The %s must be configured", m_strTitleAfterSelect.c_str() );
		m_lblConfigure.SetWindowText( zLabel.operator LPCTSTR() );
		showReminder();

		m_comboObject.SetFocus();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05340")

	return FALSE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//-------------------------------------------------------------------------------------------------
void CObjSelectDlg::OnOK() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Retrieve latest text
		UpdateData( TRUE );
		
		// check whether the object requires configuration not empty.
		UCLID_COMUTILSLib::IMustBeConfiguredObjectPtr ipConfiguredObj(m_ipComponent);
		if (ipConfiguredObj)
		{
			// Has object been configured yet?
			if (ipConfiguredObj->IsConfigured() == VARIANT_FALSE)
			{
				MessageBox("Object has not been configured completely.  Please specify all required properties.", "Configuration");
				// the object hasn't been configured yet, show the configuration
				// dialog to the user 
				OnBtnConfigure();
				
				// do not close this dialog
				return;
			}
		}
		
		// always append component description at the end of the actual description
		int iIndex = m_comboObject.GetCurSel();
		if (iIndex > -1)
		{
			CString zComponentDesc;
			// get current selected component description
			m_comboObject.GetLBText( iIndex, zComponentDesc );
			if (zComponentDesc.Compare( gstrNone.c_str() ) != 0)
			{
				// append to the description provided by the user
				m_zDescription += "<" + zComponentDesc + ">";
			}
			else // "<None>" selected
			{
				// discard the description text
				m_zDescription = "";

				// set the inner object to NULL
				m_ipObject->Object = NULL;
			}
		}
		
		////////////////////////////////
		// Update the object information
		////////////////////////////////
		if (m_ipObject != __nullptr)
		{
			// Set object pointer
			m_ipObject->Object = m_ipComponent;
			
			// Set description
			_bstr_t	strDescription( m_zDescription.operator LPCTSTR() );
			m_ipObject->Description = strDescription;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05341")
		
	CDialog::OnOK();
}

//-------------------------------------------------------------------------------------------------
// Message handlers
//-------------------------------------------------------------------------------------------------
void CObjSelectDlg::OnBtnConfigure() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Must have a combo box selection
		if (m_ipComponent != __nullptr)
		{
			// Create the ObjectPropertiesUI object
			UCLID_COMUTILSLib::IObjectPropertiesUIPtr	ipProperties( 
				CLSID_ObjectPropertiesUI );
			ASSERT_RESOURCE_ALLOCATION("ELI08453", ipProperties != __nullptr);

			UCLID_COMUTILSLib::ICopyableObjectPtr ipCopyObj = m_ipComponent;
			ASSERT_RESOURCE_ALLOCATION("ELI08454", ipCopyObj != __nullptr);
			UCLID_COMUTILSLib::ICategorizedComponentPtr ipCopy = ipCopyObj->Clone();

			string strComponentDesc = ipCopy->GetComponentDescription();
			string strTitle = string( "Configure " ) + strComponentDesc;
			_bstr_t	bstrTitle( strTitle.c_str() );

			if(asCppBool(ipProperties->DisplayProperties1(ipCopy, bstrTitle)))
			{
				m_ipComponent = ipCopy;
			}

			// Check configured state of Component
			showReminder();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05342")
}
//-------------------------------------------------------------------------------------------------
void CObjSelectDlg::OnSelchangeComboObj() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Get selection text
		int iSel = m_comboObject.GetCurSel();
		CString	zText;
		m_comboObject.GetLBText( iSel, zText );
		
		// Get the object
		// if previous component is same as current component, then no change
		if ((m_ipComponent != __nullptr) && 
			(m_ipComponent->GetComponentDescription() == _bstr_t(zText)))
		{
			return;
		}
		
		m_ipComponent = getObjectFromName( zText.operator LPCTSTR() );
		
		// Replace the user-supplied description
		m_zDescription = m_strUserDescription.c_str();
		
		// Enable or disable the Edit box
		if (m_ipComponent != __nullptr)
		{
			// Component selected, enable the edit box
			GetDlgItem( IDC_EDIT_DESC )->EnableWindow( TRUE );
		}
		else
		{
			// "<None>" selected, disable the edit box
			GetDlgItem( IDC_EDIT_DESC )->EnableWindow( FALSE );
		}
		
		// Check if the object supports configuration.
		m_btnConfigure.EnableWindow(asMFCBool(CMiscUtils::SupportsConfiguration(m_ipComponent)));
		
		// Check configured state of Component
		showReminder();
		
		UpdateData(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05343")
}
//-------------------------------------------------------------------------------------------------
void CObjSelectDlg::OnUpdateEditDesc() 
{
	UpdateData( TRUE );

	// Store the new user string to be used if object selection changes
	m_strUserDescription = m_zDescription.operator LPCTSTR();
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
UCLID_COMUTILSLib::ICategorizedComponentPtr CObjSelectDlg::getObjectFromName(std::string strName)
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
void CObjSelectDlg::populateCombo() 
{
	// If desired, add "<None>" as a choice
	if (m_bAllowNone)
	{
		m_comboObject.AddString( gstrNone.c_str() );
	}

	// Get names from map
	CString	zName;
	long nNumEntries = m_ipObjectMap->GetSize();
	UCLID_COMUTILSLib::IVariantVectorPtr ipKeys = m_ipObjectMap->GetKeys();
	ASSERT_RESOURCE_ALLOCATION("ELI08139", ipKeys != __nullptr);

	for (int i = 0; i < nNumEntries; i++)
	{
		zName = (char *)_bstr_t( ipKeys->GetItem( i ) );

		// The category manager will check licensing when building the cache; don't 
		// recheck here.  If a licensing issue was introduced since the last time
		// the cache was built, an exception will be presented upon object use.

		// Add name to combo box
		m_comboObject.AddString( zName );
	}
}
//-------------------------------------------------------------------------------------------------
void CObjSelectDlg::setCombo() 
{
	// Only act if object is available
	if (m_ipObject != __nullptr)
	{
		// Object needs a pointer, too
		UCLID_COMUTILSLib::ICategorizedComponentPtr	ipComponent = m_ipObject->Object;
		if (ipComponent != __nullptr)
		{
			// Get Component Description of the object
			_bstr_t	bstrCompDesc = ipComponent->GetComponentDescription();
			std::string	strActualName( bstrCompDesc );

			// Set the combo box selection
			m_comboObject.SelectString( -1, strActualName.c_str() );

			// Set the component
			m_ipComponent = ipComponent;
		}
		else
		{
			// Make sure that combo box has items
			int iCount = m_comboObject.GetCount();
			if (iCount > 0)
			{
				// Consider special case where "<None>" is only item
				if (m_bAllowNone && (iCount == 1))
				{
					// Just select it and return
					m_comboObject.SetCurSel( 0 );
					return;
				}

				// Select first combo box item != "<None>"
				int iFirst = m_bAllowNone ? 1 : 0;
				m_comboObject.SetCurSel( iFirst );

				// Get the object from the name
				CString	zName;
				m_comboObject.GetLBText( iFirst, zName );

				m_ipComponent = getObjectFromName( zName.operator LPCTSTR() );
				if (m_ipComponent == __nullptr)
				{
					// Create and throw an exception
					UCLIDException	ue( "ELI04275", "Cannot find object.");
					ue.addDebugInfo( "Object name", zName.operator LPCTSTR() );
					throw ue;
				}
			}
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CObjSelectDlg::showReminder() 
{
	// Check configured state of object
	UCLID_COMUTILSLib::IMustBeConfiguredObjectPtr ipConfiguredObj(m_ipComponent);
	if (ipConfiguredObj)
	{
		// Has object been configured yet?
		if (ipConfiguredObj->IsConfigured() == VARIANT_FALSE)
		{
			// Else Component IS NOT configured and
			// label should be shown
			m_lblConfigure.ShowWindow( SW_SHOW );
			return;
		}
	}

	m_lblConfigure.ShowWindow( SW_HIDE );
}
//-------------------------------------------------------------------------------------------------
