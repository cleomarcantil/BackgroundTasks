using BackgroundWorksRunner.Workers;
using BackgroundWorksRunner.WorksRunner;

Console.WriteLine("WorksRunner");


WorkerManager wm = new();

var wmTask = wm.Start();

wm.AddToRun<WorkRunner1>(new DefaultWorkRunnerStarter(15_000, 10_000));
wm.AddToRun<WorkRunner2>();
wm.AddToRun<WorkRunner3>(new HoraMinutoStarter(12, 37));


await wmTask;

record HoraMinutoStarter(int Hora, int Minuto) : IWorkRunnerStarterCheck
{
    public bool CanStart(DateTime? lastStartTime, DateTime? lastEndTime)
    {
        var now = DateTime.Now;

        return (now.Hour == Hora && now.Minute == Minuto);
    }
}