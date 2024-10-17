using System.ComponentModel;
using System.Reflection;

namespace BackgroundWorksRunner.WorksRunner;

internal record WorkRunnerItem(Type WorkType, int StartDelay, int? RepeatInterval, Func<IWorkRunnerContext> RunnerContextFactory) : IWorkRunnerStatus
{
    public string Key { get; } = Guid.NewGuid().ToString();
    public string Name { get; } = WorkType.GetCustomAttributes<DescriptionAttribute>()?.FirstOrDefault()?.Description ?? WorkType.Name;

    public DateTime? NextStartTime { get; private set; } = DateTime.Now.AddMilliseconds(StartDelay);

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

    public void ExecuteTask(Action onComplete)
    {
        RunningTask = Task.Run(async () =>
        {
            var startTime = DateTime.Now;
            LastExecutionTime = (startTime, null);
            NextStartTime = (RepeatInterval is { } ri) ? NextStartTime.Value.AddMilliseconds(ri) : null;

            using var runnerContext = RunnerContextFactory.Invoke();

            var wr = runnerContext.GetInstance();

            try
            {
                UpdateStatusInfo(string.Empty, null);
                await wr.Execute(this);
            }
            catch (Exception)
            {
                // TODO: log...
            }
            finally
            {
                LastExecutionTime = (startTime, DateTime.Now);
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
        });
    }
}

internal interface IWorkRunnerContext : IDisposable
{
    IWorkRunner GetInstance();
}