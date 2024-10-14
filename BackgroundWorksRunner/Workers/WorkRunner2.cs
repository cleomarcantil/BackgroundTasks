using BackgroundWorksRunner.WorksRunner;

namespace BackgroundWorksRunner.Workers;

public class WorkRunner2 : IWorkRunner
{
    public async Task Execute()
    {
        while (true)
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss} Executando {(this as IWorkRunner).Name}...");
            await Task.Delay(1000);
        }
    }
}
