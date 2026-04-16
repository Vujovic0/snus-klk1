using System;
using System.Collections.Generic;
using System.Text;

namespace snus_klk1.model
{
    internal class JobRecord
    {
        public Job Job { get; set; }
        public int? Result { get; set; }
        public bool Success { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
