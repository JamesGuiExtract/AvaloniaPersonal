// MsgBox.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

#include <iostream>
using namespace std;

void printUsage()
{
	cout << endl;
	cout << "This program requires exactly one argument, namely, the message to display." << endl;
	cout << endl;
}

int main(int argc, char* argv[])
{
	// if one argument is given, display it in a message box
	if (argc == 2)
	{
		AfxMessageBox(argv[1]);
	}
	else
	{
		printUsage();
	}

	return 0;
}
