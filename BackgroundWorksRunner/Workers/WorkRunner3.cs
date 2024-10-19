using BackgroundWorksRunner.WorksRunner;

namespace BackgroundWorksRunner.Workers;

public class WorkRunner3 : IWorkRunner
{
    public static string Name => "Serviço 3";

    public async Task Execute(IWorkRunnerStatus s, CancellationToken cancellationToken)
    {
        Console.WriteLine($"{DateTime.Now:HH:mm:ss} Executando {Name}");
        int max = 200;

        for (int n = 1; n <= max; n++)
        {
            await Task.Delay(50);
            s.UpdateStatusInfo($"{n} de {max}");
        }
    }
}