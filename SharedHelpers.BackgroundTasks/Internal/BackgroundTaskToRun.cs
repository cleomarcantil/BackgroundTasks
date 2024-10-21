namespace SharedHelpers.BackgroundTasks.Internal;

internal class BackgroundTaskToRun
{
    private readonly string _key;
    private readonly string _name;
    private readonly Func<IBackgroundTaskContext> _contextFactory;
    private DateTime? _nextStartTime;
    private readonly BackgroundTaskExecutionInfo _executionInfo;
    private readonly int? _repeatInterval;
    private Task? _runningTask;

    public BackgroundTaskToRun(string name, Func<IBackgroundTaskContext> contextFactory, DateTime startTime, int? repeatInterval, BackgroundTaskExecutionInfo.ChangesWatcher changesWatcher)
    {
        _key = Guid.NewGuid().ToString("N");
        _name = name;
        _contextFactory = contextFactory;
        _nextStartTime = startTime;
        _executionInfo = new(_key, name, changesWatcher);
        _repeatInterval = repeatInterval;
    }

    public string Key => _key;
    public string Name => _name;
    public bool IsRunning => (_runningTask != null);
    public DateTime? NextStartTime => _nextStartTime;
    public BackgroundTaskStatus GetStatusInfo() => _executionInfo.GetStatus();

    public void Start(OnCompleteCallback onComplete, OnErrorCallback onError, CancellationToken cancellationToken)
    {
        _runningTask = Task.Run(async () =>
        {
            using var runnerContext = _contextFactory.Invoke();
            bool completed = false;
            _executionInfo.Reset();

            try
            {
                var bt = runnerContext.GetInstance();
                await bt.Execute(_executionInfo, cancellationToken);
                completed = true;
            }
            catch (Exception ex)
            {
                onError.Invoke(ex);
            }
            finally
            {
                var endTime = DateTime.Now;
                _nextStartTime = _repeatInterval is { } ri ? endTime.AddMilliseconds(ri) : null;
                _executionInfo.Finalize(_nextStartTime);
                _runningTask = null;
                onComplete.Invoke(completed);
            }
        }, cancellationToken);

    }

    internal delegate void OnCompleteCallback(bool success);
    internal delegate void OnErrorCallback(Exception ex);
}
