#include "AfxAppMainWindowRestorer.h"
#include "UCLIDException.h"

//-------------------------------------------------------------------------------------------------
// AfxAppMainWindowRestorer
//-------------------------------------------------------------------------------------------------
AfxAppMainWindowRestorer::AfxAppMainWindowRestorer()
{
	m_pMainWnd = AfxGetApp()->m_pMainWnd;
}
//-------------------------------------------------------------------------------------------------
AfxAppMainWindowRestorer::~AfxAppMainWindowRestorer()
{
	try
	{
		AfxGetApp()->m_pMainWnd = m_pMainWnd;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16372");
}
//-------------------------------------------------------------------------------------------------
