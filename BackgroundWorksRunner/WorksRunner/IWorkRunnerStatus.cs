namespace BackgroundWorksRunner.WorksRunner;

public interface IWorkRunnerStatus
{
    void UpdateStatusInfo(string info, int? progress = null);

    WorkRunnerStatusInfo GetStatusInfo();
}

public record WorkRunnerStatusInfo(
    bool Runing,
    string Info,
    int? Progress,
    DateTime? LastExecutionStartTime,
    DateTime? LastExecutionEndTime,
    DateTime? NextExecutionTime);
