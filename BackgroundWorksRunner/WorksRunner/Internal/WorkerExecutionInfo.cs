using BackgroundWorksRunner.WorksRunner.Helpers;

namespace BackgroundWorksRunner.WorksRunner.Internal;

internal class WorkerExecutionInfo(string key, string name, WorkerExecutionInfo.ChangesWatcher changesWatcher) : IWorkRunnerStatus
{
    private readonly object _lock = new();
    private WorkRunnerStatusInfo _statusInfo = new(false, string.Empty, null, null, null, null);

    public string Name => name;

    public void Reset()
    {
        lock (_lock)
        {
            _statusInfo = new(
                Runing: true,
                Info: string.Empty,
                Progress: null,
                LastExecutionStartTime: DateTime.Now,
                LastExecutionEndTime: null,
                NextExecutionStartTime: null);
        }
    }

    public void Finalize(DateTime? nextStartTime)
    {
        lock (_lock)
        {
            _statusInfo = _statusInfo with
            {
                Runing = false,
                LastExecutionEndTime = DateTime.Now,
                NextExecutionStartTime = nextStartTime
            };
        }
    }

    public void UpdateStatusInfo(string info, int? progress = null)
    {
        lock (_lock)
        {
            if (!_statusInfo.Runing)
                return;

            _statusInfo = _statusInfo with
            {
                Info = info,
                Progress = progress
            };

            changesWatcher.NotifyChanged(key, (name, _statusInfo));
        }
    }

    public WorkRunnerStatusInfo GetStatusInfo()
    {
        lock (_lock)
        {
            return _statusInfo;
        }
    }

    internal class ChangesWatcher(int interval) : IntervalChangesWatcher<(string Name, WorkRunnerStatusInfo Status)>(interval);
}