using System;
using System.Collections.Generic;
using System.Text;
using snus_klk1.model.enums;

namespace snus_klk1.model
{
    internal class Job
    {
        public Guid Id {private set; get; }
        public JobType Type {private set; get; }
        public string Payload {private set; get; } = string.Empty;
        public int Priority {private set; get; }

        public Job(JobType Type, string Payload, int Priority)
        {
            this.Id = Guid.NewGuid();
            this.Type = Type;
            this.Payload = Payload;
            this.Priority = Priority;
        }
    }
}
