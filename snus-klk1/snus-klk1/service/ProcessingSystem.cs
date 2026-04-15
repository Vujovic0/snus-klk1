using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using snus_klk1.model;

namespace snus_klk1.service
{
    internal class ProcessingSystem
    {
        PriorityQueue<Job, int> queue = new ();

        internal JobHandle Submit(Job job)
        {
            throw new NotImplementedException();
        }
    }
}
