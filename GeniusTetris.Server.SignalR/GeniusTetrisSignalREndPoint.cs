using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace GeniusTetris.Server.SignalR
{
    /// <summary>
    /// Trop bas niveau
    /// </summary>
    public class GeniusTetrisSignalREndPoint : PersistentConnection 
    {
        protected override System.Threading.Tasks.Task OnConnected(IRequest request, string connectionId)
        {
            return base.OnConnected(request, connectionId);
        }

        protected override System.Threading.Tasks.Task OnDisconnected(IRequest request, string connectionId)
        {
            return base.OnDisconnected(request, connectionId);
        }

        protected override System.Threading.Tasks.Task OnReconnected(IRequest request, string connectionId)
        {
            return base.OnReconnected(request, connectionId);
        }

        protected override IList<string> OnRejoiningGroups(IRequest request, IList<string> groups, string connectionId)
        {
            return base.OnRejoiningGroups(request, groups, connectionId);
        }

        protected override System.Threading.Tasks.Task OnReceived(IRequest request, string connectionId, string data)
        {
            Debug.WriteLine("[Server]Received :");
            Debug.WriteLine(data);
            return base.OnReceived(request, connectionId, data);
        }
    }
}