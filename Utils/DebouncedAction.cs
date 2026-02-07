using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace WinDash2.Utils;

/// <summary>
/// Provides debounced execution of actions with automatic cancellation handling.
/// </summary>
public class DebouncedAction(int delayMilliseconds = 500)
{
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly int _delayMilliseconds = delayMilliseconds;

    /// <summary>
    /// Executes the action after the specified delay. Cancels any previously pending execution.
    /// </summary>
    public async void Execute(Func<Task> action)
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;

        try
        {
            await Task.Delay(_delayMilliseconds, token);

            if (!token.IsCancellationRequested)
            {
                await action();
            }
        }
        catch (TaskCanceledException)
        {
            // Expected when debounce is cancelled
        }
        catch (ObjectDisposedException)
        {
            // Expected when disposed (e.g., navigating away)
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Debounced action failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Cancels any pending execution.
    /// </summary>
    public void Cancel()
    {
        try
        {
            _cancellationTokenSource?.Cancel();
        }
        catch
        {
            // Ignore cancellation errors
        }
    }
}
