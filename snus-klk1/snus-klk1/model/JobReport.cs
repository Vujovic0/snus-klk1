using System;
using System.Collections.Generic;
using System.Text;
using snus_klk1.model.enums;

namespace snus_klk1.model
{
    internal class JobReport
    {
        public JobType JobType { get; set; }
        public int Count { get; set; }
        public double AvgDurationMs { get; set; }
        public int FailedCount { get; set; }
    }
}
