//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	DrawingToolStateCurvePoint.h
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	John Hurd
//
//==================================================================================================

#pragma once

#include "DrawingToolState.h"

class DrawingToolFSM;

class DrawingToolStateCurvePoint : public DrawingToolState  
{
public:
	static DrawingToolState* sGetInstance(void);
	static void sDelete(void);

	// jumps out of the current sub state set
	virtual void cancel(DrawingToolFSM* pFSM);
	virtual void processInput(DrawingToolFSM* pFSM, ITextInputPtr ipTextInput);
	virtual void changeToProperStartState(DrawingToolFSM* pFSM, bool bHasAPoint);

protected:
	DrawingToolStateCurvePoint(EInputType eInputType,std::string strPrompt);
	virtual ~DrawingToolStateCurvePoint() {};
	virtual DrawingToolState* reset(void);

private:
	static DrawingToolStateCurvePoint* m_pInstance;
};
