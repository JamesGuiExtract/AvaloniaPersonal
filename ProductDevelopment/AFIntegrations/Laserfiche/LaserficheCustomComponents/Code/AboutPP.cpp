// AboutPP.cpp : implementation file
//

#include "stdafx.h"
#include "LaserficheCustomComponents.h"
#include "IDShieldLF.h"
#include "AboutPP.h"

#include <UCLIDException.h>
#include <Win32Util.h>

//--------------------------------------------------------------------------------------------------
// Contants
//--------------------------------------------------------------------------------------------------
static const string gstrPATCH = "B";

//--------------------------------------------------------------------------------------------------
// CAboutPP dialog
//--------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CAboutPP, CPropertyPage)
//--------------------------------------------------------------------------------------------------
CAboutPP::CAboutPP() 
	: CPropertyPage(CAboutPP::IDD)
{

}
//--------------------------------------------------------------------------------------------------
CAboutPP::~CAboutPP()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI21706");
}
//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CAboutPP, CPropertyPage)
END_MESSAGE_MAP()

//--------------------------------------------------------------------------------------------------
// Overrides
//--------------------------------------------------------------------------------------------------
void CAboutPP::DoDataExchange(CDataExchange* pDX)
{
	CPropertyPage::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_APPICON, m_Icon);
}
//--------------------------------------------------------------------------------------------------
BOOL CAboutPP::OnInitDialog()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		CPropertyPage::OnInitDialog();

		// Set the caption
		string strCaption = "About " + gstrPRODUCT_NAME;
		SetWindowText(strCaption.c_str());

		// Build the version information string
		string strVersion = gstrPRODUCT_NAME;
		strVersion += " Version ";
		strVersion += ::getFileVersion(getAppFullPath());
		if (!gstrPATCH.empty())
		{
			strVersion += gstrPATCH;
		}

		// Update the Version string in the UI
		SetDlgItemText( IDC_EDIT_PRD_VERSION, strVersion.c_str() );

		// Update the FKB Version information in the UI
		string strFKBVersion = getFKBVersion();
		SetDlgItemText( IDC_EDIT_FKB_VERSION, strFKBVersion.c_str() );

		// Provide the icon for ID Shield for LF
		HICON hIcon = LoadIcon(AfxGetInstanceHandle(), MAKEINTRESOURCE(IDI_IDSHIELD));
		m_Icon.SetIcon(hIcon);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI21710");

	return TRUE;
}

//--------------------------------------------------------------------------------------------------
// Private methods
//--------------------------------------------------------------------------------------------------
string CAboutPP::getFKBVersion()
{
	string strResult = "FKB Version: ";

	// Create an AFEngine instance to get the component data folder
	IAttributeFinderEnginePtr ipAFEngine(CLSID_AttributeFinderEngine);
	ASSERT_RESOURCE_ALLOCATION("ELI21713", ipAFEngine != NULL);

	// Get the component data folder & FKB version
	string strComponentDataFolder = ipAFEngine->GetComponentDataFolder();
	string strFKBVersionFile = strComponentDataFolder + string("\\FKBVersion.txt");

	// open the FKB version file
	ifstream infile(strFKBVersionFile.c_str());
	if (infile.good())
	{
		// read the version line
		string strVersionLine;
		getline(infile, strVersionLine);

		// ensure that the version information is in the expected format
		if (strVersionLine.find("FKB Ver.") == 0)
		{
			return strVersionLine;
		}
		else
		{
			// version file was found, but content is not as excepted
			UCLIDException ue("ELI21714", "Unexpected FKB version information in FKB version file!");
			ue.addDebugInfo("VersionLine", strVersionLine);
			ue.log();

			return string("ERROR: Unexpected FKB version!");
		}
	}
	else
	{
		// version file not found.  Log exception, and return appropriate message
		UCLIDException ue("ELI21715", "Unable to detect FKB version!");
		ue.log();

		return string("ERROR: Unknown FKB version!");
	}
}
//--------------------------------------------------------------------------------------------------