#pragma once

#include "PageExtents.h"

#include <stack2.h>

class ZoomViewsManager
{
public:
	ZoomViewsManager(){};
	ZoomViewsManager(const ZoomViewsManager& toCopy);
	ZoomViewsManager& operator = (const ZoomViewsManager& toAssign);
	// push view extents to the current view stack
	// and clean the next view stack.
	// Note: the top most view extents from current view stack
	// is always the current view
	void addView(const PageExtents& currentView);
	// clear all view stacks
	void clearViews();
	// Return current view extents
	PageExtents getCurrentView();
	// number of next views
	int getNumberOfNextViews();
	// total number of previous views
	int getNumberOfPreviousViews();
	// pop top most view from next view stack and push it to the 
	// current view stack, return the top most view from current view stack
	PageExtents gotoNextView();
	// pop top most view from current view stack and push it to the next view stack, 
	// return the top most view extents from the current view stack
	PageExtents gotoPreviousView();
	// whether or not the stack has a current view
	bool hasCurrentView() {return !m_stkCurrentViewExtents.empty();}

private:
	// current view stack includes all previous views and current view.
	// Note: top most view is the current view
	stack2<PageExtents> m_stkCurrentViewExtents;
	// current view stack includes all next views right after current view.
	stack2<PageExtents> m_stkNextViewExtents;
};