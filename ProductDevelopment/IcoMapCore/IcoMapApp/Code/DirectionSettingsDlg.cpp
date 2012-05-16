// DirectionSettingsDlg.cpp : implementation file
//

#include "stdafx.h"
#include "DirectionSettingsDlg.h"

#include <IcoMapOptions.h>
#include <TemporaryResourceOverride.h>
#include <UCLIDException.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

extern HINSTANCE gModuleResource;

/////////////////////////////////////////////////////////////////////////////
// DirectionSettingsDlg property page

IMPLEMENT_DYNCREATE(DirectionSettingsDlg, CPropertyPage)

DirectionSettingsDlg::DirectionSettingsDlg() 
: CPropertyPage(DirectionSettingsDlg::IDD),
  m_bApplied(false),
  m_eDirection(kUnknownDirection),
 m_pToolTips(new CToolTipCtrl)
{
	//{{AFX_DATA_INIT(DirectionSettingsDlg)
	m_AngleType = -1;
	m_DirectionType = -1;
	m_nLineAngleDef = -1;
	//}}AFX_DATA_INIT
}

DirectionSettingsDlg::~DirectionSettingsDlg()
{
	// Clean up tooltips
	if (m_pToolTips)
	{
		delete m_pToolTips;
	}
}

void DirectionSettingsDlg::DoDataExchange(CDataExchange* pDX)
{
	CPropertyPage::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(DirectionSettingsDlg)
	DDX_Radio(pDX, IDC_RADIO_PolarAngle, m_AngleType);
	DDX_Radio(pDX, IDC_RADIO_Bearing, m_DirectionType);
	DDX_Radio(pDX, IDC_RADIO_Deflection, m_nLineAngleDef);
	//}}AFX_DATA_MAP
}


BEGIN_MESSAGE_MAP(DirectionSettingsDlg, CPropertyPage)
	//{{AFX_MSG_MAP(DirectionSettingsDlg)
	ON_BN_CLICKED(IDC_RADIO_Angle, OnRADIOAngle)
	ON_BN_CLICKED(IDC_RADIO_Azimuth, OnRADIOAzimuth)
	ON_BN_CLICKED(IDC_RADIO_Bearing, OnRADIOBearing)
	ON_BN_CLICKED(IDC_RADIO_PolarAngle, OnRADIOPolarAngle)
	ON_BN_CLICKED(IDC_RADIO_Deflection, OnRADIODeflection)
	ON_BN_CLICKED(IDC_RADIO_Internal, OnRADIOInternal)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// DirectionSettingsDlg message handlers

BOOL DirectionSettingsDlg::OnApply() 
{
	//////////////////////////////
	// set direction type
	switch (m_DirectionType)
	{
	case 0:		// Bearing
		m_eDirection = kBearingDir;
		break;
	case 1:		// Angles
		{
			switch (m_AngleType)
			{
			case 0:	// Polar angle
				m_eDirection = kPolarAngleDir;
				break;
			case 1:	// Azimuth
				m_eDirection = kAzimuthDir;
				break;
			default:	// no angle is selected
				{
					m_AngleType = 0;
					UpdateData(FALSE);

					m_eDirection = kPolarAngleDir;
				}
				break;
			}
		}
		break;
	default:	// no Direction is selected
		{
			m_DirectionType = 0;
			UpdateData(FALSE);

			m_eDirection = kBearingDir;
		}
		break;
	}

	// store the direction type into persistent
	IcoMapOptions::sGetInstance().setInputDirection(m_eDirection);

	// set bearing input direction
	static DirectionHelper helper;
	if (m_eDirection != helper.sGetDirectionType())
	{
		helper.sSetDirectionType(m_eDirection);
	}

	///////////////////////////////////////
	// set line angle definition
	bool bIsDeflectionAngle = true;
	// set line angle type
	switch (m_nLineAngleDef)
	{
	case 1:		// internal angle
		{
			bIsDeflectionAngle = false;
		}
		break;
	default:
		{
			// set defaul to deflecition angle
			m_nLineAngleDef = 0;
			UpdateData(FALSE);
		}
		break;
	}

	// set line angle definition
	IcoMapOptions::sGetInstance().setLineAngleDefinition(bIsDeflectionAngle);


	m_bApplied = true;
	
	return CPropertyPage::OnApply();
}

BOOL DirectionSettingsDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);
	
	try
	{
		CPropertyPage::OnInitDialog();

		// Prepare tooltips
		EnableToolTips(true);
		m_pToolTips->Create(this, TTS_ALWAYSTIP);
		m_bInitialized = true;
		
		createToolTips();

		// init direction type, i.e. bearing or azimuth angle or polar angle
		m_eDirection = static_cast<EDirection>(IcoMapOptions::sGetInstance().getInputDirection());

		switch (m_eDirection)
		{
		case kBearingDir:
			{
				// select Bearing direction
				m_DirectionType = 0;
				m_AngleType = 0;
				// disable the Polar angle and Azimuth angle options
				enableAngleDirections(false);
			}
			break;
		case kPolarAngleDir:
			{
				// enable the Polar angle and Azimuth angle options
				enableAngleDirections();
				// select Angle direction
				m_DirectionType = 1;
				m_AngleType = 0;
			}
			break;
		case kAzimuthDir:
			{
				// enable the Polar angle and Azimuth angle options
				enableAngleDirections();
				// select angle direction
				m_DirectionType = 1;
				m_AngleType = 1;
			}
			break;
		default:
			{
				// invalid input direction type, set it to default
				IcoMapOptions::sGetInstance().setInputDirection(1);
				// set default to Bearing direction
				m_DirectionType = 0;
				m_AngleType = 0;
				// disable the Polar angle and Azimuth angle options
				enableAngleDirections(false);

				m_eDirection = kBearingDir;
			}
			break;
		}
					
		// set bearing input direction
		static DirectionHelper directionHelper;
		if (m_eDirection != directionHelper.sGetDirectionType())
		{
			directionHelper.sSetDirectionType(m_eDirection);
		}

		// init line angle definition, i.e. deflection angle or internal angle
		bool bIsDeflectionAngle = IcoMapOptions::sGetInstance().isDefinedAsDeflectionAngle();
		if (bIsDeflectionAngle)
		{
			// deflection angle radio button is selected
			m_nLineAngleDef = 0;
		}
		else
		{
			// internal angle is selected
			m_nLineAngleDef = 1;
		}


		UpdateData(FALSE);

	}
	catch (UCLIDException &uclidException)
	{
		uclidException.display();
		return FALSE;
	}
	catch (...)
	{
		UCLIDException uclidException("ELI02868", "Unknown exception was caught");
		uclidException.display();
		return FALSE;
	}

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}

void DirectionSettingsDlg::OnRADIOAngle() 
{
	UpdateData(TRUE);
	enableAngleDirections();

	switch (m_AngleType)
	{
	case 0:	// Polar angle
		break;
	case 1:	// Azimuth
		break;
	default:	// no angle is selected
		{
			m_AngleType = 0;
			UpdateData(FALSE);
		}
		break;
	}

	SetModified(TRUE);
}

void DirectionSettingsDlg::OnRADIOAzimuth() 
{
	UpdateData(TRUE);
	SetModified(TRUE);
}

void DirectionSettingsDlg::OnRADIOBearing() 
{
	UpdateData(TRUE);

	// disable the Polar angle and azimuth radio buttons
	enableAngleDirections(false);

	SetModified(TRUE);
}

void DirectionSettingsDlg::OnRADIODeflection() 
{
	UpdateData(TRUE);
	SetModified(TRUE);
}

void DirectionSettingsDlg::OnRADIOInternal() 
{
	UpdateData(TRUE);
	SetModified(TRUE);
}

void DirectionSettingsDlg::OnRADIOPolarAngle() 
{
	UpdateData(TRUE);
	SetModified(TRUE);
}

BOOL DirectionSettingsDlg::OnSetActive() 
{
	try
	{
		//if (m_bApplied)
		//{
		//	CancelToClose();
		//}

		IcoMapOptions::sGetInstance().setActiveOptionPageNum(m_iDirectionSettingsPageIndex);
	}
	catch (UCLIDException &uclidException)
	{
		uclidException.display();
		return FALSE;
	}
	catch (...)
	{
		UCLIDException uclidException("ELI02869", "Unknown exception was caught");
		uclidException.display();
		return FALSE;
	}

	return CPropertyPage::OnSetActive();
}

BOOL DirectionSettingsDlg::PreTranslateMessage(MSG* pMsg) 
{
	if (m_bInitialized && m_pToolTips)
	{
		m_pToolTips->RelayEvent(pMsg); 
	}
	
	return CPropertyPage::PreTranslateMessage(pMsg);
}

void DirectionSettingsDlg::createToolTips()
{
	m_pToolTips->AddTool(GetDlgItem(IDC_RADIO_Bearing), "Specify line tool direction as bearing.");
	m_pToolTips->AddTool(GetDlgItem(IDC_RADIO_Angle), "Specify line tool direction as angle in degrees.");
	m_pToolTips->AddTool(GetDlgItem(IDC_RADIO_PolarAngle), "Specify angle as 0° = East, increasing counter-clockwise.");
	m_pToolTips->AddTool(GetDlgItem(IDC_RADIO_Azimuth), "Specify angle as 0° = North, increasing clockwise.");
	m_pToolTips->AddTool(GetDlgItem(IDC_RADIO_Deflection), "Specify angle as deflection angle.");
	m_pToolTips->AddTool(GetDlgItem(IDC_RADIO_Internal), "Specify angle as internal angle.");
}

// **************  Helper functions  ************************
void DirectionSettingsDlg::enableAngleDirections(bool bEnable)
{
	GetDlgItem(IDC_RADIO_PolarAngle)->EnableWindow(bEnable ? TRUE : FALSE);
	GetDlgItem(IDC_RADIO_Azimuth)->EnableWindow(bEnable ? TRUE : FALSE);
}
