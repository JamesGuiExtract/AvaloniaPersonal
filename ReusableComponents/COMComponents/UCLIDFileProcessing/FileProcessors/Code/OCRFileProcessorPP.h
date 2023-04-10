// OCRFileProcessorPP.h : Declaration of the COCRFileProcessorPP

#pragma once

#include "resource.h"       // main symbols

#include <ImageButtonWithStyle.h>
#include <string>

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
	COMMAND_HANDLER(IDC_BTN_OCRPARAMETERS, BN_CLICKED, OnBnClickedBtnOcrparameters)
	COMMAND_HANDLER(IDC_BTN_OCR_PARAMETERS_RULESET_DOC_TAG, BN_CLICKED, OnBnClickedBtnOcrParametersRulesetDocTag)
	COMMAND_HANDLER(IDC_BTN_BROWSE_OCR_PARAMETERS_RULESET, BN_CLICKED, OnBnClickedBtnBrowseOcrParametersRuleset)
	COMMAND_HANDLER(IDC_RADIO_NO_OCR_PARAMS, BN_CLICKED, OnBnClickedRadioNoOcrParams)
	COMMAND_HANDLER(IDC_RADIO_OCR_PARAMS_HERE, BN_CLICKED, OnBnClickedRadioOcrParamsHere)
	COMMAND_HANDLER(IDC_RADIO_OCR_PARAMS_FROM_RULESET, BN_CLICKED, OnBnClickedRadioOcrParamsFromRuleset)
	COMMAND_HANDLER(IDC_OCR_ENGINE_COMBO, CBN_SELCHANGE, OnCbnSelchangeOcrEngineCombo)
END_MSG_MAP()

// IProperty Page
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnClickedRadioAllPages(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioSpecificPages(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedBtnOcrparameters(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);
	LRESULT OnBnClickedBtnOcrParametersRulesetDocTag(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);
	LRESULT OnBnClickedBtnBrowseOcrParametersRuleset(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);
	LRESULT OnBnClickedRadioNoOcrParams(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);
	LRESULT OnBnClickedRadioOcrParamsHere(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);
	LRESULT OnBnClickedRadioOcrParamsFromRuleset(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);

private:
	///////////
	// Variables
	////////////
	ATLControls::CButton m_radioAllPages;
	ATLControls::CButton m_radioSpecificPages;
	ATLControls::CEdit m_editSpecificPages;
	ATLControls::CButton m_checkUseCleanImage;
	ATLControls::CButton m_checkParallelize;

	ATLControls::CButton m_radioNoOCRParameters;
	ATLControls::CButton m_radioSpecifiedOCRParameters;
	ATLControls::CButton m_radioOCRParametersFromRuleset;
	ATLControls::CEdit m_editOCRParametersRuleset;
	CImageButtonWithStyle m_btnOCRParametersRulesetSelectTag;
	ATLControls::CButton m_btnOCRParametersRulesetBrowse;
	ATLControls::CButton m_btnEditOCRParameters;
	ATLControls::CComboBox m_comboOCREngine;

	///////////
	// Methods
	///////////
	bool savePageSelections(UCLID_FILEPROCESSORSLib::IOCRFileProcessorPtr ipOCR);

	// ensure that this component is licensed
	void validateLicense();

	const std::string chooseFile();

	void initializeOCREngineCombo(UCLID_FILEPROCESSORSLib::EOCREngineType engineType);
	void enableOcrParameterControls(bool enable);
public:
	LRESULT OnCbnSelchangeOcrEngineCombo(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);
};
