// QueryFeature.h : Declaration of the CQueryFeature

#pragma once

#include "resource.h"       // main symbols
#include "..\..\..\..\ReusableComponents\APIs\ArcGIS\Inc\ArcCATIDs.h"


#include <memory>

class QueryDlg;
/////////////////////////////////////////////////////////////////////////////
// CQueryFeature
class ATL_NO_VTABLE CQueryFeature : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CQueryFeature, &CLSID_QueryFeature>,
	public ISupportErrorInfo,
	public IDispatchImpl<IQueryFeature, &IID_IQueryFeature, &LIBID_UCLID_GRIDGENERATORLib>,
	public IESRICommand,
	public IDispatchImpl<UCLID_COMLMLib::ILicensedComponent, &UCLID_COMLMLib::IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CQueryFeature();
	~CQueryFeature();

DECLARE_REGISTRY_RESOURCEID(IDR_QUERYFEATURE)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CQueryFeature)
	COM_INTERFACE_ENTRY(IQueryFeature)
	COM_INTERFACE_ENTRY2(IDispatch, IQueryFeature)
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

// IQueryFeature
public:

private:
	///////////
	// Variables
	////////////

	HBITMAP	m_bitmap;

	IApplicationPtr	m_ipApp;
	IEditorPtr m_ipEditor;

	std::auto_ptr<QueryDlg> m_apQueryDlg;

	//////////
	// Methods
	//////////
	void showQueryDlg();

	void validateLicense();
};
