// EditEventsSink.h : Declaration of the CEditEventsSink

#pragma once

#include "resource.h"       // main symbols
#include "IcoMapDrawingCtrl.h"

/////////////////////////////////////////////////////////////////////////////
// CEditEventsSink
class ATL_NO_VTABLE CEditEventsSink : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CEditEventsSink, &CLSID_EditEventsSink>,
	public ISupportErrorInfo,
	public IEditEventsSink,
	public IEditEvents,
	public IDocumentEvents
{
public:
	CEditEventsSink();
	~CEditEventsSink();
	void SetParentCtrl(CIcoMapDrawingCtrl *pCtrl);

	enum EType{kNothing=0, kLine, kPolygon, kOther};

DECLARE_REGISTRY_RESOURCEID(IDR_EDITEVENTSSINK)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CEditEventsSink)
	COM_INTERFACE_ENTRY(IEditEventsSink)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IDocumentEvents)
	COM_INTERFACE_ENTRY(IEditEvents)
END_COM_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

public:
// IEditEventsSink
	STDMETHOD(SetApplicationHook)(IApplication *pApp);
// IEditEvents
	STDMETHOD(raw_OnSelectionChanged)();
	STDMETHOD(raw_OnCurrentLayerChanged)();
	STDMETHOD(raw_OnCurrentTaskChanged)();
	STDMETHOD(raw_OnSketchModified)();
	STDMETHOD(raw_OnSketchFinished)();
	STDMETHOD(raw_AfterDrawSketch)(IDisplay *pDpy);
	STDMETHOD(raw_OnStartEditing)();
	STDMETHOD(raw_OnStopEditing)(VARIANT_BOOL Save);
	STDMETHOD(raw_OnConflictsDetected)();
	STDMETHOD(raw_OnUndo)();
	STDMETHOD(raw_OnRedo)();
	STDMETHOD(raw_OnCreateFeature)(IObject *obj);
	STDMETHOD(raw_OnChangeFeature)(IObject *obj);
	STDMETHOD(raw_OnDeleteFeature)(IObject *obj);
// IDocumentEvents
	STDMETHOD(raw_ActiveViewChanged)();
	STDMETHOD(raw_MapsChanged)();
	STDMETHOD(raw_OnContextMenu)(long X, long Y, VARIANT_BOOL *handled);
	STDMETHOD(raw_NewDocument)();
	STDMETHOD(raw_OpenDocument)();
	STDMETHOD(raw_BeforeCloseDocument)(VARIANT_BOOL *abortClose);
	STDMETHOD(raw_CloseDocument)();

private:
	////////////////
	// Member variables
	/////////////////

	// type of layer or feature selected
	struct OperationInfo
	{
	public:
		OperationInfo()
			:mm_strCurrentTaskName(""), mm_eTargetLayerType(CEditEventsSink::kNothing),
		 mm_eSelectionType(CEditEventsSink::kNothing), mm_bIsSingleSelection(false)
		{
		}

		// whether or not current combination of layer, task and/or feature selection
		// is suitable for icomap to operate on
		bool isOperable();

		std::string mm_strCurrentTaskName;
		EType mm_eTargetLayerType;
		EType mm_eSelectionType;
		bool mm_bIsSingleSelection;
	};

	OperationInfo m_operationInfo;

	CIcoMapDrawingCtrl *m_pIcoMapDrawingCtrl;
	IApplicationPtr m_ipApp;
	IEditorPtr m_ipEditor;

	long m_nNumOfSegments;

	///////////////////
	// helper functions
	///////////////////
	void CheckCurrentLayer();
	void CheckCurrentTask();
	void CheckCurrentSelection();
	// show uclid toolbar if it's not visible
	void ShowUCLIDToolbar();


};
