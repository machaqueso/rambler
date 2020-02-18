using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Rambler.Web.Services
{
    public abstract class CustomBackgroundService : BackgroundService
    {
        private Task executingTask;
        private CancellationTokenSource stoppingCts = new CancellationTokenSource();

        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            if (stoppingCts.IsCancellationRequested)
            {
                stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            }

            // Store the task we're executing
            executingTask = ExecuteAsync(stoppingCts.Token);

            // If the task is completed then return it, this will bubble cancellation and failure to the caller
            if (executingTask.IsCompleted)
            {
                return executingTask;
            }

            // Otherwise it's running
            return Task.CompletedTask;
        }

        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            // Stop called without start
            if (executingTask == null)
            {
                return;
            }

            try
            {
                // Signal cancellation to the executing method
                stoppingCts.Cancel();
            }
            finally
            {
                // Wait until the task completes or the stop token triggers
                await Task.WhenAny(executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
            }

        }
    }
}