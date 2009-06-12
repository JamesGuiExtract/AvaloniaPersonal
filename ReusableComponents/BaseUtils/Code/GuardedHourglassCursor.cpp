#include "stdafx.h"
#include "GuardedHourglassCursor.h"
#include "UCLIDException.h"

GuardedHourglassCursor::GuardedHourglassCursor()
{
	// set the cursor to the hourglass cursor, and keep track of the previous cursor
	previousCursor = SetCursor(LoadCursor(NULL, IDC_WAIT));

	bOriginalCursorRestored = false;
}

GuardedHourglassCursor::~GuardedHourglassCursor()
{
	try
	{
		// restore the cursor to its previous if that has not already been done
		if (!bOriginalCursorRestored)
			restoreOriginalCursor();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16384");
}

void GuardedHourglassCursor::restoreOriginalCursor()
{
	// restore the cursor to its previous.
	SetCursor(previousCursor);

	bOriginalCursorRestored = true;
}