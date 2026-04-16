using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using snus_klk1.model;

namespace snus_klk1.service
{
    internal class ProcessingSystem
    {
        private readonly ConcurrentPriorityQueue _queue;
        private readonly ConcurrentDictionary<Guid, bool> _seenJobs = new();
        private readonly int _workerCount;

        public ProcessingSystem(ConcurrentPriorityQueue queue, int workerCount)
        {
            _queue = queue;
            _workerCount = workerCount;
            StartWorkers();
        }

        internal JobHandle Submit(Job job)
        {
            if (!_seenJobs.TryAdd(job.Id, true))
                throw new Exception($"Job {job.Id} already submitted.");

            var tcs = new TaskCompletionSource<int>();

            var queuedJob = new QueuedJob(job, tcs);

            _queue.Enqueue(queuedJob);

            return new JobHandle(job.Id, tcs.Task);
        }

        private void StartWorkers()
        {
            for (int i = 0; i < _workerCount; i++)
            {
                Task.Run(async () =>
                {
                    while (true)
                    {
                        QueuedJob? qj = _queue.Dequeue();
                        if (qj == null) continue;

                        try
                        {
                            int result = await JobHandler.HandleJob(qj.Job);
                            qj.Tcs.TrySetResult(result);
                        }
                        catch (Exception ex)
                        {
                            qj.Tcs.TrySetException(ex);
                        }
                    }
                });
            }
        }
    }
}
