// FindLines.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "FindLines.h"

#include <l_bitmap.h>
#include <cpputil.hpp>
#include <UCLIDException.hpp>
#include <ExtractZoneAsImage.h>
#include <LicenseMgmt.h>
#include <UCLIDExceptionDlg.h>

#include <string>
#include <iostream>

#ifdef _DEBUG
#define new DEBUG_NEW
#endif


// The one and only application object

CWinApp theApp;

using namespace std;

struct Line
{
	UINT uiLineOrientation;
	long lStartRow;
	long lStartCol;
	long lLength;
	long lTop;
	long lLeft;
	long lBottom;
	long lRight;
	Line()
	{
		uiLineOrientation = LINEREMOVE_HORIZONTAL;
		lStartRow = 0;
		lStartCol =0;
		lLength = 0;
		lTop = 0;
		lLeft = 0;
		lBottom = 0;
		lRight = 0;
	}
};

vector<Line> vecLines;

//Line Remove
//This examples removes vertical lines that are at least 200 pixels in length
//     and no more than 5 pixels in width
//The lines can have gaps up to two pixels in length
//A callback is used to display information about each line removed
//The callback does NOT return a region

L_INT  EXT_FUNCTION	ExampleLineRemoveCB(
				 HRGN           hRgn, 
				 L_INT32        iStartRow, 
				 L_INT32        iStartCol, 
				 L_INT32        iLength, 
				 L_VOID         *pUserData )
 {
	try
	{
		L_TCHAR szMsg[200];

		wsprintf(
			szMsg, 
			TEXT("RowCol[%d,%d] Length[%d] %s\n"),
			iStartRow,
			iStartCol,
			iLength,
			(*((L_UINT *)pUserData) == LINEREMOVE_HORIZONTAL) ? "H":"V"

			);
		Line line;
		line.lStartRow = iStartRow;
		line.lStartCol = iStartCol;
		cout << szMsg;
		if ( hRgn != NULL )
		{
			CRgn rgnLine;
			rgnLine.Attach(hRgn);
			RECT rectLine;
			rgnLine.GetRgnBox( &rectLine );

			line.lTop = rectLine.top;
			line.lLeft = rectLine.left;
			line.lBottom = rectLine.bottom;
			line.lRight = rectLine.right;
			wsprintf(
				szMsg,
				TEXT("---> Top: %d Left: %d  Bottom: %d Right: %d\n"),
				rectLine.top,
				rectLine.left,
				rectLine.bottom,
				rectLine.right);
			cout << szMsg;
			DeleteObject(hRgn);
			vecLines.push_back (line);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17173");
	return SUCCESS_REMOVE; 
}

void ExampleLineRemove(pBITMAPHANDLE pBitmap, int iGapLength, int iMaxLineWidth, int iMinLineLength, L_UINT uFlags )
{
	L_INT32           nRet;
	LINEREMOVE        lr;

	try
	{
		memset(&lr,0,sizeof(LINEREMOVE));

		lr.uStructSize = sizeof(LINEREMOVE);
		lr.iGapLength     = iGapLength;
		lr.iMaxLineWidth  = iMaxLineWidth;
		lr.iMinLineLength = iMinLineLength;
		lr.iWall          = 30;
		lr.iMaxWallPercent = 15;
		lr.iVariance	 = 10;
		lr.uRemoveFlags   =  uFlags;
		lr.uFlags         = LINE_USE_GAP | LINE_USE_DPI | LINE_SINGLE_REGION | 
			LINE_USE_VARIANCE | LINE_CALLBACK_REGION;

		nRet = L_LineRemoveBitmap(
			pBitmap, 
			&lr,
			(LINEREMOVECALLBACK)(ExampleLineRemoveCB), 
			&uFlags                              
			);   
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17172");
}

IAttributePtr createAttributeFromLine(const Line& line, const string& strFileName, long nPageNumber)
{
	ILongRectanglePtr ipRect(CLSID_LongRectangle);
	ASSERT_RESOURCE_ALLOCATION("ELI17168", ipRect != NULL);

	IAttributePtr ipAttribute(CLSID_Attribute);
	ASSERT_RESOURCE_ALLOCATION("ELI17165", ipAttribute != NULL);

	ISpatialStringPtr ipString(CLSID_SpatialString);
	ASSERT_RESOURCE_ALLOCATION("ELI17166", ipString != NULL );

	IRasterZonePtr ipZone(CLSID_RasterZone);
	ASSERT_RESOURCE_ALLOCATION("ELI17167", ipZone != NULL );

	// Set the bounds of  the rectangle
	ipRect->SetBounds(line.lLeft, line.lTop, line.lRight, line.lBottom);

	// Create the zone from the rectangle
	ipZone->CreateFromLongRectangle(ipRect, nPageNumber);

	// Create zones vector
	IIUnknownVectorPtr ipZones(CLSID_IUnknownVector);
	ASSERT_RESOURCE_ALLOCATION("ELI17169", ipZones != NULL);

	// Put the zone for the line in the Zones vector
	ipZones->PushBack(ipZone);

	// Set the source doc name
	ipString->SourceDocName = strFileName.c_str();

	// create the string to put in the attribute
	string strText = "Top: " + asString(line.lTop) +
		" Left: " + asString(line.lLeft) +
		" Bottom: " + asString(line.lBottom) +
		" Right: " + asString(line.lRight);
	// Build a hybrid string
	ipString->BuildFromRasterZones(ipZones, strText.c_str());

	ipAttribute->Name = "Clue";
	ipAttribute->Value = ipString;
	return ipAttribute;
}

int _tmain(int argc, TCHAR* argv[], TCHAR* envp[])
{
	int nRetCode = 0;
	CoInitializeEx(NULL, COINIT_MULTITHREADED);
	try
	{

		// Load license file(s)
		LicenseManagement::sGetInstance().loadLicenseFilesFromFolder();


		// initialize MFC and print and error on failure
		if (!AfxWinInit(::GetModuleHandle(NULL), NULL, ::GetCommandLine(), 0))
		{
			// TODO: change error code to suit your needs
			_tprintf(_T("Fatal Error: MFC initialization failed\n"));
			nRetCode = 1;
		}
		else if (argc < 2 || argc > 7)
		{
			cout << "FindLines <File Name> [/Gn][/Wn][/L n]" << endl;
			cout << "	/Gn - n specifies the max gap length in 1/1000's of an inch(default=10)" << endl;
			cout << "	/Wn - n specifies the max line width in 1/1000's of an inch(default=10)" << endl;
			cout << "	/Ln - n specifies the min line length in 1/1000's of an inch(default=200)" << endl;
			cout << "	/H	- Find Horizontal lines (default if neither /h and /v)" << endl;
			cout << "	/V	- Find Vertical lines" << endl;

			nRetCode = 1;
		}
		else
		{
			HBITMAPLIST hFileBitmaps;
			FILEINFO fileInfo;
			fileInfo.uStructSize = sizeof( FILEINFO );
			fileInfo.Flags = 0;
			int nRet;

			L_UnlockSupport(L_SUPPORT_DOCUMENT, L_KEY_DOCUMENT);

			string strFileName = argv[1];
			int iGapLength = 10;
			int iMaxLineWidth = 10;
			int iMinLineLength = 200;
			bool bHorizontalLines = false;
			bool bVerticalLines = false;

			for(int i = 2; i < argc; i++)
			{
				string strOption = argv[i];
				makeUpperCase(strOption);
				if ( strOption.find("/G") != string::npos )
				{
					iGapLength = asLong(strOption.substr(2));
				}
				else  if ( strOption.find("/W") != string::npos )
				{
					iMaxLineWidth = asLong(strOption.substr(2));
				}
				else if (strOption.find("/L") != string::npos)
				{
					iMinLineLength = asLong(strOption.substr(2));
				}
				else if ( strOption.find("/H") != string::npos)
				{
					bHorizontalLines = true;
				}
				else if ( strOption.find("/V") != string::npos)
				{
					bVerticalLines = true;
				}
			}
			if ( !bVerticalLines && !bHorizontalLines )
			{
				bHorizontalLines = true;
			}
				

			// Load image
			nRet = L_LoadBitmapList( argv[1], &hFileBitmaps, 0, 0, 
				NULL, &fileInfo );

			IIUnknownVectorPtr ipAttributes(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI17171", ipAttributes != NULL );

			for ( int nPage = 0; nPage < fileInfo.TotalPages; nPage++ )
			{
				// Get the Page to modifiy
				BITMAPHANDLE hBitmap;
				nRet = L_GetBitmapListItem( hFileBitmaps, 0, &hBitmap, sizeof(BITMAPHANDLE));

				if ( bHorizontalLines )
				{
					ExampleLineRemove(&hBitmap, iGapLength, iMaxLineWidth, iMinLineLength, (LINEREMOVE_HORIZONTAL));
				}
				if ( bVerticalLines )
				{
					ExampleLineRemove(&hBitmap, iGapLength, iMaxLineWidth, iMinLineLength,LINEREMOVE_VERTICAL);
				}

				for each ( Line l in vecLines)
				{
					ipAttributes->PushBack(createAttributeFromLine(l, strFileName, nPage+1));
				}
			}

			ipAttributes->SaveTo((strFileName + ".voa").c_str(), VARIANT_TRUE);

			// release the memory associated with bitmap list
			L_DestroyBitmapList(hFileBitmaps);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17170");
	CoUninitialize();
	return nRetCode;
}
