// SSNFinderPP.h : Declaration of the CSSNFinderPP

#pragma once

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// CSSNFinderPP
class ATL_NO_VTABLE CSSNFinderPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CSSNFinderPP, &CLSID_SSNFinderPP>,
	public IPropertyPageImpl<CSSNFinderPP>,
	public CDialogImpl<CSSNFinderPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CSSNFinderPP();
	~CSSNFinderPP();

	enum {IDD = IDD_PROPPAGE_SSNFINDER};

DECLARE_REGISTRY_RESOURCEID(IDR_PROPPAGE_SSNFINDER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CSSNFinderPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CSSNFinderPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CSSNFinderPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
END_MSG_MAP()

// IPropertyPage
	STDMETHOD(Apply)();

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(/*[out, retval]*/ VARIANT_BOOL* pbValue);

// Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);

private:

	// controls
	ATLControls::CEdit m_editSubattributeName;
	ATLControls::CButton m_chkHybridSubattribute;
	ATLControls::CButton m_chkClearIfNoneFound;

	//---------------------------------------------------------------------------------------------
	// PROMISE: Throws an exception if SSNFinder is not licensed.
	void validateLicense();
};
