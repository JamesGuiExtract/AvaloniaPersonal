//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	InputProcessor.h
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	John Hurd
//			Duan Wang
//
//==================================================================================================
#pragma once

// what's current state for input
// kNothingState - there's no point/segment in the drawing
// kPointState -  there's a starting point
// kSegmentState - there's at least a segment in the drawing
enum ECurrentState{kNothingState = 0, kHasPointState, kHasSegmentState};

class InputProcessor  
{
public:
	InputProcessor(){};
	virtual ~InputProcessor() = 0 {};
	virtual void processExpectedInput(ITextInputPtr ipTextInput) = 0;
	virtual void notifyInputTypeChanged(bool bIsFromDIG) = 0;
	virtual void updateState(ECurrentState eCurrentState) = 0;
};
