// LoopPreprocessorPP.cpp : Implementation of CLoopPreprocessorPP

#include "stdafx.h"
#include "LoopPreprocessorPP.h"
#include <UCLIDException.h>
#include "..\..\AFCore\Code\AFCategories.h"

#include <cpputil.h>
#include <comutils.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include <SuspendWindowupdates.h>
#include <RequiredInterfaces.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const int giLINE_SPACING = 6;

//-------------------------------------------------------------------------------------------------
// CLoopPreprocessorPP
//-------------------------------------------------------------------------------------------------
CLoopPreprocessorPP::CLoopPreprocessorPP()
{
	try
	{
		// Check licensing
		validateLicense();

		m_dwTitleID = IDS_TITLELoopPreProcessorPP;
		m_dwHelpFileID = IDS_HELPFILELoopPreProcessorPP;
		m_dwDocStringID = IDS_DOCSTRINGLoopPreProcessorPP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI24165");
}
//-------------------------------------------------------------------------------------------------
HRESULT CLoopPreprocessorPP::FinalConstruct()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
void CLoopPreprocessorPP::FinalRelease()
{
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLoopPreprocessorPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ATLTRACE(_T("CLoopPreprocessorPP::Apply\n"));
		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_AFPREPROCESSORSLib::ILoopPreprocessorPtr ipLoopPreprocessor = m_ppUnk[0];
			if (ipLoopPreprocessor)
			{
				UCLID_AFPREPROCESSORSLib::ELoopType eLoopType;

				// Get the Loop type
				if (m_radioDoLoop.GetCheck() == BST_CHECKED)
				{
					eLoopType = UCLID_AFPREPROCESSORSLib::kDoLoop;
				}
				else if ( m_radioWhileLoop.GetCheck() == BST_CHECKED)
				{
					eLoopType = UCLID_AFPREPROCESSORSLib::kWhileLoop;
				}
				else if ( m_radioForLoop.GetCheck() == BST_CHECKED)
				{
					eLoopType = UCLID_AFPREPROCESSORSLib::kForLoop;
				}

				// Set the Preprocessor if a Preprocessor was selected
				if (m_ipSelectedPreprocessor != NULL)
				{
					ipLoopPreprocessor->Preprocessor = m_ipSelectedPreprocessor;;
				}
				else
				{
					// Set focus to the Preprocessor command button
					m_buttonConfigurePreprocessor.SetFocus();

					// Throw exception
					UCLIDException ue("ELI24331", "Preprocessor must be specified!");
					throw ue;
				}

				// Set the Condition if a Condition was selected
				if (m_ipSelectedCondition != NULL)
				{
					ipLoopPreprocessor->Condition = m_ipSelectedCondition;
				}
				else if ( eLoopType != kForLoop )
				{
					// Set focus to the Condition command button
					m_buttonConfigureCondition.SetFocus();
					
					// Throw exception
					UCLIDException ue("ELI24332", "Condition must be specified!");
					throw ue;
				}
				
				// Set focus and select the contents of the edit box.
				m_editIterations.SetFocus();
				m_editIterations.SetSel(0,-1);

				// Get the number of iterations
				CString zIterations;
				m_editIterations.GetWindowTextA(zIterations);
				ipLoopPreprocessor->Iterations = asLong(string(zIterations));

				// Get the Log Exception flag
				ipLoopPreprocessor->LogExceptionForMaxIterations = 
					asVariantBool(m_checkLogExceptionForMaxIterations.GetCheck() == BST_CHECKED);

				// Get the Condition value
				CString zValue;
				m_comboConditionValue.GetWindowTextA(zValue);
				ipLoopPreprocessor->ConditionValue = asVariantBool(zValue != "False");
				
				// Set the loop type. This is done last so that the condition will be reset
				ipLoopPreprocessor->LoopType = eLoopType;
			}
		}
		m_bDirty = FALSE;

		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24166");

	// Return false because exception occured and need to let the caller know the
	// apply was unsuccessful.
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CLoopPreprocessorPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, 
									 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

		try
	{
		if (m_nObjects > 0)
		{
			UCLID_AFPREPROCESSORSLib::ILoopPreprocessorPtr ipLoopPreprocessor = m_ppUnk[0];
			if (ipLoopPreprocessor)
			{
				// Set up controls
				m_editPreprocessor = GetDlgItem(IDC_EDIT_PREPROCESSOR);
				m_editCondition = GetDlgItem(IDC_EDIT_CONDITION);
				m_editIterations = GetDlgItem(IDC_EDIT_ITERATIONS);
				m_radioDoLoop = GetDlgItem(IDC_RADIO_DO);
				m_radioDoLoop.SetCheck(BST_CHECKED);
				m_radioWhileLoop = GetDlgItem(IDC_RADIO_WHILE);
				m_radioForLoop = GetDlgItem(IDC_RADIO_FOR);
				m_checkLogExceptionForMaxIterations = GetDlgItem(IDC_CHECK_LOG_EXCEPTION);
				m_comboConditionValue = GetDlgItem(IDC_COMBO_CONDITION_VALUE);
				m_staticBeginLoop = GetDlgItem(IDC_STATIC_BEGIN_LOOP_TEXT);
				m_staticEndLoop = GetDlgItem(IDC_STATIC_END_LOOP_TEXT);
				m_staticIterations = GetDlgItem(IDC_STATIC_ITERATION_TEXT);
				m_buttonConfigureCondition = GetDlgItem(IDC_BUTTON_CONFIGURE_CONDITION);
				m_buttonConfigurePreprocessor = GetDlgItem(IDC_BUTTON_CONFIGURE_PREPROCESSOR);
				m_staticBeginLoopBrace = GetDlgItem(IDC_STATIC_BEGIN_LOOP_BRACE);
				m_staticPreprocessorText = GetDlgItem(IDC_STATIC_PRE_PROCESSOR);
				m_staticLoopSetup = GetDlgItem(IDC_STATIC_LOOP_SETUP);

				// Set the Preprocessor and initialize the description if it exists
				m_ipSelectedPreprocessor = ipLoopPreprocessor->Preprocessor;
				if ( m_ipSelectedPreprocessor != NULL )
				{
					m_editPreprocessor.SetWindowTextA(m_ipSelectedPreprocessor->Description);
				}

				// Set the Condition and initialize the description if it exists
				m_ipSelectedCondition = ipLoopPreprocessor->Condition;
				if ( m_ipSelectedCondition != NULL )
				{
					m_editCondition.SetWindowTextA(m_ipSelectedCondition->Description);
				}

				// Set the number of iterations
				m_editIterations.SetWindowTextA(asString(ipLoopPreprocessor->Iterations).c_str());

				// Set the Log exception flag
				m_checkLogExceptionForMaxIterations.
					SetCheck(asBSTChecked(ipLoopPreprocessor->LogExceptionForMaxIterations));

				// Select the loop Type
				switch (ipLoopPreprocessor->LoopType)
				{
				case kDoLoop:
					m_radioDoLoop.SetCheck(BST_CHECKED);
					m_radioWhileLoop.SetCheck(BST_UNCHECKED);
					m_radioForLoop.SetCheck(BST_UNCHECKED);
					break;
				case kWhileLoop:
					m_radioWhileLoop.SetCheck(BST_CHECKED);
					m_radioDoLoop.SetCheck(BST_UNCHECKED);
					m_radioForLoop.SetCheck(BST_UNCHECKED);
					break;
				case kForLoop:
					m_radioForLoop.SetCheck(BST_CHECKED);
					m_radioDoLoop.SetCheck(BST_UNCHECKED);
					m_radioWhileLoop.SetCheck(BST_UNCHECKED);
					break;
				}

				// Set up the combo box for Conditional value
				m_comboConditionValue.AddString("True");
				m_comboConditionValue.AddString("False");

				// Select the condition value
				if (asCppBool(ipLoopPreprocessor->ConditionValue))
				{
					m_comboConditionValue.SelectString(-1, "True");
				}
				else
				{
					m_comboConditionValue.SelectString(-1,"False");
				}
			}
		}			
		SetDirty(FALSE);
		enableControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24167");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CLoopPreprocessorPP::OnBnClickedButtonConfigurePreprocessor(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	try
	{
		configureObject(m_ipSelectedPreprocessor, m_editPreprocessor, m_buttonConfigurePreprocessor,
			"Preprocessor", AFAPI_DOCUMENT_PREPROCESSORS_CATEGORYNAME);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24170");
	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CLoopPreprocessorPP::OnBnClickedButtonConfigureCondition(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	try
	{
		// Create a temporary object for the condition
		configureObject(m_ipSelectedCondition, m_editCondition, m_buttonConfigureCondition,
			"Condition", AFAPI_CONDITIONS_CATEGORYNAME);

	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24173");
	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CLoopPreprocessorPP::OnBnClickedRadioDo(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	try
	{
		enableControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24174");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CLoopPreprocessorPP::OnBnClickedRadioWhile(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	try
	{
		enableControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24175");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CLoopPreprocessorPP::OnBnClickedRadioFor(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	try
	{
		enableControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24176");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CLoopPreprocessorPP::OnLButtonDblClk(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	try
	{
		// Handling the Left button dbl click on the property page was implemented instead
		// of having the different methods for the controls, to fix the issue with double click 
		// copying the label contents to the clipboard FlexIDSCore #4227

		// Get the window ID that the mouse is in
		POINT pointMouse;
		pointMouse.x = GET_X_LPARAM(lParam); 
		pointMouse.y = GET_Y_LPARAM(lParam); 
		int iID = ChildWindowFromPointEx(pointMouse,CWP_SKIPTRANSPARENT).GetDlgCtrlID();
		
		// If the mouse was double clicked in preprocessor or condition - configure
		if (iID == IDC_EDIT_PREPROCESSOR )
		{
			configureObjectForDblClick(m_ipSelectedPreprocessor, m_editPreprocessor, "Preprocessor", 
				AFAPI_DOCUMENT_PREPROCESSORS_CATEGORYNAME);
			bHandled = TRUE;
		}
		else if (iID == IDC_EDIT_CONDITION)
		{
			configureObjectForDblClick(m_ipSelectedCondition, m_editCondition, "Condition", 
				AFAPI_CONDITIONS_CATEGORYNAME);
			bHandled = TRUE;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29979");
	return 0;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLoopPreprocessorPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		if (pbValue == NULL)
		{
			return E_POINTER;
		}

		// Check license
		validateLicense();

		// If no exception, then pbValue is true
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CLoopPreprocessorPP::validateLicense()
{
	// Property Page requires Editor license
	VALIDATE_LICENSE( gnRULESET_EDITOR_UI_OBJECT, "ELI24177", 
		"Loop Preprocessor PP" );
}
//-------------------------------------------------------------------------------------------------
IMiscUtilsPtr CLoopPreprocessorPP::getMiscUtils()
{
	// check if a MiscUtils object has all ready been created
	if (m_ipMiscUtils == NULL)
	{
		// create MiscUtils object
		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI24178", m_ipMiscUtils != NULL);
	}

	return m_ipMiscUtils;
}
//-------------------------------------------------------------------------------------------------
void CLoopPreprocessorPP::moveControlsHorizontally(ATL::CWindow &cwndPrevHorizontal, ATL::CWindow &cwndPrevTab, 
		ATL::CWindow &cwndCurrent, long nSpacing)
{
	// Get the window rect of the previous horizontal control
	CRect rectPrevHorizontal;
	cwndPrevHorizontal.GetWindowRect(&rectPrevHorizontal);

	// Get the rect for the current control to reposition
	CRect rect;
	cwndCurrent.GetWindowRect(&rect);

	// Get the height of the current control
	long nHeight;
	nHeight = rect.Height();

	// Reset the top of the current control position to the bottom of the previous control
	// + the required spacing
	rect.top = rectPrevHorizontal.bottom + nSpacing;

	// Maintain the original height of the current control
	rect.bottom = rect.top + nHeight;

	// Convert to client coordinates
	ScreenToClient(&rect);

	// Reset the window position and set the tab position after the wndPrevTab control
	cwndCurrent.SetWindowPos(cwndPrevTab, &rect, 0);
}
//-------------------------------------------------------------------------------------------------
void CLoopPreprocessorPP::positionControlsInLoop()
{
	// Position the controls within the loop relative to the begin loop brace
	moveControlsHorizontally(m_staticBeginLoopBrace, m_staticBeginLoopBrace, m_staticPreprocessorText, giLINE_SPACING);
	moveControlsHorizontally(m_staticPreprocessorText, m_staticPreprocessorText, m_editPreprocessor, giLINE_SPACING);
	moveControlsHorizontally(m_staticPreprocessorText, m_editPreprocessor, m_buttonConfigurePreprocessor, giLINE_SPACING);
	moveControlsHorizontally(m_editPreprocessor, m_buttonConfigurePreprocessor, m_staticEndLoop, giLINE_SPACING);
}
//-------------------------------------------------------------------------------------------------
void CLoopPreprocessorPP::positionDoOrFor()
{
	// Don't redraw until controls have been repositioned
	LockWindowUpdate();

	// Position the begin loop brace to be after the Begin loop static control
	moveControlsHorizontally(m_staticBeginLoop, m_staticBeginLoop, m_staticBeginLoopBrace, giLINE_SPACING);

	positionControlsInLoop();

	// Position the Condition value after the End loop static control
	moveControlsHorizontally(m_editPreprocessor, m_staticEndLoop, m_comboConditionValue, giLINE_SPACING);

	// Position the Condition after the Condition value control
	moveControlsHorizontally(m_comboConditionValue, m_comboConditionValue, m_editCondition, giLINE_SPACING);

	// Position the Configure condition button beside the Condition
	moveControlsHorizontally(m_comboConditionValue, m_editCondition, m_buttonConfigureCondition, giLINE_SPACING);

	// Get the rect for the Loop Setup group box
	CRect rect;
	m_staticLoopSetup.GetWindowRect(&rect);

	// Convert to client coordinates;
	ScreenToClient(&rect);

	// Invalidate the area of the Loop setup group box
	InvalidateRect(&rect);

	// Allow window update
	LockWindowUpdate(FALSE);
}
//-------------------------------------------------------------------------------------------------
void CLoopPreprocessorPP::positionWhile()
{
	// Don't redraw until controls have been repositioned
	LockWindowUpdate();
	
	// Get the rect for the Condition Value
	CRect rect;
	m_comboConditionValue.GetWindowRect(&rect);

	// Set the position of the condition value beside the Begin loop by using the spacing - height
	moveControlsHorizontally(m_staticBeginLoop, m_staticBeginLoop, m_comboConditionValue, -rect.Height());

	// Position the Condition below the condition Value control
	moveControlsHorizontally(m_comboConditionValue, m_comboConditionValue, m_editCondition, giLINE_SPACING);

	// Position the Configure Condition control beside the condition value control.
	moveControlsHorizontally(m_comboConditionValue, m_editCondition, m_buttonConfigureCondition, giLINE_SPACING);

	// Position the begin loop brace below the condition control
	moveControlsHorizontally(m_editCondition, m_buttonConfigureCondition, m_staticBeginLoopBrace, giLINE_SPACING);

	positionControlsInLoop();

	// Get the rect for the Loop Setup group box
	m_staticLoopSetup.GetWindowRect(&rect);
	
	// Convert to client coordinates;
	ScreenToClient(&rect);
	
	// Invalidate the area of the Loop setup group box
	InvalidateRect(&rect);

	// Allow window update
	LockWindowUpdate(FALSE);
}
//-------------------------------------------------------------------------------------------------
void CLoopPreprocessorPP::enableControls()
{
	// Set default visibility to show condition objects
	int iShowOrHide = SW_SHOW;

	// If do loop
	if (m_radioDoLoop.GetCheck() == BST_CHECKED)
	{
		// Set do loop text
		m_staticBeginLoop.SetWindowTextA("Do");
		m_staticEndLoop.SetWindowTextA("} while this condition is ");
		m_staticIterations.SetWindowTextA("Maximum number of iterations ");

		// Position controls
		positionDoOrFor();
	}
	else if ( m_radioWhileLoop.GetCheck() == BST_CHECKED)
	{
		// Set while loop text
		m_staticBeginLoop.SetWindowTextA("While this condition is ");
		m_staticEndLoop.SetWindowTextA("}");
		m_staticIterations.SetWindowTextA("Maximum number of iterations ");

		// Position controls
		positionWhile();
	}
	else if ( m_radioForLoop.GetCheck() == BST_CHECKED)
	{
		// Set for loop text
		m_staticBeginLoop.SetWindowTextA("For ");
		m_staticEndLoop.SetWindowTextA("}");
		m_staticIterations.SetWindowTextA("Number of iterations ");
		
		// Set the visiblity flag for condition objects to Hide
		iShowOrHide = SW_HIDE;

		// Position controls
		positionDoOrFor();
	}

	// Set the visibility of the condition objects
	m_comboConditionValue.ShowWindow(iShowOrHide);
	m_editCondition.ShowWindow(iShowOrHide);
	m_buttonConfigureCondition.ShowWindow(iShowOrHide);

	// Enable or disable the log Exception based on the state of the condition visiblity
	m_checkLogExceptionForMaxIterations.EnableWindow(iShowOrHide == SW_HIDE ? FALSE:TRUE);

	// Refresh the form so the button states are displayed correctly PVCS FlexIDSCore #4259
	Invalidate();
}
//-------------------------------------------------------------------------------------------------
IObjectWithDescriptionPtr CLoopPreprocessorPP::updateUIForSelected(IObjectWithDescriptionPtr ipSelected, 
												  ATLControls::CEdit &rEditControl)
{
	// Convert selected object to Categorized component to get description
	ICategorizedComponentPtr ipCategoryObj = ipSelected->Object;
	ASSERT_RESOURCE_ALLOCATION("ELI24774", ipCategoryObj != NULL);

	// Set the description for the object.
	rEditControl.SetWindowTextA(ipSelected->Description);

	// Set the new preprocessor
	return ipSelected;
}
//-------------------------------------------------------------------------------------------------
void CLoopPreprocessorPP::configureObject(IObjectWithDescriptionPtr &ipObject, ATLControls::CEdit &rEditControl,
								   ATLControls::CButton &rButtonControl, const string &strCategory, 
								   const string &strAFAPICategory)
{
	// Create a temporary object and set to object to configure
	IObjectWithDescriptionPtr ipSelectedObject = ipObject;

	// If there is no selected object create it
	if ( ipSelectedObject == NULL)
	{
		ipSelectedObject.CreateInstance(CLSID_ObjectWithDescription);
		ASSERT_RESOURCE_ALLOCATION("ELI24775", ipSelectedObject != NULL);
	}

	// Get the configure button rect for positioning the Select menu
	CRect rect;
	rButtonControl.GetWindowRect(&rect);

	// allow the user to select and configure object
	VARIANT_BOOL vbDirty = getMiscUtils()->HandlePlugInObjectCommandButtonClick(ipSelectedObject, 
		strCategory.c_str(), strAFAPICategory.c_str(), VARIANT_FALSE, 
		gRequiredInterfaces.ulCount, gRequiredInterfaces.pIIDs, rect.right, rect.top);

	// If there was a selection 
	if (asCppBool(vbDirty))
	{
		ipObject = updateUIForSelected(ipSelectedObject, rEditControl);
	}
}
//-------------------------------------------------------------------------------------------------
void CLoopPreprocessorPP::configureObjectForDblClick(IObjectWithDescriptionPtr &ipObject, ATLControls::CEdit &rEditControl, 
									const string &strCategory, const string &strAFAPICategory)
{
	// Create a temporary object and set to object to configure
	IObjectWithDescriptionPtr ipSelectedObject = ipObject;

	// If there is no selected object create it
	if ( ipSelectedObject == NULL)
	{
		ipSelectedObject.CreateInstance(CLSID_ObjectWithDescription);
		ASSERT_RESOURCE_ALLOCATION("ELI24776", ipSelectedObject != NULL);
	}

	// allow the user to select and configure object
	VARIANT_BOOL vbDirty = getMiscUtils()->HandlePlugInObjectDoubleClick(ipSelectedObject,
		strCategory.c_str(), strAFAPICategory.c_str(), VARIANT_FALSE, 
		gRequiredInterfaces.ulCount, gRequiredInterfaces.pIIDs);

	// check if GlobalOutputHandler was changed
	if (asCppBool(vbDirty))
	{
		ipObject = updateUIForSelected(ipSelectedObject, rEditControl);
	}
}
//-------------------------------------------------------------------------------------------------
