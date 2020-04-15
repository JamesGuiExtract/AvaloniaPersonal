//============================================================================
//
// COPYRIGHT (c) 2002 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	UCLIDCOMPackages.h
//
// PURPOSE:	Definition of the COMPackages class
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//============================================================================

#pragma once

#include <string>
#include <vector>
#include <map>

using namespace std;

class COMPackages
{
public:
	
	//=======================================================================
	// PURPOSE: Constructs a COMPackages object and reads the Packages.dat 
	//				file.
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	None
	COMPackages();

	//=======================================================================
	// PURPOSE: Destructor for the COMPackages object.
	// REQUIRE: Nothing
	// PROMISE: TBD
	// ARGS:	None
	~COMPackages();

	//=======================================================================
	// PURPOSE: Provides the collection of components.
	// REQUIRE: Nothing
	// PROMISE: Returns a map of component IDs and associated names.  
	//				Components are defined in Components.dat
	// ARGS:	None
	std::map<unsigned long,std::string> getComponents();

	//=======================================================================
	// PURPOSE: Provides the collection of components contained in the 
	//				specified package.
	// REQUIRE: Nothing
	// PROMISE: Returns a vector of component IDs defined as being contained 
	//				in the specified package.  Package names and components 
	//				are defined in Packages.dat
	// ARGS:	strPackageName - name of selected package
	std::vector<unsigned long> getPackageComponents(std::string strPackageName);

	//=======================================================================
	// PURPOSE: Provides the collection of packages.
	// REQUIRE: Nothing
	// PROMISE: Returns a vector of package names.  Package names and 
	//				components are defined in Packages.dat
	// ARGS:	None
	std::vector<std::string> getPackages();

	//=======================================================================
	// PURPOSE: Initialize the object data by reading the Components.dat 
	//				file and then the Packages.dat file.
	// REQUIRE: Components.dat and Packages.dat files must be present in the 
	//				same directory as the executable.
	// PROMISE: Nothing
	// ARGS:	None
	void init(bool bUseEmbeddedResources = false);

private:

	//=======================================================================
	// PURPOSE: Read the Components.dat file and populate the data map.
	// REQUIRE: Components.dat file must be present in the same directory
	//				as the executable.
	// PROMISE: Returns the number of components read.
	// ARGS:	bUseEmbeddedResource- true to use the embedded components.dat
	//			and packages.dat, false to use the files that exist parallel
	//			to the running process.
	int readComponentsFile(bool bUseEmbeddedResource);

	//=======================================================================
	// PURPOSE: Read the Packages.dat file and populate the data vector.
	// REQUIRE: Packages.dat file must be present in the same directory
	//				as the executable.
	// PROMISE: Returns the number of packages read.
	// ARGS:	bUseEmbeddedResource- true to use the embedded components.dat
	//			and packages.dat, false to use the files that exist parallel
	//			to the running process.
	int readPackagesFile(bool bUseEmbeddedResource);

	//=======================================================================
	// PURPOSE: Returns the lines from the specified filename
	vector<string> getFileLines(string strFileName);

	//=======================================================================
	// PURPOSE: Returns the lines from the specified embedded resource file
	vector<string> getFileLines(unsigned long ulResourceID);


///////////////
// DATA MEMBERS
///////////////
private:

	// Collection of package names
//	std::vector<std::string> m_vecPackageNames;
	std::map<unsigned long,std::string> m_mapPackageIDsToNames;

	// Collection of component names
	std::map<unsigned long,std::string> m_mapComponentIDsToNames;

	// Collection of package and component associations
	// string1 = package name
	// string2 = component IDs delimited by colons
	std::map<unsigned long,std::string> m_mapPackageIDsToComponents;
};
