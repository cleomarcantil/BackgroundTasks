using BackgroundWorksRunner;
using BackgroundWorksRunner.Workers;
using BackgroundWorksRunner.WorksRunner;

Console.WriteLine("WorksRunner");


BackgroundTasksRunner tasksRunner = new();

var trTask = tasksRunner.Start(CancellationToken.None);

var statusTask = tasksRunner.CaptureStatus(async (x) =>
{
    var tasksInfos = x
        .Select(w => $"- {w.Name} ({w.Status})");

    int l = 15;
    foreach (var taskInfo in tasksInfos)
    {
        ConsoleHelpers.WriteLine(taskInfo, l += 2, 1);
    }
}, CancellationToken.None);
 
tasksRunner.AddToRun("Serviço 1", new WorkRunner1(), 10_000);
tasksRunner.AddToRun("Serviço 2", new WorkRunner2(), 5_000, 10_000);
tasksRunner.AddToRun("Serviço 3", new WorkRunner3(), 3_000);

await trTask;
