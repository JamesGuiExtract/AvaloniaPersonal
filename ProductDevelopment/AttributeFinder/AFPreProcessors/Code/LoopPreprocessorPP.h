// LoopPreprocessorPP.h : Declaration of the CLoopPreprocessorPP


#pragma once
#include "resource.h"       // main symbols
#include "AFPreprocessors.h"

#include <string>
using namespace std;
// CLoopPreprocessorPP

class ATL_NO_VTABLE CLoopPreprocessorPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CLoopPreprocessorPP, &CLSID_LoopPreprocessorPP>,
	public IPropertyPageImpl<CLoopPreprocessorPP>,
	public CDialogImpl<CLoopPreprocessorPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CLoopPreprocessorPP();

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();

	void FinalRelease();

	enum {IDD = IDD_LOOPPREPROCESSORRPP};

DECLARE_REGISTRY_RESOURCEID(IDR_LOOPPREPROCESSORPP)

BEGIN_COM_MAP(CLoopPreprocessorPP)
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)

END_COM_MAP()

BEGIN_MSG_MAP(CLoopPreprocessorPP)
	COMMAND_HANDLER(IDC_BUTTON_CONFIGURE_PREPROCESSOR, BN_CLICKED, OnBnClickedButtonConfigurePreprocessor)
	COMMAND_HANDLER(IDC_BUTTON_CONFIGURE_CONDITION, BN_CLICKED, OnBnClickedButtonConfigureCondition)
	COMMAND_HANDLER(IDC_RADIO_DO, BN_CLICKED, OnBnClickedRadioDo)
	COMMAND_HANDLER(IDC_RADIO_WHILE, BN_CLICKED, OnBnClickedRadioWhile)
	COMMAND_HANDLER(IDC_RADIO_FOR, BN_CLICKED, OnBnClickedRadioFor)
	MESSAGE_HANDLER(WM_LBUTTONDBLCLK, OnLButtonDblClk)
	CHAIN_MSG_MAP(IPropertyPageImpl<CLoopPreprocessorPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
END_MSG_MAP()

// Handler prototypes:
//  LRESULT MessageHandler(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
//  LRESULT CommandHandler(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
//  LRESULT NotifyHandler(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnBnClickedButtonConfigurePreprocessor(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);
	LRESULT OnBnClickedButtonConfigureCondition(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);
	LRESULT OnBnClickedRadioDo(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);
	LRESULT OnBnClickedRadioWhile(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);
	LRESULT OnBnClickedRadioFor(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);
	LRESULT OnLButtonDblClk(UINT /*uMsg*/, WPARAM /*wParam*/, LPARAM /*lParam*/, BOOL& /*bHandled*/);

// IPropertyPage
	STDMETHOD(Apply)(void);
// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	// Variables

	// Control variables
	ATLControls::CButton m_radioDoLoop;
	ATLControls::CButton m_radioWhileLoop;
	ATLControls::CButton m_radioForLoop;
	ATLControls::CEdit m_editPreprocessor;
	ATLControls::CEdit m_editCondition;
	ATLControls::CEdit m_editIterations;
	ATLControls::CButton m_checkLogExceptionForMaxIterations;
	ATLControls::CComboBox m_comboConditionValue;
	ATLControls::CStatic m_staticBeginLoop;
	ATLControls::CStatic m_staticEndLoop;
	ATLControls::CStatic m_staticIterations;
	ATLControls::CStatic m_staticBeginLoopBrace;
	ATLControls::CStatic m_staticPreprocessorText;
	ATLControls::CButton m_buttonConfigureCondition;
	ATLControls::CButton m_buttonConfigurePreprocessor;
	ATLControls::CStatic m_staticLoopSetup;

	// Pointer to common MiscUtils object
	IMiscUtilsPtr m_ipMiscUtils;

	// Pointer for the Preprocessor
	IObjectWithDescriptionPtr m_ipSelectedPreprocessor;

	// Pointer for the condition
	IObjectWithDescriptionPtr m_ipSelectedCondition;

	// Methods

	// ensure that this component is licensed
	void validateLicense();
	
	// Method to get the MiscUtils object
	IMiscUtilsPtr getMiscUtils();

	// Method to change the position of the control relative the cwndPrevHorizontal.
	// The cwndCurrent will be moved to have the top be below the cwndPrevHorizontal control + nSpacing
	// and the tab order will be changed to be after the cwndPrevTab window
	void moveControlsHorizontally(ATL::CWindow &cwndPrevHorizontal, ATL::CWindow &cwndPrevTab, 
		ATL::CWindow &cwndCurrent, long nSpacing);

	// The controls before the loop need to be positioned before this is called
	// the loop controls are positioned relative to m_staticBeginLoopBrace
	void positionControlsInLoop();

	// Positions the controls for either the do or for loop
	void positionDoOrFor();

	// Positions the controls for the while loop
	void positionWhile();

	// Hides or shows the controls as appropriate for the loop type.
	void enableControls();

	// Updates the given edit control using the description in the ipSelected object
	IObjectWithDescriptionPtr updateUIForSelected(IObjectWithDescriptionPtr ipSelected, 
		ATLControls::CEdit &rEditControl);

	// Selects or configures the object that given by ipObject that should contain either no object
	// or an object that is of the category given by strAFAPICategory.  rEditControl will be 
	// updated with the description entered in the object that is configured.
	// the rButtonControl is used to determine the location of the context menu on the screen.
	void configureObject(IObjectWithDescriptionPtr &ipObject, ATLControls::CEdit &rEditControl, 
		ATLControls::CButton &rButtonControl, const string &strCategory, 
		const string &strAFAPICategory);

	// Configures the ipObject that is of the category strAFAPICategory and will update the 
	// rEditControl with the description from the updated object.
	void configureObjectForDblClick(IObjectWithDescriptionPtr &ipObject, ATLControls::CEdit &rEditControl, 
		const string &strCategory, const string &strAFAPICategory);
};

OBJECT_ENTRY_AUTO(__uuidof(LoopPreprocessorPP), CLoopPreprocessorPP)
