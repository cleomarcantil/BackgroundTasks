using BackgroundWorksRunner.WorksRunner;

namespace BackgroundWorksRunner.Workers;

public class WorkRunner1 : IWorkRunner
{
    public async Task Execute(IWorkRunnerStatus s)
    {
        for (int n = 1; n <= 5; n++)
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss} Executando ({n}) {s.Name}");
            await Task.Delay(1000);

            s.UpdateStatusInfo($"{n} de 5", n * 20);
        }
    }
}
