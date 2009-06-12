// DialogSelect.cpp : implementation file
//

#include "stdafx.h"
#include "FAMUtils.h"
#include "DialogSelect.h"
#include "DotNetUtils.h"
#include "FAMUtilsConstants.h"

#include <ExtractMFCUtils.h>
#include <TemporaryResourceOverride.h>

#include <UCLIDException.h>

extern HINSTANCE gFAMUtilsModuleResource;

// Constants
const string gstrSERVER_TITLE = "Select Database Server";
const string gstrDB_TITLE = "Select Database Name";
const string gstrSERVER_COMBO_LABEL = "Database server name";
const string gstrDB_COMBO_LABEL = "Database name";

//-------------------------------------------------------------------------------------------------
// CDialogSelect dialog
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CDialogSelect, CDialog)

CDialogSelect::CDialogSelect(CWnd* pParent /*=NULL*/)
	: CDialog(CDialogSelect::IDD, pParent),
	m_zComboValue(""),
	m_comboData(DBInfoCombo::kServerName),
	m_strServer("")
{
}
//-------------------------------------------------------------------------------------------------
CDialogSelect::CDialogSelect(const string& strServer, CWnd* pParent /*=NULL*/)
	: CDialog(CDialogSelect::IDD, pParent),
	m_zComboValue(""),
	m_comboData(DBInfoCombo::kDatabaseName),
	m_strServer(strServer)
{
	m_comboData.setSQLServer(strServer);
}
//-------------------------------------------------------------------------------------------------
CDialogSelect::~CDialogSelect()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20394");
}
//-------------------------------------------------------------------------------------------------
void CDialogSelect::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_STATIC_COMBO, m_staticComboLabel);
	DDX_Control(pDX, IDC_COMBO_LIST, m_comboData);
	DDX_CBString(pDX, IDC_COMBO_LIST, m_zComboValue);
}
//-------------------------------------------------------------------------------------------------
INT_PTR CDialogSelect::DoModal()
{
	TemporaryResourceOverride rcOverride(gFAMUtilsModuleResource);

	// call the base class member
	return CDialog::DoModal();
}

//-------------------------------------------------------------------------------------------------
// Message Map
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CDialogSelect, CDialog)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CDialogSelect message handlers
//-------------------------------------------------------------------------------------------------
BOOL CDialogSelect::OnInitDialog()
{
	try
	{
		CDialog::OnInitDialog();

		// Set up with the appropriate labels
		switch (m_comboData.getDBInfoType())
		{
		case DBInfoCombo::kServerName:
			SetWindowTextA(gstrSERVER_TITLE.c_str());
			m_staticComboLabel.SetWindowTextA(gstrSERVER_COMBO_LABEL.c_str());
			break;
		case DBInfoCombo::kDatabaseName:
			SetWindowTextA(gstrDB_TITLE.c_str());
			m_staticComboLabel.SetWindowTextA(gstrDB_COMBO_LABEL.c_str());
			break;
		default:
			THROW_LOGIC_ERROR_EXCEPTION("ELI17619");
		};
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17431");
	
	return TRUE;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
