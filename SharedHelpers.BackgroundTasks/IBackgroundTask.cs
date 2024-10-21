namespace SharedHelpers.BackgroundTasks;

public interface IBackgroundTask
{
    Task Execute(IBackgroundTaskStatusAccess s, CancellationToken cancellationToken);
}
