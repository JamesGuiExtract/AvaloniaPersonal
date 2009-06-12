// CleanupImageFileProcessorPP.h : Declaration of the CCleanupImageFileProcessorPP

#pragma once
#include "resource.h"       // main symbols

#include "FileProcessors.h"
#include <ImageButtonWithStyle.h>


#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif



// CCleanupImageFileProcessorPP

class ATL_NO_VTABLE CCleanupImageFileProcessorPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CCleanupImageFileProcessorPP, &CLSID_CleanupImageFileProcessorPP>,
	public IPropertyPageImpl<CCleanupImageFileProcessorPP>,
	public CDialogImpl<CCleanupImageFileProcessorPP>
{
public:
	CCleanupImageFileProcessorPP();
	~CCleanupImageFileProcessorPP();

	enum {IDD = IDD_CLEANUPIMAGEFILEPROCESSORPP};

DECLARE_REGISTRY_RESOURCEID(IDR_CLEANUPIMAGEFILEPROCESSORPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CCleanupImageFileProcessorPP)
	COM_INTERFACE_ENTRY(IPropertyPage)
END_COM_MAP()

BEGIN_MSG_MAP(CCleanupImageFileProcessorPP)
	COMMAND_HANDLER(IDC_BTN_SELECT_ICS_FILENAME_DOC_TAG, BN_CLICKED, OnClickedBtnFileSelectTag)
	COMMAND_HANDLER(IDC_BTN_BROWSE_ICS_FILENAME, BN_CLICKED, OnClickedBtnBrowseFile)
	CHAIN_MSG_MAP(IPropertyPageImpl<CCleanupImageFileProcessorPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	// REFLECT_NOTIFICATIONS needed by ImageButtonWithSytle
	REFLECT_NOTIFICATIONS()
END_MSG_MAP()

// IPropertyPage
	STDMETHOD(Apply)(void);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnClickedBtnFileSelectTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnBrowseFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:
	// Variables
	ATLControls::CEdit m_edtSettingsFileName;
	CImageButtonWithStyle m_btnSelectTag;

	// Methods
	// PURPOSE: To get the start and end position of the cursor
	void getEditBoxSelection(int& rnStartChar, int& rnEndChar);
};

OBJECT_ENTRY_AUTO(__uuidof(CleanupImageFileProcessorPP), CCleanupImageFileProcessorPP)
