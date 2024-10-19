using System.Collections.Concurrent;

namespace BackgroundWorksRunner.WorksRunner.Internal;

internal class WorkRunnerQueue
{
    private ConcurrentQueue<WorkRunnerItem> _tasksToRun = new();

    public void Add(Type wrType, int startDelay, int? repeatInterval, Func<IWorkRunnerContext> wrContextFactory, WorkRunnerItem.ChangesWatcher changesWatcher)
    {
        var name = wrType.GetProperty(nameof(IWorkRunner.Name))
            ?.GetValue(null)
            ?.ToString()
            ?? wrType.Name;

        WorkRunnerItem wrItem = new(name, startDelay, repeatInterval, wrContextFactory, changesWatcher);

        _tasksToRun.Enqueue(wrItem);
    }

    public void ExtractAll(Action<WorkRunnerItem> action)
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