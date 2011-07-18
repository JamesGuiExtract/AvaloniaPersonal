#include "stdafx.h"
#include "resource.h"
#include "GridDrawer.h"

#include <INIFilePersistenceMgr.h>
#include "PromptValuesDlg.h"
#include "Constants.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <COMUtils.h>
#include <TPLineSegment.h>
#include <set>
#include "DisplayQtrsNotDrawnDlg.h"

// length of each side of a township, unit is in U.S. Survey Feet
//static const double g_dTownshipLength = 31680;

//-------------------------------------------------------------------------------------------------
// GridDrawer
//-------------------------------------------------------------------------------------------------
GridDrawer::GridDrawer()
{
	try
	{
		string strINIPath = ::getModuleDirectory(_Module.m_hInst) + "\\" + string( "GridGenerator.ini" );
		loadLayersFromINI(strINIPath);


		m_mapLayerToDrawLayer.clear();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12651");
}
//-------------------------------------------------------------------------------------------------
GridDrawer::~GridDrawer()
{
}

//-------------------------------------------------------------------------------------------------
// Public functions
//-------------------------------------------------------------------------------------------------
AttributeSet::AttributeSet()
: m_strAttrName(""),
  m_nFieldLength(0),
  m_fieldType(esriFieldTypeSmallInteger),
  m_strAttrValue("")
{
}
//-------------------------------------------------------------------------------------------------
AttributeSet::AttributeSet(const AttributeSet& toCopy)
{
	m_strAttrName = toCopy.m_strAttrName;
	m_nFieldLength = toCopy.m_nFieldLength;
	m_fieldType = toCopy.m_fieldType;
	m_strAttrValue = toCopy.m_strAttrValue;
}
//-------------------------------------------------------------------------------------------------
AttributeSet& AttributeSet::operator = (const AttributeSet& toAssign)
{
	m_strAttrName = toAssign.m_strAttrName;
	m_nFieldLength = toAssign.m_nFieldLength;
	m_fieldType = toAssign.m_fieldType;
	m_strAttrValue = toAssign.m_strAttrValue;

	return *this;
}
//-------------------------------------------------------------------------------------------------
AttributeValues::AttributeValues()
{
	m_CountyCode.m_strAttrName = COUNTY_CODE;
	m_CountyCode.m_nFieldLength = 3;

	m_Township.m_strAttrName = TOWNSHIP;
	m_Township.m_nFieldLength = 3;

	m_TownshipDir.m_strAttrName = TOWNSHIP_DIR;
	m_TownshipDir.m_nFieldLength = 1;
	m_TownshipDir.m_fieldType = esriFieldTypeString;

	m_Range.m_strAttrName = RANGE;
	m_Range.m_nFieldLength = 3;

	m_RangeDir.m_strAttrName = RANGE_DIR;
	m_RangeDir.m_nFieldLength = 1;
	m_RangeDir.m_fieldType = esriFieldTypeString;

	// Note : ArcGIS won't allow any field to be named as "Section"
	m_SectionNum.m_strAttrName = SECTION_NUM;
	m_SectionNum.m_nFieldLength = 3;

	m_Quarter.m_strAttrName = QUARTER;
	m_Quarter.m_nFieldLength = 4;

	m_QQ.m_strAttrName = QUARTER_QUARTER;
	m_QQ.m_nFieldLength = 4;

	m_QQQ.m_strAttrName = QUARTER_QUARTER_QUARTER;
	m_QQQ.m_nFieldLength = 4;

	m_QQQQ.m_strAttrName = QQQQ;
	m_QQQQ.m_nFieldLength = 4;
}
//-------------------------------------------------------------------------------------------------
AttributeValues::AttributeValues(const AttributeValues& toCopy)
{
	m_CountyCode = toCopy.m_CountyCode;
	m_Township = toCopy.m_Township;
	m_TownshipDir = toCopy.m_TownshipDir;
	m_Range = toCopy.m_Range;
	m_RangeDir = toCopy.m_RangeDir;
	m_SectionNum = toCopy.m_SectionNum;
	m_Quarter = toCopy.m_Quarter;
	m_QQ = toCopy.m_QQ;
	m_QQQ = toCopy.m_QQQ;
	m_QQQQ = toCopy.m_QQQQ;
}
//-------------------------------------------------------------------------------------------------
AttributeValues& AttributeValues::operator = (const AttributeValues& toAssign)
{
	m_CountyCode = toAssign.m_CountyCode;
	m_Township = toAssign.m_Township;
	m_TownshipDir = toAssign.m_TownshipDir;
	m_Range = toAssign.m_Range;
	m_RangeDir = toAssign.m_RangeDir;
	m_SectionNum = toAssign.m_SectionNum;
	m_Quarter = toAssign.m_Quarter;
	m_QQ = toAssign.m_QQ;
	m_QQQ = toAssign.m_QQQ;
	m_QQQQ = toAssign.m_QQQQ;

	return *this;
}
//-------------------------------------------------------------------------------------------------
void GridDrawer::createAllFields()
{
	// eitor must not be null
	if (m_ipEditor == NULL)
	{
		throw UCLIDException("ELI08189", "Editor must be set before this call can be made.");
	}

	AttributeValues attrValues;
	IMapPtr ipMap = m_ipEditor->Map;
	long nNumOfLayers = ipMap->LayerCount;
	for (long n=0; n<nNumOfLayers; n++)
	{
		ILayerPtr ipLayer = ipMap->GetLayer(n);
		string strLayer = _bstr_t(ipLayer->Name);
		// if this layer is one of the layers to create features
		map<ELayer, string>::iterator itMap = m_mapLayerCodeToName.begin();
		for (; itMap != m_mapLayerCodeToName.end(); itMap++)
		{
			if (strLayer == itMap->second)
			{
				ELayer eLayer = itMap->first;
				IFeatureLayerPtr ipFeatureLayer = ipLayer;
				// make sure the feature attributes on this layer is all created
				IFeatureClassPtr ipFeatureClass(ipFeatureLayer->FeatureClass);
				// all layer shall have these following fields
				findFeatureAttributeField(ipFeatureClass, attrValues.m_CountyCode.m_strAttrName,
					attrValues.m_CountyCode.m_fieldType, attrValues.m_CountyCode.m_nFieldLength, true);
				findFeatureAttributeField(ipFeatureClass, attrValues.m_Township.m_strAttrName,
					attrValues.m_Township.m_fieldType, attrValues.m_Township.m_nFieldLength, true);
				findFeatureAttributeField(ipFeatureClass, attrValues.m_TownshipDir.m_strAttrName,
					attrValues.m_TownshipDir.m_fieldType, attrValues.m_TownshipDir.m_nFieldLength, true);
				findFeatureAttributeField(ipFeatureClass, attrValues.m_Range.m_strAttrName,
					attrValues.m_Range.m_fieldType, attrValues.m_Range.m_nFieldLength, true);
				findFeatureAttributeField(ipFeatureClass, attrValues.m_RangeDir.m_strAttrName,
					attrValues.m_RangeDir.m_fieldType, attrValues.m_RangeDir.m_nFieldLength, true);
				
				switch (eLayer)
				{
				case kQQQQLayer:
					findFeatureAttributeField(ipFeatureClass, attrValues.m_SectionNum.m_strAttrName,
						attrValues.m_SectionNum.m_fieldType, attrValues.m_SectionNum.m_nFieldLength, true);
					findFeatureAttributeField(ipFeatureClass, attrValues.m_QQQQ.m_strAttrName,
						attrValues.m_QQQQ.m_fieldType, attrValues.m_QQQQ.m_nFieldLength, true);
					break;
				
				case kQQQLayer:
					findFeatureAttributeField(ipFeatureClass, attrValues.m_QQQ.m_strAttrName,
						attrValues.m_QQQ.m_fieldType, attrValues.m_QQQ.m_nFieldLength, true);

				case kQQLayer:
					findFeatureAttributeField(ipFeatureClass, attrValues.m_QQ.m_strAttrName,
						attrValues.m_QQ.m_fieldType, attrValues.m_QQ.m_nFieldLength, true);
					
				case kQLayer:
					findFeatureAttributeField(ipFeatureClass, attrValues.m_Quarter.m_strAttrName,
						attrValues.m_Quarter.m_fieldType, attrValues.m_Quarter.m_nFieldLength, true);
					
				case kSection:
					findFeatureAttributeField(ipFeatureClass, attrValues.m_SectionNum.m_strAttrName,
						attrValues.m_SectionNum.m_fieldType, attrValues.m_SectionNum.m_nFieldLength, true);
				}

				break;
			}
		}
	}
}
//-------------------------------------------------------------------------------------------------
void GridDrawer::drawGrid()
{
	// eitor must not be null
	if (m_ipEditor == NULL)
	{
		throw UCLIDException("ELI08130", "Editor must not be NULL.");
	}

	// before bringing up the dialog, make sure there's one and only one
	// Section feature selected
	IFeaturePtr ipSelectedFeature = getCurrentSelectedFeature();
	ELayer eLayer = getFeatureLayer(ipSelectedFeature);

	// prompt for county code, township & direction, range & direction, 
	// section, quarter, quarter-quarter
	PromptValuesDlg promptDlg;
	switch (eLayer)
	{
	case kSection:
		break;

	case kQQQLayer:
		promptDlg.enableQuarterQuarterQuarter(true);
		
	case kQQLayer:
		promptDlg.enableQuarterQuarter(true);

	case kQLayer:
		promptDlg.enableQuarter(true);
		break;

	default:
		{
			string strMsg("You must select one and only one feature on the ");
			strMsg += SECTION_LAYER + ", " + QUARTER_LAYER  + ", " + QQ_LAYER  + ", or " + QQQ_LAYER + " layer.";
			throw UCLIDException("ELI08435", strMsg);
		}
		break;
	}
	// Changed the text in the top static control to indicate the selected features layer
	CString zStaticSelectText;
	zStaticSelectText.Format("You have selected a [%s] Feature to subdivide into smaller parts. Please specify attributes for this feature.", m_mapLayerCodeToName[eLayer].c_str());
	promptDlg.setStaticSelectText(zStaticSelectText);
	// Display Prompt dialog
	if (promptDlg.DoModal() != IDOK)
	{
		return;
	}
	
	string strINIPath = ::getModuleDirectory(_Module.m_hInst) + "\\" + string( "GridGenerator.ini" );
	loadLayersFromINI(strINIPath);

	// create AttributeValues to hold these values
	AttributeValues attrValues;
	attrValues.m_CountyCode.m_strAttrValue = (LPCTSTR)promptDlg.m_zCountyCode;
	attrValues.m_Township.m_strAttrValue = (LPCTSTR)promptDlg.m_zTownship;
	attrValues.m_TownshipDir.m_strAttrValue = (LPCTSTR)promptDlg.m_cmbTownshipDir == 0 ? "N" : "S";
	attrValues.m_Range.m_strAttrValue = (LPCTSTR)promptDlg.m_zRange;
	attrValues.m_RangeDir.m_strAttrValue = (LPCTSTR)promptDlg.m_cmbRangeDir == 0 ? "E" : "W";
	attrValues.m_SectionNum.m_strAttrValue = (LPCTSTR)promptDlg.m_zSection;
	if (!promptDlg.m_zQuarter.IsEmpty())
	{
		attrValues.m_Quarter.m_strAttrValue = (LPCTSTR)promptDlg.m_zQuarter;
	}
	if (!promptDlg.m_zQQ.IsEmpty())
	{
		attrValues.m_QQ.m_strAttrValue = (LPCTSTR)promptDlg.m_zQQ;
	}
	if ( !promptDlg.m_zQQQ.IsEmpty())
	{
		attrValues.m_QQQ.m_strAttrValue = (LPCTSTR)promptDlg.m_zQQQ;
	}

	CWaitCursor wait;

	m_bUseExistingFeatures = promptDlg.m_bUseExisting == TRUE;

	// Setup the layers to draw
	m_mapLayerToDrawLayer[kQLayer] = promptDlg.m_bDrawQuarter;
	m_mapLayerToDrawLayer[kQQLayer] = promptDlg.m_bDrawQQ;
	m_mapLayerToDrawLayer[kQQQLayer] = promptDlg.m_bDrawQQQ;
	m_mapLayerToDrawLayer[kQQQQLayer] = promptDlg.m_bDrawQQQQ;
	

	// store attributes for current feature
	storeAttributeValues(eLayer, attrValues);

	// get the vertices of the feature to divide
	vector<TPPoint> vecQuarterCorners = getPolygonVertices(ipSelectedFeature);
	
	// Set the problem string to empty
	m_strProblemStrings = "";
	
	// draw the quarters inside each section
	drawQuarters(vecQuarterCorners, ((int)eLayer)-1, attrValues);

	// If unable to create quarters from existing m_strProblemStrings will be non empty
	if ( m_strProblemStrings != "" )
	{
		// Display the quarters not drawn
		DisplayQtrsNotDrawnDlg dlg;
		dlg.setQuartersNotDrawnText(m_strProblemStrings);
		dlg.DoModal();
	}
}
//-------------------------------------------------------------------------------------------------

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
PointsMatrix GridDrawer::calculateQuarterPointsMatrix(const vector<TPPoint>& vecPoints)
{
	int nSize = vecPoints.size();
	// make sure the vector contains only four points
	PointsMatrix matrix;
	if (nSize != 4)
	{
		if ( m_bUseExistingFeatures )
		{
			// if Using Existing features return an empty matrix
			return matrix;
		}
		throw UCLIDException("ELI08432", "Each polygon must be four-side polygon.");
	}

	// calculate the mid point of each side
	TPPoint pMid1 = getMidPoint(vecPoints[0], vecPoints[1]);
	TPPoint pMid2 = getMidPoint(vecPoints[1], vecPoints[2]);
	TPPoint pMid3 = getMidPoint(vecPoints[2], vecPoints[3]);
	TPPoint pMid4 = getMidPoint(vecPoints[3], vecPoints[0]);
	// the mid point of the two mid points
	TPPoint pMid5 = getMidPoint(pMid2, pMid4);

	
	vector<TPPoint> vecPointsInARow;
	vecPointsInARow.push_back(vecPoints[0]);
	vecPointsInARow.push_back(pMid1);
	vecPointsInARow.push_back(vecPoints[1]);
	matrix.push_back(vecPointsInARow);

	vecPointsInARow.clear();
	vecPointsInARow.push_back(pMid4);
	vecPointsInARow.push_back(pMid5);
	vecPointsInARow.push_back(pMid2);
	matrix.push_back(vecPointsInARow);

	vecPointsInARow.clear();
	vecPointsInARow.push_back(vecPoints[3]);
	vecPointsInARow.push_back(pMid3);
	vecPointsInARow.push_back(vecPoints[2]);
	matrix.push_back(vecPointsInARow);

	return matrix;
}
//-------------------------------------------------------------------------------------------------
void GridDrawer::drawQuarters( const vector<TPPoint>& vecPoints, int nQuarterLevel, AttributeValues attrValues )
{
	// No layers to draw under 4
	if ( nQuarterLevel > 4 )
	{
		return;
	}
	// Get the Features Points
	if ( vecPoints.size() != 4 && !m_bUseExistingFeatures )
	{
		// The Quarters can not be drawn
		UCLIDException ue ("ELI13461", "Selected polygon must be a four-side polygon." );
		ue.addDebugInfo("# sides",  vecPoints.size());
		throw ue;
	}
	PointsMatrix matrix;
	// This is used to indicate if the matrix has been calculated
	// it only needs to be calculated if not using existing features or those existing features don't
	// exist and 
	bool bMatrixCalculated = false;

	// Calculate lay above current
	ELayer eLayerToDraw = (ELayer)((int)kQLayer + nQuarterLevel - 1);

	// Loop thru quarters to draw
	for ( int nQuarterNum = 1; nQuarterNum <= 4; nQuarterNum++ )
	{
		// get the quarter number one level above this quarter
		string strOneLevelUpQuarterValue("");
		if (nQuarterLevel > 1)
		{
			strOneLevelUpQuarterValue = getQuarterValue(nQuarterLevel - 1, attrValues);
		}

		// Set the value for the new quarter section being drawn in th attrValues structure
		setQuarterValue(attrValues, nQuarterLevel, 
			::asString(nQuarterNum) + strOneLevelUpQuarterValue);

		// If drawing using existing features need to check if the feature exists
		if ( m_bUseExistingFeatures ) 
		{
			// Select the feature with the attributes
			bool bFeatureExists = selectFeature ( eLayerToDraw, attrValues );

			IMapPtr ipMap = m_ipEditor->Map;
			if ( ipMap->SelectionCount > 1 )
			{
				// More that one features with the given attrValues
				displayMultipleFeaturesFoundDlg(ipMap->SelectionCount,  attrValues, nQuarterLevel );
				// don't know which to use so go to next 
				continue;
			}
			else if ( ipMap->SelectionCount == 1 )
			{
				// Found an existing feature use its corners to draw lower level features
				vector<TPPoint> vecQuarterCorners = getPolygonVertices(getCurrentSelectedFeature());
				drawQuarters( vecQuarterCorners, nQuarterLevel + 1,  attrValues );
				// go to the next quarter, this already exists
				continue;
			}
		}
		// will need to try to draw this feature, so if Matrix is not calculated calculate it
		if ( !bMatrixCalculated )
		{
			matrix = calculateQuarterPointsMatrix( vecPoints );
			bMatrixCalculated = true;
		}
		vector<TPPoint> vecQuarterCorners;
		// matrix size could be zero if use existing and not 4 points
		if ( matrix.size() != 0 )
		{
			// Get the point for the quarter being drawn or to pass to next level
			vecQuarterCorners = getQtrPoints( matrix, nQuarterNum );
			if ( m_mapLayerToDrawLayer[eLayerToDraw] == TRUE )
			{
				// draw the quarter
				drawPolygon(vecQuarterCorners, eLayerToDraw, attrValues);
			}
		}
		else 
		{
			// The where not 4 vertices so the quarters could not be calculated
			if (m_mapLayerToDrawLayer[eLayerToDraw] == TRUE )
			{
				// Add a string the the Problem strings to show this quarter was not drawn
				string strAddString = "Quarter " + getQuarterValue(nQuarterLevel , attrValues )
					+ " could not be drawn\r\n"; 
				m_strProblemStrings = m_strProblemStrings + strAddString;
			}
		}
		// draw quarters for next lavel
		drawQuarters ( vecQuarterCorners, nQuarterLevel+1, attrValues );
	}
}
//-------------------------------------------------------------------------------------------------
vector<TPPoint> GridDrawer::getQtrPoints( const PointsMatrix &quarterMatrix, int nQtr )
{
	// get the start col position for the quarter
	int nColPos = nQtr % 2;
	int nRowPos;
	// Get the start row position for the quarter
	if ( nQtr < 3 )
	{
		nRowPos = 0;
	}
	else
	{
		nRowPos = 1;
	}
	vector<TPPoint> vecPrevRowPoints = quarterMatrix[nRowPos];
	// the row after previous row
	vector<TPPoint> vecSecondRowPoints = quarterMatrix[nRowPos+1];
	
	vector<TPPoint> vecQuarterCorners;
	vecQuarterCorners.push_back(vecPrevRowPoints[nColPos]);
	vecQuarterCorners.push_back(vecPrevRowPoints[nColPos+1]);
	vecQuarterCorners.push_back(vecSecondRowPoints[nColPos+1]);
	vecQuarterCorners.push_back(vecSecondRowPoints[nColPos]);

	return vecQuarterCorners;
}
//-------------------------------------------------------------------------------------------------
void GridDrawer::drawPolygon(const vector<TPPoint>& vecPoints,
							 ELayer eLayer, 
							 AttributeValues attrValues)
{
	// set current layer if not set already
	setCurrentLayer(eLayer, attrValues);

	ISketchOperationPtr ipSkOp(CLSID_SketchOperation);
	ASSERT_RESOURCE_ALLOCATION("ELI08138", ipSkOp != NULL);

	// record the sketch operation on stack
	ipSkOp->Start(m_ipEditor);

	// create a new feature
	IEditSketchPtr ipEditSketch(m_ipEditor);
	ASSERT_RESOURCE_ALLOCATION("ELI08134", ipEditSketch != NULL);

	ISegmentCollectionPtr ipSegColl = ipEditSketch->Geometry;
	ASSERT_RESOURCE_ALLOCATION("ELI08135", ipSegColl != NULL);

	// create a new segment
	for (unsigned int n=0; n<vecPoints.size(); n++)
	{
		TPPoint fromPoint = vecPoints[n];
		TPPoint toPoint;
		if (n == vecPoints.size()-1)
		{
			// the last point of the rectangular is the first point in the vector
			toPoint = vecPoints[0];
		}
		else
		{
			toPoint = vecPoints[n+1];
		}

		ILinePtr ipLine(CLSID_Line);
		IPointPtr ipFromPoint(CLSID_Point);
		IPointPtr ipToPoint(CLSID_Point);
		ipFromPoint->PutX(fromPoint.m_dX);
		ipFromPoint->PutY(fromPoint.m_dY);
		ipToPoint->PutX(toPoint.m_dX);
		ipToPoint->PutY(toPoint.m_dY);
		ipLine->PutFromPoint(ipFromPoint);
		ipLine->PutToPoint(ipToPoint);
			
		ISegmentPtr ipSegment(ipLine);
		ASSERT_RESOURCE_ALLOCATION("ELI08137", ipSegment != NULL);
		
		ipSegColl->AddSegment(ipSegment);
	}

	// finish the sketch
	ipSkOp->Finish(ipEditSketch->Geometry->Envelope);

	ipEditSketch->RefreshSketch();
	ipEditSketch->FinishSketch();

	// add attribute values to each field of the newly created feature
	storeAttributeValues(eLayer, attrValues);
}
//-------------------------------------------------------------------------------------------------
long GridDrawer::findFeatureAttributeField(IFeatureClassPtr ipFeatureClass, 
										   const string& strAttrName,
										   esriFieldType type,
										   long length,
										   bool bCreate)
{
	_bstr_t bstrFieldName(strAttrName.c_str());
	long index = ipFeatureClass->FindField(bstrFieldName);
	// if the field doesn't exist
	if (index < 0 && bCreate)
	{
		// create a new field
		IFieldPtr ipField(CLSID_Field);
		ASSERT_RESOURCE_ALLOCATION("ELI08188", ipField != NULL);
		IFieldEditPtr ipFieldEdit(ipField);
		if (ipFieldEdit)
		{
			ipFieldEdit->Name = bstrFieldName;
			ipFieldEdit->Type = type;
			
			ipFieldEdit->Length = length;
			ipFieldEdit->IsNullable = VARIANT_TRUE;
			ipFieldEdit->Required = VARIANT_FALSE;
			
			try
			{
				// now add the field
				ipFeatureClass->AddField(ipField);
			}
			catch (...)
			{
				// if the field can not be created, ignore it.
			}
		}
	}

	return index;
}
//-------------------------------------------------------------------------------------------------
IFeaturePtr GridDrawer::getCurrentSelectedFeature()
{
	IMapPtr ipMap = m_ipEditor->Map;
	if (ipMap->SelectionCount != 1)
	{
		throw UCLIDException("ELI08433", "Please select one and only one feature.");
	}

	IEnumFeaturePtr ipEnumFeature = ipMap->FeatureSelection;
	ipEnumFeature->Reset();
	IFeaturePtr ipFeature = ipEnumFeature->Next();

	return ipFeature;
}
//-------------------------------------------------------------------------------------------------
ELayer GridDrawer::getFeatureLayer(IFeaturePtr ipFeature)
{
	IFeatureClassPtr ipFeatureClass = ipFeature->Class;

	map<ELayer, std::string>::iterator itMap = m_mapLayerCodeToName.begin();
	for (; itMap != m_mapLayerCodeToName.end(); itMap++)
	{	
		string strLayerName = itMap->second;
		IFeatureLayerPtr ipFeatureLayer = getLayer(strLayerName);
		if (ipFeatureLayer == NULL)
		{
			throw UCLIDException("ELI08436", "Can't find " + strLayerName + " layer");
		}
		
		IFeatureClassPtr ipFeatureClass2 = ipFeatureLayer->FeatureClass;
		if (ipFeatureClass->FeatureClassID == ipFeatureClass2->FeatureClassID)
		{
			return itMap->first;
		}
	}

	return kNoLayer;
}
//-------------------------------------------------------------------------------------------------
ILayerPtr GridDrawer::getLayer(const string& strLayerName)
{
	IMapPtr ipMap = m_ipEditor->Map;
	long nNumOfLayers = ipMap->LayerCount;
	for (long n=0; n<nNumOfLayers; n++)
	{
		ILayerPtr ipLayer = ipMap->GetLayer(n);
		string strLayer = _bstr_t(ipLayer->Name);
		// if this layer is the expected layer
		if (strLayer == strLayerName)
		{
			return ipLayer;
		}
	}

	return NULL;
}
//-------------------------------------------------------------------------------------------------
TPPoint GridDrawer::getMidPoint(const TPPoint& p1, const TPPoint& p2)
{
	TPLineSegment line(p1, p2);
	return line.getMidTPPoint();
}
//-------------------------------------------------------------------------------------------------
string GridDrawer::getQuarterValue(int nQuarterLevel, const AttributeValues& attrValues)
{
	string strQuarterValue("");

	switch (nQuarterLevel)
	{
	case 1:
		strQuarterValue = attrValues.m_Quarter.m_strAttrValue;
		break;

	case 2:
		strQuarterValue = attrValues.m_QQ.m_strAttrValue;
		break;

	case 3:
		strQuarterValue = attrValues.m_QQQ.m_strAttrValue;
		break;

	case 4:
		strQuarterValue = attrValues.m_QQQQ.m_strAttrValue;
		break;

	default:
		{
			UCLIDException ue("ELI08179", "Invalid quarter level.");
			ue.addDebugInfo("Quarter Level", nQuarterLevel);
			throw ue;
		}
		break;
	}

	return strQuarterValue;
}
//-------------------------------------------------------------------------------------------------
vector<TPPoint> GridDrawer::getPolygonVertices(IFeaturePtr ipPolygonFeature)
{
	vector<TPPoint> vecPolygonPoints;
	IGeometryPtr ipGeo = ipPolygonFeature->Shape;
	IPointCollectionPtr ipPntCollection(ipGeo);
	ASSERT_RESOURCE_ALLOCATION("ELI08438", ipPntCollection != NULL);
	long nNumOfPoints = ipPntCollection->PointCount;
	// assume all features are created as polygons (exterior rings, i.e. their
	// orientation is clockwise)
	// Note: each polygon points collection always has number-of-vertex + 1
	for (long n=0; n<nNumOfPoints-1; n++)
	{
		IPointPtr ipPoint = ipPntCollection->GetPoint(n);
		TPPoint pt(ipPoint->GetX(), ipPoint->GetY());
		vecPolygonPoints.push_back(pt);
	}

	// create a point that is at the top-left most position
	TPPoint topleftPt;
	bool bInitialized = false;
	for (unsigned int n=0; n<vecPolygonPoints.size(); n++)
	{
		if (!bInitialized)
		{
			topleftPt = vecPolygonPoints[n];
			bInitialized = true;
			continue;
		}

		if (topleftPt.m_dX > vecPolygonPoints[n].m_dX)
		{
			topleftPt.m_dX = vecPolygonPoints[n].m_dX;
		}

		if (topleftPt.m_dY < vecPolygonPoints[n].m_dY)
		{
			topleftPt.m_dY = vecPolygonPoints[n].m_dY;
		}
	}

	// find out which point closest to the top-left most point
	// then that is the topleft point of the polygon
	double dShortestDist = -1.0;
	int nTopLeftIndex = 0;
	bInitialized = false;
	for (unsigned int n=0; n<vecPolygonPoints.size(); n++)
	{
		double dDist = topleftPt.distanceTo(vecPolygonPoints[n]);
		if (!bInitialized || dShortestDist > dDist)
		{
			dShortestDist = dDist;
			nTopLeftIndex = n;
			bInitialized = true;
		}
	}

	if (nTopLeftIndex == 0)
	{
		return vecPolygonPoints;
	}

	vector<TPPoint> vecCopy;
	for (unsigned int n = nTopLeftIndex; n<vecPolygonPoints.size(); n++)
	{
		vecCopy.push_back(vecPolygonPoints[n]);
	}
	for ( int n = 0; n<nTopLeftIndex; n++)
	{
		vecCopy.push_back(vecPolygonPoints[n]);
	}

	return vecCopy;
}
//-------------------------------------------------------------------------------------------------
void GridDrawer::setCurrentLayer(ELayer eLayer, AttributeValues attrValues)
{
	IEditLayersPtr ipEditLayers(m_ipEditor);
	ASSERT_RESOURCE_ALLOCATION("ELI08131", ipEditLayers != NULL);

	// find out the current layer
	IFeatureLayerPtr ipFeatureLayer = ipEditLayers->CurrentLayer;
	string strCurrentLayerName = _bstr_t(ipFeatureLayer->Name);

	map<ELayer, string>::iterator itMap = m_mapLayerCodeToName.find(eLayer);
	if (itMap == m_mapLayerCodeToName.end())
	{
		UCLIDException ue("ELI08145", "Layer name is not specified.");
		ue.addDebugInfo("Layer", eLayer);
		throw ue;
	}
	string strLayerToSet = itMap->second;

	ILayerPtr ipLayer = getLayer(strLayerToSet);
	// if this layer is the expected layer
	if (ipLayer != NULL)
	{
		// if current layer is not the layer we want to draw feature on,
		// set the layer
		if (strCurrentLayerName != strLayerToSet)
		{
			ipFeatureLayer = ipLayer;
			ipEditLayers->SetCurrentLayer(ipFeatureLayer, 0);
			strCurrentLayerName = strLayerToSet;
		}
	}

	if (strCurrentLayerName != strLayerToSet)
	{
		UCLIDException ue("ELI08144", "Can't find the layer name specified.");
		ue.addDebugInfo("Expected layer name", strLayerToSet);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void GridDrawer::setQuarterValue(AttributeValues& attrValues, 
								 int nQuarterLevel, 
								 const string& strQuarterValue)
{
	switch (nQuarterLevel)
	{
	case 1:
		attrValues.m_Quarter.m_strAttrValue = strQuarterValue;
		break;

	case 2:
		attrValues.m_QQ.m_strAttrValue = strQuarterValue;
		break;

	case 3:
		attrValues.m_QQQ.m_strAttrValue = strQuarterValue;
		break;

	case 4:
		attrValues.m_QQQQ.m_strAttrValue = strQuarterValue;
		break;

	default:
		{
			UCLIDException ue("ELI08182", "Invalid quarter level.");
			ue.addDebugInfo("Quarter Level", nQuarterLevel);
			throw ue;
		}
		break;
	}
}
//-------------------------------------------------------------------------------------------------
void GridDrawer::storeAttributeValues(ELayer eLayer, const AttributeValues& attrValues)
{
	IMapPtr ipMap = m_ipEditor->Map;
	if (ipMap->SelectionCount == 1)
	{
		IEnumFeaturePtr ipEnumFeature = ipMap->FeatureSelection;
		IFeaturePtr ipFeature = ipEnumFeature->Next();
		
		vector<AttributeSet> vecAttrSets;

		vecAttrSets.push_back(attrValues.m_CountyCode);
		vecAttrSets.push_back(attrValues.m_Township);
		vecAttrSets.push_back(attrValues.m_TownshipDir);
		vecAttrSets.push_back(attrValues.m_Range);
		vecAttrSets.push_back(attrValues.m_RangeDir);

		switch (eLayer)
		{
			case kQQQQLayer:
				vecAttrSets.push_back(attrValues.m_SectionNum);
				vecAttrSets.push_back(attrValues.m_QQQQ);
				break;

			case kQQQLayer:
				vecAttrSets.push_back(attrValues.m_QQQ);
				
			case kQQLayer:
				vecAttrSets.push_back(attrValues.m_QQ);
					
			case kQLayer:
				vecAttrSets.push_back(attrValues.m_Quarter);
					
			case kSection:
				vecAttrSets.push_back(attrValues.m_SectionNum);
		}

		storeRow(ipFeature, vecAttrSets);
	}
}
//-------------------------------------------------------------------------------------------------
void GridDrawer::storeRow(IFeaturePtr ipFeature, const vector<AttributeSet>& vecAttrSets)
{
	if (vecAttrSets.empty())
	{
		return;
	}

	m_ipEditor->StartOperation();
	for (unsigned int n=0; n<vecAttrSets.size(); n++)
	{
		AttributeSet attrSet = vecAttrSets[n];
		long nIndex = findFeatureAttributeField(ipFeature->Class, 
			attrSet.m_strAttrName, attrSet.m_fieldType, attrSet.m_nFieldLength, false);
		
		if (nIndex < 0)
		{
			UCLIDException ue("ELI08198", "No such field found with current created feature.");
			ue.addDebugInfo("Field Name", attrSet.m_strAttrName);
			throw ue;
		}
		_variant_t _varValue(attrSet.m_strAttrValue.c_str());
		ipFeature->PutValue(nIndex, _varValue);
	}
	// commit to the database
	ipFeature->Store();
	m_ipEditor->StopOperation(_bstr_t("Stored feature string database."));
}
//-------------------------------------------------------------------------------------------------
void GridDrawer::loadLayersFromINI( std::string strINIFile )
{
	INIFilePersistenceMgr	mgrSettings( strINIFile  );

		// Create folder name from strSection
	string strFolder = strINIFile;
	strFolder += "\\";
	strFolder += "Layers";

	std::map<ELayer, std::string> mapLCodeToName;

	// Set the default values
	mapLCodeToName[kSection] = SECTION_LAYER;
	mapLCodeToName[kQLayer] = QUARTER_LAYER;
	mapLCodeToName[kQQLayer] = QQ_LAYER;
	mapLCodeToName[kQQQLayer] = QQQ_LAYER;
	mapLCodeToName[kQQQQLayer] = QQQQ_LAYER;

	string strLayerName;

	// Clear out the map
	m_mapLayerCodeToName.clear();

	for ( ELayer i = kSection; i <= kQQQQLayer; i = (ELayer)((int)i + 1) )
	{
		strLayerName = mgrSettings.getKeyValue( strFolder, mapLCodeToName[i], "" );
		if ( !strLayerName.empty() )
		{
			m_mapLayerCodeToName[i] = strLayerName ;
		}
		else
		{
			UCLIDException ue("ELI12675", "Missing Layer Name in GridGenerator.ini file");
			ue.addDebugInfo ( "LayerNameMissing", mapLCodeToName[i] );
			throw ue;
		}
	}

}
//-------------------------------------------------------------------------------------------------
std::string GridDrawer::getWhereClause(AttributeValues& attrValues, 
								 int nQuarterLevel)
{
	string strWhereClause("");
	// county code
	addANDClause( strWhereClause, attrValues.m_CountyCode );
	addANDClause( strWhereClause, attrValues.m_Township );
	addANDClause( strWhereClause, attrValues.m_TownshipDir );
	addANDClause( strWhereClause, attrValues.m_Range );
	addANDClause( strWhereClause, attrValues.m_RangeDir );

	switch (nQuarterLevel)
	{ 
	case 4:	// QQQQ
		addANDClause ( strWhereClause, attrValues.m_SectionNum );
		addANDClause ( strWhereClause, attrValues.m_QQQQ );
		break;

	case 3:	// QQQ
		addANDClause ( strWhereClause, attrValues.m_QQQ );

	case 2:	// QQ
		addANDClause ( strWhereClause, attrValues.m_QQ );

	case 1:	// Quarter
		addANDClause ( strWhereClause, attrValues.m_Quarter );
		addANDClause ( strWhereClause, attrValues.m_SectionNum );
	}

	return strWhereClause;
}
//-------------------------------------------------------------------------------------------------
void GridDrawer::addANDClause( std::string& strWhereClause, AttributeSet attributeSet)
{
	if ( !attributeSet.m_strAttrValue.empty() )
	{
		if ( !strWhereClause.empty() )
		{
			strWhereClause += " AND ";
		}
		if  ( attributeSet.m_fieldType == esriFieldTypeString )
		{
			strWhereClause += attributeSet.m_strAttrName + " = '" + attributeSet.m_strAttrValue + "'";
		}
		else
		{
			strWhereClause += attributeSet.m_strAttrName + " = " + attributeSet.m_strAttrValue;
		}
	}
}
//-------------------------------------------------------------------------------------------------
bool GridDrawer::selectFeature(ELayer eLayer, AttributeValues& attrValues)
{

	string strLayerName = m_mapLayerCodeToName[eLayer];

	IFeatureLayerPtr ipFeatureLayer = this->getLayer(strLayerName);

	// Check to see if layer was found
	if ( ipFeatureLayer == NULL )
	{
		UCLIDException ue("ELI13469", "Unable to get layer");
		ue.addDebugInfo("Layer", strLayerName );
		throw ue;
	}

	IFeatureSelectionPtr ipFeatureSel(ipFeatureLayer);
	ASSERT_RESOURCE_ALLOCATION("ELI12669", ipFeatureSel != NULL);

	IQueryFilterPtr ipQFilter(CLSID_QueryFilter);
	ASSERT_RESOURCE_ALLOCATION("ELI12668", ipQFilter != NULL);

	string strWhereClause = getWhereClause(attrValues, (int)eLayer - (int)kSection);
	if (strWhereClause.empty())
	{
		// nothing to be selected
		return false;
	}

	ipQFilter->WhereClause = _bstr_t(strWhereClause.c_str());

	// Clear any existing selections
	IMapPtr ipMap = m_ipEditor->Map;
	ipMap->ClearSelection();
	
	ipFeatureSel->SelectFeatures(ipQFilter, esriSelectionResultNew, VARIANT_FALSE);
	
	if (ipMap->SelectionCount > 0 )
	{
		return true;
	}
	return false;
}
//-------------------------------------------------------------------------------------------------
void GridDrawer::displayMultipleFeaturesFoundDlg(int nNumberFound, AttributeValues& attrValues, 
								 int nQuarterLevel)
{
	string strMsg;
	strMsg = "There were " + asString(nNumberFound) + " features found with the following attriubtes\n\n";
	strMsg += "\t" + attrValues.m_CountyCode.m_strAttrName + " = " + attrValues.m_CountyCode.m_strAttrValue + "\n";
	strMsg += "\t" + attrValues.m_Township.m_strAttrName + " = " + attrValues.m_Township.m_strAttrValue + "\n";
	strMsg += "\t" + attrValues.m_TownshipDir.m_strAttrName + " = '" + attrValues.m_TownshipDir.m_strAttrValue + "'\n";
	strMsg += "\t" + attrValues.m_Range.m_strAttrName + " = " + attrValues.m_Range.m_strAttrValue + "\n";
	strMsg += "\t" + attrValues.m_RangeDir.m_strAttrName + " = '" + attrValues.m_RangeDir.m_strAttrValue + "'\n";
	strMsg += "\t" + attrValues.m_SectionNum.m_strAttrName + " = '" + attrValues.m_SectionNum.m_strAttrValue + "'\n";

	switch ( nQuarterLevel )
	{

	case 4:
		strMsg += "\t" + attrValues.m_QQQQ.m_strAttrName + " = " + attrValues.m_QQQQ.m_strAttrValue + "\n";
		break;
	
	case 3:
		strMsg += "\t" + attrValues.m_QQQ.m_strAttrName + " = " + attrValues.m_QQQ.m_strAttrValue + "\n";
	
	case 2:
		strMsg += "\t" + attrValues.m_QQ.m_strAttrName + " = " + attrValues.m_QQ.m_strAttrValue + "\n";
	
	case 1:
		strMsg += "\t" + attrValues.m_Quarter.m_strAttrName + " = " + attrValues.m_Quarter.m_strAttrValue + "\n";
	}
	AfxMessageBox ( strMsg.c_str(), MB_OK );

}