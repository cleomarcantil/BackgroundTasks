using System.Collections.Concurrent;

namespace BackgroundWorksRunner.WorksRunner;

public class WorkerManager
{
    private IWorkRunner CreateWorkerInstance(Type workerType)
    {
        // TODO: Usar injeção de dependência
        var wr = (IWorkRunner)Activator.CreateInstance(workerType)!;

        return wr;
    }

    public async Task Start()
    {
        ConcurrentDictionary<string, WorkRunnerItem> works = new();

        while (true)
        {
            await Task.Delay(200);

            if (!_tasksToRun.IsEmpty)
            {
                while (_tasksToRun.TryDequeue(out var w))
                {
                    works.TryAdd(w.Key, w);
                }
            }

            foreach (var wrItem in works.Values)
            {
                if (wrItem.RunningTask is null && wrItem.StarterCheck.CanStart(wrItem.LastStartTime, wrItem.LastEndTime))
                {
                    var wr = CreateWorkerInstance(wrItem.WorkType)!;

                    wrItem.LastStartTime = DateTime.Now;
                    wrItem.LastEndTime = null;
                    wrItem.RunningTask = Task.Run(async () =>
                    {
                        try
                        {
                            await wr.Execute();
                        }
                        catch (Exception)
                        {
                            //throw;
                        }
                        finally
                        {
                            wrItem.LastEndTime = DateTime.Now;
                            await Task.Delay(100);
                            wrItem.RunningTask = null;
                        }
                    });
                }
            }

        }

    }

    private ConcurrentQueue<WorkRunnerItem> _tasksToRun = new();

    public void AddToRun<T>(IWorkRunnerStarterCheck? starterCheck = null)
        where T : IWorkRunner
    {
        starterCheck ??= new DefaultWorkRunnerStarter();

        WorkRunnerItem workRunnerConfig = new(typeof(T), starterCheck);

        _tasksToRun.Enqueue(workRunnerConfig);
    }

}
