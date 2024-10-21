namespace BackgroundWorksRunner.WorksRunner;

public interface IBackgroundTaskStatusAccess
{
    string Name { get; }

    void Update(string info, int? progress = null);

    BackgroundTaskStatus GetStatus();
}