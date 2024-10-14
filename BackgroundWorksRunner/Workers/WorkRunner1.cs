using BackgroundWorksRunner.WorksRunner;

namespace BackgroundWorksRunner.Workers;

public class WorkRunner1 : IWorkRunner
{
    public async Task Execute()
    {
        for (int n = 1; n <= 5; n++)
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss} Executando ({n}) {(this as IWorkRunner).Name}");
            await Task.Delay(1000);
        }
    }
}
