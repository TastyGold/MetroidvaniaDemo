using System;
using System.Collections.Generic;
using System.Text;

namespace ErrorLogging
{
    public static class ErrorLogger
    {
        public static void LogMessage(string message)
        {
            Console.WriteLine($"LOG: MSG: {message}");
        }

        public static void LogFileNotFound(string filePath)
        {
            Console.WriteLine($"LOG: FILEIO: [{filePath}] File not found");
        }
    }
}
