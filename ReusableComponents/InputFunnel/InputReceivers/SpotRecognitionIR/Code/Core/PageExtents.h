
#pragma once

// the following structure is used to keep track of the page extents
// on each of the pages of a multi-page tiff so that we can return
// back to the last working area of each page when navigating from
// page to page.
class PageExtents
{
public:
	friend bool operator == (const PageExtents& e1, const PageExtents& e2);
public:
	double dBottomLeftX, dBottomLeftY, dTopRightX, dTopRightY;

};
