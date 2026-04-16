using System;
using System.Collections.Generic;
using System.Text;

namespace snus_klk1.model
{
    internal class JobHandle
    {
        public Guid Id {private set; get; }
        public Task<int>  Result {private set; get; }

        public JobHandle(Guid id, Task<int> result)
        {
            this.Id = id;
            this.Result = result;
        }
    }
}
