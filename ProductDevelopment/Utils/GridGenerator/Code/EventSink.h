// EventSink.h : Declaration of the CEventSink

#pragma once

#include "resource.h"       // main symbols
#include "DrawGrid.h"

/////////////////////////////////////////////////////////////////////////////
// CEventSink
class ATL_NO_VTABLE CEventSink : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CEventSink, &CLSID_EventSink>,
	public ISupportErrorInfo,
	public IDispatchImpl<IEventSink, &IID_IEventSink, &LIBID_UCLID_GRIDGENERATORLib>,
	public IEditEvents
{
public:
	CEventSink();

	void SetParentCtrl(CDrawGrid *pCtrl) {m_pDrawGridCtrl = pCtrl;}

DECLARE_REGISTRY_RESOURCEID(IDR_EVENTSINK)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CEventSink)
	COM_INTERFACE_ENTRY(IEventSink)
	COM_INTERFACE_ENTRY2(IDispatch, IEventSink)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IEditEvents)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

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

// IEventSink
	STDMETHOD(SetApplicationHook)(IApplication *pApp);

private:
	///////////
	// Variables
	///////////
	IApplicationPtr m_ipApp;
	IEditorPtr m_ipEditor;

	CDrawGrid* m_pDrawGridCtrl;

	bool m_bIsEditEnabled;

	///////////
	// Methods
	///////////
	// whether or not current task is 'Create New Feature'
	bool isCurrentTaskRight();
};
