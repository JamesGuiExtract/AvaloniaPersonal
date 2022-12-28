using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extract.Utilities.WPF
{
    public class GlobalErrorHandler : IObserver<Exception>
    {
        private readonly string _eliCode;

        public GlobalErrorHandler(string eliCode)
        {
            _eliCode = eliCode;
        }

        public void OnNext(Exception value)
        {
            value.ExtractDisplay(_eliCode);
        }

        public void OnError(Exception error)
        {
            try
            {
                error.ExtractLog("ELI53779");
            }
            catch { }
        }

        public void OnCompleted() { }
    }
}
