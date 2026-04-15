using System;
using System.Collections.Generic;
using System.Text;

namespace snus_klk1.model
{
    internal class JobHandle
    {
        private Guid Id { set; get; }
        private Task<int>  Result { set; get; }
    }
}
