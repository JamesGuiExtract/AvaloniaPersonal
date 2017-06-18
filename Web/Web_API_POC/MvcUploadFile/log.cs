using System;
using System.IO;

namespace MvcUploadFile
{
    public class Log
    {
        StreamWriter _log = null;
        static int count = 0;
        Object lockObject = new Object();

        public Log(string logPath)
        {
            // lock is here due to the use of static counter...
            lock (lockObject)
            {
                // The logPath from the appsettings json file has reversed slash characters in it, 
                // so reverse these now.
                if (String.IsNullOrWhiteSpace(logPath))
                {
                    return;
                }

                var path = logPath.Replace('/', '\\');
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                DateTime dt = DateTime.Now;
                string filename = $"Log_{dt.Hour}_{dt.Minute}_{dt.Second}_{count++}.txt";
                string pathFilename = Path.Combine(path, filename);
                var fs = new FileStream(path: pathFilename, mode: FileMode.CreateNew);
                _log = new StreamWriter(fs);
            }
        }

        public void WriteLine(string text)
        {
            _log.WriteLine(text);
        }

        public void Close()
        {
            _log.Dispose();     // note: no Close() method in .net core
        }

    }
}
