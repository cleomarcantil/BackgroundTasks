using BackgroundTasks.TestApp;
using BackgroundTasks.TestApp.BackgroundTasks;
using SharedHelpers.BackgroundTasks;

Console.WriteLine("BackgroundTasks App Test");


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

tasksRunner.AddToRun("Serviço 1", new Service1(), 10_000);
tasksRunner.AddToRun("Serviço 2", new Service2(), 5_000, 10_000);
tasksRunner.AddToRun("Serviço 3", new Service3(), 3_000);

await trTask;
