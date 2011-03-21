#include "StdAfx.h"
#include "AdvancedTaskSettingsDlg.h"
#include "CommonConstants.h"

#include <UCLIDException.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// The upper bound for the threads dialog box
const int giTHREADS_UPPER_RANGE = 100;

// ProgID for the set schedule COM object
const string gstrSET_SCHEDULE_PROG_ID = "Extract.FileActionManager.Forms.SetProcessingSchedule";

//-------------------------------------------------------------------------------------------------
// AdvancedTaskSettingsDlg
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(AdvancedTaskSettingsDlg, CDialog)
//-------------------------------------------------------------------------------------------------
AdvancedTaskSettingsDlg::AdvancedTaskSettingsDlg(int iNumThreads, bool bKeepProcessing,
	IVariantVectorPtr ipSchedule, long nNumFilesFromDb, CWnd* pParent) :
    CDialog(AdvancedTaskSettingsDlg::IDD, pParent),
	m_iNumThreads(iNumThreads),
	m_bKeepProcessing(bKeepProcessing),
	m_ipSchedule(ipSchedule),
	m_bLimitProcessingTimes(asMFCBool(ipSchedule != __nullptr)),
	m_nNumFiles(nNumFilesFromDb)
{
}
//-------------------------------------------------------------------------------------------------
AdvancedTaskSettingsDlg::~AdvancedTaskSettingsDlg()
{
}
//-------------------------------------------------------------------------------------------------
void AdvancedTaskSettingsDlg::DoDataExchange(CDataExchange* pDX)
{
	DDX_Control(pDX, IDC_RADIO_MAX_THREADS, m_btnMaxThreads);
	DDX_Control(pDX, IDC_RADIO_THREADS, m_btnNumThreads);
	DDX_Control(pDX, IDC_EDIT_THREADS, m_editThreads);
	DDX_Control(pDX, IDC_SPIN_THREADS, m_SpinThreads);
	DDX_Control(pDX, IDC_RADIO_KEEP_PROCESSING_FILES, m_btnKeepProcessingWithEmptyQueue);
	DDX_Control(pDX, IDC_RADIO_STOP_PROCESSING_FILES, m_btnStopProcessingWithEmptyQueue);
	DDX_Control(pDX, IDC_STATIC_PROCESSING_SCHEDULE, m_groupProcessingSchedule);
	DDX_Check(pDX, IDC_CHECK_LIMIT_PROCESSING, m_bLimitProcessingTimes);
	DDX_Control(pDX, IDC_BUTTON_SET_SCHEDULE, m_btnSetSchedule);
	DDX_Control(pDX, IDC_CHECK_LIMIT_PROCESSING, m_checkLimitProcessing);
	DDX_Control(pDX, IDC_EDIT_NUM_FILES_FROM_DB, m_editNumFiles);
	DDX_Control(pDX, IDC_SPIN_NUM_FILES, m_SpinNumFiles);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(AdvancedTaskSettingsDlg, CDialog)
	ON_BN_CLICKED(IDC_RADIO_MAX_THREADS, OnBtnMaxThread)
	ON_BN_CLICKED(IDOK, OnBtnOK)
	ON_BN_CLICKED(IDC_RADIO_THREADS, OnBtnNumThread)
	ON_BN_CLICKED(IDC_RADIO_KEEP_PROCESSING_FILES, OnBtnKeepProcessingWithEmptyQueue)
	ON_BN_CLICKED(IDC_RADIO_STOP_PROCESSING_FILES, OnBtnStopProcessingWithEmptyQueue)
	ON_EN_CHANGE(IDC_EDIT_THREADS, OnEnChangeEditThreads)
	ON_BN_CLICKED(IDC_CHECK_LIMIT_PROCESSING, OnBtnClickedCheckLimitProcessing)
	ON_BN_CLICKED(IDC_BUTTON_SET_SCHEDULE, OnBnClickedButtonSetSchedule)
	ON_EN_CHANGE(IDC_EDIT_NUM_FILES_FROM_DB, OnEnChangeEditNumFiles)
END_MESSAGE_MAP()
//-------------------------------------------------------------------------------------------------
BOOL AdvancedTaskSettingsDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		CDialog::OnInitDialog();

		//Set the Edit Threads box to be controlled by the spin control.
		m_SpinThreads.SetBuddy(&m_editThreads);
		m_SpinThreads.SetRange32(0, giTHREADS_UPPER_RANGE);

		// Set the edit num files box to be controlled by the spin control
		m_SpinNumFiles.SetBuddy(&m_editNumFiles);
		m_SpinNumFiles.SetRange32(gnNUM_FILES_LOWER_RANGE, gnNUM_FILES_UPPER_RANGE);

		bool bMaxThreads = m_iNumThreads == 0;
		m_btnMaxThreads.SetCheck(asBSTChecked(bMaxThreads));
		m_btnNumThreads.SetCheck(asBSTChecked(!bMaxThreads));
		if (!bMaxThreads)
		{
			string strNum = asString(m_iNumThreads);
			m_editThreads.SetWindowText(strNum.c_str());
		}

		m_btnKeepProcessingWithEmptyQueue.SetCheck(asBSTChecked(m_bKeepProcessing));
		m_btnStopProcessingWithEmptyQueue.SetCheck(asBSTChecked(!m_bKeepProcessing));

		m_editNumFiles.SetWindowText(asString(m_nNumFiles).c_str());

		updateEnabledStates();

		CenterWindow();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI32120")
	
	return TRUE;  // return TRUE unless you set the focus to a control
}
//-------------------------------------------------------------------------------------------------
void AdvancedTaskSettingsDlg::OnBtnOK()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		UpdateData(TRUE);

		// Validate the settings.
		if (m_bLimitProcessingTimes == TRUE && m_ipSchedule == __nullptr)
		{
			MessageBox("You must define a processing schedule.", "No Schedule",
				MB_ICONWARNING | MB_OK);
			m_btnSetSchedule.SetFocus();
			return;
		}
		if (m_btnNumThreads.GetCheck() == BST_CHECKED && m_editThreads.LineLength() == 0)
		{
			MessageBox("Number of threads to use for processing must not be blank.",
				"Invalid Entry", MB_ICONWARNING | MB_OK);
			m_editThreads.SetFocus();
			return;
		}
		if (m_editNumFiles.LineLength() == 0)
		{
			MessageBox("Number of files to retrieve from the queue must not be blank.",
				"Invalid Entry", MB_ICONWARNING | MB_OK);
			m_editNumFiles.SetFocus();
			return;
		}

		m_iNumThreads = getNumThreads();
		m_nNumFiles = getNumFiles();

		CDialog::OnOK();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI32121");
}
//-------------------------------------------------------------------------------------------------
void AdvancedTaskSettingsDlg::OnBtnMaxThread() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		m_iNumThreads = 0;
		updateEnabledStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI32122");
}
//--------------------------------------------------------------------------------------------------
void AdvancedTaskSettingsDlg::OnBtnNumThread()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// if the editbox is empty, default its value to 1
		CString zText;
		m_editThreads.GetWindowText(zText);
		if (zText.IsEmpty())
		{
			m_editThreads.SetWindowText("1");
		}

		// Provide number of threads to the MgmtRole object (P13 #4312)
		m_iNumThreads = getNumThreads();

		// Enable and disable controls as appropriate
		updateEnabledStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI32123");
}
//--------------------------------------------------------------------------------------------------
void AdvancedTaskSettingsDlg::OnEnChangeEditThreads()
{
	try
	{
		m_iNumThreads = getNumThreads();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI32124");
}
//--------------------------------------------------------------------------------------------------
void AdvancedTaskSettingsDlg::OnEnChangeEditNumFiles()
{
	try
	{
		m_nNumFiles = getNumFiles();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI32141");
}
//--------------------------------------------------------------------------------------------------
void AdvancedTaskSettingsDlg::OnBtnKeepProcessingWithEmptyQueue()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		m_bKeepProcessing = true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI32125");
}
//--------------------------------------------------------------------------------------------------
void AdvancedTaskSettingsDlg::OnBtnStopProcessingWithEmptyQueue()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		m_bKeepProcessing = false;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI32126");
}
//-------------------------------------------------------------------------------------------------
void AdvancedTaskSettingsDlg::OnBtnClickedCheckLimitProcessing()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		UpdateData(TRUE);

		// Enable / disable controls as appropriate
		updateEnabledStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI32127");
}
//-------------------------------------------------------------------------------------------------
void AdvancedTaskSettingsDlg::OnBnClickedButtonSetSchedule()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		bool bCreated = false;
		if (m_ipSchedule == __nullptr)
		{
			m_ipSchedule.CreateInstance(CLSID_VariantVector);
			ASSERT_RESOURCE_ALLOCATION("ELI32128", m_ipSchedule != __nullptr);
			for(int i=0; i < giNUMBER_OF_HOURS_IN_WEEK; i++)
			{
				m_ipSchedule->PushBack(_variant_t(true));
			}
			bCreated = true;
		}

		// Get the set schedule COM object
		UCLID_FILEPROCESSINGLib::ISetProcessingSchedulePtr ipSet(gstrSET_SCHEDULE_PROG_ID.c_str());
		ASSERT_RESOURCE_ALLOCATION("ELI32129", ipSet != NULL);

		// Prompt the user to set the new schedule
		IVariantVectorPtr ipSchedule = ipSet->PromptForSchedule(m_ipSchedule);
		if (ipSchedule != __nullptr || bCreated)
		{
			// Update the schedule
			m_ipSchedule = ipSchedule;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI32130");
}
//-------------------------------------------------------------------------------------------------
int AdvancedTaskSettingsDlg::getNumberOfThreads()
{
	return m_iNumThreads;
}
//-------------------------------------------------------------------------------------------------
bool AdvancedTaskSettingsDlg::getKeepProcessing()
{
	return m_bKeepProcessing;
}
//-------------------------------------------------------------------------------------------------
IVariantVectorPtr AdvancedTaskSettingsDlg::getSchedule()
{
	if (m_bLimitProcessingTimes == TRUE)
	{
		return m_ipSchedule;
	}

	return __nullptr;
}
//-------------------------------------------------------------------------------------------------
long AdvancedTaskSettingsDlg::getNumberOfFilesFromDb()
{
	return m_nNumFiles;
}
//-------------------------------------------------------------------------------------------------
void AdvancedTaskSettingsDlg::updateEnabledStates()
{
	UpdateData(FALSE);

	BOOL bNumThreadsChecked = asMFCBool(m_btnNumThreads.GetCheck() == BST_CHECKED);
	m_editThreads.EnableWindow( bNumThreadsChecked );
	m_SpinThreads.EnableWindow( bNumThreadsChecked );

	m_btnSetSchedule.EnableWindow(m_bLimitProcessingTimes);
}
//-------------------------------------------------------------------------------------------------
int AdvancedTaskSettingsDlg::getNumThreads()
{
	// Default to Max
	int nNewValue = 0;
	if ( m_btnNumThreads.GetCheck() == BST_CHECKED )
	{
		CString zNumThreads;
		m_editThreads.GetWindowText( zNumThreads);
		string strNumThreads = zNumThreads;
		if ( strNumThreads != "" )
		{
			bool bUpdateText = false;
			try
			{
				// Get the value
				nNewValue = asLong( strNumThreads );

				// Check that it is in the acceptable range
				if (nNewValue > giTHREADS_UPPER_RANGE)
				{
					bUpdateText = true;
					nNewValue = giTHREADS_UPPER_RANGE;
				}
				else if (nNewValue < 0)
				{
					bUpdateText = true;
					nNewValue = 0;
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
				m_editThreads.SetWindowText(asString(nNewValue).c_str());
				int nLength = m_editThreads.LineLength();
				m_editThreads.SetSel(0, nLength);
			}
		}
		else
		{
			nNewValue = 0;
		}
	}

	return nNewValue;
}
//-------------------------------------------------------------------------------------------------
long AdvancedTaskSettingsDlg::getNumFiles()
{
	// Default to default value
	long nNewValue = gnMAX_NUMBER_OF_FILES_FROM_DB;
	CString zNumFiles;
	m_editNumFiles.GetWindowText(zNumFiles);
	string strNumFiles = zNumFiles;
	if ( strNumFiles != "" )
	{
		bool bUpdateText = false;
		try
		{
			// Get the value
			nNewValue = asLong( strNumFiles );

			// Check that it is in the acceptable range
			if (nNewValue > gnNUM_FILES_UPPER_RANGE)
			{
				bUpdateText = true;
				nNewValue = gnNUM_FILES_UPPER_RANGE;
			}
			else if (nNewValue < gnNUM_FILES_LOWER_RANGE)
			{
				bUpdateText = true;
				nNewValue = gnNUM_FILES_LOWER_RANGE;
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
			m_editNumFiles.SetWindowText(asString(nNewValue).c_str());
			int nLength = m_editNumFiles.LineLength();
			m_editNumFiles.SetSel(0, nLength);
		}
	}

	return nNewValue;
}
//-------------------------------------------------------------------------------------------------