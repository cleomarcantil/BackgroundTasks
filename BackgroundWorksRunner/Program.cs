using BackgroundWorksRunner.Workers;
using BackgroundWorksRunner.WorksRunner;

Console.WriteLine("WorksRunner");


WorkerManager wm = new();

var wmTask = wm.Start();

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

        Console.WriteLine(string.Join("\n", tasksInfos));
    }
});


wm.AddToRun<WorkRunner1>(15_000, 10_000);
wm.AddToRun<WorkRunner2>();
wm.AddToRun(new WorkRunner3());


await wmTask;

