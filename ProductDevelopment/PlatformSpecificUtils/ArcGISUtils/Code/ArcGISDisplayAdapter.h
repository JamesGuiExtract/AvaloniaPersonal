// ArcGISDisplayAdapter.h : Declaration of the CArcGISDisplayAdapter

#pragma once

#include "resource.h"       // main symbols

#include <IConfigurationSettingsPersistenceMgr.h>

#include <string>
#include <memory>

/////////////////////////////////////////////////////////////////////////////
// CArcGISDisplayAdapter
class ATL_NO_VTABLE CArcGISDisplayAdapter : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CArcGISDisplayAdapter, &CLSID_ArcGISDisplayAdapter>,
	public ISupportErrorInfo,
	public IDispatchImpl<IDisplayAdapter, &IID_IDisplayAdapter, &LIBID_UCLID_GISPLATINTERFACESLib>,
	public IDispatchImpl<UCLID_COMLMLib::ILicensedComponent, &UCLID_COMLMLib::IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IArcGISDependentComponent, &IID_IArcGISDependentComponent, &LIBID_UCLID_ARCGISUTILSLib>
{
public:
	CArcGISDisplayAdapter();
	~CArcGISDisplayAdapter();

DECLARE_REGISTRY_RESOURCEID(IDR_ARCGISDISPLAYADAPTER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CArcGISDisplayAdapter)
	COM_INTERFACE_ENTRY(IDisplayAdapter)
	COM_INTERFACE_ENTRY2(IDispatch, IDisplayAdapter)
	COM_INTERFACE_ENTRY(IArcGISDependentComponent)
	COM_INTERFACE_ENTRY(UCLID_COMLMLib::ILicensedComponent)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
END_COM_MAP()

public:

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IArcGISDependentComponent
	STDMETHOD(SetApplicationHook)(IDispatch *pApp);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// IDisplayAdapter
		STDMETHOD(raw_AddLineSegment)(UCLID_FEATUREMGMTLib::ILineSegment *ipLineSegment, BSTR* segmentID);
		STDMETHOD(raw_AddCurveSegment)(UCLID_FEATUREMGMTLib::IArcSegment *ipArcSegment, BSTR* segmentID);
		STDMETHOD(raw_FinishCurrentSketch)(BSTR* featureID);
		STDMETHOD(raw_DeleteCurrentSketch)();
		STDMETHOD(raw_FinishCurrentPart)();
		STDMETHOD(raw_EraseLastSegment)();
		STDMETHOD(raw_get_SupportsPartCreation)(VARIANT_BOOL *pVal);
		STDMETHOD(raw_get_SupportsSketchCreation)(VARIANT_BOOL *pVal);
		STDMETHOD(raw_SelectDefaultTool)();
		STDMETHOD(raw_GetLastPoint)(double* dX, double* dY, VARIANT_BOOL *pVal);
		STDMETHOD(raw_Reset)(void);
		STDMETHOD(raw_SelectFeatures)(BSTR strCommonSourceDoc);
		STDMETHOD(raw_GetCurrentDistanceUnit)(EDistanceUnitType *eCurrentUnitType);
		STDMETHOD(raw_SetFeatureGeometry)(BSTR strFeatureID, UCLID_FEATUREMGMTLib::IUCLDFeature *ipFeature); 
		STDMETHOD(raw_GetFeatureGeometry)(BSTR strFeatureID, UCLID_FEATUREMGMTLib::IUCLDFeature **ipFeature);
		STDMETHOD(raw_SetStartPointForNextPart)(double dX, double dY);
		STDMETHOD(raw_SelectTool)(BSTR strToolName);
		STDMETHOD(raw_GetLastSegmentTanOutAsPolarAngleInRadians)(double *pdTangentOutAngle, VARIANT_BOOL *pbSucceeded);
		STDMETHOD(raw_Undo)();
		STDMETHOD(raw_Redo)();
		STDMETHOD(raw_GetFeatureType)(long *peFeatureType);
		STDMETHOD(raw_FlashSegment)(long nPartIndex, long nSegmentIndex);
		STDMETHOD(raw_UpdateSegments)(long nPartIndex, 
									  long nStartSegmentIndex, 
									  IIUnknownVector* pUpdatedSegmentsForThisPart);


private:
	//////////////
	// Constants
	//////////////
	const static std::string ROOT_FOLDER;
	const static std::string GENERAL_FOLDER;
	const static std::string GROUNDTOGRID_KEY;
	const static std::string TOOLNAME_GUID_FOLDER;

	////////////
	// Variables
	////////////

	//=============
	// ESRI objects
	//=============
	/*esriFramework::*/IApplicationPtr m_ipArcMapApp;
	/*esriEditor::*/IEditorPtr m_ipArcMapEditor;
	// contains all segments within the current sketch.
	// each sketch could contain one or more parts
	/*esriGeometry::*/IGeometryCollectionPtr m_ipCurrentSketch;
	// current part of the sketch. Each part is of type Curve
	/*esriGeometry::*/ICurvePtr m_ipCurrentPart;

	// whether or not a sketch is currently being drawn
	bool m_bIsDrawingSketch;
	// whether or not current sketch is created on a polygon layer
	bool m_bIsPolygon;

	//=======================
	// Ground-To-Grid related
	//=======================
	// Actual distance  = Entered distance * m_dDistanceFactor
	double m_dDistanceFactor;
	// Actual angle = Entered angle + m_dAngleOffset
	// this value is in radians
	double m_dAngleOffset;
	// persistent manager
	std::auto_ptr<IConfigurationSettingsPersistenceMgr> m_apCfgMgr;

	//////////
	// Methods
	//////////
	// add a segment to the current sketch part
	void addSegment(/*esriGeometry::*/IESRSegmentPtr ipSegment);

	// whether or not these two commands are the same
	bool areSameCommandItems(/*esriFramework::*/ICommandItemPtr ipFirstCommand, /*esriFramework::*/ICommandItemPtr ipSecondCommand);

	// Adds all uclid segments starting from nStartSegmentIndex into esri part
	// ipOriginUCLIDSegments - vector of all segments in uclid format
	// nStartSegmentIndex - start from this segment from the ipOriginUCLIDSegments
	// nNumOfSegmentsToAdd - starting from nStartSegmentIndex, nNumOfSegmentsToAdd
	//						 of segments in sequence from the ipOriginUCLIDSegments
	//						 will be added to the segment collection
	// Note: This function will do Ground-to-Grid conversion for each segment
	void addToSegmentCollection(/*esriGeometry::*/IESRSegmentCollectionPtr ipSegmentCol, 
				   IIUnknownVectorPtr ipOriginUCLIDSegments,
				   long nStartSegmentIndex,
				   long nNumOfSegmentsToAdd);

	// convert the arc segment from uclid to esri form
	/*esriGeometry::*/IESRSegmentPtr convertArcSegmentToESRISegment(
				ICartographicPointPtr ipUCLIDStartPoint, 
				UCLID_FEATUREMGMTLib::IESSegmentPtr ipUCLIDSegment);

	// convert the line segment from uclid to esri form
	/*esriGeometry::*/IESRSegmentPtr convertLineSegmentToESRISegment(
				ICartographicPointPtr ipUCLIDStartPoint, 
				UCLID_FEATUREMGMTLib::IESSegmentPtr ipUCLIDSegment);

	// Re-calculate the segment if the Ground-To-Grid is on
	UCLID_FEATUREMGMTLib::IESSegmentPtr convertSegment(UCLID_FEATUREMGMTLib::IESSegmentPtr ipSegment);

	// pass in the command guid in string format
	void executeCommandWithGUID(const std::string& strCommandGUID);

	// pass in the command name in string format
	// returns false if command could not be found, otherwise true
	bool executeCommandWithName(const std::string& strCommandName);

	// get command item object using command GUID
	/*esriFramework::*/ICommandItemPtr getCommandItemByGUID(const std::string& strCommandGUID);

	// get command item object using command name
	/*esriFramework::*/ICommandItemPtr getCommandItemByName(const std::string& strCommandName);

	IConfigurationSettingsPersistenceMgr* getConfigManager();

	// Return the end point of the segment collection
	/*esriGeometry::*/IPointPtr getEndPointOfSegmentCollection(/*esriGeometry::*/IESRSegmentCollectionPtr ipSegCol);

	// Retrieve info from current esri feature and store them into uclid feature
	void getFeature(/*esriGeometry::*/IGeometryPtr ipGeometry, 
		UCLID_FEATUREMGMTLib::IUCLDFeaturePtr ipUCLIDFeature);

	// get last point from the current sketch
	// if last point is null, then there's no point for current sketch
	/*esriGeometry::*/IPointPtr getLastPointFromSketch();

	// get last segment's tangent out bearing in string format 
	std::string getLastTangentOutBearingAsStringValue();

	// return the last segment's tangent out direction in Radians
	bool getLastTangentOutInRadians(double& dTangentOutInRadians);

	// get info from current part in ArcMap and store them in uclid part
	void getPart(/*esriGeometry::*/ICurvePtr ipCurve, IPartPtr ipUCLIDPart);

	// get all info from esri geometry and store them into the uclid segment
	void getSegment(/*esriGeometry::*/IESRSegmentPtr ipESRSegment, 
					UCLID_FEATUREMGMTLib::IESSegmentPtr ipUCLIDSegment);

	// depend on the curve type, call groundToGridBearingConversion or
	// groundToGridDistanceConversion internally.
	std::string groundToGridConversion(ECurveParameterType eCurveType, const std::string& strValue);

	// adds ground to grid correction to the input bearing
	std::string groundToGridBearingConversion(const std::string& strInputBearing);

	// adds ground to grid correction to the input distance
	// REQUIRE: the input string must be valid distance string, such as 122.34ft,
	//			45 meter, 345.56 m, etc.
	std::string groundToGridDistanceConversion(const std::string& strInputDistance);

	// Initialize geometry parameters, we are about to draw a sketch
	void initDrawing();

	// whether or not the Ground-To-Grid is on
	bool isGroundToGridOn();

	// store uclid feature into esri feature
	void setFeature(/*esriGeometry::*/IGeometryPtr ipGeometry, UCLID_FEATUREMGMTLib::IUCLDFeaturePtr ipUCLIDFeature);

	// store uclid part into esri part
	void setPart(/*esriGeometry::*/ICurvePtr ipCurve, IPartPtr ipUCLIDPart);

	// store uclid segment info into esri segment
	void setSegment(/*esriGeometry::*/IESRSegmentPtr ipESRSegment, 
					UCLID_FEATUREMGMTLib::IESSegmentPtr ipUCLIDSegment,
					ICartographicPointPtr ipUCLIDStartPoint);

	void validateLicense();

	// validate objects
	void validateObjects();
};

