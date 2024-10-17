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
                    RunWorkItem(wrItem);
                }
            }
        }
    }


    private void RunWorkItem(WorkRunnerItem wrItem)
    {
        wrItem.RunningTask = Task.Run(async () =>
        {
            var startTime = DateTime.Now;
            wrItem.LastExecutionTime = (startTime, null);
            wrItem.NextStartTime = (wrItem.RepeatInterval is { } interval) ? wrItem.NextStartTime.Value.AddMilliseconds(interval) : null;

            var wr = wrItem.OnGetInstance();

            try
            {
                wrItem.UpdateStatusInfo(string.Empty, null);
                await wr.Execute(wrItem);
            }
            catch (Exception)
            {
                // TODO: log...
            }
            finally
            {
                wrItem.LastExecutionTime = (startTime, DateTime.Now);
                wrItem.RunningTask = null;

                if (wrItem.RepeatInterval == null)
                {
                    _works.TryRemove(wrItem.Key, out var _);
                }

                if (wr is IDisposable d)
                {
                    try
                    {
                        d.Dispose();
                    }
                    catch (Exception)
                    {
                        // TODO: log...
                    }
                }
            }
        });
    }

    private ConcurrentQueue<WorkRunnerItem> _tasksToRun = new();

    public void AddToRun<T>(int startDelay = 0, int? repeatInterval = null)
        where T : IWorkRunner
    {
        var wrType = typeof(T);

        var getInstance = () =>
        {
            // TODO: Usar injeção de dependência
            return (IWorkRunner)Activator.CreateInstance(wrType)!;
        };

        WorkRunnerItem workRunnerConfig = new(wrType, startDelay, repeatInterval, getInstance);

        _tasksToRun.Enqueue(workRunnerConfig);
    }

    public void AddToRun<T>(T instance, int startDelay = 0, int? repeatInterval = null)
        where T : IWorkRunner
    {
        WorkRunnerItem workRunnerConfig = new(typeof(T), startDelay, repeatInterval, () => instance);

        _tasksToRun.Enqueue(workRunnerConfig);
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
}
