#pragma warning(disable:4786)

#include <CommonToExtractProducts.h>
#include <afxwin.h>

#include <iostream>
#include <FileDirectorySearcher.h>
#include <cpputil.h>

#include <string>
using std::string;
#include <vector>
using std::vector; 

enum EChangeType
{
	kNoChange,
	kUpper,
	kLower,
	kTitle
} eChangeType = kNoChange;

bool bVerbose = false;
bool bRecursive = false;
bool bDirectory = false;

//------------------------------------------------------------------------------
void displayUsage(void)
{
	// there has to be at least three arguments
	cout << endl;
	cout << "Usage: <filename> [/u /l /t] [OPTIONS]"<< endl;
	cout << "\t/u - CHANGE FILE TO UPPER CASE"<<endl;
	cout << "\t/l - change file to lower case"<<endl;
	cout << "\t/t - Change File To Title Case"<<endl;
	cout << "OPTIONS:" << endl;
	cout << "\t/v - Provide verbose output"<<endl;
	cout << "\t/d - Treat filename as a directory search"<<endl;
	cout << "\t     i.e. c:\\tmp\\*.txt"<<endl;
	cout << "\t/r - Recursively search subdirectories"<<endl;
	cout << "\t     only has an effect when /d is also specified"<<endl;

}

bool changeCase(string strFileName)
{

	//this will store the contents of the input file
	std::string strFileContents;

	// open the input file and get its contents
	ifstream infile(strFileName.c_str());
	if (!infile)
	{
		cout << "Unable to open file " << strFileName.c_str() << " in read mode!" << endl;
		return EXIT_FAILURE;
	}
	// iterate though the input file, get each line, perform the substitution
	// and write to the output file
	while (infile.good())
	{
		char c;
		infile.get(c);
		strFileContents += c;
	}
	infile.close();

	switch(eChangeType)
	{
	case kUpper:
		makeUpperCase(strFileContents);
		break;
	case kLower:
		makeLowerCase(strFileContents);
		break;
	case kTitle:
		makeTitleCase(strFileContents);
		break;
	}


	// write the modified contents back out
	ofstream outfile(strFileName.c_str());
	if (!outfile)
	{
		cout << "Unable to open file " << strFileName.c_str() << " in write mode!" << endl;
		return EXIT_FAILURE;
	}
	
	for(unsigned int i = 0; i < strFileContents.length(); i++)
	{
		outfile.put(strFileContents.at(i));
	}
	outfile.close();
	waitForFileAccess(strFileName, giMODE_READ_ONLY);

	return EXIT_SUCCESS;
}
//------------------------------------------------------------------------------
int main(int argc, char** argv)
{

	string strFileName, strType;



	if (argc < 3)
	{
		displayUsage();
		return EXIT_FAILURE;
	}
	else
	{
	
		// the file name is always the first argument
		strFileName = argv[1];
		strType = argv[2];
		if(strType == "/u")
		{
			eChangeType = kUpper;
		}
		else if(strType == "/l")
		{
			eChangeType = kLower;
		}
		else if(strType == "/t")
		{
			eChangeType = kTitle;
		}
		else
		{
			displayUsage();
			return EXIT_FAILURE;
		}

		int i = 3;
		for(i = 3; i < argc; i++)
		{
			string arg = argv[i];
			if(arg == "/v")
			{
				bVerbose = true;
			}
			else if(arg == "/r")
			{
				bRecursive = true;
			}
			else if(arg == "/d")
			{
				bDirectory = true;
			}
			else
			{
				displayUsage();
				return EXIT_FAILURE;
			}
		}
	}


	if(bVerbose)
	{
		cout << "Change Case Utility:" << endl;
		string strTmp;
		if(eChangeType == kUpper)
			strTmp = "Upper";
		else if(eChangeType == kLower)
			strTmp = "Lower";
		else if(eChangeType == kTitle)
			strTmp = "Title";
		cout << "Changing all files to " << strTmp.c_str() << " case"  << endl;
		
	}

	int success = EXIT_FAILURE;
	if(bDirectory)
	{
		FileDirectorySearcher fsd;
		
		cout<<"Sdfsdfsdfsdf"<<endl;

		vector<string> files = fsd.searchFiles(strFileName, bRecursive);

		cout<<"Sdfsdfsdfsdf"<<endl;
		
		for(unsigned int i = 0; i < files.size(); i++)
		{
			if(bVerbose)
			{
				cout << "  strFileName  = <" << files[i].c_str() << ">"  << endl;
			}
			success = changeCase(files[i]);
		}
	}
	else
	{
		if(bVerbose)
		{
			cout << "  strFileName  = <" << strFileName.c_str() << ">"  << endl;
		}
		success = changeCase(strFileName);
	}
	return success;
	
}