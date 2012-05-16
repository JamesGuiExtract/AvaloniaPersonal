// IcoMapDrawingCtrl.h : Declaration of the CIcoMapDrawingCtrl

#pragma once

#include "resource.h"       // main symbols

#include "..\..\..\..\ReusableComponents\APIs\ArcGIS\Inc\ArcCATIDs.h"
#include "..\..\..\PlatformSpecificUtils\ArcGISUtils\Code\UCLIDArcMapToolbarCATID.h"

#include <IConfigurationSettingsPersistenceMgr.h>
#include <memory>

_COM_SMARTPTR_TYPEDEF(IEditEventsSink, __uuidof(IEditEventsSink));

enum EWorkspaceType {
	kNone = -1,
	kGeodatabase = 0,
	kCoverage,
	kPersonalGeodatabase,
	kShapefile
};
/////////////////////////////////////////////////////////////////////////////
// CIcoMapDrawingCtrl
class ATL_NO_VTABLE CIcoMapDrawingCtrl : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CIcoMapDrawingCtrl, &CLSID_IcoMapDrawingCtrl>,
	public IIcoMapDrawingCtrl,
	public ISupportErrorInfo,
	public IESRCommand,
	public ITool,
	public IExtension,
	public IExtensionConfig
{
public:
	CIcoMapDrawingCtrl();
	~CIcoMapDrawingCtrl();

	DECLARE_REGISTRY_RESOURCEID(IDR_ICOMAPDRAWINGCTRL)

	DECLARE_CLASSFACTORY_SINGLETON(CIcoMapDrawingCtrl)

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	BEGIN_COM_MAP(CIcoMapDrawingCtrl)
		COM_INTERFACE_ENTRY(IIcoMapDrawingCtrl)
		COM_INTERFACE_ENTRY(ISupportErrorInfo)
		COM_INTERFACE_ENTRY(IESRCommand)
		COM_INTERFACE_ENTRY(ITool)
		COM_INTERFACE_ENTRY(IExtension)
		COM_INTERFACE_ENTRY(IExtensionConfig)
	END_COM_MAP()

	BEGIN_CATEGORY_MAP(CIcoMapDrawingCtrl)
		IMPLEMENTED_CATEGORY(__uuidof(CATID_MxCommands))
		IMPLEMENTED_CATEGORY(CATID_UCLIDArcMapToolbar)
	END_CATEGORY_MAP()


	// IIcoMapDrawingCtrl
public:

	void HideIcoMapDlg(void);
	void ShowIcoMapDlg(void);
	esriGeometryType QueryCurrentLayerGeometryType(void);
	// Query current layer's feature type
	esriFeatureType QueryCurrentLayerFeatureType(void);

	void ConvertMouseToMapPoint(LONG xPt,LONG yPt,IPointPtr& ipPoint);
	bool FindCommandItem(LPCOLESTR sProgID,ICommandItemPtr& ipCmdItem);

	// see article "Command Implementing Events With an Intermediate Sink Object" from 
	// ArcObject online
	void ConnectToEventsSink();
	void DisconnectFromEventsSink();

	void reset();
	void notifySketchModified(long nActualNumOfSegments);

	// when starting an edit session, make sure the icomap attribute field is there
	// for each polyline and polygon feature class
	void createIcoMapAttributeField();

	// whether or not feature creation is enabled for IcoMap
	void enableFeatureCreation(bool bEnable);
	// process feature creation related events
	void processEnableFeatureEdit();
	// whether or not it's in edit mode
	// Edit mode is critical for most of icomap tools (i.e. drawing tools, view/edit attribute tool)
	// 1) View/edit attributes tool is enabled when it's in edit mode
	// 2) Drawing tools (including all drawing tool related operations like finish 
	//    sketch, toggle curve, etc.) are enabled when it's in edit mode, and the layer and task
	//    are operable (i.e. polyline and polygon layers, create new features)
	void enableEditMode(bool bEnable);

	// To set IcoMap as current tool or not
	void setIcoMapAsCurrentTool(bool bAsCurrent);

	// mark as the start of a new document or open another document
	void startADocument(bool bNew) {m_bStartADoc = bNew;}
public:
	// ICommand
	STDMETHOD(get_Enabled)(VARIANT_BOOL * Enabled);
	STDMETHOD(get_Checked)(VARIANT_BOOL * Checked);
	STDMETHOD(get_Name)(BSTR * Name);
	STDMETHOD(get_Caption)(BSTR * Caption);
	STDMETHOD(get_Tooltip)(BSTR * Tooltip);
	STDMETHOD(get_Message)(BSTR * Message);
	STDMETHOD(get_HelpFile)(BSTR * HelpFile);
	STDMETHOD(get_HelpContextID)(LONG * helpID);
	STDMETHOD(get_Bitmap)(OLE_HANDLE * Bitmap);
	STDMETHOD(get_Category)(BSTR * categoryName);
	STDMETHOD(raw_OnCreate)(IDispatch * hook);
	STDMETHOD(raw_OnClick)(void);
	
	// ITool
	STDMETHOD(get_Cursor)(OLE_HANDLE * Cursor);
	STDMETHOD(raw_OnMouseDown)(LONG Button, LONG Shift, LONG X, LONG Y);
	STDMETHOD(raw_OnMouseMove)(LONG Button, LONG Shift, LONG X, LONG Y);
	STDMETHOD(raw_OnMouseUp)(LONG Button, LONG Shift, LONG X, LONG Y);
	STDMETHOD(raw_OnDblClick)(void);
	STDMETHOD(raw_OnKeyDown)(LONG keyCode, LONG Shift);
	STDMETHOD(raw_OnKeyUp)(LONG keyCode, LONG Shift);
	STDMETHOD(raw_OnContextMenu)(LONG X, LONG Y, VARIANT_BOOL * handled);
	STDMETHOD(raw_Refresh)(OLE_HANDLE hDC);
	STDMETHOD(raw_Deactivate)(VARIANT_BOOL * complete);
	
	// ISupportErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

	// IExtension Methods
	STDMETHOD(raw_Startup)(VARIANT * initializationData);
	STDMETHOD(raw_Shutdown)();

	// IExtensionConfig Methods
	STDMETHOD(get_ProductName)(BSTR * Name);
	STDMETHOD(get_Description)(BSTR * Description);
	STDMETHOD(get_State)(esriExtensionState * State);
	STDMETHOD(put_State)(esriExtensionState State);

private:
	HBITMAP	m_bitmap;
	HCURSOR m_cursor;

	IApplicationPtr	m_ipApp;            // Valid upon create
	IEditorPtr m_ipEditor;
	IDocumentPtr m_ipDocument;
	IMxDocumentPtr m_ipMxDoc;
	IPointPtr m_ipActivePoint;

	IIcoMapApplicationPtr m_ipIcoMapApp;
	IDisplayAdapterPtr m_ipDisplayAdapter;
	IAttributeManagerPtr m_ipAttributeManager;		

	IEditEventsSinkPtr m_ipEditEventsSink;

	DWORD m_dwEditCookie;				//Cookie to disconnect from event source with
	DWORD m_dwDocumentCookie;				//Cookie to disconnect from event source with

	bool m_bIsFeatureCreationEnabled;
	bool m_bIsInEditMode;
	bool m_bStartADoc;					// whether or not just open a document or a new document just created

	// Flag to indicate if the dialog is visible before calling HideIcoMapDlg
	bool m_bIsDlgVisible;

	// Variable to keep track of the extension state
	esriExtensionState m_ExtState;

	// configure manager for IcoMap for ArcGIS root
	std::auto_ptr<IConfigurationSettingsPersistenceMgr> m_apIcoMapCfgMgr;
	// configure manager for ArcGISUtils root
	std::auto_ptr<IConfigurationSettingsPersistenceMgr> m_apArcGISUtilsCfgMgr;


	///////////////////
	// Helper functions
	///////////////////
	// for a specified feature class (i.e. ipFeatureClass), find whether icomap
	// attribute field is already there, if not, add it if bCreate is true
	long findIcoMapAttributeField(IFeatureClassPtr ipFeatureClass, bool bCreate);

	// find out what workspace the feature is currently in
	EWorkspaceType getWorkspaceType(IFeatureClassPtr ipFeatureClass);

	// whether or not it's edit mode
	// This is a little bit different than m_bIsInEditMode. 
	// It will check ArcMap to see if it's in edit mode
	bool isInEditMode();

	// whether or not view/edit feature attributes from IcoMap dlg is enabled
	bool isFeatureSelectionEnabled();

	// make a start and followed by stop editing immediately if it's not in edit mode
	void quickStartOrStopEditing(IFeatureClassPtr ipFeatureClass, bool bStart, bool bSaveEdits = false);

	// Undepress IcoMap toolbar button
	void selectDefaultArcMapTool(void);

	// select a feature around position of X,Y and pass on the event of onFeatureSelected
	void selectFeatureAround(LONG X, LONG Y);

	// store ToolName-Guid pair in Registry
	void storeToolNameGuidPair(const std::string& strToolName, const std::string& strGUID);

	// Check licensing state
	void validateLicense();

	// Check the overall license state
	void validateIcoMapLicense();
};
