#pragma once
#include "stdafx.h"

#include <LeadToolsLicenseRestrictor.h>

namespace Extract
{
	namespace Imaging
	{
		namespace Utilities
		{
			// Class to use for restricting 
			public ref class LeadtoolsGuard
			{
			public:
				LeadtoolsGuard();
				~LeadtoolsGuard();

			private:
				LeadToolsLicenseRestrictor* m_pRestrict;
			};
		}
	}
}

