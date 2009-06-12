// OCRAreaPP.h : Declaration of the COCRAreaPP

#pragma once

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// COCRAreaPP
class ATL_NO_VTABLE COCRAreaPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<COCRAreaPP, &CLSID_OCRAreaPP>,
	public IPropertyPageImpl<COCRAreaPP>,
	public CDialogImpl<COCRAreaPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	COCRAreaPP();
	~COCRAreaPP();

	enum {IDD = IDD_PROPPAGE_OCRAREA};

DECLARE_REGISTRY_RESOURCEID(IDR_PROPPAGE_OCRAREA)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(COCRAreaPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(COCRAreaPP)
	COMMAND_HANDLER(IDC_OCRAREA_CHECK_CUSTOM, BN_CLICKED, OnCheckCustomFilterClicked)
	CHAIN_MSG_MAP(IPropertyPageImpl<COCRAreaPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
END_MSG_MAP()

// IPropertyPage
	STDMETHOD(Apply)();

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(/*[out, retval]*/ VARIANT_BOOL* pbValue);

// Message Handlers
	LRESULT OnCheckCustomFilterClicked(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	
private:

	//---------------------------------------------------------------------------------------------
	// Controls
	//---------------------------------------------------------------------------------------------

	// filter options
	ATLControls::CButton m_chkAlphaFilter;
	ATLControls::CButton m_chkDigitsFilter;
	ATLControls::CButton m_chkPeriodFilter;
	ATLControls::CButton m_chkHyphenFilter;
	ATLControls::CButton m_chkUnderscoreFilter;
	ATLControls::CButton m_chkCommaFilter;
	ATLControls::CButton m_chkForwardSlashFilter;
	ATLControls::CButton m_chkUnrecognizedFilter;
	ATLControls::CButton m_chkCustomFilter;
	ATLControls::CEdit m_editCustomFilterCharacters;

	// detect handwriting or printed text
	ATLControls::CButton m_radioHandwriting;
	ATLControls::CButton m_radioPrintedText;

	// clear if no text found
	ATLControls::CButton m_radioClearIfNoneFound;
	ATLControls::CButton m_radioRetainIfNoneFound;

	//---------------------------------------------------------------------------------------------
	// PROMISE: Throws an exception if OCRArea is not licensed.
	void validateLicense();
};
