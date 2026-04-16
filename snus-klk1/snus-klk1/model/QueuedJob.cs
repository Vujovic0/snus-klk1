using System;
using System.Threading.Tasks;

namespace snus_klk1.model
{
    internal class QueuedJob
    {
        public Job Job { get; }
        public TaskCompletionSource<int> Tcs { get; }

        public QueuedJob(Job job, TaskCompletionSource<int> tcs)
        {
            Job = job;
            Tcs = tcs;
        }
    }
}