#include "TPPoint.h"

//--------------------------------------------------------------------------------------------------
// user defined predicate function class
//--------------------------------------------------------------------------------------------------
class YValueIsSmaller
{
public:
	// used to check if the first point's Y coord is 
	// smaller than the second point's Y coord.
	bool operator()(const TPPoint& p1, const TPPoint& p2);
};

//--------------------------------------------------------------------------------------------------
// user defined predicate function class
//--------------------------------------------------------------------------------------------------
class CosValueIsSmaller
{
public:
	// Constructor, initialize m_MinYPoint's Y coord to a large value
	CosValueIsSmaller(const TPPoint& minYPoint);

	// Using p1 and m_MinYPoint to create a line segment and
	// Using p2 and m_MinYPoint to create the second line segment
	// Get the cosine value of the angle of the two line segments and
	// comprare them
	bool operator()(const TPPoint& p1, const TPPoint& p2);

private:
	double calCosine(const TPPoint& point);

	TPPoint m_MinYPoint;
};
//--------------------------------------------------------------------------------------------------