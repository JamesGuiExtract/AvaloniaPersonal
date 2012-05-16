//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	DrawingToolStateCurveParam3.h
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

class DrawingToolStateCurveParam3 : public DrawingToolState  
{
public:
	static DrawingToolState* sGetInstance(void);
	static void sDelete(void);

	virtual void cancel(DrawingToolFSM* pFSM);
	virtual void processInput(DrawingToolFSM* pFSM, ITextInputPtr ipTextInput);
	virtual void changeToProperStartState(DrawingToolFSM* pFSM, bool bHasAPoint);

protected:
	DrawingToolStateCurveParam3(EInputType eInputType,std::string strPrompt);
	virtual ~DrawingToolStateCurveParam3() {};
	virtual DrawingToolState* reset(void);

private:
	static DrawingToolStateCurveParam3* m_pInstance;
};
