
#pragma once
#include "stdafx.h"

#include <MiscLeadUtils.h>

namespace Extract
{
	namespace Imaging
	{
		namespace Utilities
		{
			using namespace System;

			public ref class PdfLock sealed
			{
			private:
				LeadToolsPDFLoadLocker* pLoadLocker;

				// Finalize method for the pdf locker to ensure the LoadLocker is deleted
				!PdfLock();

			public:
				PdfLock();

				// Initializes a new instance of the PdfLock class
				PdfLock(bool lock);

				// Dispose method for PdfLock
				~PdfLock();
			}; // End class PdfLock
		}; // End namespace Utilities
	}; // End namespace Imaging
}; // End namespace Extract