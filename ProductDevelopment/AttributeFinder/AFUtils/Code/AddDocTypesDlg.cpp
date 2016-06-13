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
	const std::string notFoundText = "No document types found.";
	const std::string defaultTagRelativeFolder = "<ComponentDataDir>\\DocumentClassifiers";

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

	bool IsFAMContext()
	{
		std::vector<char> buffer(1024, 0);
		::GetModuleFileName(nullptr, buffer.data(), buffer.size());
		std::string fullname(buffer.data());
		std::string name;
		auto pos = fullname.find_last_of("\\");
		if (std::string::npos == pos)
		{
			name = fullname;
		}
		else
		{
			name = fullname.substr(pos + 1);
		}

		if (0 == ::_stricmp(name.c_str(), "RuleSetEditor.exe"))
		{
			return false;
		}
		else
		{
			return true;
		}
	}

	std::string GetComponentDataFolder()
	{
		IAttributeFinderEnginePtr ipAFEngine(CLSID_AttributeFinderEngine);
		ASSERT_RESOURCE_ALLOCATION("ELI40007", ipAFEngine != __nullptr);

		std::string folder = asString(ipAFEngine->GetComponentDataFolder());
		ASSERT_RUNTIME_CONDITION("ELI40008", !folder.empty(), "Could not resolve the component data folder");

		return folder;
	}
}

//-------------------------------------------------------------------------------------------------
// AddDocTypesDlg dialog
//-------------------------------------------------------------------------------------------------
AddDocTypesDlg::AddDocTypesDlg(const std::string& strIndustry, 
							   bool bAllowSpecial, 
							   bool bAllowMultiSelect, 
							   bool bAllowMultiplyClassified, 
							   const std::string& documentClassifiersPath,  
							   CWnd* pParent)
: CDialog(AddDocTypesDlg::IDD, pParent),
  m_strCategory(strIndustry),
  m_bAllowSpecial(bAllowSpecial),
  m_bAllowMultipleSelection(bAllowMultiSelect),
  m_bAllowMultiplyClassified(bAllowMultiplyClassified),
  m_bLockIndustry(false),
  m_isFamContext(IsFAMContext()),
  m_ipTagManager(IsFAMContext() ? CLSID_MiscUtils : CLSID_AFUtility)
{
	//{{AFX_DATA_INIT(AddDocTypesDlg)
	//}}AFX_DATA_INIT

	ASSERT_RESOURCE_ALLOCATION("ELI40006", m_ipTagManager != __nullptr);

	std::string folder = GetComponentDataFolder();
	if (IsFAMContext())
	{
		// The FAM tag manager doesn't know about this tag, so add it.
		m_ipTagManager->AddTag("ComponentDataDir", folder.c_str());
	}

	if (!documentClassifiersPath.empty())
	{
		m_initialFkbPath = documentClassifiersPath;
	}
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

	SetDocTagButton();
	SetupRootFolder();
	SetupFkbVersionTextBox();

	std::string folder = m_initialFkbPath.empty() ? defaultTagRelativeFolder : m_initialFkbPath;
	SetRootFolderValue(folder);

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

	for (int i = 0; i < m_listTypes.GetCount(); i++)
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
	// Before switching to the new category, check to see if anything has
	// been selected in the current category. If so, prompt user, warning
	// that doc type selections in the current category will be lost. If
	// they cancel the operation, don't switch to the new category.
	if (DocTypesAreSelected())
	{
		const UINT okAndCancelButtons = 0x00000001L | MB_ICONWARNING;
		int iRet = ::MessageBox(this->m_hWnd, 
								"If you switch document categories, currently selected document "
								"types will be discarded. Do you want to continue (and discard selections)?",
								"Warning: selections will be discarded...",
								okAndCancelButtons);
		if (IDCANCEL == iRet)
		{
			int i = GetPreviousComboboxIndex();
			m_cmbIndustry.SetCurSel(i);
			return;
		}
	}

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
	m_initialFkbPath = value;

	SetFkbVersion();
	UpdateCategoriesAndTypes();
}
//-------------------------------------------------------------------------------------------------
void AddDocTypesDlg::OnClickedCustomFiltersDocTag()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ChooseDocTagForEditBox(m_ipTagManager, m_btnCustomFiltersDocTag, m_editRootFolder);

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
		if (DocTypesAreSelected())
		{
			const UINT okAndCancelButtons = 0x00000001L | MB_ICONWARNING;
			int iRet = ::MessageBox(this->m_hWnd, 
									"If you change the path, currently selected document "
									"types will be discarded. Do you want to continue (and discard selections)?",
									"Warning: selections will be discarded...",
									okAndCancelButtons);
			if (IDCANCEL == iRet)
			{
				int i = GetPreviousComboboxIndex();
				m_cmbIndustry.SetCurSel(i);
				return;
			}
		}

		std::string startFolder;
		if (m_initialFkbPath.empty())
		{
			std::string folder = GetRootFolderValue();
			startFolder = ExpandFileName(folder);
		}
		else
		{
			startFolder = ExpandFileName(m_initialFkbPath);
		}


		// Display the folder browser
		char pszPath[MAX_PATH + 1] = {0};
		if (XBrowseForFolder(m_hWnd, startFolder.c_str(), pszPath, sizeof(pszPath)))
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

	std::string lcFolder(folder);
	std::string lcSubfolder(classifierSubfolderName);
	makeLowerCase(lcFolder);
	makeLowerCase(lcSubfolder);

	if (Contains(lcFolder, lcSubfolder) && isValidFolder(folder))
	{
		m_ipDocUtils->DocumentClassifiersPath = folder.c_str();

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
//-------------------------------------------------------------------------------------------------
std::string AddDocTypesDlg::ExpandFileName(const std::string& folder)
{
	IAFDocumentPtr ipAFDoc(CLSID_AFDocument);
	ASSERT_RESOURCE_ALLOCATION("ELI39996", ipAFDoc);

	std::string expandedPath = asString(m_ipTagManager->ExpandTagsAndFunctions(folder.c_str(),
																			   "dummy.voa",
																			   ipAFDoc));
	return expandedPath;
}
//-------------------------------------------------------------------------------------------------
bool AddDocTypesDlg::DocTypesAreSelected()
{
	for (int i = 0; i < m_listTypes.GetCount(); ++i)
	{
		if (m_listTypes.GetSel(i) <= 0)
		{
			continue;
		}

		return true;
	}

	return false;
}
//-------------------------------------------------------------------------------------------------
int AddDocTypesDlg::GetPreviousComboboxIndex()
{
	if (m_strCategory.empty())
	{
		return 0;
	}

	int index = m_cmbIndustry.FindStringExact(0, m_strCategory.c_str());
	return CB_ERR == index ? 0 : index;
}
//-------------------------------------------------------------------------------------------------
std::string AddDocTypesDlg::GetDocumentClassifiersFolder()
{
	return m_initialFkbPath;
}
