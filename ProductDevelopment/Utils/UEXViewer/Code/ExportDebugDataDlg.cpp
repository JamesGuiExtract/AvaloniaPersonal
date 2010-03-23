// ExportDebugDataDlg.cpp : implementation file
//

#include "stdafx.h"
#include "UEXViewer.h"
#include "ExportDebugDataDlg.h"
#include "ExceptionListControlHelper.h"

#include <UCLIDException.h>
#include <FileDialogEx.h>
#include <string>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Defines
//-------------------------------------------------------------------------------------------------
#define RADIO_ALL 0
#define RADIO_SELECTED 1

//-------------------------------------------------------------------------------------------------
// CExportDebugDataDlg dialog
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CExportDebugDataDlg, CDialog)

CExportDebugDataDlg::CExportDebugDataDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CExportDebugDataDlg::IDD, pParent)
	, m_pUEXDlg((CUEXViewerDlg *)pParent) 
	, m_zExportfile(_T(""))
	, m_iAppendToFile(0)
	, m_iScope(RADIO_ALL)
	, m_iLimitScope(0)
	, m_zELICodeToLimit(_T(""))
{

}
//-------------------------------------------------------------------------------------------------
CExportDebugDataDlg::~CExportDebugDataDlg()
{
}
//-------------------------------------------------------------------------------------------------
void CExportDebugDataDlg::DoDataExchange(CDataExchange* pDX)
{
	try
	{
		CDialog::DoDataExchange(pDX);
		DDX_Text(pDX, IDC_EDIT_DEBUG_PARAMETER, m_zDebugParamToExport);
		DDX_Text(pDX, IDC_EDIT_EXPORT_FILENAME, m_zExportfile);
		DDX_Check(pDX, IDC_CHECK_APPEND, m_iAppendToFile);
		DDX_Radio(pDX, IDC_RADIO_DISPLAY_ALL, m_iScope);
		DDX_Check(pDX, IDC_CHECK_NARROW_SCOPE, m_iLimitScope);
		DDX_Text(pDX, IDC_EDIT_ELICODE, m_zELICodeToLimit);
		DDX_Control(pDX, IDC_EDIT_ELICODE, m_editELICode);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI28747");
}
//-------------------------------------------------------------------------------------------------
BOOL CExportDebugDataDlg::OnInitDialog() 
{
	CDialog::OnInitDialog();
	
	try
	{
		m_zELICodeToLimit = "ELI";
		UpdateData(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI28820");

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//-------------------------------------------------------------------------------------------------

BEGIN_MESSAGE_MAP(CExportDebugDataDlg, CDialog)
	ON_BN_CLICKED(IDOK, &CExportDebugDataDlg::OnBnClickedOk)
	ON_BN_CLICKED(IDC_BUTTON_BROWSE, &CExportDebugDataDlg::OnBnClickedButtonBrowse)
	ON_BN_CLICKED(IDC_CHECK_NARROW_SCOPE, &CExportDebugDataDlg::OnBnClickedCheckNarrowScope)
	ON_EN_UPDATE(IDC_EDIT_ELICODE, &CExportDebugDataDlg::OnEnUpdateEditElicode)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CExportDebugDataDlg message handlers
//-------------------------------------------------------------------------------------------------
void CExportDebugDataDlg::OnBnClickedOk()
{
	try
	{
		// Set wait cursor
		CWaitCursor wait;

		// Load the data in to the member variables
		UpdateData(TRUE);

		vector<string> vecDebugParams;

		// Check if exporting from all records
		if ( m_iScope == RADIO_ALL)
		{
			int iStartIndex = 0;
			int iStopIndex = m_pUEXDlg->GetExceptionCount();

			for (int i = iStartIndex; i < iStopIndex; i++)
			{
				getExceptionDataFromUEXList(i, vecDebugParams);
			}
		}
		else
		{
			// Find position of first selection
			POSITION pos = m_pUEXDlg->m_listUEX.GetFirstSelectedItemPosition();
			int nCurrSelected;
			while(pos != NULL)
			{
				// Get index of selected item
				nCurrSelected = m_pUEXDlg->m_listUEX.GetNextSelectedItem( pos );

				getExceptionDataFromUEXList(nCurrSelected, vecDebugParams);
			}
		}

		// Output the data to the file if data was found
		if ( !vecDebugParams.empty())
		{
			// open the file in write mode
			ofstream outfile(m_zExportfile, ios::binary | 
				((m_iAppendToFile == BST_CHECKED) ? ios::app : ios::out));
			if (!outfile)
			{
				UCLIDException ue("ELI28770", "Unable to open file in write mode!");
				ue.addDebugInfo("Export FileName", (LPCSTR)m_zExportfile);
				throw ue;
			}

			// write the bytes to the file
			int nSize = vecDebugParams.size();
			for ( int i = 0; i < nSize; i++)
			{
				outfile.write( vecDebugParams[i].c_str(), vecDebugParams[i].size());
				outfile.write( "\r\n", 2);
			}
			outfile.close();

			// Display message for the number of items exported
			string strMessage = "Exported " + asString(nSize) + " entries.";
			MessageBox(strMessage.c_str(), "Entries exported");
		}
		else 
		{
			MessageBox("No entries were exported.", "Entries exported");
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI28780");
}
//-------------------------------------------------------------------------------------------------
void CExportDebugDataDlg::OnBnClickedButtonBrowse()
{
	try
	{
		// Get data from the controls
		UpdateData(TRUE);

		// Open the file selection dialog
		CFileDialogEx fileDlg(TRUE, NULL, m_zExportfile, 
			OFN_HIDEREADONLY | OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR,
			"All Files (*.*)|*.*||", NULL);
		
		//  if OK was selected set the new file name
		if (fileDlg.DoModal() == IDOK)
		{
			m_zExportfile = fileDlg.GetPathName();
			UpdateData(FALSE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI28971");
}
//-------------------------------------------------------------------------------------------------
void CExportDebugDataDlg::OnBnClickedCheckNarrowScope()
{
	try
	{
		// Get data from the dialog controls
		UpdateData();

		// enable the ELI code edit box based on the value of limit scope check box
		m_editELICode.EnableWindow(asMFCBool(m_iLimitScope == BST_CHECKED));
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI28785");
}
//-------------------------------------------------------------------------------------------------
void CExportDebugDataDlg::OnEnUpdateEditElicode()
{
	try
	{
		// Get data from dialog controls
		UpdateData(TRUE);

		// If the ELICode does not begin with ELI the set to ELI
		if ( m_zELICodeToLimit.Find("ELI") != 0)
		{
			m_zELICodeToLimit = "ELI";
			UpdateData(FALSE);	

			// Set the selection start and end to the end of ELI
			m_editELICode.SetSel(3,3);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI28821");
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void CExportDebugDataDlg::getDebugDataFromException(vector<string> &rvecDebugParams, 
													const UCLIDException &ueDebug)
{
	// get the debug vector for the exception
	const vector<NamedValueTypePair> &vecDebug = ueDebug.getDebugVector();

	// Search vector for the debug parameter
	int nSize = vecDebug.size();
	for (int i = 0; i < nSize; i++)
	{
		// Compare the name to the name that is being exported
		if (vecDebug[i].GetName().c_str() == m_zDebugParamToExport)
		{
			// Get the Pair
			ValueTypePair &pair = vecDebug[i].GetPair();

			// Get the value (decrypted if allowed) [LRCAU #5696]
			string strValue = UCLIDException::sGetDataValue(pair.getValueAsString());

			// Add the value to the vector
			rvecDebugParams.push_back(strValue);
		}
	}

	// If there is an inner exception call this function recursively
	if (ueDebug.getInnerException() != NULL)
	{
		// Call this recursively for each inner exception
		getDebugDataFromException(rvecDebugParams, *ueDebug.getInnerException());
	}
}
//-------------------------------------------------------------------------------------------------
void CExportDebugDataDlg::getExceptionDataFromUEXList(int nItemNumber, vector<string> &rvecDebugParams)
{
	// Retrieve this data structure
	ITEMINFO*	pData = (ITEMINFO *)m_pUEXDlg->m_listUEX.GetItemData( nItemNumber );
	ASSERT_RESOURCE_ALLOCATION("ELI28766", pData != NULL);

	// Create a UCLIDException object
	// using new ELI code for this application
	UCLIDException ueDebug;

	// Convert the selected data into UCLIDException
	// NOTE: if the strData is not a stringized exception the TopELI code will be the 
	// ELI code passed in otherwise it will be the top ELI code for the stringized exception
	ueDebug.createFromString("ELI28767", pData->strData);
	if (m_iLimitScope != BST_CHECKED || ueDebug.getTopELI().c_str() == m_zELICodeToLimit)
	{
		getDebugDataFromException( rvecDebugParams, ueDebug );
	}
}
//-------------------------------------------------------------------------------------------------
