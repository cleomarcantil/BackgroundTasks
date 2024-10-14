namespace BackgroundWorksRunner.WorksRunner;

public interface IWorkRunnerStarterCheck
{
    bool CanStart(DateTime? lastStartTime, DateTime? lastEndTime);
}
