// MicrFinderPP.h : Declaration of the CMicrFinderPP


#pragma once
#include "resource.h"       // main symbols
#include "AFValueFinders.h"

// CMicrFinderPP

class ATL_NO_VTABLE CMicrFinderPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CMicrFinderPP, &CLSID_MicrFinderPP>,
	public IPropertyPageImpl<CMicrFinderPP>,
	public CDialogImpl<CMicrFinderPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CMicrFinderPP();

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();

	void FinalRelease();

	enum {IDD = IDD_MICRFINDERPP};

DECLARE_REGISTRY_RESOURCEID(IDR_MICRFINDERPP)


BEGIN_COM_MAP(CMicrFinderPP)
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)

END_COM_MAP()

BEGIN_MSG_MAP(CMicrFinderPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CMicrFinderPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
END_MSG_MAP()

// Handler prototypes:
//  LRESULT MessageHandler(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
//  LRESULT CommandHandler(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
//  LRESULT NotifyHandler(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);

// IPropertyPage
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	// Variables

	// Control variables
	ATLControls::CButton m_checkSplitRoutingNumber;
	ATLControls::CButton m_checkSplitAccountNumber;
	ATLControls::CButton m_checkSplitCheckNumber;
	ATLControls::CButton m_checkSplitAmount;
	ATLControls::CButton m_checkRotate0;
	ATLControls::CButton m_checkRotate90;
	ATLControls::CButton m_checkRotate180;
	ATLControls::CButton m_checkRotate270;

	// Methods

	// ensure that this component is licensed
	void validateLicense();
};

OBJECT_ENTRY_AUTO(__uuidof(MicrFinderPP), CMicrFinderPP)

