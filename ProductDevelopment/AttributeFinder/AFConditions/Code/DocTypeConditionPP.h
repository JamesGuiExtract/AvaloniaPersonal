// DocTypeConditionPP.h : Declaration of the CDocTypeConditionPP

#pragma once

#include "resource.h"       // main symbols
#include <string>

EXTERN_C const CLSID CLSID_DocTypeConditionPP;

/////////////////////////////////////////////////////////////////////////////
// CDocTypeConditionPP
class ATL_NO_VTABLE CDocTypeConditionPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CDocTypeConditionPP, &CLSID_DocTypeConditionPP>,
	public IPropertyPageImpl<CDocTypeConditionPP>,
	public CDialogImpl<CDocTypeConditionPP>
{
public:
	CDocTypeConditionPP();

	enum {IDD = IDD_DOCTYPECONDITIONPP};

DECLARE_REGISTRY_RESOURCEID(IDR_DOCTYPECONDITIONPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CDocTypeConditionPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
END_COM_MAP()

BEGIN_MSG_MAP(CDocTypeConditionPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CDocTypeConditionPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_BTN_ADD_TYPES, BN_CLICKED, OnClickedBtnAddTypes)
	COMMAND_HANDLER(IDC_BTN_REMOVE_TYPES, BN_CLICKED, OnClickedBtnRemoveTypes)
	COMMAND_HANDLER(IDC_BTN_CLEAR_TYPES, BN_CLICKED, OnClickedBtnClearTypes)
	COMMAND_HANDLER(IDC_LIST_TYPES, LBN_SELCHANGE, OnSelChangeListTypes)
END_MSG_MAP()

// IPropertyPage
	STDMETHOD(Apply)(void);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnClickedBtnAddTypes(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnRemoveTypes(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnClearTypes(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnSelChangeListTypes(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:
	///////////////
	// Methods
	///////////////
	void populateComboBox();

	// Removes all selected items from the list box
	void removeSelected();

	void updateButtonTypes();

	///////////////
	// Variables
	///////////////
	ATLControls::CListBox m_listTypes;
	ATLControls::CComboBox m_cmbMatch;

	ATLControls::CButton m_btnAddTypes;
	ATLControls::CButton m_btnRemoveTypes;
	ATLControls::CButton m_btnClearTypes;

	ATLControls::CComboBox m_cmbMinProbability;

	IDocumentClassificationUtilsPtr m_ipDocUtils;

	// Industry associated with selected document types
	std::string		m_strCategory;
};
