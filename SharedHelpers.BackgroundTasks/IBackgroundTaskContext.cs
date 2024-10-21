namespace SharedHelpers.BackgroundTasks;

public interface IBackgroundTaskContext : IDisposable
{
    IBackgroundTask GetInstance();
}