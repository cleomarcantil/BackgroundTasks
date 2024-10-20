using System.Collections.Concurrent;

namespace BackgroundWorksRunner.WorksRunner.Internal;

internal class WorkerToRunQueue
{
    private ConcurrentQueue<WorkerToRun> _tasksToRun = new();

    public void Add(string name, Func<IWorkerContext> contextFactory, int startDelay, int? repeatInterval, WorkerExecutionInfo.ChangesWatcher changesWatcher)
    {
        WorkerToRun wrItem = new(
            name, 
            contextFactory, 
            startTime: DateTime.Now.AddMilliseconds(startDelay), 
            repeatInterval, 
            changesWatcher);

        _tasksToRun.Enqueue(wrItem);
    }

    public void ExtractAll(Action<WorkerToRun> action)
    {
        if (!_tasksToRun.IsEmpty)
        {
            while (_tasksToRun.TryDequeue(out var w))
            {
                action.Invoke(w);
            }
        }
    }
}