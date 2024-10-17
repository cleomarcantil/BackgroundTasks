using BackgroundWorksRunner.WorksRunner;

namespace BackgroundWorksRunner.Workers;

public class WorkRunner2 : IWorkRunner
{
    public async Task Execute(IWorkRunnerStatus s)
    {
        while (true)
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss} Executando {s.Name}...");
            await Task.Delay(1000);

        }
    }
}
