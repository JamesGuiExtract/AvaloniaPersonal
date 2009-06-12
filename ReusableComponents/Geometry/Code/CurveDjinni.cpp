//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	CurveDjinni.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan (Aug 2001 to present)
//			John Hurd (till July 2001)
//
//==================================================================================================

#include "stdafx.h"    // this file is expected to be in the project directory that uses this class
#include "CurveDjinni.h"

#include <algorithm>

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

using namespace std;

CurveDjinni::CurveDjinni()
{
	createCurveMatrix();
}

CurveDjinni::~CurveDjinni()
{
}

CurveMatrixEntry& CurveDjinni::addCurveMatrixEntry(ECurveParameterType eParameter1,
												   ECurveParameterType eParameter2,
												   ECurveParameterType eParameter3)
{
	CurveMatrixEntry entry = createCurveMatrixEntry(eParameter1,eParameter2,eParameter3);

	m_vecCurveMatrix.push_back(entry);

	return m_vecCurveMatrix.back();
}

void CurveDjinni::addToToggleMaps(CurveMatrixEntry& rEntry,bool bEnableToggleCurveDirection,bool bEnableToggleCurveDeltaAngle)
{
	m_mapEnableToggleCurveDirection[rEntry] = bEnableToggleCurveDirection;
	m_mapEnableToggleCurveDeltaAngle[rEntry] = bEnableToggleCurveDeltaAngle;
}

void CurveDjinni::createCurveMatrix(void)
{
	CurveMatrixEntry entry;
	CurveMatrixEntry& rEntry = entry;

	rEntry = addCurveMatrixEntry(kArcTangentInBearing,	kArcTangentOutBearing,	kArcRadius);
	addToToggleMaps(rEntry,true,false);		//need toggle direction
	rEntry = addCurveMatrixEntry(kArcTangentInBearing,	kArcTangentOutBearing,	kArcLength);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcTangentInBearing,	kArcTangentOutBearing,	kArcChordLength);
	addToToggleMaps(rEntry,true,false);		//need toggle direction		

	rEntry = addCurveMatrixEntry(kArcTangentInBearing,	kArcRadialOutBearing,	kArcRadius);
	addToToggleMaps(rEntry,true,false);		//need toggle direction
	rEntry = addCurveMatrixEntry(kArcTangentInBearing,	kArcRadialOutBearing,	kArcLength);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcTangentInBearing,	kArcRadialOutBearing,	kArcChordLength);
	addToToggleMaps(rEntry,true,false);		//need toggle direction

	rEntry = addCurveMatrixEntry(kArcTangentInBearing,	kArcChordBearing,		kArcRadius);
	addToToggleMaps(rEntry,false,false);
	rEntry = addCurveMatrixEntry(kArcTangentInBearing,	kArcChordBearing,		kArcLength);
	addToToggleMaps(rEntry,false,false);
	rEntry = addCurveMatrixEntry(kArcTangentInBearing,	kArcChordBearing,		kArcChordLength);
	addToToggleMaps(rEntry,false,false);

	rEntry = addCurveMatrixEntry(kArcTangentInBearing,	kArcDelta,				kArcRadius);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcTangentInBearing,	kArcDelta,				kArcLength);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcTangentInBearing,	kArcDelta,				kArcChordLength);
	addToToggleMaps(rEntry,true,false);

	rEntry = addCurveMatrixEntry(kArcTangentInBearing,	kArcRadius,				kArcLength);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcTangentInBearing,	kArcRadius,				kArcChordLength);
	addToToggleMaps(rEntry,true,true);
//	rEntry = addCurveMatrixEntry(kArcTangentInBearing,	kArcLength,				kArcChordLength);
//	addToToggleMaps(rEntry,true,false);

	rEntry = addCurveMatrixEntry(kArcTangentOutBearing,	kArcRadialInBearing,	kArcRadius);
	addToToggleMaps(rEntry,true,false);		//need toggle direction
	rEntry = addCurveMatrixEntry(kArcTangentOutBearing,	kArcRadialInBearing,	kArcLength);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcTangentOutBearing,	kArcRadialInBearing,	kArcChordLength);
	addToToggleMaps(rEntry,true,false);		//need toggle direction

	rEntry = addCurveMatrixEntry(kArcTangentOutBearing,	kArcChordBearing,		kArcRadius);
	addToToggleMaps(rEntry,false,false);
	rEntry = addCurveMatrixEntry(kArcTangentOutBearing,	kArcChordBearing,		kArcLength);
	addToToggleMaps(rEntry,false,false);
	rEntry = addCurveMatrixEntry(kArcTangentOutBearing,	kArcChordBearing,		kArcChordLength);
	addToToggleMaps(rEntry,false,false);

	rEntry = addCurveMatrixEntry(kArcTangentOutBearing,	kArcDelta,				kArcRadius);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcTangentOutBearing,	kArcDelta,				kArcLength);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcTangentOutBearing,	kArcDelta,				kArcChordLength);
	addToToggleMaps(rEntry,true,false);

	rEntry = addCurveMatrixEntry(kArcTangentOutBearing,	kArcRadius,				kArcLength);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcTangentOutBearing,	kArcRadius,				kArcChordLength);
	addToToggleMaps(rEntry,true,true);
//	rEntry = addCurveMatrixEntry(kArcTangentOutBearing,	kArcLength,				kArcChordLength);
//	addToToggleMaps(rEntry,true,false);

	rEntry = addCurveMatrixEntry(kArcRadialInBearing,	kArcRadialOutBearing,	kArcRadius);
	addToToggleMaps(rEntry,true,false);		//need toggle direction
	rEntry = addCurveMatrixEntry(kArcRadialInBearing,	kArcRadialOutBearing,	kArcLength);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcRadialInBearing,	kArcRadialOutBearing,	kArcChordLength);
	addToToggleMaps(rEntry,true,false);		//need toggle direction

	rEntry = addCurveMatrixEntry(kArcRadialInBearing,	kArcChordBearing,		kArcRadius);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcRadialInBearing,	kArcChordBearing,		kArcLength);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcRadialInBearing,	kArcChordBearing,		kArcChordLength);
	addToToggleMaps(rEntry,true,false);

	rEntry = addCurveMatrixEntry(kArcRadialInBearing,	kArcDelta,				kArcRadius);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcRadialInBearing,	kArcDelta,				kArcLength);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcRadialInBearing,	kArcDelta,				kArcChordLength);
	addToToggleMaps(rEntry,true,false);

	rEntry = addCurveMatrixEntry(kArcRadialInBearing,	kArcRadius,				kArcLength);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcRadialInBearing,	kArcRadius,				kArcChordLength);
	addToToggleMaps(rEntry,true,true);
//	rEntry = addCurveMatrixEntry(kArcRadialInBearing,	kArcLength,				kArcChordLength);
//	addToToggleMaps(rEntry,true,false);

	rEntry = addCurveMatrixEntry(kArcRadialOutBearing,	kArcChordBearing,		kArcRadius);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcRadialOutBearing,	kArcChordBearing,		kArcLength);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcRadialOutBearing,	kArcChordBearing,		kArcChordLength);
	addToToggleMaps(rEntry,true,false);

	rEntry = addCurveMatrixEntry(kArcRadialOutBearing,	kArcDelta,				kArcRadius);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcRadialOutBearing,	kArcDelta,				kArcLength);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcRadialOutBearing,	kArcDelta,				kArcChordLength);
	addToToggleMaps(rEntry,true,false);

	rEntry = addCurveMatrixEntry(kArcRadialOutBearing,	kArcRadius,				kArcLength);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcRadialOutBearing,	kArcRadius,				kArcChordLength);
	addToToggleMaps(rEntry,true,true);
//	rEntry = addCurveMatrixEntry(kArcRadialOutBearing,	kArcLength,				kArcChordLength);
//	addToToggleMaps(rEntry,true,false);

	rEntry = addCurveMatrixEntry(kArcChordBearing,		kArcDelta,				kArcRadius);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcChordBearing,		kArcDelta,				kArcLength);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcChordBearing,		kArcDelta,				kArcChordLength);
	addToToggleMaps(rEntry,true,false);

	rEntry = addCurveMatrixEntry(kArcChordBearing,		kArcRadius,				kArcLength);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcChordBearing,		kArcRadius,				kArcChordLength);
	addToToggleMaps(rEntry,true,true);
//	rEntry = addCurveMatrixEntry(kArcChordBearing,		kArcLength,				kArcChordLength);
//	addToToggleMaps(rEntry,true,false);

	rEntry = addCurveMatrixEntry(kArcTangentInBearing,	kArcTangentOutBearing,	kArcDegreeOfCurveArcDef);
	addToToggleMaps(rEntry,true,false);		//need toggle direction
	rEntry = addCurveMatrixEntry(kArcTangentInBearing,	kArcRadialOutBearing,	kArcDegreeOfCurveArcDef);
	addToToggleMaps(rEntry,true,false);		//need toggle direction
	rEntry = addCurveMatrixEntry(kArcTangentInBearing,	kArcChordBearing,		kArcDegreeOfCurveArcDef);
	addToToggleMaps(rEntry,false,false);
	rEntry = addCurveMatrixEntry(kArcTangentInBearing,	kArcDelta,				kArcDegreeOfCurveArcDef);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcTangentInBearing,	kArcLength,				kArcDegreeOfCurveArcDef);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcTangentInBearing,	kArcChordLength,		kArcDegreeOfCurveArcDef);
	addToToggleMaps(rEntry,true,true);

	rEntry = addCurveMatrixEntry(kArcTangentOutBearing,	kArcRadialInBearing,	kArcDegreeOfCurveArcDef);
	addToToggleMaps(rEntry,true,false);		//need toggle direction
	rEntry = addCurveMatrixEntry(kArcTangentOutBearing,	kArcChordBearing,		kArcDegreeOfCurveArcDef);
	addToToggleMaps(rEntry,false,false);
	rEntry = addCurveMatrixEntry(kArcTangentOutBearing,	kArcDelta,				kArcDegreeOfCurveArcDef);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcTangentOutBearing,	kArcLength,				kArcDegreeOfCurveArcDef);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcTangentOutBearing,	kArcChordLength,		kArcDegreeOfCurveArcDef);
	addToToggleMaps(rEntry,true,true);

	rEntry = addCurveMatrixEntry(kArcRadialInBearing,	kArcRadialOutBearing,	kArcDegreeOfCurveArcDef);
	addToToggleMaps(rEntry,true,false);		//need toggle direction
	rEntry = addCurveMatrixEntry(kArcRadialInBearing,	kArcChordBearing,		kArcDegreeOfCurveArcDef);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcRadialInBearing,	kArcDelta,				kArcDegreeOfCurveArcDef);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcRadialInBearing,	kArcLength,				kArcDegreeOfCurveArcDef);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcRadialInBearing,	kArcChordLength,		kArcDegreeOfCurveArcDef);
	addToToggleMaps(rEntry,true,true);

	rEntry = addCurveMatrixEntry(kArcRadialOutBearing,	kArcChordBearing,		kArcDegreeOfCurveArcDef);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcRadialOutBearing,	kArcDelta,				kArcDegreeOfCurveArcDef);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcRadialOutBearing,	kArcLength,				kArcDegreeOfCurveArcDef);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcRadialOutBearing,	kArcChordLength,		kArcDegreeOfCurveArcDef);
	addToToggleMaps(rEntry,true,true);

	rEntry = addCurveMatrixEntry(kArcChordBearing,		kArcDelta,				kArcDegreeOfCurveArcDef);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcChordBearing,		kArcLength,				kArcDegreeOfCurveArcDef);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcChordBearing,		kArcChordLength,		kArcDegreeOfCurveArcDef);
	addToToggleMaps(rEntry,true,true);

	rEntry = addCurveMatrixEntry(kArcTangentInBearing,	kArcTangentOutBearing,	kArcDegreeOfCurveChordDef);
	addToToggleMaps(rEntry,true,false);		//need toggle direction
	rEntry = addCurveMatrixEntry(kArcTangentInBearing,	kArcRadialOutBearing,	kArcDegreeOfCurveChordDef);
	addToToggleMaps(rEntry,true,false);		//need toggle direction
	rEntry = addCurveMatrixEntry(kArcTangentInBearing,	kArcChordBearing,		kArcDegreeOfCurveChordDef);
	addToToggleMaps(rEntry,false,false);
	rEntry = addCurveMatrixEntry(kArcTangentInBearing,	kArcDelta,				kArcDegreeOfCurveChordDef);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcTangentInBearing,	kArcLength,				kArcDegreeOfCurveChordDef);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcTangentInBearing,	kArcChordLength,		kArcDegreeOfCurveChordDef);
	addToToggleMaps(rEntry,true,true);

	rEntry = addCurveMatrixEntry(kArcTangentOutBearing,	kArcRadialInBearing,	kArcDegreeOfCurveChordDef);
	addToToggleMaps(rEntry,true,false);		//need toggle direction
	rEntry = addCurveMatrixEntry(kArcTangentOutBearing,	kArcChordBearing,		kArcDegreeOfCurveChordDef);
	addToToggleMaps(rEntry,false,false);
	rEntry = addCurveMatrixEntry(kArcTangentOutBearing,	kArcDelta,				kArcDegreeOfCurveChordDef);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcTangentOutBearing,	kArcLength,				kArcDegreeOfCurveChordDef);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcTangentOutBearing,	kArcChordLength,		kArcDegreeOfCurveChordDef);
	addToToggleMaps(rEntry,true,true);

	rEntry = addCurveMatrixEntry(kArcRadialInBearing,	kArcRadialOutBearing,	kArcDegreeOfCurveChordDef);
	addToToggleMaps(rEntry,true,false);		//need toggle direction
	rEntry = addCurveMatrixEntry(kArcRadialInBearing,	kArcChordBearing,		kArcDegreeOfCurveChordDef);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcRadialInBearing,	kArcDelta,				kArcDegreeOfCurveChordDef);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcRadialInBearing,	kArcLength,				kArcDegreeOfCurveChordDef);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcRadialInBearing,	kArcChordLength,		kArcDegreeOfCurveChordDef);
	addToToggleMaps(rEntry,true,true);

	rEntry = addCurveMatrixEntry(kArcRadialOutBearing,	kArcChordBearing,		kArcDegreeOfCurveChordDef);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcRadialOutBearing,	kArcDelta,				kArcDegreeOfCurveChordDef);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcRadialOutBearing,	kArcLength,				kArcDegreeOfCurveChordDef);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcRadialOutBearing,	kArcChordLength,		kArcDegreeOfCurveChordDef);
	addToToggleMaps(rEntry,true,true);

	rEntry = addCurveMatrixEntry(kArcChordBearing,		kArcDelta,				kArcDegreeOfCurveChordDef);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcChordBearing,		kArcLength,				kArcDegreeOfCurveChordDef);
	addToToggleMaps(rEntry,true,false);
	rEntry = addCurveMatrixEntry(kArcChordBearing,		kArcChordLength,		kArcDegreeOfCurveChordDef);
	addToToggleMaps(rEntry,true,true);
}

CurveMatrixEntry CurveDjinni::createCurveMatrixEntry(ECurveParameterType eParameter1,
													 ECurveParameterType eParameter2,
													 ECurveParameterType eParameter3)
{
	CurveMatrixEntry entry;

	entry.push_back(eParameter1);
	entry.push_back(eParameter2);
	entry.push_back(eParameter3);

	return entry;
}

bool CurveDjinni::doesParameterExistInCurveMatrix(ECurveParameterType eCurveParameter,const CurveMatrix& rMatrix)
{
	bool bExtant = false;

	CurveMatrixEntry entry;
	CurveMatrixEntry::const_iterator itEntry;
	for (CurveMatrix::const_iterator itMatrix = rMatrix.begin(); itMatrix != rMatrix.end(); itMatrix++)
	{
		entry = *itMatrix;
		itEntry = find(entry.begin(),entry.end(),eCurveParameter);
		if (itEntry != entry.end())
		{
			bExtant = true;
			break;
		}
	}

	return bExtant;
}

CurveMatrix CurveDjinni::filterCurveMatrix(ECurveParameterType eCurveParameter,const CurveMatrix& rMatrix) const
{
	CurveMatrix filteredMatrix;

	CurveMatrixEntry entry;
	CurveMatrixEntry::const_iterator itEntry;
	for (CurveMatrix::const_iterator itMatrix = rMatrix.begin(); itMatrix != rMatrix.end(); itMatrix++)
	{
		entry = *itMatrix;
		itEntry = find(entry.begin(),entry.end(),eCurveParameter);
		if (itEntry != entry.end())
		{
			filteredMatrix.push_back(entry);
		}
	}

	return filteredMatrix;
}

CurveMatrixEntryMapConstIter CurveDjinni::findEntryInCurveMatrixEntryMap(const CurveMatrixEntry& rTargetEntry,const CurveMatrixEntryMap& rCurveMatrixEntryMap)
{
	CurveMatrixEntryMap::const_iterator itPotentialMatch;

	//	Inspect each entry in the CurveMatrixEntryMap to see if it matches the DesiredCurveMatrixEntry.
	//	A match occurs if each of the 3 CurveParameters specified by the DesiredCurveMatrixEntry
	//	occurs within the current entry's CurveParameter collection.  The CurveParameters may
	//	occur in any order.
	
	for (itPotentialMatch = rCurveMatrixEntryMap.begin();itPotentialMatch != rCurveMatrixEntryMap.end();itPotentialMatch++)
	{
		// Reset the match counter for each entry in the map.
		int iMatchCnt = 0;							
		const CurveMatrixEntry& rPotentialMatchEntry = (*itPotentialMatch).first;
		CurveMatrixEntry::const_iterator itTargetEntry;
		for (itTargetEntry = rTargetEntry.begin(); itTargetEntry != rTargetEntry.end(); itTargetEntry++)
		{
			ECurveParameterType eTargetCurveParameter = *itTargetEntry;
			CurveMatrixEntry::const_iterator itEntryPotentialMatch;
			for (itEntryPotentialMatch = rPotentialMatchEntry.begin(); itEntryPotentialMatch != rPotentialMatchEntry.end(); 
					itEntryPotentialMatch++)
			{
				ECurveParameterType eCurveParameterPotentialMatch = *itEntryPotentialMatch;
				if (eCurveParameterPotentialMatch == eTargetCurveParameter)
				{
					// Another CurveParameter matches the target.
					++iMatchCnt;					
					if (iMatchCnt == kCurveParameterCnt)
					{
						// A sufficient number of CurveParameters match the target.
						return itPotentialMatch;	
					}
				}
			}
		}
	}

	return rCurveMatrixEntryMap.end();
}

bool CurveDjinni::isToggleCurveDeltaAngleEnabled(const CurveMatrixEntry& rEntryIn) 
{
	bool bEnabled = false;
	
	CurveMatrixEntryMap::const_iterator it = findEntryInCurveMatrixEntryMap(rEntryIn,m_mapEnableToggleCurveDeltaAngle);
	if (it != m_mapEnableToggleCurveDeltaAngle.end())
	{
		bEnabled = (*it).second;
	}

	return bEnabled;
}

bool CurveDjinni::isToggleCurveDirectionEnabled(const CurveMatrixEntry& rEntryIn) 
{
	bool bEnabled = false;

	CurveMatrixEntryMap::const_iterator it = findEntryInCurveMatrixEntryMap(rEntryIn,m_mapEnableToggleCurveDirection);
	if (it != m_mapEnableToggleCurveDirection.end())
	{
		bEnabled = (*it).second;
	}

	return bEnabled;
}
