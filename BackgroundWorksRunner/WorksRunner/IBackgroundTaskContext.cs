namespace BackgroundWorksRunner.WorksRunner;

public interface IBackgroundTaskContext : IDisposable
{
    IBackgroundTask GetInstance();
}