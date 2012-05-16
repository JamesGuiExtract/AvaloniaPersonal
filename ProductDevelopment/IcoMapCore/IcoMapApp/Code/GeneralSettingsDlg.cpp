//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	GeneralSettingsDlg.h
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS: Duan Wang
//
//==================================================================================================

#include "stdafx.h"
#include "resource.h"
#include "GeneralSettingsDlg.h"

#include <IcoMapOptions.h>
#include <UCLIDException.h>
#include <TemporaryResourceOverride.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

extern HINSTANCE gModuleResource;

//-------------------------------------------------------------------------------------------------
// GeneralSettingsDlg property page
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(GeneralSettingsDlg, CPropertyPage)

//-------------------------------------------------------------------------------------------------
GeneralSettingsDlg::GeneralSettingsDlg() 
:CPropertyPage(GeneralSettingsDlg::IDD), 
 m_bInitialized(false), 
 m_bApplied(false),
 m_pToolTips(new CToolTipCtrl)
{
	EnableAutomation();
	//{{AFX_DATA_INIT(GeneralSettingsDlg)
	m_bAutoLinking = FALSE;
	m_iPrecision = 0;
	m_nDefaultUnit = -1;
	m_bCreateAttrField = FALSE;
	m_iPDFResolution = 0;
	//}}AFX_DATA_INIT
}
//-------------------------------------------------------------------------------------------------
GeneralSettingsDlg::~GeneralSettingsDlg()
{
	// Release all tooltips
	if (m_pToolTips)
	{
		delete m_pToolTips;
	}
}
//-------------------------------------------------------------------------------------------------
void GeneralSettingsDlg::OnFinalRelease()
{
	// When the last reference for an automation object is released
	// OnFinalRelease is called.  The base class will automatically
	// deletes the object.  Add additional cleanup required for your
	// object before calling the base class.

	CPropertyPage::OnFinalRelease();
}
//-------------------------------------------------------------------------------------------------
void GeneralSettingsDlg::DoDataExchange(CDataExchange* pDX)
{
	CPropertyPage::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(GeneralSettingsDlg)
	DDX_Control(pDX, IDC_SPIN_PRECISION, m_spin);
	DDX_Check(pDX, IDC_CHECK_AUTO_LINKING_SRCDOC, m_bAutoLinking);
	DDX_Text(pDX, IDC_EDIT_PRECISION, m_iPrecision);
	DDV_MinMaxInt(pDX, m_iPrecision, 1, 10);
	DDX_CBIndex(pDX, IDC_CMB_UNIT_TYPE, m_nDefaultUnit);
	DDX_Check(pDX, IDC_CHECK_CREATE_ICOMAPATTR, m_bCreateAttrField);
	DDX_Text(pDX, IDC_EDIT_PDF_RESOLUTION, m_iPDFResolution);
	DDV_MinMaxInt(pDX, m_iPDFResolution, 50, 300);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(GeneralSettingsDlg, CPropertyPage)
	//{{AFX_MSG_MAP(GeneralSettingsDlg)
	ON_BN_CLICKED(IDC_CHECK_AUTO_LINKING_SRCDOC, OnCheckAutoLinkingSrcdoc)
	ON_NOTIFY(UDN_DELTAPOS, IDC_SPIN_PRECISION, OnDeltaposSpinPrecision)
	ON_EN_CHANGE(IDC_EDIT_PRECISION, OnChangeEditPrecision)
	ON_CBN_SELCHANGE(IDC_CMB_UNIT_TYPE, OnSelchangeCmbUnitType)
	ON_BN_CLICKED(IDC_CHECK_CREATE_ICOMAPATTR, OnCheckCreateIcomapattr)
	ON_EN_CHANGE(IDC_EDIT_PDF_RESOLUTION, OnChangeEditPDFResolution)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
//-------------------------------------------------------------------------------------------------
BEGIN_DISPATCH_MAP(GeneralSettingsDlg, CPropertyPage)
	//{{AFX_DISPATCH_MAP(GeneralSettingsDlg)
		// NOTE - the ClassWizard will add and remove mapping macros here.
	//}}AFX_DISPATCH_MAP
END_DISPATCH_MAP()
//-------------------------------------------------------------------------------------------------
// Note: we add support for IID_IGeneralSettingsDlg to support typesafe binding
//  from VBA.  This IID must match the GUID that is attached to the 
//  dispinterface in the .ODL file.

// {98D869D3-7549-11D5-817A-0050DAD4FF55}
static const IID IID_IGeneralSettingsDlg =
{ 0x98d869d3, 0x7549, 0x11d5, { 0x81, 0x7a, 0x0, 0x50, 0xda, 0xd4, 0xff, 0x55 } };

BEGIN_INTERFACE_MAP(GeneralSettingsDlg, CPropertyPage)
	INTERFACE_PART(GeneralSettingsDlg, IID_IGeneralSettingsDlg, Dispatch)
END_INTERFACE_MAP()

//-------------------------------------------------------------------------------------------------
// GeneralSettingsDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL GeneralSettingsDlg::OnSetActive() 
{
	try
	{
		//if (m_bApplied)
		//{
		//	CancelToClose();
		//}

		IcoMapOptions::sGetInstance().setActiveOptionPageNum(m_iGeneralSettingsPageIndex);
	}
	catch (UCLIDException &uclidException)
	{
		uclidException.display();
		return FALSE;
	}
	catch (...)
	{
		UCLIDException uclidException("ELI01153", "Unknown exception was caught");
		uclidException.display();
		return FALSE;
	}

	return CPropertyPage::OnSetActive();
}
//-------------------------------------------------------------------------------------------------
BOOL GeneralSettingsDlg::PreTranslateMessage(MSG* pMsg) 
{
	// Provide support for tooltips
	if (m_bInitialized && m_pToolTips)
	{
		m_pToolTips->RelayEvent(pMsg); 
	}
	
	return CPropertyPage::PreTranslateMessage(pMsg);
}
//-------------------------------------------------------------------------------------------------
BOOL GeneralSettingsDlg::OnInitDialog() 
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());
		TemporaryResourceOverride resourceOverride(gModuleResource);

		CPropertyPage::OnInitDialog();

		// Enable tooltips
		EnableToolTips(true);
		m_pToolTips->Create(this, TTS_ALWAYSTIP);
		m_bInitialized = true;

		// Create tooltips for each control
		createToolTips();

		// Setup spin control
		m_spin.SetRange( giMIN_PRECISION_DIGITS, giMAX_PRECISION_DIGITS );

		// Limit associated edit box to 2 characters
		CEdit	*pEdit = NULL;
		pEdit = (CEdit *)GetDlgItem( IDC_EDIT_PRECISION );
		pEdit->SetLimitText( 2 );

		// Read current settings from Options
		m_bAutoLinking = IcoMapOptions::sGetInstance().autoSourceDocLinkingIsEnabled() 
			? TRUE : FALSE;

		m_bCreateAttrField = IcoMapOptions::sGetInstance().isIcoMapAttrFieldCreationEnabled() 
			? TRUE : FALSE;

		m_iPrecision = IcoMapOptions::sGetInstance().getPrecisionDigits();

		// Get PDF Resolution
		m_iPDFResolution = IcoMapOptions::sGetInstance().getPDFResolution();

		// set default distance unit
		m_nDefaultUnit = IcoMapOptions::sGetInstance().getDefaultDistanceUnitType() - 1;

		UpdateData(FALSE);
	}
	catch (UCLIDException &uclidException)
	{
		uclidException.display();
		return FALSE;
	}
	catch (...)
	{
		UCLIDException uclidException("ELI01154", "Unknown exception was caught");
		uclidException.display();
		return FALSE;
	}

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//-------------------------------------------------------------------------------------------------
void GeneralSettingsDlg::OnCheckAutoLinkingSrcdoc() 
{
	// Enable the Apply button
	SetModified( TRUE );
}
//-------------------------------------------------------------------------------------------------
void GeneralSettingsDlg::OnDeltaposSpinPrecision(NMHDR* pNMHDR, LRESULT* pResult) 
{
	NM_UPDOWN* pNMUpDown = (NM_UPDOWN*)pNMHDR;

	// Enable the Apply button
	SetModified( TRUE );
	
	*pResult = 0;
}
//-------------------------------------------------------------------------------------------------
void GeneralSettingsDlg::OnChangeEditPrecision() 
{
	// Enable the Apply button
	SetModified( TRUE );
}
//-------------------------------------------------------------------------------------------------
void GeneralSettingsDlg::OnChangeEditPDFResolution() 
{
	// Enable the Apply button
	SetModified( TRUE );
}
//-------------------------------------------------------------------------------------------------
BOOL GeneralSettingsDlg::OnApply() 
{
	// Store settings
	saveSettings();
	m_bApplied = true;
	
	return CPropertyPage::OnApply();
}
//-------------------------------------------------------------------------------------------------
void GeneralSettingsDlg::OnSelchangeCmbUnitType() 
{
	// Enable the Apply button
	SetModified(TRUE);
}
//-------------------------------------------------------------------------------------------------
void GeneralSettingsDlg::OnCheckCreateIcomapattr() 
{
	// Enable the Apply button
	SetModified(TRUE);
}

//-------------------------------------------------------------------------------------------------
// Public methods
//-------------------------------------------------------------------------------------------------
void GeneralSettingsDlg::createToolTips()
{
	m_pToolTips->AddTool(GetDlgItem(IDC_CHECK_AUTO_LINKING_SRCDOC), "Check this box if you wish to store all source documents as hyperlinks for each feature created using IcoMap drawing tools.");
	m_pToolTips->AddTool(GetDlgItem(IDC_EDIT_PRECISION), "Precision in decimal digits for distances.");
	m_pToolTips->AddTool(GetDlgItem(IDC_SPIN_PRECISION), "Increment or decrement digits of precision.");
	m_pToolTips->AddTool(GetDlgItem(IDC_CMB_UNIT_TYPE), "Default unit for distances.");
	m_pToolTips->AddTool(GetDlgItem(IDC_EDIT_PDF_RESOLUTION), "X and Y resolution in dots per inch used to load PDF images.");
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void GeneralSettingsDlg::saveSettings()
{
	if (m_bInitialized)
	{
		UpdateData(TRUE);

		// Store auto-linking option
		IcoMapOptions::sGetInstance().enableAutoSourceDocLinking(m_bAutoLinking == TRUE);

		// store IcoMapAttr field creation
		IcoMapOptions::sGetInstance().enableIcoMapAttrFieldCreation(m_bCreateAttrField == TRUE);

		// Store precision digits
		IcoMapOptions::sGetInstance().setPrecisionDigits( m_iPrecision );

		// Store PDF resolution
		IcoMapOptions::sGetInstance().setPDFResolution( m_iPDFResolution );

		// Store default distance unit
		EDistanceUnitType eUnit = static_cast<EDistanceUnitType>(m_nDefaultUnit + 1);
		IcoMapOptions::sGetInstance().setDefaultDistanceUnitType(eUnit);
	}
}
//-------------------------------------------------------------------------------------------------
