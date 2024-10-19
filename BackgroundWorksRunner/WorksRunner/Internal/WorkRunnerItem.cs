using BackgroundWorksRunner.WorksRunner.Helpers;

namespace BackgroundWorksRunner.WorksRunner.Internal;

internal class WorkRunnerItem : IWorkRunnerStatus
{
    private readonly int? _repeatInterval;
    private readonly Func<IWorkRunnerContext> _wrContextFactory;
    private readonly ChangesWatcher _changesWatcher;

    public WorkRunnerItem(
        string name,
        int startDelay,
        int? repeatInterval,
        Func<IWorkRunnerContext> wrContextFactory,
        ChangesWatcher changesWatcher)
    {
        _repeatInterval = repeatInterval;
        _wrContextFactory = wrContextFactory;
        _changesWatcher = changesWatcher;
        Key = Guid.NewGuid().ToString();
        Name = name;
        NextStartTime = DateTime.Now.AddMilliseconds(startDelay);

        var statusInfo = new WorkRunnerStatusInfo(false, string.Empty, null, null, null, null);
        _extendedStatusInfo = new(this, statusInfo);
    }

    public string Key { get; }
    public string Name { get; }

    public DateTime? NextStartTime { get; private set; }
    public bool Running => RunningTask != null;

    public Task? RunningTask { get; private set; }
    public (DateTime Start, DateTime? End)? LastExecutionTime { get; private set; }

    #region StatusInfo

    private object _statusInfoLock = new();
    private ExtendedStatusInfo _extendedStatusInfo;

    public void UpdateStatusInfo(string info, int? progress)
    {
        lock (_statusInfoLock)
        {
            var newStatusInfo = new WorkRunnerStatusInfo(
                Running, 
                info, 
                progress, 
                LastExecutionTime?.Start,
                LastExecutionTime?.End,
                NextStartTime);

            if (_extendedStatusInfo.Status != newStatusInfo)
            {
                _extendedStatusInfo = new(this, newStatusInfo);
                _changesWatcher.NotifyChanged(Key, _extendedStatusInfo);
            }
        }
    }

    public ExtendedStatusInfo GetExtendedStatusInfo()
    {
        lock (_statusInfoLock)
        {
            return _extendedStatusInfo;
        }
    }

    WorkRunnerStatusInfo IWorkRunnerStatus.GetStatusInfo() => GetExtendedStatusInfo().Status;

    #endregion

    public void StartTask(OnCompleteCallback onComplete, OnErrorCalback onError, CancellationToken cancellationToken)
    {
        RunningTask = Task.Run(async () =>
        {
            using var runnerContext = _wrContextFactory.Invoke();
            var wr = runnerContext.GetInstance();
            var startTime = DateTime.Now;
            bool completed = false;
            IWorkRunnerStatus s = this;

            try
            {
                LastExecutionTime = (startTime, null);
                UpdateStatusInfo(string.Empty, null);
                await wr.Execute(s, cancellationToken);
                completed = true;
            }
            catch (Exception ex)
            {
                onError.Invoke(ex);
            }
            finally
            {
                var endTime = DateTime.Now;
                LastExecutionTime = (startTime, endTime);
                NextStartTime = _repeatInterval is { } ri ? endTime.AddMilliseconds(ri) : null;
                RunningTask = null;
                var lastStatusInfo = GetExtendedStatusInfo().Status;
                UpdateStatusInfo(lastStatusInfo.Info, lastStatusInfo.Progress);

                onComplete.Invoke(completed);

                if (wr is IDisposable d)
                {
                    try
                    {
                        d.Dispose();
                    }
                    catch (Exception)
                    {
                        // TODO: log...
                    }
                }
            }
        }, cancellationToken);
    }

    public delegate void OnCompleteCallback(bool success);
    public delegate void OnErrorCalback(Exception ex);

    public record ExtendedStatusInfo(WorkRunnerItem WRItem, WorkRunnerStatusInfo Status);

    public class ChangesWatcher(int interval) : IntervalChangesWatcher<ExtendedStatusInfo>(interval);
}
