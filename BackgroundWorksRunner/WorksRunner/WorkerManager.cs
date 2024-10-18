using System.Collections.Concurrent;

namespace BackgroundWorksRunner.WorksRunner;

public class WorkerManager(WorkRunnerContextFactory workRunnerContextFactory) : IWorkerManager
{
    private readonly ConcurrentDictionary<string, WorkRunnerItem> _works = new();

    public async Task Start(CancellationToken cancellationToken)
    {
        while (true)
        {
            await Task.Delay(200);

            if (!_tasksToRun.IsEmpty)
            {
                while (_tasksToRun.TryDequeue(out var w))
                {
                    _works.TryAdd(w.Key, w);
                }
            }

            foreach (var wrItem in _works.Values)
            {
                if (!wrItem.Running && (wrItem.NextStartTime is { } t && t <= DateTime.Now))
                {
                    wrItem.ExecuteTask(
                        onComplete: () =>
                        {
                            if (wrItem.NextStartTime == null)
                                _works.TryRemove(wrItem.Key, out var _);
                        }, cancellationToken);
                }
            }
        }
    }


    private ConcurrentQueue<WorkRunnerItem> _tasksToRun = new();

    public void AddToRun<T>(int startDelay = 0, int? repeatInterval = null)
        where T : IWorkRunner
    {
        var wrType = typeof(T);
        var wrContextFactory = () => workRunnerContextFactory.Invoke(wrType);

        AddWorRunnerItem(startDelay, repeatInterval, wrType, wrContextFactory);
    }

    public void AddToRun<T>(T instance, int startDelay = 0, int? repeatInterval = null)
        where T : IWorkRunner
    {
        var wrType = typeof(T);
        var runnerContextFactory = () => new WorkRunnerInstanceContext(instance);

        AddWorRunnerItem(startDelay, repeatInterval, wrType, runnerContextFactory);
    }

    private void AddWorRunnerItem(int startDelay, int? repeatInterval, Type wrType, Func<IWorkRunnerContext> wrContextFactory)
    {
        WorkRunnerItem wrItem = new(wrType, startDelay, repeatInterval, wrContextFactory);

        _tasksToRun.Enqueue(wrItem);
    }


    public (bool Running, string Info, int? Progress) GetStatusInfo(string key)
    {
        if (!_works.TryGetValue(key, out var wrItem))
            return (false, string.Empty, null);

        var (info, progress) = wrItem.GetStatusInfo();

        return (wrItem.Running, info, progress);
    }

    public IEnumerable<(string Key, string Name)> GetWorkers()
    {
        foreach (var wrItem in _works)
        {
            yield return (wrItem.Key, wrItem.Value.Name);
        }
    }


    class WorkRunnerInstanceContext(IWorkRunner instance) : IWorkRunnerContext
    {
        public void Dispose() { }
        public IWorkRunner GetInstance() => instance;
    }
}

public delegate IWorkRunnerContext WorkRunnerContextFactory(Type wrType);