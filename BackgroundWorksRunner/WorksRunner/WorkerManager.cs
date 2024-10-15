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
                if (wrItem.RunningTask is null && (wrItem.NextStartTime is { } t && t <= DateTime.Now))
                {
                    var wr = CreateWorkerInstance(wrItem.WorkType)!;

                    wrItem.RunningTask = Task.Run(async () =>
                    {
                        var startTime = DateTime.Now;
                        wrItem.LastExecutionTime = (startTime, null);
                        wrItem.NextStartTime = (wrItem.RepeatInterval is { } interval) ? wrItem.NextStartTime.Value.AddMilliseconds(interval) : null;

                        try
                        {
                            await wr.Execute();
                        }
                        catch (Exception)
                        {
                            //...
                        }
                        finally
                        {
                            wrItem.LastExecutionTime = (startTime, DateTime.Now);

                            await Task.Delay(100);
                            wrItem.RunningTask = null;

                            if (wrItem.RepeatInterval == null)
                            {
                                works.TryRemove(wrItem.Key, out var _);
                            }
                        }
                    });
                }
            }
        }
    }

    private ConcurrentQueue<WorkRunnerItem> _tasksToRun = new();

    /// <summary>
    /// Adiciona uma tarefa para executar em background
    /// </summary>
    /// <typeparam name="T">Tipo que implementa IWorkRunner</typeparam>
    /// <param name="startDelay">Tempo de atraso até o início</param>
    /// <param name="repeatInterval">Intervalo para repetição</param>
    public void AddToRun<T>(int startDelay = 0, int? repeatInterval = null)
        where T : IWorkRunner
    {
        WorkRunnerItem workRunnerConfig = new(typeof(T), startDelay, repeatInterval);

        _tasksToRun.Enqueue(workRunnerConfig);
    }

}
