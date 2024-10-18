using BackgroundWorksRunner.WorksRunner;

namespace BackgroundWorksRunner.Workers;

public class WorkRunner1 : IWorkRunner
{
    public async Task Execute(IWorkRunnerStatus s, CancellationToken cancellationToken)
    {
        Console.WriteLine($"{DateTime.Now:HH:mm:ss} Executando {s.Name}");
        
        for (int n = 1; n <= 100; n += 2)
        {
            await Task.Delay(250);

            s.UpdateStatusInfo($"{n} de 100", n);
        }
    }
}
