using KeePassLib.Interfaces;
using System;

namespace OnePasswordImporterPluginTest
{
    class ConsoleLogger : IStatusLogger
    {
        private string _strOperation;

        public bool ContinueWork()
        {
            return true;
        }

        public bool SetProgress(uint uPercent)
        {
            Console.WriteLine($"Progress: {uPercent}");
            return true;
        }

        public bool SetText(string strNewText, LogStatusType lsType)
        {
            Console.WriteLine($"{lsType} {strNewText}");
            return true;
        }

        public void StartLogging(string strOperation, bool bWriteOperationToLog)
        {
            _strOperation = strOperation;
            Console.WriteLine($"[START] {_strOperation}");
        }

        public void EndLogging()
        {
            if (string.IsNullOrEmpty(_strOperation))
                Console.WriteLine($"[END]");
            else
                Console.WriteLine($"[END] {_strOperation}");
        }
    }
}
