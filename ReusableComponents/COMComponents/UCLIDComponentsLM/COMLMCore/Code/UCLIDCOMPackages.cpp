//============================================================================
//
// COPYRIGHT (c) 2002 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	UCLIDCOMPackages.cpp
//
// PURPOSE:	Implementation of the COMPackages class
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//============================================================================

#include "stdafx.h"
#include "UCLIDCOMPackages.h"
#include "Resource.h"

#include <StringTokenizer.h>
#include <CppUtil.h>
#include <CommentedTextFileReader.h>
#include <UCLIDException.h>

using namespace std;

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

//-------------------------------------------------------------------------------------------------
// COMPackages
//-------------------------------------------------------------------------------------------------
COMPackages::COMPackages()
{
}
//-------------------------------------------------------------------------------------------------
COMPackages::~COMPackages()
{
	// Release the package and component data
	m_mapPackageIDsToNames.clear();
	m_mapComponentIDsToNames.clear();
	m_mapPackageIDsToComponents.clear();
}

//-------------------------------------------------------------------------------------------------
// Public methods
//-------------------------------------------------------------------------------------------------
std::map<unsigned long,std::string> COMPackages::getComponents()
{
	// Return map populated during readComponentsFile()
	return m_mapComponentIDsToNames;
}
//-------------------------------------------------------------------------------------------------
std::vector<unsigned long> COMPackages::getPackageComponents(std::string strPackageName)
{
	std::vector<unsigned long>	vecIDs;
	std::string					strCollectedComponents = "";
	unsigned long				ulPackageID = 0;

	// First, find the associated package ID
	map<unsigned long, std::string>::iterator iterPackage;
	for (iterPackage = m_mapPackageIDsToNames.begin(); 
		iterPackage != m_mapPackageIDsToNames.end(); iterPackage++)
	{
		// Retrieve this package name - trimming leading dash & whitespace
		string strThisName = trim( iterPackage->second, "- ", "" );

		// Case-insensitive comparison of strings
		if (_strcmpi( strThisName.c_str(), strPackageName.c_str() ) == 0)
		{
			// Retrieve package ID
			ulPackageID = iterPackage->first;

			// Found a match, stop searching
			break;
		}
	}

	// Continue if an ID was found
	if (ulPackageID != 0)
	{
		// Check the package to component map
		map<unsigned long, std::string>::iterator iter;
		for (iter = m_mapPackageIDsToComponents.begin(); 
			iter != m_mapPackageIDsToComponents.end(); iter++)
		{
			// Compare package IDs
			if (iter->first == ulPackageID)
			{
				// IDs match, retrieve string
				strCollectedComponents = iter->second;

				// Found a match, stop searching
				break;
			}
		}
	}

	// Continue processing if a collection is available
	if (strCollectedComponents.size() > 0)
	{
		//////////////////////////////////////////////////
		// Pre-process string to replace any package names
		// with associated strings
		//////////////////////////////////////////////////
		long lPos = strCollectedComponents.find( '$' );
		while (lPos != -1)
		{
			// Locate the terminating colon
			long lEnd = strCollectedComponents.find( ':', lPos );

			// Extract the package name
			string	strName;
			if (lEnd != -1)
			{
				// Package name is a substring
				strName = strCollectedComponents.substr( lPos + 1, 
					lEnd - lPos - 1 );
			}
			else
			{
				// Package name finishes the string
				long lLength = strCollectedComponents.length();
				strName = strCollectedComponents.substr( lPos + 1, 
					lLength - lPos - 1 );
			}

			// Locate this package ID
			bool			bFound = false;
			map<unsigned long, std::string>::iterator iter;
			for (iter = m_mapPackageIDsToNames.begin(); 
				iter != m_mapPackageIDsToNames.end(); iter++)
			{
				// Extract package name from map
				string	strMapName( iter->second );

				// Remove any leading exclamation point
				if (strMapName[0] == '!')
				{
					strMapName = strMapName.substr( 1 );
				}

				// Compare names
				if (_strcmpi( strName.c_str(), strMapName.c_str() ) == 0)
				{
					// Get the associated package ID
					ulPackageID = iter->first;

					bFound = true;
					break;
				}
			}

			// Replace package name with ID's
			if (bFound)
			{
				// Check the package to component map
				for (iter = m_mapPackageIDsToComponents.begin(); 
					iter != m_mapPackageIDsToComponents.end(); iter++)
				{
					// Compare package IDs
					if (iter->first == ulPackageID)
					{
						// IDs match, replace strName with the associated IDs
						strCollectedComponents.replace( lPos, lEnd - lPos, 
							(iter->second).c_str() );

						// Found a match, stop searching
						break;
					}
				}
			}
			else
			{
				// TODO: Create and throw exception - Package not found
			}

			// Search again
			lPos = strCollectedComponents.find( '$' );
		}

		//////////////////////////////////////
		// Parse the string into component IDs
		//////////////////////////////////////
		vector<string> vecTokens;
		StringTokenizer	st( ':', false );
		st.parse( strCollectedComponents, vecTokens );

		unsigned long	ulID = 0;
	
		// Step through each prospective ID
		for (unsigned int ui = 0; ui < vecTokens.size(); ui++)
		{
			// Retrieve the ID as a string
			string	strID = vecTokens[ui];

			// Convert string to ID
			ulID = asLong( strID );

			// Find the ID in the component map
			if (m_mapComponentIDsToNames.find( ulID ) != 
				m_mapComponentIDsToNames.end())
			{
				// Add the component ID to the vector
				vecIDs.push_back( ulID );

			}		// end if component ID is in the map
			else
			{
				// Create warning string
				CString	zTemp;
				zTemp.Format( "Component ID \"%d\" is not found in the Components map.  Include this ID?", ulID );

				// Provide warning to user
				if (MessageBox( NULL, zTemp, "Warning", MB_YESNO | MB_ICONQUESTION ) == IDYES)
				{
					vecIDs.push_back( ulID );
				}
			}		// end else component ID is NOT in the map
		}			// end for each prospective ID
	}				// end if this package was found

	return vecIDs;
}
//-------------------------------------------------------------------------------------------------
std::vector<std::string> COMPackages::getPackages()
{
	std::vector<string>	vecPackageNames;

	// Step through map of package IDs and names
	std::map<unsigned long, std::string>::iterator iter;
	for (iter = m_mapPackageIDsToNames.begin(); 
		iter != m_mapPackageIDsToNames.end(); iter++)
	{
		// Ignore the package name if leading exclamation point
		string	strName( iter->second );
		if (strName[0] != '!')
		{
			// Add the name to the vector
			vecPackageNames.push_back( strName );
		}
	}

	return vecPackageNames;
}
//-------------------------------------------------------------------------------------------------
void COMPackages::init(bool bUseEmbeddedResources/* = false*/)
{
	try
	{
		// Read the components file
		int	iCount = readComponentsFile(bUseEmbeddedResources);

		// Read the packages file
		iCount = readPackagesFile(bUseEmbeddedResources);
	}
	catch (UCLIDException ue)
	{
		// Display the exception to the user
		ue.display();
	}
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
int COMPackages::readComponentsFile(bool bUseEmbeddedResource)
{
	int iCount = 0;

	vector<string> vecFileLines = bUseEmbeddedResource
		? getFileLines(IDR_COMPONENTS_FILE)
		: getFileLines("Components.dat");

	CommentedTextFileReader fileReader(vecFileLines);

	while (!fileReader.reachedEndOfStream())
	{
		// Retrieve the next non-comment line
		string strLine = fileReader.getLineText();

		// Skip any empty lines
		if (strLine.empty()) 
		{
			continue;
		}

		// Parse the line using comma delimiter
		vector<string> vecTokens;
		StringTokenizer	st( ',' );
		st.parse( strLine.c_str(), vecTokens );

		// Error if not exactly two tokens
		if (vecTokens.size() != 2)
		{
			// Create and throw exception
			UCLIDException ue("ELI11975", "Unable to parse string - token count != 2" );
			ue.addDebugInfo( "Token Count", vecTokens.size() );
			ue.addDebugInfo( "Input Line", strLine );
			throw ue;
		}
		else
		{
			string	strID = vecTokens[0];
			string	strName = vecTokens[1];

			// Convert ID to number
			unsigned long ulID = asUnsignedLong( strID );

			// Add item to component map and increment count
			m_mapComponentIDsToNames[ulID] = strName;
			++iCount;
		}		// end else exactly two tokens
	}
	//while(ifs);

	return iCount;
}
//-------------------------------------------------------------------------------------------------
//int COMPackages::readPackagesFile2()
//{
//	int iCount = 0;
//	CommentedTextFileReader* pFileReader;
//
//	HMODULE handle = ::GetModuleHandle("COMLMCore.dll");
//	HRSRC rc = ::FindResource(handle, MAKEINTRESOURCE(IDR_PACKAGES_TEXT_FILE), RT_RCDATA);
//	HGLOBAL rcData = ::LoadResource(handle, rc);
//	string strPackagesFileText = string(static_cast<const char*>(::LockResource(rcData)));
//	
//	vector<string> vecLines;
//	// Tokenize the lines using the newline character
//	StringTokenizer st('\n');
//	st.parse(strPackagesFileText, vecLines);
//
//	CommentedTextFileReader fileReader(vecLines);
//	string strLine;
//	do
//	{
//		// Retrieve the next non-comment line
//		strLine = fileReader.getLineText();
//
//		// Skip any empty lines
//		if (strLine.empty())
//		{
//			continue;
//		}
//
//		// Parse the line using comma delimiter
//		vector<string> vecTokens;
//		StringTokenizer	st(',');
//		st.parse(strLine.c_str(), vecTokens);
//
//		// Error if not exactly two tokens
//		if (vecTokens.size() != 2)
//		{
//			// Create and throw exception
//			UCLIDException ue("ELI11977", "Unable to parse string - token count != 2");
//			ue.addDebugInfo("Token Count", vecTokens.size());
//			ue.addDebugInfo("Input Line", strLine);
//			throw ue;
//		}
//		else
//		{
//			// Ensure that package name is not already present
//			bool			bDuplicate = false;
//			unsigned long	ulMaxID = 0;
//			std::map<unsigned long, std::string>::iterator iter;
//			for (iter = m_mapPackageIDsToNames.begin();
//				iter != m_mapPackageIDsToNames.end(); iter++)
//			{
//				// Compare names
//				if (_strcmpi((vecTokens[0]).c_str(),
//					(iter->second).c_str()) == 0)
//				{
//					bDuplicate = true;
//				}
//
//				// Check the package IDs because we need to 
//				// create a new one
//				ulMaxID = max(ulMaxID, iter->first);
//			}
//
//			// Continue processing the line only if 
//			// this is a new package
//			if (!bDuplicate)
//			{
//				// Add package to package map
//				m_mapPackageIDsToNames[ulMaxID + 1] = vecTokens[0];
//
//				// Add collected component IDs to package map
//				m_mapPackageIDsToComponents[ulMaxID + 1] = vecTokens[1];
//
//				// Increment package count
//				++iCount;
//
//			}	// end if package not already included
//		}		// end else exactly two tokens
//
//	} while (!fileReader.reachedEndOfStream());
//
//	return iCount;
//}
//-------------------------------------------------------------------------------------------------
vector<string> COMPackages::getFileLines(unsigned long ulResourceID)
{
	HMODULE handle = ::GetModuleHandle("COMLMCore.dll");
	HRSRC rc = ::FindResource(handle, MAKEINTRESOURCE(ulResourceID), RT_RCDATA);
	HGLOBAL rcData = ::LoadResource(handle, rc);
	string strPackagesFileText = string(static_cast<const char*>(::LockResource(rcData)));

	vector<string> vecLines;
	// Tokenize the lines using the newline character
	StringTokenizer st('\n');
	st.parse(strPackagesFileText, vecLines);
	
	return vecLines;
}
//-------------------------------------------------------------------------------------------------
vector<string> COMPackages::getFileLines(string strFileName)
{
	// Get path to bin folder and DAT file
	string	strPath = getModuleDirectory("COMLMCore.DLL") + "\\" + strFileName;
	if (!isFileOrFolderValid(strPath))
	{
		// Create and throw exception
		UCLIDException ue("ELI11976", "Unable to open Packages file");
		ue.addDebugInfo("Path", strPath);
		throw ue;
	}

	// Open Packages.dat file
	ifstream ifs(strPath.c_str());

	vector<string> vecLines;
	while (ifs)
	{
		string strLine;
		getline(ifs, strLine);
		vecLines.push_back(strLine);
	}

	return vecLines;
}
//-------------------------------------------------------------------------------------------------
int COMPackages::readPackagesFile(bool bUseEmbeddedResource/* = false*/)
{
	int iCount = 0;

	vector<string> vecFileLines = bUseEmbeddedResource
		? getFileLines(IDR_PACKAGES_FILE)
		: getFileLines("Packages.dat");

	CommentedTextFileReader fileReader(vecFileLines);

	//// Get path to bin folder and DAT file
	//string	strPath = getModuleDirectory( "COMLMCore.DLL" ) + 
	//	"\\Packages.dat";
	//if (!isFileOrFolderValid( strPath ))
	//{
	//	// Create and throw exception
	//	UCLIDException ue("ELI11976", "Unable to open Packages file" );
	//	ue.addDebugInfo( "Path", strPath );
	//	throw ue;
	//}
	//
	//// Open Packages.dat file
	//ifstream ifs(strPath.c_str());
	//CommentedTextFileReader fileReader(ifs, "//", true);
	//string strLine;
	while (!fileReader.reachedEndOfStream())
	{
		// Retrieve the next non-comment line
		string strLine = fileReader.getLineText();

		// Skip any empty lines
		if (strLine.empty()) 
		{
			continue;
		}

		// Parse the line using comma delimiter
		vector<string> vecTokens;
		StringTokenizer	st( ',' );
		st.parse( strLine.c_str(), vecTokens );

		// Error if not exactly two tokens
		if (vecTokens.size() != 2)
		{
			// Create and throw exception
			UCLIDException ue("ELI11977", "Unable to parse string - token count != 2" );
			ue.addDebugInfo( "Token Count", vecTokens.size() );
			ue.addDebugInfo( "Input Line", strLine );
			throw ue;
		}
		else
		{
			// Ensure that package name is not already present
			bool			bDuplicate = false;
			unsigned long	ulMaxID = 0;
			std::map<unsigned long, std::string>::iterator iter;
			for (iter = m_mapPackageIDsToNames.begin(); 
				iter != m_mapPackageIDsToNames.end(); iter++)
			{
				// Compare names
				if (_strcmpi( (vecTokens[0]).c_str(), 
					(iter->second).c_str() ) == 0)
				{
					bDuplicate = true;
				}

				// Check the package IDs because we need to 
				// create a new one
				ulMaxID = max( ulMaxID, iter->first );
			}

			// Continue processing the line only if 
			// this is a new package
			if (!bDuplicate)
			{
				// Add package to package map
				m_mapPackageIDsToNames[ulMaxID+1] = vecTokens[0];

				// Add collected component IDs to package map
				m_mapPackageIDsToComponents[ulMaxID+1] = vecTokens[1];

				// Increment package count
				++iCount;

			}	// end if package not already included
		}		// end else exactly two tokens
	}
	//while(!fileReader.reachedEndOfStream());

	return iCount;
}
//-------------------------------------------------------------------------------------------------
