// DocumentClassifierPP.h : Declaration of the CDocumentClassifierPP

#pragma once

#include "resource.h"       // main symbols

#include <string>

EXTERN_C const CLSID CLSID_DocumentClassifierPP;

/////////////////////////////////////////////////////////////////////////////
// CDocumentClassifierPP
class ATL_NO_VTABLE CDocumentClassifierPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CDocumentClassifierPP, &CLSID_DocumentClassifierPP>,
	public IPropertyPageImpl<CDocumentClassifierPP>,
	public CDialogImpl<CDocumentClassifierPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CDocumentClassifierPP();
	~CDocumentClassifierPP();

	enum {IDD = IDD_DOCUMENTCLASSIFIERPP};

DECLARE_REGISTRY_RESOURCEID(IDR_DOCUMENTCLASSIFIERPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CDocumentClassifierPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CDocumentClassifierPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CDocumentClassifierPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
END_MSG_MAP()

// IPropertyPage
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);

private:
	/////////////
	// Variables
	/////////////
	ATLControls::CComboBox m_cmbCategoryName;
	ATLControls::CButton m_chkReRunClassifier;

	UCLID_AFUTILSLib::IAFUtilityPtr m_ipAFUtility;

	//////////
	// Methods
	//////////
	void populateComboBox(const std::string& strCurrentText);

	// ensure that this component is licensed
	void validateLicense();
};
