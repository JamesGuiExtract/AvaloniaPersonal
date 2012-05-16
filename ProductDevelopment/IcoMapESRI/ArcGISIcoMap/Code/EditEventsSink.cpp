// EditEventsSink.cpp : Implementation of CEditEventsSink
#include "stdafx.h"
#include "ArcGISIcoMap.h"
#include "EditEventsSink.h"

#include <UCLIDException.h>
#include <TemporaryResourceOverride.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

extern HINSTANCE gModuleResource;

static const string CREATE_NEW_FEATURE = "Create New Feature";
static const string RESHAPE_FEATURE = "Reshape Feature";
static const string MODIFY_FEATURE = "Modify Feature";
static const string CUT_POLYGON_FEATURES = "Cut Polygon Features";
static const string EXTEND_TRIM_FEATURE = "Extend/Trim Features";
static const string AUTO_COMPLETE_POLYGON = "Auto-Complete Polygon";
static const string SELECT_FEATURE_WITH_LINE = "Select Features Using a Line";
static const string SELECT_FEATURE_WITH_AREA = "Select Features Using an Area";
static const string CREATE_2POINT_LINE_FEATURE = "Create 2-Point Line Features";

//-------------------------------------------------------------------------------------------------
// CEditEventsSink
//-------------------------------------------------------------------------------------------------
CEditEventsSink::CEditEventsSink()
:m_pIcoMapDrawingCtrl(NULL), 
 m_ipApp(NULL), 
 m_ipEditor(NULL),
 m_nNumOfSegments(0)
{
}
//-------------------------------------------------------------------------------------------------
CEditEventsSink::~CEditEventsSink()
{
	// Release objects
	m_ipEditor = NULL;
	m_ipApp = NULL;
}

//-------------------------------------------------------------------------------------------------
// ISupportErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEditEventsSink::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IEditEventsSink
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//--------------------------------------------------------------------------------------------------
// IEditEventsSink
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEditEventsSink::SetApplicationHook(IApplication *pApp)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_ipApp = pApp;
		
		// set m_ipEditor
		_bstr_t sName("ESRI Object Editor");
		IExtensionPtr ipExtension(m_ipApp->FindExtensionByName(sName));
		m_ipEditor = ipExtension;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04137")

	return S_OK;
}

//--------------------------------------------------------------------------------------------------
// IEditEvents
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEditEventsSink::raw_OnSelectionChanged()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		CheckCurrentSelection();
		
		if (m_operationInfo.mm_strCurrentTaskName == RESHAPE_FEATURE
			|| m_operationInfo.mm_strCurrentTaskName == CUT_POLYGON_FEATURES
			|| m_operationInfo.mm_strCurrentTaskName == EXTEND_TRIM_FEATURE
			|| m_operationInfo.mm_strCurrentTaskName == MODIFY_FEATURE)
		{
			if (m_pIcoMapDrawingCtrl)
			{
				m_pIcoMapDrawingCtrl->enableFeatureCreation(m_operationInfo.isOperable());
				m_pIcoMapDrawingCtrl->reset();
				m_nNumOfSegments = 0;
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04138")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEditEventsSink::raw_OnCurrentLayerChanged()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		CheckCurrentLayer();
		// check current task as well since the task name is used below
		CheckCurrentTask();
		
		if (m_pIcoMapDrawingCtrl)
		{
			m_pIcoMapDrawingCtrl->enableFeatureCreation(m_operationInfo.isOperable());
			// If current task is "Create New Feature", the sketch needs to be deleted if any
			// Notify icomap ctrl to response properly
			if (m_operationInfo.mm_strCurrentTaskName == CREATE_NEW_FEATURE)
			{
				m_pIcoMapDrawingCtrl->reset();
				m_nNumOfSegments = 0;
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI01221")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEditEventsSink::raw_OnCurrentTaskChanged()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		CheckCurrentTask();

		if (m_pIcoMapDrawingCtrl)
		{
			m_pIcoMapDrawingCtrl->enableFeatureCreation(m_operationInfo.isOperable());
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI01220")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEditEventsSink::raw_OnSketchModified()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	// whenever there's a modification to the current sketch (ex. segment added, undo a vertex, etc.)
	try
	{		
		// QI edit sketch to editor
		IEditSketchPtr ipEditSketch(m_ipEditor);
		if (ipEditSketch)
		{
			IGeometryPtr ipGeometry(ipEditSketch->Geometry);
			IESRSegmentCollectionPtr ipSegCol(ipGeometry);
			
			if (ipSegCol)
			{
				// get actual segment count from current sketch
				long nSegmentCounts = ipSegCol->SegmentCount;

				
				// if it's polygon and current task is either create new feature
				// or select feature with polygon, reduce one count
				if (m_operationInfo.mm_eTargetLayerType == CEditEventsSink::kPolygon
					&& (m_operationInfo.mm_strCurrentTaskName == CREATE_NEW_FEATURE
						|| m_operationInfo.mm_strCurrentTaskName == SELECT_FEATURE_WITH_AREA))
				{
					// how many parts are there?
					IGeometryCollectionPtr ipGeoCol(ipGeometry);
					long nNumOfParts = ipGeoCol->GeometryCount;
					// decrement one count for each part
					nSegmentCounts = nSegmentCounts - nNumOfParts;
				}

				// reset to zero if it's negative
				if (nSegmentCounts < 0)
				{
					nSegmentCounts = 0;
				}

				//notify IcoMap about the modification of the sketch
				m_pIcoMapDrawingCtrl->notifySketchModified(nSegmentCounts);
			}	
			else
			{
				// there's no sketch at all
				m_pIcoMapDrawingCtrl->notifySketchModified(0);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI02183")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEditEventsSink::raw_OnSketchFinished()
{
	return E_NOTIMPL;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEditEventsSink::raw_AfterDrawSketch(IDisplay *pDpy)
{
	return E_NOTIMPL;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEditEventsSink::raw_OnStartEditing()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	// For some reason, if an exception is thrown from inside the try block below, and the
	// current exception handler is an MFC-based exception handler (such as the one exported
	// from UCLIDExceptionViewer), then the program crashes immediately.  So, for now,
	// just use the default message-box based exception handler for this scope
	GuardedExceptionHandler defaultExceptionHandler(NULL);

	// TODO: Modify code in this project so that MFC based exception handler will work
	// if an excpetion is thrown from the try block below.  Once this is done, the above
	// line of code can be removed.

	try
	{
		CheckCurrentTask();
		CheckCurrentLayer();
		CheckCurrentSelection();

		// it's now in edit session
		m_pIcoMapDrawingCtrl->enableEditMode(true);

		// create icomap attribute field if it's not there yet
		// TODO: make sure it's feature class, or shape file but not coverage
		m_pIcoMapDrawingCtrl->createIcoMapAttributeField();
		// it's no more the beginning of a doc
		m_pIcoMapDrawingCtrl->startADocument(false);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI01219");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEditEventsSink::raw_OnStopEditing(VARIANT_BOOL Save)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		if (m_pIcoMapDrawingCtrl)
		{
			// make sure icomap clears everything
			m_pIcoMapDrawingCtrl->notifySketchModified(0);
			m_pIcoMapDrawingCtrl->enableEditMode(false);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04139")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEditEventsSink::raw_OnConflictsDetected()
{
	return E_NOTIMPL;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEditEventsSink::raw_OnUndo()
{
	return E_NOTIMPL;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEditEventsSink::raw_OnRedo()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return E_NOTIMPL;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEditEventsSink::raw_OnCreateFeature(IObject *obj)
{
	return E_NOTIMPL;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEditEventsSink::raw_OnChangeFeature(IObject *obj)
{
	return E_NOTIMPL;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEditEventsSink::raw_OnDeleteFeature(IObject *obj)
{
	return E_NOTIMPL;
}

//==================================================================================================
// IDocumentEvents
STDMETHODIMP CEditEventsSink::raw_ActiveViewChanged()
{
	return E_NOTIMPL;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEditEventsSink::raw_MapsChanged()
{
	return E_NOTIMPL;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEditEventsSink::raw_OnContextMenu(long X, long Y, VARIANT_BOOL *handled)
{
	return E_NOTIMPL;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEditEventsSink::raw_NewDocument()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	ShowUCLIDToolbar();
	// mark as the beginning of a doc
	m_pIcoMapDrawingCtrl->startADocument(true);

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEditEventsSink::raw_OpenDocument()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	ShowUCLIDToolbar();
	// mark as the beginning of a doc
	m_pIcoMapDrawingCtrl->startADocument(true);

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEditEventsSink::raw_BeforeCloseDocument(VARIANT_BOOL *abortClose)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	// before closing document, make sure icomap is properly reset
	// For instance, non icomap toolbar button is pressed down, drawing tools are disabled
	if (m_pIcoMapDrawingCtrl)
	{
		m_pIcoMapDrawingCtrl->enableEditMode(false);
		m_pIcoMapDrawingCtrl->setIcoMapAsCurrentTool(false);
	}

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEditEventsSink::raw_CloseDocument()
{
	return E_NOTIMPL;
}

///////////////////////////
// Helper functions
//--------------------------------------------------------------------------------------------------
void CEditEventsSink::SetParentCtrl(CIcoMapDrawingCtrl *pCtrl)
{
	m_pIcoMapDrawingCtrl = pCtrl;
}
//--------------------------------------------------------------------------------------------------
void CEditEventsSink::CheckCurrentTask()
{
	if (m_ipEditor)
	{
		IEditTaskPtr ipEditTask(m_ipEditor->CurrentTask);
		if (ipEditTask)
		{
			//get current task name
			string strCurrentTaskName(_bstr_t(ipEditTask->Name));
			m_operationInfo.mm_strCurrentTaskName = strCurrentTaskName;
		}
	}
}
//--------------------------------------------------------------------------------------------------
void CEditEventsSink::CheckCurrentLayer()
{
	if (m_pIcoMapDrawingCtrl)
	{
		// If the Feature type of the currently layer is not esriFTSimple,
		// (include Polygon, Polyline and point...) it will set the lay type
		// to kOther.

		// QueryCurrentLayerGeometryType() is used only to check esriGeometryPolyline, 
		// esriGeometryPolygon, and esriGeometryPoint. 
		// If the current layer doesn't belong to esriFTSimple, for 
		// example, the annotation layer, the method QueryCurrentLayerGeometryType()
		// Will return it as esriGeometryPolygon, that is why we can not use this  
		// method for those layers.

		if (m_pIcoMapDrawingCtrl->QueryCurrentLayerFeatureType() != esriFTSimple)
		{
			m_operationInfo.mm_eTargetLayerType = CEditEventsSink::kOther;
			return;
		}

		switch(m_pIcoMapDrawingCtrl->QueryCurrentLayerGeometryType())
		{
		case esriGeometryPolyline:
			m_operationInfo.mm_eTargetLayerType = CEditEventsSink::kLine;
			break;
		case esriGeometryPolygon:
			m_operationInfo.mm_eTargetLayerType = CEditEventsSink::kPolygon;
			break;
		default:
			{
				m_operationInfo.mm_eTargetLayerType = CEditEventsSink::kOther;
			}
			break;
		}
	}
}
//--------------------------------------------------------------------------------------------------
void CEditEventsSink::CheckCurrentSelection()
{
	long nNumOfPolygonSelected = 0;
	long nNumOfLineSelected = 0;
	long nNumOfOtherSelected = 0;

	IMxDocumentPtr ipMxDoc(m_ipApp->Document);
	if (ipMxDoc)
	{
		IActiveViewPtr ipActiveView(ipMxDoc->ActiveView);
		if (ipActiveView)
		{
			IMapPtr ipMap(ipActiveView->FocusMap);
			if (ipMap)
			{
				// get the selected feature class
				ISelectionPtr ipSelection(ipMap->FeatureSelection);
				if (ipSelection)
				{
					IEnumFeaturePtr ipEnumFeature(ipSelection);
					if (ipEnumFeature)
					{
						// get the selected feature
						ipEnumFeature->Reset();
						IFeaturePtr ipFeature(ipEnumFeature->Next());
						while (ipFeature)
						{
							IGeometryPtr ipShape = ipFeature->Shape;
							esriGeometryType eShape = ipShape->GeometryType;
							switch (eShape)
							{
							case esriGeometryPolyline:
								nNumOfLineSelected++;
								break;
							case esriGeometryPolygon:
								nNumOfPolygonSelected++;
								break;
							}
							ipFeature = ipEnumFeature->Next();
						}
					}
				}
				nNumOfOtherSelected = ipMap->SelectionCount - nNumOfPolygonSelected - nNumOfLineSelected;
			}
		}
	}
	
	if (nNumOfOtherSelected > 0 ||
		(nNumOfLineSelected > 0 && nNumOfPolygonSelected > 0))
	{
		m_operationInfo.mm_eSelectionType = CEditEventsSink::kOther;
	}
	else if (nNumOfLineSelected > 0 && nNumOfPolygonSelected == 0)
	{
		m_operationInfo.mm_eSelectionType = CEditEventsSink::kLine;
		m_operationInfo.mm_bIsSingleSelection = nNumOfLineSelected==1 ? true:false;
	}
	else if (nNumOfLineSelected == 0 && nNumOfPolygonSelected > 0)
	{
		m_operationInfo.mm_eSelectionType = CEditEventsSink::kPolygon;
		m_operationInfo.mm_bIsSingleSelection = nNumOfPolygonSelected==1 ? true:false;
	}
	else
	{
		m_operationInfo.mm_eSelectionType = kNothing;
	}
}
//--------------------------------------------------------------------------------------------------
void CEditEventsSink::ShowUCLIDToolbar()
{
	if (m_pIcoMapDrawingCtrl)
	{
		// check if uclidtoolbar is visible or not
		ICommandItemPtr ipCommandItem;
		if (m_pIcoMapDrawingCtrl->FindCommandItem(L"UCLIDArcGISUtils.ArcMapToolbar", ipCommandItem))
		{
			ICommandBarPtr ipCommandBar(ipCommandItem);
			// found uclid toolbar
			if (ipCommandBar)
			{
				VARIANT_BOOL bVisible = ipCommandBar->IsVisible();
				// if not visible, show the toolbar
				if (bVisible == VARIANT_FALSE)
				{
					ipCommandBar->Dock(esriDockShow, NULL);
				}
			}
		}
	}
}
//--------------------------------------------------------------------------------------------------
bool CEditEventsSink::OperationInfo::isOperable()
{
	bool bIsOperable = false;

	if (mm_eTargetLayerType == kOther || mm_eTargetLayerType == kNothing)
	{
		return bIsOperable;
	}

	if (mm_strCurrentTaskName == CREATE_NEW_FEATURE)
	{
		// if current layer is line or polygon
		switch (mm_eTargetLayerType)
		{
		case CEditEventsSink::kLine:
		case CEditEventsSink::kPolygon:
			bIsOperable = true;
			break;
		}
	}
	else if (mm_strCurrentTaskName == RESHAPE_FEATURE
			 || mm_strCurrentTaskName == MODIFY_FEATURE)
	{
		switch (mm_eSelectionType)
		{
		case CEditEventsSink::kLine:
		case CEditEventsSink::kPolygon:
			{
				// only allow single selection of line or polygon feature
				if (mm_bIsSingleSelection)
				{
					bIsOperable = true;
				}
			}
			break;
		}
	}
	else if (mm_strCurrentTaskName == CUT_POLYGON_FEATURES)
	{
		// one or more polygon feature selected
		if (mm_eSelectionType == CEditEventsSink::kPolygon)
		{
			bIsOperable = true;
		}
	}
	else if (mm_strCurrentTaskName == EXTEND_TRIM_FEATURE)
	{
		// one or more line feature selected
		if (mm_eSelectionType == CEditEventsSink::kLine)
		{
			bIsOperable = true;
		}
	}
	else if (mm_strCurrentTaskName == AUTO_COMPLETE_POLYGON)
	{
		// if current target layer is polygon
		if (mm_eTargetLayerType == CEditEventsSink::kPolygon)
		{
			bIsOperable = true;
		}
	}
	else if (mm_strCurrentTaskName == SELECT_FEATURE_WITH_LINE
			 || mm_strCurrentTaskName == SELECT_FEATURE_WITH_AREA)
	{
		bIsOperable = true;
	}
	else if (mm_strCurrentTaskName == CREATE_2POINT_LINE_FEATURE)
	{
		// if current target layer is line
		if (mm_eTargetLayerType == CEditEventsSink::kLine)
		{
			bIsOperable = true;
		}
	}

	return bIsOperable;
}