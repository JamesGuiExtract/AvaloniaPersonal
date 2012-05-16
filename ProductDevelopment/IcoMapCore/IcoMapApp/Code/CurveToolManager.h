#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	CurveToolManager.h
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	John Hurd
//
//==================================================================================================

#include <Singleton.h>
#include <ECurveToolID.h>
#include <ECurveParameter.h>

#include <map>
#include <vector>

class CurveTool;
typedef std::vector<ECurveParameterType> CurveParameters;

class CurveToolManager : public Singleton<CurveToolManager>
{
	ALLOW_SINGLETON_ACCESS(CurveToolManager);
public:
	virtual ~CurveToolManager();

	CurveTool* getCurveTool(ECurveToolID eCurveToolID) const;
	void initializeCurrentCurveTool(CurveTool* pTool) const;
	void restoreDefaultStates(void);
	void restoreDefaultState(ECurveToolID eCurveToolID);
	void restoreState(void);
	void saveState(void);
	void updateCurrentCurveTool(const CurveParameters& curveParameters);

	// returns vector of curve tools whose parameters have 
	// been reset by CurveWizardResetDlg and CurveWizardSaveAsDlg
	std::vector<ECurveToolID> getUpdatedCurveToolIDs() {return m_vecUpdatedCurveToolIDs;}
	// add the tool id to m_vecResetCurveToolIDs
	void addUpdatedCurveToolIDS(ECurveToolID eCurveToolID) {m_vecUpdatedCurveToolIDs.push_back(eCurveToolID);}
	// clean the content of m_vecResetCurveToolIDs
	void clearUpdatedCurveToolIDs() {m_vecUpdatedCurveToolIDs.clear();}

private:
	typedef std::vector<CurveTool*> CurveTools;
	CurveTools m_vecCurveTools;					// collection of curve tools
	std::vector<ECurveToolID> m_vecUpdatedCurveToolIDs;

	typedef std::map<ECurveToolID,CurveParameters> DefaultCurveToolParameters;
	DefaultCurveToolParameters m_mapDefaultCurveParameters;		// collection of default curve tool parameters

	CurveToolManager();
	// following required for all singletons in general
	CurveToolManager(const CurveToolManager& toCopy);
	CurveToolManager& operator = (const CurveToolManager& toAssign);

	void createDefaultCurveToolState(void);

};
