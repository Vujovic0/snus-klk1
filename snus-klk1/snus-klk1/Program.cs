using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using snus_klk1;
using snus_klk1.model;
using snus_klk1.service;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            Console.WriteLine("System running... press ENTER to exit");
            var config = XMLParser.Parse("SystemConfig.xml");
            int workerCount = Config.workerCount;
            int maxQueueSize = Config.maxQueueSize;
            if (Config.setupFromFile)
            {
                workerCount = (int)config["WorkerCount"];
                maxQueueSize = (int)config["MaxQueueSize"];
            }
            List<Job> jobs = (List<Job>)config["Jobs"];
            var queue = new ConcurrentPriorityQueue(maxQueueSize);
            var system = new ProcessingSystem(queue, workerCount, Config.reportCooldownSeconds, Config.allowedDelayMs);
            system.JobCompleted += async (sender, e) =>
            {
                await Logger.LogAsync("COMPLETED", e.JobId, e.Result.ToString());
            };
            system.JobFailed += async (sender, e) =>
            {
                await Logger.LogAsync("ABORT", e.JobId, "FAILED");
            };
            foreach (var job in jobs)
            {
                system.Submit(job);
            }
            var producer = new JobProducer(workerCount, queue, system, Config.primeRange, Config.ioRangeMs, Config.priorityRange);
            system.StartWorkers();
            _ = Task.Run(() => producer.ParallelProduceJobs());
            Console.ReadLine();
        } catch (Exception e)
        {
            Console.WriteLine(e.StackTrace);
        }
        
    }
}