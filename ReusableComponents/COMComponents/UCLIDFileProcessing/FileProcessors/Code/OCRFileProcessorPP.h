// OCRFileProcessorPP.h : Declaration of the COCRFileProcessorPP

#pragma once

#include "resource.h"       // main symbols

EXTERN_C const CLSID CLSID_OCRFileProcessorPP;

/////////////////////////////////////////////////////////////////////////////
// COCRFileProcessorPP
class ATL_NO_VTABLE COCRFileProcessorPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<COCRFileProcessorPP, &CLSID_OCRFileProcessorPP>,
	public IPropertyPageImpl<COCRFileProcessorPP>,
	public CDialogImpl<COCRFileProcessorPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	COCRFileProcessorPP();

	enum {IDD = IDD_OCRFILEPROCESSORPP};

DECLARE_REGISTRY_RESOURCEID(IDR_OCRFILEPROCESSORPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(COCRFileProcessorPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(COCRFileProcessorPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<COCRFileProcessorPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_RADIO_OCR_ALL, BN_CLICKED, OnClickedRadioAllPages)
	COMMAND_HANDLER(IDC_RADIO_OCR_SPECIFIED, BN_CLICKED, OnClickedRadioSpecificPages)
END_MSG_MAP()

// IProperty Page
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnClickedRadioAllPages(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioSpecificPages(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:
	///////////
	// Variables
	////////////
	ATLControls::CButton m_radioAllPages;
	ATLControls::CButton m_radioSpecificPages;
	ATLControls::CEdit m_editSpecificPages;
	ATLControls::CButton m_checkUseCleanImage;

	///////////
	// Methods
	///////////
	bool savePageSelections(UCLID_FILEPROCESSORSLib::IOCRFileProcessorPtr ipOCR);

	// ensure that this component is licensed
	void validateLicense();
};
