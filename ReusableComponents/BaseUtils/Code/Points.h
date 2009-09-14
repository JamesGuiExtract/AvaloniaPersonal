#pragma once

#include "BaseUtils.h"
#include "Point.h"

#include <vector>

using namespace std;

//instantiate classes vector<Point>. This doesn't create an object
EXPIMP_TEMPLATE_BASEUTILS template class EXPORT_BaseUtils std::vector<Point>;

class EXPORT_BaseUtils Points : public vector<Point>
{
public:
	virtual void addPoint(const Point& point);

	bool contains(const Point& point) const;

	EXPORT_BaseUtils friend Points operator && (const Points& p1, const Points& p2);
};
