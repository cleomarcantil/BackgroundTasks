using System.Collections.Concurrent;

namespace SharedHelpers.BackgroundTasks.Internal;

internal class BackgroundTaskToRunQueue
{
    private ConcurrentQueue<BackgroundTaskToRun> _backgroundTasksToRun = new();

    public void Add(string name, Func<IBackgroundTaskContext> contextFactory, int startDelay, int? repeatInterval, BackgroundTaskExecutionInfo.ChangesMonitor changesMonitor)
    {
        BackgroundTaskToRun btToRun = new(
            name, 
            contextFactory, 
            startTime: DateTime.Now.AddMilliseconds(startDelay), 
            repeatInterval, 
            changesMonitor);

        _backgroundTasksToRun.Enqueue(btToRun);
    }

    public void ExtractAll(Action<BackgroundTaskToRun> action)
    {
        if (!_backgroundTasksToRun.IsEmpty)
        {
            while (_backgroundTasksToRun.TryDequeue(out var btToRun))
            {
                action.Invoke(btToRun);
            }
        }
    }
}