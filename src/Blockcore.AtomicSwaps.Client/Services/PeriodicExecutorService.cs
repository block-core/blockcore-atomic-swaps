using Blockcore.Utilities;
using System.Timers;

namespace Blockcore.AtomicSwaps.Client.Services
{
    public class PeriodicExecutorService : IDisposable
    {
        public event EventHandler<JobExecutedEventArgs> JobExecuted;
        void OnJobExecuted()
        {
            JobExecuted?.Invoke(this, new JobExecutedEventArgs());
        }

        System.Timers.Timer _Timer;
        bool _Running;

        public void StartExecuting(double interval)
        {
            if (!_Running)
            {
                // Initiate a Timer
                _Timer = new System.Timers.Timer();
                _Timer.Interval = interval;  // every 5 mins
                _Timer.Elapsed += HandleTimer;
                _Timer.AutoReset = true;
                _Timer.Enabled = true;

                _Running = true;
            }
        }
        void HandleTimer(object source, ElapsedEventArgs e)
        {
            // Execute required job

            // Notify any subscribers to the event
            OnJobExecuted();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_Running)
            {
                _Timer.Stop();
                _Timer.Dispose();
                _Running = false;

            }
        }
    }

}
