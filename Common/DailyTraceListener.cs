using System;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace BenchMaestro
{
    public class DailyTraceListener : TraceListener
    {
        Thread thread = Thread.CurrentThread;
        private string _LogFileLocation = "";
        private DateTime _CurrentDate;
        StreamWriter _TraceWriter;

        public DailyTraceListener(string FileName)
        {
            _LogFileLocation = FileName;
            _TraceWriter = new StreamWriter(GenerateFileName(), false);
        }

        public override void Write(string message)
        {
            CheckRollover();
            _TraceWriter.Write(message);
        }

        public override void Write(string message, string category)
        {
            CheckRollover();
            _TraceWriter.Write(category + " " + message);
        }


        public override void WriteLine(string message)
        {
            CheckRollover();
            StringBuilder sb = new StringBuilder();
            sb.Append(DateTime.Now);
            sb.Append("##");
            sb.Append(Process.GetCurrentProcess().Id);
            sb.Append("##");
            sb.Append(String.Format("Background: {0}\n", thread.IsBackground) +
            String.Format("Thread Pool: {0}\n", thread.IsThreadPoolThread) +
            String.Format("Thread ID: {0}\n", thread.ManagedThreadId)); 
            sb.Append("##");
            sb.Append(message);
            _TraceWriter.WriteLine(sb.ToString());
        }

        public override void WriteLine(string message, string category)
        {
            CheckRollover();
            StringBuilder sb = new StringBuilder();
            sb.Append(DateTime.Now);
            sb.Append("##");
            sb.Append(Process.GetCurrentProcess().Id);
            sb.Append("##");
            sb.Append(String.Format("Background: {0}\n", thread.IsBackground) +
            String.Format("Thread Pool: {0}\n", thread.IsThreadPoolThread) +
            String.Format("Thread ID: {0}\n", thread.ManagedThreadId));
            sb.Append("##");
            sb.Append(category);
            sb.Append("##");
            sb.Append(message);
            _TraceWriter.WriteLine(sb.ToString());
        }

        private string GenerateFileName()
        {
            _CurrentDate = DateTime.Today;
            return Path.Combine(Path.GetDirectoryName(_LogFileLocation), Path.GetFileNameWithoutExtension(_LogFileLocation) + "_" + _CurrentDate.ToString("yyyyMMdd") + Path.GetExtension(_LogFileLocation));
        }

        private void CheckRollover()
        {
            if (_CurrentDate.CompareTo(DateTime.Today) != 0)
            {
                _TraceWriter.Close();
                _TraceWriter = new StreamWriter(GenerateFileName(), true);
            }
        }

        public override void Flush()
        {
            lock (this)
            {
                if (_TraceWriter != null)
                {
                    _TraceWriter.Flush();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _TraceWriter.Close();
            }
        }
    }
}
