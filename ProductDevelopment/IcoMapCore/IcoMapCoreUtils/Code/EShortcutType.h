//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	EShortcutType.h
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//==================================================================================================
#pragma once

// IcoMap command IDs in the Speech Grammar XML file "sit_grammar.xml"
// correspond to the ordinal numbers of the shortcuts in this enum. If you edit
// this enum, make sure the XML file is updated.
enum EShortcutType
{
	kShortcutNull = 0,
	kShortcutCurve1,
	kShortcutCurve2,
	kShortcutCurve3,
	kShortcutCurve4,
	kShortcutCurve5,
	kShortcutCurve6,
	kShortcutCurve7,
	kShortcutCurve8,
	kShortcutLine,
	kShortcutLineAngle,
	kShortcutGenie,
	kShortcutRight,
	kShortcutLeft,
	kShortcutGreater,
	kShortcutLess,
	kShortcutForward,
	kShortcutReverse,
	kShortcutFinishSketch,
	kShortcutFinishPart,
	kShortcutDeleteSketch,
	kShortcutUndo,
	kShortcutRedo,
	kShortcutEnter
};
