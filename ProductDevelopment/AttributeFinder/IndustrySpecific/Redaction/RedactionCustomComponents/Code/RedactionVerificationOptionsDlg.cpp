// RedactionVerificationOptionsDlg.cpp : implementation file
//

#include "stdafx.h"
#include "RedactionVerificationOptionsDlg.h"
#include "Settings.h"

#include <TemporaryResourceOverride.h>
#include <UCLIDException.h>
#include <cpputil.h>

//-------------------------------------------------------------------------------------------------
// RedactionVerificationOptionsDlg dialog
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(RedactionVerificationOptionsDlg, CDialog)
//-------------------------------------------------------------------------------------------------
RedactionVerificationOptionsDlg::RedactionVerificationOptionsDlg(CWnd* pParent /*=NULL*/)
	: CDialog(RedactionVerificationOptionsDlg::IDD, pParent),
	  m_eAutoTool(kPan),
	  m_iAutoZoomScale(-1),
      m_bInit(false)
{
}
//-------------------------------------------------------------------------------------------------
RedactionVerificationOptionsDlg::~RedactionVerificationOptionsDlg()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16482");
}
//-------------------------------------------------------------------------------------------------
void RedactionVerificationOptionsDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Check( pDX, IDC_CHECK_AUTOZOOM, m_bAutoZoom);
	DDX_Control(pDX, IDC_ZOOM_SLIDER, m_sliderZoomScale);
	DDX_Control(pDX, IDC_STATIC_ZOOM, m_staticZoomScale);
	DDX_Control(pDX, IDC_CHECK_AUTO_TOOL, m_checkAutoTool);
	DDX_Control(pDX, IDC_COMBO_AUTO_TOOL, m_comboAutoTool);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(RedactionVerificationOptionsDlg, CDialog)
	ON_BN_CLICKED(IDC_CHECK_AUTOZOOM, &RedactionVerificationOptionsDlg::OnBnClickedCheckAutozoom)
	ON_NOTIFY(NM_CUSTOMDRAW, IDC_ZOOM_SLIDER, &RedactionVerificationOptionsDlg::OnNMCustomdrawZoomSlider)
	ON_BN_CLICKED(IDC_CHECK_AUTO_TOOL, &RedactionVerificationOptionsDlg::OnBnClickedCheckAutoTool)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// RedactionVerificationOptionsDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL RedactionVerificationOptionsDlg::OnInitDialog()
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// UpdateData in order to initialize the buttons' HWND
		CDialog::OnInitDialog();

		// Set the auto zoom slider params
		m_sliderZoomScale.SetLineSize(1);
		m_sliderZoomScale.SetRange(1, giZOOM_LEVEL_COUNT);

		// Set the init flag to initialized
		m_bInit = true;
		
		// Default to selecting the pan tool
		int iSelected = m_eAutoTool == kNoTool ? kPan : m_eAutoTool;

		// Fill the combo box
		for (int i = 1; i < kNumAutoSelectTools; i++)
		{
			// Set the text and data for this item
			int iIndex = m_comboAutoTool.AddString( getAutoSelectToolName(i).c_str() );
			m_comboAutoTool.SetItemData(iIndex, i);

			// Select this item if necessary
			if (i == iSelected)
			{
				m_comboAutoTool.SetCurSel(iIndex);
			}
		}

		// Check the auto select tool checkbox iff an auto tool is selected
		m_checkAutoTool.SetCheck(asBSTChecked(m_eAutoTool != kNoTool));

		// Update the UI to reflect the settings
		updateUI();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI14534");

	return TRUE;
}
//-------------------------------------------------------------------------------------------------
void RedactionVerificationOptionsDlg::OnOK()
{
	try
	{
		m_eAutoTool = kNoTool;
		if (m_checkAutoTool.GetState() == BST_CHECKED)
		{
			int iCurSel = m_comboAutoTool.GetCurSel();
			m_eAutoTool = (EAutoSelectTool) m_comboAutoTool.GetItemData(iCurSel);
		}

		CDialog::OnOK();

		m_bInit = false;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI25196")
}
//-------------------------------------------------------------------------------------------------
void RedactionVerificationOptionsDlg::OnBnClickedCheckAutozoom()
{
	try
	{
		UpdateData( TRUE );

		// Based upon the check, enable / disable the slider control
		updateUI();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI25197")
}
//-------------------------------------------------------------------------------------------------
void RedactionVerificationOptionsDlg::OnBnClickedCheckAutoTool()
{
	try
	{
		updateUI();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI25198")
}

//-------------------------------------------------------------------------------------------------
// Public Methods
//-------------------------------------------------------------------------------------------------
bool RedactionVerificationOptionsDlg::getAutoZoom()
{
	return (m_bAutoZoom == TRUE); 
}
//-------------------------------------------------------------------------------------------------
void RedactionVerificationOptionsDlg::setAutoZoom (bool bCheck)
{
	try
	{
		// Set the auto-zoom variable
		if( bCheck )
		{
			m_bAutoZoom = TRUE;
		}
		else
		{
			m_bAutoZoom = FALSE;
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS( "ELI14535" );
}
//-------------------------------------------------------------------------------------------------
int RedactionVerificationOptionsDlg::getAutoZoomScale()
{
	// The return zoom scale 
	int iZoomScale;

	try
	{
		// If auto zoom scale is out of range
		if (m_iAutoZoomScale > giZOOM_LEVEL_COUNT || m_iAutoZoomScale < 1)
		{
			// Create an exception and log it
			UCLIDException ue("ELI14537", "Invalid auto zoom scale, reset to default value!");
			ue.addDebugInfo( "AutoZoomScale", m_iAutoZoomScale );
			ue.log();

			// Set the zoom scale index to default index
			m_iAutoZoomScale = giDEFAULT_ZOOM_LEVEL_INDEX;
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI14541");

	// Set the zoom scale to the current value and return it
	iZoomScale = giZOOM_LEVEL[m_iAutoZoomScale - 1];
	return iZoomScale;
}
//-------------------------------------------------------------------------------------------------
void RedactionVerificationOptionsDlg::setAutoZoomScale( int iScale )
{
	try
	{
		// A flag to control if iScale can be found in the setting
		bool bZoomScaleExist = false;

		for (unsigned int i = 0; i < giZOOM_LEVEL_COUNT; i++)
		{
			if (giZOOM_LEVEL[i] == iScale)
			{
				// Set the auto zoom scale index
				m_iAutoZoomScale = i + 1;
				bZoomScaleExist = true;
				break;
			}
		}

		if (!bZoomScaleExist)
		{
			// Create an exception and log it
			UCLIDException ue("ELI14540", "Invalid auto zoom scale, reset to default value!");
			ue.addDebugInfo( "Scale", iScale );
			ue.log();

			// Set the zoom scale index to default index
			m_iAutoZoomScale = giDEFAULT_ZOOM_LEVEL_INDEX;
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI14536");
}
//-------------------------------------------------------------------------------------------------
EAutoSelectTool RedactionVerificationOptionsDlg::getAutoSelectTool()
{
	return m_eAutoTool; 
}
//-------------------------------------------------------------------------------------------------
void RedactionVerificationOptionsDlg::setAutoSelectTool(EAutoSelectTool eTool)
{	
	if (eTool < 0 || eTool >= kNumAutoSelectTools)
	{
		UCLIDException ue("ELI25199", "Unexpected auto select tool.");
		ue.addDebugInfo("Tool number", eTool);
		ue.log();
		return;
	}

	// Set the auto tool member variable
	m_eAutoTool = eTool;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
string RedactionVerificationOptionsDlg::getAutoSelectToolName(int eTool)
{
	switch (eTool)
	{
	case kPan:
		return "pan";
		break;

	case kSelectHighlight:
		return "select highlight";
		break;
	}

	UCLIDException ue("ELI25195", "Unexpected auto select tool.");
	ue.addDebugInfo("Tool number", eTool);
	throw ue;
}
//-------------------------------------------------------------------------------------------------
void RedactionVerificationOptionsDlg::updateUI()
{
	try
	{
		// Enable/disable the slider control and static control
		m_sliderZoomScale.EnableWindow(m_bAutoZoom);
		m_staticZoomScale.EnableWindow(m_bAutoZoom);
		
		m_sliderZoomScale.SetPos(m_iAutoZoomScale);
		m_staticZoomScale.SetWindowText( asString(m_iAutoZoomScale).c_str() );
			
		UpdateData( TRUE );

		// Enable or disable the auto select combo box
		bool bChecked = m_checkAutoTool.GetCheck() == BST_CHECKED;
		m_comboAutoTool.EnableWindow(asBSTChecked(bChecked));
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14542");
}
//-------------------------------------------------------------------------------------------------
void RedactionVerificationOptionsDlg::OnNMCustomdrawZoomSlider(NMHDR *pNMHDR, LRESULT *pResult)
{
	LPNMCUSTOMDRAW pNMCD = reinterpret_cast<LPNMCUSTOMDRAW>(pNMHDR);
	if (m_bInit && m_bAutoZoom == TRUE)
	{
		// Get the zoom scale from slider bar and set it to the static box
		m_iAutoZoomScale = m_sliderZoomScale.GetPos();
		m_staticZoomScale.SetWindowText(asString(m_iAutoZoomScale).c_str());
	}
	*pResult = 0;
}
//-------------------------------------------------------------------------------------------------