using System;
using System.Collections.Generic;
using System.Text;

namespace snus_klk1.model
{
    internal static class GlobalMutex
    {
        public static object consoleMutex = new();
    }
}
