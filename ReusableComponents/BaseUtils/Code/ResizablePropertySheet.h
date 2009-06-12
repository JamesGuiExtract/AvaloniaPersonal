
#pragma once

#include "BaseUtils.h"

//-------------------------------------------------------------------------------------------------
class EXPORT_BaseUtils ResizablePropertySheet : public CPropertySheet
{
public:
	void resize(const CRect& newClientRect);
};
//-------------------------------------------------------------------------------------------------
