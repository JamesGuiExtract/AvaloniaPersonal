// OutputToXMLPP.h : Declaration of the COutputToXMLPP

#pragma once

#include "resource.h"       // main symbols

EXTERN_C const CLSID CLSID_OutputToXMLPP;

/////////////////////////////////////////////////////////////////////////////
// COutputToXMLPP
class ATL_NO_VTABLE COutputToXMLPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<COutputToXMLPP, &CLSID_OutputToXMLPP>,
	public IPropertyPageImpl<COutputToXMLPP>,
	public CDialogImpl<COutputToXMLPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	COutputToXMLPP();

	enum {IDD = IDD_OUTPUTTOXMLPP};

DECLARE_REGISTRY_RESOURCEID(IDR_OUTPUTTOXMLPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(COutputToXMLPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(COutputToXMLPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<COutputToXMLPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_BTN_BROWSE_FILE, BN_CLICKED, OnClickedBtnBrowseFile)
	COMMAND_HANDLER(IDC_BTN_SELECT_DOC_TAG, BN_CLICKED, OnClickedSelectDocTag)
	COMMAND_HANDLER(IDC_RADIO_ORIGINAL, BN_CLICKED, OnBnClickedRadioOriginal)
	COMMAND_HANDLER(IDC_RADIO_SCHEMA, BN_CLICKED, OnBnClickedRadioSchema)
	COMMAND_HANDLER(IDC_CHECK_NAMES, BN_CLICKED, OnBnClickedCheckNames)
	COMMAND_HANDLER(IDC_CHECK_SCHEMA, BN_CLICKED, OnBnClickedCheckSchema)
END_MSG_MAP()

public:
// IPropertyPage
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnClickedBtnBrowseFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedSelectDocTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedRadioOriginal(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);
	LRESULT OnBnClickedRadioSchema(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);
	LRESULT OnBnClickedCheckNames(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);
	LRESULT OnBnClickedCheckSchema(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);

private:
	// Data members
	int		m_iXMLFormat;
	bool	m_bNamedAttributes;
	bool	m_bSchemaName;

	// controls
	ATLControls::CEdit m_editFileName;
	ATLControls::CButton m_btnBrowse;
	ATLControls::CButton m_btnSelectDocTag;
	ATLControls::CButton m_btnNames;
	ATLControls::CButton m_btnSchema;
	ATLControls::CEdit m_editSchemaName;

	// helper functions
	void validateLicense();
};
