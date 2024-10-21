namespace BackgroundWorksRunner.WorksRunner;

public interface IBackgroundTask
{
    Task Execute(IBackgroundTaskStatusAccess s, CancellationToken cancellationToken);
}
