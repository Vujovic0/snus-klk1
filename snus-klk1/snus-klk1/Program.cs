using System;
using System.Threading.Tasks;
using snus_klk1.model;
using snus_klk1.service;

class Program
{
    static async Task Main(string[] args)
    {
        var queue = new ConcurrentPriorityQueue(100);
        var system = new ProcessingSystem(queue, workerCount: 4);
        system.JobCompleted += async (sender, e) =>
        {
            await Logger.LogAsync("COMPLETED", e.JobId, e.Result.ToString());
        };
        system.JobFailed += async (sender, e) =>
        {
            await Logger.LogAsync("ABORT", e.JobId, "FAILED");
        };
        var producer = new JobProducer(workerCount: 3, queue, system);
        producer.ParallelProduceJobs();
        Console.WriteLine("System running... Press ENTER to stop.\n");
        Console.ReadLine();
    }
}