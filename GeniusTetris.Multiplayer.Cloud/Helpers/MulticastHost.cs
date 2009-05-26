using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Net;

namespace GeniusTetris.Multiplayer.Cloud.Helpers
{
    /// <summary>
    /// permet d'encapsuler un sender/receiver en multicast
    /// </summary>
    /// <typeparam name="THostType"></typeparam>
    /// <typeparam name="TInterfaceService"></typeparam>
    /// <typeparam name="TInterfaceServiceClient"></typeparam>
    class MulticastHost<THostType, TInterfaceService, TInterfaceServiceClient> : IDisposable
        where TInterfaceServiceClient : IClientChannel
        where THostType : new()
    {
        bool _started = false;
        ServiceHost _host;
        ChannelFactory<TInterfaceServiceClient> _channelFactory;
        TInterfaceServiceClient _client;
        Uri _serviceAddress;
        string _endPointConfigurationName;

        public TInterfaceService Sender { get; private set; }
        public THostType Receiver { get; private set; }
        public Uri Uri { get; private set; }

        public Action<MulticastHost<THostType, TInterfaceService, TInterfaceServiceClient>> AfterStart { get; set; }
        public Action<MulticastHost<THostType, TInterfaceService, TInterfaceServiceClient>> BeforeShutdown { get; set; }

        public MulticastHost(Uri serviceAddress, string endPointConfigurationName)
        {
            Receiver = new THostType();
            _serviceAddress = serviceAddress;
            _endPointConfigurationName = endPointConfigurationName;
        }


        public void Start()
        {
            if (!_started)
            {
                WebRequest.DefaultWebProxy.Credentials = CredentialCache.DefaultNetworkCredentials;
                _started = true;
                _host = new ServiceHost(Receiver, _serviceAddress);
                _host.Open();

                _channelFactory = new ChannelFactory<TInterfaceServiceClient>(_endPointConfigurationName, new EndpointAddress(_serviceAddress));

                _client = _channelFactory.CreateChannel();
                Sender = (TInterfaceService)(object)_client;
                _client.Open();
                if (AfterStart != null)
                    AfterStart(this);
            }
        }

        public void Shutdown()
        {
            if (_started)
            {
                _started = false;
                if (BeforeShutdown != null)
                    BeforeShutdown(this);
                if (_host != null && _host.State == CommunicationState.Opened)
                {
                    _host.Close();
                }
                _host = null;
                if (_client != null && _client.State == CommunicationState.Opened)
                {
                    _client.Close();
                    _channelFactory.Close();
                }
                _channelFactory = null;
                Sender = default(TInterfaceService);
                _client = default(TInterfaceServiceClient);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                Shutdown();
                GC.SuppressFinalize(this);
            }
            else
            {
                if (_started)
                {
                    _host = null;
                    _client = default(TInterfaceServiceClient);
                    Sender = default(TInterfaceService);
                    _channelFactory = null;
                }
            }
        }
        #endregion

        ~MulticastHost()
        {
            Dispose(false);
        }
    }
}
