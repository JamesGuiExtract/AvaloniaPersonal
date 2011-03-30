#include "stdafx.h"
#include "OCRFilterSettingsDlg.h"

#include "OpenSaveFileDlg.h"

#include <TemporaryResourceOverride.h>
#include <UCLIDException.h>
#include <cpputil.h>

#include <algorithm>

using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

extern AFX_EXTENSION_MODULE OCRFilteringBaseModule;

//--------------------------------------------------------------------------------------------------
int OCRFilterSettingsDlg::DoModal() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(OCRFilteringBaseModule.hResource);
	
	try
	{
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03508")

	return CDialog::DoModal();
}
//--------------------------------------------------------------------------------------------------
BOOL OCRFilterSettingsDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(OCRFilteringBaseModule.hResource);

	try
	{
		CDialog::OnInitDialog();
		
		// Initialize dialog state, i.e. whether the dlg is
		// opened as editable or not, and populate all list boxes
		initDialogState();

		UpdateData(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03509")
	
	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::OnOK() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(OCRFilteringBaseModule.hResource);

	try
	{
		// before close the dialog, inspect current choice list to see
		// if there's any change there
		inspectCurrentChoiceCheckList();

		// ask for whether the user wants to save
		saveBeforeClose();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03510")
	
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::OnCancel() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(OCRFilteringBaseModule.hResource);

	try
	{
		// before close the dialog, inspect current choice list to see
		// if there's any change there
		inspectCurrentChoiceCheckList();

		// ask for whether the user wants to save
		saveBeforeClose();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03545")
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::OnBTNAddAffectedInputType() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(OCRFilteringBaseModule.hResource);
	
	try
	{
		if (m_bEditFOD)
		{
			if (m_pCurrentFOD)
			{
				// prompt for input type name
				string strInputType = promptForData("Add Input Type", "New Input Type Name : ");
				if (!strInputType.empty())
				{
					m_pCurrentFOD->addInputType(strInputType);
					m_listAffectedInputTypes.AddString(strInputType.c_str());
				}
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03697")
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::OnBTNAddInputCategories() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(OCRFilteringBaseModule.hResource);
	
	try
	{
		if (m_bEditFOD)
		{
			createNewFOD();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03698")	
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::OnBTNClearChoices() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(OCRFilteringBaseModule.hResource);
	
	try
	{
		if (!m_bEditFOD)
		{
			
			int nCount = m_chklistChoices.GetCount();
			for (int n = 0; n < nCount; n++)
			{
				// clear all checks
				m_chklistChoices.SetCheck(n, 0);
			}

			// whether there's change to the choices
			inspectCurrentChoiceCheckList();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03699")	
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::OnBTNClose() 
{
	OnOK();
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::OnBTNNew() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(OCRFilteringBaseModule.hResource);
	
	try
	{
		if (!m_bEditFOD)
		{
			// if current FSD is modified, prompt for saving
			if (m_pCurrentFSD->isModified())
			{
				int nRes = MessageBox("Current filtering setting has been modified. Do you wish to save it before continue?",
					"File Modified", MB_YESNOCANCEL);
				if (nRes == IDYES)
				{
					bool bSuccess = doSave();
					if (!bSuccess)
					{
						// user might cancel out the saving part
						return;
					}
				}
				else if (nRes == IDCANCEL)
				{
					return;
				}
				// else, do not save
			}

			// create a brand new scheme
			m_NewFSD = FilterSchemeDefinition(m_pFODs);
			m_pCurrentFSD = &m_NewFSD;
			m_pCurrentFSD->enableFiltering(true);
			// reset the dialog title
			setWindowTitle("(No file name specified)");
			
			updateDisableFilteringState();

			// update the list choices
			selectInputCategoriesItem(0);
		}
		
		UpdateData(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03700")	
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::OnBTNOpen() 
{	
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(OCRFilteringBaseModule.hResource);
	
	try
	{
		if (!m_bEditFOD)
		{
			// if current FSD is modified, prompt for saving
			if (m_pCurrentFSD->isModified())
			{
				int nRes = MessageBox("Current filtering setting has been modified. Do you wish to save it before continue?",
										"File Modified", MB_YESNOCANCEL);
				if (nRes == IDYES)
				{
					bool bSuccess = doSave();
					if (!bSuccess)
					{
						// user might cancel out the saving part
						return;
					}
				}
				else if (nRes == IDCANCEL)
				{
					return;
				}
				// else, do not save
			}

			CString zOpenDirectory((getCurrentModuleFullPath() + "\\").c_str());
			CString zFileExtension("FSD");
			// show the open/saveas dialog
			OpenSaveFileDlg openDialog(zOpenDirectory, zFileExtension);
			int nRes = openDialog.DoModal();
			if (nRes == IDOK)
			{
				string strFSDName(::getFileNameWithoutExtension((LPCTSTR)openDialog.m_zFileName));
				FilterSchemeDefinitions::iterator itFSD = m_pFSDs->find(strFSDName);
				int n = m_pFSDs->size();
				if (itFSD == m_pFSDs->end())
				{
					UCLIDException uclidException("ELI03696", "Failed to open the scheme.");
					uclidException.addDebugInfo("FSD file name", strFSDName);
					throw uclidException;
				}

				m_pCurrentFSD = &itFSD->second;

				m_strCurrentFSDName = strFSDName;
				m_strPrevFSDName = strFSDName;

				updateDisableFilteringState();

				// refresh the display
				selectInputCategoriesItem(0);
				// reset the dialog title
				setWindowTitle(strFSDName);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03701")
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::OnBTNRemoveAffectedInputType() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(OCRFilteringBaseModule.hResource);
	
	try
	{
		if (m_bEditFOD)
		{
			// get current selected item
			int nItemToRemove = m_listAffectedInputTypes.GetCurSel();
			if (nItemToRemove != LB_ERR)
			{
				int nRes = MessageBox("Remove this Input Type?", "Remove", MB_YESNO | MB_ICONWARNING);
				if (nRes == IDNO)
				{
					return;
				}

				CString cstrInputType("");
				m_listAffectedInputTypes.GetText(nItemToRemove, cstrInputType);
				if (!cstrInputType.IsEmpty())
				{
					m_pCurrentFOD->removeInputType((LPCTSTR)cstrInputType);
					m_listAffectedInputTypes.DeleteString(nItemToRemove);
				}
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03702")	
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::OnBTNRemoveInputCategories() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(OCRFilteringBaseModule.hResource);
	
	try
	{
		if (m_bEditFOD)
		{
			// removes currently selected input type
			int nItemToRemove = m_listInputCategories.GetCurSel();
			if (nItemToRemove == LB_ERR)
			{
				// if there's no more item to be removed
				m_strSelectedInputCategory = "";
				return;
			}

			int nRes = MessageBox("If you delete this category, its associated FOD file will be deleted permanently. Do you wish to continue?", 
				"Remove", MB_YESNO | MB_ICONWARNING);
			if (nRes == IDNO)
			{
				return;
			}

			string strInputCategory(m_strSelectedInputCategory);
			// remove the FOD from the map
			if (!removeFOD(strInputCategory))
			{
				return;
			}
			
			int nTotalNumRemain = m_listInputCategories.DeleteString(nItemToRemove);
							
			// select the string if any below the deleted item
			if (nTotalNumRemain > nItemToRemove)
			{
				selectInputCategoriesItem(nItemToRemove);
			}
			else
			{
				// no more input category left
				// set m_pCurrentFOD to null and clear all list box and edit box
				m_strSelectedInputCategory = "";
				m_pCurrentFOD = NULL;
				m_listInputCategories.ResetContent();
				m_chklistChoices.ResetContent();
				m_listAffectedInputTypes.ResetContent();
				UpdateData(FALSE);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03703")
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::OnBTNSave() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(OCRFilteringBaseModule.hResource);

	try
	{
		doSave();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03704")

	return;
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::OnBTNSaveAs() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(OCRFilteringBaseModule.hResource);
	
	try
	{
		doSaveAs();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03705")

	return;
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::OnBTNAddChoice() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(OCRFilteringBaseModule.hResource);
	
	try
	{
		if (m_bEditFOD)
		{
			modifyChoiceInfo(true);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03515")
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::OnBTNRemoveChoice() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(OCRFilteringBaseModule.hResource);
	
	try
	{
		if (m_bEditFOD)
		{
			if (m_pCurrentFOD != __nullptr && m_chklistChoices.GetCount() > 0)
			{
				int nRes = MessageBox("Remove this Sub String Choice?", "Remove", MB_YESNO | MB_ICONWARNING);
				if (nRes == IDNO)
				{
					return;
				}
				
				// removes currently selected choice
				int nItemToRemove = m_chklistChoices.GetCurSel();
				if (nItemToRemove != LB_ERR)
				{
					// get this item choice id
					string strChoiceID(getItemChoiceID(nItemToRemove));
					// remove the string from the choice list
					int nTotalNumRemain = m_chklistChoices.DeleteString(nItemToRemove);
					// remove the choice from the FOD
					m_pCurrentFOD->removeInputChoiceInfo(strChoiceID);
					
					// select the string if any below the deleted item
					if (nTotalNumRemain > nItemToRemove)
					{
						m_chklistChoices.SetCurSel(nItemToRemove);
					}
				}
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03518")
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::OnCHKAllLower() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(OCRFilteringBaseModule.hResource);
	
	try
	{
		if (!m_bEditFOD)
		{
			storeCaseSensitivities();
			updateCaseCheckBoxStates();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03520")
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::OnCHKAllUpper() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(OCRFilteringBaseModule.hResource);
	
	try
	{
		if (!m_bEditFOD)
		{
			storeCaseSensitivities();
			updateCaseCheckBoxStates();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03521")
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::OnCHKEnableFiltering() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(OCRFilteringBaseModule.hResource);
	
	try
	{
		if (!m_bEditFOD)
		{
			UpdateData(TRUE);
			m_pCurrentFSD->enableFiltering(m_chkDisableFiltering ? false : true);

			updateDisableFilteringState();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03522")
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::OnCHKExact() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(OCRFilteringBaseModule.hResource);
	
	try
	{
		if (!m_bEditFOD)
		{
			storeCaseSensitivities();
			updateCaseCheckBoxStates();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03523")
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::OnChangeEDITCharsAlwaysOn() 
{	
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(OCRFilteringBaseModule.hResource);
	
	try
	{
		// If it's not in edit FOD mode this edit box will be enabled
		if (m_bEditFOD)
		{
			UpdateData(TRUE);

			// set chars always enabled
			if (m_pCurrentFOD)
			{
				m_pCurrentFOD->setCharsAlwaysEnabled((LPCTSTR)m_strCharsAlwaysOn);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03524")
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::OnDblclkLISTAffectedInputTypes() 
{	
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(OCRFilteringBaseModule.hResource);
	
	try
	{
		if (m_bEditFOD)
		{
			int nItemToBeModified = m_listAffectedInputTypes.GetCurSel();
			if (nItemToBeModified != LB_ERR)
			{
				CString cstrInputType("");
				m_listAffectedInputTypes.GetText(nItemToBeModified, cstrInputType);
				string strOldInputTypeName(cstrInputType);
				string strNewInputTypeName = promptForData("Modify", "Input Type:", strOldInputTypeName);
				if (!strNewInputTypeName.empty() 
					&& _stricmp(strNewInputTypeName.c_str(), strOldInputTypeName.c_str()) != 0)
				{
					// add the new input type name and delete the old input type name
					m_pCurrentFOD->addInputType(strNewInputTypeName);
					m_pCurrentFOD->removeInputType(strOldInputTypeName);
					updateAffectedInputTypeList();
				}
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03767")
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::OnDblclkLISTChoices() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(OCRFilteringBaseModule.hResource);
	
	try
	{
		if (m_bEditFOD)
		{
			modifyChoiceInfo();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03528")
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::OnKillfocusLISTChoices() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(OCRFilteringBaseModule.hResource);
	
	try
	{
		if (!m_bEditFOD)
		{
			// before kill the focus on current list choice,
			// inspect current choice list to see if there's any change there
			inspectCurrentChoiceCheckList();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03527")	
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::OnSelchangeLISTInputCategories() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(OCRFilteringBaseModule.hResource);
	
	try
	{
		// get current selected item index
		int nCurrentSelectedIndex = m_listInputCategories.GetCurSel();
		if (nCurrentSelectedIndex != LB_ERR)
		{
			selectInputCategoriesItem(nCurrentSelectedIndex);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03529")
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSettingsDlg::OnDblclkLISTInputCategories() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(OCRFilteringBaseModule.hResource);
	
	try
	{
		if (m_bEditFOD)
		{
			string strModifiedInputCategory(m_strSelectedInputCategory);
			strModifiedInputCategory = promptForData("Modify", "Input Category:", strModifiedInputCategory);
			if (strModifiedInputCategory.empty() 
				|| _stricmp(strModifiedInputCategory.c_str(), m_strSelectedInputCategory.c_str()) == 0)
			{
				return;
			}

			// Add the modified input type to the FODs,
			// and remove the orginal input type from FODs
			string strInputCategoryToBeRemoved(m_strSelectedInputCategory);
			FilterOptionsDefinitions::iterator itFOD = m_pFODs->find(strInputCategoryToBeRemoved);
			if (itFOD != m_pFODs->end())
			{
				// make a copy of the FOD
				FilterOptionsDefinition tempFOD(itFOD->second);
				// if failed to remove the old FOD, just return
				if (!removeFOD(strInputCategoryToBeRemoved))
				{
					return;
				}

				// create FOD file
				string strFODFile = getCurrentModuleFullPath() + "\\" + strModifiedInputCategory + ".FOD";
				tempFOD.writeToFile(strFODFile);
				// insert it into the FODs
				m_pFODs->insert(make_pair(strModifiedInputCategory, tempFOD));
				updateInputCategoriesList();
				// find the modified item index in the input type list
				int nIndex = m_listInputCategories.FindStringExact(-1, strModifiedInputCategory.c_str());
				if (nIndex != LB_ERR)
				{
					selectInputCategoriesItem(nIndex);
				}
			}
			else
			{
				UCLIDException uclidException("ELI03560", "Unable to modify current input type.");
				uclidException.addDebugInfo("Input Category", strInputCategoryToBeRemoved);
				throw uclidException;
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03530")	
}
