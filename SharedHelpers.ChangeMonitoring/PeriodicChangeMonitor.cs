using System.Collections.Concurrent;

namespace SharedHelpers.ChangeMonitoring;

public class PeriodicChangeMonitor<TKey, TValue>(int interval) : IDisposable
{
    private readonly ConcurrentDictionary<TKey, TValue> _changes = new();
    private readonly ConcurrentDictionary<string, ChangedCallback> _monitors = new();

    private object _changedLock = new();
    private DateTime? _lastChangedTime;

    private readonly object _monitoringTaskLock = new();
    private Task? _monitoringTask;
    private CancellationTokenSource _monitoringTaskToken = new();

    public void Dispose()
    {
        _monitoringTaskToken.Cancel();
        _monitoringTaskToken.Dispose();
    }

    public void NotifyChanged(TKey key, TValue value)
    {
        lock (_changedLock)
        {
            _changes.AddOrUpdate(key, (_) => value, (_, _) => value);
            _lastChangedTime = DateTime.Now;
        }
    }

    public async Task Monitore(ChangedCallback changedCallback, CancellationToken cancellationToken)
    {
        var monitorKey = Guid.NewGuid().ToString();

        if (_monitors.TryAdd(monitorKey, changedCallback))
        {
            lock (_monitoringTaskLock)
            {
                if (_monitoringTask is null)
                {
                    _monitoringTask = StartMonitoring(
                        onComplete: () =>
                        {
                            lock (_monitoringTaskLock)
                            {
                                _monitoringTask = null;
                            }
                        });
                }
            }
            
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(interval, cancellationToken);
            }

            _monitors.TryRemove(monitorKey, out var _);
        }
    }

    private async Task StartMonitoring(Action onComplete)
    {
        while (!_monitors.IsEmpty && !_monitoringTaskToken.IsCancellationRequested)
        {
            await Task.Delay(interval, _monitoringTaskToken.Token);

            KeyValuePair<TKey, TValue>[]? changesToNotify = null;

            lock (_changedLock)
            {
                if (_lastChangedTime is not null)
                {
                    changesToNotify = _changes.ToArray();
                    _changes.Clear();
                    _lastChangedTime = null;
                }
            }

            if (changesToNotify is { Length: > 0 })
            {
                foreach (var (_, callback) in _monitors)
                {
                    try
                    {
                        await callback.Invoke(changesToNotify ?? []);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        onComplete.Invoke();
    }

    public delegate Task ChangedCallback(KeyValuePair<TKey, TValue>[] changes);
}