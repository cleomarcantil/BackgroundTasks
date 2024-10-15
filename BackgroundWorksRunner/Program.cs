using BackgroundWorksRunner.Workers;
using BackgroundWorksRunner.WorksRunner;

Console.WriteLine("WorksRunner");


WorkerManager wm = new();

var wmTask = wm.Start();

wm.AddToRun<WorkRunner1>(15_000, 10_000);
wm.AddToRun<WorkRunner2>();
wm.AddToRun<WorkRunner3>(2_000, 3_000);


await wmTask;

