using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace snus_klk1
{
    internal static class Config
    {
        public static readonly int allowedDelayMs = 2000;
        public static readonly int workerCount = 5;
        public static readonly int maxQueueSize = 100;
        public static readonly bool setupFromFile = true;
        public static readonly int reportCooldownSeconds = 60;
        public static readonly int primeRange = 1000000;
        public static readonly int ioRangeMs = 3000;
        public static readonly int priorityRange = 10;
    }
}
