// DrawGrid.h : Declaration of the CDrawGrid

#pragma once

#include "resource.h"       // main symbols

#include "..\..\..\..\ReusableComponents\APIs\ArcGIS\Inc\ArcCATIDs.h"
#include "GridDrawer.h"

/////////////////////////////////////////////////////////////////////////////
// CDrawGrid
class ATL_NO_VTABLE CDrawGrid : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CDrawGrid, &CLSID_DrawGrid>,
	public ISupportErrorInfo,
	public IDispatchImpl<IDrawGrid, &IID_IDrawGrid, &LIBID_UCLID_GRIDGENERATORLib>,
	public IDispatchImpl<UCLID_COMLMLib::ILicensedComponent, &UCLID_COMLMLib::IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IESRICommand
{
public:
	CDrawGrid();
	~CDrawGrid();

	void createAllFields();

	// whether or not to enable this tool
	// Only enable this tool if it's in the edit mode 
	// and current task is set to 'Create New Feature'
	bool m_bEnbleTool;

DECLARE_REGISTRY_RESOURCEID(IDR_DRAWGRID)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CDrawGrid)
	COM_INTERFACE_ENTRY(IDrawGrid)
	COM_INTERFACE_ENTRY2(IDispatch, IDrawGrid)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IESRICommand)
	COM_INTERFACE_ENTRY(UCLID_COMLMLib::ILicensedComponent)
END_COM_MAP()

BEGIN_CATEGORY_MAP(CIcoMapDrawingCtrl)
	IMPLEMENTED_CATEGORY(__uuidof(CATID_MxCommands))
END_CATEGORY_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IESRICommand
	STDMETHOD(raw_get_Enabled)(VARIANT_BOOL * Enabled);
	STDMETHOD(raw_get_Checked)(VARIANT_BOOL * Checked);
	STDMETHOD(raw_get_Name)(BSTR * Name);
	STDMETHOD(raw_get_Caption)(BSTR * Caption);
	STDMETHOD(raw_get_Tooltip)(BSTR * Tooltip);
	STDMETHOD(raw_get_Message)(BSTR * Message);
	STDMETHOD(raw_get_HelpFile)(BSTR * HelpFile);
	STDMETHOD(raw_get_HelpContextID)(LONG * helpID);
	STDMETHOD(raw_get_Bitmap)(OLE_HANDLE * Bitmap);
	STDMETHOD(raw_get_Category)(BSTR * categoryName);
	STDMETHOD(raw_OnCreate)(IDispatch * hook);
	STDMETHOD(raw_OnClick)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);


// IDrawGrid
public:

private:
	///////////
	// Variables
	////////////

	HBITMAP	m_bitmap;

	IApplicationPtr	m_ipApp;
	IEditorPtr m_ipEditor;

	DWORD m_dwEditCookie;				//Cookie to disconnect from event source with

	UCLID_GRIDGENERATORLib::IEventSinkPtr m_ipEventSink;

	GridDrawer m_GridDrawer;

	///////////
	// Methods
	///////////
	void connectToEventsSink();
	void disconnectFromEventsSink();

	void validateLicense();
};
