using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal class SystemNotifications
    {
        internal event EventHandler HostInfoUpdated;

        protected virtual void OnHostInfoUpdated(HostUpdatedEventArgs e)
        {
            EventHandler handler = HostInfoUpdated;
            handler?.Invoke(this, e);
        }

        internal void RaiseUpdateHostUpdatedEvent(HostInfo info)
        {
            var args = new HostUpdatedEventArgs();
            args.HostInfo = info;
            HostInfoUpdated?.Invoke(this, args);
        }
    }

    internal class HostUpdatedEventArgs : EventArgs
    {
        internal HostInfo HostInfo { get; set; }
    }
}
