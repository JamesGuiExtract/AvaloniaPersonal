#pragma once

#include <string>

class IIcoMapUI
{
public:
	virtual ~IIcoMapUI(){};
	virtual void enableToggle(bool bEnableToggleDirection, bool bEnableToggleDeltaAngle) = 0;
	virtual void enableDeflectionAngleTool(bool bEnable) = 0;
	virtual void enableInput(IInputValidator *pInputValidator, const char *pszPrompt) = 0;
	virtual void disableInput() = 0;
	// notify icomap about input received from sources other than SRIR, HTIR
	// or IcoMap command
	virtual void setInput(const std::string& strInput) = 0;
	// will set the toggle direction button and menu item state
	virtual void setToggleDirectionState(bool bLeft) = 0;
	// will set the toggle delta angle button menu item state
	virtual void setToggleDeltaAngleState(bool bGreaterThan180) = 0;
};