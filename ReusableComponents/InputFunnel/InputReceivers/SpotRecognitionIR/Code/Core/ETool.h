
#pragma once

// the following enums describe the various tools available on the toolbar
enum ETool { kNone = 0, 
			 kOpenImage = 1,
			 kSave = 2,
			 kZoomWindow = 3, 
			 kZoomIn = 4, 
			 kZoomOut = 5, 
			 // Not used per P13 #3937 - WEL 11/21/06
			 kZoomExtents = 6,
			 kPan = 7,
			 kSelectText = 8,
			 kInactiveSelectText = 9,
			 kSetHighlightHeight = 10,
			 kEditZoneText = 11,
			 kDeleteEntities = 12,
			 kRecognizeTextInRectRegion = 13,
			 kRecognizeTextInPolyRegion = 14,
			 kOpenSubImgInWindow = 15,
			 kSelectRectText = 16,
			 kInactiveSelectRectText = 17,
			 kPrint = 18, 
			 kFitPage = 19, 
			 kFitWidth = 20,
			 kSelectHighlight = 21,
			 kSelectWordText = 32
			};
