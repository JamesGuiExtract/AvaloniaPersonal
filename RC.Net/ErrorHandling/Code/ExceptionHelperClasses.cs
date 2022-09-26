using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Extract.ErrorHandling
{
    //VM for the Error Details Display
    public class ExceptionStackVM
    {
        public Stack<ExceptionVM> ELIStack { get; set; }
    }

    //VM for the Exception section of the Error Details Display
    public class ExceptionVM
    {
        public string ELICode { get; set; }
        public string ELIMessage { get; set; }
        public string DebugDisplay { get; set; }
        public string StackTraceDisplay { get; set; }
        public int Index { get; set; }
        public IList<DebugVM> DebugInfo { get; set; }
        public IList<StackTraceVM> StackTraceLines { get; set; }
    }

    //VM for the Debug Data section of the Error Details Display
    public class DebugVM
    {
        public string Key { get; set; }
        public object Value { get; set; }
        public Type Type { get; set; }
    }

    //VM for the Stack Trace section of the Error Details Display
    public class StackTraceVM
    {
        public string Line { get; set; }
    }
}
