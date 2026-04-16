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
        ProcessingSystem system;

        public JobProducer(int workerCount, ConcurrentPriorityQueue queue)
        {
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
                        system.Submit(job);
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
            return new Job(type, payload, rnd.Next(10));
        }

        private string GeneratePrimePayload(Random rnd)
        {
            int numbers = rnd.Next(200000);
            int threads = rnd.Next(1, 9);
            return $"numbers:{numbers:N0},threads:{threads}".Replace(",", "_");
        }

        private string GenerateIOPayload(Random rnd)
        {
            int delay = rnd.Next(15000);

            return $"delay:{delay:N0}".Replace(",", "_");
        }
    }
}
