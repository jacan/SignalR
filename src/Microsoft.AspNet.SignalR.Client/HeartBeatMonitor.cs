using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Microsoft.AspNet.SignalR.Client
{
    public class HeartBeatMonitor
    {
        // Variable to keep track of whether the user has been notified
        private bool _userNotified;

        // Timer to determine when to notify the user and reconnect if required
        private Timer _timer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        public void Start(Connection connection)
        {
            connection.KeepAliveData.UpdateLastKeepAlive();
            _userNotified = false;
            _timer = new Timer(_ => Beat(connection), state: null, dueTime: connection.KeepAliveData.CheckInterval, period: connection.KeepAliveData.CheckInterval);
            //connection.Reconnected += connection_Reconnected;
        }

        /// <summary>
        /// 
        /// </summary>
        void connection_Reconnected()
        {            
            //what special thing happens after a reconnection
        }

        /// <summary>
        /// Kills the timer
        /// </summary>
        /// <param name="connection"></param>
        public void Stop(IConnection connection)
        {
            _timer.Dispose();
        }

        /// <summary>
        /// Callback funcion for the timer which determines if we need to notify the user or attempt to reconnect
        /// </summary>
        /// <param name="connection"></param>
        private void Beat(IConnection connection)
        {
            if (connection.State == ConnectionState.Connected)
            {
                //calculate the time elapsed since the lastKeepAlive heart beat
                TimeSpan timeElapsed = DateTime.UtcNow.Subtract(connection.KeepAliveData.lastKeepAlive);
                if (timeElapsed.CompareTo(connection.KeepAliveData.Timeout) >= 0)
                {
                    //connection has been lost
                    connection.OnReconnecting();
                    Console.WriteLine("Connection Timed-out : Reconnecting {0}", DateTime.UtcNow.ToLongTimeString());
                }

                else if (timeElapsed.CompareTo(connection.KeepAliveData.TimeoutWarning) >= 0)
                {
                    if (!_userNotified)
                    {
                        //inform user and set userNotified to true
                        _userNotified = true;
                        Console.WriteLine("Connection Timeout Warning : Notifying user {0}", DateTime.UtcNow.ToLongTimeString());
                    }
                }

                else
                {
                    _userNotified = false;
                }
            }
        }
    }
}
