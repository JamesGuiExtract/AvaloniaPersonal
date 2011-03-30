
#pragma once

#include "BaseUtils.h"
#include "Win32Event.h"

#include <memory>

// forward declarations
class TimeRollbackPreventer;

// This class just wraps the TimeRollbackPreventer class, except that the interface
// is a pure C++ interface, and therefore does not expose any MFC objects, and therefore
// can be used from non-MFC projects (like services and ATL COM EXE's).
class EXPORT_BaseUtils TimeRollbackPreventerWrapper
{
public:
	TimeRollbackPreventerWrapper(Win32Event& rStateIsInvalidEvent);
	~TimeRollbackPreventerWrapper();

private:
	// NOTE: unique_ptr was not used here because it caused the following warning in
	// ExtractTRP project:
	// warning C4251: 'TimeRollbackPreventerWrapper::m_apTRP' : class 'std::unique_ptr<_Ty>' 
	// needs to have dll-interface to be used by clients of class 'TimeRollbackPreventerWrapper'
	TimeRollbackPreventer *m_pTRP;
};
