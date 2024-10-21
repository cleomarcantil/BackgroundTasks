using BackgroundWorksRunner.WorksRunner.Internal;
using System.Collections.Concurrent;

namespace BackgroundWorksRunner.WorksRunner;

public class BackgroundTasksRunner : IDisposable
{
    private readonly BackgroundTaskToRunQueue _backgroundTasksToRun = new();
    private readonly ConcurrentDictionary<string, BackgroundTaskToRun> _backgroundTasks = new();
    private readonly BackgroundTaskExecutionInfo.ChangesWatcher _changesWatcher = new(200);

    public void Dispose()
        => _changesWatcher.Dispose();

    public void AddToRun(string name, IBackgroundTask instance, int startDelay = 0, int? repeatInterval = null, bool dispose = true)
        => _backgroundTasksToRun.Add(name, () => new BackgroundTaskInstance(instance, dispose), startDelay, repeatInterval, _changesWatcher);

    public void AddToRun(string name, Func<IBackgroundTaskContext> contextFactory, int startDelay = 0, int? repeatInterval = null)
        => _backgroundTasksToRun.Add(name, contextFactory, startDelay, repeatInterval, _changesWatcher);

    public async Task Start(CancellationToken cancellationToken)
    {
        while (true)
        {
            await Task.Delay(200, cancellationToken);

            _backgroundTasksToRun.ExtractAll(btToRun =>
            {
                _backgroundTasks.TryAdd(btToRun.Key, btToRun);
            });

            foreach (var bt in _backgroundTasks.Values)
            {
                if (!bt.IsRunning && bt.NextStartTime is { } t && t <= DateTime.Now)
                {
                    bt.Start(
                        onComplete: (success) =>
                        {
                            if (bt.NextStartTime == null)
                                _backgroundTasks.TryRemove(bt.Key, out var _);
                        },
                        onError: (ex) =>
                        {

                        },
                        cancellationToken);
                }
            }
        }
    }

    public async Task CaptureStatus(StatusChanged callback, CancellationToken cancellationToken)
    {
        var backgroundTasksStatus = _backgroundTasks.Values
            .ToDictionary(x => x.Key, x => (x.Name, Status: x.GetStatusInfo()));

        async Task InvokeCallback()
        {
            var statusChanges = backgroundTasksStatus.Values.Select(v => (v.Name, v.Status));

            await callback.Invoke(statusChanges);
        }

        await InvokeCallback();

        await _changesWatcher.Watch(async (changes) =>
        {
            foreach (var change in changes)
            {
                backgroundTasksStatus[change.Key] = change.Value;
            }

            await InvokeCallback();
        }, cancellationToken);
    }

    public delegate Task StatusChanged(IEnumerable<(string Name, BackgroundTaskStatus Status)> statusChanges);

    class BackgroundTaskInstance(IBackgroundTask instance, bool dispose) : IBackgroundTaskContext
    {
        public void Dispose()
        {
            if (dispose && instance is IDisposable d)
                d.Dispose();
        }

        public IBackgroundTask GetInstance() => instance;
    }
}

