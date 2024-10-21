using SharedHelpers.ChangeMonitoring;

namespace SharedHelpers.BackgroundTasks.Internal;

internal class BackgroundTaskExecutionInfo(
    string key, 
    string name, 
    BackgroundTaskExecutionInfo.ChangesMonitor changesMonitor) 
    : IBackgroundTaskStatusAccess
{
    private readonly object _lock = new();
    private BackgroundTaskStatus _statusInfo = new(false, string.Empty, null, null, null, null, 0);

    public string Key => key;
    public string Name => name;

    public void Reset()
    {
        lock (_lock)
        {
            _statusInfo = new(
                Running: true,
                Info: string.Empty,
                Progress: null,
                LastExecutionStartTime: DateTime.Now,
                LastExecutionEndTime: null,
                NextExecutionStartTime: null,
                ExecutionCount: _statusInfo.ExecutionCount + 1);

            changesMonitor.NotifyChanged(key, (name, _statusInfo));
        }
    }

    public void Finalize(DateTime? nextStartTime)
    {
        lock (_lock)
        {
            _statusInfo = _statusInfo with
            {
                Running = false,
                LastExecutionEndTime = DateTime.Now,
                NextExecutionStartTime = nextStartTime
            };

            changesMonitor.NotifyChanged(key, (name, _statusInfo));
        }
    }

    public void Update(string info, int? progress = null)
    {
        lock (_lock)
        {
            if (!_statusInfo.Running)
                return;

            _statusInfo = _statusInfo with
            {
                Info = info,
                Progress = progress
            };

            changesMonitor.NotifyChanged(key, (name, _statusInfo));
        }
    }

    public BackgroundTaskStatus GetStatus()
    {
        lock (_lock)
        {
            return _statusInfo;
        }
    }

    internal class ChangesMonitor(int interval) 
        : PeriodicChangeMonitor<string, (string Name, BackgroundTaskStatus Status)>(interval);
}