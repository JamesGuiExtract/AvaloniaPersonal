//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	EntityAttributes.h
//
// PURPOSE:	This is an header file for EntityAttributes 
//			where this class has been declared as base 
//			class.  The code written in this file makes it possible for
//			initialize the combo controls.
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================

#pragma warning(disable:4786)

#include "stdafx.h"
#include <string>
#include <map>

using namespace std;

typedef map<string, string> STR2STR;
//==================================================================================================
//
// CLASS:	EntityAttributes
//
// PURPOSE:	To keep track of a list of attributes for each entity.  Each attribute has a name and
//			value, both of which are strings.
//
// REQUIRE:	Nothing.
// 
// INVARIANTS:
//			None.
//
// EXTENSIONS:
//			None.
//
// NOTES:	From a pure C++ perspective, this class is nothing but a map of string objects to
//			string objects.
//
class EntityAttributes : public map<string, string>
{
public:
	//	constructor
	EntityAttributes();
	EntityAttributes(string, string);

	EntityAttributes const& operator = (EntityAttributes const& );

public:
	//	attribute name
	string	m_strName;

	//	attribute value
	string	m_strValue;

	//	destructor
	virtual ~EntityAttributes();

};
//==================================================================================================
