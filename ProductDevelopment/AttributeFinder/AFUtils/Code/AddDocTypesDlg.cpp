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
#include <DocTagUtils.h>
#include "SpecialStringDefinitions.h"
#include <XBrowseForFolder.h>

using std::string;
using std::vector;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

namespace		
{
	// Given a path such as:
	// C:\engineering\Binaries\FlexIndexComponents\ComponentData\15.3.0.48\DocumentClassifiers\County Document
	// find the version - in this case : 15.3.0.48
	//
	std::string FindFkbVersionInPath(const std::string& path)
	{
		VectorOfString subfolders = Split(path, '\\');
		if (subfolders.empty())
		{
			return "Not found";
		}

		for (size_t i = 0; i < subfolders.size(); ++i)
		{
			VectorOfString parts = Split(subfolders[i], '.');
			if (4 == parts.size())
			{
				return subfolders[i];
			}
		}

		return "Not found";
	}

	std::string ExpandFileName(const std::string& folder)
	{
		ITagUtilityPtr ipTagUtility(CLSID_AFUtility);
		ASSERT_RESOURCE_ALLOCATION("ELI39992", ipTagUtility != __nullptr);

		IAFDocumentPtr ipAFDoc(CLSID_AFDocument);
		ASSERT_RESOURCE_ALLOCATION("ELI39996", ipAFDoc);

		std::string expandedPath = asString(ipTagUtility->ExpandTagsAndFunctions(folder.c_str(),
																				 "dummy.voa",
																				 ipAFDoc));
		return expandedPath;
	}

	const std::string notFoundText = "No document types found.";
}

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
	ON_BN_CLICKED(IDC_BTN_CUSTOM_FILTERS_DOC_TAG, OnClickedCustomFiltersDocTag)
	ON_CBN_SELCHANGE(IDC_CMB_CATEGORY, OnSelchangeComboCategory)
	ON_BN_CLICKED(IDC_BUTTON_SELECT_FOLDER, OnButtonBrowseFile)
	ON_EN_KILLFOCUS(IDC_EDIT_ROOT_FOLDER, OnEditRootFolderLooseFocus)
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

	// Create the Document Classification Utils object
	ASSERT_RUNTIME_CONDITION("ELI40002", m_ipDocUtils == nullptr, "m_ipDocUtils is set, should be NULL");
	m_ipDocUtils.CreateInstance( CLSID_DocumentClassifier );
	ASSERT_RESOURCE_ALLOCATION("ELI11935", m_ipDocUtils != __nullptr);
	
	// Clear collection of selected types
	m_vecTypes.clear();

	// Populate Combo box and list box
	populateListBox();
	SetDocTagButton();

	SetupRootFolder();
	SetupFkbVersionTextBox();

	SetRootFolderValue("<ComponentDataDir>\\DocumentClassifiers");

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
		if (str == notFoundText)
		{
			continue;
		}

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
	IVariantVectorPtr ipTypes = nullptr;
	try
	{
		ipTypes = m_ipDocUtils->GetDocumentTypes( _bstr_t( m_strCategory.c_str() ) );
		ASSERT_RESOURCE_ALLOCATION("ELI11938", ipTypes != __nullptr);
	}
	catch (_com_error&)
	{
		// This will happen when there are no document types associated with the category, 
		// possibly because the category is dynamic.
		m_listTypes.InsertString(0, notFoundText.c_str());
		return;
	}

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
void AddDocTypesDlg::SetDocTagButton()
{
	m_btnCustomFiltersDocTag.SubclassDlgItem(IDC_BTN_CUSTOM_FILTERS_DOC_TAG, 
											 CWnd::FromHandle(m_hWnd));

	m_btnCustomFiltersDocTag.SetIcon(::LoadIcon(_Module.m_hInstResource, 
									 MAKEINTRESOURCE(IDI_SELECT_DOC_TAG_ARROW_ICON)));
}
//-------------------------------------------------------------------------------------------------
void AddDocTypesDlg::SetupFkbVersionTextBox()
{
	m_fkbVersion.SubclassDlgItem(IDC_EDIT_FKB_VERSION, CWnd::FromHandle(m_hWnd));
}

void AddDocTypesDlg::SetupRootFolder()
{
	m_editRootFolder.SubclassDlgItem(IDC_EDIT_ROOT_FOLDER, CWnd::FromHandle(m_hWnd));
}
//-------------------------------------------------------------------------------------------------
void AddDocTypesDlg::SetRootFolderValue(const std::string& value)
{
	m_editRootFolder.SetWindowText(value.c_str());
	SetFkbVersion();
	UpdateCategoriesAndTypes();
}
//-------------------------------------------------------------------------------------------------
void AddDocTypesDlg::OnClickedCustomFiltersDocTag()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ChooseDocTagForEditBox(
			ITagUtilityPtr(CLSID_AFUtility), m_btnCustomFiltersDocTag, m_editRootFolder);

		SetFkbVersion();
		UpdateCategoriesAndTypes();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI40005");
}
//-------------------------------------------------------------------------------------------------
void AddDocTypesDlg::OnButtonBrowseFile()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		if (m_initialFkbPath.empty())
		{
			std::string folder = GetRootFolderValue();
			m_initialFkbPath = ExpandFileName(folder);
		}

		// Display the folder browser
		char pszPath[MAX_PATH + 1] = {0};
		if(XBrowseForFolder(m_hWnd, m_initialFkbPath.c_str(), pszPath, sizeof(pszPath)))
		{
			// Ensure there is a path
			if (pszPath != "")
			{
				// Set the path in the UI
				m_editRootFolder.SetWindowText(pszPath);
				m_initialFkbPath = pszPath;
				SetFkbVersion();
				UpdateCategoriesAndTypes();
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI39981");
}
//-------------------------------------------------------------------------------------------------
void AddDocTypesDlg::OnEditRootFolderLooseFocus()
{
	try
	{
		SetFkbVersion();
		UpdateCategoriesAndTypes();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI39993");
}
//-------------------------------------------------------------------------------------------------
std::string AddDocTypesDlg::GetRootFolderValue()
{
	std::vector<char> buf(1024, 0);
	m_editRootFolder.GetWindowText(buf.data(), buf.size());

	std::string folder(buf.data());
	return folder;
}
//-------------------------------------------------------------------------------------------------
void AddDocTypesDlg::SetFkbVersion()
{
	std::string folder = GetRootFolderValue();
	folder = ExpandFileName(folder);
	std::string folderVersion = FindFkbVersionInPath(folder);
	m_fkbVersion.SetWindowText(folderVersion.c_str());
}
//-------------------------------------------------------------------------------------------------
void AddDocTypesDlg::UpdateCategoriesAndTypes()
{
	std::string folder = GetRootFolderValue();
	folder = ExpandFileName(folder);
	std::string classifierSubfolderName = asString(m_ipDocUtils->DocumentClassifiersSubfolderName);

	// Reset the category to prevent a spurious exception from being thrown in populatecomboBox() - 
	// either this time or next time.
	m_strCategory = "";
	if (Contains(folder, classifierSubfolderName))
	{
		m_ipDocUtils->DocumentClassifiersPath = _bstr_t(folder.c_str());
		populateComboBox();
		populateListBox();
	}
	else
	{
		m_cmbIndustry.ResetContent();
		m_listTypes.ResetContent();
		m_listTypes.InsertString(0, notFoundText.c_str());
	}
}