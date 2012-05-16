//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	DrawingToolStateLineDistance.h
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

class DrawingToolStateLineDistance : public DrawingToolState  
{
public:
	static DrawingToolState* sGetInstance(void);
	static void sDelete(void);

	virtual void cancel(DrawingToolFSM* pFSM);
	virtual void processInput(DrawingToolFSM* pFSM, ITextInputPtr ipTextInput);
	virtual void changeToProperStartState(DrawingToolFSM* pFSM, bool bHasAPoint);

protected:
	DrawingToolStateLineDistance(EInputType eInputType,std::string strPrompt);
	virtual ~DrawingToolStateLineDistance() {};
	virtual DrawingToolState* reset(void);

private:
	static DrawingToolStateLineDistance* m_pInstance;
};
