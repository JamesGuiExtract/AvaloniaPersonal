
#pragma once

class UCLIDException;

#include <KernelAPI.h>

void loadScansoftRecErrInfo(UCLIDException& ue, RECERR rc);
//--------------------------------------------------------------------------------------------------
// PURPOSE: To add debug information about the current page size to a UCLIDException
void addPageSizeDebugInfo(UCLIDException& ue, const IMG_INFO& info);
void addPageSizeDebugInfo(UCLIDException& ue, HIMGFILE hImgFile, int iPageIndex);
//--------------------------------------------------------------------------------------------------

