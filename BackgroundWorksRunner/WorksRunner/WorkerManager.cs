using BackgroundWorksRunner.WorksRunner.Helpers;
using BackgroundWorksRunner.WorksRunner.Internal;
using System.Collections.Concurrent;

namespace BackgroundWorksRunner.WorksRunner;

public class WorkerManager(WorkRunnerContextFactory workRunnerContextFactory) : IWorkerManager, IDisposable
{
    private readonly ConcurrentDictionary<string, WorkRunnerItem> _works = new();
    private readonly WorkRunnerItem.ChangesWatcher _changesWatcher = new(200);

    public void Dispose()
        => _changesWatcher.Dispose();

    public async Task Start(CancellationToken cancellationToken)
    {
        while (true)
        {
            await Task.Delay(200, cancellationToken);

            _tasksToRun.ExtractAll(w =>
            {
                _works.TryAdd(w.Key, w);
            });

            foreach (var wrItem in _works.Values)
            {
                if (!wrItem.Running && (wrItem.NextStartTime is { } t && t <= DateTime.Now))
                {
                    wrItem.StartTask(
                        onComplete: (success) =>
                        {
                            if (wrItem.NextStartTime == null)
                                _works.TryRemove(wrItem.Key, out var _);
                        },
                        onError: (ex) =>
                        {

                        },
                        cancellationToken);
                }
            }
        }
    }

    #region Add

    private WorkRunnerQueue _tasksToRun = new();

    public void AddToRun<T>(int startDelay = 0, int? repeatInterval = null) where T : IWorkRunner
        => _tasksToRun.Add(typeof(T), startDelay, repeatInterval,
            () => workRunnerContextFactory.Invoke(typeof(T)), _changesWatcher);

    public void AddToRun(IWorkRunner instance, int startDelay = 0, int? repeatInterval = null)
        => _tasksToRun.Add(instance.GetType(), startDelay, repeatInterval, 
            () => new WorkRunnerInstanceContext(instance), _changesWatcher);

    #endregion

    public async Task CaptureWorkersRunnerStatus(WorkRunnerStatusChanged callback, CancellationToken cancellationToken)
    {
        var worksStatus = _works.Values
            .ToDictionary(x => x.Key, x => x.GetExtendedStatusInfo());

        async Task InvokeCallback()
        {
            var statusChanges = worksStatus.Values.Select(v => (v.WRItem.Name, v.Status));

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

    class WorkRunnerInstanceContext(IWorkRunner instance) : IWorkRunnerContext
    {
        public void Dispose() { }
        public IWorkRunner GetInstance() => instance;
    }
}

public delegate IWorkRunnerContext WorkRunnerContextFactory(Type wrType);
