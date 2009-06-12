// TestOCREngines.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

#include "RecognitionEngineTester.h"

#include <LicenseMgmt.h>

#include <iostream>


using namespace std;

int main(int argc, char* argv[])
{
	// initialize com object library
	CoInitializeEx(NULL, COINIT_MULTITHREADED);
	
	{
		// initialize license
		LicenseManagement::sGetInstance().initializeLicense("AW247YHUG8");

		// start the timer
		clock_t  start , finish;
		start = clock();
		
		RecognitionEngineTester engineTester(argv[1]);
		engineTester.processFile(argv[1], 0);
		
		// stop the timer
		finish = clock();
		
		double dDuration = (double)(finish - start) / CLOCKS_PER_SEC;
		
		unsigned long ulFailedCases = engineTester.getNumberOfFailedCases();
		unsigned long ulPassedCases = engineTester.getNumberOfSucceededCases();
		unsigned long ulTotalTestCases = ulFailedCases + ulPassedCases;
		// print summery
		cout << endl << endl;
		cout << "-----------------" << endl;
		cout << "TOTAL TEST REPORT" << endl;
		cout << "-----------------" << endl;
		cout << "Total test cases processed : " << ulTotalTestCases << endl;
		cout << "Total test cases passed    : " << ulPassedCases << endl;
		cout << "Total test cases failed    : " << ulFailedCases << endl;
		
		if (ulTotalTestCases)
		{
			cout << "Total percentage success   : " << ulPassedCases*100/(ulTotalTestCases) << "%" << endl;
		}
		
		cout << "Total processing time      : " << dDuration << "s" << endl;
		
	}
	// uninitialize com object library
	CoUninitialize();

	return 0;
}

