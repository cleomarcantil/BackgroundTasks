using BackgroundWorksRunner.WorksRunner.Internal;
using System.Collections.Concurrent;

namespace BackgroundWorksRunner.WorksRunner;

public class WorkerManager : IDisposable
{
    private readonly WorkerToRunQueue _workersToRun = new();
    private readonly ConcurrentDictionary<string, WorkerToRun> _workers = new();
    private readonly WorkerExecutionInfo.ChangesWatcher _changesWatcher = new(200);

    public void Dispose()
        => _changesWatcher.Dispose();

    public void AddToRun(string name, Func<IWorkerContext> contextFactory, int startDelay = 0, int? repeatInterval = null)
        => _workersToRun.Add(name, contextFactory, startDelay, repeatInterval, _changesWatcher);

    public async Task Start(CancellationToken cancellationToken)
    {
        while (true)
        {
            await Task.Delay(200, cancellationToken);

            _workersToRun.ExtractAll(w =>
            {
                _workers.TryAdd(w.Key, w);
            });

            foreach (var worker in _workers.Values)
            {
                if (!worker.IsRunning && worker.NextStartTime is { } t && t <= DateTime.Now)
                {
                    worker.Start(
                        onComplete: (success) =>
                        {
                            if (worker.NextStartTime == null)
                                _workers.TryRemove(worker.Key, out var _);
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
        var worksStatus = _workers.Values
            .ToDictionary(x => x.Key, x => (x.Name, Status: x.GetStatusInfo()));

        async Task InvokeCallback()
        {
            var statusChanges = worksStatus.Values.Select(v => (v.Name, v.Status));

            await callback.Invoke(statusChanges);
        }

        await InvokeCallback();

        await _changesWatcher.Watch(async (changes) =>
        {
            foreach (var change in changes)
            {
                worksStatus[change.Key] = change.Value;
            }

            await InvokeCallback();
        }, cancellationToken);
    }

    public delegate Task StatusChanged(IEnumerable<(string Name, WorkRunnerStatusInfo Status)> statusChanges);
}

