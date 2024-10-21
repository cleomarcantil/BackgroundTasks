using SharedHelpers.BackgroundTasks;

namespace BackgroundTasks.TestApp.BackgroundTasks;

public class Service1 : IBackgroundTask
{
    public async Task Execute(IBackgroundTaskStatusAccess s, CancellationToken cancellationToken)
    {
        const int l = 3;

        ConsoleHelpers.WriteLine($"{DateTime.Now:HH:mm:ss} Executando {s.Name}", l, 2);

        for (int n = 1; n <= 100; n += 1)
        {
            await Task.Delay(50);

            s.Update($"{n} de 100", n);
        }

        ConsoleHelpers.WriteLine($"{DateTime.Now:HH:mm:ss} {s.Name} Finalizado", l, 40);
    }
}