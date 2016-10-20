// This is the main DLL file.

#include "stdafx.h"

// zlibExterns.h must be included before ZipWrapper or any .NET usings because of defines of Byte which
// are also defined in .net
#include <zlibExterns.h>
#include "ZipWrapper.h"
#include <intrin.h>

using namespace Extract;
using namespace System::Runtime::InteropServices;

#include <vector>
using namespace std;

namespace Extract {
    namespace Interop {
        namespace Zip
        {

            ManagedInflater::ManagedInflater(Stream ^input) :m_input(input)
            {

            }


            Stream ^ManagedInflater::InflateToStream()
            {
                try
                {
                    // get the length of the input stream.
					ExtractException::Assert("ELI41572", "Length must be less that max unsigned int.", m_input->Length < UINT_MAX);
                    unsigned int length = (unsigned int)m_input->Length;

                    // Create vector for the data from the stream that will be passed to the unmanaged code
                    vector<unsigned char> vecData(length);

					// Create Unmanaged memory stream using vecData
                    UnmanagedMemoryStream ^source = gcnew UnmanagedMemoryStream(&vecData[0], 0, length, FileAccess::Write);
					
					// Copy the input ot the source stream
                    m_input->CopyTo(source);

                    // Inflate the data
                    Inflater inflateBytes(vecData);
                    inflateBytes.Inflate();
                    vector<unsigned char> vecInflated = inflateBytes.GetDecompressedData();

                    // Create memory stream using the unmanaged inflated vector
                    UnmanagedMemoryStream ^uncompressed = gcnew UnmanagedMemoryStream(&vecInflated[0], vecInflated.size());

                    // Create the memory stream to return the data
                    MemoryStream ^returnStream = gcnew MemoryStream();

                    // Copy the inflated data to the return memory stream
                    uncompressed->CopyTo(returnStream);

                    return returnStream;
                }
                catch (Exception ^ex)
                {
                    throw ExtractException::AsExtractException("ELI41531", ex);
                }
            }

        }
    }
}
