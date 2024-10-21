using System.Collections.Concurrent;

namespace SharedHelpers.BackgroundTasks.Helpers;

internal class IntervalChangesWatcher<T>(int interval) : IDisposable
{
    private readonly ConcurrentDictionary<string, T> _changes = new();
    private readonly ConcurrentDictionary<string, ChangedCallback> _watches = new();

    private object _changedLock = new();
    private DateTime? _lastChangedTime;

    private readonly object _watchTaskLock = new();
    private Task? _watchTask;
    private CancellationTokenSource _watchTaskToken = new();

    public void Dispose()
    {
        _watchTaskToken.Cancel();
        _watchTaskToken.Dispose();
    }

    public void NotifyChanged(string key, T value)
    {
        lock (_changedLock)
        {
            _changes.AddOrUpdate(key, (_) => value, (_, _) => value);
            _lastChangedTime = DateTime.Now;
        }
    }

    public async Task Watch(ChangedCallback changedCallback, CancellationToken cancellationToken)
    {
        var key = Guid.NewGuid().ToString();

        if (_watches.TryAdd(key, changedCallback))
        {
            lock (_watchTaskLock)
            {
                if (_watchTask is null)
                {
                    _watchTask = StartWatch(
                        onComplete: () =>
                        {
                            lock (_watchTaskLock)
                            {
                                _watchTask = null;
                            }
                        });
                }
            }
            
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(interval, cancellationToken);
            }

            _watches.TryRemove(key, out var _);
        }
    }

    private async Task StartWatch(Action onComplete)
    {
        while (!_watches.IsEmpty && !_watchTaskToken.IsCancellationRequested)
        {
            await Task.Delay(interval, _watchTaskToken.Token);

            KeyValuePair<string, T>[]? changesToNotify = null;

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
                foreach (var (_, callback) in _watches)
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

    public delegate Task ChangedCallback(KeyValuePair<string, T>[] changes);
}