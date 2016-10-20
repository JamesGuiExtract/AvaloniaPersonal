// Extract.Interop.Zip.h

#pragma once

using namespace System;
using namespace System::IO;

namespace Extract {
    namespace Interop {
        namespace Zip
        {
            // Class that wraps the Inflating of a stream that calls into the c++ ZipLibUtil
            public ref class ManagedInflater
            {
            public:
                // input is the stream with zipped data that needs to be inflated.
                ManagedInflater(Stream ^input);

                // Inflate the data to a stream
                Stream ^InflateToStream();

            private:
                // Stream to inflate
                Stream^ m_input;

            };
        }
    }
}
