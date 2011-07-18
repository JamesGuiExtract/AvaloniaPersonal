// FPAboutDlg.cpp : implementation file
//

#include "stdafx.h"
#include "FPAboutDlg.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <TemporaryResourceOverride.h>
#include <RegistryPersistenceMgr.h>
#include <RegConstants.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////
// Constants
/////////////
const string gstrSETTINGS_FOLDER = "\\AttributeFinder\\Settings";
const string gstrCOMPONENT_DATA_FOLDER = "ComponentDataFolder";
const string gstrFKBTextFile = "\\FKBVersion.txt";

//-------------------------------------------------------------------------------------------------
// CFPAboutDlg dialog
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CFPAboutDlg, CDialog)

//-------------------------------------------------------------------------------------------------
CFPAboutDlg::CFPAboutDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CFPAboutDlg::IDD, pParent)
{

}
//-------------------------------------------------------------------------------------------------
CFPAboutDlg::~CFPAboutDlg()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16531");
}
//-------------------------------------------------------------------------------------------------
void CFPAboutDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CFPAboutDlg, CDialog)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CAFAboutDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL CFPAboutDlg::OnInitDialog(void)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		CDialog::OnInitDialog();
		
		// Setup Registry persistence item
		ma_pSettingsCfgMgr = unique_ptr<IConfigurationSettingsPersistenceMgr>(
			new RegistryPersistenceMgr( HKEY_CURRENT_USER, gstrREG_ROOT_KEY ) );

		// Set the Version string
		SetDlgItemText( IDC_EDIT_VERSION, getFileProcessingManagerVersion().c_str() );

		SetDlgItemText( IDC_EDIT_FKB_VERSION, getFKBUpdateVersion().c_str() );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19397")

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
string CFPAboutDlg::getFileProcessingManagerVersion()
{
	// Get module path and filename
	char zFileName[MAX_PATH];
	int ret = ::GetModuleFileName( _Module.m_hInst, zFileName, MAX_PATH );
	if (ret == 0)
	{
		throw UCLIDException( "ELI19373", "Unable to retrieve module file name!" );
	}

	// Retrieve version information from this module
	string strVersion = "File Action Manager";
	strVersion += " Version ";
	strVersion += ::getFileVersion( string( zFileName ) );

	return strVersion;
}
//-------------------------------------------------------------------------------------------------
string CFPAboutDlg::getFKBUpdateVersion()
{
	string strFKBVersion = "";

	// Get the Component data folder
	string strComponentDataFolder = getComponentDataDirectory();

	// Get the complete path to the FKBVersion text file
	string strFKBVersionFile = strComponentDataFolder + string(gstrFKBTextFile);

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
			UCLIDException ue("ELI15707", "Unexpected FKB version information in FKB version file.");
			ue.addDebugInfo("VersionLine", strVersionLine);
			ue.log();

			return string("ERROR: Unexpected FKB version.");
		}
	}
	else
	{
		// version file not found.  Log exception, and return appropriate message
		UCLIDException ue("ELI15708", "Unable to detect FKB version.");
		ue.addDebugInfo("FKBInfoFile", strFKBVersionFile, true);
		ue.log();

		return string("ERROR: Unknown FKB version.");
	}
}
//-------------------------------------------------------------------------------------------------
string CFPAboutDlg::getComponentDataDirectory()
{
	try
	{
		string strComponentDataDir = "";

		if (ma_pSettingsCfgMgr->keyExists(gstrSETTINGS_FOLDER, gstrCOMPONENT_DATA_FOLDER))
		{
			strComponentDataDir = ma_pSettingsCfgMgr->getKeyValue(gstrSETTINGS_FOLDER,
				gstrCOMPONENT_DATA_FOLDER, "");
		}

		// If string is not empty then return the value found from the registry
		if (!strComponentDataDir.empty())
		{
			return strComponentDataDir;
		}

		// Registry key was not defined or the definition was empty
		// build default path to ComponentData
		const string COMPONENT_DATA = "ComponentData";
		string strThisModulePath = ::getModuleDirectory(_Module.m_hInst);

		// Go up one level
		int nLastSlash = strThisModulePath.find_last_of("\\");
		strComponentDataDir = strThisModulePath.substr(0, nLastSlash+1);

		// All modules should be installed in CommonComponents folder, go
		// up one level and into FlexIndexComponents (parallel to CommonComponents)
		strComponentDataDir += "FlexIndexComponents\\" + COMPONENT_DATA;

		return strComponentDataDir;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30250");
}