// UpdateFileVersion.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

#include <TemporaryFileName.h>
#include <cppUtil.h>

#include <Windows.h>
#include <io.h>
#include <tchar.h>
#include <fstream>
#include <iostream>
#include <string>
#include <algorithm>

using namespace std;

//--------------------------------------------------------------------------------------------------
// Local functions
//--------------------------------------------------------------------------------------------------
bool ParseTargetFileVersion(const string& sSrc, string& sTargetVersion)
{
	bool bSuccess(false);

	int pos = sSrc.find_first_of("0123456789");
	if (pos != string::npos)
	{
		sTargetVersion = sSrc.substr(pos,string::npos);
		replace(sTargetVersion.begin(),sTargetVersion.end(),',','.');
		pos = 0;
		while ( (pos = sTargetVersion.find(' ',pos)) != string::npos)
		{
			sTargetVersion.erase(pos,1);	// delete any spaces
		}
		bSuccess = true;
	}
	else
	{
		cout << "Failed to parse the file version from the target file." << endl;
	}

	return bSuccess;
}
//--------------------------------------------------------------------------------------------------
bool ValidateFileVersion(const string& sInBuf,const string& sLabelVersion)
{
	bool bMatch(false);
	
	bool bStrFileVersion(false);
	string sTargetVersion;
	if (ParseTargetFileVersion(sInBuf,sTargetVersion))
	{
		int pos = 0;
		if ( (pos = sTargetVersion.find('\\',pos)) != string::npos)
		{
			bStrFileVersion = true;
			sTargetVersion.erase(pos,string::npos);
		}
		if (!sTargetVersion.compare(sLabelVersion))
		{
			bMatch = true;
		}
		else
		{
			if (bStrFileVersion)
			{
				cout << "\nWARNING: The target string file version " << sTargetVersion 
					<< " does not match the label version: " << sLabelVersion  << endl;
			}
			else
			{
				cout << "\nWARNING: The target file version " << sTargetVersion 
					<< " does not match the label version: " << sLabelVersion  << endl;
			}
		}
	}
		
	return bMatch;
}
//--------------------------------------------------------------------------------------------------
size_t FindNocase(const string& sText,const string& sTag)
{
	size_t pos = string::npos;

	string::const_iterator iterText = sText.begin();
	string::const_iterator iterTag = sTag.begin();
	while (iterText != sText.end())
	{
		while (iterTag != sTag.end())
		{
			if (toupper(*iterText) == toupper(*iterTag))
			{
				++iterText;
				if (++iterTag == sTag.end())
				{
					pos = (iterText - sText.begin()) - sTag.size();
					return pos;
				}
			}
			else
			{
				break;			
			}
		}
		++iterText;
		iterTag = sTag.begin();
	}

	return string::npos;
}
//--------------------------------------------------------------------------------------------------
size_t FindFirstDigitAfterTag(const string& sText,const string& sTag)
{
	size_t pos = FindNocase(sText,sTag);
	if (pos != string::npos)
	{
		pos += sTag.size();
		while (!isdigit((unsigned char)sText[pos]))
		{
			if (++pos >= sText.size())
			{
				pos = string::npos;
				break;
			}
		}	
	}
	if (pos == string::npos)
	{
		cout << _T("\nInvalid argument: ") << sText << _T(". Expecting digit(s) to follow ") << sTag;
	}


	return pos;
}
//--------------------------------------------------------------------------------------------------
// PURPOSE: To determine the number of characters long the product version number is given the
//			label and start of the version number.  This size excludes any suffix on the label
//			such as "B" for patch B.  It is assumed that a suffix will not contain any '.' chars.
size_t FindSizeOfVersionNumber(const string &strLabel, size_t posVersionStart)
{
	string strLabelEnd = strLabel.substr(posVersionStart);
	// Find the last '.' in the version.
	size_t posEnd = strLabelEnd.find_last_of('.');
	if (posEnd != string::npos)
	{
		// If we found the last '.', find the first non-numeric character that follows.
		posEnd = strLabelEnd.find_first_not_of("0123456789", posEnd+1);

		// string::npos indicates there were no characters following the version.
		// Simply return the length of the label from posVersionStart to the end.
		if (posEnd == string::npos)
		{
			posEnd = strLabelEnd.length();
		}
	}

	return posEnd;
}
//--------------------------------------------------------------------------------------------------
bool ParseLabelVersion(const string& sLabel, string& sVersion)
{
	bool bSuccess(false);

	sVersion.erase();
	string sTag(_T("ver. "));
	size_t pos = FindFirstDigitAfterTag(sLabel,sTag);
	if (pos != string::npos)
	{
		size_t sizeOfVersion = FindSizeOfVersionNumber(sLabel, pos);

		if (sizeOfVersion != string::npos)
		{
			sVersion = sLabel.substr(pos, sizeOfVersion);
			bSuccess = true;
		}
	}

	return bSuccess;
}
//--------------------------------------------------------------------------------------------------
string FileVersion(const string& sVersion)
{
	string sFileVersion(sVersion);

	replace(sFileVersion.begin(),sFileVersion.end(),'.',',');

	return sFileVersion;
}
//--------------------------------------------------------------------------------------------------
string StrFileVersion(const string& sVersion)
{
	string sStrFileVersion('"'+sVersion);

	replace(sStrFileVersion.begin(),sStrFileVersion.end(),'.',',');

	// insert a space after every comma
	size_t pos = 0;
	while ( (pos = sStrFileVersion.find(',',pos)) != string::npos)
	{
		sStrFileVersion.insert(++pos,_T(" "));		
	}

	sStrFileVersion += _T("\\0\"");

	return sStrFileVersion;
}
//--------------------------------------------------------------------------------------------------
string ProductVersion(const string& strVersion)
{
	string strProductVersion(strVersion);

	// product version shall be first two digits in the file version
	// ex. file version is 1.2.0.3, product version shall be 1.2.0.0
	int nDotPosition = strVersion.find(".");
	if (nDotPosition != string::npos)
	{
		// find next dot
		nDotPosition = strVersion.find(".", nDotPosition+1);
		if (nDotPosition != string::npos)
		{
			strProductVersion = strVersion.substr(0, nDotPosition+1) + "0.0";
		}
	}

	// replace all . with , 
	replace(strProductVersion.begin(),strProductVersion.end(),'.',',');

	return strProductVersion;
}
//--------------------------------------------------------------------------------------------------
string StrProductVersion(const string& sVersion)
{
	string sProductVersion('"'+ ProductVersion(sVersion));

	// insert a space after every comma
	size_t pos = 0;
	while ( (pos = sProductVersion.find(',',pos)) != string::npos)
	{
		sProductVersion.insert(++pos,_T(" "));		
	}

	sProductVersion += _T("\\0\"");

	return sProductVersion;
}
//--------------------------------------------------------------------------------------------------
bool UpdateVersionInfoInTmpFile(const string& sSrcFileName, const string& sTmpFileName, const string& sVersion)
{
	bool bSuccess(true);

	const string sFILEVER(FileVersion(sVersion));
	const string sProductVersion(ProductVersion(sVersion));
	const string sSTRFILEVER(StrFileVersion(sVersion));
	const string sStrProductVersion(StrProductVersion(sVersion));

	if (!sFILEVER.empty() && !sSTRFILEVER.empty())
	{
		std::ofstream outFile(sTmpFileName.c_str());
		if (outFile)
		{
			string sOutBuf;
			string sInBuf;
			std::ifstream inFile(sSrcFileName.c_str());
			bool bInXmlVersionNode = false;
			if (inFile.good())
			{
				while (!inFile.eof())
				{
					// If the return count is 0 then need to break out of the loop
					if (getline(inFile,sInBuf))
					{
						break;
					}

					// Make sure it is not a comment line
					if (sInBuf.find("//") == 0)
					{
						sOutBuf = sInBuf;
					}
					else if ( (sInBuf.find(_T(" FILEVERSION "))) != string::npos)	// must preceede FILEVER in logic sequence
					{
						sOutBuf = _T(" FILEVERSION ") + sFILEVER;
					}
					else if ( (sInBuf.find(_T(" PRODUCTVERSION "))) != string::npos)
					{
						sOutBuf = _T(" PRODUCTVERSION ") + sProductVersion;
					}
					else if ( (sInBuf.find(_T("            VALUE \"FileVersion\", "))) != string::npos)
					{
						sOutBuf = _T("            VALUE \"FileVersion\", ") + sSTRFILEVER;
					}
					else if ( (sInBuf.find(_T("            VALUE \"ProductVersion\", "))) != string::npos)
					{
						sOutBuf = _T("            VALUE \"ProductVersion\", ") + sStrProductVersion;
					}
					// for .net AssemblyInfo.cs file
					else if ((sInBuf.find(_T("[assembly: AssemblyVersion("))) != string::npos)
					{
						sOutBuf = _T("[assembly: AssemblyVersion(\"") + sVersion + "\")]";
					}
					// for .net AssemblyInfo.cs file
					else if ((sInBuf.find(_T("[assembly: AssemblyFileVersion("))) != string::npos)
					{
						sOutBuf = _T("[assembly: AssemblyFileVersion(\"") + sVersion + "\")]";
					}
					// for .net .resx resource file
					else if (bInXmlVersionNode &&
							 (sInBuf.find(_T("<value>1.0.0.0</value>"))) != string::npos)
					{
						sOutBuf = _T("    <value>" + sVersion + "</value>");
					}
					else
					{
						sOutBuf = sInBuf;
					}
					outFile << sOutBuf.c_str() << endl;

					// for .net .resx resource file
					bInXmlVersionNode = (sInBuf.find("<data name=\"Version\"") != string::npos);
				}
			}
			else
			{
				cout << _T("Failed to open src input file: ") << sSrcFileName;
			}

			outFile.close();
			waitForFileAccess(sTmpFileName, giMODE_READ_ONLY);
		}
		else
		{
			cout << _T("Failed to open temporary out file: ") << sTmpFileName;
		}
	}

  return bSuccess;
}
//--------------------------------------------------------------------------------------------------
bool ReplaceFileWithCopy(const string& strTargetFileName,const string& strTmpFileName)
{
	bool bSuccess(false);

	// Move the file, allowing: 
	// - different destination drive
	// - overwrite existing file
	// - do not return until move is complete
	if (MoveFileEx( strTmpFileName.c_str(), strTargetFileName.c_str(), 
		MOVEFILE_COPY_ALLOWED | MOVEFILE_REPLACE_EXISTING | MOVEFILE_WRITE_THROUGH ))
	{
		bSuccess = true;
	}
	else
	{
		DWORD dwResult = GetLastError();
		cout << _T("\nError from MoveFileEx: ") << dwResult << endl;
	}
	
	return bSuccess;
}

//--------------------------------------------------------------------------------------------------
// Main function
//--------------------------------------------------------------------------------------------------
int main(int argc, char* argv[])
{
	if (argc == 3)
	{
		string strTargetFileName = argv[1];
		string strLabel = argv[2];
		#ifdef _DEBUG
		cout << _T("\nTarget File: ") << strTargetFileName; 
		cout << _T("\nLabel: ")  << strLabel;
		#endif

		// Retrieve actual version information
		string strVersion;
		if (ParseLabelVersion(strLabel,strVersion))
		{
			#ifdef _DEBUG
			cout << _T("\nParsed version: ") << strVersion;
			#endif

			// Create a temporary file
			TemporaryFileName tfn( "", ".tmp", true );
			#ifdef _DEBUG
			cout << _T("\ntmpFileName: ") << tfn.getName() << endl;
			#endif

			// Update the version information into the temp file
			if (UpdateVersionInfoInTmpFile( strTargetFileName, tfn.getName(), strVersion ))
			{
				// Replace original file with update
				if (ReplaceFileWithCopy( strTargetFileName, tfn.getName() ))
				{
					cout << "\n\nUpdateFileVersion successful for the file '" << strTargetFileName 
						<<"' label version '" << strVersion << "'\n" << endl;
				}
				else
				{
					cout << _T("\nFailed to update file: ") << strTargetFileName;
				}
			}
		}
	}
	else
	{
		// Incorrect usage
		cout << _T("\nExpecting 2 parameters, not ") << argc-1;
		cout << _T("\nUsage:");
		cout << _T("\n\targ1 - fully-qualified name of file to update");
		cout << _T("\n\targ2 - labeled version, i.e. \"IcoMap ver. 3.5.1.47\"");
		cout << endl;
		return 1;
	}

	return 0;
}
//--------------------------------------------------------------------------------------------------
