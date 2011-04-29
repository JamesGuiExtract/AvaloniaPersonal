// AFAboutDlg.cpp : implementation file
//

#include "stdafx.h"
#include "AFCore.h"
#include "AFAboutDlg.h"
#include "AttributeFinderEngine.h"

#include <TemporaryResourceOverride.h>
#include <UCLIDException.h>
#include <cpputil.h>

using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const string gstrPATCH_LETTER = ""; // "A", "B", "C", etc

//-------------------------------------------------------------------------------------------------
std::string getModuleFileName()
{
	// Get module path and filename
	char zFileName[MAX_PATH];
	int ret = ::GetModuleFileName( _Module.m_hInst, zFileName, MAX_PATH );
	if (ret == 0)
	{
		throw UCLIDException( "ELI09713", "Unable to retrieve module file name!" );
	}

	return string( zFileName );
}

//-------------------------------------------------------------------------------------------------
std::string getProductName(EHelpAboutType eType)
{
	// Get Product Name
	CString zProductName;
	switch (eType)
	{
	case UCLID_AFCORELib::kFlexIndexHelpAbout:
		zProductName.LoadString( IDS_FLEXINDEX_PRODUCT );
		break;

	case UCLID_AFCORELib::kIDShieldHelpAbout:
		zProductName.LoadString( IDS_IDSHIELD_PRODUCT );
		break;

	case UCLID_AFCORELib::kRuleTesterHelpAbout:
		zProductName.LoadString( IDS_RULETESTER_PRODUCT );
		break;

	default:
		zProductName.LoadString( IDS_UNKNOWN_PRODUCT );
		break;
	}

	return zProductName.operator LPCTSTR();
}

//-------------------------------------------------------------------------------------------------
std::string getProductVersion(std::string strProduct)
{
	// Retrieve version information from this module
	string strVersion = strProduct;
	strVersion += " Version ";
	strVersion += ::getFileVersion( getModuleFileName() );

	return strVersion;
}

//-------------------------------------------------------------------------------------------------
std::string getAttributeFinderEngineVersion(EHelpAboutType eType)
{
	return getProductVersion( getProductName( eType ) );
}

//-------------------------------------------------------------------------------------------------
// CAFAboutDlg dialog
//-------------------------------------------------------------------------------------------------
CAFAboutDlg::CAFAboutDlg(EHelpAboutType eHelpAboutType, std::string strProduct)
	: CDialog(CAFAboutDlg::IDD),
	  m_strProduct(strProduct),
	  m_eType(eHelpAboutType)
{
	//{{AFX_DATA_INIT(CAFAboutDlg)
		// NOTE: the ClassWizard will add member initialization here
	//}}AFX_DATA_INIT
}
//-------------------------------------------------------------------------------------------------
void CAFAboutDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CAFAboutDlg)
	// NOTE: the ClassWizard will add DDX and DDV calls here
	//}}AFX_DATA_MAP
	DDX_Control(pDX, IDC_APPICON, m_Icon);
}
//-------------------------------------------------------------------------------------------------
HICON CAFAboutDlg::getIconName(EHelpAboutType eType)
{
	// Get the icon for current product
	HICON hCurrentApplicationIcon;
	switch (eType)
	{
	case UCLID_AFCORELib::kFlexIndexHelpAbout:
		hCurrentApplicationIcon = AfxGetApp()->LoadIcon( IDR_RSEDITOR );
		break;

	case UCLID_AFCORELib::kIDShieldHelpAbout:
		hCurrentApplicationIcon = AfxGetApp()->LoadIcon( IDI_ID_SHIELD );
		break;

	case UCLID_AFCORELib::kRuleTesterHelpAbout:
		hCurrentApplicationIcon = AfxGetApp()->LoadIcon( IDI_ICON_RULE_TESTER );
		break;

	default:
		hCurrentApplicationIcon = AfxGetApp()->LoadIcon( IDR_RSEDITOR );
		break;
	}
	return hCurrentApplicationIcon;
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CAFAboutDlg, CDialog)
	//{{AFX_MSG_MAP(CAFAboutDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CAFAboutDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL CAFAboutDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		CDialog::OnInitDialog();

		// Set the caption
		string strCaption = "About ";
		strCaption += getProductName( m_eType );
		SetWindowText( strCaption.c_str() );

		// build the version information string
		string strBaseVersion = getAttributeFinderEngineVersion( m_eType );
		string strVersionString = strBaseVersion + gstrPATCH_LETTER;

		// Add second line to version for Product details
		if (m_strProduct.length() > 0)
		{
			strVersionString += "\r\n";
			strVersionString += m_strProduct;
		}

		// update the Version string in the UI
		SetDlgItemText( IDC_EDIT_PRD_VERSION, strVersionString.c_str() );

		// Update the FKB Version information in the UI
		string strFKBVersion = getFKBVersion();
		SetDlgItemText( IDC_EDIT_FKB_VERSION, strFKBVersion.c_str() );

		// Provide the icon for current product
		HICON hIcon = getIconName( m_eType );
		m_Icon.SetIcon( hIcon );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11642")

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//-------------------------------------------------------------------------------------------------
string CAFAboutDlg::getFKBVersion()
{
	vector<string> vecVersions;

	// Get the non FKB version specific component data folder
	string strComponentDataFolder;
	bool bOverridden;
	CAttributeFinderEngine::getRootComponentDataFolder(strComponentDataFolder, bOverridden);
	
	// First, attempt to retrieve the FKB version of any installation at the root of the directory.	
	string strFKBVersionFile = strComponentDataFolder + string("\\FKBVersion.txt");

	ifstream infile(strFKBVersionFile.c_str());
	if (infile.good())
	{
		// Read the version line
		string strVersionLine;
		getline(infile, strVersionLine);

		// Ensure that the version information is in the expected format
		if (strVersionLine.find("FKB Ver.") == 0)
		{
			vecVersions.push_back(strVersionLine.substr(9));
		}
		else
		{
			// Version file was found, but content is not as excepted
			UCLIDException ue("ELI13443", "Unexpected FKB version information in FKB version file!");
			ue.addDebugInfo("VersionLine", strVersionLine);
			ue.log();

			vecVersions.push_back("[Unexpected FKB version]");
		}
	}

	if (!bOverridden)
	{
		// Now iterate the subdirectories and assume any sub-directory that does not contain an alpha
		// character in the name is another FKB installation where the folder name is the version number.
		vector<string> vecSubFolders = getSubDirectories(strComponentDataFolder, false);
		for (vector<string>::iterator iter = vecSubFolders.begin(); iter != vecSubFolders.end(); iter++)
		{
			string strFolderName = getFileNameFromFullPath(*iter);
			bool containsAlphaChars = false;

			if (strFolderName.find_first_of(gstrALPHA) == string::npos)
			{
				vecVersions.push_back(strFolderName);
			}
		}
	}

	if (vecVersions.size() == 0)
	{
		UCLIDException ue("ELI13442", "Unable to detect FKB version!");
		ue.log();

		return "ERROR: Unknown FKB version!";
	}
	
	string strResult = (vecVersions.size() == 1) ? "FKB Version: " : "FKB Versions: ";
	if (bOverridden)
	{
		// Indicate the component directory location has been overridden via registry key.
		strResult += "(overridden location) ";
	}
	strResult += asString(vecVersions, true, ",").substr(1);

	return strResult;
}
//-------------------------------------------------------------------------------------------------
