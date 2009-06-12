//==================================================================================================
//
// COPYRIGHT (c) 1998 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	Specify the name of the file here (without the path)
//
// PURPOSE:	Specify the purpose of the code that resides in this file.  Usually a few sentences 
//			would do.  When multiple lines need to be used, make sure to ***use tabs*** to align the
//			various lines of text so that the paragraph of text appears neatly indented like this
//			paragraph.
//
// NOTES:	Attach any notes here that you feel are pertinent to maintainers of this code.
//
// AUTHORS:	Indicate the full name of the authors (one on each line) that have performed non-trivial
//			Maintenance of code in this file.
//
//==================================================================================================

#ifndef FOO_CLASS_HPP
#define FOO_CLASS_HPP

//==================================================================================================
//
// CLASS:	Specify the name of the class.
//
// PURPOSE:	Describe the purpose of this class.  Describe why it was created, and what it 
//			accomplishes at a high level.  If this class is part of an implementation of a design 
//			pattern, please describe usage/modification of the design pattern here in breif words.
//
// REQUIRE:	If there are any requirements for using this class (such as using a specific operating
//			system, having administrative previleges, calling a certain static method before the 
//			class can be used, etc), then document those general requirements here.  Provide any 
//			other appropriate information here as well regarding class usage requirements at a 
//			high level.
// 
// INVARIANTS:
//			A class invariant is an expression that is always true with respect to the class.  For
//			instance, for a Book class, the following are class invariants:
//			uiNumPages > 0
//			uiNumChapters > 0
//			Document one class invariant on each line, followed by any applicable comments or notes.
//			Finally, all class invariants are expected to be written in 'programming terms'.
//
// EXTENSIONS:
//			Add any notes here for developers that may be extending the functionality of this 
//			class by subclassing or by adding new methods.
//
// NOTES:	Finally, add any other oddball information that is appropriate for any one using, or
//			maintaining this class.
//
//==================================================================================================

class Foo
{
public:
	//==============================================================================================
	// PURPOSE: Specify the purpose of this method in not more than three sentences.
	// REQUIRE: Specify ANYTHING the caller MUST DO before they can call this method.  
	//			For instance you may want to mention that methodXYZ() should have been called 
	//			at least once before a call to this method.  You should also specify any 
	//			requirements on the parameters (such as range validation, etc.).  If any of the
	//			requirements are not met, the method MUST throw an exception with the appropriate
	//			details.  Similary, if the caller has satisfied all the requirements you MUST 
	//			satisfy all the promises.  Finally, all require statements must be written in 
	//			'programming terms' whenever possible.  See below for examples.
	// PROMISE: Specify ANYTHING that you promise to the caller.  For instance, you may promise what
	//			you are going to return to the caller.  Or you may promise that a future call to
	//			methodXYZ() will return a certain value, and so on.  All promise statements must
	//			be written in 'programming terms' whenever possible.  See below for examples.
	// ARGS:	lParam1: Specify the semantic meaning of parameter 1.
	//			dParam2: Specify the semantic meaning of parameter 2.
	//			iParam3: Specify the semantic meaning of parameter 3
	void method1(long ulParam1, double dParam2, int iParam3) const;
	//==============================================================================================
	// PURPOSE: To retrieve the ID associated with this object.
	// REQUIRE: setProperties() must have been called at least once.
	// PROMISE: To return the value passed as the ulNewID argument to setProperties().
	// ARGS:	None.
	unsigned long getID() const;
	//==============================================================================================
	// PURPOSE: To retrieve the name associated with this object.
	// REQUIRE: setProperties() must have been called at least once.
	// PROMISE: To return the value passed as the pszNewObjectName argument to setProperties().
	// ARGS:	None.
	const char *getName() const;
	//==============================================================================================
	// PURPOSE: To set the ID of this object.
	// REQUIRE: ulNewID > 0
	//			pszObjectName != NULL
	// PROMISE: getID() == ulNewID, after method returns.
	//			getName() == pszNewObjectName, after method returns.
	// ARGS:	ulNewID: the new ID to be associated with this object.
	//			pszObjectName: the new name to be associated with this object.
	void setProperties(unsigned long ulNewID, const char *pszNewObjectName);
	//==============================================================================================

private:

	// Put each class attribute on its own line.  Describe the purpose of the class attribute, how
	// it is used, and any other pertinent information for developers (oddities, state transition 
	// information, etc.) as part of these comments.  Generally, be brief and only provide as much
	// information as useful.  See below for examples.
	double dParam1;
	
	// ulID stores the ID associated with this object.
	unsigned long ulID;

	// pszObjectName stores the name associated with this object.  A **COPY*** of the name data that
	// was passed to the setProperties() method is stored in this variable.
	const char *pszObjectName;
};

#endif // FOO_CLASS_HPP