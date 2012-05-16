//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	DrawingToolFSM.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	John Hurd
//			Duan Wang
//
//==================================================================================================

#include "stdafx.h"
#include "DrawingToolFSM.h"

#include "CommandPrompt.h"
#include "CurrentCurveTool.h"
#include "DrawingToolState.h"
#include "DrawingToolStateCurve.h"
#include "DrawingToolStateCurveParam1.h"
#include "DrawingToolStateCurveParam2.h"
#include "DrawingToolStateCurveParam3.h"
#include "DrawingToolStateCurvePoint.h"
#include "DrawingToolStateLine.h"
#include "DrawingToolStateLineBearing.h"
#include "DrawingToolStateLineDistance.h"
#include "DrawingToolStateLinePoint.h"
#include "DrawingToolStateDeflectionAngle.h"
#include "DrawingToolStateDeflectionDistance.h"
#include "DrawingToolStateView.h"
#include "LineCalculationEngine.h"
#include "IIcoMapUI.h"

#include <IcoMapOptions.h>
#include <Angle.hpp>
#include <UCLIDException.h>
#include <cpputil.h>
#include <TPPoint.h>

#include <algorithm>

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

using namespace std;

//--------------------------------------------------------------------------------------------------
DrawingToolFSM::DrawingToolFSM(IInputManager *pInputManager, 
							   CommandPrompt* pCommandPrompt,
							   DynamicInputGridWnd* pDIGWnd) :
	m_ipAttributeManager(NULL),
	m_ipDisplayAdapter(NULL),
	m_pCommandPrompt(pCommandPrompt),
	m_pInputManager(pInputManager),
	m_pDIGWnd(pDIGWnd),
	m_pState(DrawingToolStateLine::sGetInstance()),	// Now always start with line state
	m_nNumOfSegmentsAdded(0),
	m_pIcoMapUI(NULL),
	m_eAngleToggleInputType(kToggleAngleLeft),
	m_bLeft(true),
	m_bBig(false),
	m_bInputTypeIsFromDIG(false),
	m_ipAngleInputValidator(__uuidof(UCLID_LANDRECORDSIVLib::AngleInputValidator)),
	m_ipDirectionInputValidator(__uuidof(UCLID_LANDRECORDSIVLib::DirectionInputValidator)),
	m_ipDistanceInputValidator(__uuidof(UCLID_LANDRECORDSIVLib::DistanceInputValidator)),
	m_ipPointInputValidator(__uuidof(UCLID_LANDRECORDSIVLib::CartographicPointInputValidator)),
	m_ipNothingInputValidator(__uuidof(ICOMAPAPPLib::NothingInputValidator))
{
	m_pCommandPrompt->setCommandPrompt(m_pState->getPrompt(), false); // Requires CommandPrompt subclassed window to be initialized!

	// set correct input validator for input receivers
	enableInput();

	try
	{
		// initialize the input direction type for bearing
		m_dirHelper.sSetDirectionType(static_cast<EDirection>(IcoMapOptions::sGetInstance().getInputDirection()));
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI02871")
	
	m_mapCurveParamToDefaultValue.clear();
}
//--------------------------------------------------------------------------------------------------
DrawingToolFSM::~DrawingToolFSM()
{
	try
	{
		// All DrawingToolState singletons must be deleted, otherwise leak memory
		DrawingToolStateCurve::sDelete();
		DrawingToolStateCurveParam1::sDelete();
		DrawingToolStateCurveParam2::sDelete();
		DrawingToolStateCurveParam3::sDelete();
		DrawingToolStateCurvePoint::sDelete();
		DrawingToolStateDeflectionAngle::sDelete();
		DrawingToolStateDeflectionDistance::sDelete();
		DrawingToolStateLine::sDelete();
		DrawingToolStateLineBearing::sDelete();
		DrawingToolStateLineDistance::sDelete();
		DrawingToolStateLinePoint::sDelete();
		DrawingToolStateView::sDelete();
		
		m_pState = NULL;	// synchronize pointer with object state

		m_mapCurveParamToDefaultValue.clear();

		m_ipDisplayAdapter = NULL;
		m_ipAttributeManager = NULL;
		m_pIcoMapUI = NULL;
		m_pDIGWnd = NULL;
	}
	catch (...)
	{
	}
}
//--------------------------------------------------------------------------------------------------
void DrawingToolFSM::addSourceDocument(ITextInputPtr ipTextInput)
{
	IInputEntityPtr ipInputEntity(ipTextInput->GetInputEntity());

	if (ipInputEntity)
	{
		// if the text has persistent source
		VARIANT_BOOL bFromPersistentSource = ipInputEntity->IsFromPersistentSource();
		// only add source doc if the text input is from some source doc
		if (bFromPersistentSource == VARIANT_TRUE)
		{
			_bstr_t _bstrDoc(ipInputEntity->GetPersistentSourceName());
			string strSrcDoc(_bstrDoc);
			
			addToSourceDocCollection(strSrcDoc);
		}

		// check to see if the text is from indirect source
		VARIANT_BOOL bHasSource = ipInputEntity->HasIndirectSource();
		if (bHasSource == VARIANT_TRUE)
		{
			_bstr_t _bstrIndirectSource(ipInputEntity->GetIndirectSource());
			string strIndirectSource (_bstrIndirectSource);
			addToSourceDocCollection(strIndirectSource);
		}
	}
}
//--------------------------------------------------------------------------------------------------
void DrawingToolFSM::addToSourceDocCollection(string strSourceDocToAdd)
{
	if (!strSourceDocToAdd.empty())
	{
		// Only collect the doc name if it is not yet in the collection.
		vector<string>::const_iterator it = find(m_vecSourceDocs.begin(),m_vecSourceDocs.end(),strSourceDocToAdd);
		if (it == m_vecSourceDocs.end())
		{
			m_vecSourceDocs.push_back(strSourceDocToAdd);
		}
	}
}
//--------------------------------------------------------------------------------------------------
void DrawingToolFSM::changeState(DrawingToolState* pState)
{
	m_pState = pState;
	// enable input in input manager
	enableInput();

	// update command prompt
	refreshPrompt();
}
//--------------------------------------------------------------------------------------------------
void DrawingToolFSM::deleteSketch(void)
{
	// delete sketch from DIG
	m_pDIGWnd->deleteSketch();
	
	// set command prompt to show the status
	m_pCommandPrompt->setCommandPrompt("Sketch operation", false);
	m_pCommandPrompt->setCommandInput("Sketch is deleted.");
	
	// clear up the vec of sourve docs when the sketch is deleted
	m_vecSourceDocs.clear();
				
	// !!!TODO: make sure when cancel, state should go back to either DrawingToolStateCurve
	// or DrawingToolStateLine.
	m_pState->cancel(this);
	
	// disable the internal/deflection angle tool
	m_pIcoMapUI->enableDeflectionAngleTool(false);
}
//--------------------------------------------------------------------------------------------------
void DrawingToolFSM::updateDefaultValues(void)
{
	// get the tangent-out direction of last segment 
	double dTangentInRadians = getLastSegmentTanOutAngleInRadians();
	if (Angle::isPrevInReverseMode())
	{
		dTangentInRadians += MathVars::PI;
		if (dTangentInRadians >= MathVars::PI * 2)
		{
			dTangentInRadians = dTangentInRadians - MathVars::PI * 2;
		}
	}
	// convert it to the current format
	string strTangentInText = m_dirHelper.polarAngleInRadiansToDirectionInString(dTangentInRadians);

	m_mapCurveParamToDefaultValue[kArcTangentInBearing] = strTangentInText;
	// set the next line bearing as last tangent-out bearing
	m_mapCurveParamToDefaultValue[kLineBearing] = strTangentInText;
}
//--------------------------------------------------------------------------------------------------
void DrawingToolFSM::enableInput()
{
	// current input validator that will be used to enable input receivers
	IInputValidator* ipCurrentInputValidator;

	EInputType eInputType = getCurrentExpectedInputType();
	CString zPrompt("");
	// if the input type is from DIG
	if (!m_bInputTypeIsFromDIG)
	{
		zPrompt = m_pState->getPrompt().c_str();
	}
	
	switch (eInputType)
	{
	case kAngle:
		{
			ipCurrentInputValidator = m_ipAngleInputValidator;
			if (zPrompt.IsEmpty())
			{
				zPrompt = "Enter angle value";
			}
		}
		break;
	case kBearing:
		{
			ipCurrentInputValidator = m_ipDirectionInputValidator;
			if (zPrompt.IsEmpty())
			{
				zPrompt = "Enter direction value";
			}
		}
		break;
	case kDistance:
		{
			ipCurrentInputValidator = m_ipDistanceInputValidator;
			if (zPrompt.IsEmpty())
			{
				zPrompt = "Enter distance value";
			}
		}
		break;
	case kPoint:
		{
			ipCurrentInputValidator = m_ipPointInputValidator;
			if (zPrompt.IsEmpty())
			{
				zPrompt = "Enter start point";
			}
		}
		break;
	case kToggleCurve:
	case kToggleAngle:
		{
			ipCurrentInputValidator = m_ipNothingInputValidator;
			if (zPrompt.IsEmpty())
			{
				zPrompt = "Toggle curve/angle";
			}
		}
		break;
	case kNone:
		{
			ipCurrentInputValidator = m_ipNothingInputValidator;
			if (zPrompt.IsEmpty())
			{
				zPrompt = "Do not enter anything";
			}
		}
		break;
	default:
		{
			UCLIDException uclidException("ELI02767", "Invalid input type");
			uclidException.addDebugInfo("eInputType", eInputType);
			throw uclidException;
		}
		break;
	}
	
	if (m_pIcoMapUI)
	{
		// enable input of the current type
		m_pIcoMapUI->enableInput(ipCurrentInputValidator, zPrompt);

		// only disable toggling when current input type is 
		// not obtained from DIG
/*		if (!bIsFromDIG)
		{
			// disable the toggle
			m_pIcoMapUI->enableToggle(false);
		}*/
	}
}
//--------------------------------------------------------------------------------------------------
void DrawingToolFSM::finishPart(void)
{
	m_pDIGWnd->finishCurrentPart();

	m_pCommandPrompt->setCommandPrompt("Sketch operation", false);
	m_pCommandPrompt->setCommandInput("Sketch part is finished.");

	// disable the internal/deflection angle tool
	m_pIcoMapUI->enableDeflectionAngleTool(false);

	m_pState->cancel(this);
}
//--------------------------------------------------------------------------------------------------
void DrawingToolFSM::finishSketch(void)
{
	// clear the number of segments added for current sketch
	m_nNumOfSegmentsAdded = 0;
	
	// get the feature id from the newly created sketch
	string strFeatureID = m_pDIGWnd->finishCurrentSketch();
	
	// update command line prompt
	m_pCommandPrompt->setCommandPrompt("Sketch operation", false);
	m_pCommandPrompt->setCommandInput("Sketch is finished.");
	
	updateSketchAttributes(strFeatureID);

	// clear up the vec of sourve docs when the sketch is finished
	m_vecSourceDocs.clear();
	
	// !!!TODO: make sure when cancel, state should go back to either DrawingToolStateCurve
	// or DrawingToolStateLine.
	m_pState->cancel(this);
	
	// disable the internal/deflection angle tool
	m_pIcoMapUI->enableDeflectionAngleTool(false);
}
//--------------------------------------------------------------------------------------------------
EDeflectionAngleToggleInput DrawingToolFSM::getCurrentAngleToggleInputType() const
{
	return m_eAngleToggleInputType;;
}
//--------------------------------------------------------------------------------------------------
ECurveToggleInput DrawingToolFSM::getCurrentCurveToggleInputType() const
{
	return m_eCurveToggleInputType;
}
//--------------------------------------------------------------------------------------------------
string DrawingToolFSM::getCurrentDefaultCurveParameterValue() const
{
	map<ECurveParameterType, string>::const_iterator mapIter = m_mapCurveParamToDefaultValue.find(m_pState->getCurrentCurveParameter());
	if (mapIter != m_mapCurveParamToDefaultValue.end())
	{
		return mapIter->second;
	}

	return "";
}
//--------------------------------------------------------------------------------------------------
EInputType DrawingToolFSM::getCurrentExpectedInputType() const 
{
	EInputType eInputType = m_pDIGWnd->getCurrentInputType();
	if (!m_bInputTypeIsFromDIG)
	{
		eInputType = m_pState->getExpectedInputType();
	}

	return eInputType;
}
//--------------------------------------------------------------------------------------------------
double DrawingToolFSM::getLastSegmentTanOutAngleInRadians()
{
	double dTanOutAngle = 0.0;

	if (m_ipDisplayAdapter)
	{
		bool bSucceeded = 
			m_ipDisplayAdapter->GetLastSegmentTanOutAsPolarAngleInRadians(&dTanOutAngle) == VARIANT_TRUE;
		if (!bSucceeded)
		{
			throw UCLIDException("ELI02914", "Failed to get tangent-out angle from last segment");
		}
	}

	return dTanOutAngle;
}
//--------------------------------------------------------------------------------------------------
EDistanceUnitType DrawingToolFSM::getCurrentDistanceUnitType()
{
	static EDistanceUnitType eCurrentUnit = kFeet;
	
	try
	{
		if (m_ipDisplayAdapter)
		{
			EDistanceUnitType eTempUnit = m_ipDisplayAdapter->GetCurrentDistanceUnit();

			if (eTempUnit == kUnknownUnit)
			{
				// show the message only once for each drawing that's with unsupported unit
				if (eCurrentUnit != eTempUnit)
				{
					AfxMessageBox("Distance unit for current drawing is not supported by IcoMap. Distance value will be treated in terms of Feet");

					eCurrentUnit = eTempUnit;
				}

				// treat unknown type as feet
				return kFeet;
			}

			eCurrentUnit = eTempUnit;
		}
	}
	catch (UCLIDException &uclidException)
	{
		uclidException.display();

		eCurrentUnit = kUnknownUnit;
	}
	catch (_com_error& e)
	{
		UCLIDException uclidException("ELI01404","Unable to finish sketch.");
		uclidException.createFromString("ELI03138", string(e.Description()));
		uclidException.display();

		eCurrentUnit = kUnknownUnit;
	}
	catch (...)
	{
		UCLIDException uclidException("ELI01405","Unknown exception was caught");
		uclidException.display();

		eCurrentUnit = kUnknownUnit;
	}
	
	return eCurrentUnit;
}
//--------------------------------------------------------------------------------------------------
void DrawingToolFSM::notifySketchModified(long nActualNumOfSegments)
{
	try
	{
		// notify DIG about the sketch modification so that DIG
		// could properly enable/disable its display
		if (m_pDIGWnd->notifySketchModified(nActualNumOfSegments))
		{
			return;
		}

		// Implement undo/redo here when there's no official undo/redo event 
		// if actual num of segments is not in accordance with num of segments added
		// using icomap drawing tool, there might be undo, redo or other tool operation
		// happened to current sketch, set the drawing state to the start
		if (nActualNumOfSegments != m_nNumOfSegmentsAdded)
		{
			m_nNumOfSegmentsAdded = nActualNumOfSegments;
			
			if (m_ipDisplayAdapter)
			{
				if (nActualNumOfSegments == 0)
				{
					// the sketch might either be deleted or just finished or the start of a sketch			
					if (m_ipDisplayAdapter) m_ipDisplayAdapter->Reset();
					// clear up the vec of sourve docs when the sketch is deleted
					m_vecSourceDocs.clear();
				}

				// if there's a point from last segment, proceed to the next state 
				// (i.e. if it's cure, waiting for first parameter, if it's line,
				// waiting for line bearing input)
				double dX, dY;
				bool bSuccess = m_ipDisplayAdapter->GetLastPoint(&dX, &dY) == VARIANT_TRUE;
				m_pState->changeToProperStartState(this, bSuccess);
			}
			
			bool bEnableAngleTool = true;
			try
			{
				// if there's at least one segment in the current sketch
				getLastSegmentTanOutAngleInRadians();
			}
			catch (...)
			{
				// No segment is found in the current sketch
				// disable the internal/deflection angle tool
				bEnableAngleTool = false;
			}
			
			m_pIcoMapUI->enableDeflectionAngleTool(bEnableAngleTool);

			return;
		}
		else if (nActualNumOfSegments == 0)
		{			
			if (m_ipDisplayAdapter)
			{
				// if there's a point from last segment, proceed to the next state 
				// (i.e. if it's cure, prompting for first parameter, if it's line,
				// waiting for line bearing input)
				double dX, dY;
				bool bSuccess = m_ipDisplayAdapter->GetLastPoint(&dX, &dY) == VARIANT_TRUE;
				m_pState->changeToProperStartState(this, bSuccess);
			}

			// disable the internal/deflection angle tool
			m_pIcoMapUI->enableDeflectionAngleTool(false);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI02186")
}
//--------------------------------------------------------------------------------------------------
string DrawingToolFSM::getText(EInputType eInputType, ITextInputPtr ipTextInput)
{
	string strValue("");
	
	// what's current input type
	switch (eInputType)
	{
	case kAngle:
		{
			if (ipTextInput)
			{
				strValue = ipTextInput->GetText();
				// get the angle
				// reverse mode or not, it's all counted inside the Angle class
				Angle angle(strValue.c_str());
				// get the string out
				strValue = angle.asStringDMS();
			}
		}
		break;
	case kBearing:
		{
			if (ipTextInput)
			{
				strValue = ipTextInput->GetText();
				m_dirHelper.evaluateDirection(strValue);
				
				if (!m_dirHelper.isDirectionValid())
				{
					UCLIDException ue("ELI12228", "Invalid direction input");
					ue.addDebugInfo("Input", strValue);
					throw ue;
				}
				
				// The direction all in current direction format
				// Note: This direction string always represents the
				// Normal mode even though the current mode might be
				// in Reverse. Only when you calling one of the getting
				// actual value methods, it will count the reverse fact.
				strValue = m_dirHelper.getAlternateStringsAsDirection()[0];
			}
		}
		break;
	case kDistance:
		{
			if (ipTextInput)
			{
				strValue = ipTextInput->GetText();
				m_distance.evaluate(strValue);
				// distance always store in feet unit
				strValue = m_distance.asStringInUnit(kFeet);
			}
		}
		break;
	case kToggleCurve:
		{
			ECurveToggleInput eCurveToggleInput = getCurrentCurveToggleInputType();
			switch (eCurveToggleInput)
			{
			case kToggleDeltaIsGreaterThan180Degrees:
			case kToggleCurveDirectionIsLeft:
				strValue = "1";
				break;
			case kToggleDeltaIsLessThan180Degrees:
			case kToggleCurveDirectionIsRight:
				strValue = "0";
				break;
			}
		}
		break;
	case kToggleAngle:
		{
			EDeflectionAngleToggleInput eAngleToggleInput = getCurrentAngleToggleInputType();
			switch (eAngleToggleInput)
			{
			case kToggleAngleLeft:
				strValue = "1";
				break;
			case kToggleAngleRight:
				strValue = "0";
				break;
			}
		}
		break;
	default:
		strValue = "";
		break;
	}

	return strValue;
}
//--------------------------------------------------------------------------------------------------
void DrawingToolFSM::processExpectedInput(ITextInputPtr ipTextInput)
{
	// catch all exceptions - under no circumstances do we want exceptions to be thrown
	// back to the next scope (which would be the Input Manager).  This is the scope at
	// which all exceptions need to be handled.
	try
	{
		// set current distance unit type
		DistanceCore::setCurrentDistanceUnit(getCurrentDistanceUnitType());
		
		// if the input is toggle info, it shall only be processed by DIG
		string strText("");
		if (ipTextInput == NULL)
		{
			// what's the current segment type
			ESegmentType eSegmentType = m_pDIGWnd->getCurrentSegmentType();
			switch (eSegmentType)
			{
			case UCLID_FEATUREMGMTLib::kArc:
				{
					ECurveParameterType eCurveParamType = kInvalidParameterType;
					strText = getText(kToggleCurve, ipTextInput);
					if (!strText.empty())
					{
						ECurveToggleInput eCurveToggleInput = getCurrentCurveToggleInputType();
						switch (eCurveToggleInput)
						{
						case kToggleDeltaIsGreaterThan180Degrees:
						case kToggleDeltaIsLessThan180Degrees:
							eCurveParamType = kArcDeltaGreaterThan180Degrees;
							break;
						case kToggleCurveDirectionIsLeft:
						case kToggleCurveDirectionIsRight:
							eCurveParamType = kArcConcaveLeft;
							break;
						}
						
						m_pDIGWnd->setSegmentParameter(UCLID_FEATUREMGMTLib::kArc, eCurveParamType, strText);
						// update the segment in the drawing right away
						m_pDIGWnd->redrawCurrentSegment();

						// update default value and refresh prompt
						updateDefaultValues();
						refreshPrompt(true);

						return;
					}
				}
				break;
				
			case UCLID_FEATUREMGMTLib::kLine:
				{
					// it might be toggling angle
					if (strText.empty())
					{
						strText = getText(kToggleAngle, ipTextInput);
					}
					if (!strText.empty())
					{
						m_pDIGWnd->setSegmentParameter(UCLID_FEATUREMGMTLib::kLine, kArcConcaveLeft, strText);
						// update the segment in the drawing right away
						m_pDIGWnd->redrawCurrentSegment();
						// update default value and refresh prompt
						updateDefaultValues();
						refreshPrompt(true);

						return;
					}
				}
				break;
			}
		}
			
		EInputType eInputType = getCurrentExpectedInputType();
		// first process the input by DIG
		if (m_bInputTypeIsFromDIG)
		{
			strText = getText(eInputType, ipTextInput);
			
			bool bProcessedByDIG = m_pDIGWnd->processInput(strText);
			if (bProcessedByDIG)
			{
				// no more process needed
				return;
			}
		}

		// now process the input by FSM
		switch (eInputType)
		{
		case kToggleCurve:
		case kToggleAngle:
			break;

		default:
			{
				// if the text input is from some source doc
				addSourceDocument(ipTextInput);
				
				// post the output onto the command line
				string strTextOutput = ipTextInput->GetText();
				// store in default values map
				ECurveParameterType eCurrentCurveParam = m_pState->getCurrentCurveParameter();
				m_mapCurveParamToDefaultValue[eCurrentCurveParam] = strTextOutput;
				// set command prompt
				m_pCommandPrompt->setCommandInput(strTextOutput);
			}
			break;

		}
		m_pState->processInput(this, ipTextInput);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI02140")
}
//--------------------------------------------------------------------------------------------------
void DrawingToolFSM::notifyInputTypeChanged(bool bIsFromDIG)
{
	m_bInputTypeIsFromDIG = bIsFromDIG;
	// since input type changed, we need to update the input validator
	// and notify input manager
	enableInput();
}
//--------------------------------------------------------------------------------------------------
void DrawingToolFSM::updateState(ECurrentState eCurrentState)
{
	bool bEnableAngleTool = false;
	bool bHasAPoint = true;
	switch (eCurrentState)
	{
	case kNothingState:
		bHasAPoint = false;
		break;
	case kHasPointState:
		break;
	case kHasSegmentState:
		bEnableAngleTool = true;
		break;
	}

	m_pState->changeToProperStartState(this, bHasAPoint);
	m_pIcoMapUI->enableDeflectionAngleTool(bEnableAngleTool);
}
//--------------------------------------------------------------------------------------------------
void DrawingToolFSM::refreshPrompt(bool bIgnoreDIG)
{
	// current expected input type
	bool bShowDefaultValue = true;

	// if DIG is not expecting input, we can refresh command line prompt here
	if (!m_bInputTypeIsFromDIG)
	{
		EInputType eInputType = getCurrentExpectedInputType();
		if (eInputType == kToggleCurve || eInputType == kToggleAngle)
		{
			bShowDefaultValue = false;
		}

		m_pCommandPrompt->setCommandPrompt(m_pState->getPrompt(), bShowDefaultValue);

		return;
	}

	// go ahead and refresh the command prompt no matter the input
	// type is currently coming from the DIG
	if (bIgnoreDIG)
	{
		m_pCommandPrompt->setCommandPrompt(m_pState->getPrompt(), true);
	}
}
//--------------------------------------------------------------------------------------------------
void DrawingToolFSM::reset(void)
{
	m_vecSourceDocs.clear();
	m_nNumOfSegmentsAdded = 0;

	// display adapter needs to be reset
	if (m_ipDisplayAdapter) 
	{
		m_ipDisplayAdapter->Reset();
	}

	if (m_pDIGWnd)
	{
		m_pDIGWnd->reset();
	}

	// change state back to the beginning of current state
	// for instance, if it's line distance state, cancel will change 
	// the state to the starting of line, i.e. state line point
	m_pState->cancel(this);

	// disable the internal/deflection angle tool
	m_pIcoMapUI->enableDeflectionAngleTool(false);
}
//--------------------------------------------------------------------------------------------------
void DrawingToolFSM::selectFeatures(const string &strCommonSourceDocName)
{
	if (m_ipDisplayAdapter)
	{
		_bstr_t bstrSourceDocName(strCommonSourceDocName.c_str());
		
		m_ipDisplayAdapter->SelectFeatures(bstrSourceDocName);
	}
}
//--------------------------------------------------------------------------------------------------
void DrawingToolFSM::setAttributeManager(IAttributeManager* pAttributeManager)
{
	m_ipAttributeManager = pAttributeManager;
	ASSERT_RESOURCE_ALLOCATION("ELI12147", m_ipAttributeManager != NULL);

	m_pDIGWnd->setAttributeManager(pAttributeManager);
}
//--------------------------------------------------------------------------------------------------
void DrawingToolFSM::setIcoMapUI(IIcoMapUI* pIcoMapUI)
{
	m_pIcoMapUI = pIcoMapUI;
	ASSERT_RESOURCE_ALLOCATION("ELI12148", m_pIcoMapUI != NULL);

	m_pDIGWnd->setIcoMapUI(pIcoMapUI);
	m_pDIGWnd->setInputProcessor(this);
}
//--------------------------------------------------------------------------------------------------
void DrawingToolFSM::setDisplayAdapter(IDisplayAdapter* pDisplayAdapter)
{
	m_ipDisplayAdapter = pDisplayAdapter;
	ASSERT_RESOURCE_ALLOCATION("ELI12149", m_ipDisplayAdapter != NULL);

	m_pDIGWnd->setDisplayAdapter(pDisplayAdapter);
}
//--------------------------------------------------------------------------------------------------
void DrawingToolFSM::startCurveDrawing(void)
{
	// discard any segment in progress
	m_pDIGWnd->discardSegmentInProgress();

	if (m_ipDisplayAdapter)
	{
		bool bSupport = m_ipDisplayAdapter->SupportsSketchCreation == VARIANT_TRUE;
		
		if (!bSupport)
		{
			UCLIDException uclidException("ELI01075","Display adapter does not support sketch creation.");
			throw uclidException;
		}
		
		TPPoint startPoint;
		bool bSuccess = 
			m_ipDisplayAdapter->GetLastPoint(&startPoint.m_dX, &startPoint.m_dY) == VARIANT_TRUE;
		if (bSuccess)
		{
			changeState(DrawingToolStateCurveParam1::sGetInstance());
		}
		else
		{
			changeState(DrawingToolStateCurve::sGetInstance());
		}
	}
}
//--------------------------------------------------------------------------------------------------
void DrawingToolFSM::startLineDeflectionAngleDrawing(void)
{
	// discard any segment in progress
	m_pDIGWnd->discardSegmentInProgress();

	if (m_ipDisplayAdapter)
	{
		bool bSupport = m_ipDisplayAdapter->SupportsSketchCreation == VARIANT_TRUE;
		
		if (!bSupport)
		{
			throw UCLIDException("ELI02915","Display adapter does not support sketch creation.");
		}
		
		// retrieve tangent out from existing segments
		double dDummy;
		bool bSucceeded = 
			m_ipDisplayAdapter->GetLastSegmentTanOutAsPolarAngleInRadians(&dDummy) == VARIANT_TRUE;
		if (bSucceeded)
		{
			// if there's at least one segment in the drawing
			changeState(DrawingToolStateDeflectionAngle::sGetInstance());
		}
		else
		{
			changeState(DrawingToolStateLine::sGetInstance());
		}
	}
}
//--------------------------------------------------------------------------------------------------
void DrawingToolFSM::startLineDrawing(void)
{	
	m_pDIGWnd->discardSegmentInProgress();

	if (m_ipDisplayAdapter)
	{
		bool bSupport = m_ipDisplayAdapter->SupportsSketchCreation == VARIANT_TRUE;
		
		if (!bSupport)
		{
			UCLIDException uclidException("ELI01074","Display adapter does not support sketch creation.");
			throw uclidException;
		}
		
		// retrieve starting point from existing segments
		TPPoint startPoint;
		bool bSuccess = 
			m_ipDisplayAdapter->GetLastPoint(&startPoint.m_dX, &startPoint.m_dY) == VARIANT_TRUE;
		if (bSuccess)
		{
			changeState(DrawingToolStateLineBearing::sGetInstance());
		}
		else
		{
			changeState(DrawingToolStateLine::sGetInstance());
		}
	}
}
//--------------------------------------------------------------------------------------------------
void DrawingToolFSM::updateSketchAttributes(const string& strFeatureID)
{
	bool bOptionUpdateSketchAttributes = IcoMapOptions::sGetInstance().autoSourceDocLinkingIsEnabled();

	if (bOptionUpdateSketchAttributes && m_ipAttributeManager != NULL)
	{
		if (!m_vecSourceDocs.empty())
		{
			// Build the COM collection of source documents
			_bstr_t bstr;
			_variant_t variant;
			IVariantVectorPtr ipCollection;
			ipCollection.CreateInstance(__uuidof(VariantVector));
			vector<std::string>::const_iterator it;
			for (it = m_vecSourceDocs.begin();it != m_vecSourceDocs.end();it++)
			{
				bstr = (*it).c_str();
				variant = bstr;
				ipCollection->PushBack(variant);
			}
			
			m_ipAttributeManager->SetSketchSourceDocuments(
								_bstr_t(strFeatureID.c_str()), ipCollection);
		}
	}
}
//--------------------------------------------------------------------------------------------------
