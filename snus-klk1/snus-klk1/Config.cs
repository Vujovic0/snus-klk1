using System;
using System.Collections.Generic;
using System.Text;

namespace snus_klk1
{
    internal static class Config
    {
        public static int allowedDelayMs = 2000;
        public static int workerCount = 5;
        public static int maxQueueSize = 100;
        public static bool setupFromFile = true;
    }
}
