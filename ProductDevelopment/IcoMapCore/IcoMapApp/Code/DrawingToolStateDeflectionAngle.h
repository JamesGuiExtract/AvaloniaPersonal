#pragma once

#include "DrawingToolState.h"

class DrawingToolFSM;

class DrawingToolStateDeflectionAngle : public DrawingToolState  
{
public:
	static DrawingToolState* sGetInstance(void);
	static void sDelete(void);

	virtual void cancel(DrawingToolFSM* pFSM);
	virtual void processInput(DrawingToolFSM* pFSM, ITextInputPtr ipTextInput);
	virtual void changeToProperStartState(DrawingToolFSM* pFSM, bool bHasAPoint);
	virtual std::string getPrompt(void);
	virtual ECurveParameterType getCurrentCurveParameter();

protected:
	DrawingToolStateDeflectionAngle(EInputType eInputType,std::string strPrompt);
	virtual ~DrawingToolStateDeflectionAngle() {};
	virtual DrawingToolState* reset(void);

private:
	static DrawingToolStateDeflectionAngle* m_pInstance;
};
