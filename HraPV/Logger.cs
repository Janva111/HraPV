using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HraPV
{
    public static class Logger
    {
        private static readonly string LogFile = "server.log";
        private static readonly object _lock = new();

        public static void Log(string message)
        {
            string entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
            Console.WriteLine(entry);

            _ = File.AppendAllTextAsync(LogFile, entry + Environment.NewLine);
        }
    }
}
