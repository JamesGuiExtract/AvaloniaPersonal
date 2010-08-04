// PackageDlg.cpp : implementation file
//

#include "stdafx.h"
#include "resource.h"
#include "PackageDlg.h"

#include <UCLIDException.h>
#include <RegistryPersistenceMgr.h>
#include <StringTokenizer.h>
#include <cpputil.h>
#include <comutils.h>
#include <Zipper.h>
#include <LoadFileDlgThread.h>

#include "..\\..\\..\\AFCore\\Code\\Common.h"


#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------

// String constant for Feedback Manager RunRules file
const std::string	gstrRUNRULESFILE = "UCLID_FeedbackRunRules.dat";
const string gstrAF_UTILS_KEY = gstrAF_REG_ROOT_FOLDER_PATH + string("\\Utils");

//-------------------------------------------------------------------------------------------------
// CPackageDlg dialog
//-------------------------------------------------------------------------------------------------
CPackageDlg::CPackageDlg(IFeedbackMgrInternalsPtr ipFBMgr, CWnd* pParent /*=NULL*/)
	: CDialog(CPackageDlg::IDD, pParent),
	  m_bReadDatabase(false),
	  m_ipFBMgr(ipFBMgr)
{
	try
	{
		//{{AFX_DATA_INIT(CPackageDlg)
		m_bClear = FALSE;
		m_zSize = _T("");
		m_zFile = _T("");
		//}}AFX_DATA_INIT

		// Get Persistence Manager for dialog
		ma_pUserCfgMgr = auto_ptr<IConfigurationSettingsPersistenceMgr>(
			new RegistryPersistenceMgr( HKEY_CURRENT_USER, gstrAF_UTILS_KEY ) );
		ASSERT_RESOURCE_ALLOCATION( "ELI09158", ma_pUserCfgMgr.get() != NULL );
		
		ma_pCfgFeedbackMgr = auto_ptr<PersistenceMgr>(new PersistenceMgr( 
			ma_pUserCfgMgr.get(), "\\FeedbackManager" ) );
		ASSERT_RESOURCE_ALLOCATION( "ELI09159", ma_pCfgFeedbackMgr.get() != NULL );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08474")
}
//-------------------------------------------------------------------------------------------------
void CPackageDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CPackageDlg)
	DDX_Check(pDX, IDC_CHECK_CLEAR, m_bClear);
	DDX_Text(pDX, IDC_STATIC_SIZE, m_zSize);
	DDX_Text(pDX, IDC_EDIT_FILE, m_zFile);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CPackageDlg, CDialog)
	//{{AFX_MSG_MAP(CPackageDlg)
	ON_BN_CLICKED(IDC_BTN_BROWSEFOLDER, OnBtnBrowse)
	ON_BN_CLICKED(ID_BTN_READINDEX, OnBtnReadindex)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CPackageDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL CPackageDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		CDialog::OnInitDialog();

		// Read stored settings
		readRegistrySettings();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18601");

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//-------------------------------------------------------------------------------------------------
void CPackageDlg::OnBtnReadindex() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		if (!m_bReadDatabase)
		{
			// Read Feedback database and collect file list
			readDatabase();

			// Close the database connection
			m_ipFBMgr->CloseConnection();

			// Compute and display the estimated package size
			estimatePackageSize();

			// Set flag
			m_bReadDatabase = true;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09099")
}
//-------------------------------------------------------------------------------------------------
void CPackageDlg::OnBtnBrowse() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		UpdateData( TRUE );

		/////////////////////////////
		// Prepare a File Open dialog
		/////////////////////////////

		// Create the filters string with all the supported extensions
		char pszStrFilter[] = 
			"Zip Files (*.zip)|*.zip|"
			"All Files (*.*)|*.*||";

		// Show the file dialog to select the feedback package file
		CFileDialog fileDlg( TRUE, NULL, m_zFile, 
			OFN_READONLY | OFN_HIDEREADONLY, 
			pszStrFilter, this );

		// Pass the pointer of dialog to create ThreadFileDlg object
		ThreadFileDlg tfd(&fileDlg);

		if (tfd.doModal() == IDOK)
		{
			// Get chosen directory
			string strDirectory = getDirectoryFromFullPath( 
				fileDlg.GetPathName().operator LPCTSTR() );

			// Get Feedback folder
			string	strFeedbackFolder = ma_pCfgFeedbackMgr->getFeedbackFolder();

			// Directories must be different
			if (strDirectory.compare( strFeedbackFolder ) == 0)
			{
				MessageBox( "The Feedback Results file cannot be in the Feedback folder!", 
					"Error", MB_YESNO | MB_ICONEXCLAMATION );
			}
			else
			{
				// Save the filename
				m_zFile = fileDlg.GetPathName();
				UpdateData( FALSE );
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07994")
}
//-------------------------------------------------------------------------------------------------
void CPackageDlg::OnOK() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		UpdateData( TRUE );

		// Check file definition
		if (m_zFile.IsEmpty())
		{
			MessageBox( "A Feedback package file must be defined", "Error", 
				MB_ICONEXCLAMATION | MB_OK );

			return;
		}
		else
		{
			// Read Index file, if not already read
			if (!m_bReadDatabase)
			{
				// Read Feedback database and collect file list
				readDatabase();

				// Close the database connection
				m_ipFBMgr->CloseConnection();
			}

			// Warn the user that Feedback data will be deleted
			if ((m_vecFBFiles.size() > 0) && (m_bClear == TRUE))
			{
				CString zPrompt;
				zPrompt.Format( "After Packaging, delete all files in the Feedback folder \"%s\"?", 
					ma_pCfgFeedbackMgr->getFeedbackFolder().c_str() );
				int iResult = MessageBox( zPrompt, "Warning", MB_YESNOCANCEL | MB_ICONQUESTION );
				if (iResult != IDCANCEL)
				{
					// Zip the collected files
					zipCollectedFiles();

					///////////////////////
					// Clear Feedback files
					///////////////////////
					if ((iResult == IDYES) && (m_ipFBMgr != NULL))
					{
						// Call method on interface
						m_ipFBMgr->ClearFeedbackData( VARIANT_FALSE );
					}
				}
			}
			// Just collect the files
			else
			{
				// Zip the collected files
				zipCollectedFiles();
			}
		}
		
		// Save results to registry
		writeRegistrySettings();

		CDialog::OnOK();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07995")
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CPackageDlg::estimatePackageSize() 
{
	// Set wait cursor
	CWaitCursor	wait;

	long lTotalZippedSizeKB = 0;

	// Walk through file list
	vector<std::string>::iterator iter;
	for (iter = m_vecFBFiles.begin(); iter != m_vecFBFiles.end(); iter++)
	{
		// Get file name
		string strFile = *iter;

		// Get file size and convert to long integer in KB
		ULONGLONG ullFileSize = 0;
		try
		{
			ullFileSize = getSizeOfFile( strFile );
		}
		catch (...)
		{
			// Ignore an exception here, do not increase estimated package size
		}
		long lSizeKB = (long)(ullFileSize / 1000);

		// Get file extension
		string strExt = getExtensionFromFullPath( strFile, true );

		// Check for image file
		if (isImageFileExtension( strExt ))
		{
			// Assume 20% reduction in size and update total size
			long lZippedSizeKB = (long)((double) lSizeKB * 0.80);
			lTotalZippedSizeKB += lZippedSizeKB;
		}
		// Check for VOA file
		else if (strExt == ".voa")
		{
			// Assume 89% reduction in size and update total size
			long lZippedSizeKB = (long)((double) lSizeKB * 0.11);
			lTotalZippedSizeKB += lZippedSizeKB;
		}
		// Check for RSD file
		else if (strExt == ".rsd")
		{
			// Assume 74% reduction in size and update total size
			long lZippedSizeKB = (long)((double) lSizeKB * 0.26);
			lTotalZippedSizeKB += lZippedSizeKB;
		}
		// Check for ETF file
		else if (strExt == ".etf")
		{
			// Assume 47% reduction in size and update total size
			long lZippedSizeKB = (long)((double) lSizeKB * 0.53);
			lTotalZippedSizeKB += lZippedSizeKB;
		}
		// Check for UEX file
		else if (strExt == ".uex")
		{
			// Assume 95% reduction in size and update total size
			long lZippedSizeKB = (long)((double) lSizeKB * 0.05);
			lTotalZippedSizeKB += lZippedSizeKB;
		}
		// Check for DAT file
		else if (strExt == ".dat")
		{
			// Assume 72% reduction in size and update total size
			long lZippedSizeKB = (long)((double) lSizeKB * 0.28);
			lTotalZippedSizeKB += lZippedSizeKB;
		}
		// Check for MDB file
		else if (strExt == ".mdb")
		{
			// Assume 95% reduction in size and update total size
			long lZippedSizeKB = (long)((double) lSizeKB * 0.05);
			lTotalZippedSizeKB += lZippedSizeKB;
		}
		// Other file type, assume no reduction in size
		//    includes: USS files
		else
		{
			// Add file size to total size
			lTotalZippedSizeKB += lSizeKB;
		}
	}

	// Clear size if no files found
	if (lTotalZippedSizeKB == 0)
	{
		m_zSize = "";
	}
	else
	{
		// Round size up to next MB
		m_zSize.Format( "%ld MB", (lTotalZippedSizeKB / 1000) + 1 );
	}

	// Refresh display
	UpdateData( FALSE );
}
//-------------------------------------------------------------------------------------------------
void CPackageDlg::readDatabase()
{
	// Set wait cursor
	CWaitCursor	wait;

	// Clear the file list
	m_vecFBFiles.clear();

	// Create fully qualified path to Index file
	string	strFeedbackFolder = ma_pCfgFeedbackMgr->getFeedbackFolder();
	string	strDir = getCurrentProcessEXEFullPath();
	strDir = getModuleDirectory( strDir );
	string	strFileName = strDir + "\\" + gstrINDEXFILE;

	// Check existence of database
	if (!isFileOrFolderValid( strFileName ))
	{
		// Throw exception
		UCLIDException ue( "ELI09097", "Feedback database not found!" );
		ue.addDebugInfo( "Filename", strFileName );
		throw ue;
	}
	else
	{
		/////////////////////////////////////////////////
		// Construct RunRules.DAT file in Feedback folder
		/////////////////////////////////////////////////
		string	strRunRules = strFeedbackFolder + "\\" + gstrRUNRULESFILE;

		// Create the file, if not already present
		if (!isFileOrFolderValid( strRunRules ))
		{
			HANDLE hFile = ::CreateFile( strRunRules.c_str(), GENERIC_WRITE, 0, NULL, 
				CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL );

			if (hFile == INVALID_HANDLE_VALUE)
			{
				// Throw exception
				UCLIDException ue( "ELI09884", "Unable to create RunRules file!" );
				ue.addWin32ErrorInfo();
				throw ue;
			}

			CloseHandle( hFile );
			waitForFileToBeReadable(strRunRules);
		}

		// Open the file for writing
		CStdioFile	fileRunRules;
		if (!fileRunRules.Open( strRunRules.c_str(), 
			CFile::modeCreate | CFile::modeWrite | CFile::shareDenyWrite ))
		{
			// Check for file existence
			// Unable to open existing file implies that another process already has the file open
			if (isFileOrFolderValid( strRunRules ))
			{
				// Just return
				return;
			}
			// File does not exist and it should
			else
			{
				// Throw exception
				UCLIDException ue( "ELI09885", "Unable to open Run Rules file!" );
				ue.addDebugInfo( "Path", strRunRules );
				throw ue;
			}
		}

		////////////////////////////////
		// Process the Feedback database
		////////////////////////////////

		// Create local map for RSD filenames
		std::map<std::string, std::string>	mapRSDFiles;

		// Get Recordset
		_RecordsetPtr	ipRS = m_ipFBMgr->GetFeedbackRecords();
//		ASSERT_RESOURCE_ALLOCATION( "ELI10186", ipRS != NULL );
		if (ipRS == NULL)
		{
			// Display message to user and return
			MessageBox( "Unable to retrieve Feedback records!  Either database is locked by another application or no Feedback information is available for packaging.", 
				"Status", MB_ICONEXCLAMATION | MB_OK );
			return;
		}

		// Process each record
		while (ipRS->adoEOF == VARIANT_FALSE)
		{
			// Get Rule ID
			long lRuleID = 0;
			_variant_t	vtValue;
			vtValue = ipRS->GetCollect( gstrRuleIDField );
			if (vtValue.vt == VT_I4)
			{
				lRuleID = vtValue.lVal;
			}

			// Get Source Doc
			string	strSource;
			vtValue = ipRS->GetCollect( gstrSourceDocField );
			if (vtValue.vt == VT_BSTR)
			{
				strSource = asString(_bstr_t( vtValue.bstrVal ));
			}

			// Get Package Source Doc
			bool	bPackageSource = true;
			vtValue = ipRS->GetCollect( gstrPackageSrcDocField );
			if (vtValue.bVal == 0)
			{
				bPackageSource = false;
			}

			// Get RSD File
			string	strRSD;
			vtValue = ipRS->GetCollect( gstrRSDFileField );
			if (vtValue.vt == VT_BSTR)
			{
				strRSD = asString( vtValue.bstrVal );
			}

			/////////////////
			// Check RSD file
			/////////////////
			string	strLocalRSD;
			if ((strRSD.length() > 0) && (isFileOrFolderValid( strRSD )))
			{
				// Search for the RSD file in the map
				bool	bFoundRSD = false;
				std::map<std::string, std::string>::iterator it = mapRSDFiles.find( strRSD );
				if (it != mapRSDFiles.end())
				{
					// RSD file already contained in map
					strLocalRSD = it->second.c_str();
				}
				else
				{
					// Construct local name for new RSD file
					string strExt = getExtensionFromFullPath( strRSD );

					CString	zTemp;
					zTemp.Format( "RuleSet%d%s", mapRSDFiles.size() + 1, strExt.c_str() );
					strLocalRSD = zTemp.operator LPCTSTR();
					CString zLocalFullPath;
					zLocalFullPath.Format( "%s\\%s", strFeedbackFolder.c_str(), 
						strLocalRSD.c_str() );

					// Copy RSD file to Feedback folder
					copyFile(strRSD, (LPCTSTR) zLocalFullPath, true);

					// Add this file to the collection
					m_vecFBFiles.push_back( zLocalFullPath.operator LPCTSTR() );

					// Add new map entry
					mapRSDFiles[strRSD] = strLocalRSD;
				}
			}

			//////////////////////////////////////////
			// Check source document and add to vector
			//////////////////////////////////////////

			CString	zSourceName;
			CString	zSourceFullPath;
			bool	bAddToVector = false;
			if (bPackageSource)
			{
				// Get expected file extension
				string strExt = getExtensionFromFullPath( strSource );

				// Different file extension if converted to text
				if (ma_pCfgFeedbackMgr->getDocumentConversion())
				{
					string strName = getFileNameFromFullPath( strSource );
					long lExtPos = strName.find( '.' );
					if (lExtPos != string::npos)
					{
						// Extract complete extension, typically ".tif.uss"
						strExt = strName.substr( lExtPos, 
							strName.length() - lExtPos );
					}
				}

				// Construct Source file name
				zSourceName.Format( "%08d%s", lRuleID, strExt.c_str() );
				zSourceFullPath.Format( "%s\\%s", strFeedbackFolder.c_str(), zSourceName );

				// Does source file still need collecting?
				bAddToVector = true;
				if (!isFileOrFolderValid( zSourceFullPath.operator LPCTSTR() ))
				{
					// Search for original source document
					if ((strSource.length() > 0) && (isFileOrFolderValid( strSource )))
					{
						try
						{
							copyFile(strSource, (LPCTSTR) zSourceFullPath, true);
						}
						catch(...)
						{
							// Failed to copy source file, do not add to vector
							bAddToVector = false;
						}
					}		// end if source file found
					else
					{
						// Source file is unavailable, do not add to vector
						bAddToVector = false;
					}		// end else source file not found
				}			// end if source file not already in Feedback folder

				// Add the Source file to the collection
				if (bAddToVector)
				{
					m_vecFBFiles.push_back( zSourceFullPath.operator LPCTSTR() );
				}
			}

			/////////////////////////////////////////////////////
			// Search for files associated with Rule Execution ID
			//    ID.found.voa
			//    ID.correct.voa
			//    ID.uex
			/////////////////////////////////////////////////////

			// Construct Found file name
			CString	zFoundName;
			zFoundName.Format( "%08d.found.voa", lRuleID );
			CString	zFoundFullPath;
			zFoundFullPath.Format( "%s\\%s", strFeedbackFolder.c_str(), zFoundName );

			// Construct Correct file name
			CString	zCorrectName;
			zCorrectName.Format( "%08d.correct.voa", lRuleID );
			CString	zCorrectFullPath;
			zCorrectFullPath.Format( "%s\\%s", strFeedbackFolder.c_str(), zCorrectName );

			// Construct UEX file name
			CString	zUEXName;
			zUEXName.Format( "%08d.uex", lRuleID );
			CString	zUEXFullPath;
			zUEXFullPath.Format( "%s\\%s", strFeedbackFolder.c_str(), zUEXName );

			// Look for UEX, Found and Correct files
			bool	bFound = isFileOrFolderValid( zFoundFullPath.operator LPCTSTR() );
			bool	bCorrect = isFileOrFolderValid( zCorrectFullPath.operator LPCTSTR() );
			bool	bUEX = isFileOrFolderValid( zUEXFullPath.operator LPCTSTR() );

			// Add found files to vector
			if (bFound)
			{
				m_vecFBFiles.push_back( zFoundFullPath.operator LPCTSTR() );
			}

			if (bCorrect)
			{
				m_vecFBFiles.push_back( zCorrectFullPath.operator LPCTSTR() );
			}

			if (bUEX)
			{
				m_vecFBFiles.push_back( zUEXFullPath.operator LPCTSTR() );
			}

			// Add entry to Run Rules DAT file
			if (bFound && bAddToVector)
			{
				// <TESTCASE> element
				string strRunRuleEntry( "<TESTCASE>;" );

				// RSD file element
				strRunRuleEntry += strLocalRSD.c_str();
				strRunRuleEntry += ";";

				// USS file element
				strRunRuleEntry += zSourceName.operator LPCTSTR();
				strRunRuleEntry += ";";

				// VOA file element
				strRunRuleEntry += zFoundName.operator LPCTSTR();
				strRunRuleEntry += "\n";

				// Write to DAT file
				fileRunRules.WriteString( strRunRuleEntry.c_str() );
				fileRunRules.Flush();
			}

			// Move to the next record
			ipRS->MoveNext();
		}			// end for each record in Recordset

		// Add the Run Rules DAT file to final vector
		fileRunRules.Close();
		waitForFileToBeReadable(strRunRules);
		m_vecFBFiles.push_back( strRunRules );

		// Add the Database file to final vector
		m_vecFBFiles.push_back( strFileName );

	}				// end else Index file exists
}
//-------------------------------------------------------------------------------------------------
void CPackageDlg::readRegistrySettings() 
{
	// Get Clear Data setting
	m_bClear = ma_pCfgFeedbackMgr->getClearAfterPackage();

	// Get package file
	m_zFile = (ma_pCfgFeedbackMgr->getPackageFile()).c_str();

	// Refresh display
	UpdateData( FALSE );
}
//-------------------------------------------------------------------------------------------------
void CPackageDlg::writeRegistrySettings() 
{
	UpdateData( TRUE );

	// Store Clear Data setting
	ma_pCfgFeedbackMgr->setClearAfterPackage( (m_bClear == TRUE) ? true : false );

	// Store package file
	ma_pCfgFeedbackMgr->setPackageFile( m_zFile.operator LPCTSTR() );
}
//-------------------------------------------------------------------------------------------------
void CPackageDlg::zipCollectedFiles()
{
	// Set wait cursor
	CWaitCursor	wait;

	// Create Zipper object
	CZipper	zip( m_zFile.operator LPCTSTR() );

	// Add each collected file to Zip
	vector<std::string>::iterator iter;
	for (iter = m_vecFBFiles.begin(); iter != m_vecFBFiles.end(); iter++)
	{
		// Ignore Path information
		zip.AddFileToZip( (*iter).c_str(), TRUE );
	}

	// Close the Zip file
	zip.CloseZip();
}
//-------------------------------------------------------------------------------------------------
