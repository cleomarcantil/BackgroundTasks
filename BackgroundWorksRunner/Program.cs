using BackgroundWorksRunner.Workers;
using BackgroundWorksRunner.WorksRunner;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("WorksRunner");


WorkerManager wm = new(t => new WorkRunnerTypeContext(t));

var wmTask = wm.Start(CancellationToken.None);

var statusTask = wm.CaptureWorkersRunnerStatus(async (x) =>
{
    var tasksInfos = x
        .Select(w => $" - {w.Name} (running: {w.Status.Runing}, info: '{w.Status.Info}', progress: {w.Status.Progress})");

    Console.CursorTop = 10;
    Console.WriteLine(string.Join("\n", tasksInfos));
}, CancellationToken.None);

wm.AddToRun<WorkRunner1>(3_000);
wm.AddToRun<WorkRunner2>(10_000, 10_000);
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