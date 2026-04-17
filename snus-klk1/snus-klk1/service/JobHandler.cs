using System;
using System.Collections.Generic;
using System.Text;
using snus_klk1.model;
using snus_klk1.model.enums;

namespace snus_klk1.service
{
    internal static class JobHandler
    {
        public static bool IsPrime(int number)
        {
            if (number < 2) return false;
            if (number == 2) return true;
            if (number % 2 == 0) return false;
            for (int i = 3; i*i <= number; i += 2){
                if (number % i == 0)
                {
                    return false;
                }
            }
            return true;
        }

        public static int SerialCountPrimes(int start, int end, CancellationToken ct)
        {
            int primesCount = 0;

            for (int i = start; i <= end; i++)
            {
                ct.ThrowIfCancellationRequested();
                if (IsPrime(i))
                {
                    primesCount++;
                }
            }

            return primesCount;
        }

        public static async Task<int> ParallelCountPrimes(int limit, int threadNum, CancellationToken ct)
        {
            threadNum = Math.Clamp(threadNum, 1, 8);
            int chunkSize = limit / threadNum;
            List<Task<int>> tasks = new();
            for (int i = 0; i < threadNum; i ++)
            {
                int start = i*chunkSize + 1;
                int end = (i+1 == threadNum) ? limit : (i + 1) * chunkSize;
                tasks.Add(Task.Run(() =>
                {
                    return SerialCountPrimes(start, end, ct);
                }, ct));
            }
            int[] results = await Task.WhenAll(tasks);
            int total = 0;

            foreach(int result in results){
                total += result;
            }

            return total;
        }

        public static Task<int> SimulateIO(int ms)
        {
            return Task.Run(() =>
            {
                Thread.Sleep(ms);
                return Random.Shared.Next(101);
            });
        }

        public static Dictionary<string, int> ParsePayload(string payload){
            Dictionary<string, int> result = new ();
            if (string.IsNullOrWhiteSpace(payload))
                return result;
            string[] parts = payload.Split(',');
            foreach (var part in parts)
            {
                string[] kvpair = part.Split(':');
                if (kvpair.Length != 2)
                    continue;
                string key = kvpair[0].Trim();
                int value = int.Parse(kvpair[1].Replace("_", ""));
                result[key] = value;
            }

            return result;
        }

        public static async Task<int> HandleJob(Job job, int allowedDelayMs)
        {
            Dictionary<string, int> payload = ParsePayload(job.Payload);
            int tries = 0;
            const int maxTries = 3;

            while (tries < maxTries)
            {
                tries++;

                using var cts = new CancellationTokenSource();
                Task<int> jobExecution = ExecuteJob(job, payload, cts.Token);
                Task timeout = Task.Delay(allowedDelayMs);

                Task completed = await Task.WhenAny(jobExecution, timeout);

                if (completed == jobExecution)
                {
                    return await jobExecution;
                }

                cts.Cancel();

                try
                {
                    await jobExecution;
                }
                catch (OperationCanceledException)
                {
                }
            }

            throw new TimeoutException("Job failed after 3 attempts");
        }

        private static async Task<int> ExecuteJob(Job job, Dictionary<string, int> payload, CancellationToken ct)
        {
            if (job.Type == JobType.PRIME)
            {
                return await ParallelCountPrimes(payload["numbers"], payload["threads"], ct);
            }
            else
            {
                return await SimulateIO(payload["delay"]);
            }
        }
    }
}
