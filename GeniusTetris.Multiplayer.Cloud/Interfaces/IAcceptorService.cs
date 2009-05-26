using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace GeniusTetris.Multiplayer.Cloud.Interfaces
{
    [ServiceContract(Namespace = Constants.ContractNamespace)]
    public interface IAcceptorService
    {
        [OperationContract]
        Uri IWouldLikeToplay(PeerPlayer player);
    }

    public interface IAcceptorServiceChannel : IAcceptorService, IClientChannel
    {
    }
}
