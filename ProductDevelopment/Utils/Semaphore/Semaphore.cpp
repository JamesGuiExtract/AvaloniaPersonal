
#pragma warning(disable:4786)

#include <Win32Semaphore.h>
#include <UCLIDException.h>

#include <string>
#include <iostream>
#include <cpputil.h>
using namespace std;

//-------------------------------------------------------------------------------------------------
void printUsage()
{
	cout << "Usage: " << endl;
	cout << "  Semaphore Create {SemaphoreName}" << endl;
	cout << "  Semaphore Acquire {SemaphoreName}" << endl;
	cout << "  Semaphore Release {SemaphoreName}" << endl;
	cout << "  Semaphore Delete {SemaphoreName}" << endl;
	cout << endl;
}
//-------------------------------------------------------------------------------------------------
string getInternalSemaphoreName(const string& strSemaphore)
{
	return string("SEMAPHORE_EXE_") + strSemaphore;
}
//-------------------------------------------------------------------------------------------------
int main(int argc, char* argv[])
{
	try
	{
		if (argc != 3)
		{
			printUsage();
			return 0;
		}

		string strAction(argv[1]);
		string strSemaphore(argv[2]);

		makeUpperCase(strAction);
		makeUpperCase(strSemaphore);
		
		if (strAction == "CREATE")
		{
			// if the named semaphore already exists, then just exit the program
			// after displaying a message
			try
			{
				Win32Semaphore sem( strSemaphore );
				cout << "The semaphore <" << strSemaphore << "> already exists." << endl;
			}
			catch (...)
			{
				// the named semaphore does not exist...so create one
				string strInternalSemaphoreName = getInternalSemaphoreName(strSemaphore);
				string strTemp;
				Win32Semaphore semInternal( 1, 1, strInternalSemaphoreName );
				Win32Semaphore semUser( 1, 1, strSemaphore );
				cout << "The semaphore <" << strSemaphore << "> has been created." << endl;
				semInternal.acquire();
				semInternal.acquire();
			}
		}
		else if (strAction == "DELETE")
		{
			string strInternalSemaphoreName = getInternalSemaphoreName(strSemaphore);
			Win32Semaphore semInternal( strInternalSemaphoreName );
			semInternal.release();
			cout << "The semaphore <" << strSemaphore << "> has been deleted." << endl;
		}
		else if (strAction == "ACQUIRE")
		{
			Win32Semaphore semUser( strSemaphore );
			cout << "Waiting to acquire semaphore <" << strSemaphore << "> ... " << flush;
			semUser.acquire();
			cout << "done." << endl;
		}
		else if (strAction == "RELEASE")
		{
			Win32Semaphore semUser( strSemaphore );
			semUser.release();
			cout << "The semaphore <" << strSemaphore << "> has been released." << endl;
		}
		else
		{
			cout << "ERROR: Invalid argument!" << endl;
			printUsage();
			cout << endl;
		}
	}
	catch (UCLIDException& ex)
	{
		string strMsg;
		ex.asString(strMsg);
		cout << "ERROR:" << endl;
		cout << strMsg << endl;
	}
	catch (...)
	{
		cout << "ERROR:" << endl;
		cout << "Unknown exception caught!" << endl;
	}

	return 0;
}
//-------------------------------------------------------------------------------------------------
