//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	DrawingToolStateLineBearing.h
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

class DrawingToolStateLineBearing : public DrawingToolState  
{
public:
	static DrawingToolState* sGetInstance(void);
	static void sDelete(void);

	virtual void cancel(DrawingToolFSM* pFSM);
	virtual void processInput(DrawingToolFSM* pFSM, ITextInputPtr ipTextInput);
	virtual void changeToProperStartState(DrawingToolFSM* pFSM, bool bHasAPoint);
	virtual std::string getPrompt(void);

protected:
	DrawingToolStateLineBearing(EInputType eInputType,std::string strPrompt);
	virtual ~DrawingToolStateLineBearing() {};
	virtual DrawingToolState* reset(void);

private:
	static DrawingToolStateLineBearing* m_pInstance;
};
