// ConvertFPSFileDlg.cpp : implementation file
//

#include "stdafx.h"
#include "ConvertFPSFile.h"
#include "ConvertFPSFileDlg.h"
#include "..\..\..\Code\HelperFunctions.h"

#include <UCLIDException.h>
#include <ComUtils.h>
#include <cpputil.h>
#include <ByteStream.h>
#include <ByteStreamManipulator.h>
#include <LoadFileDlgThread.h>

using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const std::string gstrSTREAM_NAME = "FileProcessingManager";
const int gnMAX_CONVERT_VERSION = 10;

//-------------------------------------------------------------------------------------------------
// CConvertFPSFileDlg dialog
//-------------------------------------------------------------------------------------------------
CConvertFPSFileDlg::CConvertFPSFileDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CConvertFPSFileDlg::IDD, pParent)
{
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}
//-------------------------------------------------------------------------------------------------
void CConvertFPSFileDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Text(pDX, IDC_EDIT_INPUT, m_zInputFile);
	DDX_Text(pDX, IDC_EDIT_OUTPUT, m_zOutputFile);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CConvertFPSFileDlg, CDialog)
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	//}}AFX_MSG_MAP
	ON_BN_CLICKED(ID_BTN_INPUT, &CConvertFPSFileDlg::OnBnClickedBtnInput)
	ON_BN_CLICKED(ID_BTN_OUTPUT, &CConvertFPSFileDlg::OnBnClickedBtnOutput)
	ON_BN_CLICKED(IDOK, &CConvertFPSFileDlg::OnBnClickedOk)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CConvertFPSFileDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL CConvertFPSFileDlg::OnInitDialog()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		CDialog::OnInitDialog();

		// Set the icon for this dialog.  The framework does this automatically
		//  when the application's main window is not a dialog
		SetIcon(m_hIcon, TRUE);			// Set big icon
		SetIcon(m_hIcon, FALSE);		// Set small icon
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18620");

	return TRUE;  // return TRUE  unless you set the focus to a control
}
//-------------------------------------------------------------------------------------------------
// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.
void CConvertFPSFileDlg::OnPaint()
{
	if (IsIconic())
	{
		CPaintDC dc(this); // device context for painting

		SendMessage(WM_ICONERASEBKGND, reinterpret_cast<WPARAM>(dc.GetSafeHdc()), 0);

		// Center icon in client rectangle
		int cxIcon = GetSystemMetrics(SM_CXICON);
		int cyIcon = GetSystemMetrics(SM_CYICON);
		CRect rect;
		GetClientRect(&rect);
		int x = (rect.Width() - cxIcon + 1) / 2;
		int y = (rect.Height() - cyIcon + 1) / 2;

		// Draw the icon
		dc.DrawIcon(x, y, m_hIcon);
	}
	else
	{
		CDialog::OnPaint();
	}
}
//-------------------------------------------------------------------------------------------------
// The system calls this function to obtain the cursor to display while the user drags
//  the minimized window.
HCURSOR CConvertFPSFileDlg::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}
//-------------------------------------------------------------------------------------------------
void CConvertFPSFileDlg::OnBnClickedBtnInput()
{
	try
	{
		// Define supported file extensions
		const static string s_strAllFiles = "FPS Files (*.fps)|*.fps"
												"|All Files (*.*)|*.*||";

		// Create File Open dialog
		string strFileExtension( s_strAllFiles );
		CFileDialog fileDlg(TRUE, ".fps", NULL, OFN_ENABLESIZING | 
			OFN_EXPLORER | OFN_PATHMUSTEXIST,
			strFileExtension.c_str(), this);

		// Pass the pointer of dialog to create ThreadDataStruct object
		ThreadFileDlg tfd(&fileDlg);

		// If the OK button is clicked
		if (tfd.doModal() == IDOK)
		{
			// Update edit box
			m_zInputFile = fileDlg.GetPathName();
			UpdateData( FALSE );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14160");
}
//-------------------------------------------------------------------------------------------------
void CConvertFPSFileDlg::OnBnClickedBtnOutput()
{
	try
	{
		// Define supported file extensions
		const static string s_strAllFiles = "FPS Files (*.fps)|*.fps"
												"|All Files (*.*)|*.*||";

		// Create File Save dialog
		string strFileExtension( s_strAllFiles );	
		CFileDialog fileDlg(FALSE, ".fps", NULL, OFN_ENABLESIZING | 
			OFN_EXPLORER | OFN_PATHMUSTEXIST,
			strFileExtension.c_str(), NULL);

		// Pass the pointer of dialog to create ThreadDataStruct object
		ThreadFileDlg tfd(&fileDlg);
	
		// If the OK button is clicked
		if (tfd.doModal() == IDOK)
		{
			// Update edit box
			m_zOutputFile = fileDlg.GetPathName();
			UpdateData( FALSE );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14161");
}
//-------------------------------------------------------------------------------------------------
void CConvertFPSFileDlg::OnBnClickedOk()
{
	try
	{
		UpdateData( TRUE );

		// Check that FPS files are defined
		if (m_zInputFile.IsEmpty())
		{
			UCLIDException ue( "ELI14156", "You must define an FPS file to be converted." );
			throw ue;
		}

		if (m_zOutputFile.IsEmpty())
		{
			UCLIDException ue( "ELI14157", "You must define a destination FPS file." );
			throw ue;
		}

		// Check to see if original FPS file is to be overwritten
		CString zText;
		bool bConfirm = false;
		if (m_zInputFile.Compare( m_zOutputFile ) == 0)
		{
			zText.Format( "Are you sure you want to overwrite the original file '%s'?", m_zInputFile );
			bConfirm = true;
		}
		// Check to see if output FPS file already exists (P13 #4119)
		else if (isValidFile( (LPCTSTR) m_zOutputFile ))
		{
			zText.Format( "File \"%s\" already exists.  Overwrite the file?", m_zOutputFile );
			bConfirm = true;
		}

		// Show confirmation message box, if needed
		if (bConfirm)
		{
			int iResult = MessageBox( zText, "Confirm Overwrite", MB_YESNO | MB_ICONQUESTION );
			if (iResult == IDNO)
			{
				// Select entire text in output file edit box
				CEdit	*pEdit = (CEdit *)GetDlgItem( IDC_EDIT_OUTPUT );
				if (pEdit != NULL)
				{
					pEdit->SetSel( 0, -1 );
					pEdit->SetFocus();
				}

				// Do not overwrite, so do not convert
				return;
			}
		}

		// Convert the FPS file
		// Failure will throw an exception and skip the Message Box
		convertFPSFile( m_zInputFile.operator LPCTSTR(), m_zOutputFile.operator LPCTSTR() );

		// Provide status message
		zText.Format( "The file was successfully converted." );
		MessageBox( zText, "Success", MB_OK | MB_ICONEXCLAMATION );

		// Do not call base class - must click Cancel or Close button to exit
//		OnOK();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14162");
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CConvertFPSFileDlg::convertFPSFile(std::string strInput, std::string strOutput)
{
	// Wait cursor in case conversion takes a long time
	CWaitCursor	wait;

	// Scope for COM pointers so that release is called on the storage stream
	{
		///////////////////////
		// Declare NULL objects that may be used in FPS file conversion
		///////////////////////
		// File Suppliers
		IFolderFSPtr			ipFolderFS;
		IStaticFileListFSPtr	ipStaticListFS;
		IIUnknownVectorPtr		ipFileSupplier;

		// FAM Conditions
		IFileExistenceFAMConditionPtr		ipSkipFile;
		IFileNamePatternFAMConditionPtr		ipSkipPattern;
		IFAMConditionPtr			ipSkipOR;
		IObjectWithDescriptionPtr	ipSkipOWD;

		// Flags
		bool bUseStreamedFileSuppliers = false;
		bool bUseStreamedSkipCondition = false;
		bool bSupplyingRoleEnabled = true;
		bool bProcessingRoleEnabled = true;
		bool bDisplayOfStatisticsEnabled = false;

		// Other
		string strActionName = gstrCONVERTED_FPS_ACTION_NAME.c_str();
		long nNumThreads = 0;		// default to max available threads
		IIUnknownVectorPtr ipFileProcessors;

		///////////////////////
		// Get stream for input FPS file
		///////////////////////
		// Check file existence
		validateFileOrFolderExistence( strInput );

		// Open stream to read input FPS file
		IStoragePtr ipStorage;
		readStorageFromFile(&ipStorage, get_bstr_t(strInput));

		// Open the FPM stream from the file object
		IStreamPtr ipStream;
		_bstr_t _bstrStreamName = get_bstr_t(gstrSTREAM_NAME);
		readStreamFromStorage(&ipStream, ipStorage, _bstrStreamName);

		///////////////////////
		// Interpret stream elements
		///////////////////////
		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		ipStream->Read(&nDataLength, sizeof(nDataLength), NULL);
		ByteStream data(nDataLength);
		ipStream->Read(data.getData(), nDataLength, NULL);
		ByteStreamManipulator dataReader(ByteStreamManipulator::kRead, data);

		// Read the individual data items from the bytestream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;

		// Check for newer version
		if (nDataVersion > gnMAX_CONVERT_VERSION)
		{
			// Throw exception
			UCLIDException ue( "ELI14169",
				"FPS file cannot be converted because it is not in an old format." );
			ue.addDebugInfo( "Input file", strInput );
			ue.addDebugInfo( "Input version", nDataVersion );
			ue.addDebugInfo( "Newest version for conversion", gnMAX_CONVERT_VERSION );
			throw ue;
		}

		/////////////////////////////////
		// Handle newer versions 9 and 10
		/////////////////////////////////
		if (nDataVersion == 10)
		{
			// Read the settings in Action tab
			// Read the action name
			dataReader >> strActionName;

			// Read supplier check box status
			dataReader >> bSupplyingRoleEnabled;

			// Read processor check box status
			dataReader >> bProcessingRoleEnabled;
			
			// Read statistics check box status
			dataReader >> bDisplayOfStatisticsEnabled;
		}

		if (nDataVersion >= 9)
		{
			// Number of threads for processing
			dataReader >> nNumThreads;

			// Read in the collected File Suppliers
			IPersistStreamPtr ipFSObj;
			readObjectFromStream( ipFSObj, ipStream, "ELI14407" );
			ipFileSupplier = ipFSObj;
			ASSERT_RESOURCE_ALLOCATION( "ELI14408", ipFileSupplier != NULL );
			bUseStreamedFileSuppliers = true;

			// Read in the Skip Condition with its Description
			IPersistStreamPtr ipSkipObj;
			readObjectFromStream( ipSkipObj, ipStream, "ELI14409" );
			ipSkipOWD = ipSkipObj;
			ASSERT_RESOURCE_ALLOCATION( "ELI14410", ipSkipOWD != NULL );
			bUseStreamedSkipCondition = true;

			// Read in the file processors
			IPersistStreamPtr ipObj;
			readObjectFromStream( ipObj, ipStream, "ELI14411" );
			ipFileProcessors = ipObj;
			ASSERT_RESOURCE_ALLOCATION( "ELI14412", ipFileProcessors != NULL );
		}

		////////////////////////////
		// Handle older versions < 9
		////////////////////////////
		if (nDataVersion < 9)
		{
			// Read scope information
			long nTmp;
			dataReader >> nTmp;

			EScopeType eScopeType;
			eScopeType = (EScopeType) nTmp;

			// Should settings be mapped to Folder File Supplier?
			if (eScopeType == kFolderScope)
			{
				// Read folder name
				string strFolderName;
				dataReader >> strFolderName;

				// Read in the file extension list
				dataReader >> nTmp;
				string	strFileExtensions;
				int i;
				for (i = 0; i < nTmp; i++)
				{
					// Read this file extension
					string strTmp;
					dataReader >> strTmp;

					// Append to group with ';' separator
					if (!strFileExtensions.empty())
					{
						strFileExtensions += ";";
					}
					strFileExtensions += strTmp.c_str();
				}

				// Protect against lack of file extensions
				if (nTmp == 0)
				{
					// Use "*.*" as default for all files
					strFileExtensions = "*.*";
				}

				// Read whether or not to recursively process sub folders
				bool bRecursion = false;
				dataReader >> bRecursion;

				// Read whether or not to listen to the folders for file additions
				bool bProcessNewFiles = false;
				dataReader >> bProcessNewFiles;

				/////////////////
				// Apply settings to Folder FS
				/////////////////
				ipFolderFS.CreateInstance( CLSID_FolderFS );
				ASSERT_RESOURCE_ALLOCATION("ELI14166", ipFolderFS != NULL);

				// Set folder name
				ipFolderFS->FolderName = get_bstr_t( strFolderName );

				// Set file extensions
				ipFolderFS->FileExtensions = get_bstr_t( strFileExtensions );

				// Apply recursion
				ipFolderFS->RecurseFolders = bRecursion ? VARIANT_TRUE : VARIANT_FALSE;

				// Apply listening
				// - Old-style listening implies AddedFiles and RenamedFiles, NOT ModifiedFiles
				ipFolderFS->AddedFiles = bProcessNewFiles ? VARIANT_TRUE : VARIANT_FALSE;
				ipFolderFS->TargetOfMoveOrRename = bProcessNewFiles ? VARIANT_TRUE : VARIANT_FALSE;
				ipFolderFS->ModifiedFiles = VARIANT_FALSE;
			}
			// Should settings be mapped to Static File List File Supplier?
			else if (eScopeType == kIndividualFilesScope)
			{
				// Read in the number of files
				dataReader >> nTmp;

				// Create a VariantVector to hold the filenames
				IVariantVectorPtr ipFileList( CLSID_VariantVector );
				ASSERT_RESOURCE_ALLOCATION("ELI14207", ipFileList != NULL);

				// Add each file to a VariantVector
				int i;
				for(i = 0; i < nTmp; i++)
				{
					// Read this filename
					string strTmp;
					dataReader >> strTmp;

					// Add this filename to the VariantVector
					ipFileList->PushBack( strTmp.c_str() );
				}

				/////////////////
				// Apply settings to Static File List FS
				/////////////////
				ipStaticListFS.CreateInstance( CLSID_StaticFileListFS );
				ASSERT_RESOURCE_ALLOCATION("ELI14206", ipStaticListFS != NULL);

				// Set the file list
				ipStaticListFS->FileList = ipFileList;
			}
			else if ((eScopeType == kNoScope) && (nDataVersion <= 7))
			{
				// Scope is expected to be defined
				THROW_LOGIC_ERROR_EXCEPTION("ELI14274");
			}
			// else kNoScope && (nDataVersion > 7), no error

			// Read whether or not to skip already processed files
			bool bSkipProcessedFile;
			dataReader >> bSkipProcessedFile;
			if (bSkipProcessedFile)
			{
				// Read filename of file whose existence implies input file should be skipped
				string strSkipFile;
				dataReader >> strSkipFile;

				/////////////////
				// Apply setting to File Existence Skip Condition
				/////////////////
				ipSkipFile.CreateInstance( CLSID_FileExistence );
				ASSERT_RESOURCE_ALLOCATION("ELI14208", ipSkipFile != NULL);

				// Set the filename (tag may be included)
				ipSkipFile->FileString = get_bstr_t( strSkipFile );

				// Set File Existence flag
				ipSkipFile->FileExists = VARIANT_TRUE;
			}

			// Read and ignore m_nMaxAttempts
			dataReader >> nTmp;

			// Read filtering type
			dataReader >> nTmp;
			EFilterPatternType eFilterPatternType;
			eFilterPatternType = (EFilterPatternType)nTmp;

			// Should settings be mapped to FileNamePattern Skip Condition?
			if (eFilterPatternType == kFilterWithRegExp)
			{
				// Read the filter pattern
				string strFilterPattern;
				dataReader >> strFilterPattern;

				// Read case-sensitivity flag
				bool bFilterCaseSensitive = false;
				dataReader >> bFilterCaseSensitive;

				// Read the filter type
				dataReader >> nTmp;
				EFilterType eFilterType;
				eFilterType = (EFilterType)nTmp;

				/////////////////
				// Apply settings to File Name Pattern Skip Condition
				/////////////////
				ipSkipPattern.CreateInstance( CLSID_FileNamePattern );
				ASSERT_RESOURCE_ALLOCATION("ELI14209", ipSkipPattern != NULL);

				// File to be tested against pattern is always the Source Document
				// Always testing for Contains - either Does or Does Not
				// Never testing for (Does or Does Not) Exactly Match
				ipSkipPattern->FileString = get_bstr_t( "<SourceDocName>" );
				ipSkipPattern->ContainMatch = VARIANT_TRUE;

				// (P13 #4253) Note that eFilterType == kContain implies 
				// "Process this source document if filename Does Contain the expression"
				// This is equivalent to Skip Processing this source document if 
				// "filename Does Not Contain the expression"
				ipSkipPattern->DoesContainOrMatch = 
					(eFilterType == kContain) ? VARIANT_FALSE : VARIANT_TRUE;

				// Set the pattern
				ipSkipPattern->RegPattern = get_bstr_t( strFilterPattern );

				// Set case sensitivity
				ipSkipPattern->IsCaseSensitive =
					bFilterCaseSensitive ? VARIANT_TRUE : VARIANT_FALSE;
			}
			// else (eFilterPatternType == kNoFilter) and nothing needs to happen

			// Should existing files be ignored?
			if ( nDataVersion >= 5 )
			{
				bool bProcessOnlyNewFiles = false;
				dataReader >> bProcessOnlyNewFiles;

				// Apply this setting to Folder File Supplier
				if (ipFolderFS != NULL)
				{
					ipFolderFS->NoExistingFiles =
						bProcessOnlyNewFiles ? VARIANT_TRUE : VARIANT_FALSE;
				}
			}

			// Read and ignore NumStoredRecords
			if (nDataVersion == 3)
			{
				bool bTmp;
				dataReader >> bTmp;

				if (bTmp)
				{
					long nTmp;
					dataReader >> nTmp;
				}
			}

			// Read number of threads for processing
			if (nDataVersion >= 6)
			{
				dataReader >> nNumThreads;
			}

			// Read collection of File Suppliers
			if (nDataVersion >= 8)
			{
				// Read in the collected File Suppliers
				IPersistStreamPtr ipFSObj;
				readObjectFromStream( ipFSObj, ipStream, "ELI14282" );
				ipFileSupplier = ipFSObj;
				ASSERT_RESOURCE_ALLOCATION( "ELI14283", ipFileSupplier != NULL );

				// Set flag to use these File Suppliers instead of those built
				// from Folder or File Scope information
				bUseStreamedFileSuppliers = true;
			}

			// Read Skip Condition
			if (nDataVersion >= 7)
			{
				// Read in the Skip Condition with its Description
				IPersistStreamPtr ipSkipObj;
				readObjectFromStream( ipSkipObj, ipStream, "ELI14284" );
				ipSkipOWD = ipSkipObj;
				ASSERT_RESOURCE_ALLOCATION( "ELI14285", ipSkipOWD != NULL );

				// Set flag to use these File Suppliers instead of those built
				// from Folder or File Scope information
				bUseStreamedSkipCondition = true;
			}

			// Read in the file processors
			IPersistStreamPtr ipObj;
			readObjectFromStream( ipObj, ipStream, "ELI14214" );
			ipFileProcessors = ipObj;
		}

		/////////////////
		// Create File Processing Manager object to save settings to current format
		/////////////////
		IFileProcessingManagerPtr	ipFPMgr( CLSID_FileProcessingManager );
		ASSERT_RESOURCE_ALLOCATION("ELI14168", ipFPMgr != NULL);

		// Check to see if File Supplier creation is needed
		if (!bUseStreamedFileSuppliers)
		{
			// Create File Supplier Data object
			// - Not Force Processing, Inactive
			IFileSupplierDataPtr ipFSD( CLSID_FileSupplierData );
			ASSERT_RESOURCE_ALLOCATION("ELI14250", ipFSD != NULL);
			ipFSD->ForceProcessing = VARIANT_FALSE;
			ipFSD->FileSupplierStatus = kInactiveStatus;

			// Create ObjectWithDescription to be associated with File Supplier Data
			// - Enabled
			IObjectWithDescriptionPtr ipOWD( CLSID_ObjectWithDescription );
			ASSERT_RESOURCE_ALLOCATION("ELI14251", ipOWD != NULL);
			ipOWD->Enabled = VARIANT_TRUE;

			// Associate proper File Supplier to Object With Description
			// - either FolderFS or StaticFileListFS
			if (ipFolderFS != NULL)
			{
				// Set File Supplier object in Object With Description, push into vector
				ipOWD->Object = ipFolderFS;

				// Set Description to description from Categorized Component
				// and surround text with <>
				ipOWD->Description = get_bstr_t( makeDescription( ipFolderFS ) );
			}
			else if (ipStaticListFS != NULL)
			{
				ipOWD->Object = ipStaticListFS;

				// Set Description to description from Categorized Component
				// and surround text with <>
				ipOWD->Description = get_bstr_t( makeDescription( ipStaticListFS ) );
			}

			// Push Object With Description into File Supplier Data
			ipFSD->FileSupplier = ipOWD;

			// Build collection of one File Supplier Data object
			ipFileSupplier.CreateInstance( CLSID_IUnknownVector );
			ASSERT_RESOURCE_ALLOCATION("ELI14215", ipFileSupplier != NULL);
			ipFileSupplier->PushBack( ipFSD );
		}

		// Provide File Supplier Data collection to File Supplier Mgr
		// - via Stream if bUseStreamedFileSuppliers == true
		// OR
		// - via constructed components and Folder Scope or File Scope settings
		IFileSupplyingMgmtRolePtr	ipFSMgmt = ipFPMgr->FileSupplyingMgmtRole;
		ASSERT_RESOURCE_ALLOCATION("ELI14279", ipFSMgmt != NULL);
		ipFSMgmt->FileSuppliers = ipFileSupplier;

		/////////////////
		// Provide collected File Processors
		/////////////////
		IFileProcessingMgmtRolePtr	ipFPMgmt = ipFPMgr->FileProcessingMgmtRole;
		ASSERT_RESOURCE_ALLOCATION("ELI14349", ipFPMgmt != NULL);
		ipFPMgmt->FileProcessors = ipFileProcessors;

		/////////////////
		// Provide single Skip Condition as one of the following
		// - NULL, if neither File Existence or File Name Pattern items are defined
		// - ORSkipCondition, if both File Existence and File Name Pattern items are defined
		// - File Existence, if only File Existence is defined
		// - File Name Pattern, if only File Name Pattern is defined
		/////////////////

		if (!bUseStreamedSkipCondition)
		{
			// Create an Object With Description for the combined Skip Condition
			ipSkipOWD.CreateInstance( CLSID_ObjectWithDescription );
			ASSERT_RESOURCE_ALLOCATION("ELI14220", ipSkipOWD != NULL);
			if ((ipSkipFile != NULL) && (ipSkipPattern != NULL))
			{
				// Create the OR Skip Condition
				ipSkipOR.CreateInstance( CLSID_MultiFAMConditionOR );
				ASSERT_RESOURCE_ALLOCATION("ELI14217", ipSkipOR != NULL);

				// Create an ObjectWithDescription for first Skip Condition
				IObjectWithDescriptionPtr	ipOWD1( CLSID_ObjectWithDescription );
				ASSERT_RESOURCE_ALLOCATION("ELI14269", ipOWD1 != NULL);
				ipOWD1->Object = ipSkipFile;
				ipOWD1->Description = get_bstr_t( makeDescription( ipSkipFile ) );

				// Create an ObjectWithDescription for second Skip Condition
				IObjectWithDescriptionPtr	ipOWD2( CLSID_ObjectWithDescription );
				ASSERT_RESOURCE_ALLOCATION("ELI14270", ipOWD2 != NULL);
				ipOWD2->Object = ipSkipPattern;
				ipOWD2->Description = get_bstr_t( makeDescription( ipSkipPattern ) );

				// Add each Skip Condition OWD to an IIUnknownVector
				IIUnknownVectorPtr	ipCollection( CLSID_IUnknownVector );
				ASSERT_RESOURCE_ALLOCATION("ELI14219", ipCollection != NULL);
				ipCollection->PushBack( ipOWD1 );
				ipCollection->PushBack( ipOWD2 );

				// Retrieve contained Multiple Object Holder
				IMultipleObjectHolderPtr ipMOH = ipSkipOR;
				ASSERT_RESOURCE_ALLOCATION("ELI14218", ipMOH != NULL);

				// Provide the collected Skip Conditions to the Multiple Object Holder
				ipMOH->ObjectsVector = ipCollection;

				// Put the combined Skip Condition into the Object With Description
				ipSkipOWD->PutObject( ipSkipOR );
			}
			else if (ipSkipFile != NULL)
			{
				// Put the Skip Condition into the Object With Description
				ipSkipOWD->PutObject( ipSkipFile );
			}
			else
			{
				// Put the Skip Condition into the Object With Description
				ipSkipOWD->PutObject( ipSkipPattern );
			}

			// Set OWD Description to description from Categorized Component of the Skip Condition
			// and surround text with <>
			if (ipSkipOWD->Object != NULL)
			{
				ipSkipOWD->Description = get_bstr_t( makeDescription( ipSkipOWD->Object ) );
			}
			// else there is no defined Skip Condition and the description should remain empty
		}

		// Provide the resulting Skip Condition to the File Supplier Manager
		ipFSMgmt->FAMCondition = ipSkipOWD;

		/////////////////
		// Apply the Action tab settings
		/////////////////
		// Set the action name
		ipFPMgr->ActionName = strActionName.c_str();

		// Enable File Supplying
		IFileActionMgmtRolePtr ipMgmtRole = ipFSMgmt;
		ASSERT_RESOURCE_ALLOCATION( "ELI14430", ipMgmtRole != NULL );
		ipMgmtRole->Enabled = bSupplyingRoleEnabled ? VARIANT_TRUE : VARIANT_FALSE;

		// Enable File Processing
		ipMgmtRole = ipFPMgmt;
		ASSERT_RESOURCE_ALLOCATION( "ELI14431", ipMgmtRole != NULL );
		ipMgmtRole->Enabled = bProcessingRoleEnabled ? VARIANT_TRUE : VARIANT_FALSE;

		// Enable Statistics
		ipFPMgr->DisplayOfStatisticsEnabled = 
			bDisplayOfStatisticsEnabled ? VARIANT_TRUE : VARIANT_FALSE;

		/////////////////
		// Save the new FPS file
		/////////////////
		ipFPMgr->SaveTo( get_bstr_t( strOutput.c_str() ), VARIANT_TRUE );
	}

	// Wait until the file is readable
	waitForFileToBeReadable(strOutput);
}
//-------------------------------------------------------------------------------------------------
std::string CConvertFPSFileDlg::makeDescription(IUnknownPtr ipObject)
{
	string strDescription;

	// Get ICategorizedComponentPtr
	ICategorizedComponentPtr ipComp = ipObject;
	ASSERT_RESOURCE_ALLOCATION("ELI14268", ipComp != NULL);

	// Get object description
	_bstr_t	bstrDesc = ipComp->GetComponentDescription();
	if (bstrDesc.length() > 0)
	{
		// Build final description
		strDescription += "<";
		strDescription += asString( bstrDesc ).c_str();
		strDescription += ">";
	}

	return strDescription;
}
//-------------------------------------------------------------------------------------------------
