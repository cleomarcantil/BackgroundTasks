namespace BackgroundWorksRunner.WorksRunner;

public class DefaultWorkRunnerStarter(int Delay = 0, int RepeatInterval = 0) : IWorkRunnerStarterCheck
{
    private DateTime _nextExecutionTime = DateTime.Now.AddMilliseconds(Delay);

    public bool CanStart(DateTime? lastStartTime, DateTime? lastEndTime)
    {
        if (RepeatInterval > 0 && lastEndTime is not null)
        {
            _nextExecutionTime = lastEndTime.Value.AddMilliseconds(RepeatInterval);
        }

        return (_nextExecutionTime <= DateTime.Now);
    }
}