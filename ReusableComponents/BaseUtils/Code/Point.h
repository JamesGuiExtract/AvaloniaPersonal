#pragma once

#include "BaseUtils.h"

class EXPORT_BaseUtils Point 
{
public:

	Point(const Point& p)
	{
		dX = p.dX;
		dY = p.dY;
		ulID = p.ulID;
		bInitialized = true;
	}
	
	Point();
	Point(double x, double y);

	inline double getX(void) const
	{
		return dX;
	}

	inline double getY(void) const
	{
		return dY;
	}

	Point& operator = (const Point& p)
	{
		dX = p.dX;
		dY = p.dY;
		bInitialized = p.bInitialized;
		ulID = p.ulID;
		return *this;
	}

	void setID(unsigned long _ulID);
	
	inline unsigned long getID(void) const
	{
		return ulID;
	}

	bool isInitialized() const;

	double distanceTo(const Point& p) const;

	double angleTo(const Point& p) const;

	EXPORT_BaseUtils friend bool operator == (const Point& a, const Point& b);

	EXPORT_BaseUtils friend bool operator < (const Point& a, const Point& b);

private:
	unsigned long ulID;
	bool bInitialized;
	double dX, dY;
};
