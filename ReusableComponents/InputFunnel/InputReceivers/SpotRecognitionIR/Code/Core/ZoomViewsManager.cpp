#include "stdafx.h"
#include "ZoomViewsManager.h"

#include <UCLIDException.h>

using namespace std;

ZoomViewsManager::ZoomViewsManager(const ZoomViewsManager& toCopy)
{
	m_stkCurrentViewExtents = toCopy.m_stkCurrentViewExtents;
	m_stkNextViewExtents = toCopy.m_stkNextViewExtents;
}

ZoomViewsManager& ZoomViewsManager::operator = (const ZoomViewsManager& toAssign)
{
	m_stkCurrentViewExtents = toAssign.m_stkCurrentViewExtents;
	m_stkNextViewExtents = toAssign.m_stkNextViewExtents;

	return *this;
}

void ZoomViewsManager::addView(const PageExtents& currentView)
{
	// compare current view with the one about to be added, 
	// if they the same, the currentView will not be added to the stack
	if (!m_stkCurrentViewExtents.empty() && m_stkCurrentViewExtents.top() == currentView)
	{
		return;
	}

	// add the view to the current stack
	m_stkCurrentViewExtents.push(currentView);

	// clean next view stack
	m_stkNextViewExtents.clear();
}

void ZoomViewsManager::clearViews()
{
	m_stkCurrentViewExtents.clear();
	m_stkNextViewExtents.clear();
}

PageExtents ZoomViewsManager::getCurrentView()
{
	// current view stack shall not be empty
	if (m_stkCurrentViewExtents.empty())
	{
		throw UCLIDException("ELI03210", "There's no current view available.");
	}

	return m_stkCurrentViewExtents.top();
}

int ZoomViewsManager::getNumberOfNextViews()
{
	return m_stkNextViewExtents.size();
}

int ZoomViewsManager::getNumberOfPreviousViews()
{
	// shall not include current view
	return m_stkCurrentViewExtents.size() - 1;
}

PageExtents ZoomViewsManager::gotoNextView()
{
	if (m_stkNextViewExtents.empty())
	{
		throw UCLIDException("ELI03214", "Failed to goto next view.");
	}

	PageExtents nextView = m_stkNextViewExtents.top();
	// move top of the next view stack to the current view stack
	m_stkCurrentViewExtents.push(nextView);
	m_stkNextViewExtents.pop();

	return nextView;
}

PageExtents ZoomViewsManager::gotoPreviousView()
{
	// if there's no view or only one view in the current view stack
	// then return false.
	if (m_stkCurrentViewExtents.size() <= 1)
	{
		throw UCLIDException("ELI03213", "Failed to goto previous view.");
	}

	// move top view from the current view stack to the next view stack
	m_stkNextViewExtents.push(m_stkCurrentViewExtents.top());
	m_stkCurrentViewExtents.pop();

	// the top most view was the previous view, now it is the current view.
	return m_stkCurrentViewExtents.top();
}

