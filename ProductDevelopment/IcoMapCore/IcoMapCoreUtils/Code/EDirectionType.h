//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	EDirectionType.h
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan (Aug 2001 to present)
//
//==================================================================================================
#pragma once

enum EDirectionType
{
	kInvalidDirection,		// invalid direction
	kN,						//true North 
	kE,						//true East 
	kS,						//true South
	kW,						//true West
	kNE,					//Start from North, going toward East
	kSE,					//Start from South, going toward East
	kSW,					//Start from South, going toward West
	kNW						//Start from North, going toward West
};
