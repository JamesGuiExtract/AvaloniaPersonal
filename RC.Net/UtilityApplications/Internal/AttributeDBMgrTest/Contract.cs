using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace AttributeDBMgrTest
{
    class Contract
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        static public void Assert( bool condition, string format, params object[] args )
        {
            if ( true == condition )
                return;

            StringBuilder step1 = new StringBuilder();
            StackFrame sf = new StackFrame(1, true);
            string name = sf.GetMethod().Name;
            step1.AppendFormat("From: {0}, {1}", name, format);

            StringBuilder step2 = new StringBuilder();
            step2.AppendFormat(step1.ToString(), args);

            InvalidOperationException ioex = new InvalidOperationException(step2.ToString());            
            throw ioex;
        }
    }
}
