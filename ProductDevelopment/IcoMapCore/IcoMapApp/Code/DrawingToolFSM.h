//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	DrawingToolFSM.h
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	John Hurd
//
//==================================================================================================

#pragma once

#include "InputProcessor.h"
#include "DynamicInputGridWnd.h"

#include <EInputType.h>
#include <DistanceCore.h>
#include <DirectionHelper.h>

#include <string>
#include <vector>
#include <map>

class CommandPrompt;
class DrawingToolState;
class LineCalculationEngine;
class ICurveCalculationEngine;
class IIcoMapUI;

class DrawingToolFSM  : public InputProcessor
{
public:
	DrawingToolFSM(IInputManager *pInputManager, 
					CommandPrompt* pCommandPrompt,
					DynamicInputGridWnd* pDIGWnd);
	virtual ~DrawingToolFSM();

	virtual void processExpectedInput(ITextInputPtr ipTextInput);
	virtual void notifyInputTypeChanged(bool bIsFromDIG);
	virtual void updateState(ECurrentState eCurrentState);

	virtual void changeState(DrawingToolState* pState);

	void addSourceDocument(ITextInputPtr ipTextInput);
	void selectFeatures(const std::string& strCommonSourceDocName);
	void deleteSketch(void);
	// update default values such as next tangent-in, next line bearing, etc.
	void updateDefaultValues(void);
	void finishSketch(void);
	void finishPart(void);
	void reset(void);

	// refresh command prompt according to current state
	// bIgnoreDIG: true - no matter the input is from DIG, refresh
	// the command line prompt according to the current state of FSM
	void refreshPrompt(bool bIgnoreDIG = false);

	// return the pointer to the DIG
	DynamicInputGridWnd* getDIG() {return m_pDIGWnd;}
			
	// retrieves current unit type for distance
	EDistanceUnitType getCurrentDistanceUnitType();
	
	void setAttributeManager(IAttributeManager* pAttributeManager);
	void setDisplayAdapter(IDisplayAdapter* pDisplayAdapter);
	
	// start the line drawing with input as direction (i.e. bearing, 
	// polar angle and and azimuth) distance
	void startLineDrawing(void);
	// start the line drawing with input as internal/deflection angle and distance
	void startLineDeflectionAngleDrawing(void);
	// start curve drawing with three parameters
	void startCurveDrawing(void);

	void notifySketchModified(long nActualNumOfSegments);

	// Retrieves tangent-out angle of the last segment drawn in the sketch 
	// as polar angle in radians 
	double getLastSegmentTanOutAngleInRadians();

	// sets current toggle inputs: left or right, big or small (for both
	// toggle curve as well as toggle deflection angle)
	void setCurrentToggleInputs(bool bLeft, bool bBig) {m_bLeft = bLeft; m_bBig = bBig;}

	// get current toggle inputs
	void getCurrentToggleInputs(bool& bLeft, bool& bBig) {bLeft = m_bLeft; bBig = m_bBig;}

	// get text out from TextInput object based on the input type
	std::string getText(EInputType eInputType, ITextInputPtr ipTextInput);

	//***************************************
	// New Methods to integrate input funnel

	// according to current state, set/enable input for the input manager 
	void enableInput();

	// return the default value in string for the current curve parameter
	std::string getCurrentDefaultCurveParameterValue() const;

	// get/set currently expected curve toggle input type
	ECurveToggleInput getCurrentCurveToggleInputType() const;
	void setCurrentCurveToggleInputType(ECurveToggleInput eToggleType) {m_eCurveToggleInputType = eToggleType;}

	EDeflectionAngleToggleInput getCurrentAngleToggleInputType() const;
	void setCurrentAngleToggleInputType(EDeflectionAngleToggleInput eAngleToggleInput) {m_eAngleToggleInputType = eAngleToggleInput;}

	// get current expected input type
	// Note: this value has nothing to do with any input type 
	// of previous segment. 
	EInputType getCurrentExpectedInputType() const;

	// set the reference to IcoMapUI
	void setIcoMapUI(IIcoMapUI* pIcoMapUI);

protected:
	IAttributeManagerPtr m_ipAttributeManager;	// smart COM pointer to IAttributeManager interface
	IDisplayAdapterPtr m_ipDisplayAdapter;		// smart COM pointer to IDisplayAdapter interface
	CommandPrompt* m_pCommandPrompt;			// prompts the cartographer to act
	IInputManager* m_pInputManager;				// manages input from input receiver sources

	DrawingToolState* m_pState;					// current state of the Finite State Machine
	std::vector<std::string> m_vecSourceDocs;	// collection of source document names
	DistanceCore	m_distance;					// for checking units

	void setExpectedInputType(EInputType eInputType, const std::string& strInputVariableName);
	void updateSketchAttributes(const std::string& strFeatureID);


private:
	//*************************************
	// Memeber variables

	// number of segment drawn in the current sketch
	long m_nNumOfSegmentsAdded;

	// input validators to be used in the application
	IInputValidatorPtr m_ipAngleInputValidator;
	IInputValidatorPtr m_ipDirectionInputValidator;
	IInputValidatorPtr m_ipDistanceInputValidator;
	IInputValidatorPtr m_ipPointInputValidator;
	IInputValidatorPtr m_ipNothingInputValidator;

	std::map<ECurveParameterType, std::string> m_mapCurveParamToDefaultValue;

	// if current expected input type is kToggleCurve, 
	// check what's the current toggle input type
	ECurveToggleInput m_eCurveToggleInputType;

	// if current expected input type is kToggleAngle, 
	// check what's the current angle toggle input type
	EDeflectionAngleToggleInput m_eAngleToggleInputType;

	// reference to the IcoMapUI
	IIcoMapUI* m_pIcoMapUI;

	// current toggle inputs
	bool m_bLeft;
	bool m_bBig;

	DirectionHelper m_dirHelper;

	// grid wnd
	DynamicInputGridWnd* m_pDIGWnd;

	// whether or not the current input type is coming from DIG
	bool m_bInputTypeIsFromDIG;

	//**********************************
	// Helper functions
	void addToSourceDocCollection(std::string strSourceDocToAdd);
};

