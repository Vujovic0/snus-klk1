using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using snus_klk1.model;
using snus_klk1.service;

class Program
{
    static async Task Main(string[] args)
    {
        var queue = new ConcurrentPriorityQueue(100);
        var system = new ProcessingSystem(queue, workerCount: 4);

        List<JobHandle> handles = new();

        // ručno submitovanje nekoliko jobova
        for (int i = 0; i < 10; i++)
        {
            var job = new Job(
                i % 2 == 0 ? snus_klk1.model.enums.JobType.PRIME : snus_klk1.model.enums.JobType.IO,
                i % 2 == 0
                    ? "numbers:1000000,threads:4"
                    : "delay:1000",
                Priority: i % 5
            );

            var handle = system.Submit(job);
            handles.Add(handle);
        }

        Console.WriteLine("Jobs submitted...\n");

        // čekanje rezultata
        foreach (var handle in handles)
        {
            try
            {
                int result = await handle.Result;
                Console.WriteLine($"Job {handle.Id} result: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Job {handle.Id} failed: {ex.Message}");
            }
        }

        Console.WriteLine("\nAll jobs finished.");
    }
}