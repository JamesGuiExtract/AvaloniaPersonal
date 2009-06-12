#pragma once

#include "BaseUtils.h"

class EXPORT_BaseUtils GuardedHourglassCursor
{
public:
	GuardedHourglassCursor();
	~GuardedHourglassCursor();
	void restoreOriginalCursor();

private:
	HCURSOR previousCursor;
	bool bOriginalCursorRestored;
};