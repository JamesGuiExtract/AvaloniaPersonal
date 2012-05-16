//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	CurveToolManager.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	John Hurd
//
//==================================================================================================

#include "stdafx.h"
#include "CurveToolManager.h"

#include "CurrentCurveTool.h"
#include "CurveTool.h"
#include <IcoMapOptions.h>
#include <UCLIDException.h>

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

using namespace std;


CurveToolManager::CurveToolManager()
{
	ASSERT (kCurve1 == 0);
	for (int iCurveToolID = kCurve1; iCurveToolID < kCurveToolCnt; iCurveToolID++)
	{
		m_vecCurveTools.push_back(new CurveTool(static_cast<ECurveToolID>(iCurveToolID)));
	}
	createDefaultCurveToolState();
	restoreState();

	m_vecUpdatedCurveToolIDs.clear();
}

CurveToolManager::CurveToolManager(const CurveToolManager& toCopy)
{
	throw UCLIDException("ELI02203", "Internal error: copy constructor of singleton class called!");
}

CurveToolManager& CurveToolManager::operator = (const CurveToolManager& toAssign)
{
	throw UCLIDException("ELI02204", "Internal error: assignment operator of singleton class called!");
}

CurveToolManager::~CurveToolManager()
{
	saveState();
	for (CurveTools::iterator it = m_vecCurveTools.begin(); it != m_vecCurveTools.end(); it++)
	{
		delete (*it);
	}
}

void CurveToolManager::createDefaultCurveToolState(void)
{
	m_mapDefaultCurveParameters.clear();
	
	try
	{
		for (int iCurveToolID = kCurve1; iCurveToolID < kCurveToolCnt; iCurveToolID++)
		{	
			m_mapDefaultCurveParameters[static_cast<ECurveToolID>(iCurveToolID)] 
				= IcoMapOptions::sGetInstance().getCurveToolDefaultParameters(static_cast<ECurveToolID>(iCurveToolID));
		}
	}
	catch (UCLIDException &uclidException)
	{
		uclidException.addHistoryRecord("ELI01198", "Failed to get default curve tool state from persistence manager.");
		throw uclidException;
	}
}

CurveTool* CurveToolManager::getCurveTool(ECurveToolID eCurveToolID) const
{
	CurveTool* pTool = NULL;
	try
	{
		pTool = m_vecCurveTools[eCurveToolID];
	}
	catch(...)
	{
		UCLIDException uclidException("ELI01105","Curve Tool ID out of range");
		uclidException.addDebugInfo("eCurveToolID", eCurveToolID);
		throw uclidException;
	}

	return pTool;
}

void CurveToolManager::initializeCurrentCurveTool(CurveTool* pTool) const
{
	CurrentCurveTool& currentTool = CurrentCurveTool::sGetInstance();
	currentTool.reset();

	int iCurveParameterID = 0;
	list<int> listParameterID = pTool->getSortedCurveParameterIDList();
	list<int>::const_iterator it;
	for (it = listParameterID.begin(); it != listParameterID.end(); it++)
	{
		iCurveParameterID = *it;
		currentTool.setCurveParameter(iCurveParameterID,pTool->getCurveParameter(iCurveParameterID));
	}
	currentTool.updateStateOfCurveToggles();

	// generate tooltip/decription for current tool
	currentTool.generateToolTip();
}

void CurveToolManager::restoreDefaultStates(void)
{
	for (int iCurveToolID = kCurve1; iCurveToolID < kCurveToolCnt; iCurveToolID++)
	{
		restoreDefaultState(static_cast<ECurveToolID>(iCurveToolID));
	}
}

void CurveToolManager::restoreDefaultState(ECurveToolID iCurveToolID)
{
	DefaultCurveToolParameters::const_iterator itDefault;
	CurveParameters::const_iterator itParameter;
	try
	{
		CurveTool* pTool = m_vecCurveTools[iCurveToolID];
		DefaultCurveToolParameters::const_iterator itDefault = m_mapDefaultCurveParameters.find(iCurveToolID);
		if (itDefault != m_mapDefaultCurveParameters.end())
		{
			pTool->reset();
			CurveParameters parameters = (*itDefault).second;
			int iCurveParameterID = 1;
			for (itParameter=parameters.begin();itParameter!=parameters.end();itParameter++)
			{
				pTool->setCurveParameter(iCurveParameterID,*itParameter);
				++iCurveParameterID;
			}
		}
		else
		{
			UCLIDException uclidException("ELI01108","Internal error.");
			uclidException.addDebugInfo("m_mapDefaultCurveParameters", "DefaultCurveParameters map is out of synch with the CurveTools vector");
			throw uclidException;
		}
		
		// set tooltip/description for current curve tool
		pTool->generateToolTip();
	}
	catch(...)
	{
		UCLIDException uclidException("ELI01110","Internal error.");
		uclidException.addDebugInfo("curveToolID", iCurveToolID);
		throw uclidException;
	}
}

void CurveToolManager::restoreState(void)
{
	try
	{
		int iCurveToolID = kCurve1;
		for (CurveTools::iterator it = m_vecCurveTools.begin(); it != m_vecCurveTools.end(); it++)
		{
			if (!(*it)->restoreState())
			{
				restoreDefaultState(static_cast<ECurveToolID>(iCurveToolID));
			}
			++iCurveToolID;
		}
	}
	catch(...)
	{
		restoreDefaultStates();
		UCLIDException uclidException("ELI01109","Internal error.");
		throw uclidException;
	}
}

void CurveToolManager::saveState(void)
{
	for (CurveTools::iterator it = m_vecCurveTools.begin(); it != m_vecCurveTools.end(); it++)
	{
		(*it)->saveState();
	}
}

void CurveToolManager::updateCurrentCurveTool(const CurveParameters& curveParameters)
{
	CurrentCurveTool& currentTool = CurrentCurveTool::sGetInstance();
	currentTool.reset();

	int iCurveParameterID = 1;
	for (CurveParameters::const_iterator it = curveParameters.begin(); it != curveParameters.end(); it++)
	{
		currentTool.setCurveParameter(iCurveParameterID,*it);
		++iCurveParameterID;
	}
	currentTool.updateStateOfCurveToggles();
	
	// regenerate tooltip/decription for current tool
	currentTool.generateToolTip();
}
