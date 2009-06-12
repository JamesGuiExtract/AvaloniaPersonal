// Test.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

#include <TPPolygon.h>


#include <iostream>

int main(int argc, char* argv[])
{
	TPPolygon a, b;

	a.addPoint(TPPoint(0,-1));
	a.addPoint(TPPoint(10,-1));
	a.addPoint(TPPoint(10, 1));
	a.addPoint(TPPoint(0, 1));

	TPPoint p1(10,1.01);

	bool bTemp = a.encloses(p1, true);

	cout << "a contains p1: ";
	if (bTemp)
		cout << "true" << endl;
	else
		cout << "false" << endl;

	/*
	// overlapping B
	b.addPoint(TPPoint(-1,-1));
	b.addPoint(TPPoint(5, 1));
	b.addPoint(TPPoint(6, 1));
	b.addPoint(TPPoint(12, -2));
	*/

	// non-overlapping B
	b.addPoint(TPPoint(10,10));
	b.addPoint(TPPoint(5, 3));
	b.addPoint(TPPoint(6, 4));
	b.addPoint(TPPoint(12, 2));

	bTemp = a.overlaps(b);

	cout << "a overlaps b: ";
	if (bTemp)
		cout << "true" << endl;
	else
		cout << "false" << endl;

	return 0;
}
