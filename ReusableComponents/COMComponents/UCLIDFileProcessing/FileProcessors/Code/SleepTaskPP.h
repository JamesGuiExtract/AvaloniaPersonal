//==================================================================================================
//
// COPYRIGHT (c) 2009 EXTRACT SYSTEMS, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	SleepTaskPP.h
//
// PURPOSE:	Header file for Sleep file processing task property page
//
// AUTHORS:	Jeff Shergalis
//
//==================================================================================================

#pragma once
#include "resource.h"       // main symbols
#include "FileProcessors.h"

#include <string>

using namespace std;

EXTERN_C const CLSID CLSID_SleepTaskPP;

/////////////////////////////////////////////////////////////////////////////
// CSleepTaskPP
class ATL_NO_VTABLE CSleepTaskPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CSleepTaskPP, &CLSID_SleepTaskPP>,
	public IPropertyPageImpl<CSleepTaskPP>,
	public CDialogImpl<CSleepTaskPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CSleepTaskPP();
	~CSleepTaskPP();

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	enum {IDD = IDD_SLEEP_TASKPP};

	DECLARE_REGISTRY_RESOURCEID(IDR_SLEEP_TASKPP)

	BEGIN_COM_MAP(CSleepTaskPP)
		COM_INTERFACE_ENTRY(IPropertyPage)
	END_COM_MAP()

	BEGIN_MSG_MAP(CSleepTaskPP)
		CHAIN_MSG_MAP(IPropertyPageImpl<CSleepTaskPP>)
		MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
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

private:
	// Ensures that this component is licensed
	void validateLicense();

	void prepareControls();

	///////
	// Data
	///////
	// Various controls
	ATLControls::CEdit m_editSleepTime;
	ATLControls::CComboBox m_comboUnits;
	ATLControls::CButton m_checkRandom;
};

OBJECT_ENTRY_AUTO(__uuidof(SleepTaskPP), CSleepTaskPP)