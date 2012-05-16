#pragma once

#include "DrawingToolState.h"

class DrawingToolFSM;

class DrawingToolStateDeflectionDistance : public DrawingToolState  
{
public:
	static DrawingToolState* sGetInstance(void);
	static void sDelete(void);

	virtual void cancel(DrawingToolFSM* pFSM);
	virtual void processInput(DrawingToolFSM* pFSM, ITextInputPtr ipTextInput);
	virtual void changeToProperStartState(DrawingToolFSM* pFSM, bool bHasAPoint);

protected:
	DrawingToolStateDeflectionDistance(EInputType eInputType,std::string strPrompt);
	virtual ~DrawingToolStateDeflectionDistance() {};
	virtual DrawingToolState* reset(void);

private:
	static DrawingToolStateDeflectionDistance* m_pInstance;
};
