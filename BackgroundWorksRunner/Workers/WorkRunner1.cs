using BackgroundWorksRunner.WorksRunner;

namespace BackgroundWorksRunner.Workers;

public class WorkRunner1 : IWorkRunner
{
    public static string Name => "Serviço 1";

    public async Task Execute(IWorkRunnerStatus s, CancellationToken cancellationToken)
    {
        Console.WriteLine($"{DateTime.Now:HH:mm:ss} Executando {Name}");
        
        for (int n = 1; n <= 100; n += 1)
        {
            await Task.Delay(50);

            s.UpdateStatusInfo($"{n} de 100", n);
        }
    }
}
