#if !defined(AFX_CURVEDJINNI_H__D3DAB3F2_2A9F_425E_A1F1_CE3D954323C9__INCLUDED_)
#define AFX_CURVEDJINNI_H__D3DAB3F2_2A9F_425E_A1F1_CE3D954323C9__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	CurveDjinni.h
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan (Aug 2001 to present)
//			John Hurd (till July 2001)
//
//==================================================================================================

#include "..\CurveCalculationEngine\Code\ECurveParameter.h"

#include <map>
#include <vector>

typedef std::vector<ECurveParameterType> CurveMatrixEntry;
typedef std::vector<CurveMatrixEntry> CurveMatrix;
typedef std::map<CurveMatrixEntry,bool> CurveMatrixEntryMap;
typedef CurveMatrixEntryMap::const_iterator CurveMatrixEntryMapConstIter;

class CurveDjinni  
{
public:
	CurveDjinni();
	virtual ~CurveDjinni();

	CurveMatrixEntry createCurveMatrixEntry(ECurveParameterType eParameter1,
											ECurveParameterType eParameter2,
											ECurveParameterType eParameter3);
	bool doesParameterExistInCurveMatrix(ECurveParameterType eCurveParameter,const CurveMatrix& rMatrix);
	CurveMatrix filterCurveMatrix(ECurveParameterType eCurveParameter,const CurveMatrix& rMatrix) const;
	const CurveMatrix& getCurveMatrix(void) const {return m_vecCurveMatrix;}
	bool isToggleCurveDeltaAngleEnabled(const CurveMatrixEntry& entry);
	bool isToggleCurveDirectionEnabled(const CurveMatrixEntry& entry);

protected:
	enum
	{
		kCurveParameterCnt = 3
	};

	CurveMatrix m_vecCurveMatrix;
	CurveMatrixEntryMap m_mapEnableToggleCurveDeltaAngle;	// whether or not the curve delta angle may be toggled for a particualar CurveParameterEntry
	CurveMatrixEntryMap m_mapEnableToggleCurveDirection;	// whether or not the curve direction may be toggled for a particualar CurveParameterEntry

	CurveMatrixEntry& addCurveMatrixEntry(ECurveParameterType eParameter1,
										  ECurveParameterType eParameter2,
										  ECurveParameterType eParameter3);
	void addToToggleMaps(CurveMatrixEntry& rEntry,bool bEnableToggleCurveDeltaAngle,bool bEnableToggleCurveDirection);
	void createCurveMatrix(void);
	CurveMatrixEntryMapConstIter findEntryInCurveMatrixEntryMap(const CurveMatrixEntry& rEntryIn,const CurveMatrixEntryMap& rMapIn);

};

#endif // !defined(AFX_CURVEDJINNI_H__D3DAB3F2_2A9F_425E_A1F1_CE3D954323C9__INCLUDED_)
