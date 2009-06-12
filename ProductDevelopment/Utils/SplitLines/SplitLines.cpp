// SplitLines.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <windows.h>

// windows.h must be included before cppUtil.h
#include <cppUtil.h>

#include <string>
#include <fstream>
#include <iostream>
using namespace std;

#include <stdlib.h>

void printUsage()
{
	cout << "Usage: this program needs three arguments" << endl;
	cout << " arg1: the name of the input file" << endl;
	cout << " arg2: the name of the output file" << endl;
	cout << " arg3: the number of characters per line to output" << endl;
	cout << endl;
}

int splitFile(const string& strInputFile, const string& strOutputFile, int iNumCharsPerLine)
{
	ifstream infile(strInputFile.c_str());
	ofstream outfile(strOutputFile.c_str());
	infile.unsetf(ios::skipws);
	while (infile && outfile)
	{
		string strCurrLine;
		getline(infile, strCurrLine);

		if ((int)strCurrLine.length() > iNumCharsPerLine)
		{
			while (true)
			{
				string strTemp(strCurrLine, 0, iNumCharsPerLine);
				strCurrLine.erase(0, iNumCharsPerLine);

				int iLastChar = iNumCharsPerLine-1;
				if (isspace((unsigned char)strTemp[iLastChar]))
				{
					// the found character is a whitespace char
					// write out strTemp with a newline, and then
					// skip all whitespace char until the next
					// non-whitespace char is encountered
					outfile << strTemp << endl;
					while (strCurrLine.length() > 0 && isspace((unsigned char)strCurrLine[0]))
						strCurrLine.erase(0, 1);
				}
				else
				{
					// TODO: make more efficient
					// the char found is not a whitespace char.
					// go backwards until a space is encountered
					while (!isspace((unsigned char)strTemp[strTemp.length() - 1]))
					{
						string strCharTemp;
						strCharTemp = (strTemp[strTemp.length() - 1]);
						strTemp.erase(strTemp.length() - 1, 1);
						strCurrLine.insert(0, strCharTemp);
					}

					// if strTemp has any characters in it, then
					// write it out and continue
					if (strTemp.length() > 0)
						outfile << strTemp << endl;
					else
					{
						strTemp = string(strCurrLine, 0, iNumCharsPerLine);
						outfile << strTemp << endl;
					}
				}

				// if there are no more characters on the line, then break
				if ((int)strCurrLine.length() <= iNumCharsPerLine)
				{
					outfile << strCurrLine << endl;
					break;
				}
			}

		}
		else
		{
			outfile << strCurrLine << endl;
		}
		
	}
	outfile.close();
	waitForFileAccess(strOutputFile, giMODE_READ_ONLY);

	return EXIT_SUCCESS;
}

int main(int argc, char* argv[])
{
	if (argc != 4)
	{
		printUsage();
		return EXIT_FAILURE;
	}
	string strInputFile = argv[1];
	string strOutputFile = argv[2];
	int iNumCharsPerLine = atoi(argv[3]);

	if (iNumCharsPerLine <= 0)
	{
		cout << "Invalid third argument!" << endl;
		cout << "Please enter a valid value for the number of characters to output per line!" << endl;
		cout << endl;
		return EXIT_FAILURE;
	}

	return splitFile(strInputFile, strOutputFile, iNumCharsPerLine);
}
