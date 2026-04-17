using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using snus_klk1.model;

namespace snus_klk1.service
{
    public class JobCompletedEventArgs : EventArgs
    {
        public Guid JobId { get; }
        public int Result { get; }

        public JobCompletedEventArgs(Guid jobId, int result)
        {
            JobId = jobId;
            Result = result;
        }
    }

    public class JobFailedEventArgs : EventArgs
    {
        public Guid JobId { get; }
        public Exception Exception { get; }

        public JobFailedEventArgs(Guid jobId, Exception ex)
        {
            JobId = jobId;
            Exception = ex;
        }
    }

    internal class ProcessingSystem
    {
        private readonly ConcurrentPriorityQueue _queue;
        private readonly ConcurrentDictionary<Guid, bool> _seenJobs = new();
        private readonly int _workerCount;
        public event EventHandler<JobCompletedEventArgs>? JobCompleted;
        public event EventHandler<JobFailedEventArgs>? JobFailed;
        private readonly ConcurrentDictionary<Guid, JobRecord> _jobs = new();
        private readonly Reporter reporter;

        public ProcessingSystem(ConcurrentPriorityQueue queue, int workerCount, int reportDelaySeconds)
        {
            _queue = queue;
            _workerCount = workerCount;
            reporter = new(_jobs, reportDelaySeconds);
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
                        QueuedJob? queuedJob = _queue.Dequeue();
                        if (queuedJob == null) continue;

                        try
                        {
                            var start = DateTime.Now;

                            int result = await JobHandler.HandleJob(queuedJob.Job);

                            var duration = DateTime.Now - start;

                            _jobs[queuedJob.Job.Id] = new JobRecord
                            {
                                Job = queuedJob.Job,
                                Result = result,
                                Success = true,
                                Duration = duration
                            };
                            queuedJob.Tcs.TrySetResult(result);
                            JobCompleted?.Invoke(this, new JobCompletedEventArgs(queuedJob.Job.Id, result));
                        }
                        catch (Exception ex)
                        {
                            _jobs[queuedJob.Job.Id] = new JobRecord
                            {
                                Job = queuedJob.Job,
                                Result = null,
                                Success = false,
                                Duration = TimeSpan.Zero
                            };
                            queuedJob.Tcs.TrySetException(ex);
                            JobFailed?.Invoke(this,new JobFailedEventArgs(queuedJob.Job.Id, ex));
                        }
                    }
                });
            }
        }

        public Job GetJob(Guid id)
        {
            return _jobs.TryGetValue(id, out var record)
                ? record.Job
                : null;
        }

        public IEnumerable<Job> GetTopJobs(int n)
        {
            return this._queue.GetTopJobs(n);
        }


    }
}
