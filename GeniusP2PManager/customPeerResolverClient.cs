//  Copyright (c) Microsoft Corporation.  All Rights Reserved.
using System;

using System.ServiceModel.Channels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Configuration;
using System.ServiceModel.PeerResolvers;


// Custom peer resolver client-side implementation. Derives from PeerResolver and delegates the actual
// registration, unregistration, and resolution to the resolver service (which is a separate application).

// If you are unfamiliar with new concepts used in this sample, refer to the Indigo Basic\GettingStarted sample.
//Repris des exemple fourni par le SDK
namespace GeniusP2PManager
{
    // CustomPeerResolver Service contract. It is manually generated since svcutil currently does not
    // support WSDL generation when EndpointAddress[] is used in the service contract.
    [ServiceContract(Namespace = Constants.NameSpaceContract)]
    public interface ICustomPeerResolver
    {
        [OperationContract]
        int Register(string meshId, PeerNodeAddress nodeAddresses);
        [OperationContract]
        void Unregister(int registrationId);
        [OperationContract]
        void Update(int registrationId, PeerNodeAddress updatedNodeAddress);
        [OperationContract]
        PeerNodeAddress[] Resolve(string meshId, int maxAddresses);
    }

    public interface ICustomPeerResolverChannel : ICustomPeerResolver, IClientChannel
    {
    }

    public class CustomPeerResolver : PeerResolver
    {
        const string config = "CustomPeerResolverEndpoint";

        private string _peerAddress;

        public CustomPeerResolver(string peerAddress)
        {
            Console.WriteLine("CustomPeerResolver created !");
            _peerAddress = peerAddress;
        }

        // Allow the clients to share referals
        public override bool CanShareReferrals
        {
            get { return true; }
        }

        // Register address for a node participating in a mesh identified by meshId with the resolver service
        public override object Register(string meshId, PeerNodeAddress nodeAddress, TimeSpan timeout)
        {
            using (ChannelFactory<ICustomPeerResolverChannel> factory = CreaeChannelFactory())
            {
                using (ICustomPeerResolverChannel client = factory.CreateChannel())
                {
                    MaskScopeId(nodeAddress.IPAddresses);
                    int registrationId = client.Register(meshId, nodeAddress);
                    return registrationId;
                }
            }
        }

        // Unregister address for a node from the resolver service
        public override void Unregister(object registrationId, TimeSpan timeout)
        {
            try
            {
                EndpointAddress endpoint = new EndpointAddress(_peerAddress);
                using (ChannelFactory<ICustomPeerResolverChannel> factory = new ChannelFactory<ICustomPeerResolverChannel>(config, endpoint))
                {
                    using (ICustomPeerResolverChannel client = factory.CreateChannel())
                    {
                        client.Unregister((int)registrationId);
                    }
                }
            }

            catch { }

        }

        // Updates a node's registration with the resolver service.
        public override void Update(object registrationId, PeerNodeAddress updatedNodeAddress, TimeSpan timeout)
        {
            using (ChannelFactory<ICustomPeerResolverChannel> factory = CreaeChannelFactory())
            {
                using (ICustomPeerResolverChannel client = factory.CreateChannel())
                {
                    MaskScopeId(updatedNodeAddress.IPAddresses);
                    client.Update((int)registrationId, updatedNodeAddress);
                }
            }
        }

        // Query the resolver service for addresses associated with a mesh ID
        public override ReadOnlyCollection<PeerNodeAddress> Resolve(string meshId, int maxAddresses, TimeSpan timeout)
        {
            PeerNodeAddress[] addresses = null;

            using (ChannelFactory<ICustomPeerResolverChannel> factory = CreaeChannelFactory())
            {
                using (ICustomPeerResolverChannel client = factory.CreateChannel())
                {
                    addresses = client.Resolve(meshId, maxAddresses);
                }
            }

            // If addresses couldn't be obtained, return empty collection
            if (addresses == null)
                addresses = new PeerNodeAddress[0];

            return new ReadOnlyCollection<PeerNodeAddress>(addresses);
        }

        // Since we send the IP addresses to the service, mask the scope ID (scopeIDs only have local significance)
        void MaskScopeId(ReadOnlyCollection<IPAddress> ipAddresses)
        {
            foreach (IPAddress address in ipAddresses)
            {
                if (address.AddressFamily == AddressFamily.InterNetworkV6)
                    address.ScopeId = 0;
            }
        }

        private ChannelFactory<ICustomPeerResolverChannel> CreaeChannelFactory()
        {
            EndpointAddress endpoint = new EndpointAddress(_peerAddress);
            BasicHttpBinding binding = new BasicHttpBinding();
            //using (ChannelFactory<ICustomPeerResolverChannel> factory = new ChannelFactory<ICustomPeerResolverChannel>(config, endpoint))
            return new ChannelFactory<ICustomPeerResolverChannel>(binding, endpoint);
        }
    }

    // Custom resolver binding element implementation
    public class CustomPeerResolverBindingElement : PeerResolverBindingElement
    {
        PeerReferralPolicy peerReferralPolicy = PeerReferralPolicy.Share;
        static CustomPeerResolver resolverClient = new CustomPeerResolver("");

        public CustomPeerResolverBindingElement() { }
        protected CustomPeerResolverBindingElement(CustomPeerResolverBindingElement other) : base(other) { }

        public override PeerReferralPolicy ReferralPolicy
        {
            get { return peerReferralPolicy; }
            set { peerReferralPolicy = value; }
        }

        public override BindingElement Clone()
        {
            return new CustomPeerResolverBindingElement(this);
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            context.RemainingBindingElements.Add(this);
            return context.BuildInnerChannelFactory<TChannel>();
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            context.RemainingBindingElements.Add(this);
            return context.CanBuildInnerChannelFactory<TChannel>();
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            context.RemainingBindingElements.Add(this);
            return context.BuildInnerChannelListener<TChannel>();
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            context.RemainingBindingElements.Add(this);
            return context.CanBuildInnerChannelListener<TChannel>();
        }

        // Returns reference to the singleton resolver client
        public override PeerResolver CreatePeerResolver()
        {
            return resolverClient;
        }

        public override T GetProperty<T>(BindingContext context)
        {
            return context.GetInnerProperty<T>();
        }
    }

    // Custom resolver configuration binding element implementation
    public class CustomPeerResolverConfigurationBindingElement : BindingElementExtensionElement //: BindingElementExtensionSection
    {
        public override Type BindingElementType
        {
            get { return typeof(CustomPeerResolverBindingElement); }
        }

        protected override BindingElement CreateBindingElement()
        {
            return new CustomPeerResolverBindingElement();
        }
    }
}
