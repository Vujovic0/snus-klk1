using System;
using System.Collections.Generic;
using System.Text;
using snus_klk1.model.enums;

namespace snus_klk1.model
{
    internal class Job
    {
        private Guid Id { set; get; }
        private JobType Type { set; get; }
        private string Payload { set; get; } = string.Empty;
        private int Priority { set; get; }
    }
}
