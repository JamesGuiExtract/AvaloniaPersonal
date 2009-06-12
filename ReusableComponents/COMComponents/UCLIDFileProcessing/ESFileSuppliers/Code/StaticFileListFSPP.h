// CStaticFileListFSPP.h : Declaration of the CStaticFileListFSPP

#pragma once

#include "resource.h"       // main symbols
#include "ESFileSuppliers.h"

#include <vector>
#include <string>


EXTERN_C const CLSID CLSID_StaticFileListFSPP;

/////////////////////////////////////////////////////////////////////////////
// CStaticFileListFSPP
class ATL_NO_VTABLE CStaticFileListFSPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CStaticFileListFSPP, &CLSID_StaticFileListFSPP>,
	public IPropertyPageImpl<CStaticFileListFSPP>,
	public CDialogImpl<CStaticFileListFSPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CStaticFileListFSPP();

	enum {IDD = IDD_STATICFILELISTFSPP};

DECLARE_REGISTRY_RESOURCEID(IDR_STATICFILELISTFSPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CStaticFileListFSPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CStaticFileListFSPP)
	NOTIFY_HANDLER(IDC_FILE_LIST, NM_CLICK, OnNMClickFileList)
	CHAIN_MSG_MAP(IPropertyPageImpl<CStaticFileListFSPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_BTN_ADD, BN_CLICKED, OnBnClickedBtnAdd)
	COMMAND_HANDLER(IDC_BTN_CLEAR, BN_CLICKED, OnBnClickedBtnClear)
	COMMAND_HANDLER(IDC_BTN_LOAD_LIST, BN_CLICKED, OnBnClickedBtnLoadList)
	COMMAND_HANDLER(IDC_BTN_REMOVE, BN_CLICKED, OnBnClickedBtnRemove)
END_MSG_MAP()

// IProperty Page
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnBnClickedBtnAdd(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);
	LRESULT OnBnClickedBtnClear(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);
	LRESULT OnBnClickedBtnLoadList(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);
	LRESULT OnBnClickedBtnRemove(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);
	LRESULT OnNMClickFileList(int /*idCtrl*/, LPNMHDR pNMHDR, BOOL& /*bHandled*/);

private:
	///////////
	// Variables
	////////////

	// individually defined list of files
	IVariantVectorPtr m_vecFileList;

	ATLControls::CListViewCtrl m_FileList;
	ATLControls::CButton m_btnAddFile;
	ATLControls::CButton m_btnRemoveFile;
	ATLControls::CButton m_btnClearList;
	ATLControls::CButton m_btnAddList;

	///////////
	// Methods
	///////////
	
	// Add a single file to the end of the list
	// return true if user has not choosen to break out and 
	// another file can be added.
	bool addFile(std::string strFileName);

	//Add the contents of a file to the list
	void addListOfFiles();

	//update the enabled/disabled status of delete and clear
	void updateButtons();

	//Prompt user with yes/no, if yes clear all the variables and return true
	//else return false
	bool clear();

	// ensure that this component is licensed
	void validateLicense();
};

OBJECT_ENTRY_AUTO(__uuidof(StaticFileListFSPP), CStaticFileListFSPP)
