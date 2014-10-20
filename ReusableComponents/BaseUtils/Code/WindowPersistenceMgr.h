//==================================================================================================
//
// COPYRIGHT (c) 2012 Extract Systems, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	WindowPersistenceMgr.h
//
// PURPOSE:	To simplify restoring a CWnd to the position it was in the last time it was open.
//
// AUTHORS:	Steve Kurth
//
//==================================================================================================

#pragma once

#include "RegistryPersistenceMgr.h"

#include <string>

using namespace std;

class EXPORT_BaseUtils WindowPersistenceMgr
{
public:
	WindowPersistenceMgr(CWnd *pWnd, string strRegistryKey);

	// Saves the window's current position & size to the registry.
	void SaveWindowPosition();

	// Restores the window to the position & size last stored in the registry.
	void RestoreWindowPosition();
	
	// Moves controls that are anchored to the top right corner of the window
	void moveAnchoredTopRight(CWnd &pCtr, int nOldWidth, int nOldHeight, BOOL bRepaint = TRUE);

	// Moves controls that are anchored to the top, left and right sides of the window
	void moveAnchoredTopLeftRight(CWnd &pCtr, int nOldWidth, int nOldHeight, BOOL bRepaint = TRUE);

	// Moves controls that are anchored to the bottom left corner of the window
	void moveAnchoredBottomLeft(CWnd &pCtr, int nOldWidth, int nOldHeight, BOOL bRepaint = TRUE);

	// Moves controls anchored the the bottom right corner of the window
	void moveAnchoredBottomRight(CWnd &pCtr, int nOldWidth, int nOldHeight, BOOL bRepaint = TRUE);

	// Moves controls that are anchored to the bottom, left and right sides of the window
	void moveAnchoredBottomLeftRight(CWnd &pCtr, int nOldWidth, int nOldHeight, BOOL bRepaint = TRUE);

	// Moves controls that are anchored to all sides of the window
	void moveAnchoredAll(CWnd &pCtr, int nOldWidth, int nOldHeight, BOOL bRepaint = TRUE);
private:
	
	// The window for which position information should be saved/restored from the registry
	CWnd& m_wnd;

	// The RegistryPersistenceMgr that will be used to read/write from the registry
	RegistryPersistenceMgr m_registryManager;

	// Creates the required registry values if they don't already exist.
	// Returns true if the values were created, false if they already existed.
	bool createRegistryValues();
};