// AddDocTypesDlg.cpp : implementation file
//

#include "stdafx.h"
#include "AFUtils.h"
#include "resource.h"
#include "AddDocTypesDlg.h"

#include  <io.h>
#include <UCLIDException.h>
#include <ExtractMFCUtils.h>
#include <cpputil.h>
#include <comutils.h>
#include "SpecialStringDefinitions.h"

using std::string;
using std::vector;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// AddDocTypesDlg dialog
//-------------------------------------------------------------------------------------------------
AddDocTypesDlg::AddDocTypesDlg(std::string strIndustry, bool bAllowSpecial, bool bAllowMultiSelect, 
							   bool bAllowMultiplyClassified, CWnd* pParent)
: CDialog(AddDocTypesDlg::IDD, pParent),
  m_strCategory(strIndustry),
  m_bAllowSpecial(bAllowSpecial),
  m_bAllowMultipleSelection(bAllowMultiSelect),
  m_bAllowMultiplyClassified(bAllowMultiplyClassified),
  m_bLockIndustry(false)
{
	//{{AFX_DATA_INIT(AddDocTypesDlg)
	//}}AFX_DATA_INIT
}
//-------------------------------------------------------------------------------------------------
void AddDocTypesDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(AddDocTypesDlg)
	DDX_Control(pDX, IDC_LIST_CHOOSE_TYPES, m_listTypes);
	DDX_Control(pDX, IDC_CMB_CATEGORY, m_cmbIndustry);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(AddDocTypesDlg, CDialog)
	//{{AFX_MSG_MAP(AddDocTypesDlg)
	ON_CBN_SELCHANGE(IDC_CMB_CATEGORY, OnSelchangeComboCategory)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// Public methods
//-------------------------------------------------------------------------------------------------
vector<string>& AddDocTypesDlg::getChosenTypes()
{
	// Provide collection of selected types
	return m_vecTypes;
}
//-------------------------------------------------------------------------------------------------
std::string AddDocTypesDlg::getSelectedIndustry()
{
	return m_strCategory;
}
//-------------------------------------------------------------------------------------------------
void AddDocTypesDlg::lockIndustrySelection()
{
	m_bLockIndustry = true;
}

//-------------------------------------------------------------------------------------------------
// AddDocTypesDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL AddDocTypesDlg::OnInitDialog() 
{
	CDialog::OnInitDialog();
	
	// Clear collection of selected types
	m_vecTypes.clear();

	// Populate Combo box and list box
	populateComboBox();
	populateListBox();

	// Set selection restriction on list box
	if (!m_bAllowMultipleSelection)
	{
		// Remove multiple selection
		m_listTypes.ModifyStyle( LBS_MULTIPLESEL, 0 );

		// Recreate the list box to force application of the new setting
		recreateListBox( &m_listTypes );
	}

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//-------------------------------------------------------------------------------------------------
void AddDocTypesDlg::OnOK() 
{
	// Clear collection of selected types
	m_vecTypes.clear();

	int i;
	for (i = 0; i < m_listTypes.GetCount(); i++)
	{
		if (m_listTypes.GetSel(i) <= 0)
		{
			continue;
		}
		CString cstr;
		m_listTypes.GetText(i, cstr);
		string str = cstr;
		m_vecTypes.push_back(str);
	}
	
	CDialog::OnOK();
}
//-------------------------------------------------------------------------------------------------
void AddDocTypesDlg::OnSelchangeComboCategory() 
{
	// Store new category name
	int iSel = m_cmbIndustry.GetCurSel();
	CString	zCategory;
	m_cmbIndustry.GetLBText( iSel, zCategory );
	m_strCategory = zCategory.operator LPCTSTR();

	// Repopulate list from new category
	populateListBox();
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void AddDocTypesDlg::populateComboBox()
{
	// Create the Document Classification Utils object
	if (m_ipDocUtils == __nullptr)
	{
		m_ipDocUtils.CreateInstance( CLSID_DocumentClassifier );
		ASSERT_RESOURCE_ALLOCATION("ELI11935", m_ipDocUtils != __nullptr);
	}

	// Clear combo box before populating
	m_cmbIndustry.ResetContent();

	// Get vector of Industry names
	m_ipIndustries = m_ipDocUtils->GetDocumentIndustries();

	// Add Industries from vector
	long lCount = m_ipIndustries->Size;
	int iSelected = -1;
	int i, j;
	for (i = 0; i < lCount; i++)
	{
		string strTemp = asString(_bstr_t( m_ipIndustries->GetItem( i )));
		j = m_cmbIndustry.AddString( strTemp.c_str() );

		// Check for match to input Industry
		if (strTemp.compare( m_strCategory.c_str() ) == 0)
		{
			iSelected = j;
		}
	}

	// Select input item
	if (iSelected != -1)
	{
		m_cmbIndustry.SetCurSel( iSelected );
	}
	// Check for provided input Industry not found
	else if (m_strCategory.length() > 0)
	{
		// Throw exception
		UCLIDException ue("ELI11936", "Unknown industry name.");
		ue.addDebugInfo( "Input industry", m_strCategory );
		throw ue;
	}
	// else no input Industry provided, just default selection to first item
	else
	{
		// Set the selection
		m_cmbIndustry.SetCurSel( 0 );

		// Store industry name
		CString zSel;
		m_cmbIndustry.GetLBText( 0, zSel );
		m_strCategory = zSel.operator LPCTSTR();
	}

	// Disable combo box if desired
	if (m_bLockIndustry)
	{
		m_cmbIndustry.EnableWindow( FALSE );
	}
}
//-------------------------------------------------------------------------------------------------
void AddDocTypesDlg::populateListBox()
{
	// Clear list box before populating
	m_listTypes.ResetContent();

	// Get vector of document types
	IVariantVectorPtr ipTypes = m_ipDocUtils->GetDocumentTypes( _bstr_t( m_strCategory.c_str() ) );
	ASSERT_RESOURCE_ALLOCATION("ELI11938", ipTypes != __nullptr);

	// Get vector of Special types
	int i;
	long lCount;
	if (m_bAllowSpecial)
	{
		IVariantVectorPtr ipSpecial =
			m_ipDocUtils->GetSpecialDocTypeTags(asVariantBool(m_bAllowMultiplyClassified));
		ASSERT_RESOURCE_ALLOCATION("ELI11939", ipSpecial != __nullptr);

		lCount = ipSpecial->Size;
		for (i = 0; i < lCount; i++)
		{
			string strType = asString(_bstr_t( ipSpecial->GetItem( i )));
			m_listTypes.InsertString( i, strType.c_str() );
		}
	}

	// Add types from vector - after any Special types
	long lBase = m_listTypes.GetCount();
	lCount = ipTypes->Size;
	for (i = 0; i < lCount; i++)
	{
		string strType = asString(_bstr_t( ipTypes->GetItem( i )));
		m_listTypes.InsertString( i + lBase, strType.c_str() );
	}
}
//-------------------------------------------------------------------------------------------------
