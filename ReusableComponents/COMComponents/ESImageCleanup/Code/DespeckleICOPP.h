// DespeckleICOPP.h : Declaration of the CDespeckleICOPP

#pragma once

#include "resource.h"
#include "stdafx.h"
#include "ESImageCleanup.h"

// CDespeckleICOPP

class ATL_NO_VTABLE CDespeckleICOPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CDespeckleICOPP, &CLSID_DespeckleICOPP>,
	public IPropertyPageImpl<CDespeckleICOPP>,
	public CDialogImpl<CDespeckleICOPP>
{
public:
	CDespeckleICOPP();
	~CDespeckleICOPP();

	enum {IDD = IDD_DESPECKLEICOPP};

DECLARE_REGISTRY_RESOURCEID(IDR_DESPECKLEICOPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CDespeckleICOPP)
	COM_INTERFACE_ENTRY(IPropertyPage)
END_COM_MAP()

BEGIN_MSG_MAP(CDespeckleICOPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CDespeckleICOPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
END_MSG_MAP()

// IPropertyPage
	STDMETHOD(Apply)(void);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);

private:
	// Variables
	ATLControls::CEdit m_editNoiseSize;
};

OBJECT_ENTRY_AUTO(__uuidof(DespeckleICOPP), CDespeckleICOPP)