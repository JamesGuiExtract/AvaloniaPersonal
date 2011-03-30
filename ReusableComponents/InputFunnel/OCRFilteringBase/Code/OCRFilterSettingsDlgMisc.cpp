// OCRFilterSettingsDlg.cpp : implementation file
//

#include "stdafx.h"
#include "OCRFilterSettingsDlg.h"

#include "ChoiceEditDlg.h"

#include <TemporaryResourceOverride.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <PromptDlg.h>

#include "OpenSaveFileDlg.h"

#include <io.h>
#include <SYS\STAT.H>

using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

extern AFX_EXTENSION_MODULE OCRFilteringBaseModule;

/////////////////////////////////////////////////////////////////////////////
// OCRFilterSettingsDlg dialog

OCRFilterSettingsDlg::OCRFilterSettingsDlg(FilterOptionsDefinitions* pFODs, CWnd* pParent /*=NULL*/)
: CDialog(OCRFilterSettingsDlg::IDD, pParent),
  m_pFODs(pFODs),
  m_pCurrentFSD(NULL),
  m_bEditFOD(true),
  m_strSelectedInputCategory(""),
  m_strCurrentFSDName(""),
  m_strPrevFSDName(""),
  m_pCurrentFOD(NULL)
{
	//{{AFX_DATA_INIT(OCRFilterSettingsDlg)
	m_strCharsAlwaysOn = _T("");
	m_chkDisableFiltering = TRUE;
	m_bAllLower = FALSE;
	m_bAllUpper = FALSE;
	m_bExactCase = FALSE;
	//}}AFX_DATA_INIT
}

OCRFilterSettingsDlg::OCRFilterSettingsDlg(FilterOptionsDefinitions* pFODs,
										   FilterSchemeDefinitions* pFSDs,
										   const string& strCurrentSchemeName,
										   CWnd* pParent /*=NULL*/)
: CDialog(OCRFilterSettingsDlg::IDD, pParent),
  m_pFODs(pFODs),
  m_pFSDs(pFSDs),
  m_pCurrentFOD(NULL),
  m_pCurrentFSD(NULL),
  m_strPrevFSDName(strCurrentSchemeName),
  m_strCurrentFSDName(strCurrentSchemeName),
  m_bEditFOD(false),
  m_strSelectedInputCategory("")
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(OCRFilteringBaseModule.hResource);

	m_strCharsAlwaysOn = _T("");

	m_chkDisableFiltering = TRUE;
	m_bAllLower = FALSE;
	m_bAllUpper = FALSE;
	m_bExactCase = FALSE;

	// if current scheme is empty, create a new one
	if (m_strCurrentFSDName.empty())
	{
		// create a brand new scheme
		m_NewFSD = FilterSchemeDefinition(m_pFODs);
		m_pCurrentFSD = &m_NewFSD;
		m_pCurrentFSD->enableFiltering(true);
		setWindowTitle("(No file name specified)");
	}
	else
	{	
		// get the current FSD
		FilterSchemeDefinitions::iterator itFSD = m_pFSDs->find(m_strCurrentFSDName);
		if (itFSD != m_pFSDs->end())
		{
			m_pCurrentFSD = &itFSD->second;
			m_strCurrentFSDName = ::getFileNameWithoutExtension(m_pCurrentFSD->getFSDFileName());
		}
	}
}

void OCRFilterSettingsDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(OCRFilterSettingsDlg)
	DDX_Control(pDX, IDC_BTN_ClearChoices, m_btnClearChoices);
	DDX_Control(pDX, IDC_BTN_RemoveAffectedInputType, m_btnRemoveAffectedInputType);
	DDX_Control(pDX, IDC_BTN_AddAffectedInputType, m_btnAddAffectedInputType);
	DDX_Control(pDX, IDC_LIST_AffectedInputTypes, m_listAffectedInputTypes);
	DDX_Control(pDX, IDC_LIST_InputCategories, m_listInputCategories);
	DDX_Control(pDX, IDC_BTN_RemoveInputCategories, m_btnRemoveInputCategories);
	DDX_Control(pDX, IDC_BTN_AddInputCategories, m_btnAddInputCategories);
	DDX_Control(pDX, IDC_CHK_Exact, m_chkExactCase);
	DDX_Control(pDX, IDC_CHK_AllUpper, m_chkAllUpper);
	DDX_Control(pDX, IDC_CHK_AllLower, m_chkAllLower);
	DDX_Control(pDX, IDC_BTN_RemoveChoice, m_btnRemoveChoice);
	DDX_Control(pDX, IDC_BTN_AddChoice, m_btnAddChoice);
	DDX_Control(pDX, IDC_EDIT_CharsAlwaysOn, m_editCharsAlwaysOn);
	DDX_Control(pDX, IDC_LIST_Choices, m_chklistChoices);
	DDX_Text(pDX, IDC_EDIT_CharsAlwaysOn, m_strCharsAlwaysOn);
	DDX_Check(pDX, IDC_CHK_EnableFiltering, m_chkDisableFiltering);
	DDX_Check(pDX, IDC_CHK_AllLower, m_bAllLower);
	DDX_Check(pDX, IDC_CHK_AllUpper, m_bAllUpper);
	DDX_Check(pDX, IDC_CHK_Exact, m_bExactCase);
	//}}AFX_DATA_MAP
}


BEGIN_MESSAGE_MAP(OCRFilterSettingsDlg, CDialog)
	//{{AFX_MSG_MAP(OCRFilterSettingsDlg)
	ON_BN_CLICKED(IDC_BTN_AddChoice, OnBTNAddChoice)
	ON_BN_CLICKED(IDC_BTN_RemoveChoice, OnBTNRemoveChoice)
	ON_BN_CLICKED(IDC_CHK_AllLower, OnCHKAllLower)
	ON_BN_CLICKED(IDC_CHK_AllUpper, OnCHKAllUpper)
	ON_BN_CLICKED(IDC_CHK_EnableFiltering, OnCHKEnableFiltering)
	ON_BN_CLICKED(IDC_CHK_Exact, OnCHKExact)
	ON_EN_CHANGE(IDC_EDIT_CharsAlwaysOn, OnChangeEDITCharsAlwaysOn)
	ON_LBN_DBLCLK(IDC_LIST_Choices, OnDblclkLISTChoices)
	ON_LBN_KILLFOCUS(IDC_LIST_Choices, OnKillfocusLISTChoices)
	ON_BN_CLICKED(IDC_BTN_AddAffectedInputType, OnBTNAddAffectedInputType)
	ON_BN_CLICKED(IDC_BTN_AddInputCategories, OnBTNAddInputCategories)
	ON_BN_CLICKED(IDC_BTN_ClearChoices, OnBTNClearChoices)
	ON_BN_CLICKED(IDC_BTN_Close, OnBTNClose)
	ON_BN_CLICKED(IDC_BTN_New, OnBTNNew)
	ON_BN_CLICKED(IDC_BTN_Open, OnBTNOpen)
	ON_BN_CLICKED(IDC_BTN_RemoveAffectedInputType, OnBTNRemoveAffectedInputType)
	ON_BN_CLICKED(IDC_BTN_RemoveInputCategories, OnBTNRemoveInputCategories)
	ON_BN_CLICKED(IDC_BTN_Save, OnBTNSave)
	ON_BN_CLICKED(IDC_BTN_SaveAs, OnBTNSaveAs)
	ON_LBN_SELCHANGE(IDC_LIST_InputCategories, OnSelchangeLISTInputCategories)
	ON_LBN_DBLCLK(IDC_LIST_InputCategories, OnDblclkLISTInputCategories)
	ON_WM_CLOSE()
	ON_LBN_DBLCLK(IDC_LIST_AffectedInputTypes, OnDblclkLISTAffectedInputTypes)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//==============================================================================================
// Public Functions
//==============================================================================================


//==============================================================================================
// Private Functions
//==============================================================================================
void OCRFilterSettingsDlg::createFOD(const string& strFODFileName, bool bCreateFile)
{
	// get the FSD Name (i.e. the name of the file without extension)
	string strFODName(::getFileNameWithoutExtension(strFODFileName));
		
	// create FSDs
	FilterOptionsDefinition newFOD(strFODFileName);
	if (bCreateFile)
	{
		// create the FOD file
		newFOD.writeToFile(strFODFileName);
	}

	int nItemIndex = m_listInputCategories.FindStringExact(-1, strFODName.c_str());
	FilterOptionsDefinitions::iterator itFOD = m_pFODs->find(strFODName);
	if (itFOD == m_pFODs->end())
	{
		// insert this new FOD to the map
		m_pFODs->insert(make_pair(strFODName, newFOD));
		// set current selection to the newly created FOD
		nItemIndex = m_listInputCategories.AddString(strFODName.c_str());
	}
	else
	{
		itFOD->second = newFOD;
	}
	
	selectInputCategoriesItem(nItemIndex);
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::createNewFOD()
{
	while (true)
	{
		// get the fod name
		string strFODName = promptForData("New FOD", "Enter the FOD name (No extension shall be provided)");
		if (strFODName.empty())
		{
			return;
		}

		// first confirm no FOD with same name already exists in m_pFODs
		if (m_pFODs->find(strFODName) != m_pFODs->end())
		{
			int nRes = MessageBox("Same FOD already exists. Do you wish to overwrite the existing one?",
				"Overwrite", MB_YESNO);
			
			if (nRes == IDNO)
			{
				return;
			}
		}
		
		// get full path in the same directory as current dll module
		string strFODFileName = getCurrentModuleFullPath() + "\\" + strFODName + ".FOD";
		
		// create FSD as well as the file
		createFOD(strFODFileName, true);

		break;
	}
}
//--------------------------------------------------------------------------------------------------
string OCRFilterSettingsDlg::getCurrentModuleFullPath()
{
	try
	{
		return getModuleDirectory(OCRFilteringBaseModule.hModule);
	}
	catch (...)
	{
		return string("");
	}
}
//--------------------------------------------------------------------------------------------------
string OCRFilterSettingsDlg::getItemChoiceID(unsigned int nItemIndex)
{
	if (m_pCurrentFOD == NULL)
	{
		UCLIDException uclidException("ELI03531", "Invalid input type exists.");
		uclidException.addDebugInfo("InputType", m_strSelectedInputCategory);
		throw uclidException;
	}

	// get the vector of all choice ids under the selected category name of current input type
	vector<string> vecChoiceIDs = m_pCurrentFOD->getInputChoiceIDs();
	if (nItemIndex < 0 || nItemIndex >= vecChoiceIDs.size())
	{
		UCLIDException uclidException("ELI03532", "Item index for choices is out of bounds.");
		uclidException.addDebugInfo("Index", nItemIndex);
		uclidException.addDebugInfo("Total number of choices", vecChoiceIDs.size());
		throw uclidException;
	}
	
	// IMPORTANT!!!
	// The choice check list box is not sorted to any order.
	// All items displayed in the check list box of the choices
	// are in the order of the squence they are added.
	return vecChoiceIDs[nItemIndex];
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::initDialogState()
{	
	if (m_bEditFOD)
	{
		// first load icon for the buttons
		m_btnAddAffectedInputType.SetIcon(::LoadIcon(OCRFilteringBaseModule.hResource, MAKEINTRESOURCE(IDI_ICON_Add)));
		m_btnRemoveAffectedInputType.SetIcon(::LoadIcon(OCRFilteringBaseModule.hResource, MAKEINTRESOURCE(IDI_ICON_Remove)));
		m_btnAddInputCategories.SetIcon(::LoadIcon(OCRFilteringBaseModule.hResource, MAKEINTRESOURCE(IDI_ICON_Add)));
		m_btnRemoveInputCategories.SetIcon(::LoadIcon(OCRFilteringBaseModule.hResource, MAKEINTRESOURCE(IDI_ICON_Remove)));
		m_btnAddChoice.SetIcon(::LoadIcon(OCRFilteringBaseModule.hResource, MAKEINTRESOURCE(IDI_ICON_Add)));
		m_btnRemoveChoice.SetIcon(::LoadIcon(OCRFilteringBaseModule.hResource, MAKEINTRESOURCE(IDI_ICON_Remove)));
	}

	int nShowWindow = m_bEditFOD? SW_SHOW : SW_HIDE;
	// Show/hide buttons
	m_btnAddAffectedInputType.ShowWindow(nShowWindow);
	m_btnRemoveAffectedInputType.ShowWindow(nShowWindow);
	m_btnAddInputCategories.ShowWindow(nShowWindow);
	m_btnRemoveInputCategories.ShowWindow(nShowWindow);
	m_btnAddChoice.ShowWindow(nShowWindow);
	m_btnRemoveChoice.ShowWindow(nShowWindow);

	// enable/disable depends on whether it's in editable mode or not
	BOOL bEnable = FALSE;
	if (m_bEditFOD)
	{
		bEnable = TRUE;
	}
	GetDlgItem(IDC_EDIT_CharsAlwaysOn)->EnableWindow(bEnable);
	GetDlgItem(IDC_BTN_New)->EnableWindow(!bEnable);
	GetDlgItem(IDC_BTN_Open)->EnableWindow(!bEnable);
	GetDlgItem(IDC_BTN_SaveAs)->EnableWindow(!bEnable);
	GetDlgItem(IDC_BTN_ClearChoices)->EnableWindow(!bEnable);
	GetDlgItem(IDC_LIST_AffectedInputTypes)->EnableWindow(bEnable);

	// Enable/disable check boxes
	GetDlgItem(IDC_CHK_EnableFiltering)->EnableWindow(!bEnable);
	m_chkAllLower.EnableWindow(!bEnable);
	m_chkAllUpper.EnableWindow(!bEnable);
	m_chkExactCase.EnableWindow(!bEnable);

	// Enable/disable current input filtering according to the current FSD
	updateDisableFilteringState();

	// update the dialog according to FODs and current FSD
	updateInputCategoriesList();
	if (m_listInputCategories.GetCount()>0)
	{
		// now selected the first item in the input categories list box.
		// Once an item is selected, the contents of the choices list box
		// will be updated accordingly
		selectInputCategoriesItem(0);
	}

	string strCurrentFSDName("");
	if (!m_bEditFOD && m_pCurrentFSD != __nullptr)
	{
		strCurrentFSDName = m_pCurrentFSD->getFSDFileName();
	}

	setWindowTitle(strCurrentFSDName);
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::inspectCurrentChoiceCheckList()
{
	if (!m_bEditFOD)
	{
		// iterate through all items of current list choice
		for (int nItemIndex = 0; nItemIndex < m_chklistChoices.GetCount() ; nItemIndex++)
		{
			// check to see if the item has or not a check
			int nCheckState = m_chklistChoices.GetCheck(nItemIndex);
			bool bCurrentChecked = (nCheckState == 1)? true : false;
			
			string strChoiceID = getItemChoiceID(nItemIndex);
			if (bCurrentChecked != m_pCurrentFSD->isInputChoiceEnabled(m_strSelectedInputCategory, strChoiceID))
			{
				// save current check box state
				m_pCurrentFSD->enableInputChoice(m_strSelectedInputCategory, strChoiceID, bCurrentChecked);
			}
		}
	}
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::modifyChoiceInfo(bool bCreateNew)
{
	if (m_pCurrentFOD)
	{
		string strChoiceID("");
		string strDescription("");
		string strChars("");
		// if it is going to modify an existing choice info
		// then fill the description and chars with existing strings
		if (!bCreateNew)
		{
			// get current selected choice id
			int nSelectedItemIndex = m_chklistChoices.GetCurSel();
			strChoiceID = getItemChoiceID(nSelectedItemIndex);
			strDescription = m_pCurrentFOD->getInputChoiceDescription(strChoiceID);
			strChars = m_pCurrentFOD->getCharsForInputChoice(strChoiceID);
		}

		// prompt for choice info
		ChoiceEditDlg choiceDlg(strDescription.c_str(), strChars.c_str(), this);
		while (true)
		{
			int nRes = choiceDlg.DoModal();
			if (nRes == IDCANCEL)
			{
				return;
			}
			
			strDescription = ::trim((LPCTSTR)choiceDlg.m_strDescription, " \t", " \t");
			if (strDescription.empty())
			{
				MessageBox("Description can't be empty.", "Invalid Entry", MB_ICONEXCLAMATION);
				continue;
			}
			
			strChars = ::trim((LPCTSTR)choiceDlg.m_strChars, " \t", " \t");
			// if strChars is empty
			if (strChars.empty())
			{
				// generate a unique set of chars from the description
				strChars = strDescription;
			}
			
			strChars = FilterOptionsDefinition::sCreateUniqueCharsSet(strChars);
			
			if (bCreateNew)
			{
				// create a new choice info and add the description to
				// the end of the choice list
				m_pCurrentFOD->addInputChoiceInfo(strDescription, strChars);
			}
			else
			{
				saveChoiceInfo(strChoiceID, strDescription, strChars);
			}

			// update the choice list
			updateChoiceCheckList(m_strSelectedInputCategory);
			
			return;
		}
	}
}
//--------------------------------------------------------------------------------------------------
string OCRFilterSettingsDlg::promptForData(const string& strTitle, 
										   const string& strPrompt,
										   const std::string& strEditString)
{
	string strData("");
	while (true)
	{
		PromptDlg promptDlg(strTitle.c_str(), strPrompt.c_str());
		promptDlg.m_zInput = strEditString.c_str();
		int nRes = promptDlg.DoModal();
		if (nRes == IDCANCEL)
		{
			return "";
		}

		strData = ::trim((LPCTSTR)promptDlg.m_zInput, " \t", " \t");
		if (strData.empty())
		{
			MessageBox("Please enter non-empty data.", "Invalid entry", MB_ICONEXCLAMATION);
			continue;
		}

		break;
	}

	return strData;
}
//--------------------------------------------------------------------------------------------------
bool OCRFilterSettingsDlg::removeFOD(const std::string& strInputCategory)
{
	FilterOptionsDefinitions::iterator itFOD = m_pFODs->find(strInputCategory);
	if (itFOD != m_pFODs->end())
	{
		string strFODFileName(itFOD->second.getFODFileName());

		if (isValidFile(strFODFileName))
		{
			// make sure the file has read/write permission
			if (isFileReadOnly(strFODFileName))
			{
				// if not, change the mode to read/write
				if (_chmod(strFODFileName.c_str(), _S_IREAD | _S_IWRITE) == -1)
				{
					CString zMsg("Failed to remove ");
					zMsg += strFODFileName.c_str();
					zMsg += ". Please make sure the file is not shared by another program.";
					// failed to change the mode
					AfxMessageBox(zMsg);
					return false;
				}
			}

			if (::DeleteFile(strFODFileName.c_str()))
			{
				// only remove the fod if the file can be deleted successfully
				m_pFODs->erase(itFOD);
				return true;
			}
		}
	}

	return false;
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::saveChoiceInfo(const string& strChoiceID, 
										  const string& strChoiceDescription,
										  const string& strChoiceChars)
{
	if (m_pCurrentFOD)
	{
		m_pCurrentFOD->setInputChoiceDescription(strChoiceID, strChoiceDescription);

		string strChars(strChoiceChars);
		// trim off leading and trailing spaces
		strChars = ::trim(strChars, " \t", " \t");
		// if strChoiceChar is empty
		if (strChars.empty())
		{
			// generate a unique set of chars from the description
			strChars = strChoiceDescription;
		}

		strChars = FilterOptionsDefinition::sCreateUniqueCharsSet(strChars);
		m_pCurrentFOD->setInputChoiceChars(strChoiceID, strChars);
	}
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::saveBeforeClose()
{
	// if current FSD is modified, then prompt for saving
	if (!m_bEditFOD)
	{
		if (!m_pCurrentFSD->isModified())
		{
			CDialog::OnOK();
			return;
		}
		
		int nRes = MessageBox("Current OCR Filter setting is changed. Do you wish to save the changes?", 
			"Save Settings", MB_YESNOCANCEL|MB_ICONQUESTION);
		if (nRes == IDYES)
		{
			if (doSave())
			{
				CDialog::OnOK();
			}
		}
		else if (nRes == IDNO)
		{
			CDialog::OnCancel();
		}			
	}
	else
	{
		// iterate through the collection of FODs to see
		// if there's any modification being made
		FilterOptionsDefinitions::iterator itFOD = m_pFODs->begin();
		for (; itFOD != m_pFODs->end(); itFOD++)
		{
			if (itFOD->second.isModified())
			{
				int nRes =MessageBox("Do you wish to save all changes before exiting?", "Save",
					MB_YESNOCANCEL | MB_ICONQUESTION);
				if (nRes == IDYES)
				{
					saveFODFiles();
					CDialog::OnOK();
					return;
				}
				else if (nRes == IDNO)
				{
					CDialog::OnCancel();
					return;
				}
				else
				{
					return;
				}
			}
		}

		CDialog::OnCancel();
	}
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::saveFODFiles()
{
	if (m_bEditFOD)
	{
		// get the FOD full path
		string strFODFilePath(getCurrentModuleFullPath() + "\\");

		// get the FOD object
		FilterOptionsDefinitions::iterator itFOD = m_pFODs->begin();
		for (; itFOD != m_pFODs->end(); itFOD++)
		{
			string strFODFileName = itFOD->first + ".FOD";
			itFOD->second.writeToFile(strFODFilePath + strFODFileName);
		}
	}
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::saveFSDFile(const std::string& strFSDFileName)
{
	// save the FSD to the file
	m_pCurrentFSD->writeToFile(strFSDFileName);

	// if the FSD hasn't been added to the FSDs, add it
	string strFSDName(::getFileNameWithoutExtension(strFSDFileName));
	FilterSchemeDefinitions::iterator itFSD = m_pFSDs->find(strFSDName);
	if (itFSD != m_pFSDs->end())
	{
		m_pFSDs->insert(make_pair(strFSDName, *m_pCurrentFSD));
	}

	string strCurrentFSDName(::getFileNameWithoutExtension(m_pCurrentFSD->getFSDFileName()));
	m_strCurrentFSDName = strCurrentFSDName;
	m_strPrevFSDName = strCurrentFSDName;
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::selectInputCategoriesItem(int nItemIndex)
{
	if (m_listInputCategories.GetCount() > 0 && nItemIndex > -1)
	{
		m_listInputCategories.SetCurSel(nItemIndex);
		// update current selected input type text as well as the 
		// current selected FOD reference
		CString cstrInputCategoriesText("");
		m_listInputCategories.GetText(nItemIndex, cstrInputCategoriesText);
		m_strSelectedInputCategory = (LPCTSTR)cstrInputCategoriesText;

		// once an item (an FOD) is selected in the input categories list
		// box, the current selected FOD (i.e. m_pCurrentFOD) needs to be updated.
		updateCurrentFOD();

		// update affected input type list display when the input
		// category selection changes
		updateAffectedInputTypeList();

		updateChoiceCheckList(m_strSelectedInputCategory);

		// get char always on
		m_strCharsAlwaysOn = m_pCurrentFOD->getCharsAlwaysEnabled().c_str();
		UpdateData(FALSE);

		// update case sensitivities
		updateCaseSensitivities();
	}
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::setWindowTitle(const string& strFileName)
{
	string strWindowTitle("FOD Editor");
	if (!m_bEditFOD)
	{
		// set dialog title to display current fsd file name
		strWindowTitle = "OCR Filter Settings -- " + 
							::getFileNameWithoutExtension(strFileName);
	}
	
	SetWindowText(strWindowTitle.c_str());
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::storeCaseSensitivities()
{
	// only if the dialog is not editable
	if (!m_bEditFOD)
	{
		UpdateData(TRUE);
		
		bool bExactCase = m_bExactCase ? true : false;
		bool bAllUpperCase = m_bAllUpper ? true : false;
		bool bAllLowerCase = m_bAllLower ? true : false;

		m_pCurrentFSD->setCaseSensitivities(m_strSelectedInputCategory, bExactCase, bAllUpperCase, bAllLowerCase);
	}
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::updateAffectedInputTypeList()
{
	m_listAffectedInputTypes.ResetContent();

	if (m_pCurrentFOD == NULL)
	{
		return;
	}

	// add affected input types
	vector<string> vecAffectedInputTypes = m_pCurrentFOD->getInputTypes();
	for (unsigned int ui = 0; ui < vecAffectedInputTypes.size(); ui++)
	{
		// Each sub string category has many choice infos
		string strInputType(vecAffectedInputTypes[ui]);
		m_listAffectedInputTypes.AddString(strInputType.c_str());	
	}

	// update the list boxes and edit box
	UpdateData(FALSE);
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::updateCaseCheckBoxStates()
{
	if (!m_bEditFOD)
	{
		UpdateData(TRUE);
		// either all upper or all lower is selected
		BOOL bUpperLowerOrSelected = m_bAllLower || m_bAllUpper;
		if (!bUpperLowerOrSelected)
		{
			// if none of the upper or lower is selected, 
			// the exact case must be selected
			m_bExactCase = TRUE;
			UpdateData(FALSE);
		}
		// if none of the check box value is true, or only Exact case
		// is true, then set Exact case checked and disabled
		m_chkExactCase.EnableWindow(bUpperLowerOrSelected);
	}
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::updateCaseSensitivities()
{
	// set case sensitivities
	// default to unchecked
	m_bExactCase = 0;
	m_bAllUpper = 0;
	m_bAllLower = 0;

	if (!m_bEditFOD && m_pCurrentFSD != __nullptr)
	{
		
		bool bExactCase, bAllUpper, bAllLower;
		m_pCurrentFSD->getCaseSensitivities(m_strSelectedInputCategory, bExactCase, bAllUpper, bAllLower);

		m_bExactCase = bExactCase?TRUE:FALSE;
		m_bAllUpper = bAllUpper?TRUE:FALSE;
		m_bAllLower = bAllLower?TRUE:FALSE;
		UpdateData(FALSE);

		updateCaseCheckBoxStates();
	}

	UpdateData(FALSE);
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::updateChoiceCheckList(const string& strInputCategory)
{
	m_chklistChoices.ResetContent();

	if (strInputCategory.empty())
	{
		return;
	}

	if (m_pCurrentFOD == NULL)
	{
		return;
	}
			
	// get all choices for this input category (i.e. FOD)
	vector<string> vecChoiceIDs = m_pCurrentFOD->getInputChoiceIDs();
	// get all choices descriptions
	vector<string> vecChoiceDescriptions 
		= m_pCurrentFOD->getInputChoiceDescriptions(vecChoiceIDs);
	// choice ids to indexes in the check list box
	for (unsigned int ui = 0; ui < vecChoiceIDs.size(); ui++)
	{
		string strChoiceID(vecChoiceIDs[ui]);
		
		// get the choice description add add it to the check list box
		m_chklistChoices.AddString(vecChoiceDescriptions[ui].c_str());

		// default to unchecked
		m_chklistChoices.SetCheck(ui, 0);

		// check/uncheck each choice if current mode is not editing FODs
		if (!m_bEditFOD && !m_pCurrentFSD->isCurrentSchemeEmpty())
		{
			if (m_pCurrentFSD->isInputChoiceEnabled(strInputCategory, strChoiceID))
			{
				// check
				m_chklistChoices.SetCheck(ui, 1);
			}
		}
	}

	// update the list boxes and edit box
	UpdateData(FALSE);

	// default to select first item
	if (m_chklistChoices.GetCount() > 0)
	{
		m_chklistChoices.SetCurSel(0);
	}
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::updateCurrentFOD()
{
	// m_strSelectedInputCategory is the FOD Name
	// Note that the m_strSelectedInputCategory must be up-to-date
	FilterOptionsDefinitions::iterator itFOD = m_pFODs->find(m_strSelectedInputCategory);
	if (itFOD != m_pFODs->end())
	{
		m_pCurrentFOD = &(itFOD->second);
	}
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::updateDisableFilteringState()
{
	if (!m_bEditFOD && m_pCurrentFSD != __nullptr)
	{
		m_chkDisableFiltering = m_pCurrentFSD->isFilteringEnabled() ? FALSE : TRUE;
		// enable/disable other controls
		m_listInputCategories.EnableWindow(!m_chkDisableFiltering);
		m_chklistChoices.EnableWindow(!m_chkDisableFiltering);
		m_chkAllLower.EnableWindow(!m_chkDisableFiltering);
		m_chkAllUpper.EnableWindow(!m_chkDisableFiltering);
		m_chkExactCase.EnableWindow(!m_chkDisableFiltering && (m_bAllLower || m_bAllUpper));
		m_btnClearChoices.EnableWindow(!m_chkDisableFiltering);

		UpdateData(FALSE);
	}
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::updateInputCategoriesList()
{
	m_listInputCategories.ResetContent();
	m_chklistChoices.ResetContent();
	m_listAffectedInputTypes.ResetContent();

	// nothing to be updated, return
	if (m_pFODs->empty())
	{
		return;
	}

	FilterOptionsDefinitions::iterator itFOD = m_pFODs->begin();
	for (; itFOD != m_pFODs->end(); itFOD++)
	{
		// get FOD name and populate the Input categories list box
		string strInputCategories(itFOD->first);
		// Add the input type first to the input type list
		m_listInputCategories.AddString(strInputCategories.c_str());
	}
	
	// update the list boxes and edit box
	UpdateData(FALSE);
}
//--------------------------------------------------------------------------------------------------
bool OCRFilterSettingsDlg::doSave()
{
	if (!m_bEditFOD)
	{
		string strCurrentFSDFileName(m_pCurrentFSD->getFSDFileName());
		// if current FSD doesn't have a name yet, i.e. this is a new FSD
		if (strCurrentFSDFileName.empty() 
			|| strCurrentFSDFileName == DEFAULT_SCHEME)
		{
			// call SaveAs
			return doSaveAs();
		}

		saveFSDFile(strCurrentFSDFileName);

		return true;
	}
	else
	{
		saveFODFiles();
	}

	return false;
}
//--------------------------------------------------------------------------------------------------
bool OCRFilterSettingsDlg::doSaveAs()
{
	if (!m_bEditFOD)
	{
		CString zOpenDirectory((getCurrentModuleFullPath() + "\\").c_str());
		CString zFileExtension("FSD");
		// show the open/saveas dialog
		OpenSaveFileDlg saveasDialog(zOpenDirectory, zFileExtension, false);
		int nRes = saveasDialog.DoModal();
		if (nRes == IDOK)
		{
			string strFSDFileName(zOpenDirectory);
			// get the FSD file name
			strFSDFileName += (LPCTSTR)saveasDialog.m_zFileName;

			setWindowTitle(strFSDFileName);

			saveFSDFile(strFSDFileName);

			return true;
		}
	}

	return false;
}
//--------------------------------------------------------------------------------------------------//--------------------------------------------------------------------------------------------------