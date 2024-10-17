using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace BackgroundWorksRunner.WorksRunner;

public class WorkerManager : IWorkerManager
{
    private readonly ConcurrentDictionary<string, WorkRunnerItem> _works = new();

    public async Task Start()
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
                if (wrItem.RunningTask is null && (wrItem.NextStartTime is { } t && t <= DateTime.Now))
                {
                    wrItem.ExecuteTask(
                        onComplete: () =>
                        {
                            if (wrItem.NextStartTime == null)
                                _works.TryRemove(wrItem.Key, out var _);
                        });
                }
            }
        }
    }


    private ConcurrentQueue<WorkRunnerItem> _tasksToRun = new();

    public void AddToRun<T>(int startDelay = 0, int? repeatInterval = null)
        where T : IWorkRunner
    {
        var wrType = typeof(T);
        var runnerContextFactory = () => new WorkRunnerTypeScope(wrType);

        AddWorRunnerItem(startDelay, repeatInterval, wrType, runnerContextFactory);
    }

    public void AddToRun<T>(T instance, int startDelay = 0, int? repeatInterval = null)
        where T : IWorkRunner
    {
        var wrType = typeof(T);
        var runnerContextFactory = () => new WorkRunnerInstanceContext(instance);

        AddWorRunnerItem(startDelay, repeatInterval, wrType, runnerContextFactory);
    }

    private void AddWorRunnerItem(int startDelay, int? repeatInterval, Type wrType, Func<IWorkRunnerContext> runnerContextFactory)
    {
        WorkRunnerItem wrItem = new(wrType, startDelay, repeatInterval, runnerContextFactory);

        _tasksToRun.Enqueue(wrItem);
    }


    public (bool Running, string Info, int? Progress) GetStatusInfo(string key)
    {
        if (!_works.TryGetValue(key, out var wrItem))
            return (false, string.Empty, null);

        bool runing = (wrItem.RunningTask != null);
        var (info, progress) = wrItem.GetStatusInfo();

        return (runing, info, progress);
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

    class WorkRunnerTypeScope(Type type) : IWorkRunnerContext
    {
        public void Dispose() { }

        public IWorkRunner GetInstance()
            => (IWorkRunner)Activator.CreateInstance(type)!;
    }

    class WorkRunnerDIScope(Type type, IServiceScopeFactory serviceScopeFactory) : IWorkRunnerContext
    {
        private readonly IServiceScope _serviceScope = serviceScopeFactory.CreateScope();

        public void Dispose()
            => _serviceScope.Dispose();

        public IWorkRunner GetInstance()
            => (IWorkRunner)_serviceScope.ServiceProvider.GetRequiredService(type);
    }
}
