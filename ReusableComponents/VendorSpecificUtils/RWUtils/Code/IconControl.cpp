#include <stdafx.h>
#include "IconControl.h"
#include "resource.h"
#include <UCLIDException.h>

int CGXIconControl::IDS_CTRL_ICON = (int)GX_IDS_CTRL_ICON;


IMPLEMENT_CONTROL(CGXIconControl, CGXStatic);

CGXIconControl::CGXIconControl(CGXGridCore* pGrid)
: CGXStatic(pGrid) 
{
}

CGXIconControl::~CGXIconControl()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16475");
}
void CGXIconControl::Draw(CDC* pDC, CRect rect, ROWCOL nRow, ROWCOL nCol, const CGXStyle& style, const CGXStyle* pStandardStyle)
{
	try
	{
		
		ASSERT(pDC != NULL && pDC->IsKindOf(RUNTIME_CLASS(CDC)));
		// ASSERTION-> Invalid Device Context ->END
		ASSERT(nRow <= Grid()->GetRowCount() && nCol <= Grid()->GetColCount());
		// ASSERTION-> Cell coordinates out of range ->END
		ASSERT_VALID(pDC);
		DrawBackground(pDC, rect, style);
		
		if (rect.right <= rect.left || rect.Width() <= 1 || rect.Height() <= 1)
			return; 
		CString str = style.GetIncludeValue() ? style.GetValue() : _T("");
		if(str.Left(4) == _T("#ICO"))
		{
			int n = str.Find(_T(")"));
			CString strIDResource = str.Mid(5,n-5);
			UINT nIDResource = _ttoi(strIDResource);
			HICON hIcon = AfxGetApp()->LoadIcon (nIDResource);
			CRect r = CGXControl::GetCellRect(nRow, nCol, rect, &style);
			if(style.GetVerticalAlignment() == DT_VCENTER)
			{
				r.top = (r.bottom + r.top) / 2 - 16;
				r.bottom = (r.bottom + r.top) / 2 + 16;
			}
			if(style.GetHorizontalAlignment() == DT_CENTER)
			{
				r.left = (r.right + r.left) / 2 - 16;
				r.right = (r.right + r.left) / 2 + 16;
			}
			pDC->DrawIcon(r.left, r.top, hIcon);
			// child Controls: spin-buttons, hotspot, combobox btn, ...
			CGXControl::Draw(pDC, rect, nRow, nCol, style, pStandardStyle);
		} 
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12795");
} 
