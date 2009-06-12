#pragma once

#include "..\..\..\APIs\RogueWave\Inc\Grid\gxall.h"
#include "RWUtils.h"



class RW_UTILS_API CGXIconControl : public CGXStatic
{ 
	DECLARE_CONTROL(CGXIconControl) 
public: 
	CGXIconControl(CGXGridCore* pGrid);
	virtual ~CGXIconControl();

	virtual void Draw(CDC* pDC, CRect rect, ROWCOL nRow, ROWCOL nCol, const CGXStyle& style, const CGXStyle* pStandardStyle);

	static int IDS_CTRL_ICON;
};