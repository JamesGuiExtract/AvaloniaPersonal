// ArcGISDisplayAdapter.cpp : Implementation of CArcGISDisplayAdapter
#include "stdafx.h"
#include "ArcGISUtils.h"
#include "ArcGISDisplayAdapter.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <mathUtil.h>
#include <Bearing.hpp>
#include <LicenseMgmt.h>
#include <RegistryPersistenceMgr.h>
#include <ValueRestorer.h>
#include <RegConstants.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>
#include <IcoMapOptions.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const string CArcGISDisplayAdapter::ROOT_FOLDER = gstrREG_ROOT_KEY + "\\ArcGISUtils\\ArcGISDisplayAdapter";
const string CArcGISDisplayAdapter::GENERAL_FOLDER = "\\General";
const string CArcGISDisplayAdapter::TOOLNAME_GUID_FOLDER = "\\ToolNameToGUID";
const string CArcGISDisplayAdapter::GROUNDTOGRID_KEY = "GroundToGridOn";

//-------------------------------------------------------------------------------------------------
// CArcGISDisplayAdapter
//-------------------------------------------------------------------------------------------------
CArcGISDisplayAdapter::CArcGISDisplayAdapter()
: m_ipArcMapApp(NULL),
  m_ipArcMapEditor(NULL),
  m_ipCurrentSketch(NULL),
  m_ipCurrentPart(NULL),
  m_bIsDrawingSketch(false),
  m_bIsPolygon(false),
  m_dDistanceFactor(1.0),
  m_dAngleOffset(0.0)
{
}
//-------------------------------------------------------------------------------------------------
CArcGISDisplayAdapter::~CArcGISDisplayAdapter()
{
}

//-------------------------------------------------------------------------------------------------
// ISupportErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcGISDisplayAdapter::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IDisplayAdapter,
		&UCLID_COMLMLib::IID_ILicensedComponent,
		&IID_IArcGISDependentComponent
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcGISDisplayAdapter::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		// if validateLicense doesn't throw any exception, then pbValue is true
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IDisplayAdapter
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcGISDisplayAdapter::raw_AddLineSegment(UCLID_FEATUREMGMTLib::ILineSegment *pLineSegment, 
													   BSTR* segmentID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		validateObjects();

		// get starting point from the current sketch
		/*esriGeometry::*/IPointPtr ipESRILastPoint = getLastPointFromSketch();
		if (ipESRILastPoint == NULL)
		{
			throw UCLIDException("ELI11660", "Please set a starting point before adding any segments.");
		}

		ICartographicPointPtr ipStartPoint(CLSID_CartographicPoint);
		ASSERT_RESOURCE_ALLOCATION("ELI11665", ipStartPoint != NULL);
		ipStartPoint->InitPointInXY(ipESRILastPoint->GetX(), ipESRILastPoint->GetY());

		// convert the line segment if Ground-To-Grid is on
		UCLID_FEATUREMGMTLib::IESSegmentPtr ipConvertedSegment 
											= convertSegment(pLineSegment);

		// whether or not this line requires tangent in
		bool bRequireTangentIn = 
			ipConvertedSegment->requireTangentInDirection() == VARIANT_TRUE;

		if (bRequireTangentIn)
		{
			// get tangent out of last segment from current sketch
			string strLastTangentOutBearing 
							= getLastTangentOutBearingAsStringValue();
			// store it in the line segment
			ipConvertedSegment->setTangentInDirection(
							_bstr_t(strLastTangentOutBearing.c_str()));
		}

		// convert to esri segment
		/*esriGeometry::*/IESRSegmentPtr ipESRSegment 
						= convertLineSegmentToESRISegment(ipStartPoint, ipConvertedSegment);

		addSegment(ipESRSegment);

		// no segment id to return, just set it to empty string
		*segmentID = _bstr_t("").copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11425")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcGISDisplayAdapter::raw_AddCurveSegment(UCLID_FEATUREMGMTLib::IArcSegment *pArcSegment, 
														BSTR* segmentID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		validateObjects();

		// get starting point from the current sketch
		/*esriGeometry::*/IPointPtr ipESRILastPoint = getLastPointFromSketch();
		if (ipESRILastPoint == NULL)
		{
			throw UCLIDException("ELI11645", "Please set a starting point before adding any segments.");
		}

		ICartographicPointPtr ipStartPoint(CLSID_CartographicPoint);
		ASSERT_RESOURCE_ALLOCATION("ELI11644", ipStartPoint != NULL);
		ipStartPoint->InitPointInXY(ipESRILastPoint->GetX(), ipESRILastPoint->GetY());

		// do the Ground-To-Grid conversion if it's on
		UCLID_FEATUREMGMTLib::IESSegmentPtr ipConvertedSegment 
											= convertSegment(pArcSegment);

		// whether or not this arc requires tangent in
		bool bRequireTangentIn = 
			ipConvertedSegment->requireTangentInDirection() == VARIANT_TRUE;

		if (bRequireTangentIn)
		{
			try
			{
				// get tangent out of last segment from current sketch
				string strLastTangentOutBearing 
								= getLastTangentOutBearingAsStringValue();
				// store it in the arc segment
				ipConvertedSegment->setTangentInDirection(
								_bstr_t(strLastTangentOutBearing.c_str()));
			}
			catch(...)
			{
				// if failed to get tangent out, ignore
				// it and move on since we have tangent-in already
				// (even though it might not be as perfect)
			}
		}
		// convert to esri segment
		/*esriGeometry::*/IESRSegmentPtr ipESRSegment 
						= convertArcSegmentToESRISegment(ipStartPoint, ipConvertedSegment);

		addSegment(ipESRSegment);

		// no segment id to return, just set it to empty string
		*segmentID = _bstr_t("").copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11426")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcGISDisplayAdapter::raw_FinishCurrentSketch(BSTR* featureID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		validateObjects();

		*featureID = _bstr_t("").copy();

		/*esriEditor::*/IEditSketchPtr ipEditSketch(m_ipArcMapEditor);
		ASSERT_RESOURCE_ALLOCATION("ELI11676", ipEditSketch != NULL);

		ipEditSketch->FinishSketch();

		m_ipCurrentPart = NULL;
		m_ipCurrentSketch = NULL;

		IMxDocumentPtr ipMxDoc(m_ipArcMapApp->Document);
		ASSERT_RESOURCE_ALLOCATION("ELI11677", ipMxDoc != NULL);
		// refresh display
		ipMxDoc->ActiveView->Refresh();

		/*esriGeoDatabase::*/IEnumFeaturePtr ipEnumFeature = ipMxDoc->FocusMap->FeatureSelection;
		ASSERT_RESOURCE_ALLOCATION("ELI11678", ipEnumFeature != NULL);
		ipEnumFeature->Reset();

		// set this feature equal to the first element in the feature selection set
		/*esriGeoDatabase::*/IFeaturePtr ipFeature = ipEnumFeature->Next();
		if (ipFeature != NULL)
		{
			if (ipFeature->HasOID == VARIANT_TRUE)
			{
				*featureID = _bstr_t(ipFeature->OID).copy();
			}
		}

		// inverse each segment if current task is "Create 2-Point Line Features"
		/*esriEditor::*/IEditTaskPtr ipEditTask(m_ipArcMapEditor->CurrentTask);
		ASSERT_RESOURCE_ALLOCATION("ELI11679", ipEditTask != NULL);
		string strCreate2PntLine("Create 2-Point Line Features");
		string strCurrentTaskName = _bstr_t(ipEditTask->Name);
		if (_stricmp(strCreate2PntLine.c_str(), strCurrentTaskName.c_str()) == 0)
		{
			// Update the COGO Attributes (P10 #3109)
			bool bResult = executeCommandWithName("Editor_UpdateCOGOAttributesCommand");
			if (!bResult)
			{
				// Use older command string
				executeCommandWithName("Editor_InverseCommand");
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11427")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcGISDisplayAdapter::raw_DeleteCurrentSketch()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		validateObjects();

		// call Delete Sketch command from the toolbar, 
		// refer to ArcMap IDs from ArcGIS developer help
		executeCommandWithGUID("{FD799455-472C-11D2-84D8-0000F875B9C6}");

		IMxDocumentPtr ipMxDoc(m_ipArcMapApp->Document);
		ASSERT_RESOURCE_ALLOCATION("ELI11639", ipMxDoc != NULL);

		m_ipCurrentPart = NULL;
		m_ipCurrentSketch = NULL;
		m_bIsDrawingSketch = false;

		// refresh display
		ipMxDoc->ActiveView->Refresh();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11428")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcGISDisplayAdapter::raw_FinishCurrentPart()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		validateObjects();

		/*esriEditor::*/IEditSketchPtr ipEditSketch(m_ipArcMapEditor);
		ASSERT_RESOURCE_ALLOCATION("ELI11637", ipEditSketch != NULL);

		ipEditSketch->RefreshSketch();
		ipEditSketch->FinishSketchPart();

		// set current part object to null
		m_ipCurrentPart = NULL;
		// create new geometry
		if (m_bIsPolygon)
		{
			m_ipCurrentPart.CreateInstance(/*esriGeometry::*/CLSID_Ring);
		}
		else
		{
			m_ipCurrentPart.CreateInstance(/*esriGeometry::*/CLSID_Path);
		}

		ASSERT_RESOURCE_ALLOCATION("ELI11638", m_ipCurrentPart != NULL);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11429")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcGISDisplayAdapter::raw_EraseLastSegment()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		validateObjects();

		if (m_ipCurrentSketch != NULL)
		{
			/*esriEditor::*/IEditSketchPtr ipEditSketch(m_ipArcMapEditor);
			ASSERT_RESOURCE_ALLOCATION("ELI11669", ipEditSketch != NULL);
			
			// record the sketch operation
			/*esriEditor::*/ISketchOperationPtr ipSketchOp(/*esriEditor::*/CLSID_SketchOperation);
			ASSERT_RESOURCE_ALLOCATION("ELI11674", ipSketchOp != NULL);
			// now mark the start of this operation (i.e. adding a 
			// new segment to the current part)
			ipSketchOp->Start(m_ipArcMapEditor);

			/*esriGeoDatabase::*/IInvalidAreaPtr ipRefresh(/*esriCarto::*/CLSID_InvalidArea);
			ASSERT_RESOURCE_ALLOCATION("ELI12513", ipRefresh != NULL);
			// add current sketch to the region whose envelopes can be refreshed
			ipRefresh->Add(ipEditSketch->Geometry);

			// set current sketch
			m_ipCurrentSketch = ipEditSketch->Geometry;
			ASSERT_RESOURCE_ALLOCATION("ELI11670", m_ipCurrentSketch != NULL);
			
			// if current sketch has at least one part
			// set to the last part
			long nNumOfParts = m_ipCurrentSketch->GeometryCount;
			if (nNumOfParts > 0)
			{
				m_ipCurrentPart = m_ipCurrentSketch->GetGeometry(nNumOfParts-1);
			}
			
			// each part is composed by a collection of points
			/*esriGeometry::*/IPointCollectionPtr ipPointCollection(m_ipCurrentPart);
			ASSERT_RESOURCE_ALLOCATION("ELI11671", ipPointCollection != NULL);
			// get current start point of the part, we will use it later
			/*esriGeometry::*/IPointPtr ipStartPoint(/*esriGeometry::*/CLSID_Point);
			ipStartPoint->PutCoords(ipPointCollection->GetPoint(0)->GetX(),
				ipPointCollection->GetPoint(0)->GetY());
			
			// each part is also composed by a collection of segments
			/*esriGeometry::*/IESRSegmentCollectionPtr ipSegCollection(m_ipCurrentPart);
			ASSERT_RESOURCE_ALLOCATION("ELI11672", ipSegCollection != NULL);
			
			// how many segments are there?
			long nNumOfSegments = ipSegCollection->SegmentCount;
			// doesn't make any sense to remove a 
			// segment if there's no segment in the part
			if (nNumOfSegments > 0)
			{
				long nNumOfSegmentsToRemove = 1;
				if (m_bIsPolygon)
				{
					// polygon is little bit trickier
					nNumOfSegmentsToRemove = 2;
				}

				// remove last segment, do not close the gap if any
				ipSegCollection->RemoveSegments(nNumOfSegments-nNumOfSegmentsToRemove, 
												nNumOfSegmentsToRemove, VARIANT_FALSE);

				// if currently no segment left in this part, then add 
				// the starting point for this part.
				// Note that the starting point is removed in addSegment()
				if (ipSegCollection->SegmentCount == 0)
				{
					ipPointCollection->AddPoint(ipStartPoint);
				}

				// close polygon
				if (m_bIsPolygon)
				{
					/*esriGeometry::*/IRingPtr ipRing = ipSegCollection;
					ASSERT_RESOURCE_ALLOCATION("ELI11675", ipRing != NULL);
					// when Close is called upon the ring, a segment is added
					// to the ring from FromPoint to the ToPoint of the ring
					ipRing->Close();
				}
			}
			
			// put the operation onto the operation stack
			ipSketchOp->Finish(ipEditSketch->Geometry->Envelope);
			
			// refresh the sketch for display purpose
			ipRefresh->Add(m_ipCurrentPart);
			ipRefresh->PutRefDisplay(m_ipArcMapEditor->Display);
			ipRefresh->Invalidate(esriNoScreenCache);
			ipEditSketch->RefreshSketch();

			IMxDocumentPtr ipMxDoc(m_ipArcMapApp->Document);
			ASSERT_RESOURCE_ALLOCATION("ELI11673", ipMxDoc != NULL);
			// refresh display
			ipMxDoc->ActiveView->Refresh();
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11430")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcGISDisplayAdapter::raw_get_SupportsPartCreation(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pVal = VARIANT_TRUE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11431")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcGISDisplayAdapter::raw_get_SupportsSketchCreation(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pVal = VARIANT_TRUE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11432")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcGISDisplayAdapter::raw_SelectDefaultTool()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		validateObjects();
		
		// set to Edit Tool if it's in editing mode, otherwise
		// set to Select Tool

		// if it's in editing mode
		// Edit tool (the "Edit Tool" on Editor toolbar)
		/*esriFramework::*/ICommandItemPtr ipTool = getCommandItemByName("Editor_EditTool");
		if (ipTool)
		{
			/*esriSystemUI::*/IESRCommandPtr ipCommand(ipTool);
			if (ipCommand != NULL && ipCommand->Enabled == VARIANT_TRUE)
			{
				// if this tool is the current tool already, do not select again
				if (areSameCommandItems(m_ipArcMapApp->CurrentTool, ipTool))
				{
					return S_OK;
				}

				ipTool->Execute();
				return S_OK;
			}
		}

		// not in editing mode
		// Select Tool (on PageLayout toolbar)
		ipTool = getCommandItemByName("PageLayout_SelectTool");
		if (ipTool)
		{
			/*esriSystemUI::*/IESRCommandPtr ipCommand(ipTool);
			if (ipCommand != NULL && ipCommand->Enabled == VARIANT_TRUE)
			{
				// if this tool is the current tool already, do not select again
				if (areSameCommandItems(m_ipArcMapApp->CurrentTool, ipTool))
				{
					return S_OK;
				}

				ipTool->Execute();
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11433")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcGISDisplayAdapter::raw_GetLastPoint(double* dX, double* dY, VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		validateObjects();

		*pVal = VARIANT_FALSE;

		/*esriGeometry::*/IPointPtr ipLastPoint = getLastPointFromSketch();

		if (ipLastPoint == NULL)
		{
			return S_OK;
		}

		*dX = ipLastPoint->GetX();
		*dY = ipLastPoint->GetY();

		*pVal = VARIANT_TRUE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11434")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcGISDisplayAdapter::raw_Reset(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		validateObjects();

		m_ipCurrentPart = NULL;
		m_ipCurrentSketch = NULL;
		m_bIsDrawingSketch = false;

		// refresh the display
		IMxDocumentPtr ipMxDoc(m_ipArcMapApp->Document);
		ASSERT_RESOURCE_ALLOCATION("ELI11628", ipMxDoc != NULL);
		ipMxDoc->ActiveView->Refresh();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11435")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcGISDisplayAdapter::raw_SelectFeatures(BSTR strCommonSourceDoc)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		validateObjects();

		string strSourceDocName = _bstr_t(strCommonSourceDoc);

		IMxDocumentPtr ipMxDoc(m_ipArcMapApp->Document);
		ASSERT_RESOURCE_ALLOCATION("ELI11701", ipMxDoc != NULL);
		/*esriCarto::*/IMapPtr ipMap(ipMxDoc->FocusMap);
		ASSERT_RESOURCE_ALLOCATION("ELI11702", ipMap != NULL);

		// how many feature layers are there in the current document
		long nNumOfLayers = ipMap->LayerCount;

		// vector of feature ids
		vector<string> vecFeatureIDs;

		// what's the selection method?
		/*esriCarto::*/esriSelectionResultEnum selectionMethod = /*esriCarto::*/esriSelectionResultNew;

		// go through all feature layers to get OIDs of those features
		// that have strCommonSourceDoc as one of their hyper links
		for (long n=0; n<nNumOfLayers; n++)
		{
			// empty the feature id vector first
			vecFeatureIDs.clear();

			/*esriCarto::*/ILayerPtr ipLayer = ipMap->GetLayer(n);
			ASSERT_RESOURCE_ALLOCATION("ELI11703", ipLayer != NULL);
			/*esriCarto::*/IFeatureSelectionPtr ipFeatureSelection = ipLayer;
			ASSERT_RESOURCE_ALLOCATION("ELI11704", ipFeatureSelection != NULL);

			// before select any feature, clear all selections
			ipFeatureSelection->Clear();
			ipMxDoc->ActiveView->Refresh();

			/*esriCarto::*/IHyperlinkContainerPtr ipHyperlinkContainer(ipLayer);
			ASSERT_RESOURCE_ALLOCATION("ELI11705", ipHyperlinkContainer != NULL);
			long nNumOfHyperlinks = ipHyperlinkContainer->HyperlinkCount;

			// go through all hyperlinks on this layer and get
			// feature ids of these hyperlinks
			for (long i=0; i<nNumOfHyperlinks; i++)
			{
				/*esriCarto::*/IHyperlinkPtr ipHyperlink = ipHyperlinkContainer->GetHyperlink(i);
				ASSERT_RESOURCE_ALLOCATION("ELI11706", ipHyperlink != NULL);
				// only add the feature id to the vector if
				// the hyperlink name is same as strCommonSourceDoc
				string strLinkName = _bstr_t(ipHyperlink->Link);
				if (_stricmp(strSourceDocName.c_str(), strLinkName.c_str()) == 0)
				{
					// convert the feature id from long to string
					string strFeatureID = ::asString(ipHyperlink->FeatureId);
					vecFeatureIDs.push_back(strFeatureID);
				}
			}

			/*esriCarto::*/IFeatureLayerPtr ipFeatureLayer(ipLayer);
			ASSERT_RESOURCE_ALLOCATION("ELI11707", ipFeatureLayer != NULL);
			if (vecFeatureIDs.size() > 0)
			{
				string strWhereClause("");
				// if we collected all object ids belong to those 
				// features that have strCommonSourceDoc as their 
				// hyperlink, then filter out these features
				for (unsigned int i = 0; i < vecFeatureIDs.size(); i++)
				{
					/*esriGeoDatabase::*/IFeatureClassPtr ipFeatureClass = ipFeatureLayer->FeatureClass;
					ASSERT_RESOURCE_ALLOCATION("ELI11708", ipFeatureClass != NULL);
					// the name for OID field in the database table
					string strOIDName = asString(ipFeatureClass->OIDFieldName);
					// add to the where clause
					if (i > 0)
					{
						strWhereClause += " OR ";
					}

					strWhereClause += strOIDName + " = " + vecFeatureIDs[i];
				}

				if (!strWhereClause.empty())
				{
					// create a query filter
					/*esriGeoDatabase::*/IQueryFilterPtr ipQFilter(/*esriGeoDatabase::*/CLSID_QueryFilter);
					ASSERT_RESOURCE_ALLOCATION("ELI11709", ipQFilter != NULL);
					ipQFilter->WhereClause = _bstr_t(strWhereClause.c_str());

					// select the features
					ipFeatureSelection->SelectFeatures(ipQFilter, selectionMethod, VARIANT_FALSE);
					// refresh the selected part of view
					ipMxDoc->ActiveView->PartialRefresh(/*esriCarto::*/esriViewGeoSelection, NULL, NULL);
				}
			}

			// next set will be appended to the existing result set
			selectionMethod = /*esriCarto::*/esriSelectionResultAdd;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11436")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcGISDisplayAdapter::raw_GetCurrentDistanceUnit(EDistanceUnitType *eCurrentUnitType)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		validateObjects();

		IMxDocumentPtr ipMxDoc(m_ipArcMapApp->Document);
		ASSERT_RESOURCE_ALLOCATION("ELI11624", ipMxDoc != NULL);
		/*esriCarto::*/IMapPtr ipMap(ipMxDoc->FocusMap);
		ASSERT_RESOURCE_ALLOCATION("ELI11625", ipMap != NULL);

		EDistanceUnitType eUnitType = kUnknownUnit;

		esriUnits currentUnit = ipMap->MapUnits;
		switch (currentUnit)
		{
		case esriInches:
			eUnitType = kInches;
			break;
		case esriFeet:
			eUnitType = kFeet;
			break;
		case esriYards:
			eUnitType = kYards;
			break;
		case esriMiles:
			eUnitType = kMiles;
			break;
		case esriCentimeters:
			eUnitType = kCentimeters;
			break;
		case esriMeters:
			eUnitType = kMeters;
			break;
		case esriKilometers:
			eUnitType = kKilometers;
			break;
		}

		*eCurrentUnitType = eUnitType;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11437")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcGISDisplayAdapter::raw_SetFeatureGeometry(BSTR strFeatureID, 
														   UCLID_FEATUREMGMTLib::IUCLDFeature *ipUCLIDFeature)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		validateObjects();

		IMxDocumentPtr ipMxDoc(m_ipArcMapApp->Document);
		ASSERT_RESOURCE_ALLOCATION("ELI11693", ipMxDoc != NULL);

		// if there's only one feature selected
		if (ipMxDoc->FocusMap->SelectionCount == 1)
		{
			/*esriGeoDatabase::*/IEnumFeaturePtr ipEnumFeature = ipMxDoc->FocusMap->FeatureSelection;
			ASSERT_RESOURCE_ALLOCATION("ELI11694", ipEnumFeature != NULL);
			ipEnumFeature->Reset();
			/*esriGeoDatabase::*/IFeaturePtr ipSelectedFeature = ipEnumFeature->Next();
			ASSERT_RESOURCE_ALLOCATION("ELI11695", ipSelectedFeature != NULL);

			// record the operation
			m_ipArcMapEditor->StartOperation();

			if (ipUCLIDFeature == NULL)
			{
				// remove selected feature
				ipSelectedFeature->Delete();
			}
			else
			{
				/*esriGeometry::*/IGeometryPtr ipGeometry(ipSelectedFeature->Shape);
				ASSERT_RESOURCE_ALLOCATION("ELI11696", ipGeometry != NULL);
				if (ipGeometry->GeometryType == /*esriGeometry::*/esriGeometryPolyline)
				{
					ipUCLIDFeature->setFeatureType(kPolyline);
				}
				else if (ipGeometry->GeometryType == /*esriGeometry::*/esriGeometryPolygon)
				{
					ipUCLIDFeature->setFeatureType(kPolygon);
				}
				else
				{
					throw UCLIDException("ELI11697", "We only support polyline and polygon feature creation.");
				}

				setFeature(ipGeometry, ipUCLIDFeature);
				// store it in the database
				ipSelectedFeature->Shape = ipGeometry;
				// commit the transaction in the database
				ipSelectedFeature->Store();
			}

			m_ipArcMapEditor->StopOperation(_bstr_t("Modified selected feature."));
			ipMxDoc->ActiveView->Refresh();
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11438")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcGISDisplayAdapter::raw_GetFeatureGeometry(BSTR strFeatureID, 
														   UCLID_FEATUREMGMTLib::IUCLDFeature **ipUCLIDFeature)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		validateObjects();
/*
TODO: what to do with strFeatureID? Now we only select one feature at a time and
traverse the feature to get each segment info, and then store them in uclid IUCLDFeature.
strFeatureID is not used here so far since the feature id is only unique at the 
layer level, not the whole map level.
*/

/*
		***** IMPORTANT!!! *******
		For each sketch that is about to finish, it will be more accurate to
		obtain the current sketch info than to obtain info from the feature that
		will be created from the current sketch.
		This will preserve as much original measurement as we can for each feature.
		We will have the "convention" that if the strFeatureID equals to "Sketch",
		the current geometry of the current sketch shall be retrieved
		**************************
*/
		*ipUCLIDFeature = NULL;

		IMxDocumentPtr ipMxDoc(m_ipArcMapApp->Document);
		ASSERT_RESOURCE_ALLOCATION("ELI11689", ipMxDoc != NULL);

		/*esriEditor::*/IEditSketchPtr ipEditSketch = m_ipArcMapEditor;
		ASSERT_RESOURCE_ALLOCATION("ELI11688", ipEditSketch != NULL);

		/*esriGeometry::*/IGeometryPtr ipGeometry(NULL);
		string stdstrFeatureID = _bstr_t(strFeatureID);
		if (stdstrFeatureID == "Sketch" 
			&& ipEditSketch->Geometry->GetIsEmpty() == VARIANT_FALSE)
		{
			// set to equal to current sketch
			ipGeometry = ipEditSketch->Geometry;
		}
		// if there's only one feature selected in the map
		else if (ipMxDoc->FocusMap->SelectionCount == 1)
		{
			/*esriGeoDatabase::*/IEnumFeaturePtr ipEnumFeature = ipMxDoc->FocusMap->FeatureSelection;
			ASSERT_RESOURCE_ALLOCATION("ELI11690", ipEnumFeature != NULL);
			ipEnumFeature->Reset();
			/*esriGeoDatabase::*/IFeaturePtr ipSelectedFeature = ipEnumFeature->Next();
			ASSERT_RESOURCE_ALLOCATION("ELI11691", ipSelectedFeature != NULL);

			ipGeometry = ipSelectedFeature->Shape;
		}

		if (ipGeometry == NULL)
		{
			return S_OK;
		}

		if (ipGeometry->GetIsEmpty() == VARIANT_TRUE)
		{
			return S_OK;
		}

		// create a new uclid feature
		UCLID_FEATUREMGMTLib::IUCLDFeaturePtr ipUCLIDNewFeature(__uuidof(UCLID_FEATUREMGMTLib::Feature));
		ASSERT_RESOURCE_ALLOCATION("ELI11692", ipUCLIDNewFeature != NULL);

		// if the feature is of type polyline or polygon
		if (ipGeometry->GeometryType == /*esriGeometry::*/esriGeometryPolyline)
		{
			ipUCLIDNewFeature->setFeatureType(kPolyline);
		}
		else if (ipGeometry->GeometryType == /*esriGeometry::*/esriGeometryPolygon)
		{
			ipUCLIDNewFeature->setFeatureType(kPolygon);
		}
		else
		{
			throw UCLIDException("ELI11698", "We only support polyline and polygon feature creation.");
		}

		getFeature(ipGeometry, ipUCLIDNewFeature);

		*ipUCLIDFeature = ipUCLIDNewFeature.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11439")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcGISDisplayAdapter::raw_SetStartPointForNextPart(double dX, double dY)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		validateObjects();

		// create a start point
		/*esriGeometry::*/IPointPtr ipStartPoint(/*esriGeometry::*/CLSID_Point);
		ASSERT_RESOURCE_ALLOCATION("ELI11681", ipStartPoint != NULL);
		ipStartPoint->PutCoords(dX, dY);

		/*esriEditor::*/IEditSketchPtr ipEditSketch = m_ipArcMapEditor;
		ASSERT_RESOURCE_ALLOCATION("ELI11680", ipEditSketch != NULL);
		ipEditSketch->AddPoint(ipStartPoint, VARIANT_TRUE);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11440")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcGISDisplayAdapter::raw_SelectTool(BSTR strToolName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		validateObjects();

		string strName = _bstr_t(strToolName);
		// if key doesn't exist, throw exception
		if (!getConfigManager()->keyExists(TOOLNAME_GUID_FOLDER, strName))
		{
			UCLIDException ue("ELI11643", "Internal error: Invalid tool name!");
			ue.addDebugInfo("Tool Name", strName);
			throw ue;
		}

		// look in the Registry for GUID associated with the Tool Name
		string strCommandGUID = getConfigManager()->getKeyValue(TOOLNAME_GUID_FOLDER, strName);

		ICommandItemPtr ipTool = getCommandItemByGUID(strCommandGUID);
		// select the tool if it's enabled
		if (ipTool)
		{
			/*esriSystemUI::*/IESRCommandPtr ipCommand(ipTool);
			if (ipCommand != NULL && ipCommand->Enabled == VARIANT_TRUE)
			{
				// if this tool is not current tool
				if (!areSameCommandItems(m_ipArcMapApp->CurrentTool, ipTool))
				{
					// select this tool
					ipTool->Execute();
				}
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11441")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcGISDisplayAdapter::raw_GetLastSegmentTanOutAsPolarAngleInRadians(double *pdTangentOutAngle, 
																				  VARIANT_BOOL *pbSucceeded)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		validateObjects();

		*pbSucceeded = VARIANT_FALSE;

		double dTangentOut = 0.0;
		bool bSuccess = getLastTangentOutInRadians(dTangentOut);

		if (bSuccess)
		{
			*pbSucceeded = VARIANT_TRUE;
			*pdTangentOutAngle = dTangentOut;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11442")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcGISDisplayAdapter::raw_Undo()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		validateObjects();

		/////////
		// Search for "ArcMap IDs" from arcgis developer help
		/////////
		executeCommandWithName("Edit_Undo");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11443")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcGISDisplayAdapter::raw_Redo()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		validateObjects();

		/////////
		// Search for "ArcMap IDs" from arcgis developer help
		/////////
		executeCommandWithName("Edit_Redo");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11444")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcGISDisplayAdapter::raw_GetFeatureType(long *peFeatureType)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		validateObjects();

		EFeatureType eFeatureType = kInvalidFeatureType;

		/*esriEditor::*/IEditSketchPtr ipEditSketch(m_ipArcMapEditor);
		/*esriGeometry::*/esriGeometryType geoType = ipEditSketch->GeometryType;

		if (geoType == /*esriGeometry::*/esriGeometryPolyline)
		{
			eFeatureType = kPolyline;
		}
		else if (geoType == /*esriGeometry::*/esriGeometryPolygon)
		{
			eFeatureType = kPolygon;
		}
		else
		{
			throw UCLIDException("ELI12127", "Unable to determine current feature layer type.");
		}

		*peFeatureType = (long)eFeatureType;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12126")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcGISDisplayAdapter::raw_FlashSegment(long nPartIndex, long nSegmentIndex)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		validateObjects();

		// get the specified part
		if (m_ipCurrentSketch != NULL)
		{	
			long nNumOfParts = m_ipCurrentSketch->GeometryCount;
			if (nPartIndex >= nNumOfParts)
			{
				UCLIDException ue("ELI12048", "Invalid part index");
				ue.addDebugInfo("Part Index", nPartIndex);
				throw ue;
			}

			// get the part
			/*esriGeometry::*/IGeometryPtr ipGeometry(m_ipCurrentSketch->GetGeometry(nPartIndex));
			ASSERT_RESOURCE_ALLOCATION("ELI12049", ipGeometry != NULL);

			// get the specified segment
			/*esriGeometry::*/IESRSegmentCollectionPtr ipSegmentCol(ipGeometry);
			ASSERT_RESOURCE_ALLOCATION("ELI12050", ipSegmentCol != NULL);

			long nNumOfSegments = ipSegmentCol->SegmentCount;
			if (nSegmentIndex >= nNumOfSegments)
			{
				UCLIDException ue("ELI12051", "Invalid segment index");
				ue.addDebugInfo("Segment Index", nNumOfSegments);
				throw ue;
			}

			/*esriGeometry::*/IESRSegmentPtr ipSegment(ipSegmentCol->GetSegment(nSegmentIndex));
			ASSERT_RESOURCE_ALLOCATION("ELI12052", ipSegment != NULL);
			
			// what's the segment type, line or arc?
			/*esriGeometry::*/IPointCollectionPtr ipPolyline(/*esriGeometry::*/CLSID_Polyline);
			ASSERT_RESOURCE_ALLOCATION("ELI12408", ipPolyline != NULL);

			/*esriGeometry::*/IPointPtr ipStartPoint = ipSegment->GetFromPoint();
			/*esriGeometry::*/IPointPtr ipEndPoint = ipSegment->GetToPoint();
			/*esriGeometry::*/esriGeometryType eGeoType = ipSegment->GeometryType;
			if (eGeoType == /*esriGeometry::*/esriGeometryLine)
			{
				// directly add start and end point to the polyline
				ipPolyline->AddPoint(ipStartPoint);
				ipPolyline->AddPoint(ipEndPoint);
			}
			else if (eGeoType == /*esriGeometry::*/esriGeometryCircularArc)
			{
				// get the center point of the arc
				/*esriGeometry::*/ICircularArcPtr ipArc(ipSegment);
				double dCentralAngle = ipArc->GetCentralAngle();
				VARIANT_BOOL bCCW = ipArc->GetIsCounterClockwise();

				// create a circular arc
				/*esriGeometry::*/ICircularArcPtr ipCircularArc(/*esriGeometry::*/CLSID_CircularArc);
				ASSERT_RESOURCE_ALLOCATION("ELI12406", ipCircularArc != NULL);
				/*esriGeometry::*/IConstructCircularArcPtr ipConstructArc(ipCircularArc);
				
				ipConstructArc->ConstructEndPointsAngle(ipStartPoint, 
					ipEndPoint, bCCW, dCentralAngle);
				/*esriGeometry::*/IESRSegmentPtr ipNewArc(ipCircularArc);

				/*esriGeometry::*/IESRSegmentCollectionPtr ipSegCol(ipPolyline);
				ipSegCol->AddSegment(ipNewArc);
			}
			else
			{
				throw UCLIDException("ELI12410", "Invalid segment type.");
			}

			// QI
			ipGeometry = ipPolyline;
			ASSERT_RESOURCE_ALLOCATION("ELI12053", ipGeometry != NULL);

			IMxDocumentPtr ipMxDoc(m_ipArcMapApp->Document);
			IScreenDisplayPtr ipScreenDisplay(ipMxDoc->ActiveView->ScreenDisplay);
			ASSERT_RESOURCE_ALLOCATION("ELI12044", ipScreenDisplay != NULL);
			
			ipScreenDisplay->StartDrawing(0, esriNoScreenCache);
			
			ISimpleLineSymbolPtr ipSimpleLineSymbol(CLSID_SimpleLineSymbol);
			ASSERT_RESOURCE_ALLOCATION("ELI12043", ipSimpleLineSymbol != NULL);
			
			ISymbolPtr ipSymbol(ipSimpleLineSymbol);
			// erase itself when drawn twice
			ipSymbol->ROP2 = esriROPNotXOrPen;
			/*esriDisplay::*/IRgbColorPtr ipColor(CLSID_RgbColor);
			ASSERT_RESOURCE_ALLOCATION("ELI12045", ipColor != NULL);
			// make it red
			ipColor->PutESRI_RGB(RGB(255, 0, 0));
			ipSimpleLineSymbol->Color = ipColor;
			ipSimpleLineSymbol->Width = 4;

			// draw the highlight line on top of the segment
			ipScreenDisplay->SetSymbol(ipSymbol);
			ipScreenDisplay->DrawPolyline(ipGeometry);
			// make the highlight line stay for a bit while
			::Sleep(100);
			// cancel out the highlight
			ipScreenDisplay->DrawPolyline(ipGeometry);

			ipScreenDisplay->FinishDrawing();
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12042")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcGISDisplayAdapter::raw_UpdateSegments(long nPartIndex,
													   long nStartSegmentIndex, 
													   IIUnknownVector* pUpdatedSegmentsForThisPart)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		validateObjects();

		/*esriEditor::*/IEditSketchPtr ipEditSketch(m_ipArcMapEditor);
		ASSERT_RESOURCE_ALLOCATION("ELI12117", ipEditSketch != NULL);
		if (ipEditSketch->GeometryType == /*esriGeometry::*/esriGeometryPolyline)
		{
			m_bIsPolygon = false;
		}
		else if (ipEditSketch->GeometryType == /*esriGeometry::*/esriGeometryPolygon)
		{
			m_bIsPolygon = true;
		}
		else
		{
			throw UCLIDException("ELI12446", "No support for layers other than polyline or polygon.");
		}
		
		/*esriGeoDatabase::*/IInvalidAreaPtr ipRefresh(/*esriCarto::*/CLSID_InvalidArea);
		ASSERT_RESOURCE_ALLOCATION("ELI12118", ipRefresh != NULL);
		// add current sketch to the region whose envelopes can be refreshed
		ipRefresh->Add(ipEditSketch->Geometry);

		// record the sketch operation
		/*esriEditor::*/ISketchOperationPtr ipSketchOp(/*esriEditor::*/CLSID_SketchOperation);
		ASSERT_RESOURCE_ALLOCATION("ELI12120", ipSketchOp != NULL);
		// now mark the start of this operation (i.e. adding a 
		// new segment to the current part)
		ipSketchOp->Start(m_ipArcMapEditor);

		// update current sketch with the latest 
		m_ipCurrentSketch = ipEditSketch->Geometry;

		// how many parts are there in the current sketch?
		long nNumOfParts = m_ipCurrentSketch->GeometryCount;
		if (nPartIndex >= nNumOfParts)
		{
			UCLIDException ue("ELI12119", "Invalid part index number");
			ue.addDebugInfo("Part Index", nPartIndex);
			ue.addDebugInfo("Number of Parts", nNumOfParts);
			throw ue;
		}

		// get the specified part
		/*esriGeometry::*/IESRSegmentCollectionPtr ipSegmentColOfPart 
							= m_ipCurrentSketch->GetGeometry(nPartIndex);

		// what's the actual total number of segments for this sketch part 
		int nCurrentPartTotalSegCount = ipSegmentColOfPart->SegmentCount;

		IIUnknownVectorPtr ipUpdatedAllSegments(pUpdatedSegmentsForThisPart);
		ASSERT_ARGUMENT("ELI12105", ipUpdatedAllSegments != NULL);

		// total number of segments of updated
		long nTotalNumSegmentOfUpdated = ipUpdatedAllSegments->Size();

		// no operation
		if (nStartSegmentIndex < 0
			|| nTotalNumSegmentOfUpdated < 1
			|| nStartSegmentIndex >= nTotalNumSegmentOfUpdated
			|| (nStartSegmentIndex > nCurrentPartTotalSegCount
				&& !m_bIsPolygon)
			|| (nStartSegmentIndex > nCurrentPartTotalSegCount-1
				&& m_bIsPolygon))
		{
			return S_OK;
		}

		// remove the rest of the segments starting from the start index
		int nNumOfSegmentToRemove = nCurrentPartTotalSegCount - nStartSegmentIndex;

		if (nNumOfSegmentToRemove > 0)
		{
			// retain the starting point if the inserting index is 0
			/*esriGeometry::*/IPointPtr ipESRIStartPoint(NULL);
			bool bNeedStartPoint = false;
			if (nStartSegmentIndex == 0)
			{
				// store the starting point first
				/*esriGeometry::*/ICurvePtr ipCurve(ipSegmentColOfPart);
				ipESRIStartPoint = ipCurve->GetFromPoint();
				bNeedStartPoint = true;
			}

			ipSegmentColOfPart->RemoveSegments(nStartSegmentIndex, 
				nNumOfSegmentToRemove, VARIANT_FALSE);

			if (m_bIsPolygon)
			{
				/*esriGeometry::*/IRingPtr ipRing(ipSegmentColOfPart);
				ASSERT_RESOURCE_ALLOCATION("ELI12465", ipRing != NULL);
				// close the current part
				ipRing->Close();
			}

			if (bNeedStartPoint && ipESRIStartPoint != NULL)
			{
				// set start point for the part
				/*esriGeometry::*/ICurvePtr ipCurve(ipSegmentColOfPart);
				ipCurve->PutFromPoint(ipESRIStartPoint);
			}
		}
		
		// draw/redraw all segments starting from the 
		// inserted segment
		addToSegmentCollection(ipSegmentColOfPart, ipUpdatedAllSegments,
			nStartSegmentIndex, 
			nTotalNumSegmentOfUpdated - nStartSegmentIndex);

		// close the polygon
		if (m_bIsPolygon)
		{
			/*esriGeometry::*/IRingPtr ipRing(ipSegmentColOfPart);
			ASSERT_RESOURCE_ALLOCATION("ELI12464", ipRing != NULL);
			// close the current part
			ipRing->Close();
		}
		
		// put the operation onto the operation stack
		ipSketchOp->Finish(ipEditSketch->Geometry->Envelope);

		// refresh the sketch for display purpose
		/*esriGeometry::*/ICurvePtr ipThisPart(ipSegmentColOfPart);
		ipRefresh->Add(ipThisPart);
		ipRefresh->PutRefDisplay(m_ipArcMapEditor->Display);
		ipRefresh->Invalidate(esriNoScreenCache);
		ipEditSketch->RefreshSketch();

		IMxDocumentPtr ipMxDoc(m_ipArcMapApp->Document);
		// refresh display
		ipMxDoc->ActiveView->Refresh();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12089")
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IArcGISDependentComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CArcGISDisplayAdapter::SetApplicationHook(IDispatch *pApp)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Do not call validate license here - it will cause an exception to 
		// be thrown before ArcMap window comes up
		//validateLicense();

		m_ipArcMapApp = pApp;
		ASSERT_RESOURCE_ALLOCATION("ELI11467", m_ipArcMapApp != NULL);
		IUIDPtr ipID(CLSID_UID);
		ASSERT_RESOURCE_ALLOCATION("ELI11468", ipID != NULL);
		ipID->Value = _variant_t(_bstr_t("esriEditor.editor"));

		m_ipArcMapEditor = m_ipArcMapApp->FindExtensionByCLSID(ipID);
		ASSERT_RESOURCE_ALLOCATION("ELI11473", m_ipArcMapEditor != NULL);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11462")
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CArcGISDisplayAdapter::addSegment(/*esriGeometry::*/IESRSegmentPtr ipSegmentToAdd)
{
	/*esriEditor::*/IEditSketchPtr ipEditSketch(m_ipArcMapEditor);
	ASSERT_RESOURCE_ALLOCATION("ELI11503", ipEditSketch != NULL);
	
	/*esriGeoDatabase::*/IInvalidAreaPtr ipRefresh(/*esriCarto::*/CLSID_InvalidArea);
	ASSERT_RESOURCE_ALLOCATION("ELI11505", ipRefresh != NULL);
	// add current sketch to the region whose envelopes can be refreshed
	ipRefresh->Add(ipEditSketch->Geometry);

	// update current sketch with the latest 
	m_ipCurrentSketch = ipEditSketch->Geometry;
	ASSERT_RESOURCE_ALLOCATION("ELI11504", m_ipCurrentSketch != NULL);

	// whether or not the current sketch has only one part
	bool bOnlyOnePart = true;
	// how many parts are there in the current sketch?
	long nNumOfParts = m_ipCurrentSketch->GeometryCount;
	if (nNumOfParts <= 1)
	{
		// this is either the beginning of a sketch or current sketch
		// has only one part
		m_ipCurrentPart = ipEditSketch->Geometry;
	}
	else
	{
		// there are more than one part
		bOnlyOnePart = false;
		// get the last part
		m_ipCurrentPart = m_ipCurrentSketch->GetGeometry(nNumOfParts-1);
	}

	// record the sketch operation
	/*esriEditor::*/ISketchOperationPtr ipSketchOp(/*esriEditor::*/CLSID_SketchOperation);
	ASSERT_RESOURCE_ALLOCATION("ELI11506", ipSketchOp != NULL);
	// now mark the start of this operation (i.e. adding a 
	// new segment to the current part)
	ipSketchOp->Start(m_ipArcMapEditor);

	// current part is composed by a collection of segments
	/*esriGeometry::*/IESRSegmentCollectionPtr ipSegmentColOfCurrentPart = m_ipCurrentPart;
	ASSERT_RESOURCE_ALLOCATION("ELI11507", ipSegmentColOfCurrentPart != NULL);

	// each part is consist of points
	/*esriGeometry::*/IPointCollectionPtr ipPointCol(NULL);
	
	// If it's polygon layer, make sure all rings 
	// are closed after each segment is added
	if (m_bIsPolygon)
	{
		// if there are more than one part, use Ring as current part
		/*esriGeometry::*/IRingPtr ipRing(NULL);
		// if there's only one part or it's the start of the sketch
		// use Polygon as the current part.
		/*esriGeometry::*/IPolygonPtr ipPolygon(NULL);
		// how many segments are there in the current part
		long nNumOfSeg = ipSegmentColOfCurrentPart->SegmentCount;
		if (nNumOfSeg <= 1)
		{
			// if there's at most one segment
			ipSegmentColOfCurrentPart->AddSegment(ipSegmentToAdd);
			if (bOnlyOnePart)
			{
				ipPolygon = ipSegmentColOfCurrentPart;
				ASSERT_RESOURCE_ALLOCATION("ELI11508", ipPolygon != NULL);
				// close the current part
				ipPolygon->Close();

				ipPointCol = ipPolygon;
				ASSERT_RESOURCE_ALLOCATION("ELI11509", ipPointCol != NULL);
			}
			else
			{
				ipRing = ipSegmentColOfCurrentPart;
				ASSERT_RESOURCE_ALLOCATION("ELI11510", ipRing != NULL);
				// close the current part
				ipRing->Close();

				ipPointCol = ipRing;
				ASSERT_RESOURCE_ALLOCATION("ELI11511", ipPointCol != NULL);
			}

            // Remove the very first point that we added as the starting
            // point for the current sketch part
            ipPointCol->RemovePoints(0, 1);		
		}
		else
		{
			// if segment count is more than 1, let's remove the
            // last segment from the ring first
			ipSegmentColOfCurrentPart->RemoveSegments(nNumOfSeg - 1, 1, VARIANT_FALSE);
			// then add the new segment
			ipSegmentColOfCurrentPart->AddSegment(ipSegmentToAdd);
			if (bOnlyOnePart)
			{
				ipPolygon = ipSegmentColOfCurrentPart;
				ASSERT_RESOURCE_ALLOCATION("ELI11513", ipPolygon != NULL);
                // if the polygon is already closed, only add the point to
                // the end of the segment (which moves the red dot from start
				// point to the end point)				
				if (ipPolygon->IsClosed == VARIANT_TRUE)
				{
					ipEditSketch->AddPoint(ipSegmentToAdd->GetToPoint(), VARIANT_FALSE);
				}
				else
				{
					// close the current part
					ipPolygon->Close();
				}
			}
			else
			{
				ipRing = ipSegmentColOfCurrentPart;
				ASSERT_RESOURCE_ALLOCATION("ELI11514", ipRing != NULL);
                // if the polygon is already closed, only add the point to
                // the end of the segment (which moves the red dot from start
				// point to the end point)				
				if (ipRing->IsClosed == VARIANT_TRUE)
				{
					ipEditSketch->AddPoint(ipSegmentToAdd->GetToPoint(), VARIANT_FALSE);
				}
				else
				{
					// close the current part
					ipRing->Close();
				}
			}
		}
	}
	else	// polyline
	{
		ipPointCol = m_ipCurrentPart;
		if (ipSegmentColOfCurrentPart->SegmentCount == 0
			&& ipPointCol->PointCount == 1)
		{
			// remove the redundant starting point
			ipPointCol->RemovePoints(0, 1);
		}

		ipSegmentColOfCurrentPart->AddSegment(ipSegmentToAdd);
	}

	// put the operation onto the operation stack
	ipSketchOp->Finish(ipEditSketch->Geometry->Envelope);

	// refresh the sketch for display purpose
	ipRefresh->Add(m_ipCurrentPart);
	ipRefresh->PutRefDisplay(m_ipArcMapEditor->Display);
	ipRefresh->Invalidate(esriNoScreenCache);
	ipEditSketch->RefreshSketch();
}
//-------------------------------------------------------------------------------------------------
void CArcGISDisplayAdapter::addToSegmentCollection(
									  /*esriGeometry::*/IESRSegmentCollectionPtr ipSegmentCol, 
									  IIUnknownVectorPtr ipOriginUCLIDSegments,
									  long nStartSegmentIndex,
									  long nNumOfSegmentsToAdd)
{
	// make sure start index is valid
	_bstr_t _bstrTangentInDirection("");
	int nSize = ipOriginUCLIDSegments->Size();
	if (nStartSegmentIndex < 0 || nStartSegmentIndex >= nSize)
	{
		UCLIDException ue("ELI12100", "Invalid start segment index");
		ue.addDebugInfo("Start Index", nStartSegmentIndex);
		throw ue;
	}

	ICartographicPointPtr ipUCLIDStartPoint(CLSID_CartographicPoint);
	ASSERT_RESOURCE_ALLOCATION("ELI12102", ipUCLIDStartPoint != NULL);

	// get end point of the segment collection
	/*esriGeometry::*/IPointPtr ipESRIEndPoint = getEndPointOfSegmentCollection(ipSegmentCol);
	ipUCLIDStartPoint->InitPointInXY(ipESRIEndPoint->GetX(), ipESRIEndPoint->GetY());

	long nActualSize = nStartSegmentIndex + nNumOfSegmentsToAdd;
	if (nActualSize > nSize)
	{
		nActualSize = nSize;
	}

	// how many segments in total initially?
	long nNumOfSegmentsInitial = ipSegmentCol->SegmentCount;
	if (m_bIsPolygon && nNumOfSegmentsInitial > 1)
	{
		/*esriGeometry::*/IRingPtr ipRing(ipSegmentCol);
		// if the ring is closed, remove last segment
		if (ipRing != NULL && ipRing->IsClosed == VARIANT_TRUE)
		{
			// remove the last closing segment
			ipSegmentCol->RemoveSegments(nNumOfSegmentsInitial-1, 1, VARIANT_FALSE);
		}
	}

	for (long n=nStartSegmentIndex; n<nActualSize; n++)
	{
		UCLID_FEATUREMGMTLib::IESSegmentPtr ipUCLIDSegment(ipOriginUCLIDSegments->At(n));
		bool bRequireTangentIn = 
			ipUCLIDSegment->requireTangentInDirection() == VARIANT_TRUE;
		
		if (bRequireTangentIn)
		{
			// if this is first segment in the vec
			if (n == 0)
			{
				throw UCLIDException("ELI12101", "First segment of each part shall not require any tangent-in info.");
			}

			// get previous segment's tangent out
			UCLID_FEATUREMGMTLib::IESSegmentPtr ipUCLIDPrevSegment(ipOriginUCLIDSegments->At(n-1));
			_bstrTangentInDirection = ipUCLIDPrevSegment->getTangentOutDirection();
			ipUCLIDSegment->setTangentInDirection(_bstrTangentInDirection);
		}

		// do the Ground-To-Grid conversion if it's on
		UCLID_FEATUREMGMTLib::IESSegmentPtr ipConvertedUCLIDSegment 
											= convertSegment(ipUCLIDSegment);

		// check for segment type
		/*esriGeometry::*/IESRSegmentPtr ipESRSegment(NULL);
		if (ipConvertedUCLIDSegment->getSegmentType() == UCLID_FEATUREMGMTLib::kArc)
		{
			 ipESRSegment = convertArcSegmentToESRISegment(ipUCLIDStartPoint, ipConvertedUCLIDSegment);
		}
		else if (ipConvertedUCLIDSegment->getSegmentType() == UCLID_FEATUREMGMTLib::kLine)
		{
			ipESRSegment = convertLineSegmentToESRISegment(ipUCLIDStartPoint, ipConvertedUCLIDSegment);
		}
		ASSERT_RESOURCE_ALLOCATION("ELI12093", ipESRSegment != NULL);

		// add the esri segment to the segment collection
		ipSegmentCol->AddSegment(ipESRSegment);

		// update the start point to be the end point of last segment
		ipESRIEndPoint = ipESRSegment->GetToPoint();
		ipUCLIDStartPoint->InitPointInXY(ipESRIEndPoint->GetX(), ipESRIEndPoint->GetY());
	}

	if (nStartSegmentIndex == 0 && nActualSize > 0)
	{
		/*esriGeometry::*/IPointCollectionPtr ipPointCol(ipSegmentCol);
		// Remove the very first point that we added as the starting
		// point for the current sketch part
		ipPointCol->RemovePoints(0, 1);	
	}
}
//-------------------------------------------------------------------------------------------------
bool CArcGISDisplayAdapter::areSameCommandItems(ICommandItemPtr ipFirstCommand,
												ICommandItemPtr ipSecondCommand)
{
	if (ipFirstCommand != NULL && ipSecondCommand != NULL)
	{
		string strFirstCommandName = _bstr_t(ipFirstCommand->Name);
		string strSecondCommandName = _bstr_t(ipSecondCommand->Name);
		if (_stricmp(strFirstCommandName.c_str(), strSecondCommandName.c_str()) == 0)
		{
			return true;
		}
	}

	return false;
}
//-------------------------------------------------------------------------------------------------
/*esriGeometry::*/IESRSegmentPtr CArcGISDisplayAdapter::convertArcSegmentToESRISegment(
											ICartographicPointPtr ipUCLIDStartPoint,
											UCLID_FEATUREMGMTLib::IESSegmentPtr ipUCLIDSegment)
{
	IArcSegmentPtr ipUCLIDArc(ipUCLIDSegment);
	ASSERT_RESOURCE_ALLOCATION("ELI12097", ipUCLIDArc != NULL);
	
	// get mid and end point from the arc
	ICartographicPointPtr ipMidPoint(NULL), ipEndPoint(NULL);
	ipUCLIDArc->getCoordsFromParams(ipUCLIDStartPoint, &ipMidPoint, &ipEndPoint);
	ASSERT_RESOURCE_ALLOCATION("ELI11658", ipMidPoint != NULL);
	ASSERT_RESOURCE_ALLOCATION("ELI11659", ipEndPoint != NULL);
	
	// create three esri points from uclid points
	/*esriGeometry::*/IPointPtr ipESRIStartPoint(/*esriGeometry::*/CLSID_Point), ipESRIMidPoint(/*esriGeometry::*/CLSID_Point), ipESRIEndPoint(/*esriGeometry::*/CLSID_Point);
	ASSERT_RESOURCE_ALLOCATION("ELI12103", ipESRIStartPoint != NULL);
	ASSERT_RESOURCE_ALLOCATION("ELI11661", ipESRIMidPoint != NULL);
	ASSERT_RESOURCE_ALLOCATION("ELI11662", ipESRIEndPoint != NULL);
	
	double dX, dY;
	ipUCLIDStartPoint->GetPointInXY(&dX, &dY);
	ipESRIStartPoint->PutCoords(dX, dY);
	ipMidPoint->GetPointInXY(&dX, &dY);
	ipESRIMidPoint->PutCoords(dX, dY);
	ipEndPoint->GetPointInXY(&dX, &dY);
	ipESRIEndPoint->PutCoords(dX, dY);
	
	// construct an esri circular arc
	/*esriGeometry::*/ICircularArcPtr ipCircularArc(/*esriGeometry::*/CLSID_CircularArc);
	ASSERT_RESOURCE_ALLOCATION("ELI11663", ipCircularArc != NULL);
	/*esriGeometry::*/IConstructCircularArcPtr ipConstructArc(ipCircularArc);
	ASSERT_RESOURCE_ALLOCATION("ELI11664", ipConstructArc != NULL);
	
	ipConstructArc->ConstructThreePoints(ipESRIStartPoint, ipESRIMidPoint, 
		ipESRIEndPoint, VARIANT_FALSE);

	/*esriGeometry::*/IESRSegmentPtr ipESRSegment = ipConstructArc;
	ASSERT_RESOURCE_ALLOCATION("ELI12094", ipESRSegment != NULL);

	return ipESRSegment;
}
//-------------------------------------------------------------------------------------------------
/*esriGeometry::*/IESRSegmentPtr CArcGISDisplayAdapter::convertLineSegmentToESRISegment(
											ICartographicPointPtr ipUCLIDStartPoint,
											UCLID_FEATUREMGMTLib::IESSegmentPtr ipUCLIDSegment)
{
	ILineSegmentPtr ipUCLIDLine(ipUCLIDSegment);
	ASSERT_RESOURCE_ALLOCATION("ELI12096", ipUCLIDLine != NULL);
	
	// get the end point of the line
	ICartographicPointPtr ipEndPoint(NULL);
	ipUCLIDLine->getCoordsFromParams(ipUCLIDStartPoint, &ipEndPoint);
	ASSERT_RESOURCE_ALLOCATION("ELI11666", ipEndPoint != NULL);
	
	/*esriGeometry::*/IPointPtr ipESRIFromPoint(/*esriGeometry::*/CLSID_Point), ipESRIToPoint(/*esriGeometry::*/CLSID_Point);
	ASSERT_RESOURCE_ALLOCATION("ELI12104", ipESRIFromPoint != NULL);
	ASSERT_RESOURCE_ALLOCATION("ELI11667", ipESRIToPoint != NULL);
	
	double dX, dY;
	ipUCLIDStartPoint->GetPointInXY(&dX, &dY);
	ipESRIFromPoint->PutCoords(dX, dY);
	ipEndPoint->GetPointInXY(&dX, &dY);
	ipESRIToPoint->PutCoords(dX, dY);
	
	/*esriGeometry::*/ILinePtr ipESRILine(/*esriGeometry::*/CLSID_Line);
	ASSERT_RESOURCE_ALLOCATION("ELI11668", ipESRILine != NULL);
	ipESRILine->PutCoords(ipESRIFromPoint, ipESRIToPoint);

	/*esriGeometry::*/IESRSegmentPtr ipESRSegment = ipESRILine;
	ASSERT_RESOURCE_ALLOCATION("ELI12095", ipESRSegment != NULL);

	return ipESRSegment;
}
//-------------------------------------------------------------------------------------------------
UCLID_FEATUREMGMTLib::IESSegmentPtr CArcGISDisplayAdapter::convertSegment(
										UCLID_FEATUREMGMTLib::IESSegmentPtr ipSegment)
{
	if (!isGroundToGridOn())
	{
		return ipSegment;
	}

	// is it an arc or a line?
	ESegmentType eSegmentType = ipSegment->getSegmentType();

	// create a new segment object for storing converted info
	UCLID_FEATUREMGMTLib::IESSegmentPtr ipNewSegment(NULL);
	if (eSegmentType == UCLID_FEATUREMGMTLib::kArc)
	{
		// create a new arc segment to store converted info
		IArcSegmentPtr ipNewArc(__uuidof(ArcSegment));
		ipNewSegment = ipNewArc;
	}
	else if (eSegmentType == UCLID_FEATUREMGMTLib::kLine)
	{
		ILineSegmentPtr ipNewLine(__uuidof(LineSegment));
		ipNewSegment = ipNewLine;
	}
	ASSERT_RESOURCE_ALLOCATION("ELI11657", ipNewSegment != NULL);

	// get parameters from original segment
	IIUnknownVectorPtr ipParams = ipSegment->getParameters();
	long nSize = ipParams->Size();

	// create a new vector to store parameters
	IIUnknownVectorPtr ipNewParams(CLSID_IUnknownVector);
	ASSERT_RESOURCE_ALLOCATION("ELI11656", ipNewParams != NULL);

	// go through all parameters of the curve segment
	for (long n=0; n<nSize; n++)
	{
		// type value pair
		IParameterTypeValuePairPtr ipTypeValuePair = ipParams->At(n);
		string strConvertedValue = _bstr_t(ipTypeValuePair->strValue);
		strConvertedValue = groundToGridConversion(ipTypeValuePair->eParamType, strConvertedValue);
		// update the parameter
		IParameterTypeValuePairPtr ipNewTypeValuePair(__uuidof(ParameterTypeValuePair));
		ASSERT_RESOURCE_ALLOCATION("ELI11655", ipNewTypeValuePair != NULL);

		ipNewTypeValuePair->eParamType = ipTypeValuePair->eParamType;
		ipNewTypeValuePair->strValue = _bstr_t(strConvertedValue.c_str());

		ipNewParams->PushBack(ipNewTypeValuePair);
	}

	ipNewSegment->setParameters(ipNewParams);

	return ipNewSegment;
}
//-------------------------------------------------------------------------------------------------
void CArcGISDisplayAdapter::executeCommandWithGUID(const string& strCommandGUID)
{
	// ******************
	// Refer to ICommandBars::Find sample
	// ******************
	
	// find command from ArcMap toolbar
	/*esriFramework::*/ICommandItemPtr ipCommandItem = getCommandItemByGUID(strCommandGUID);
	if (ipCommandItem != NULL)
	{
		// only execute the command if it's enabled
		IESRCommandPtr ipCommand = ipCommandItem->Command;
		if (ipCommand != NULL && ipCommand->Enabled == VARIANT_TRUE)
		{
			// if it is a tool and is already selected, just return
			if (areSameCommandItems(m_ipArcMapApp->CurrentTool, ipCommandItem))
			{
				return;
			}

			ipCommandItem->Execute();
		}
	}
}
//-------------------------------------------------------------------------------------------------
bool CArcGISDisplayAdapter::executeCommandWithName(const string& strCommandName)
{
	ICommandItemPtr ipCommandItem = getCommandItemByName(strCommandName);

	// Check to see if command item was found
	if (ipCommandItem == NULL)
	{
		// Not found, return false in case an alternate command is available
		return false;
	}
	else
	{
		// only execute the command if it's enabled
		IESRCommandPtr ipCommand = ipCommandItem->Command;
		if (ipCommand != NULL && ipCommand->Enabled == VARIANT_TRUE)
		{
			// if it is a tool and is already selected, just return
			if (areSameCommandItems(m_ipArcMapApp->CurrentTool, ipCommandItem))
			{
				return true;
			}

			ipCommandItem->Execute();
		}
	}

	// Return true even if not enabled at this time because the command was found
	return true;
}
//-------------------------------------------------------------------------------------------------
/*esriFramework::*/ICommandItemPtr CArcGISDisplayAdapter::getCommandItemByGUID(const string& strCommandGUID)
{
	// create a UID object
	IUIDPtr ipUID(CLSID_UID);
	ASSERT_RESOURCE_ALLOCATION("ELI11640", ipUID != NULL);
	// set UID value with the GUID string
	ipUID->Value = _variant_t(_bstr_t(strCommandGUID.c_str()));

	// create a variant with the UID object's IUnknown interface pointer
	_variant_t _vUID((IUnknown*)ipUID);

	/*esriFramework::*/ICommandBarsPtr ipCommandBars = m_ipArcMapApp->Document->CommandBars;
	ASSERT_RESOURCE_ALLOCATION("ELI11627", ipCommandBars != NULL);
	
	// find command from ArcMap toolbar
	ICommandItemPtr ipCommandItem = ipCommandBars->Find(_vUID, VARIANT_FALSE, VARIANT_FALSE);

	return ipCommandItem;
}
//-------------------------------------------------------------------------------------------------
/*esriFramework::*/ICommandItemPtr CArcGISDisplayAdapter::getCommandItemByName(const string& strCommandName)
{
	ICommandBarsPtr ipCommandBars = m_ipArcMapApp->Document->CommandBars;
	ASSERT_RESOURCE_ALLOCATION("ELI11629", ipCommandBars != NULL);
	
	// find command from ArcMap toolbar
	ICommandItemPtr ipCommandItem 
		= ipCommandBars->Find(_variant_t(_bstr_t(strCommandName.c_str())), VARIANT_FALSE, VARIANT_FALSE);

	return ipCommandItem;
}
//-------------------------------------------------------------------------------------------------
IConfigurationSettingsPersistenceMgr* CArcGISDisplayAdapter::getConfigManager()
{
	if (m_apCfgMgr.get() == NULL)
	{
		m_apCfgMgr = auto_ptr<IConfigurationSettingsPersistenceMgr>(
			new RegistryPersistenceMgr(HKEY_CURRENT_USER, ROOT_FOLDER));

		ASSERT_RESOURCE_ALLOCATION("ELI11718", m_apCfgMgr.get() != NULL);
	}

	return m_apCfgMgr.get();
}
//-------------------------------------------------------------------------------------------------
/*esriGeometry::*/IPointPtr CArcGISDisplayAdapter::getEndPointOfSegmentCollection(/*esriGeometry::*/IESRSegmentCollectionPtr ipSegCol)
{	
	// by default, it's the end point of the part
	/*esriGeometry::*/ICurvePtr ipCurve(ipSegCol);
	/*esriGeometry::*/IPointPtr ipLastPoint = ipCurve->GetToPoint();

	// how many segments are there?
	long nNumOfSegments = ipSegCol->GetSegmentCount();
	if (nNumOfSegments > 0)
	{
		// if target layer is polygon
		if (m_bIsPolygon)
		{
			// polygon layer is little bit tricky
			/*esriGeometry::*/IESRSegmentPtr ipSeg(NULL);
			if (nNumOfSegments > 1)
			{
				ipSeg = ipSegCol->GetSegment(nNumOfSegments - 2);
			}
			else if (nNumOfSegments == 1)
			{
				ipSeg = ipSegCol->GetSegment(nNumOfSegments - 1);
			}
			
			if (ipSeg)
			{
				ipLastPoint = ipSeg->GetToPoint();
			}
		}
	}

	return ipLastPoint;
}
//-------------------------------------------------------------------------------------------------
void CArcGISDisplayAdapter::getFeature(/*esriGeometry::*/IGeometryPtr ipGeometry, 
									   UCLID_FEATUREMGMTLib::IUCLDFeaturePtr ipUCLIDFeature)
{
	ASSERT_ARGUMENT("ELI11583", ipGeometry != NULL);
	ASSERT_ARGUMENT("ELI11584", ipUCLIDFeature != NULL);

	// each feature is consist of a collection of parts
	/*esriGeometry::*/IGeometryCollectionPtr ipGeoCollection(ipGeometry);
	ASSERT_RESOURCE_ALLOCATION("ELI11585", ipGeoCollection != NULL);

	long nNumOfParts = ipGeoCollection->GeometryCount;
	for (long n=0; n<nNumOfParts; n++)
	{
		/*esriGeometry::*/ICurvePtr ipCurve(ipGeoCollection->GetGeometry(n));
		ASSERT_RESOURCE_ALLOCATION("ELI11586", ipCurve != NULL);
		IPartPtr ipUCLIDPart(__uuidof(Part));
		ASSERT_RESOURCE_ALLOCATION("ELI11587", ipUCLIDPart != NULL);

		getPart(ipCurve, ipUCLIDPart);

		if (ipUCLIDPart->getNumSegments() > 0)
		{
			// add to the feature
			ipUCLIDFeature->addPart(ipUCLIDPart);
		}
	}
}
//-------------------------------------------------------------------------------------------------
/*esriGeometry::*/IPointPtr CArcGISDisplayAdapter::getLastPointFromSketch()
{
	/*esriEditor::*/IEditSketchPtr ipEditSketch(m_ipArcMapEditor);
	ASSERT_RESOURCE_ALLOCATION("ELI11485", ipEditSketch != NULL);

	/*esriGeometry::*/IPointPtr ipLastPoint(NULL);

	// if current sketch is not empty
	/*esriGeometry::*/IGeometryPtr ipCurrentGeometry(ipEditSketch->Geometry);
	if (ipCurrentGeometry != NULL 
		&& ipCurrentGeometry->GetIsEmpty() == VARIANT_FALSE)
	{
		// since current sketch is not empty, then initialize
		// related parameters if not set yet
		if (!m_bIsDrawingSketch)
		{
			// if hasn't started drawing yet
			initDrawing();

			// If we are using layer doesn't support by IcoMap
			// m_ipCurrentPart is NULL
			if (!m_ipCurrentPart)
			{
				m_ipCurrentSketch = NULL;
				m_bIsDrawingSketch = false;

				return ipLastPoint;
			}
		}

		// get current sketch
		m_ipCurrentSketch = ipCurrentGeometry;
		ASSERT_RESOURCE_ALLOCATION("ELI11486", m_ipCurrentSketch != NULL);

		// how many parts are there?
		long nNumOfParts = m_ipCurrentSketch->GeometryCount;
		// assument current part is always the last geometry in the current sketch
		m_ipCurrentPart = m_ipCurrentSketch->GetGeometry(nNumOfParts - 1);
		if (m_ipCurrentPart)
		{
			// each part is composed by a collection of segments
			/*esriGeometry::*/IESRSegmentCollectionPtr ipSegmentCol(m_ipCurrentPart);
			ASSERT_RESOURCE_ALLOCATION("ELI11488", ipSegmentCol != NULL);

			ipLastPoint = getEndPointOfSegmentCollection(ipSegmentCol);
		}
	}
	else
	{
		// if current sketch is empty, reset variables
		m_ipCurrentPart = NULL;
		m_ipCurrentSketch = NULL;
		m_bIsDrawingSketch = false;
	}

	return ipLastPoint;
}
//-------------------------------------------------------------------------------------------------
bool CArcGISDisplayAdapter::getLastTangentOutInRadians(double& dTangentOutInRadians)
{
	bool bSuccess = false;
	
	// only able to get the value if current sketch is still under construction
	if (m_ipCurrentSketch != NULL)
	{
		/*esriEditor::*/IEditSketchPtr ipEditSketch(m_ipArcMapEditor);
		ASSERT_RESOURCE_ALLOCATION("ELI11682", ipEditSketch != NULL);
		
		m_ipCurrentSketch = ipEditSketch->Geometry;
		if (m_ipCurrentSketch == NULL)
		{
			return false;
		}
		
		long nNumOfParts = m_ipCurrentSketch->GeometryCount;
		// if there's at least one part
		if (nNumOfParts > 0)
		{
			// set current part to the last part in the current sketch
			m_ipCurrentPart = m_ipCurrentSketch->GetGeometry(nNumOfParts-1);
			ASSERT_RESOURCE_ALLOCATION("ELI11684", m_ipCurrentPart != NULL);
		}
		
		// use IESRSegmentCollection to access each segment
		/*esriGeometry::*/IESRSegmentCollectionPtr ipSegCollection(m_ipCurrentPart);
		ASSERT_RESOURCE_ALLOCATION("ELI11685", ipSegCollection != NULL);
		
		long nSegmentCount = ipSegCollection->SegmentCount;
		if (nSegmentCount > 0)
		{
			// which segment is the last segment? For line, it's the last segment
			long nNum = 1;
			if (m_bIsPolygon)
			{
				// if number of segment is less than 2, return false
				if (nSegmentCount < 2)
				{
					return false;
				}
				
				// for polygon, it's the second to last segment
				nNum = 2;
			}
			
			/*esriGeometry::*/IESRSegmentPtr ipLastSegment = ipSegCollection->GetSegment(nSegmentCount - nNum);
			ASSERT_RESOURCE_ALLOCATION("ELI11686", ipLastSegment != NULL);
			
			// just in case ESRI internal error, make sure this is the acutal
			// segment with a length more than 0
			double dLastSegmentLen = ipLastSegment->Length;
			if (dLastSegmentLen <= 0)
			{
				return false;
			}
			
			/*esriGeometry::*/ILinePtr ipTangentOutLine(/*esriGeometry::*/CLSID_Line);
			ASSERT_RESOURCE_ALLOCATION("ELI11687", ipTangentOutLine != NULL);
			// what's the tangent out line from the last segment
			ipLastSegment->QueryTangent(/*esriGeometry::*/esriExtendTangentAtTo, 
				1, VARIANT_TRUE, 20, ipTangentOutLine);
			
			dTangentOutInRadians = ipTangentOutLine->Angle;

			bSuccess = true;
		}
	}

	return bSuccess;
}
//-------------------------------------------------------------------------------------------------
void CArcGISDisplayAdapter::getPart(/*esriGeometry::*/ICurvePtr ipCurve, IPartPtr ipUCLIDPart)
{
	ASSERT_ARGUMENT("ELI11577", ipCurve != NULL);
	ASSERT_ARGUMENT("ELI11578", ipUCLIDPart != NULL);

	// first set starting point of the part
	ICartographicPointPtr ipUCLIDStartPoint(CLSID_CartographicPoint);
	ASSERT_RESOURCE_ALLOCATION("ELI11579", ipUCLIDStartPoint != NULL);
	ipUCLIDStartPoint->InitPointInXY(ipCurve->GetFromPoint()->GetX(),
										ipCurve->GetFromPoint()->GetY());
	ipUCLIDPart->setStartingPoint(ipUCLIDStartPoint);

    // Traverse the curve (could be a path on polyline layer or a ring on polygon layer),
    // get each segment info and then add them to the uclid part
	/*esriGeometry::*/IESRSegmentCollectionPtr ipSegmentCol(ipCurve);
	ASSERT_RESOURCE_ALLOCATION("ELI11580", ipSegmentCol != NULL);
	long nNumOfSegments = ipSegmentCol->SegmentCount;
	for (long n=0; n<nNumOfSegments; n++)
	{
		// go through each segment in the currrent part
		/*esriGeometry::*/IESRSegmentPtr ipESRSegment(ipSegmentCol->GetSegment(n));
		ASSERT_RESOURCE_ALLOCATION("ELI11582", ipESRSegment != NULL);

		UCLID_FEATUREMGMTLib::IESSegmentPtr ipUCLIDSegment(NULL);
		// Is it a line or a curve?
		/*esriGeometry::*/esriGeometryType eGeoType = ipESRSegment->GeometryType;
		if (eGeoType == /*esriGeometry::*/esriGeometryLine)
		{
			ipUCLIDSegment.CreateInstance(__uuidof(LineSegment));
		}
		else if (eGeoType == /*esriGeometry::*/esriGeometryCircularArc)
		{
			ipUCLIDSegment.CreateInstance(__uuidof(ArcSegment));
		}

		if (ipUCLIDSegment)
		{
			double dStartX = ipESRSegment->GetFromPoint()->GetX();
			double dStartY = ipESRSegment->GetFromPoint()->GetY();
			double dEndX = ipESRSegment->GetToPoint()->GetX();
			double dEndY = ipESRSegment->GetToPoint()->GetY();
			// if this is a line segment and start point equals end point, skip it
			if (!(eGeoType == /*esriGeometry::*/esriGeometryLine 
				&& fabs(dStartX - dEndX) <= MathVars::ZERO 
				&& fabs(dStartY - dEndY) <= MathVars::ZERO))
			{
				getSegment(ipESRSegment, ipUCLIDSegment);
				ipUCLIDPart->addSegment(ipUCLIDSegment);
			}
		}

		ipUCLIDSegment = NULL;
	}
}
//-------------------------------------------------------------------------------------------------
void CArcGISDisplayAdapter::getSegment(/*esriGeometry::*/IESRSegmentPtr ipESRSegment, 
									   UCLID_FEATUREMGMTLib::IESSegmentPtr ipUCLIDSegment)
{
	ASSERT_ARGUMENT("ELI11559", ipESRSegment != NULL);
	ASSERT_ARGUMENT("ELI11560", ipUCLIDSegment != NULL);
	
	// get start and end point from esri segment
	ICartographicPointPtr ipUCLIDStartPoint(CLSID_CartographicPoint);
	ASSERT_RESOURCE_ALLOCATION("ELI11561", ipUCLIDStartPoint != NULL);
	ICartographicPointPtr ipUCLIDEndPoint(CLSID_CartographicPoint);
	ASSERT_RESOURCE_ALLOCATION("ELI11562", ipUCLIDEndPoint != NULL);
	
	double dStartX = ipESRSegment->GetFromPoint()->GetX();
	double dStartY = ipESRSegment->GetFromPoint()->GetY();
	double dEndX = ipESRSegment->GetToPoint()->GetX();
	double dEndY = ipESRSegment->GetToPoint()->GetY();
	ipUCLIDStartPoint->InitPointInXY(dStartX, dStartY);
	ipUCLIDEndPoint->InitPointInXY(dEndX, dEndY);
	
	// is current segment a line or a curve?
	/*esriGeometry::*/esriGeometryType eGeoType = ipESRSegment->GeometryType;
	if (eGeoType == /*esriGeometry::*/esriGeometryLine)
	{
		// if it's a line
		ILineSegmentPtr ipLine(ipUCLIDSegment);
		ASSERT_RESOURCE_ALLOCATION("ELI11563", ipLine != NULL);
		// store start and end point
		ipLine->setParamsFromCoords(ipUCLIDStartPoint, ipUCLIDEndPoint);
	}
	else if (eGeoType == /*esriGeometry::*/esriGeometryCircularArc)
	{
		// get the center point of the curve
		/*esriGeometry::*/ICircularArcPtr ipCircularArc(ipESRSegment);
		ASSERT_RESOURCE_ALLOCATION("ELI11566", ipCircularArc != NULL);
		
		ICartographicPointPtr ipUCLIDCenterPoint(CLSID_CartographicPoint);
		ASSERT_RESOURCE_ALLOCATION("ELI11565", ipUCLIDCenterPoint != NULL);
		
		double dCenterX = ipCircularArc->GetCenterPoint()->GetX();
		double dCenterY = ipCircularArc->GetCenterPoint()->GetY();
		ipUCLIDCenterPoint->InitPointInXY(dCenterX, dCenterY);
		
		// store start, center, end point and concavity in a vector
		IIUnknownVectorPtr ipParamsVec(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI11567", ipParamsVec != NULL);
		
		// starting point
		IParameterTypeValuePairPtr ipParam1(__uuidof(ParameterTypeValuePair));
		ASSERT_RESOURCE_ALLOCATION("ELI11572", ipParam1 != NULL);
		ipParam1->eParamType = kArcStartingPoint;
		string strValue = asString(dStartX) + "," + asString(dStartY);
		ipParam1->strValue = _bstr_t(strValue.c_str());
		ipParamsVec->PushBack(ipParam1);
		
		// center point
		IParameterTypeValuePairPtr ipParam2(__uuidof(ParameterTypeValuePair));
		ASSERT_RESOURCE_ALLOCATION("ELI11573", ipParam2 != NULL);
		ipParam2->eParamType = kArcCenter;
		strValue = asString(dCenterX) + "," + asString(dCenterY);
		ipParam2->strValue = _bstr_t(strValue.c_str());
		ipParamsVec->PushBack(ipParam2);
		
		// end point
		IParameterTypeValuePairPtr ipParam3(__uuidof(ParameterTypeValuePair));
		ASSERT_RESOURCE_ALLOCATION("ELI11574", ipParam3 != NULL);
		ipParam3->eParamType = kArcEndingPoint;
		strValue = asString(dEndX) + "," + asString(dEndY);
		ipParam3->strValue = _bstr_t(strValue.c_str());
		ipParamsVec->PushBack(ipParam3);
		
		// concavity
		IParameterTypeValuePairPtr ipParam4(__uuidof(ParameterTypeValuePair));
		ASSERT_RESOURCE_ALLOCATION("ELI11575", ipParam4 != NULL);
		ipParam4->eParamType = kArcConcaveLeft;
		strValue = ipCircularArc->GetIsCounterClockwise() == VARIANT_TRUE ? "1" : "0";
		ipParam4->strValue = _bstr_t(strValue.c_str());
		ipParamsVec->PushBack(ipParam4);

		// set parameters
		ipUCLIDSegment->setParameters(ipParamsVec);
	}
	else
	{
		// internal error
		throw UCLIDException("ELI11564", "We only support line and curve creation.");
	}
}
//-------------------------------------------------------------------------------------------------
string CArcGISDisplayAdapter::getLastTangentOutBearingAsStringValue()
{
	// get last tangent-out
	double dTangentOut = 0.0;
	bool bSuccess = getLastTangentOutInRadians(dTangentOut);
	
	if (!bSuccess)
	{
		throw UCLIDException("ELI12040", 
			"Failed to get tangent out direction from last segment.");
	}
	
	// store the original mode, then set it back once 
	// this method is out of scope
	ReverseModeValueRestorer rmvr;
	// always work in normal mode here
	AbstractMeasurement::workInReverseMode(false);
	
	// convert the radians into quadrant bearing
	Bearing tempBearing;
	tempBearing.evaluateRadians(dTangentOut);
	string strTangentOutBearing = tempBearing.asString();

	return strTangentOutBearing;
}
//-------------------------------------------------------------------------------------------------
string CArcGISDisplayAdapter::groundToGridConversion(ECurveParameterType eCurveType, 
													 const string& strValue)
{
	string strConvertedValue(strValue);

	switch (eCurveType)
	{
	// bearings
	case kArcTangentInBearing:
	case kArcTangentOutBearing:
	case kArcChordBearing:
	case kArcRadialInBearing:
	case kArcRadialOutBearing:
	case kLineBearing:
		{
			strConvertedValue = groundToGridBearingConversion(strValue);
		}
		break;
	// distances
	case kArcRadius:
	case kArcLength:
	case kArcChordLength:
	case kArcExternalDistance:
	case kArcMiddleOrdinate:
	case kArcTangentDistance:
	case kLineDistance:
		{
			strConvertedValue = groundToGridDistanceConversion(strValue);
		}
		break;
	}

	return strConvertedValue;
}
//-------------------------------------------------------------------------------------------------
string CArcGISDisplayAdapter::groundToGridBearingConversion(const string& strInputBearing)
{
	// store the original mode, then set it back once 
	// this method is out of scope
	ReverseModeValueRestorer rmvr;
	// always work in normal mode here
	AbstractMeasurement::workInReverseMode(false);

	string strBearing(strInputBearing);
	// create a bearing object for calculation purpose
	Bearing bearingObj(strBearing.c_str());

	// get the angle in radians
	double dOriginalAngle = bearingObj.getRadians();
	// count the angle offset
	double dActualAngle = dOriginalAngle + m_dAngleOffset;
	// now evaluate the actual angle using the bearing object
	bearingObj.resetVariables();
	bearingObj.evaluateRadians(dActualAngle);
	// get the string format of the bearing out
	strBearing = bearingObj.asString();

	return strBearing;
}
//-------------------------------------------------------------------------------------------------
string CArcGISDisplayAdapter::groundToGridDistanceConversion(const string& strInputDistance)
{
	// *******************************
	// This function assumes that strInputDistance string is a valid distance
	// *******************************

	// parse the string into number part and unit part
	int nLen = strInputDistance.size();
	string strDistance("");
	int n = 0;
	while (n < nLen)
	{
		char cChar(strInputDistance[n]);
		// get all the numbers and decimal point char
		if (::isDigitChar(cChar) || cChar == '.')
		{
			strDistance += cChar;
		}
		else
		{
			break;
		}

		n++;
	}

	// get the distance unit
	string strUnit = strInputDistance.substr(n, nLen-n);

	// convert the distance string into a double
	double dDistance = ::asDouble(strDistance);
	// correct the distance value with the distance factor
	dDistance = dDistance * m_dDistanceFactor;

	// conver the distance back to string format
	strDistance = ::asString(dDistance);
	// append the unit
	strDistance += strUnit;

	return strDistance;
}
//-------------------------------------------------------------------------------------------------
void CArcGISDisplayAdapter::initDrawing()
{
	m_ipCurrentPart = NULL;

	/*esriEditor::*/IEditSketchPtr ipEditSketch(m_ipArcMapEditor);
	ASSERT_RESOURCE_ALLOCATION("ELI11471", ipEditSketch != NULL);

	m_bIsPolygon = false;
	m_bIsDrawingSketch = true;
	// create new sketch part before adding any segment
	// Currently we only support drawing on line or polygon layer.
	// Both Path and Ring are of type Curve
	if (ipEditSketch->GeometryType == /*esriGeometry::*/esriGeometryPolyline)
	{
		m_ipCurrentPart.CreateInstance(/*esriGeometry::*/CLSID_Path);
		// Current part shall not be null
		ASSERT_RESOURCE_ALLOCATION("ELI11472", m_ipCurrentPart != NULL);
	}
	else if (ipEditSketch->GeometryType == /*esriGeometry::*/esriGeometryPolygon)
	{
		m_ipCurrentPart.CreateInstance(/*esriGeometry::*/CLSID_Ring);
		m_bIsPolygon = true;
		// Current part shall not be null
		ASSERT_RESOURCE_ALLOCATION("ELI14216", m_ipCurrentPart != NULL);
	}

	// Current part shall not be null
//	ASSERT_RESOURCE_ALLOCATION("ELI11472", m_ipCurrentPart != NULL);
}
//-------------------------------------------------------------------------------------------------
bool CArcGISDisplayAdapter::isGroundToGridOn()
{
	bool bIsGroundToGridChecked = false;
	m_dDistanceFactor = 1.0;
	m_dAngleOffset = 0.0;

	// check the Ground-To-Grid option from ArcMap->Editor
	/*esriEditor::*/IEditProperties2Ptr ipEditProperties(m_ipArcMapEditor);
	if (ipEditProperties != NULL && ipEditProperties->UseGroundToGrid == VARIANT_TRUE)
	{
		bIsGroundToGridChecked = true;
		m_dDistanceFactor = ipEditProperties->DistanceCorrectionFactor;
		// the following value is specified in radians
		m_dAngleOffset = ipEditProperties->AngularCorrectionOffset;
	}

	return bIsGroundToGridChecked;
}
//-------------------------------------------------------------------------------------------------
void CArcGISDisplayAdapter::setFeature(/*esriGeometry::*/IGeometryPtr ipGeometry, 
									   UCLID_FEATUREMGMTLib::IUCLDFeaturePtr ipUCLIDFeature)
{
	ASSERT_ARGUMENT("ELI11615", ipGeometry != NULL);
	ASSERT_ARGUMENT("ELI11616", ipUCLIDFeature != NULL);

	// each feature is consist of a collection of parts
	/*esriGeometry::*/IGeometryCollectionPtr ipGeoCollection(ipGeometry);
	ASSERT_RESOURCE_ALLOCATION("ELI11617", ipGeoCollection != NULL);

	// create a new esri geometry for temporarily store the part info
	/*esriGeometry::*/IGeometryPtr ipESRITempGeometry(NULL);
	EFeatureType eFeatureType = ipUCLIDFeature->getFeatureType();
	if (eFeatureType == kPolyline)
	{
		// create a polyline feature
		ipESRITempGeometry.CreateInstance(/*esriGeometry::*/CLSID_Polyline);
	}
	else if (eFeatureType == kPolygon)
	{
		// create a polygon feature
		ipESRITempGeometry.CreateInstance(/*esriGeometry::*/CLSID_Polygon);
	}
	else
	{
		throw UCLIDException("ELI11619", "We only support line and polygon creation.");
	}

	ASSERT_RESOURCE_ALLOCATION("ELI11620", ipESRITempGeometry != NULL);
	/*esriGeometry::*/IGeometryCollectionPtr ipTempGeoCollection(ipESRITempGeometry);
	ASSERT_RESOURCE_ALLOCATION("ELI11621", ipTempGeoCollection != NULL);

	// traverse each part in this uclid feature
	UCLID_FEATUREMGMTLib::IEnumPartPtr ipUCLIDEnumPart = ipUCLIDFeature->getParts();
	ASSERT_RESOURCE_ALLOCATION("ELI11618", ipUCLIDEnumPart != NULL);
	// set to the beginning
	ipUCLIDEnumPart->reset();
	IPartPtr ipUCLIDPart = ipUCLIDEnumPart->next();
	while (ipUCLIDPart != NULL)
	{
		// create a new curve geometry
		/*esriGeometry::*/ICurvePtr ipESRIPart(NULL);
		if (eFeatureType == kPolyline)
		{
			ipESRIPart.CreateInstance(/*esriGeometry::*/CLSID_Path);
		}
		else if (eFeatureType == kPolygon)
		{
			ipESRIPart.CreateInstance(/*esriGeometry::*/CLSID_Ring);
		}

		ASSERT_RESOURCE_ALLOCATION("ELI11622", ipESRIPart != NULL);
		// get the part from the uclid part
		setPart(ipESRIPart, ipUCLIDPart);
		// add the part to the esri feature
		ipTempGeoCollection->AddGeometry(ipESRIPart);
		// close the polygon if it's not yet closed
		if (eFeatureType == kPolygon && ipESRIPart->IsClosed == VARIANT_FALSE)
		{
			/*esriGeometry::*/IRingPtr ipRing(ipESRIPart);
			ASSERT_RESOURCE_ALLOCATION("ELI11623", ipRing != NULL);
			ipRing->Close();
		}

		// go to next part
		ipUCLIDPart = ipUCLIDEnumPart->next();
	}

	// replace the old feature with the new one
	if (ipUCLIDFeature->getNumParts() > 0)
	{
		ipGeoCollection->RemoveGeometries(0, ipGeoCollection->GeometryCount);
		ipGeoCollection->SetGeometryCollection(ipTempGeoCollection);
	}
}
//-------------------------------------------------------------------------------------------------
void CArcGISDisplayAdapter::setPart(/*esriGeometry::*/ICurvePtr ipCurve, IPartPtr ipUCLIDPart)
{
	ASSERT_ARGUMENT("ELI11603", ipCurve != NULL);
	ASSERT_ARGUMENT("ELI11604", ipUCLIDPart != NULL);

	/*esriGeometry::*/IESRSegmentCollectionPtr ipSegCollection(ipCurve);
	ASSERT_RESOURCE_ALLOCATION("ELI11605", ipSegCollection != NULL);

	// get the start point of the current part
	ICartographicPointPtr ipUCLIDStartPoint = ipUCLIDPart->getStartingPoint();
	ASSERT_RESOURCE_ALLOCATION("ELI11606", ipUCLIDStartPoint != NULL);

	// go through all segments in the current part
	UCLID_FEATUREMGMTLib::IEnumSegmentPtr ipUCLIDEnumSegment = ipUCLIDPart->getSegments();
	ASSERT_RESOURCE_ALLOCATION("ELI11607", ipUCLIDEnumSegment != NULL);
	// set to the first segment
	ipUCLIDEnumSegment->reset();
	// get the first segment
	UCLID_FEATUREMGMTLib::IESSegmentPtr ipUCLIDSegment = ipUCLIDEnumSegment->next();
	while (ipUCLIDSegment != NULL)
	{
		// only create the segment if it's a line or an arc
		/*esriGeometry::*/IESRSegmentPtr ipESRSegment(NULL);
		if (ipUCLIDSegment->getSegmentType() == UCLID_FEATUREMGMTLib::kLine)
		{
			ipESRSegment.CreateInstance(/*esriGeometry::*/CLSID_Line);
		}
		else if (ipUCLIDSegment->getSegmentType() == UCLID_FEATUREMGMTLib::kArc)
		{
			ipESRSegment.CreateInstance(/*esriGeometry::*/CLSID_CircularArc);
		}
		else
		{
			// We only support line and arc
			throw UCLIDException("ELI11609", "We only support line and arc creation.");
		}

		// make sure the segment object is not null
		ASSERT_RESOURCE_ALLOCATION("ELI11608", ipESRSegment != NULL);

		// get info from uclid segment and store them in the esri segment
		setSegment(ipESRSegment, ipUCLIDSegment, ipUCLIDStartPoint);

		// add segment to current part
		ipSegCollection->AddSegment(ipESRSegment);

		// reset the start point equal to the end point of the last segment
		ipUCLIDStartPoint->InitPointInXY(ipESRSegment->GetToPoint()->GetX(),
									ipESRSegment->GetToPoint()->GetY());

		ipUCLIDSegment = ipUCLIDEnumSegment->next();
	}
}
//-------------------------------------------------------------------------------------------------
void CArcGISDisplayAdapter::setSegment(/*esriGeometry::*/IESRSegmentPtr ipESRSegment, 
									   UCLID_FEATUREMGMTLib::IESSegmentPtr ipUCLIDSegment,
									   ICartographicPointPtr ipUCLIDStartPoint)
{
	ASSERT_ARGUMENT("ELI11588", ipESRSegment != NULL);
	ASSERT_ARGUMENT("ELI11589", ipUCLIDSegment != NULL);
	ASSERT_ARGUMENT("ELI11590", ipUCLIDStartPoint != NULL);

	/*esriGeometry::*/IPointPtr ipESRIStartPoint(/*esriGeometry::*/CLSID_Point);
	ASSERT_RESOURCE_ALLOCATION("ELI11591", ipESRIStartPoint != NULL);

	// store uclid start point in to esri start point
	double dX, dY;
	ipUCLIDStartPoint->GetPointInXY(&dX, &dY);
	ipESRIStartPoint->PutCoords(dX, dY);

	// is it a line or a curve?
	/*esriGeometry::*/esriGeometryType eGeoType = ipESRSegment->GeometryType;
	if (eGeoType == /*esriGeometry::*/esriGeometryLine)
	{
		// get end point of the line
		ILineSegmentPtr ipUCLIDLine(ipUCLIDSegment);
		ASSERT_RESOURCE_ALLOCATION("ELI11593", ipUCLIDLine != NULL);

		ICartographicPointPtr ipUCLIDEndPoint(NULL);
		ipUCLIDLine->getCoordsFromParams(ipUCLIDStartPoint, &ipUCLIDEndPoint);
		ASSERT_RESOURCE_ALLOCATION("ELI11595", ipUCLIDEndPoint != NULL);

		// get x, y
		ipUCLIDEndPoint->GetPointInXY(&dX, &dY);

		// store the end point
		/*esriGeometry::*/IPointPtr ipESRIEndPoint(/*esriGeometry::*/CLSID_Point);
		ASSERT_RESOURCE_ALLOCATION("ELI11594", ipESRIEndPoint != NULL);
		ipESRIEndPoint->PutCoords(dX, dY);

		// using start, end point to construct the line
		/*esriGeometry::*/ILinePtr ipESRILine(ipESRSegment);
		ASSERT_RESOURCE_ALLOCATION("ELI11592", ipESRILine != NULL);
		ipESRILine->PutCoords(ipESRIStartPoint, ipESRIEndPoint);
	}
	else if (eGeoType == /*esriGeometry::*/esriGeometryCircularArc)
	{
		IArcSegmentPtr ipUCLIDArc(ipUCLIDSegment);
		ASSERT_RESOURCE_ALLOCATION("ELI11596", ipUCLIDArc != NULL);

		ICartographicPointPtr ipUCLIDMidPoint(NULL), ipUCLIDEndPoint(NULL);
		ipUCLIDArc->getCoordsFromParams(ipUCLIDStartPoint, &ipUCLIDMidPoint, &ipUCLIDEndPoint);
		ASSERT_RESOURCE_ALLOCATION("ELI11597", ipUCLIDMidPoint != NULL);
		ASSERT_RESOURCE_ALLOCATION("ELI11598", ipUCLIDEndPoint != NULL);

		/*esriGeometry::*/IPointPtr ipESRIMidPoint(/*esriGeometry::*/CLSID_Point), ipESRIEndPoint(/*esriGeometry::*/CLSID_Point);
		ASSERT_RESOURCE_ALLOCATION("ELI11599", ipESRIMidPoint != NULL);
		ASSERT_RESOURCE_ALLOCATION("ELI11600", ipESRIEndPoint != NULL);

		ipUCLIDMidPoint->GetPointInXY(&dX, &dY);
		ipESRIMidPoint->PutCoords(dX, dY);
		ipUCLIDEndPoint->GetPointInXY(&dX, &dY);
		ipESRIEndPoint->PutCoords(dX, dY);

		/*esriGeometry::*/ICircularArcPtr ipCircularArc(ipESRSegment);
		ASSERT_RESOURCE_ALLOCATION("ELI11601", ipCircularArc != NULL);
		/*esriGeometry::*/IConstructCircularArcPtr ipConstructArc(ipCircularArc);
		ASSERT_RESOURCE_ALLOCATION("ELI11602", ipConstructArc != NULL);

		// construct the curve using start, mid and end points
		ipConstructArc->ConstructThreePoints(ipESRIStartPoint,
							ipESRIMidPoint, ipESRIEndPoint, VARIANT_FALSE);
	}
}
//-------------------------------------------------------------------------------------------------
void CArcGISDisplayAdapter::validateLicense()
{
	// Call validateIcoMapLicensed() in IcoMapOptions in order to check 
	// either license file or USB key license
	IcoMapOptions::sGetInstance().validateIcoMapLicensed();
}
//-------------------------------------------------------------------------------------------------
void CArcGISDisplayAdapter::validateObjects()
{
	// m_ipArcMapApp and m_ipArcMapEditor must be set
	if (m_ipArcMapApp == NULL || m_ipArcMapEditor == NULL)
	{
		throw UCLIDException("ELI11484", "Internal Error: you must call SetApplicationHook() first.");
	}
}
//-------------------------------------------------------------------------------------------------
