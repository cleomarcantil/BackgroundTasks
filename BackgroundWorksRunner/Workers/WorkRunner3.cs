using SharedHelpers.BackgroundTasks;

namespace BackgroundWorksRunner.Workers;

public class WorkRunner3 : IBackgroundTask
{
    public async Task Execute(IBackgroundTaskStatusAccess s, CancellationToken cancellationToken)
    {
        const int l = 5;

        ConsoleHelpers.WriteLine($"{DateTime.Now:HH:mm:ss} Executando {s.Name}", l, 2);

        int max = 200;

        for (int n = 1; n <= max; n++)
        {
            await Task.Delay(50);
            s.Update($"{n} de {max}");
        }

        ConsoleHelpers.WriteLine($"{DateTime.Now:HH:mm:ss} {s.Name} Finalizado", l, 40);
    }
}
