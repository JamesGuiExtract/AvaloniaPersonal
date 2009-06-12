// RemoveSubAttributesPP.h : Declaration of the CRemoveSubAttributesPP

#pragma once

#include "resource.h"       // main symbols
#include <string>

EXTERN_C const CLSID CLSID_RemoveSubAttributesPP;

/////////////////////////////////////////////////////////////////////////////
// CRemoveSubAttributesPP
class ATL_NO_VTABLE CRemoveSubAttributesPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CRemoveSubAttributesPP, &CLSID_RemoveSubAttributesPP>,
	public IPropertyPageImpl<CRemoveSubAttributesPP>,
	public CDialogImpl<CRemoveSubAttributesPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CRemoveSubAttributesPP();

	enum {IDD = IDD_REMOVESUBATTRIBUTESPP};

DECLARE_REGISTRY_RESOURCEID(IDR_REMOVESUBATTRIBUTESPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CRemoveSubAttributesPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CRemoveSubAttributesPP)
	COMMAND_HANDLER(IDC_BUTTON_CONFIGURE_SELECTOR, BN_CLICKED, OnBnClickedButtonConfigureSelector)
	COMMAND_HANDLER(IDC_COMBO_ATTRIBUTE_SELECTOR, CBN_SELCHANGE, OnCbnSelchangeComboAttributeSelector)
	CHAIN_MSG_MAP(IPropertyPageImpl<CRemoveSubAttributesPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_BUTTON_CHOOSE_DATA_SCORER, BN_CLICKED, OnClickedChooseDataScorer)
	COMMAND_HANDLER(IDC_CHECK_REMOVE_IF_SCORE, BN_CLICKED, OnClickedRemoveIfScore)
END_MSG_MAP()
// Handler prototypes:
//  LRESULT MessageHandler(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
//  LRESULT CommandHandler(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
//  LRESULT NotifyHandler(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);

// IPropertyPage
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnClickedChooseDataScorer(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRemoveIfScore(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedButtonConfigureSelector(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);
	LRESULT OnCbnSelchangeComboAttributeSelector(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);

private:
	
	ATLControls::CComboBox m_cmbAttributeSelectors;
	ATLControls::CButton m_btnConfigure;
	ATLControls::CButton m_butChooseDataScorer;
	ATLControls::CButton m_chkRemoveIfScore;
	ATLControls::CEdit m_editDataScorerName;
	ATLControls::CComboBox m_cmbCondition;
	ATLControls::CEdit m_editScore;
	
	IObjectWithDescriptionPtr m_ipDataScorer;

	// Used to get the object map
	ICategoryManagerPtr m_ipCategoryMgr;

	// Stores association between registered Object names and ProgIDs
	UCLID_COMUTILSLib::IStrToStrMapPtr	m_ipObjectMap;

	// This is the currently selected componenet
	ICategorizedComponentPtr m_ipObject;

	///////////
	// Methods
	///////////

	void validateLicense();

	// Fills the object combo box with objects based on the currently selected object type
	void populateObjectCombo();

	// sets m_ipObject to a new instance of whatever type of object is selected in the object combo
	void createSelectedObject();

	// This will create a categorized component based on the description string
	// str name
	ICategorizedComponentPtr createObjectFromName(std::string strName);

	// Shows/Hides the configure button
	void updateConfigureButton();

	// Updates the combo box to the type of the m_ipObject if it is not null otherwise creates default of QueryBasedAS
	void updateComboSelection();

};

