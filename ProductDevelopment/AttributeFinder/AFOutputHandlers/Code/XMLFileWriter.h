
#pragma once

#include "StdAfx.h"

#include <string>
using namespace std;

//-------------------------------------------------------------------------------------------------
// Imports
//-------------------------------------------------------------------------------------------------
// NOTE: The import is being done here instead of stdafx.h because the derived classes are 
// the only ones that need the XML support
#import <msxml.tlb> named_guids

class XMLFileWriter
{
public:
	virtual void WriteFile(const string& strFile, IIUnknownVector *pAttributes) = 0;
};
