using System.ComponentModel;
using System.Reflection;

namespace BackgroundWorksRunner.WorksRunner;

internal class WorkRunnerItem(
    Type workType, 
    int startDelay, 
    int? repeatInterval, 
    Func<IWorkRunnerContext> wrContextFactory) : IWorkRunnerStatus
{
    public string Key { get; } = Guid.NewGuid().ToString();
    public string Name { get; } = workType.GetCustomAttributes<DescriptionAttribute>()?.FirstOrDefault()?.Description ?? workType.Name;

    public DateTime? NextStartTime { get; private set; } = DateTime.Now.AddMilliseconds(startDelay);
    public bool Running => (RunningTask != null);

    public Task? RunningTask { get; private set; }
    public (DateTime Start, DateTime? End)? LastExecutionTime { get; private set; }

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

    public void ExecuteTask(Action onComplete, CancellationToken cancellationToken)
    {
        RunningTask = Task.Run(async () =>
        {
            var startTime = DateTime.Now;
            LastExecutionTime = (startTime, null);

            using var runnerContext = wrContextFactory.Invoke();
            var wr = runnerContext.GetInstance();

            try
            {
                UpdateStatusInfo(string.Empty, null);
                await wr.Execute(this, cancellationToken);
            }
            catch (Exception)
            {
                // TODO: log...
            }
            finally
            {
                var endTime = DateTime.Now;
                LastExecutionTime = (startTime, endTime);
                NextStartTime = (repeatInterval is { } ri) ? endTime.AddMilliseconds(ri) : null;
                RunningTask = null;

                onComplete.Invoke();

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
}
