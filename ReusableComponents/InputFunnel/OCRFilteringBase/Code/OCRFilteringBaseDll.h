#pragma once

#ifdef ExportOCRFilteringBaseDll
#define OCRFilteringBaseDLL _declspec(dllexport)
#else
#define OCRFilteringBaseDLL _declspec(dllimport)
#endif
