//==================================================================================================
//
// COPYRIGHT (c) 2009 EXTRACT SYSTEMS, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ManageTagsTaskPP.h
//
// PURPOSE:	Header file for Manage tags file processing task property page
//
// AUTHORS:	Jeff Shergalis
//
//==================================================================================================

#pragma once
#include "resource.h"       // main symbols
#include "FileProcessors.h"

#include <string>

using namespace std;

EXTERN_C const CLSID CLSID_ManageTagsTaskPP;

/////////////////////////////////////////////////////////////////////////////
// CManageTagsTaskPP
class ATL_NO_VTABLE CManageTagsTaskPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CManageTagsTaskPP, &CLSID_ManageTagsTaskPP>,
	public IPropertyPageImpl<CManageTagsTaskPP>,
	public CDialogImpl<CManageTagsTaskPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CManageTagsTaskPP();
	~CManageTagsTaskPP();

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	enum {IDD = IDD_MANAGE_TAGS_TASKPP};

	DECLARE_REGISTRY_RESOURCEID(IDR_MANAGE_TAGS_TASKPP)

	BEGIN_COM_MAP(CManageTagsTaskPP)
		COM_INTERFACE_ENTRY(IPropertyPage)
	END_COM_MAP()

	BEGIN_MSG_MAP(CManageTagsTaskPP)
		CHAIN_MSG_MAP(IPropertyPageImpl<CManageTagsTaskPP>)
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
	LRESULT OnClickedBtnRadioOperation(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:
	// Ensures that this component is licensed
	void validateLicense();

	// Loads the tags from the database, returns true if there was at least 1 tag
	// to load, false otherwise
	void loadTagsFromDatabase(const IFileProcessingDBPtr& ipDB);

	void prepareControls();

	void selectTags(const UCLID_FILEPROCESSORSLib::IManageTagsTaskPtr& ipManageTags);

	///////
	// Data
	///////
	// Various controls
	ATLControls::CButton m_radioAddTags;
	ATLControls::CButton m_radioRemoveTags;
	ATLControls::CButton m_radioToggleTags;
	ATLControls::CListViewCtrl m_listTags;
};

OBJECT_ENTRY_AUTO(__uuidof(ManageTagsTaskPP), CManageTagsTaskPP)