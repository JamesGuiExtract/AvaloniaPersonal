//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	DrawingToolStateLine.h
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

class DrawingToolStateView : public DrawingToolState  
{
public:
	virtual ~DrawingToolStateView() {};
	static DrawingToolState* sGetInstance();
	static void sDelete(void);

	virtual void cancel(DrawingToolFSM* pFSM);
	virtual void processInput(DrawingToolFSM* pFSM, ITextInputPtr ipTextInput);
	virtual void changeToProperStartState(DrawingToolFSM* pFSM, bool bHasAPoint) {};

protected:
	DrawingToolStateView(EInputType eInputType,std::string strPrompt);
	virtual DrawingToolState* reset(void);

private:
	static DrawingToolStateView* m_pInstance;
};
