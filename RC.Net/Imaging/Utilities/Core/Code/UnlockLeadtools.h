#pragma once
#include "stdafx.h"

namespace Extract
{
	namespace Imaging
	{
		namespace Utilities
		{
			public ref class UnlockLeadtools sealed
			{
			private:
				// Added to remove FxCop error - http://msdn.microsoft.com/en-us/ms182169.aspx
				// Microsoft.Design::CA1053 - Static holder types should not have constructors
				UnlockLeadtools(void) {};

			public:
				// This loads the license file for Leadtools
				static void UnlockLeadToolsSupport();

				// Attempts to unlock the document support toolkit, if document support
				// is not licensed and returnExceptionIfUnlicensed == true then an
				// unlicensed exception will be returned, otherwise the return value
				// will be null (and document support toolkit will not be unlocked).
				// If the document support toolkit is successfully unlocked the return value
				// will be null, if it is not unlocked then an exception stating it was not
				// unlocked will be returned
				static ExtractException^ UnlockDocumentSupport(bool returnExceptionIfUnlicensed);

				// Attempts to unlock the pdf read and write libraries
				// If returnExceptionIfUnlicensed is true and pdf support is not licensed
				// then an unlicensed exception will be returned, otherwise the return
				// value will be null (and pdf support will not be unlocked).
				// If support is successfully unlocked the return value will be null,
				// if pdf support is not unlocked an exception will be returned.
				static ExtractException^ UnlockPdfSupport(bool returnExceptionIfUnlicensed);
			};
		};
	};
};