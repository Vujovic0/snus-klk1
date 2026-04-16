using System;
using System.Collections.Generic;
using System.Text;

namespace snus_klk1.model
{
    internal class ConcurrentPriorityQueue
    {
        PriorityQueue<QueuedJob, int> Queue;
        SemaphoreSlim AccessSemaphore = new(1,1);
        SemaphoreSlim FreeSpaceCounterSemaphore;
        SemaphoreSlim QueueCounterSemaphore;

        public ConcurrentPriorityQueue(int maxSize)
        {
            Queue = new();
            FreeSpaceCounterSemaphore = new(maxSize, maxSize);
            QueueCounterSemaphore = new(0, maxSize);
        }

        public void Enqueue(QueuedJob job)
        {
            FreeSpaceCounterSemaphore.Wait();
            AccessSemaphore.Wait();
            try
            {
                Queue.Enqueue(job, job.Job.Priority);
                QueueCounterSemaphore.Release();
            }
            catch
            {
                FreeSpaceCounterSemaphore.Release();
            }
            finally
            {
                AccessSemaphore.Release();
            }
        }

        public QueuedJob? Dequeue()
        {
            QueueCounterSemaphore.Wait();
            AccessSemaphore.Wait();
            try
            {
                QueuedJob job = Queue.Dequeue();
                FreeSpaceCounterSemaphore.Release();
                return job;
            }
            catch
            {
                QueueCounterSemaphore.Release();
            }
            finally
            {
                AccessSemaphore.Release();
            }
            return null;
        }


    }
}
