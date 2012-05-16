#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	DrawingToolState.h
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	John Hurd
//
//==================================================================================================

#include <EInputType.h>

#include <string>

class DrawingToolFSM;

class DrawingToolState  
{
public:
	virtual ~DrawingToolState() = 0 {};

	// cancel current state and go one state level up
	// ex1. if current state is DrawingToolStateCurveParam2 , 
	// cancel and go back to DrawingToolStateCurve.
	// ex2. if current state is DrawingToolStateLine,
	// cancel and go back to DraiwngToolStateView
	virtual void cancel(DrawingToolFSM* pFSM) = 0;
	virtual void processInput(DrawingToolFSM* pFSM, ITextInputPtr ipTextInput) = 0;
	// set to an appropriate state depending upon whether it needs a starting point or not.
	// bHasAPoint indicates whether or not a point state should be skipped
	virtual void changeToProperStartState(DrawingToolFSM* pFSM, bool bHasAPoint) = 0;

	virtual std::string getPrompt(void) {return m_strPrompt;}

	EInputType getExpectedInputType(void) const {return m_eInputType;}
	void setExpectedInputType(EInputType eInputType){m_eInputType = eInputType;}
	void setPrompt(const std::string& strPrompt){m_strPrompt = strPrompt;}
	
	// returns curve parameter type while in current state
	virtual ECurveParameterType getCurrentCurveParameter() {return m_eCurveParameter;}
	void setCurrentCurveParameter(ECurveParameterType eCurveParam) {m_eCurveParameter = eCurveParam;}

protected:
	EInputType m_eInputType;			// expected type of input when in this state
	ECurveParameterType m_eCurveParameter;	// expected curve parameter when in this state
	std::string m_strPrompt;			// prompt for the user when in this state

	DrawingToolState(EInputType eInputType,std::string& strPrompt);
	virtual void changeState(DrawingToolFSM* pFSM, DrawingToolState* pState);
	virtual DrawingToolState* reset(void) = 0;

};
