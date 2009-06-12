
#pragma once

#include "stdafx.h"

struct RequiredInterfaces
{
	enum ENumRequiredInterfaces {kNumRequiredInterfaces = 2};

	const unsigned long ulCount;
	IID pIIDs[kNumRequiredInterfaces];

	RequiredInterfaces()
	:ulCount(kNumRequiredInterfaces)
	{
		pIIDs[0] = IID_IPersistStream;
		pIIDs[1] = IID_ICopyableObject;
	}
};

// global variable
static RequiredInterfaces gRequiredInterfaces;