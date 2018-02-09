using System;
using System.Threading;


namespace Extract.Utilities
{
    /// <summary>
    /// Class to support use of Cancellable Model between applications.
    /// This class was modified from
    /// https://codereview.stackexchange.com/questions/137969/interprocess-cancellationtoken
    /// </summary>
    public class NamedTokenSource : CancellationTokenSource
    {
        static readonly string Namespace = "3ED2EB02-23E0-4EDA-881A-E9B323CF0A48";

        public RegisteredWaitHandle RegisteredWaitHandle { get; private set; }

        public NamedTokenSource(string name)
            : this(new EventWaitHandle(false, EventResetMode.ManualReset, name + Namespace))
        {
        }

        public NamedTokenSource(EventWaitHandle handle)
        {
            try
            {
                Handle = handle;

                RegisteredWaitHandle = ThreadPool.RegisterWaitForSingleObject(handle,
                                           (s, to) =>
                                           {
                                               Cancel();
                                               ((NamedTokenSource)s).RegisteredWaitHandle?.Unregister(null);
                                           },
                                           this,
                                           -1,
                                           true);
                Token.Register(() => handle.Set());
            }
            catch (Exception ex)
            {

                throw ex.AsExtract("ELI45574");
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                RegisteredWaitHandle?.Unregister(null);
                Handle.Dispose();
            }
        }

        /// <summary>
        /// Handle to the object that when signaled indicates the Token for this source is canceled 
        /// </summary>
        EventWaitHandle Handle { get; }

    }
}
