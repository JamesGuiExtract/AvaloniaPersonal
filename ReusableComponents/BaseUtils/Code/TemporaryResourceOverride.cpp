#include "stdafx.h"
#include "TemporaryResourceOverride.h"
#include "UCLIDException.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

HINSTANCE TemporaryResourceOverride::m_hInstanceDefault = NULL;

// For using the default constructor, you've got to use this guy once.
// Typically, you do this once in your DLLMain function....
void TemporaryResourceOverride::sSetDefaultResource(HINSTANCE hInstDefault)
{
    ASSERT(hInstDefault != __nullptr);

    m_hInstanceDefault = hInstDefault;
}


// If you're always using the same resource instance, set the default, and use this constructor
TemporaryResourceOverride::TemporaryResourceOverride() 
	:m_hInstanceOld(NULL)
{
    ASSERT(m_hInstanceDefault != __nullptr);

    Init(m_hInstanceDefault);
}

// If you have a specific resource instance in mind, use this constructor.
TemporaryResourceOverride::TemporaryResourceOverride(HINSTANCE hInstNew)
	:m_hInstanceOld(NULL)
{
    ASSERT(hInstNew != __nullptr);

    Init(hInstNew);
}

TemporaryResourceOverride::~TemporaryResourceOverride()
{
	try
	{
		AfxSetResourceHandle(m_hInstanceOld);		// restore previous resource handle
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16408");
}

// Handles the various flavors of construction with a common handler
void TemporaryResourceOverride::Init(HINSTANCE hInst)
{
	m_hInstanceOld = AfxGetResourceHandle();	// preserve current resource handle
	AfxSetResourceHandle(hInst);				// use desired resources
}


