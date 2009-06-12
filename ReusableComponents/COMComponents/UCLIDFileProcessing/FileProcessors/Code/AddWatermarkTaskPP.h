//==================================================================================================
//
// COPYRIGHT (c) 2008 EXTRACT SYSTEMS, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	AddWatermarkTaskPP.h
//
// PURPOSE:	Header file for Add Watermark file processing property page
//
// AUTHORS:	Jeff Shergalis
//
//==================================================================================================

#pragma once
#include "resource.h"       // main symbols
#include "FileProcessors.h"
#include <ImageButtonWithStyle.h>

EXTERN_C const CLSID CLSID_AddWatermarkTaskPP;

/////////////////////////////////////////////////////////////////////////////
// CAddWatermarkTaskPP
class ATL_NO_VTABLE CAddWatermarkTaskPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CAddWatermarkTaskPP, &CLSID_AddWatermarkTaskPP>,
	public IPropertyPageImpl<CAddWatermarkTaskPP>,
	public CDialogImpl<CAddWatermarkTaskPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CAddWatermarkTaskPP();
	~CAddWatermarkTaskPP();

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	enum {IDD = IDD_ADD_WATERMARK_TASKPP};

	DECLARE_REGISTRY_RESOURCEID(IDR_ADD_WATERMARK_TASKPP)

	BEGIN_COM_MAP(CAddWatermarkTaskPP)
		COM_INTERFACE_ENTRY(IPropertyPage)
	END_COM_MAP()

	BEGIN_MSG_MAP(CAddWatermarkTaskPP)
		CHAIN_MSG_MAP(IPropertyPageImpl<CAddWatermarkTaskPP>)
		MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
		COMMAND_HANDLER(IDC_BTN_WATERMARK_INPUT_IMAGE_DOC_TAG, BN_CLICKED, OnClickedBtnInputImageDocTag)
		COMMAND_HANDLER(IDC_BTN_WATERMARK_BROWSE_INPUT_IMAGE, BN_CLICKED, OnClickedBtnInputImageBrowse)
		COMMAND_HANDLER(IDC_BTN_WATERMARK_STAMP_IMAGE_DOC_TAG, BN_CLICKED, OnClickedBtnStampImageDocTag)
		COMMAND_HANDLER(IDC_BTN_WATERMARK_BROWSE_STAMP_IMAGE, BN_CLICKED, OnClickedBtnStampImageBrowse)
		COMMAND_HANDLER(IDC_RADIO_WATERMARK_FIRSTPAGE, BN_CLICKED, OnClickedBtnRadioPage)
		COMMAND_HANDLER(IDC_RADIO_WATERMARK_LASTPAGE, BN_CLICKED, OnClickedBtnRadioPage)
		COMMAND_HANDLER(IDC_RADIO_WATERMARK_SPECIFIEDPAGE, BN_CLICKED, OnClickedBtnRadioPage)
		// REFLECT_NOTIFICATIONS needed by ImageButtonWithSytle
		REFLECT_NOTIFICATIONS()
	END_MSG_MAP()

// Handler prototypes:
//  LRESULT MessageHandler(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
//  LRESULT CommandHandler(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
//  LRESULT NotifyHandler(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);

	// IPropertyPage
	STDMETHOD(Apply)(void);

	// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

	// Message handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnClickedBtnInputImageDocTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnInputImageBrowse(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnStampImageDocTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnStampImageBrowse(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnRadioPage(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:
	// Ensures that this component is licensed
	void	validateLicense();

	///////
	// Data
	///////
	// Various controls
	ATLControls::CEdit m_editInputImage;
	ATLControls::CEdit m_editStampImage;
	ATLControls::CEdit m_editHorizontalPercentage;
	ATLControls::CEdit m_editVerticalPercentage;
	ATLControls::CEdit m_editSpecifiedPages;
	CImageButtonWithStyle m_btnInputImageDocTag;
	ATLControls::CButton m_btnInputImageBrowse;
	CImageButtonWithStyle m_btnStampImageDocTag;
	ATLControls::CButton m_btnStampImageBrowse;
	ATLControls::CButton m_radioFirstPage;
	ATLControls::CButton m_radioLastPage;
	ATLControls::CButton m_radioSpecifiedPage;
};

OBJECT_ENTRY_AUTO(__uuidof(AddWatermarkTaskPP), CAddWatermarkTaskPP)