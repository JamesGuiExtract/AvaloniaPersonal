// FileProcessingOptionsDlg.cpp : implementation file
//

#include "stdafx.h"
#include "uclidfileprocessing.h"
#include "FileProcessingOptionsDlg.h"
#include <cpputil.h>
#include <UCLIDException.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const int giMAX_DISPLAY_RECORDS = 999;

//-------------------------------------------------------------------------------------------------
// FileProcessingOptionsDlg dialog
//-------------------------------------------------------------------------------------------------
FileProcessingOptionsDlg::FileProcessingOptionsDlg(CWnd* pParent /*=NULL*/)
: CDialog(FileProcessingOptionsDlg::IDD, pParent),
  m_pConfigManager(__nullptr)
{
}
//-------------------------------------------------------------------------------------------------
void FileProcessingOptionsDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_EDIT_MAX_NUM_RECORDS, m_editMaxDisplayRecords);
	DDX_Control(pDX, IDC_SPIN_MAX_NUM_RECORDS, m_SpinMaxRecords);
	DDX_Check(pDX, IDC_CHECK_AUTO_SAVE_FPS, m_bAutoSave);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(FileProcessingOptionsDlg, CDialog)
	ON_EN_CHANGE(IDC_EDIT_MAX_NUM_RECORDS, OnEnChangeEditMaxDisplay)
END_MESSAGE_MAP()
//-------------------------------------------------------------------------------------------------
void FileProcessingOptionsDlg::setConfigManager(FileProcessingConfigMgr* pConfigManager)
{
	m_pConfigManager = pConfigManager;
}
//-------------------------------------------------------------------------------------------------
long FileProcessingOptionsDlg::getMaxDisplayRecords()
{
	return min(m_pConfigManager->getMaxStoredRecords(), giMAX_DISPLAY_RECORDS);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingOptionsDlg::setMaxDisplayRecords(long nMaxDisplayRecords)
{
	m_pConfigManager->setMaxStoredRecords(nMaxDisplayRecords);
}
//-------------------------------------------------------------------------------------------------
bool FileProcessingOptionsDlg::getAutoSaveFPSFile()
{
	return m_pConfigManager->getAutoSaveFPSOnRun();
}
//-------------------------------------------------------------------------------------------------
void FileProcessingOptionsDlg::setAutoSaveFPSFile()
{
	m_pConfigManager->setAutoSaveFPSOnRun(asCppBool(m_bAutoSave));
}

//-------------------------------------------------------------------------------------------------
// FileProcessingOptionsDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL FileProcessingOptionsDlg::OnInitDialog() 
{
	CDialog::OnInitDialog();
	try
	{
		if (m_pConfigManager == __nullptr)
		{
			return TRUE;
		}

		//Set the Edit Threads box to be controlled by the spin control.
		m_SpinMaxRecords.SetBuddy(&m_editMaxDisplayRecords);
		m_SpinMaxRecords.SetRange32(1, giMAX_DISPLAY_RECORDS);

		// Number of records is always restricted, so always set a value
		m_editMaxDisplayRecords.SetWindowText(asString(getMaxDisplayRecords()).c_str());

		m_bAutoSave = asMFCBool(getAutoSaveFPSFile());

		UpdateData(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12470");
	
	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//-------------------------------------------------------------------------------------------------
void FileProcessingOptionsDlg::OnOK() 
{
	try
	{
		UpdateData(TRUE);

		// Save number of displayed records
		setMaxDisplayRecords(getMaxNumberOfRecordsFromDialog());

		setAutoSaveFPSFile();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12469");

	CDialog::OnOK();
}
//-------------------------------------------------------------------------------------------------
void FileProcessingOptionsDlg::OnEnChangeEditMaxDisplay()
{
	try
	{
		getMaxNumberOfRecordsFromDialog();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI32153");
}
//-------------------------------------------------------------------------------------------------
long FileProcessingOptionsDlg::getMaxNumberOfRecordsFromDialog()
{
	// Default to default value
	long nNewValue = giMAX_DISPLAY_RECORDS;
	CString zNumRecords;
	m_editMaxDisplayRecords.GetWindowText(zNumRecords);
	string strNumRecords = zNumRecords;
	if (!strNumRecords.empty())
	{
		bool bUpdateText = false;
		try
		{
			// Get the value
			nNewValue = asLong(strNumRecords);

			// Check that it is in the acceptable range
			if (nNewValue > giMAX_DISPLAY_RECORDS)
			{
				bUpdateText = true;
				nNewValue = giMAX_DISPLAY_RECORDS;
			}
			else if (nNewValue < 1)
			{
				bUpdateText = true;
				nNewValue = 1;
			}
		}
		catch(...)
		{
			// Set the value to 1
			nNewValue = 1;
			bUpdateText = true;
		}

		if (bUpdateText)
		{
			m_editMaxDisplayRecords.SetWindowText(asString(nNewValue).c_str());
		}
	}

	return nNewValue;
}
//-------------------------------------------------------------------------------------------------
