using BackgroundWorksRunner.Workers;
using BackgroundWorksRunner.WorksRunner;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("WorksRunner");


WorkerManager wm = new(t => new WorkRunnerTypeContext(t));

var wmTask = wm.Start(CancellationToken.None);

var statusTask = Task.Run(async () =>
{
    while (!wmTask.IsCompleted)
    {
        await Task.Delay(1000);
        var tasksInfos = wm.GetWorkers()
            .Select(w =>
            {
                var (running, info, progress) = wm.GetStatusInfo(w.Key);
                return $" * {w.Name} (running: {running}, info: '{info}', progress: {progress})";
            })
            .ToArray();

        Console.CursorTop = 10;
        //Console.Clear();
        Console.WriteLine(string.Join("\n", tasksInfos));
    }
});


wm.AddToRun<WorkRunner1>(10_000, 10_000);
wm.AddToRun<WorkRunner2>(5_000);
wm.AddToRun(new WorkRunner3(), 3_000);


await wmTask;


class WorkRunnerTypeContext(Type type) : IWorkRunnerContext
{
    public void Dispose() { }

    public IWorkRunner GetInstance()
        => (IWorkRunner)Activator.CreateInstance(type)!;
}

class WorkRunnerServiceScopeContext(Type type, IServiceScopeFactory serviceScopeFactory) : IWorkRunnerContext
{
    private readonly IServiceScope _serviceScope = serviceScopeFactory.CreateScope();

    public void Dispose()
        => _serviceScope.Dispose();

    public IWorkRunner GetInstance()
        => (IWorkRunner)_serviceScope.ServiceProvider.GetRequiredService(type);
}