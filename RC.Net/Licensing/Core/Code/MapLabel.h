#pragma once

namespace Extract
{
	namespace Licensing
	{
		// This class serves no other purpose other than to provide an argument
		// to the LoadLicenseFilesFromFolder function so that it cannot be called
		// as a delegate or event handler.  This class should not be used anywhere
		// else in our code other than as an argument to the LoadLicenseFilesFromFolder
		// Added as per DotNetRCAndUtils #106 - JDS - 07/22/2008
		public ref class MapLabel sealed
		{
		public:
			MapLabel(void) {}
		}; // end class MapLabel
	}; // end namespace Licensing
}; // end namespace Extract
