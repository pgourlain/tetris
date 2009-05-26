using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using GeniusTetris.Multiplayer.Cloud.Interfaces;

namespace GeniusTetris.Multiplayer.Cloud.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class AcceptorService : IAcceptorService
    {
        Func<PeerPlayer, Uri> _inner;
        public AcceptorService(Func<PeerPlayer, Uri> inner)
        {
            _inner = inner;
        }

        #region IAcceptorService Members

        public Uri IWouldLikeToplay(PeerPlayer player)
        {
            if (_inner != null)
                return _inner(player);
            return null;
        }

        #endregion
    }
}
