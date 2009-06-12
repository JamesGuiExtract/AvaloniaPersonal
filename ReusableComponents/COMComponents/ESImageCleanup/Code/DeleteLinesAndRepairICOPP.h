// DeleteLinesAndRepairICOPP.h : Declaration of the CDeleteLinesAndRepairICOPP

#pragma once

#include "resource.h"
#include "stdafx.h"
#include "ESImageCleanup.h"

// CDespeckleICOPP

class ATL_NO_VTABLE CDeleteLinesAndRepairICOPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CDeleteLinesAndRepairICOPP, &CLSID_DeleteLinesAndRepairICOPP>,
	public IPropertyPageImpl<CDeleteLinesAndRepairICOPP>,
	public CDialogImpl<CDeleteLinesAndRepairICOPP>
{
public:
	CDeleteLinesAndRepairICOPP();
	~CDeleteLinesAndRepairICOPP();

	enum {IDD = IDD_DELETELINESANDREPAIRICOPP};

DECLARE_REGISTRY_RESOURCEID(IDR_DELETELINESANDREPAIRICOPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CDeleteLinesAndRepairICOPP)
	COM_INTERFACE_ENTRY(IPropertyPage)
END_COM_MAP()

BEGIN_MSG_MAP(CDespeckleICOPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CDeleteLinesAndRepairICOPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
END_MSG_MAP()

// IPropertyPage
	STDMETHOD(Apply)(void);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);

private:
	// Variables
	ATLControls::CEdit m_editLineLength;
	ATLControls::CEdit m_editLineGap;

	// Methods
	unsigned long getLineDirection();
};

OBJECT_ENTRY_AUTO(__uuidof(DeleteLinesAndRepairICOPP), CDeleteLinesAndRepairICOPP)
