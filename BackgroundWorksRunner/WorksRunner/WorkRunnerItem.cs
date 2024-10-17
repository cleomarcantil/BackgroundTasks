using System.ComponentModel;
using System.Reflection;

namespace BackgroundWorksRunner.WorksRunner;

internal record WorkRunnerItem(Type WorkType, int StartDelay, int? RepeatInterval, Func<IWorkRunner> OnGetInstance) : IWorkRunnerStatus
{
    public string Key { get; } = Guid.NewGuid().ToString();
    public string Name { get; } = WorkType.GetCustomAttributes<DescriptionAttribute>()?.FirstOrDefault()?.Description ?? WorkType.Name;

    public DateTime? NextStartTime { get; set; } = DateTime.Now.AddMilliseconds(StartDelay);

    public Task? RunningTask { get; set; }
    public (DateTime Start, DateTime? End)? LastExecutionTime { get; set; }

    #region StatusInfo

    private object _lockStatusInfo = new();
    private (string Info, int? Progress) _statusInfo { get; set; }

    public void UpdateStatusInfo(string info, int? progress)
    {
        lock (_lockStatusInfo)
        {
            _statusInfo = (info, progress);
        }
    }

    public (string Info, int? Progress) GetStatusInfo()
    {
        lock (_lockStatusInfo)
        {
            return (_statusInfo.Info, _statusInfo.Progress);
        }
    }

    #endregion
}
