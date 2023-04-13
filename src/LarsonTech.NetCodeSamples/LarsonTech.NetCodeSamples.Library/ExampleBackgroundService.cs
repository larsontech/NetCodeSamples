using Microsoft.Extensions.Hosting;
namespace LarsonTech.NetCodeSamples.Library;

/// <summary>
/// An example background service to demonstrate proper use of the IHostedService interface.
/// </summary>
public sealed class ExampleBackgroundService : IHostedService, IDisposable
{
    private Task? _workerTask;

    private readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();


    /// <summary>
    /// Called to start the service.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the start process has been aborted.
    /// This is only a token for this method, not for the long running processes.</param>
    public Task StartAsync(CancellationToken cancellationToken)
    {

        // Store the task we're executing
        _workerTask = DoSomething(_stoppingCts.Token);

        return _workerTask.IsCompleted ? _workerTask :
            Task.CompletedTask;
    }

    private async Task DoSomething(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
            await Task.Delay(10, token);
    }

    /// <summary>
    /// Triggered when the application host is performing a graceful shutdown.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // Stop called without start
        if (_workerTask == null)
        {
            return;
        }

        try
        {
            // Signal cancellation to the executing method
            _stoppingCts.Cancel();
        }
        finally
        {
            // Wait until the task completes or the stop token triggers (in 5 seconds)
            await Task.WhenAny(_workerTask, Task.Delay(Timeout.Infinite, cancellationToken)).ConfigureAwait(false);
        }
    }

    public void Dispose()
    {
        _stoppingCts.Cancel();
        _stoppingCts.Dispose();
    }
}