#pragma once

#include <TPPoint.h>

#include <string>
#include <vector>
#include <map>

// store matrix of point coordinates
//  P(0,0)  P(0,1)  P(0,2)  P(0,3) ...
//  P(1,0)  P(1,1)  P(1,2)  P(1,3) ...
//  P(2,0)  P(2,1)  P(2,2)  P(2,3) ...
//  ...
typedef std::vector<std::vector<TPPoint> > PointsMatrix;

struct AttributeSet
{
	AttributeSet();
	
	AttributeSet(const AttributeSet& toCopy);
	AttributeSet& operator = (const AttributeSet& toAssign);

	std::string m_strAttrName;
	long m_nFieldLength;
	esriFieldType m_fieldType;
	std::string m_strAttrValue;
};

// stores all attribute values
struct AttributeValues
{
	AttributeValues();
	
	AttributeValues(const AttributeValues& toCopy);
	AttributeValues& operator = (const AttributeValues& toAssign);
	
	AttributeSet m_CountyCode;
	AttributeSet m_Township;
	AttributeSet m_TownshipDir;
	AttributeSet m_Range;
	AttributeSet m_RangeDir;
	AttributeSet m_SectionNum;
	AttributeSet m_Quarter;
	AttributeSet m_QQ;
	AttributeSet m_QQQ;	// this value will not be stored since this feature is not required
	AttributeSet m_QQQQ;
};

enum ELayer{kNoLayer = 0, kTownship, kSection, kQLayer, kQQLayer, kQQQLayer, kQQQQLayer};

class GridDrawer
{
public:
	GridDrawer();
	~GridDrawer();

	// creates proper fields for each layer
	void createAllFields();

	// draw the grid
	void drawGrid();

	// set the ArcMap editor
	void setEditor(IEditorPtr ipEditor) {m_ipEditor = ipEditor;}

private:

	/////////////
	// Variables
	////////////

	IEditorPtr m_ipEditor;

	std::map<ELayer, std::string> m_mapLayerCodeToName;
	// maps the layer with the bool flag to indicate if it should be drawn
	std::map<ELayer, BOOL> m_mapLayerToDrawLayer;

	bool m_bUseExistingFeatures;

	// Used to contain tell user which qtr sections could not be drawn due to incorrect number of vertices
	std::string m_strProblemStrings;

	////////////
	// Methods
	///////////
	// Divide four-side polygon proportionally into four quarters.
	// and store these points in the matrix.
	PointsMatrix calculateQuarterPointsMatrix(const std::vector<TPPoint>& vecPoints);

	// Given four corner coordinates of the quarter, draw each quarter
	// nQuarterLevel - which level should these quarters at. For instance, the
	// quarters within each section is at level 1, the quarters within level 1 
	// quarter is at level 2, etc.
	// vecPoints is the points that define the features being quartered
	void drawQuarters( const std::vector<TPPoint>& vecPoints, int nQuarterLevel, AttributeValues attrValues );

	// Returns the 4 corners of the quarter indicated with nQtr 
	std::vector<TPPoint> getQtrPoints( const PointsMatrix &quarterMatrix, int nQtr );

	// draw polygon on the specified layer with all points defined
	void drawPolygon(const std::vector<TPPoint>& vecPoints,
					ELayer eLayer, AttributeValues attrValues);

	// Check if the specified attribute exists or not. Only create
	// the field if it's not there
	long findFeatureAttributeField(IFeatureClassPtr ipFeatureClass, 
									const std::string& strAttrName,
									esriFieldType type, long length, bool bCreate);

	// get current selected feature. 
	// Require : there's one and only one feature selected
	IFeaturePtr getCurrentSelectedFeature();

	// Which layer is this feature on
	ELayer getFeatureLayer(IFeaturePtr ipSelectedFeature);

	// return the specified layer
	ILayerPtr getLayer(const std::string& strLayerName);

	// get the mid point of the line formed by p1 and p2
	TPPoint getMidPoint(const TPPoint& p1, const TPPoint& p2);

	// get polygon vertices in a clockwise direction
	std::vector<TPPoint> getPolygonVertices(IFeaturePtr ipPolygonFeature);

	// get the specified quarter value
	// For instance, if nQuarterLevel == 1, the value is obtained
	// from attrValue.m_strQuarter. The value shall
	// be containing one digit, eg. 1, 2, 3 or 4
	std::string getQuarterValue(int nQuarterLevel, const AttributeValues& attrValues);

	// set current layer
	void setCurrentLayer(ELayer eLayer, AttributeValues attrValues);

	// set quarter value
	void setQuarterValue(AttributeValues& attrValues, 
		int nQuarterLevel, const std::string& strQuarterValue);

	void storeAttributeValues(ELayer eLayer, const AttributeValues& attrValues);

	// store a row of attribute values
	void storeRow(IFeaturePtr ipFeature, const std::vector<AttributeSet>& vecAttrSets);

	// load drawing layers from INI file into m_mapLayerCodeToName map
	void loadLayersFromINI( std::string strINIFile );

	std::string getWhereClause(AttributeValues& attrValues, 
								 int nQuarterLevel);

	void addANDClause( std::string& strWhereClause, AttributeSet attributeSet);
	
	// Selects the features on the given layer with the given attrValues
	bool selectFeature(ELayer eLayer, AttributeValues& attrValues);
	
	void displayMultipleFeaturesFoundDlg(int nNumberFound, AttributeValues& attrValues, 
								 int nQuarterLevel);
};