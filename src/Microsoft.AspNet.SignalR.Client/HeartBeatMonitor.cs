using System;
using System.Diagnostics;
using System.Threading;

namespace Microsoft.AspNet.SignalR.Client
{
    public class HeartBeatMonitor : IDisposable
    {
        // Variable to keep track of whether the user has been notified
        private bool _userNotified;

        // Timer to determine when to notify the user and reconnect if required
        private Timer _timer;

        private IConnection _connection;

        public HeartBeatMonitor(IConnection connection)
        {
            _connection = connection;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            _connection.KeepAliveData.UpdateLastKeepAlive();
            _userNotified = false;
            _timer = new Timer(_ => Beat(), state: null, dueTime: _connection.KeepAliveData.CheckInterval, period: _connection.KeepAliveData.CheckInterval);
        }
        
        /// <summary>
        /// Callback funcion for the timer which determines if we need to notify the user or attempt to reconnect
        /// </summary>
        private void Beat()
        {
            if (_connection.State == ConnectionState.Connected)
            {
                // Calculate the time elapsed since the lastKeepAlive heart beat
                TimeSpan timeElapsed = DateTime.UtcNow - _connection.KeepAliveData.LastKeepAlive;
                if (timeElapsed >= _connection.KeepAliveData.Timeout)
                {
                    // Connection has been lost
                    Debug.WriteLine("Connection Timed-out : Reconnecting {0}", DateTime.UtcNow.ToLongTimeString());

                    _connection.Transport.LostConnection(_connection);
                }
                else if (timeElapsed >= _connection.KeepAliveData.TimeoutWarning)
                {
                    if (!_userNotified)
                    {
                        // Inform user and set userNotified to true
                        _userNotified = true;
                        Debug.WriteLine("Connection Timeout Warning : Notifying user {0}", DateTime.UtcNow.ToLongTimeString());
                    }
                }
                else
                {
                    _userNotified = false;
                }
            }
        }

        public void Dispose()
        {
            if (_timer != null)
            {
                _timer.Dispose();
            }
        }
    }
}
