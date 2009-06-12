
#pragma once

// the following enums describe the various tools available on the toolbar
enum ETool { kNone, 
			 kOpenImage,
			 kSave,
			 kZoomWindow, 
			 kZoomIn, 
			 kZoomOut, 
			 // Not used per P13 #3937 - WEL 11/21/06
			 kZoomExtents,
			 kPan,
			 kSelectText,
			 kInactiveSelectText,
			 kSetHighlightHeight,
			 kEditZoneText,
			 kDeleteEntities,
			 kRecognizeTextInRectRegion,
			 kRecognizeTextInPolyRegion,
			 kOpenSubImgInWindow,
			 kSelectRectText,
			 kInactiveSelectRectText,
			 kPrint, 
			 kFitPage, 
			 kFitWidth,
			 kSelectHighlight,
			};
