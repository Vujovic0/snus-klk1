using System;
using System.Collections.Generic;
using System.Text;
using snus_klk1.model;
using snus_klk1.model.enums;

namespace snus_klk1.service
{
    internal class JobProducer
    {
        private static readonly ThreadLocal<Random> Rnd =
            new(() => new Random(Guid.NewGuid().GetHashCode()));
        int WorkerCount = 1;
        ConcurrentPriorityQueue Queue;
        private readonly ProcessingSystem _system;

        public JobProducer(int workerCount, ConcurrentPriorityQueue queue, ProcessingSystem system)
        {
            _system = system;
            Queue = queue;
            WorkerCount = workerCount;
        }

        public void ParallelProduceJobs()
        {
            for (int i = 0; i < WorkerCount; i++)
            {
                Task.Run(() =>
                {
                    var rnd = Rnd.Value;

                    while (true)
                    {
                        var job = ProduceJob(rnd);
                        _system.Submit(job);
                    }
                });
            }
        }

        public Job ProduceJob(Random rnd)
        {
            JobType type = (JobType)rnd.Next(2);
            string payload = "";
            if (type == JobType.PRIME)
            {
                payload = GeneratePrimePayload(rnd);
            } else if (type == JobType.IO)
            {
                payload = GenerateIOPayload(rnd);
            }
            Job job = new Job(type, payload, rnd.Next(10));
            //LogGenerated(job);
            return job;
        }

        private string GeneratePrimePayload(Random rnd)
        {
            int numbers = rnd.Next(200000);
            int threads = rnd.Next(1, 9);
            return $"numbers:{numbers:N0}".Replace(",", "_") + $",threads:{threads}";
        }

        private string GenerateIOPayload(Random rnd)
        {
            int delay = rnd.Next(50, 3000);

            return $"delay:{delay:N0}".Replace(",", "_");
        }

        private void LogGenerated(Job job)
        {
            lock (GlobalMutex.consoleMutex)
            {
                Console.WriteLine(
                $"[{DateTime.Now}] [GENERATED] {job.Id}, {job.Type}, {job.Payload}"
            );
            }
        }
    }
}
