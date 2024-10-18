using BackgroundWorksRunner.WorksRunner;

namespace BackgroundWorksRunner.Workers;

public class WorkRunner2 : IWorkRunner
{
    public async Task Execute(IWorkRunnerStatus s, CancellationToken cancellationToken)
    {
        Console.WriteLine($"{DateTime.Now:HH:mm:ss} Executando {s.Name}");
        long n = 0;

        while (true)
        {
            await Task.Delay(250);

            s.UpdateStatusInfo($"{n++}");
        }
    }
}
