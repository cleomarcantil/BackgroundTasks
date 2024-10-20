using BackgroundWorksRunner.Workers;
using BackgroundWorksRunner.WorksRunner;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("WorksRunner");


WorkerManager wm = new();

var wmTask = wm.Start(CancellationToken.None);

var statusTask = wm.CaptureStatus(async (x) =>
{
    var tasksInfos = x
        .Select(w => $" - {w.Name} (running: {w.Status.Runing}, info: '{w.Status.Info}', progress: {w.Status.Progress})");

    Console.CursorTop = 10;
    Console.WriteLine(string.Join("\n", tasksInfos));
}, CancellationToken.None);
 
wm.AddToRun("Serviço 1", () => new WorkerContextTypeFactory<WorkRunner1>(), 3_000);
wm.AddToRun("Serviço 2", () => new WorkerContextTypeFactory<WorkRunner2>(), 10_000, 10_000);
wm.AddToRun("Serviço 3", () => new WorkerContextTypeFactory<WorkRunner3>(), 3_000);

await wmTask;


class WorkerContextTypeFactory<T> : IWorkerContext
{
    public void Dispose() { }

    public IWorkRunner GetInstance()
        => (IWorkRunner)Activator.CreateInstance(typeof(T))!;
}

class WorkRunnerServiceScopeContext(Type type, IServiceScopeFactory serviceScopeFactory) : IWorkerContext
{
    private readonly IServiceScope _serviceScope = serviceScopeFactory.CreateScope();

    public void Dispose()
        => _serviceScope.Dispose();

    public IWorkRunner GetInstance()
        => (IWorkRunner)_serviceScope.ServiceProvider.GetRequiredService(type);
}