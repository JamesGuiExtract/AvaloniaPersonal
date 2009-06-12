#pragma once

#include "BaseUtils.h"

#include <string>

// The classes that implement this interface are 
// responsible for handling progress updates

class EXPORT_BaseUtils IProgress
{
public:
	virtual void setPercentComplete(double fPercent) = 0;
	
	// Set a title that can be indicative of the type of action
	// that is occuring
	virtual void setTitle(std::string strTitle) = 0;
	
	// Set a string to represent the type of progress being made
	virtual void setText(std::string strText) = 0;

	// returns true if progress should be stopped on the current task
	virtual bool userCanceled() = 0;
};